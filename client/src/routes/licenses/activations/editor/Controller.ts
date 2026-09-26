import { Controller, History } from "cx/ui";

import {
    createActivation,
    deactivateActivation,
    deleteActivation,
    getActivation,
    getActivationOptions,
    getVolumes,
    perUser,
    reactivateActivation,
    type VolumeOption,
    type VolumeRef,
} from "../../../../api/activations";
import { ApiError, fieldErrors } from "../../../../api/http";
import { confirm } from "../../../../components/confirm";
import { encodeDate } from "../../../../dates";
import { guardLeaving } from "../../../../leaveGuard";
import { listReturn, queryOf } from "../../../../listAddress";
import $app from "../../../../model";
import { askDeactivationDate } from "./deactivateWindow";
import m, { deviceText, type EditorState, overWarning, toForm, toView, volumeText } from "./model";

const list = "~/licenses/activations";
const a = m.activation;

export default class extends Controller {
    private saved = "";
    private release?: () => void;
    private volumes: VolumeOption[] = [];
    /** Every volume, from the options: what a volume named in the address is looked up in. */
    private volumeRefs: VolumeRef[] = [];
    private licenses: { id: string; text: string }[] = [];
    /** A volume named by the address — `new?volumeId=…`, from a licence's volume — to choose once loaded. */
    private preselect: string | null = null;

    onInit() {
        this.addTrigger("software-chosen", [a.draft.softwareId], (softwareId) =>
            this.loadVolumes(softwareId),
        );
        this.addTrigger("volume-chosen", [a.draft.volumeId], (volumeId) => {
            const volume = this.volumes.find((v) => v.id === volumeId);
            this.store.set(a.volume, volume);
            this.store.set(a.forPerson, !volume || volume.typeId === perUser);
            this.store.delete(a.errors.volumeId);
        });
        this.addTrigger("seats", [a.volume, a.draft.quantity], (volume, quantity) =>
            this.store.set(a.overWarning, overWarning(volume, quantity)),
        );
        this.addTrigger("person-edited", [a.draft.personId], () => this.store.delete(a.errors.personId));
        this.addTrigger("device-edited", [a.draft.deviceId], () => this.store.delete(a.errors.deviceId));
        this.addTrigger("date-edited", [a.draft.activationDate], () =>
            this.store.delete(a.errors.activationDate),
        );
        this.addTrigger("quantity-edited", [a.draft.quantity], () => this.store.delete(a.errors.quantity));

        this.addTrigger("address", [$app.url], () => this.open(), true);

        for (const key of [a.software, a.people, a.devices, a.volumes]) this.store.set(key, []);
        getActivationOptions()
            .then((o) => {
                this.store.set(a.software, o.software);
                this.store.set(a.people, o.people);
                this.store.set(
                    a.devices,
                    o.devices.map((d) => ({ id: d.id, text: deviceText(d) })),
                );
                this.volumeRefs = o.volumes;
                this.licenses = o.licenses;
                this.choosePreselected();
            })
            .catch(() => {});
    }

    onDestroy() {
        this.release?.();
    }

    private open() {
        const routed = this.store.get(m.$route.id);
        const id = routed === "new" ? null : routed;

        this.store.set(a.id, id);
        this.store.set(a.loading, !!id);
        this.store.set(a.saving, false);
        this.store.delete(a.view);
        this.store.delete(a.error);
        this.store.set(a.errors, {});
        this.store.set(a.visited, false);
        this.store.set(a.valid, true);
        this.store.set(a.forPerson, true);
        this.store.delete(a.volume);
        this.store.set(a.title, id ? "" : "New activation");

        this.release?.();
        this.release = undefined;

        if (!id) {
            this.preselect = queryOf(this.store.get($app.url)).get("volumeId");
            this.store.set(a.fixed, !!this.preselect);
            this.store.set(a.origin, { href: listReturn(list), text: "Activations" });
            this.store.set(a.draft, { activationDate: encodeDate(new Date()), quantity: 1 });
            this.choosePreselected();
            this.saved = JSON.stringify(toForm(this.store.get(a.draft), true));
            this.release = guardLeaving(() => this.dirty());
            return;
        }

        getActivation(id)
            .then((activation) => {
                if (this.store.get(a.id) !== id) return;
                this.show(activation);
            })
            .catch((error) =>
                this.store.set(
                    a.error,
                    error instanceof ApiError && error.status === 404
                        ? "This activation no longer exists."
                        : "The activation could not be loaded.",
                ),
            )
            .finally(() => this.store.set(a.loading, false));
    }

