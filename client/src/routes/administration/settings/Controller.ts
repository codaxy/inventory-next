import type { AccessorChain } from "cx/data";
import { Controller } from "cx/ui";

import { ApiError } from "../../../api/http";
import { getMail, getSettings, saveSettings, sendTestEmail, type SettingList } from "../../../api/settings";
import { confirm } from "../../../components/confirm";
import { guardLeaving } from "../../../leaveGuard";
import $app from "../../../model";
import m, { type GroupName, groupNames, type Lead, type Slot } from "./model";

const group = (name: GroupName) => m.settings[name] as any;

const same = (a: unknown, b: unknown) => JSON.stringify(a) === JSON.stringify(b);

export default class extends Controller {
    release?: () => void;

    onInit() {
        this.store.set(m.settings.loading, true);
        this.store.delete(m.settings.error);
        this.store.set(m.settings.test, { sending: false });
        for (const name of groupNames)
            this.store.set(group(name), {
                saved: {},
                draft: {},
                defaults: {},
                errors: {},
                viewing: true,
                saving: false,
                valid: true,
                visited: false,
            });
        this.release = guardLeaving(() => groupNames.some((name) => this.dirty(name)));
        this.addTrigger("reminder-slots", [m.settings.slots], (slots) => this.slotsToDraft(slots));
        this.load();
    }

    onDestroy() {
        this.release?.();
    }

    async load() {
        try {
            const [list, mail] = await Promise.all([getSettings(), getMail()]);
            for (const name of groupNames) this.take(name, list);
            this.store.set(m.settings.mail, {
                server: `${mail.host}:${mail.port}`,
                sender: mail.fromName ? `${mail.fromName} <${mail.from}>` : mail.from,
                signsInAs: mail.signsInAs || "No sign-in",
            });
        } catch (error) {
            this.store.set(
                m.settings.error,
                error instanceof ApiError ? error.message : "The settings could not be loaded.",
            );
        } finally {
            this.store.set(m.settings.loading, false);
        }
    }

    /** One group's values and defaults from the server's list, its card back in view. */
    take(name: GroupName, list: SettingList) {
        const values: Record<string, unknown> = {};
        const defaults: Record<string, unknown> = {};
        for (const s of list.settings.filter((s) => s.group === name)) {
            const field = s.key.slice(name.length + 1);
            values[field] = s.value;
            defaults[field] = s.default;
        }
        const g = group(name);
        this.store.set(g.saved, values);
        this.store.set(g.draft, structuredClone(values));
        this.store.set(g.defaults, defaults);
        this.store.set(g.errors, {});
        this.store.set(g.viewing, true);
        this.store.set(g.visited, false);
        if (name === "alerts") this.slotsFromDraft();
    }

    dirty(name: GroupName) {
        const g = group(name);
        return !this.store.get(g.viewing) && !same(this.store.get(g.draft), this.store.get(g.saved));
    }

    edit(name: GroupName) {
        this.store.set(group(name).viewing, false);
    }

    cancel(name: GroupName) {
        const g = group(name);
        this.store.set(g.draft, structuredClone(this.store.get(g.saved)));
        this.store.set(g.errors, {});
        this.store.set(g.visited, false);
        this.store.set(g.viewing, true);
        this.store.delete(m.settings.newDomain);
        this.store.delete(m.settings.newRecipient);
        if (name === "alerts") this.slotsFromDraft();
    }

    async save(name: GroupName) {
        this.addTyped(name);
        const g = group(name);
        if (!this.store.get(g.valid)) return this.store.set(g.visited, true);

        const draft = this.store.get(g.draft) as Record<string, unknown>;
        const values = Object.fromEntries(
            Object.entries(draft).map(([field, value]) => [`${name}.${field}`, value ?? null]),
        );

        this.store.set(g.saving, true);
        try {
            this.take(name, await saveSettings(name, values));
            this.store.delete(m.settings.newDomain);
            this.store.delete(m.settings.newRecipient);
        } catch (error) {
            if (error instanceof ApiError && error.status === 400)
                this.store.set(
                    g.errors,
                    Object.fromEntries(
                        Object.entries(error.errors).map(([key, messages]) => [
                            key.slice(name.length + 1),
                            messages[0],
                        ]),
                    ),
                );
            else
                this.store.set(
                    m.settings.error,
                    error instanceof ApiError ? error.message : "The settings could not be saved.",
                );
        } finally {
            this.store.set(g.saving, false);
        }
    }

