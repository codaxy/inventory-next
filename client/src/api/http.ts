/** Thrown for any response the caller is expected to show rather than swallow. */
export class ApiError extends Error {
    constructor(
        message: string,
        readonly status: number,
        /** A validation problem's messages by field, keyed as the JSON the client sent. */
        readonly errors: Record<string, string[]> = {},
    ) {
        super(message);
    }
}

export async function send<T>(path: string, init?: RequestInit): Promise<T> {
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
        throw new ApiError(
            detail ?? problem?.title ?? "Something went wrong.",
            response.status,
            problem?.errors ?? {},
        );
    }

    return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
}

/** A validation problem's first message per field, typed as the form's own errors. */
export function fieldErrors<T extends object>(error: ApiError): T {
    return Object.fromEntries(
        Object.entries(error.errors).map(([field, messages]) => [field, messages[0]]),
    ) as T;
}