    private show(activation: Parameters<typeof toView>[0]) {
        this.store.set(a.view, toView(activation));
        this.store.set(a.activationDate, activation.activationDate);
        this.store.set(
            a.title,
            `${activation.software.name} · ${activation.person?.name ?? activation.device?.name ?? ""}`,
        );
    }

    /** The address's volume: its software first, which loads the volumes, then the volume itself. */
    private choosePreselected() {
        const ref = this.volumeRefs.find((v) => v.id === this.preselect);
        if (!ref) return;
        // Started from a licence's volume: that licence is where the reader goes back to.
        this.store.set(a.origin, {
            href: `~/licenses/${ref.licenseId}`,
            text: this.licenses.find((l) => l.id === ref.licenseId)?.text ?? "Licence",
        });
        if (this.store.get(a.draft.softwareId) === ref.softwareId) return;
        this.store.set(a.draft.softwareId, ref.softwareId);
        this.store.set(a.draft.softwareText, ref.software);
    }

    private async loadVolumes(softwareId: string | null | undefined) {
        this.volumes = [];
        this.store.set(a.volumes, []);
        this.store.delete(a.draft.volumeId);
        this.store.delete(a.draft.volumeText);
        if (!softwareId) return;

        try {
            this.volumes = await getVolumes(softwareId);
            if (this.store.get(a.draft.softwareId) !== softwareId) return;
            this.store.set(
                a.volumes,
                this.volumes.map((v) => ({ id: v.id, text: volumeText(v) })),
            );
            const named = this.volumes.find((v) => v.id === this.preselect);
            if (named) {
                this.store.set(a.draft.volumeId, named.id);
                this.store.set(a.draft.volumeText, volumeText(named));
                this.preselect = null;
                // What the address chose is where the form starts, not an edit to guard.
                this.saved = JSON.stringify(toForm(this.store.get(a.draft), named.typeId === perUser));
            }
            // One volume is the choice already made.
            else if (this.volumes.length === 1) {
                this.store.set(a.draft.volumeId, this.volumes[0].id);
                this.store.set(a.draft.volumeText, volumeText(this.volumes[0]));
            }
        } catch {
            this.store.set(a.error, "The volumes could not be loaded.");
        }
    }

    dirty() {
        return JSON.stringify(toForm(this.store.get(a.draft), this.store.get(a.forPerson))) !== this.saved;
    }

    async save() {
        if (!this.store.get(a.valid)) return this.store.set(a.visited, true);

        this.store.set(a.saving, true);
        this.store.delete(a.error);

        try {
            await createActivation(toForm(this.store.get(a.draft), this.store.get(a.forPerson)));
            this.leave(this.store.get(a.origin)?.href ?? listReturn(list));
        } catch (error) {
            if (error instanceof ApiError && Object.keys(error.errors).length > 0)
                this.store.set(a.errors, fieldErrors<EditorState["errors"]>(error));
            else this.store.set(a.error, "The activation could not be saved.");
        } finally {
            this.store.set(a.saving, false);
        }
    }

    async deactivate() {
        const id = this.store.get(a.id);
        const activated = this.store.get(a.activationDate);
        if (!id || !activated) return;

        const date = await askDeactivationDate({ activated });
        if (!date) return;

        try {
            this.show(await deactivateActivation(id, date));
        } catch (error) {
            this.store.set(
                a.error,
                error instanceof ApiError ? error.message : "The activation could not be deactivated.",
            );
        }
    }

    async reactivate() {
        const id = this.store.get(a.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Reactivate this activation?",
            message: "Its deactivation date is discarded and its seats are in use again.",
            confirmText: "Reactivate",
            cancelText: "Keep deactivated",
        });
        if (!confirmed) return;

        try {
            this.show(await reactivateActivation(id));
        } catch (error) {
            this.store.set(
                a.error,
                error instanceof ApiError ? error.message : "The activation could not be reactivated.",
            );
        }
    }

    async remove() {
        const id = this.store.get(a.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Delete this activation?",
            message:
                "It goes from the record entirely; deactivate it instead to keep its history. This cannot be undone.",
            confirmText: "Delete activation",
            cancelText: "Keep",
            danger: true,
        });
        if (!confirmed) return;

        try {
            await deleteActivation(id);
            this.leave(listReturn(list));
        } catch {
            this.store.set(a.error, "The activation could not be deleted.");
        }
    }

    private leave(to: string) {
        this.release?.();
        this.release = undefined;
        History.pushState({}, null, to);
    }
}
