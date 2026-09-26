import {
    createVendor,
    deleteVendor,
    getVendor,
    updateVendor,
    type VendorDetail,
    type VendorFields,
} from "../../../../api/vendors";
import { RecordController } from "../../../../recordController";
import m, { toAttached, toDraft, toForm, type VendorDraft } from "./model";

export default class extends RecordController<VendorDraft, VendorDetail, VendorFields> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/company/vendors";
    protected readonly noun = "vendor";
    protected readonly newTitle = "New vendor";

    protected load = getVendor;
    protected create = createVendor;
    protected update = updateVendor;
    protected destroy = deleteVendor;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (v: VendorDetail) => v.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return `${holds} name it, and would be deleted with it. Give them another vendor first, or keep this one.`;
    }
}
