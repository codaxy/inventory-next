import {
    type ActivationItem,
    type ActivationSort,
    type Expiry,
    type ActivationQuery,
    activationsExport,
    getActivationOptions,
    listActivations,
    type NumberedOption,
    type VolumeRef,
} from "../../../api/activations";
import { type AddressValue, oneOf } from "../../../listAddress";
import { ListController } from "../../../listController";
import { volumeText, withNumber } from "../../../inventoryNumbers";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;

const licenseText = (l: NumberedOption | undefined) => l && withNumber(l.name, l.number);
const volumeRefText = (v: VolumeRef | undefined) => v && volumeText(v);
const keys = ["activated", "software", "license", "assignee", "deactivated"] as const;

const request = (f: Filters): Partial<ActivationQuery> => ({
    softwareId: f.softwareId ?? undefined,
    licenseId: f.licenseId ?? undefined,
    volumeId: f.volumeId ?? undefined,
    personId: f.personId ?? undefined,
    status: f.status ?? undefined,
    expiry: f.expiry ?? undefined,
});

export default class extends ListController<Filters, ActivationItem, Row, ActivationSort, FilterKey> {
    protected readonly s = s;
    protected readonly exportHref = s.exportHref;
    protected readonly path = "~/licenses/activations";
    protected readonly defaultSort = "-activated";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as ActivationSort[]);
    protected readonly failure = "The activations could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: ActivationSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listActivations({ ...q, ...request(f) });
    }

    protected exportUrl({ filters, ...q }: { q?: string; sort: ActivationSort; filters: Filters }) {
        return activationsExport({ ...q, ...request(filters) });
    }

    protected toRows = toRows;
    protected toChips = (f: Filters) => toChips(f, this.store.get(s.licenses), this.store.get(s.volumes));

    protected filtersFrom = (query: URLSearchParams): Filters => ({
        softwareId: query.get("softwareId"),
        licenseId: query.get("licenseId"),
        volumeId: query.get("volumeId"),
        personId: query.get("personId"),
        status: oneOf(query, "status", ["active", "deactivated"] as const),
        expiry: oneOf(query, "expiry", ["expired", "soon", "regular", "none"] as const) as
            Expiry | "none" | null,
    });

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        softwareId: f.softwareId,
        licenseId: f.licenseId,
        volumeId: f.volumeId,
        personId: f.personId,
        status: f.status,
        expiry: f.expiry,
    });

    protected without(f: Filters, key: FilterKey): Filters {
        return key === "software" || key === "license" || key === "volume" || key === "person"
            ? { ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined }
            : { ...f, [key]: undefined };
    }

    onInit() {
        super.onInit();

        // A software is broader than a license: a license with no volume of it is cleared, whether the
        // software changed or both came in the address. The volumes, from the options, say which
        // license covers which software; the software is never cleared by a license.
        this.addTrigger(
            "license-fits",
            [s.filters.licenseId, s.filters.softwareId, s.volumes],
            (license, software, volumes) => {
                if (!license || !software || !volumes?.length) return;
                if (volumes.some((v) => v.licenseId === license && v.softwareId === software)) return;
                this.store.update(s.filters, (f) => ({ ...f, licenseId: undefined, licenseText: undefined }));
            },
        );

        // The volume is the narrowest filter and lives inside one license and one software: a license
        // or software it does not belong to clears it, so the pane never holds filters that contradict
        // each other and match nothing. Its options arriving late re-checks one the address set.
        this.addTrigger(
            "volume-fits",
            [s.filters.licenseId, s.filters.softwareId, s.volumes],
            (license, software, volumes) => {
                const volumeId = this.store.get(s.filters.volumeId);
                const volume = volumes?.find((v) => v.id === volumeId);
                if (!volume) return;
                if ((license && volume.licenseId !== license) || (software && volume.softwareId !== software))
                    this.store.update(s.filters, (f) => ({
                        ...f,
                        volumeId: undefined,
                        volumeText: undefined,
                    }));
            },
        );
    }

    protected loadOptions() {
        this.store.set(s.software, []);
        this.store.set(s.licenses, []);
        this.store.set(s.volumes, []);
        this.store.set(s.people, []);
        getActivationOptions()
            .then((o) => {
                this.store.set(s.software, o.software);
                this.store.set(s.licenses, o.licenses);
                this.store.set(s.volumes, o.volumes);
                this.store.set(s.people, o.people);
                // A filter the address set has only its id; its name comes with the options.
                this.store.update(s.filters, (f) => ({
                    ...f,
                    softwareText: f.softwareText ?? o.software.find((x) => x.id === f.softwareId)?.text,
                    licenseText: f.licenseText ?? licenseText(o.licenses.find((x) => x.id === f.licenseId)),
                    volumeText: f.volumeText ?? volumeRefText(o.volumes.find((x) => x.id === f.volumeId)),
                    personText: f.personText ?? o.people.find((x) => x.id === f.personId)?.text,
                }));
            })
            .catch(() => {});
    }

    /** Dates newest first, text A to Z; then the other way. */
    sortBy(key: (typeof keys)[number]) {
        this.sortOn(key, key === "activated" || key === "deactivated");
    }

    setStatus(status: Filters["status"]) {
        this.store.set(s.filters.status, status);
    }

    setExpiry(expiry: Filters["expiry"]) {
        this.store.set(s.filters.expiry, expiry);
    }
}
