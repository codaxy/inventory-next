import {
    createDevice,
    type DeviceDetail,
    type DeviceForm,
    deleteDevice,
    getDevice,
    getDeviceOptions,
    updateDevice,
} from "../../../api/electronicDevices";
import { importanceFor } from "../../../assets";
import { RecordController } from "../../../recordController";
import m, { blankDraft, type DeviceDraft, emptyOptions, toAttached, toCopy, toDraft, toForm } from "./model";

const r = m.device;

export default class extends RecordController<DeviceDraft, DeviceDetail, DeviceForm> {
    protected readonly r = r;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/electronic-devices";
    protected readonly noun = "device";
    protected readonly newTitle = "New device";

    protected load = getDevice;
    protected create = createDevice;
    protected update = updateDevice;
    protected destroy = deleteDevice;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected duplicate = toCopy;
    protected titleOf = (d: DeviceDetail) => d.name;
    protected lastModifiedOf = (d: DeviceDetail) => d.lastModified;
    protected numberOf = (d: DeviceDetail) => d.number;
    protected emptyDraft = blankDraft;

    protected attached(d: DeviceDetail) {
        this.store.set(r.contracts, d.contracts.length);
        return toAttached(d);
    }

    /** What is on it, and what to do about exactly that: deactivate the seats, move the information. */
    protected refusal(holds: string) {
        const one = holds.startsWith("1 ") && !holds.includes(" and ");
        const steps = [
            ...(holds.includes("seat")
                ? [holds.includes("1 seat") ? "deactivate the seat" : "deactivate the seats"]
                : []),
            ...(holds.includes("information") ? ["move the information"] : []),
        ].join(" and ");
        return `${holds} ${one ? "is" : "are"} on it. ${steps[0].toUpperCase()}${steps.slice(1)} first, or keep the device.`;
    }

    protected deleteMessage() {
        const contracts = this.store.get(r.contracts) ?? 0;
        return contracts === 0
            ? "This cannot be undone."
            : `${contracts === 1 ? "Its maintenance contract goes" : `Its ${contracts} maintenance contracts go`} with it. This cannot be undone.`;
    }

    onInit() {
        this.store.set(r.options, emptyOptions);
        this.store.set(r.contracts, 0);
        super.onInit();

        // The importance follows the weights; the chips follow the chosen type.
        this.addTrigger("weights", [r.draft, r.options], (draft, options) =>
            this.store.set(r.importance, draft ? importanceFor(draft, options ?? emptyOptions) : "—"),
        );
        this.addTrigger("type-tags", [r.draft.typeId, r.options], (typeId, options) =>
            this.store.set(r.tags, typeId ? (options?.typeTags[typeId] ?? []) : []),
        );

        getDeviceOptions()
            .then((o) => this.store.set(r.options, { ...emptyOptions, ...o }))
            .catch(() => {});
    }
}
