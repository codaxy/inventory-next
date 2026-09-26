import { Culture } from "cx/ui";
import { Calendar } from "cx/widgets";

const pad = (n: number) => String(n).padStart(2, "0");

/** A calendar day as `YYYY-MM-DD`, read in local time. */
export function encodeDate(date: Date): string {
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

/**
 * British English for every date cx shows: day before month, weeks from Monday. A `DateField` stores
 * `YYYY-MM-DD`; cx's default is `toISOString()` of local midnight, which east of UTC is the day before.
 */
export function installDateCulture(): void {
    Culture.setCulture("en-GB");
    Calendar.prototype.startWithMonday = true;
    Culture.setDefaultDateEncoding(encodeDate);
}

/** Local midnight at the start of a `YYYY-MM-DD` day, as an instant. */
export function startOfDay(day: string): Date {
    const [y, m, d] = day.split("-").map(Number);
    return new Date(y, m - 1, d);
}

/** Local midnight at the start of the next day: the exclusive end of a range ending on `day`. */
export function endOfDay(day: string): Date {
    const start = startOfDay(day);
    return new Date(start.getFullYear(), start.getMonth(), start.getDate() + 1);
}

const dayFormat = new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" });
const longDayFormat = new Intl.DateTimeFormat("en-GB", {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
});
const timeFormat = new Intl.DateTimeFormat("en-GB", { hour: "2-digit", minute: "2-digit" });
const secondsFormat = new Intl.DateTimeFormat("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
});

export const formatDay = (date: Date) => dayFormat.format(date);
export const formatTime = (date: Date) => timeFormat.format(date);
export const formatDateTime = (date: Date) => `${dayFormat.format(date)}, ${secondsFormat.format(date)}`;

/** "Today", "Yesterday", or the date spelled out — a heading over a day's rows. */
export function formatDayHeading(date: Date, now = new Date()): string {
    const days = Math.round(
        (startOfDay(encodeDate(now)).getTime() - startOfDay(encodeDate(date)).getTime()) / 86_400_000,
    );
    if (days === 0) return "Today";
    if (days === 1) return "Yesterday";
    return longDayFormat.format(date);
}

/** The day after, as `YYYY-MM-DD`: the server's `to` is exclusive, the reader's "to" includes the day. */
export function dayAfter(value: string) {
    const [y, mo, d] = value.split("-").map(Number);
    return encodeDate(new Date(y, mo - 1, d + 1));
}

/** A `YYYY-MM-DD` value, as an address may or may not carry one. */
export const isDay = (value: string | null): value is string => !!value && /^\d{4}-\d{2}-\d{2}$/.test(value);
