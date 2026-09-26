import {
    createLocation,
    deleteLocation,
    getLocation,
    getLocationOptions,
    type LocationDetail,
    type LocationForm,
    updateLocation,
} from "../../../../api/locations";
import { RecordController } from "../../../../recordController";
import m, { type LocationDraft, toAttached, toDraft, toForm } from "./model";

const r = m.record;

export default class extends RecordController<LocationDraft, LocationDetail, LocationForm> {
    protected readonly r = r;
    protected readonly routeId = m.$route.id;
    protected readonly path = "~/company/locations";
    protected readonly noun = "location";
    protected readonly newTitle = "New location";

    protected load = getLocation;
    protected create = createLocation;
    protected update = updateLocation;
    protected destroy = deleteLocation;
    protected toDraft = toDraft;
    protected toForm = toForm;
    protected titleOf = (l: LocationDetail) => l.name;
    protected attached = toAttached;
    protected readonly fieldAliases = { countryCode: "countryId" };

    protected refusal(holds: string) {
        return `${holds} ${holds.startsWith("1 ") && !holds.includes(" and ") ? "is" : "are"} here. Move ${holds.startsWith("1 ") && !holds.includes(" and ") ? "it" : "them"} elsewhere first, or keep the location.`;
    }

    onInit() {
        this.store.set(r.options, { countries: [], allCities: [], allStates: [], cities: [], states: [] });
        super.onInit();

        // The city and state pickers offer only the chosen country's; one of another country goes.
        this.addTrigger(
            "country",
            [r.draft.countryId, r.options.allCities, r.options.allStates],
            (country, cities, states) => {
                const of = <T extends { countryCode: string }>(list: T[] | undefined) =>
                    (list ?? []).filter((x) => x.countryCode === country);
                this.store.set(r.options.cities, of(cities));
                this.store.set(r.options.states, of(states));
                const draft = this.store.get(r.draft);
                if (draft.cityId && !of(cities).some((c) => c.id === draft.cityId) && cities?.length)
                    this.store.update(r.draft, (d) => ({ ...d, cityId: undefined, cityText: undefined }));
                if (draft.stateId && !of(states).some((s) => s.id === draft.stateId) && states?.length)
                    this.store.update(r.draft, (d) => ({ ...d, stateId: undefined, stateText: undefined }));
            },
        );

        getLocationOptions()
            .then((o) => {
                this.store.update(r.options, (x) => ({
                    ...x,
                    countries: o.countries,
                    allCities: o.cities,
                    allStates: o.states,
                }));
                this.preselect();
            })
            .catch(() => {});
        this.addTrigger("new", [r.id, r.loading], () => this.preselect());
    }

    /**
     * A new location starts in the only country there is, and its only city: the choice already
     * made. Where the form starts, not an edit to guard.
     */
    private preselect() {
        if (this.store.get(r.id) !== null || this.store.get(r.draft.countryId)) return;
        const { countries, allCities } = this.store.get(r.options);
        if (countries.length !== 1) return;
        const country = countries[0];
        const cities = allCities.filter((c) => c.countryCode === country.id);
        this.store.update(r.draft, (d) => ({
            ...d,
            countryId: country.id,
            countryText: country.text,
            ...(cities.length === 1 ? { cityId: cities[0].id, cityText: cities[0].text } : {}),
        }));
        this.rebase();
    }
}
