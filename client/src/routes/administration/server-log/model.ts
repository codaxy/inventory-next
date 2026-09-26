import { createModel } from "cx/ui";

import type { LogEntry, LogLevel } from "../../../api/serverLog";
import { encodeDate, formatDayHeading, startOfDay } from "../../../dates";
import type { PagerState } from "../../../paging";

/** Text, or a marker the server put in place of a character that would have acted. */
export interface Piece {
    text: string;
    marker: boolean;
    /** A line break inside a message, marked so the next line reads as this entry's, not a new one. */
    newline?: boolean;
}

export interface Row {
    id: string;
    time: string;
    tag: string;
    tone: "info" | "warn" | "error" | "debug" | "raw";
    category: string;
    /** The full category name, for the tooltip; the row shows its last segment. */
    categoryFull: string;
    message: Piece[];
    exception?: Piece[];
    expanded: boolean;
}

export interface Day {
    day: string;
    text: string;
}

export interface ServerLogState {
    search?: string | null;
    level?: LogLevel | null;
    sort: "-time" | "time";
    page: number;
    /** `YYYY-MM-DD`, read as the viewer's own day. */
    day: string;
    days: Day[];

    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
}

export interface Model {
    serverLog: ServerLogState;
    $row: Row;
    $piece: Piece;
    $day: Day;
}

export default createModel<Model>();

const tags: Record<string, [string, Row["tone"]]> = {
    Verbose: ["VRB", "debug"],
    Debug: ["DBG", "debug"],
    Information: ["INF", "info"],
    Warning: ["WRN", "warn"],
    Error: ["ERR", "error"],
    Fatal: ["FTL", "error"],
    Unknown: ["???", "raw"],
};

const markers = /(⟨(?:ESC|CR|U\+[0-9A-F]{4,6})⟩)/;
const isMarker = /^⟨(?:ESC|CR|U\+[0-9A-F]{4,6})⟩$/;

/**
 * The server's markers picked out, so the row can show them as what they are rather than as text.
 * With `marking`, each line break gets a `⏎` too: a logged value that breaks the line and then reads
 * like an entry — "12:00:00 INF all good" — is visibly the tail of the one above.
 */
export function pieces(text: string, marking = false): Piece[] {
    return text
        .split(marking ? /(⟨(?:ESC|CR|U\+[0-9A-F]{4,6})⟩|\n)/ : markers)
        .filter((t) => t !== "")
        .flatMap((t): Piece[] =>
            t === "\n"
                ? [
                      { text: "⏎", marker: false, newline: true },
                      { text: "\n", marker: false },
                  ]
                : [{ text: t, marker: isMarker.test(t) }],
        );
}

const time = new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    fractionalSecondDigits: 3,
});

export function toRows(entries: LogEntry[], page: number): Row[] {
    return entries.map((entry, i) => {
        const [tag, tone] = tags[entry.level] ?? tags.Unknown;
        const category = entry.category ?? (entry.raw ? "unparsed line" : "");

        return {
            id: `${page}-${i}`,
            time: time.format(new Date(entry.time)),
            tag,
            tone,
            category: category.split(".").pop() ?? category,
            categoryFull: category,
            message: pieces(entry.message, true),
            exception: entry.exception ? pieces(entry.exception) : undefined,
            expanded: false,
        };
    });
}

/** "Today", "Yesterday", then "Thu 24 Sep" — short enough for a strip on a phone. */
const short = new Intl.DateTimeFormat("en-GB", { weekday: "short", day: "numeric", month: "short" });

export function toDays(days: string[], now = new Date()): Day[] {
    const today = encodeDate(now);
    const all = days.includes(today) ? days : [today, ...days];

    return all.map((day) => {
        const date = startOfDay(day);
        const heading = formatDayHeading(date, now);
        return { day, text: heading === "Today" || heading === "Yesterday" ? heading : short.format(date) };
    });
}
