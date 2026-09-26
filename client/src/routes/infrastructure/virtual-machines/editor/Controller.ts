import { type MachineDetail, type MachineForm, virtualMachines } from "../../../../api/infrastructure";
import { RecordController } from "../../../../recordController";
import m, { type Draft, toAttached, toDraft, toForm } from "./model";

export default class extends RecordController<Draft, MachineDetail, MachineForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/infrastructure/virtual-machines";
    protected readonly noun = "virtual machine";
    protected readonly newTitle = "New virtual machine";

    protected load = virtualMachines.get;
    protected create = virtualMachines.create;
    protected update = virtualMachines.update;
    protected destroy = virtualMachines.remove;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (d: MachineDetail) => d.name;
    protected attached = toAttached;

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") ? "is" : "are"} kept on it. Move ${holds.startsWith("1 ") ? "it" : "them"} elsewhere first, or keep it.`;
    }
}
