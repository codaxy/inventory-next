import { ApiError, send } from "./http";

export interface AuthProviders {
    google: boolean;
    oneTimeCode: boolean;
}

export interface CurrentUser {
    email: string;
    name: string;
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
