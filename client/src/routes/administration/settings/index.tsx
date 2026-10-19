import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, LookupField, Repeater, TextField, ValidationGroup } from "cx/widgets";

import { mayEdit, permissions } from "../../../access";
import { documentTitle } from "../../../components/documentTitle";
import { formFields } from "../../../components/formFields";
import { documentLanguages } from "../../../documents/languages";
import { zoneLabel } from "../../../download";
import $app from "../../../model";
import Controller from "./Controller";
import m, { type GroupName } from "./model";

const same = (a: unknown, b: unknown) => JSON.stringify(a) === JSON.stringify(b);

const on = (act: (controller: Controller, instance: any) => void) => (_e: unknown, instance: any) =>
    act(instance.controller as Controller, instance);

const fields = (name: GroupName) => {
    const g = m.settings[name] as any;
    return formFields({ draft: g.draft, errors: g.errors, viewing: g.viewing }, `settings-${name}`);
};

/** Under a field being edited whose value is not its default: the default, and a way back to it. */
function resetLine(name: GroupName, field: string, shown: (value: any) => string) {
    const g = m.settings[name] as any;
    const draft: AccessorChain<unknown> = g.draft[field];
    const fallback: AccessorChain<unknown> = g.defaults[field];
    return (
        <cx>
            <p
                class="editor-hint settings-default"
                visible={expr(g.viewing, draft, fallback, (v, d, f) => !v && !same(d ?? null, f))}
            >
                <span text={expr(fallback, (f) => `Default: ${shown(f)}.`)} />
                <Button
                    mod="hollow"
                    class="settings-reset"
                    text="Reset to default"
                    onClick={on((c) => c.reset(name, field))}
                />
            </p>
        </cx>
    );
}

/** One group: view first, Edit to change it, its own Cancel and Save at its foot. */
function card(name: GroupName, title: string, hint: string, body: any) {
    const g = m.settings[name] as any;
    return (
        <cx>
            <section class="editor-section">
                <div class="editor-section-head">
                    <h2 class="editor-section-title" text={title} />
                    <Button
                        mod="hollow"
                        visible={expr(
                            g.viewing,
                            $app.url,
                            permissions,
                            (v, url, held) => !!v && mayEdit(url, held),
                        )}
                        onClick={on((c) => c.edit(name))}
                    >
                        <Icon name="edit" class="size-4" />
                        <span text="Edit" />
                    </Button>
                </div>
                <p class="settings-hint" text={hint} />
                <ValidationGroup valid={g.valid} visited={g.visited} viewMode={g.viewing}>
                    <div class="editor-grid">{body}</div>
                </ValidationGroup>
                <div class="editor-actions" visible={falsy(g.viewing)}>
                    <Button mod="hollow" text="Cancel" onClick={on((c) => c.cancel(name))} />
                    <Button mod="primary" text="Save" disabled={g.saving} onClick={on((c) => c.save(name))} />
                </div>
            </section>
        </cx>
    );
}

const days = (n: number) => (n === 1 ? "1 day" : `${n} days`);

function dashboardCard() {
    const f = fields("dashboard");
    const window = (label: string, field: string) => (
        <cx>
            <div>
                {f.whole(label, field, { min: 1, required: true })}
                {resetLine("dashboard", field, days)}
            </div>
        </cx>
    );
    return card(
        "dashboard",
        "Dashboard",
        "How far back and ahead the dashboard looks, in days.",
        <cx>
            {window("Expired subscriptions, days back", "subscriptions_expired_days")}
            {window("Expiring subscriptions, days ahead", "subscriptions_expiring_days")}
            {window("Ended warranties, days back", "warranties_ended_days")}
            {window("Ending warranties, days ahead", "warranties_ending_days")}
        </cx>,
    );
}

function documentsCard() {
    const f = fields("documents");
    const d = m.settings.documents;
    return card(
        "documents",
        "Documents",
        "What every printed document shares. Its reader can still choose another language.",
        <cx>
            <div>
                {f.text("Place documents are signed in", "place", 100)}
                <p class="editor-hint" visible={falsy(d.viewing)} text="Empty leaves the line for a hand." />
            </div>
            <div>
                <div id="settings-language-label" class="editor-label" text="Default language" />
                <LookupField
                    id="settings-language"
                    value={d.draft.default_language}
                    options={documentLanguages as any}
                    required
                    error={d.errors.default_language}
                />
                <p
                    class="field-message"
                    visible={hasValue(d.errors.default_language)}
                    text={d.errors.default_language}
                />
            </div>
        </cx>,
    );
}

