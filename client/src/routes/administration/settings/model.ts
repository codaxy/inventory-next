import { createModel } from "cx/ui";

export interface SignIn {
    allowed_domains: string[];
}

export interface Dashboard {
    subscriptions_expired_days: number | null;
    subscriptions_expiring_days: number | null;
    warranties_ended_days: number | null;
    warranties_ending_days: number | null;
}

/** As the server's `AlertLead.Units`. */
export const AlertUnits = ["days", "weeks", "months"] as const;

export interface Alerts {
    recipients: string[];
    lead: { amount: number | null; unit: string | null };
}

export interface Documents {
    place?: string | null;
    default_language: string | null;
}

/** One card: what is saved, what is being typed, and the defaults to reset to. */
export interface Group<T> {
    saved: T;
    draft: T;
    defaults: T;
    errors: Partial<Record<keyof T, string>>;
    viewing: boolean;
    saving: boolean;
    valid: boolean;
    visited: boolean;
}

/** Keyed by the server's group names, so a setting `group.name` is `settings[group].draft[name]`. */
export interface Groups {
    sign_in: Group<SignIn>;
    dashboard: Group<Dashboard>;
    alerts: Group<Alerts>;
    documents: Group<Documents>;
}

export type GroupName = keyof Groups;

export const groupNames: GroupName[] = ["sign_in", "dashboard", "alerts", "documents"];

export interface Model {
    settings: Groups & {
        loading: boolean;
        error?: string;
        /** The domain being typed, before Add puts it among the chips. */
        newDomain?: string | null;
        /** The address being typed, before Add puts it among the recipients. */
        newRecipient?: string | null;
    };
    $chip: string;
}

export default createModel<Model>();
