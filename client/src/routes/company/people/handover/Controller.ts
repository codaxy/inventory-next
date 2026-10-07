import { Controller } from "cx/ui";

import { ApiError } from "../../../../api/http";
import { getHandover, handoverPdf } from "../../../../api/people";
import $app from "../../../../model";
import m from "./model";

const h = m.handover;

const two = (n: number) => String(n).padStart(2, "0");

/** "07.10.2026.", as the sheet's language writes a date. */
function today(): string {
    const now = new Date();
    return `${two(now.getDate())}.${two(now.getMonth() + 1)}.${now.getFullYear()}.`;
}

/** A calendar date, `YYYY-MM-DD`, as the sheet writes it: "30.08.2026.". */
function day(date: string): string {
    const [y, m, d] = date.split("-");
    return `${d}.${m}.${y}.`;
}

const numbered = (n: number | null) => (n ? `#${n}` : "");

export default class extends Controller {
    onInit() {
        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    private open() {
        const id = this.store.get(m.$route.id);
        this.store.set(h.id, id);
        this.store.set(h.name, "");
        this.store.set(h.rows, []);
        this.store.set(h.seats, []);
        this.store.set(h.place, "");
        this.store.set(h.controller, "");
        this.store.set(h.date, today());
        this.store.delete(h.pdfHref);
        this.store.set(h.print, "loading");
        this.store.delete(h.error);

        getHandover(id)
            .then((sheet) => {
                if (this.store.get(h.id) !== id) return;
                this.store.set(h.name, sheet.name);
                this.store.set(h.place, sheet.place);
                this.store.set(h.controller, sheet.controller);
                if (sheet.pdf) this.store.set(h.pdfHref, handoverPdf(id));
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
                        expires: s.expires ? day(s.expires) : "-",
                    })),
                );
                this.store.set(h.print, "ready");
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
}
