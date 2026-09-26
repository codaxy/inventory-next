import {
    createInformation,
    deleteInformation,
    getInformation,
    getInformationOptions,
    type InformationDetail,
    type InformationForm,
    updateInformation,
} from "../../../api/informations";
import { importanceFor } from "../../../assets";
import { RecordController } from "../../../recordController";
import m, {
    emptyOptions,
    type InformationDraft,
    type PlaceRow,
    placeKey,
    targetsOf,
    toDraft,
    toForm,
} from "./model";

const r = m.record;

export default class extends RecordController<InformationDraft, InformationDetail, InformationForm> {
    protected readonly r = r;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/informations";
    protected readonly noun = "information";
    protected readonly newTitle = "New information";

    protected load = getInformation;
    protected create = createInformation;
    protected update = updateInformation;
    protected destroy = deleteInformation;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (i: InformationDetail) => i.name;

    protected emptyDraft(): InformationDraft {
        return {
            personalInformation: false,
            clientsPersonalInformation: false,
            incomplete: false,
            tags: [],
            places: [],
        };
    }

    protected deleteMessage() {
        const places = this.store.get(r.draft.places)?.length ?? 0;
        return places === 0
            ? "This cannot be undone."
            : `Where it is kept goes with it. This cannot be undone.`;
    }

    onInit() {
        this.store.set(r.options, emptyOptions);
        super.onInit();

        // Computed from the three weights as the server will; "—" until all three are chosen.
        this.addTrigger("weights", [r.draft, r.options], (draft, options) =>
            this.store.set(
                r.importance,
                draft ? importanceFor(draft as any, (options ?? emptyOptions) as any) : "—",
            ),
        );

        // A new place's target is of its kind: choosing another kind clears one that no longer is.
        this.addTrigger("place-kinds", [r.draft.places, r.options], (places, options) => {
            if (
                !places?.some(
                    (p) =>
                        !p.id && p.targetId && !targetsOf(options, p.kindId).some((t) => t.id === p.targetId),
                )
            )
                return;
            this.store.set(
                r.draft.places,
                places.map((p) =>
                    !p.id && p.targetId && !targetsOf(options, p.kindId).some((t) => t.id === p.targetId)
                        ? { ...p, targetId: undefined, targetText: undefined }
                        : p,
                ),
            );
        });

        getInformationOptions()
            .then((o) => this.store.set(r.options, { ...emptyOptions, ...o }))
            .catch(() => {});
    }

    addPlace() {
        this.store.update(r.draft.places, (places) => [
            ...(places ?? []),
            { key: placeKey() } satisfies PlaceRow,
        ]);
    }

    /** A place added in this edit goes at once; a saved one is struck through until the save, and can be kept. */
    removePlace(key: string) {
        this.store.update(r.draft.places, (places) =>
            (places ?? []).flatMap((p) => (p.key !== key ? [p] : p.id ? [{ ...p, removed: true }] : [])),
        );
    }

    keepPlace(key: string) {
        this.store.update(r.draft.places, (places) =>
            (places ?? []).map((p) => (p.key === key ? { ...p, removed: false } : p)),
        );
    }
}
