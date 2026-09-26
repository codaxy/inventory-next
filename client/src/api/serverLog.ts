import type { Page } from "../paging";
import { send } from "./http";

export type LogLevel = "Verbose" | "Debug" | "Information" | "Warning" | "Error" | "Fatal";

export interface LogEntry {
    time: string;
    /** Serilog's level; `Unknown` for a line that did not parse. */
    level: LogLevel | "Unknown";
    category: string | null;
    /** Already made displayable by the server: anything that would act is a marker, `⟨ESC⟩`. */
    message: string;
    exception: string | null;
    traceId: string | null;
    raw: boolean;
}

export interface LogQuery {
    /** Instants, ISO 8601; `from` inclusive, `to` exclusive. */
    from: string;
    to: string;
    level?: LogLevel;
    q?: string;
    sort?: "time" | "-time";
    page: number;
    pageSize: number;
}

const base = "/api/administration/server-log";

/** `YYYY-MM-DD`, newest first. */
export const getLogDays = () => send<{ days: string[] }>(`${base}/days`);

export function listLogEntries(query: LogQuery) {
    const params = new URLSearchParams();

    for (const [key, value] of Object.entries(query))
        if (value !== undefined && value !== null && value !== "") params.set(key, String(value));

    return send<Page<LogEntry>>(`${base}/?${params}`);
}
