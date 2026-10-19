import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import {
    Button,
    Icon,
    LookupField,
    NumberField,
    Repeater,
    Switch,
    TextField,
    ValidationGroup,
} from "cx/widgets";

import { mayEdit, permissions } from "../../../access";
import { numberValue } from "../../../bindings";
import { documentTitle } from "../../../components/documentTitle";
import { formFields } from "../../../components/formFields";
import { documentLanguages } from "../../../documents/languages";
import { zoneLabel } from "../../../download";
import $app from "../../../model";
import Controller from "./Controller";
import m, { AlertUnits, type GroupName, type Lead, type Slot } from "./model";

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

/** The units as the number before them reads: "1 week", "2 weeks". The stored value is the plural. */
const units = (amount: number | null | undefined) =>
    AlertUnits.map((unit) => ({ id: unit, text: amount === 1 ? unit.slice(0, -1) : unit }));

/** "2 months before", "1 week before". */
/** "2 months", "1 week". */
const span = (lead: Lead | null | undefined) =>
    lead?.amount && lead.unit
        ? `${lead.amount} ${lead.amount === 1 ? lead.unit.slice(0, -1) : lead.unit}`
        : "—";

/** "2 months before". */
const leadText = (lead: Lead | null | undefined) => `${span(lead)} before`;

/** "2 months before, no reminders"; "2 months before, then 2 weeks and 2 days before". */
const timingText = (lead: Lead | null | undefined, reminders: Lead[] | null | undefined) =>
    reminders?.length
        ? `${leadText(lead)}, then ${reminders.map(span).join(" and ")} before`
        : `${leadText(lead)}, no reminders`;

/** A notice's number and unit side by side, read on as "… before". */
function leadCells(lead: AccessorChain<Lead>, id: string, label: string) {
    return (
        <cx>
            <NumberField
                value={numberValue(lead.amount)}
                required
                minValue={1}
                maxValue={999}
                format="n;0"
                inputAttrs={{ "aria-label": `${label}: how many`, inputMode: "numeric" }}
            />
            <LookupField id={id} value={lead.unit} options={expr(lead.amount, units) as any} required />
            <span class="settings-before" text="before" />
        </cx>
    );
}

/**
 * A reminder's field: its label with a switch, and while on, its lead beneath; "Off" in view.
 * `shown`: whether the field is there at all — the second reminder only once the first is on.
 */
function reminderField(slot: AccessorChain<Slot>, id: string, label: string, shown: any = true) {
    const a = m.settings.alerts;
    return (
        <cx>
            <div class="editor-wide" visible={shown}>
                <div class="settings-switch-label">
                    <div id={`${id}-label`} class="editor-label" text={label} />
                    <Switch
                        value={slot.on}
                        visible={falsy(a.viewing)}
                        inputAttrs={{ "aria-label": `${label} on` }}
                    />
                </div>
                <div
                    class="editor-value"
                    visible={a.viewing}
                    text={expr(slot, (s) => (s?.on ? leadText(s) : "Off"))}
                />
                <div class="settings-lead" visible={expr(a.viewing, slot.on, (v, o) => !v && !!o)}>
                    {leadCells(slot as any, id, label)}
                </div>
            </div>
        </cx>
    );
}

/**
 * When alerts go out, as fields like every other setting: the first notice, always on, and two
 * reminders switched on as needed — a reminder's number and unit only while it is on, the second
 * reminder only once the first is. One column, a field to a row: they are a sequence, read top to
 * bottom. One reset for all of it.
 */
function timing() {
    const a = m.settings.alerts;
    const lead = a.draft.lead;
    const slots = m.settings.slots;
    const changed = expr(
        a.viewing,
        lead,
        a.draft.reminders,
        a.defaults.lead,
        a.defaults.reminders,
        (v, l, r, dl, dr) => !v && (!same(l ?? null, dl) || !same(r ?? [], dr ?? [])),
    );
    return (
        <cx>
            <div class="editor-wide">
                <div id="settings-alerts-unit-label" class="editor-label" text="First notice" />
                <div class="editor-value" visible={a.viewing} text={expr(lead, leadText)} />
                <div class="settings-lead" visible={falsy(a.viewing)}>
                    {leadCells(lead, "settings-alerts-unit", "First notice")}
                </div>
            </div>
            {reminderField(slots.first, "settings-alerts-first-reminder", "First reminder")}
            {reminderField(
                slots.second,
                "settings-alerts-second-reminder",
                "Second reminder",
                slots.first.on,
            )}
            <div class="editor-wide" visible={falsy(a.viewing)}>
                <p class="editor-hint settings-default" visible={changed}>
                    <span
                        text={expr(
                            a.defaults.lead,
                            a.defaults.reminders,
                            (l, r) => `Default: ${timingText(l, r)}.`,
                        )}
                    />
                    <Button
                        mod="hollow"
                        class="settings-reset"
                        text="Reset to default"
                        onClick={on((c) => {
                            c.reset("alerts", "lead");
                            c.reset("alerts", "reminders");
                        })}
                    />
                </p>
                <p class="field-message" visible={hasValue(a.errors.lead)} text={a.errors.lead} />
                <p class="field-message" visible={hasValue(a.errors.reminders)} text={a.errors.reminders} />
            </div>
        </cx>
    );
}

function alertsCard() {
    const a = m.settings.alerts;
    return card(
        "alerts",
        "Expiry alerts",
        "An email before a subscription or a warranty ends, once for each date, and an optional reminder or two.",
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
            {timing()}
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
