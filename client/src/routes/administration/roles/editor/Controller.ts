import { ApiError } from "../../../../api/http";
import {
    type CatalogueEntry,
    createRole,
    deleteRole,
    getRole,
    getRoleCatalogue,
    type RoleDetail,
    type RoleForm,
    resetRole,
    updateRole,
} from "../../../../api/roles";
import { confirm } from "../../../../components/confirm";
import { RecordController } from "../../../../recordController";
import m, { flagsOf, grantsOf, type RoleDraft, toDraft, toForm, withImplied } from "./model";

export default class extends RecordController<RoleDraft, RoleDetail, RoleForm> {
    protected readonly r = m.record;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/administration/roles";
    protected readonly noun = "role";
    protected readonly newTitle = "New role";

    /** The permissions a role can grant: the ticks are built from it. */
    private catalogue: CatalogueEntry[] = [];
    private ready: Promise<void> = Promise.resolve();

    /** A role's ticks need the catalogue, so its load waits for it. */
    protected load = async (id: string) => {
        await this.ready;
        return getRole(id);
    };
    protected create = createRole;
    protected update = updateRole;
    protected destroy = deleteRole;
    protected toDraft = (r: RoleDetail) => toDraft(r, this.catalogue);
    protected toForm = toForm;
    protected titleOf = (r: RoleDetail) => r.name;

    protected emptyDraft(): RoleDraft {
        return { grants: grantsOf(this.catalogue, []) };
    }

    protected attached(r: RoleDetail) {
        return {
            sections: [],
            holds: r.users ? (r.users === 1 ? "1 user" : `${r.users} users`) : undefined,
        };
    }

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") ? "holds" : "hold"} it. Give ${holds.startsWith("1 ") ? "them" : "each"} another role first, or keep this one.`;
    }

    protected loaded(r: RoleDetail) {
        this.store.set(m.record.role, flagsOf(r));
    }

    onInit() {
        this.addTrigger("implied", [m.record.draft.grants], (grants) => {
            const fixed = grants && withImplied(grants);
            if (fixed) this.store.set(m.record.draft.grants, fixed);
        });
        this.addTrigger("adding", [m.record.id], (id) => {
            if (!id) this.store.delete(m.record.role);
        });

        this.ready = getRoleCatalogue()
            .then((catalogue) => {
                this.catalogue = catalogue;
                // A new role opened before the catalogue arrived: its ticks, all clear, are where it starts.
                if (!this.store.get(m.record.id)) {
                    this.store.set(m.record.draft.grants, grantsOf(catalogue, []));
                    this.rebase();
                }
            })
            .catch(() => {
                this.store.set(m.record.error, "The permissions could not be loaded.");
            });

        super.onInit();
    }

    async reset() {
        const id = this.store.get(m.record.id);
        if (!id) return;

        const confirmed = await confirm({
            title: "Reset this role to its defaults?",
            message: "Its description and permissions go back to what a new installation gives it.",
            confirmText: "Reset role",
            cancelText: "Keep as it is",
        });
        if (!confirmed) return;

        try {
            await resetRole(id);
            this.reload();
        } catch (error) {
            this.store.set(
                m.record.error,
                error instanceof ApiError ? error.message : "The role could not be reset.",
            );
        }
    }
}
