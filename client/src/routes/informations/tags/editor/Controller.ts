import { type GroupDetail, type GroupForm, informationTags } from "../../../../api/informations";
import { RecordController } from "../../../../recordController";
import m, { type GroupDraft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<GroupDraft, GroupDetail, GroupForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/informations/tags";
    protected readonly noun = "tag";
    protected readonly newTitle = "New tag";

    protected load = informationTags.get;
    protected create = informationTags.create;
    protected update = informationTags.update;
    protected destroy = informationTags.remove;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (g: GroupDetail) => g.name;

    private tagged = 0;

    /** A tag in use still goes — with its links, the information staying — so it is attached, never held. */
    protected attached(g: GroupDetail) {
        this.tagged = g.information.total;
        return { ...toAttached(g), holds: undefined };
    }

    protected deleteMessage() {
        return this.tagged === 0
            ? "No information has it. This cannot be undone."
            : `${this.tagged === 1 ? "1 piece of information loses" : `${this.tagged} pieces of information lose`} it. This cannot be undone.`;
    }
}
