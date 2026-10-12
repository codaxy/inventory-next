import { ApiError, send } from "./http";

export interface AuthProviders {
    google: boolean;
    oneTimeCode: boolean;
}

/** The server's catalogue (`Permissions`); the server checks every request whatever the client shows. */
export type Permission =
    | "dashboard.view"
    | "company.view"
    | "company.edit"
    | "electronic_devices.view"
    | "electronic_devices.edit"
    | "licenses.view"
    | "licenses.edit"
    | "furniture.view"
    | "furniture.edit"
    | "information.view"
    | "information.edit"
    | "infrastructure.view"
    | "infrastructure.edit"
    | "audit_log.view"
    | "server_log.view"
    | "users.manage";

export interface CurrentUser {
    email: string;
    /** As an administrator gave it, not the provider's. */
    name: string;
    role: string;
    permissions: Permission[];
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
