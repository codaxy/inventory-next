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
