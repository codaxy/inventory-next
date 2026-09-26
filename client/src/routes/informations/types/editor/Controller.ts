import { type GroupDetail, type GroupForm, informationTypes } from "../../../../api/informations";
import { RecordController } from "../../../../recordController";
import m, { type GroupDraft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<GroupDraft, GroupDetail, GroupForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/informations/types";
    protected readonly noun = "type";
    protected readonly newTitle = "New type";

    protected load = informationTypes.get;
    protected create = informationTypes.create;
    protected update = informationTypes.update;
    protected destroy = informationTypes.remove;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (g: GroupDetail) => g.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") ? "is" : "are"} of it, and would be deleted with it. Give ${holds.startsWith("1 ") ? "it" : "them"} another type first, or keep this one.`;
    }
}
