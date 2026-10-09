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

export const isLanguage = (value: unknown): value is Language =>
    documentLanguages.some((l) => l.id === value);

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
