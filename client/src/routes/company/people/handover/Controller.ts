import { Controller } from "cx/ui";

import { ApiError } from "../../../../api/http";
import { getHandover } from "../../../../api/people";
import $app from "../../../../model";
import m from "./model";

const h = m.handover;

export default class extends Controller {
    onInit() {
        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    private open() {
        const id = this.store.get(m.$route.id);
        this.store.set(h.id, id);
        this.store.set(h.name, "");
        this.store.set(h.rows, []);
        this.store.set(h.loading, true);
        this.store.delete(h.error);

        getHandover(id)
            .then((sheet) => {
                if (this.store.get(h.id) !== id) return;
                this.store.set(h.name, sheet.name);
                this.store.set(
                    h.rows,
                    sheet.assets.map((a, i) => ({
                        index: `${i + 1}.`,
                        number: a.number ? String(a.number) : "-",
                        name: a.name?.trim() || "-",
                        description: a.description?.trim() || "-",
                        type: a.type || "-",
                    })),
                );
            })
            .catch((error) =>
                this.store.set(
                    h.error,
                    error instanceof ApiError && error.status === 404
                        ? "This person no longer exists."
                        : "The handover sheet could not be loaded.",
                ),
            )
            .finally(() => this.store.set(h.loading, false));
    }

    print() {
        window.print();
    }
}
