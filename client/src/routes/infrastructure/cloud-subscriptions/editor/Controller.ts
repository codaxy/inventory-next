import { type OnVolumeDetail, type OnVolumeForm, cloudSubscriptions } from "../../../../api/infrastructure";
import { RecordController } from "../../../../recordController";
import m, { type Draft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<Draft, OnVolumeDetail, OnVolumeForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/infrastructure/cloud-subscriptions";
    protected readonly noun = "cloud subscription";
    protected readonly newTitle = "New cloud subscription";

    protected load = cloudSubscriptions.get;
    protected create = cloudSubscriptions.create;
    protected update = cloudSubscriptions.update;
    protected destroy = cloudSubscriptions.remove;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (d: OnVolumeDetail) => d.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") ? "is" : "are"} kept on it. Move ${holds.startsWith("1 ") ? "it" : "them"} elsewhere first, or keep it.`;
    }

    onInit() {
        this.store.set(m.record.options, { volumes: [] });
        super.onInit();
        cloudSubscriptions
            .options()
            .then((o) => this.store.set(m.record.options, o))
            .catch(() => {});
    }

    /** The license the volume is under, as a link beside it. */
    protected loaded(d: OnVolumeDetail) {
        this.store.set(m.record.licenseHref, `~/licenses/${d.volume.licenseId}`);
        this.store.set(m.record.licenseText, d.volume.license);
    }
}
