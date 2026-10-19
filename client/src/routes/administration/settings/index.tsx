import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, LookupField, NumberField, Repeater, TextField, ValidationGroup } from "cx/widgets";

import { mayEdit, permissions } from "../../../access";
import { numberValue } from "../../../bindings";
import { documentTitle } from "../../../components/documentTitle";
import { formFields } from "../../../components/formFields";
import { documentLanguages } from "../../../documents/languages";
import { zoneLabel } from "../../../download";
import $app from "../../../model";
import Controller from "./Controller";
import m, { type Alerts, AlertUnits, type GroupName } from "./model";

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

interface Chips {
    label: string;
    values: AccessorChain<string[]>;
    /** What is being typed, before Add puts it among the chips. */
    typed: AccessorChain<string | null | undefined>;
    viewing: AccessorChain<boolean>;
    error: AccessorChain<string | undefined>;
    /** Shown in view when there are none. */
    none: string;
    /** Shown while editing when there are none. */
    noneHint: string;
    placeholder: string;
    /** The field's name for a screen reader: "Domain to add". */
    adding: string;
    /** The remove button's name: "Remove domain". */
    removing: string;
}

/** A list of values as chips: removable while editing, a field and Add beneath. */
function chips(o: Chips) {
    return (
        <cx>
            <div class="editor-wide">
                <div class="editor-label" text={o.label} />
                <div class="editor-chips" visible={o.viewing}>
                    <Repeater records={o.values} recordAlias={m.$chip}>
                        <span class="editor-chip" text={m.$chip} />
                    </Repeater>
                </div>
                <div
                    class="editor-value editor-empty"
                    visible={expr(o.viewing, o.values, (v, list) => !!v && !list?.length)}
                    text={o.none}
                />
                <div visible={falsy(o.viewing)}>
                    <div class="editor-chips" visible={expr(o.values, (list) => !!list?.length)}>
                        <Repeater records={o.values} recordAlias={m.$chip}>
                            <button
                                type="button"
                                class="chip"
                                onClick={(_e: unknown, instance: any) =>
                                    (instance.controller as Controller).removeChip(
                                        o.values,
                                        instance.store.get(m.$chip),
                                    )
                                }
                                attrs={{ "aria-label": o.removing }}
                            >
                                <span text={m.$chip} />
                                <Icon name="close" class="size-3.5" />
                            </button>
                        </Repeater>
                    </div>
                    <div class="settings-add">
                        <TextField
                            value={o.typed}
                            placeholder={o.placeholder}
                            inputAttrs={{ "aria-label": o.adding, autoCapitalize: "off" }}
                        />
                        <Button mod="hollow" onClick={on((c) => c.addChip(o.values, o.typed))}>
                            <Icon name="created" class="size-4" />
                            <span text="Add" />
                        </Button>
                    </div>
                    <p
                        class="editor-hint"
                        visible={expr(o.values, (list) => !list?.length)}
                        text={o.noneHint}
                    />
                </div>
                <p class="field-message" visible={hasValue(o.error)} text={o.error} />
            </div>
        </cx>
    );
}

function signInCard() {
    const s = m.settings.sign_in;
    return card(
        "sign_in",
        "Sign-in",
        "Only users added under Users sign in; these domains narrow who that can be.",
        chips({
            label: "Allowed domains",
            values: s.draft.allowed_domains,
            typed: m.settings.newDomain,
            viewing: s.viewing,
            error: s.errors.allowed_domains,
            none: "Any domain",
            noneHint: "None: any domain, for whoever is added under Users.",
            placeholder: "example.com",
            adding: "Domain to add",
            removing: "Remove domain",
        }),
    );
}

const units = AlertUnits.map((unit) => ({ id: unit, text: unit }));

/** "2 months before", "1 week before". */
const leadText = (lead: Alerts["lead"] | null | undefined) =>
    lead?.amount && lead.unit
        ? `${lead.amount} ${lead.amount === 1 ? lead.unit.slice(0, -1) : lead.unit} before`
        : "—";

function alertsCard() {
    const a = m.settings.alerts;
    const lead = a.draft.lead;
    return card(
        "alerts",
        "Expiry alerts",
        "An email before a subscription or a warranty ends, once for each date.",
        <cx>
            {chips({
                label: "Recipients",
                values: a.draft.recipients,
                typed: m.settings.newRecipient,
                viewing: a.viewing,
                error: a.errors.recipients,
                none: "None: no alerts are sent",
                noneHint: "None: no alerts are sent until someone is added.",
                placeholder: "it@codaxy.com",
                adding: "Address to add",
                removing: "Remove address",
            })}
            <div>
                <div id="settings-alerts-unit-label" class="editor-label" text="Sent" />
                <div class="editor-value" visible={a.viewing} text={expr(lead, leadText)} />
                <div class="settings-lead" visible={falsy(a.viewing)}>
                    <NumberField
                        value={numberValue(lead.amount)}
                        required
                        minValue={1}
                        maxValue={999}
                        format="n;0"
                        inputAttrs={{ "aria-label": "How many", inputMode: "numeric" }}
                    />
                    <LookupField
                        id="settings-alerts-unit"
                        value={lead.unit}
                        options={units as any}
                        required
                    />
                    <span text="before" />
                </div>
                {resetLine("alerts", "lead", leadText)}
                <p class="field-message" visible={hasValue(a.errors.lead)} text={a.errors.lead} />
            </div>
        </cx>,
    );
}

/**
 * How mail is sent — the server's configuration, so shown and not edited, the password never — and a
 * test email to the signed-in administrator, which says what the mail server answered.
 */
function mailCard() {
    const mail = m.settings.mail;
    const test = m.settings.test;
    const row = (label: string, value: any) => (
        <cx>
            <div>
                <div class="editor-label" text={label} />
                <div class="editor-value" text={value} />
            </div>
        </cx>
    );
    return (
        <cx>
            <section class="editor-section">
                <div class="editor-section-head">
                    <h2 class="editor-section-title" text="Mail" />
                    <Button mod="hollow" disabled={test.sending} onClick={on((c) => c.sendTest())}>
                        <Icon name="send" class="size-4" />
                        <span text={expr(test.sending, (s) => (s ? "Sending…" : "Send test email"))} />
                    </Button>
                </div>
                <p
                    class="settings-hint"
                    text="Set in the server's configuration. A test email goes to you."
                />
                <div class="editor-grid">
                    {row("Server", mail.server)}
                    {row("Sender", mail.sender)}
                    {row("Signs in as", mail.signsInAs)}
                </div>
                <p
                    class={expr(
                        test.sent,
                        (sent) => `settings-test ${sent ? "settings-test-sent" : "settings-test-failed"}`,
                    )}
                    visible={hasValue(test.result)}
                    text={test.result}
                />
            </section>
        </cx>
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
                    {alertsCard()}
                    {mailCard()}
                    {timeZoneCard()}
                </div>
            </div>
        </div>
    </cx>
));
