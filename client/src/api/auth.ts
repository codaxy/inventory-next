export interface AuthProviders {
    google: boolean;
    oneTimeCode: boolean;
}

export interface CurrentUser {
    email: string;
    name: string;
}

/** Thrown for any response the caller is expected to show rather than swallow. */
export class ApiError extends Error {
    constructor(
        message: string,
        readonly status: number,
    ) {
        super(message);
    }
}

async function send<T>(path: string, init?: RequestInit): Promise<T> {
    const response = await fetch(path, {
        ...init,
        credentials: "same-origin",
        headers: { "content-type": "application/json", ...(init?.headers ?? {}) },
    });

    if (!response.ok) {
        // ProblemDetails is what the API answers with; anything else is a bug or a proxy. A
        // validation problem's title is always "One or more validation errors occurred.", so the
        // first field message is what says what is wrong.
        const problem = (await response.json().catch(() => null)) as {
            title?: string;
            errors?: Record<string, string[]>;
        } | null;
        const detail = Object.values(problem?.errors ?? {})[0]?.[0];
        throw new ApiError(detail ?? problem?.title ?? "Something went wrong.", response.status);
    }

    return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
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
