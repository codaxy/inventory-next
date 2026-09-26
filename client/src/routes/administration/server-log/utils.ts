import type { LogLevel } from "../../../api/serverLog";

/** A log is read in longer runs than a list of records. */
export const pageSize = 50;

/** Milliseconds of quiet after the last keystroke before the search runs. */
export const searchDelay = 300;

export const levels: { value: LogLevel | null; text: string }[] = [
    { value: null, text: "All" },
    { value: "Warning", text: "Warnings" },
    { value: "Error", text: "Errors" },
];
