import type { Language } from "../../../../../documents/languages";
import type { HandoverText } from "../model";

/**
 * The sheet's text, a file per language, each fetched only when the sheet is shown in it. A loader
 * per language, so a language without one fails the type check, as does a file missing a string.
 */
const loaders: Record<Language, () => Promise<{ default: HandoverText }>> = {
    en: () => import("./en"),
    "sr-Latn-BA": () => import("./sr-Latn-BA"),
};

export const loadHandoverText = (language: Language) => loaders[language]().then((m) => m.default);