function signInCard() {
    const s = m.settings.sign_in;
    const domains = s.draft.allowed_domains;
    return card(
        "sign_in",
        "Sign-in",
        "Only users added under Users sign in; these domains narrow who that can be.",
        <cx>
            <div class="editor-wide">
                <div class="editor-label" text="Allowed domains" />
                <div class="editor-chips" visible={s.viewing}>
                    <Repeater records={domains} recordAlias={m.$domain}>
                        <span class="editor-chip" text={m.$domain} />
                    </Repeater>
                </div>
                <div
                    class="editor-value editor-empty"
                    visible={expr(s.viewing, domains, (v, list) => !!v && !list?.length)}
                    text="Any domain"
                />
                <div visible={falsy(s.viewing)}>
                    <div class="editor-chips" visible={expr(domains, (list) => !!list?.length)}>
                        <Repeater records={domains} recordAlias={m.$domain}>
                            <button
                                type="button"
                                class="chip"
                                onClick={(_e: unknown, instance: any) =>
                                    (instance.controller as Controller).removeDomain(
                                        instance.store.get(m.$domain),
                                    )
                                }
                                attrs={{ "aria-label": "Remove domain" }}
                            >
                                <span text={m.$domain} />
                                <Icon name="close" class="size-3.5" />
                            </button>
                        </Repeater>
                    </div>
                    <div class="settings-add">
                        <TextField
                            value={m.settings.newDomain}
                            placeholder="example.com"
                            inputAttrs={{ "aria-label": "Domain to add", autoCapitalize: "off" }}
                        />
                        <Button mod="hollow" onClick={on((c) => c.addDomain())}>
                            <Icon name="created" class="size-4" />
                            <span text="Add" />
                        </Button>
                    </div>
                    <p
                        class="editor-hint"
                        visible={expr(domains, (list) => !list?.length)}
                        text="None: any domain, for whoever is added under Users."
                    />
                </div>
                <p
                    class="field-message"
                    visible={hasValue(s.errors.allowed_domains)}
                    text={s.errors.allowed_domains}
                />
            </div>
        </cx>,
    );
}

/**
 * The time zones, which are not a choice yet: the interface shows instants in the browser's zone, and
 * every export — a spreadsheet, a PDF — is asked for in the same one. Shown, so it is plain which.
 */
function timeZoneCard() {
    const zone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const current = `${zone} (${zoneLabel(zone, new Date().getFullYear())})`;
    const row = (label: string, source: string) => (
        <cx>
            <div>
                <div class="editor-label" text={label} />
                <div class="editor-value">
                    <span text={current} />
                    <span class="settings-source" text={source} />
                </div>
            </div>
        </cx>
    );
    return (
        <cx>
            <section class="editor-section">
                <div class="editor-section-head">
                    <h2 class="editor-section-title" text="Time zone" />
                </div>
                <p class="settings-hint" text="Not a choice yet." />
                <div class="editor-grid">
                    {row("Interface", "This device's time zone")}
                    {row("Exported documents", "Same as the interface")}
                </div>
            </section>
        </cx>
    );
}

/** Administration → Settings: what an administrator decides about how the application is used. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-top">
                <h1 class="page-header page-title" text="Settings" />
                {documentTitle("Settings")}
            </div>
            <div class="editor">
                <div class="editor-alert" visible={hasValue(m.settings.error)}>
                    <span text={m.settings.error} />
                </div>
                <div class="settings-loading" visible={m.settings.loading}>
                    <span class="skeleton-bar" style="width: 40%" />
                </div>
                <div visible={falsy(m.settings.loading)}>
                    {signInCard()}
                    {dashboardCard()}
                    {documentsCard()}
                    {timeZoneCard()}
                </div>
            </div>
        </div>
    </cx>
));
