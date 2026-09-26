import type { LogLevel } from "../../../api/serverLog";

/** A log line is denser than a record row, so more of them still fit a 1440px-tall display. */
export const pageSize = 25;

/** Milliseconds of quiet after the last keystroke before the search runs. */
export const searchDelay = 300;

export const levels: { value: LogLevel | null; text: string }[] = [
    { value: null, text: "All" },
    { value: "Warning", text: "Warnings" },
    { value: "Error", text: "Errors" },
];
