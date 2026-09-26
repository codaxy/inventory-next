import {
    type ActivationItem,
    type ActivationSort,
    type Expiry,
    getActivationOptions,
    listActivations,
} from "../../../api/activations";
import { type AddressValue, oneOf } from "../../../listAddress";
import { ListController } from "../../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;
const keys = ["activated", "software", "license", "assignee", "deactivated"] as const;

export default class extends ListController<Filters, ActivationItem, Row, ActivationSort, FilterKey> {
    protected readonly s = s;
    protected readonly path = "~/licenses/activations";
    protected readonly defaultSort = "-activated";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as ActivationSort[]);
    protected readonly nouns = ["activation", "activations", "No activations"] as const;
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
        return listActivations({
            ...q,
            softwareId: f.softwareId ?? undefined,
            licenseId: f.licenseId ?? undefined,
            volumeId: f.volumeId ?? undefined,
            status: f.status ?? undefined,
            expiry: f.expiry ?? undefined,
        });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    protected filtersFrom = (query: URLSearchParams): Filters => ({
        softwareId: query.get("softwareId"),
        licenseId: query.get("licenseId"),
        volumeId: query.get("volumeId"),
        status: oneOf(query, "status", ["active", "deactivated"] as const),
        expiry: oneOf(query, "expiry", ["expired", "soon", "regular", "none"] as const) as
            Expiry | "none" | null,
    });

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        softwareId: f.softwareId,
        licenseId: f.licenseId,
        volumeId: f.volumeId,
        status: f.status,
        expiry: f.expiry,
    });

    protected without(f: Filters, key: FilterKey): Filters {
        return key === "software" || key === "license" || key === "volume"
            ? { ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined }
            : { ...f, [key]: undefined };
    }

    protected loadOptions() {
        this.store.set(s.software, []);
        this.store.set(s.licenses, []);
        this.store.set(s.volumes, []);
        getActivationOptions()
            .then((o) => {
                this.store.set(s.software, o.software);
                this.store.set(s.licenses, o.licenses);
                this.store.set(s.volumes, o.volumes);
                // A filter the address set has only its id; its name comes with the options.
                this.store.update(s.filters, (f) => ({
                    ...f,
                    softwareText: f.softwareText ?? o.software.find((x) => x.id === f.softwareId)?.text,
                    licenseText: f.licenseText ?? o.licenses.find((x) => x.id === f.licenseId)?.text,
                    volumeText: f.volumeText ?? o.volumes.find((x) => x.id === f.volumeId)?.text,
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
