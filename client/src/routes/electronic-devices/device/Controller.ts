import { Controller } from "cx/ui";

import { getDevice } from "../../../api/electronicDevices";
import { ApiError } from "../../../api/http";
import $app from "../../../model";
import m, { toAttached, toDraft } from "./model";

const d = m.device;

/** A device's page, read-only: the address names the device, and it reopens when that changes. */
export default class extends Controller {
    onInit() {
        this.addTrigger("address", [$app.url], () => this.open(), true);
    }

    private open() {
        const id = this.store.get(m.$route.id);
        this.store.set(d.id, id);
        this.store.set(d.viewing, true);
        this.store.set(d.loading, true);
        this.store.set(d.title, "");
        this.store.delete(d.number);
        this.store.delete(d.error);
        this.store.set(d.errors, {});
        this.store.set(d.draft, { incomplete: false });
        this.store.set(d.tags, []);
        this.store.set(d.sections, []);
        this.store.delete(d.none);

        getDevice(id)
            .then((device) => {
                if (this.store.get(d.id) !== id) return;
                this.store.set(d.title, device.name);
                this.store.set(d.number, device.number ? `#${device.number}` : undefined);
                this.store.set(d.draft, toDraft(device));
                this.store.set(d.importance, device.importance?.name ?? "—");
                this.store.set(
                    d.tags,
                    device.tags.map((t) => ({ id: t.id, text: t.name })),
                );
                const attached = toAttached(device);
                this.store.set(d.sections, attached.sections);
                this.store.set(d.none, attached.none);
            })
            .catch((error) =>
                this.store.set(
                    d.error,
                    error instanceof ApiError && error.status === 404
                        ? "This device no longer exists."
                        : "The device could not be loaded.",
                ),
            )
            .finally(() => this.store.set(d.loading, false));
    }
}
