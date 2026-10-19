import { send } from "./http";

/** A setting as stored, keyed `group.name`, its default beside it. */
export interface SettingItem {
    key: string;
    group: string;
    value: unknown;
    default: unknown;
}

export interface SettingList {
    settings: SettingItem[];
}

const base = "/api/administration/settings";

export const getSettings = () => send<SettingList>(base);

/** Every setting of one group, saved all together or not at all; answers every setting again. */
export const saveSettings = (group: string, values: Record<string, unknown>) =>
    send<SettingList>(`${base}/${group}`, { method: "PUT", body: JSON.stringify(values) });

/** How mail is sent, from the server's configuration; never its password. */
export interface EmailRelay {
    host: string;
    port: number;
    from: string;
    fromName: string;
    /** The account mail is sent from at the relay; empty where none signs in. */
    signsInAs: string;
}

export const getMail = () => send<EmailRelay>(`${base}/mail`);

/** Whether the relay took a test email to the signed-in administrator, and what it said if not. */
export interface TestEmailResult {
    to: string;
    sent: boolean;
    problem?: string | null;
}

export const sendTestEmail = () => send<TestEmailResult>(`${base}/mail/test`, { method: "POST" });
