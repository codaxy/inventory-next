import { ApiError, send } from "./http";

export interface AuthProviders {
    google: boolean;
    oneTimeCode: boolean;
}

/** The server's catalogue (`Permissions`); the server checks every request whatever the client shows. */
export type Permission =
    | "inventory.view"
    | "inventory.edit"
    | "audit_log.view"
    | "server_log.view"
    | "users.manage"
    | "roles.manage"
    | "settings.manage";

export interface CurrentUser {
    /** A user runs the application with a role; a person is an employee seeing their own inventory. */
    kind: "user" | "person";
    email: string;
    /** As the application keeps it, not the provider's. */
    name: string;
    /** A user's role; a person has none. */
    role: string | null;
    permissions: Permission[];
    /** Whether "My inventory" has anything to show: a person's always, a user's who is a person too. */
    myInventory: boolean;
}

export const getAuthProviders = () => send<AuthProviders>("/api/auth/options");

/** Null when there is no session: not being signed in is an answer, not a failure. */
export async function getCurrentUser(): Promise<CurrentUser | null> {
    try {
        return await send<CurrentUser>("/api/auth/me");
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) return null;
        throw error;
    }
}

export const signOut = () => send<void>("/api/auth/sign-out", { method: "POST" });

export const requestOneTimeCode = (email: string) =>
    send<void>("/api/auth/one-time-code/request", {
        method: "POST",
        body: JSON.stringify({ email }),
    });

export const verifyOneTimeCode = (email: string, code: string) =>
    send<void>("/api/auth/one-time-code/verify", {
        method: "POST",
        body: JSON.stringify({ email, code }),
    });
