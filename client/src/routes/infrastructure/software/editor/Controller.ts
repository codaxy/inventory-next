import { type OnVolumeDetail, type OnVolumeForm, software } from "../../../../api/infrastructure";
import { numberText, volumeText } from "../../../../inventoryNumbers";
import { RecordController } from "../../../../recordController";
import m, { type Draft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<Draft, OnVolumeDetail, OnVolumeForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/infrastructure/software";
    protected readonly noun = "software";
    protected readonly newTitle = "New software";

    protected load = software.get;
    protected create = software.create;
    protected update = software.update;
    protected destroy = software.remove;
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
        software
            .options()
            .then((o) =>
                this.store.set(m.record.options, {
                    volumes: o.volumes.map((v) => ({ id: v.id, text: volumeText(v) })),
                }),
            )
            .catch(() => {});
    }

    /** The license the volume is under, as a link beside it, and the volume's parts for its view. */
    protected loaded(d: OnVolumeDetail) {
        this.store.set(m.record.licenseHref, `~/licenses/${d.volume.licenseId}`);
        this.store.set(m.record.licenseText, d.volume.license);
        this.store.set(m.record.licenseNumber, numberText(d.volume.licenseNumber));
        this.store.set(m.record.designator, d.volume.designator);
    }
}
