import { Controller } from "cx/ui";

import { getDocumentSettings } from "../../../../api/documents";
import { ApiError } from "../../../../api/http";
import { getHandover, handoverPdf, type Handover } from "../../../../api/people";
import { documentDate, languageOf, pageNumbers, type Language } from "../../../../documents/languages";
import { queryOf, writeAddress } from "../../../../listAddress";
import $app from "../../../../model";
import m from "./model";
import { loadHandoverText } from "./text";

const h = m.handover;

const numbered = (n: number | null) => (n ? `#${n}` : "");

export default class extends Controller {
    /** What the language re-renders: the sheet's answer, today, and the deployment's default. */
    private sheet?: Handover;
    private today = new Date();
    private defaultLanguage?: Language;

    onInit() {
        this.addTrigger("address", [$app.url], (url: string) => this.follow(url), true);
        this.addTrigger("language", [h.language], (language?: Language) => this.choose(language));
    }

    /** A new person opens the sheet afresh; the same one with another `?lang=` only changes language. */
    private follow(url: string) {
        const id = this.store.get(m.$route.id);
        if (id !== this.store.get(h.id)) this.open(id);
        else if (this.defaultLanguage) this.store.set(h.language, this.addressed(url));
    }

    /** The address's language, else the default. */
    private addressed(url: string): Language {
        return languageOf(queryOf(url).get("lang")) ?? this.defaultLanguage!;
    }

    /** A language picked: into the address, the default left out, and the sheet re-rendered in it. */
    private choose(language?: Language) {
        if (!language || !this.sheet) return;
        const id = this.store.get(h.id);
        writeAddress(this.store, `~/company/people/${id}/handover`, {
            lang: language === this.defaultLanguage ? undefined : language,
        });
        this.render(language).catch(() => this.store.set(h.error, "The handover sheet could not be loaded."));
    }

    private open(id: string) {
        this.sheet = undefined;
        this.today = new Date();
        this.store.set(h.id, id);
        this.store.set(h.name, "");
        this.store.set(h.rows, []);
        this.store.set(h.seats, []);
        this.store.set(h.place, "");
        this.store.set(h.controller, "");
        this.store.set(h.date, "");
        this.store.delete(h.language);
        this.store.delete(h.text);
        this.store.delete(h.pdfHref);
        this.store.set(h.print, "loading");
        this.store.delete(h.error);

        Promise.all([getHandover(id), getDocumentSettings()])
            .then(([sheet, settings]) => {
                if (this.store.get(h.id) !== id) return;
                this.sheet = sheet;
                this.defaultLanguage = languageOf(settings.defaultLanguage) ?? "en";
                this.store.set(h.name, sheet.name);
                this.store.set(h.place, settings.place);
                this.store.set(h.controller, sheet.controller);
                const language = this.addressed(this.store.get($app.url));
                this.store.set(h.language, language);
                // Rendered here, not left to the trigger, so `ready` waits for the text.
                return this.render(language).then(() => {
                    if (this.store.get(h.id) === id) this.store.set(h.print, "ready");
                });
            })
            .catch((error) => {
                if (this.store.get(h.id) !== id) return;
                this.store.set(
                    h.error,
                    error instanceof ApiError && error.status === 404
                        ? "This person no longer exists."
                        : "The handover sheet could not be loaded.",
                );
                this.store.set(h.print, "failed");
            });
    }

    /**
     * What the language decides: the text, the dates and the PDF's address — once its text has
     * arrived, and only while it is still the language chosen.
     */
    private async render(language: Language) {
        const text = await loadHandoverText(language);
        const sheet = this.sheet;
        if (!sheet || this.store.get(h.language) !== language) return;
        const id = this.store.get(h.id);
        this.store.set(h.text, text);
        this.store.set(h.pageNumbers, pageNumbers(language));
        this.store.set(h.date, documentDate(language, this.today));
        if (sheet.pdf) this.store.set(h.pdfHref, handoverPdf(id, language));
        this.store.set(
            h.rows,
            sheet.assets.map((a, i) => ({
                index: `${i + 1}`,
                number: a.number ? String(a.number) : "-",
                name: a.name?.trim() || "-",
                description: a.description?.trim() || "-",
                type: a.type || "-",
            })),
        );
        this.store.set(
            h.seats,
            sheet.seats.map((s, i) => ({
                index: `${i + 1}`,
                software: s.software,
                license: s.license,
                licenseNumber: numbered(s.licenseNumber),
                device: s.device ?? "-",
                deviceNumber: s.device ? numbered(s.deviceNumber) : "",
                expires: s.expires ? documentDate(language, s.expires) : "-",
            })),
        );
    }
}
