import type { AccessorChain } from "cx/data";
import { Controller } from "cx/ui";

import { ApiError } from "../../../api/http";
import { getMail, getSettings, saveSettings, sendTestEmail, type SettingList } from "../../../api/settings";
import { confirm } from "../../../components/confirm";
import { guardLeaving } from "../../../leaveGuard";
import $app from "../../../model";
import m, { type GroupName, groupNames } from "./model";

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
    }

    async save(name: GroupName) {
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
    }

    /** The typed value among the chips, once; the server tidies and checks it on save. */
    addChip(list: AccessorChain<string[]>, typed: AccessorChain<string | null | undefined>) {
        const value = this.store.get(typed)?.trim();
        if (!value) return;
        const values = this.store.get(list) ?? [];
        if (!values.includes(value)) this.store.set(list, [...values, value]);
        this.store.delete(typed);
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
