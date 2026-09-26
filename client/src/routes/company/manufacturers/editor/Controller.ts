import {
    createManufacturer,
    deleteManufacturer,
    getManufacturer,
    type ManufacturerDetail,
    type ManufacturerForm,
    updateManufacturer,
} from "../../../../api/manufacturers";
import { RecordController } from "../../../../recordController";
import m, { type ManufacturerDraft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<ManufacturerDraft, ManufacturerDetail, ManufacturerForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/company/manufacturers";
    protected readonly noun = "manufacturer";
    protected readonly newTitle = "New manufacturer";

    protected load = getManufacturer;
    protected create = createManufacturer;
    protected update = updateManufacturer;
    protected destroy = deleteManufacturer;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (d: ManufacturerDetail) => d.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return holds.includes("software")
            ? `${holds} name them, and the software would be deleted with them. Give those another manufacturer first, or keep this one.`
            : `${holds} name them. Give ${holds.startsWith("1 ") ? "it" : "them"} another manufacturer first, or keep this one.`;
    }
}