    /** A field back to its default; saved with the rest of its card. */
    reset(name: GroupName, field: string) {
        const g = group(name);
        this.store.set(g.draft[field], structuredClone(this.store.get(g.defaults[field])));
        if (name === "alerts" && field === "reminders") this.slotsFromDraft();
    }

    /** The reminder slots from the draft: one on per reminder, the rest off and ready with a value. */
    slotsFromDraft() {
        const reminders = this.store.get(m.settings.alerts.draft.reminders) ?? [];
        const slot = (lead: Lead | undefined, fallback: Lead): Slot =>
            lead ? { on: true, amount: lead.amount, unit: lead.unit } : { on: false, ...fallback };
        this.store.set(m.settings.slots, {
            first: slot(reminders[0], { amount: 1, unit: "weeks" }),
            second: slot(reminders[1], { amount: 1, unit: "days" }),
        });
    }

    /**
     * The slots into the draft. The first reminder off turns the second off too — "second" alone
     * reads oddly — each keeping its value, so switching them back on restores both as they were.
     */
    slotsToDraft(slots: { first: Slot; second: Slot } | undefined) {
        if (!slots) return;
        const { first, second } = slots;
        if (!first.on && second.on) {
            this.store.set(m.settings.slots.second.on, false);
            return;
        }
        const lead = ({ amount, unit }: Slot): Lead => ({ amount, unit });
        const reminders = [first, second].filter((s) => s.on).map(lead);
        if (!same(reminders, this.store.get(m.settings.alerts.draft.reminders) ?? []))
            this.store.set(m.settings.alerts.draft.reminders, reminders);
    }

    /**
     * What is typed among the chips — several at once where commas, semicolons or spaces separate
     * them, as a pasted list does — each once; the server tidies and checks them on save.
     */
    addChip(list: AccessorChain<string[]>, typed: AccessorChain<string | null | undefined>) {
        const entries = (this.store.get(typed) ?? "").split(/[\s,;]+/).filter((e) => e !== "");
        if (entries.length === 0) return;
        const values = this.store.get(list) ?? [];
        this.store.set(list, [
            ...values,
            ...entries.filter((e, i) => !values.includes(e) && entries.indexOf(e) === i),
        ]);
        this.store.delete(typed);
    }

    /** What is still typed in a card's chip field joins its chips: Save never drops it unseen. */
    addTyped(name: GroupName) {
        if (name === "sign_in") this.addChip(m.settings.sign_in.draft.allowed_domains, m.settings.newDomain);
        if (name === "alerts") this.addChip(m.settings.alerts.draft.recipients, m.settings.newRecipient);
    }

    removeChip(list: AccessorChain<string[]>, value: string) {
        this.store.update(list, (values) => (values ?? []).filter((v) => v !== value));
    }

    /** A test email to the signed-in administrator; the card says what came of it. */
    async sendTest() {
        const to = this.store.get($app.session.user)?.email;
        const confirmed = await confirm({
            title: "Send a test email?",
            message: `A test email goes to ${to}, through ${this.store.get(m.settings.mail)?.server}.`,
            confirmText: "Send test email",
            cancelText: "Don't send",
        });
        if (!confirmed) return;

        this.store.set(m.settings.test, { sending: true });
        try {
            const result = await sendTestEmail();
            this.store.set(m.settings.test, {
                sending: false,
                sent: result.sent,
                result: result.sent
                    ? `Sent to ${result.to}. Check your inbox.`
                    : `Not sent: ${result.problem ?? "the mail server did not take it"}`,
            });
        } catch (error) {
            this.store.set(m.settings.test, {
                sending: false,
                sent: false,
                result: error instanceof ApiError ? error.message : "The test email could not be sent.",
            });
        }
    }
}
