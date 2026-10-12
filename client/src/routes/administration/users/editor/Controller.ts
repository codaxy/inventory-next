import { ApiError } from "../../../../api/http";
import {
    createUser,
    deactivateUser,
    getUser,
    getUserOptions,
    reactivateUser,
    updateUser,
    type UserDetail,
    type UserForm,
} from "../../../../api/users";
import { confirm } from "../../../../components/confirm";
import { RecordController } from "../../../../recordController";
import m, { toDraft, toForm, type UserDraft } from "./model";

export default class extends RecordController<UserDraft, UserDetail, UserForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/administration/users";
    protected readonly noun = "user";
    protected readonly newTitle = "New user";

    protected load = getUser;
    protected create = createUser;
    protected update = updateUser;
    protected destroy = () => Promise.reject(new Error("A user is deactivated, never deleted."));
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (u: UserDetail) => u.name;

    protected loaded(u: UserDetail) {
        this.store.set(m.record.user, { isActive: u.isActive, isSelf: u.isSelf, canManage: u.canManage });
    }

    onInit() {
        super.onInit();
        this.addTrigger("adding", [m.record.id], (id) => {
            if (!id) this.store.delete(m.record.user);
        });
        this.store.set(m.record.options, { roles: [] });
        getUserOptions()
            .then((o) =>
                this.store.set(m.record.options, {
                    roles: o.roles.map((r) => ({ id: r.id, text: r.name })),
                }),
            )
            .catch(() => {});
    }

    async deactivate() {
        const id = this.store.get(m.record.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Deactivate this user?",
            message: "They are signed out on their next request and cannot sign in until reactivated.",
            confirmText: "Deactivate user",
            cancelText: "Keep active",
        });
        if (confirmed) await this.transition(() => deactivateUser(id));
    }

    async reactivate() {
        const id = this.store.get(m.record.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Reactivate this user?",
            message: "They can sign in again, with the role they hold.",
            confirmText: "Reactivate user",
            cancelText: "Keep deactivated",
        });
        if (confirmed) await this.transition(() => reactivateUser(id));
    }

    private async transition(change: () => Promise<UserDetail>) {
        this.store.delete(m.record.error);
        try {
            this.loaded(await change());
        } catch (error) {
            this.store.set(
                m.record.error,
                error instanceof ApiError ? error.message : "The user could not be changed.",
            );
        }
    }
}
