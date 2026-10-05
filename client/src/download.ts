import { ApiError } from "./api/http";

/**
 * Fetches a file the API serves and saves it under the name the server gives, so the page knows when
 * it arrived and what went wrong — a plain `<a download>` hands both to the browser, which saves an
 * error answer as a broken file.
 */
export async function download(url: string): Promise<void> {
    const response = await fetch(url, { credentials: "same-origin" });
    if (!response.ok) {
        const problem = (await response.json().catch(() => null)) as { title?: string } | null;
        throw new ApiError(problem?.title ?? "Something went wrong.", response.status);
    }

    const blob = await response.blob();
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName(response.headers.get("content-disposition")) ?? "download";
    link.click();
    // Not revoked at once: Safari reads the URL after `click` returns, and a revoked one saves nothing.
    setTimeout(() => URL.revokeObjectURL(link.href), 10_000);
}

/**
 * An export's URL naming the browser's time zone (`tz`, an IANA name), so the instants in the file read
 * as they do on screen: a spreadsheet cell holds no zone, so the server converts. `tzLabel` is what the
 * file's headers call it, made here because .NET on Linux knows no abbreviations.
 */
export function inViewerZone(url: string): string {
    const zone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const label = zoneLabel(zone, new Date().getUTCFullYear());
    return `${url}${url.includes("?") ? "&" : "?"}tz=${encodeURIComponent(zone)}&tzLabel=${encodeURIComponent(label)}`;
}

/**
 * The zone's abbreviations in mid-January and mid-July, standard time first, one when they agree:
 * "CET/CEST", "GMT/BST", "EST/EDT". British English has the European ones and US English the American,
 * so the first of the two whose names are letters is taken; a zone neither names is its offsets, "GMT+9".
 */
export function zoneLabel(zone: string, year: number): string {
    // Ordered by offset, so a southern zone's January daylight time still comes second.
    const days = [new Date(Date.UTC(year, 0, 15, 12)), new Date(Date.UTC(year, 6, 15, 12))].sort(
        (a, b) => offsetMinutes(zone, a) - offsetMinutes(zone, b),
    );
    const names = (locale: string) => [...new Set(days.map((day) => zoneName(zone, locale, "short", day)))];
    const lettered = ["en-GB", "en-US"].map(names).find((n) => n.every((name) => /^[A-Za-z]+$/.test(name)));
    return (lettered ?? names("en-GB")).join("/");
}

function zoneName(zone: string, locale: string, style: "short" | "longOffset", day: Date): string {
    return (
        new Intl.DateTimeFormat(locale, { timeZone: zone, timeZoneName: style })
            .formatToParts(day)
            .find((part) => part.type === "timeZoneName")?.value ?? ""
    );
}

/** "GMT+05:30" as 330; "GMT", the zero offset, as 0. */
function offsetMinutes(zone: string, day: Date): number {
    const match = /([+-])(\d{2}):(\d{2})/.exec(zoneName(zone, "en-GB", "longOffset", day));
    return match ? (match[1] === "-" ? -1 : 1) * (Number(match[2]) * 60 + Number(match[3])) : 0;
}

/** `filename*=UTF-8''…` when the server sends it — ASP.NET does for any name — else `filename=`. */
function fileName(disposition: string | null): string | undefined {
    if (!disposition) return undefined;
    const encoded = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
    if (encoded) return decodeURIComponent(encoded[1]);
    return /filename="?([^";]+)"?/i.exec(disposition)?.[1];
}
