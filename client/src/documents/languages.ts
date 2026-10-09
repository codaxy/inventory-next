/**
 * The languages documents are printed in, as BCP 47 tags, each named in itself. The server's
 * `DocumentLanguages` must list the same: it refuses any other. A region or a script joins a name only
 * once two languages share it.
 */
export const documentLanguages = [
    { id: "en", text: "English" },
    { id: "sr-Latn-BA", text: "Srpski" },
] as const;

export type Language = (typeof documentLanguages)[number]["id"];

/**
 * The language a tag names, as the list writes it — `sr-latn-ba` is `sr-Latn-BA`, BCP 47 tags being
 * case-insensitive — or undefined.
 */
export const languageOf = (value: string | null | undefined): Language | undefined =>
    documentLanguages.find((l) => l.id.toLowerCase() === value?.toLowerCase())?.id;

/** The foot of every printed page in each language: `{page}` and `{pages}` are the printer's counters. */
const pageOf: Record<Language, string> = {
    en: "Page {page} of {pages}",
    "sr-Latn-BA": "Strana {page} od {pages}",
};

/**
 * The rule that numbers every printed page in `language` — "Strana 2 od 5" — through `@page`'s
 * `@bottom-center` and the page counters; its look is `_handover.scss`'s.
 */
export function pageNumbers(language: Language): string {
    const content = pageOf[language]
        .split(/(\{page\}|\{pages\})/)
        .filter((part) => part !== "")
        .map((part) =>
            part === "{page}"
                ? "counter(page)"
                : part === "{pages}"
                  ? "counter(pages)"
                  : JSON.stringify(part),
        )
        .join(" ");
    return `@page { @bottom-center { content: ${content}; } }`;
}

const two = (n: number) => String(n).padStart(2, "0");

const months = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
];

/** A day as each language writes it. Not `Intl`: the printer's browser need not carry every locale. */
const days: Record<Language, (year: number, month: number, day: number) => string> = {
    en: (y, m, d) => `${d} ${months[m - 1]} ${y}`,
    "sr-Latn-BA": (y, m, d) => `${two(d)}.${two(m)}.${y}.`,
};

/** A day in a document: `7 October 2026`, `07.10.2026.` — a local `Date`, or a calendar date `YYYY-MM-DD`. */
export function documentDate(language: Language, date: Date | string): string {
    if (typeof date === "string") {
        const [y, m, d] = date.split("-").map(Number);
        return days[language](y, m, d);
    }
    return days[language](date.getFullYear(), date.getMonth() + 1, date.getDate());
}
