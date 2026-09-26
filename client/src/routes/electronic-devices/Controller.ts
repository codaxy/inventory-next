import type { Option } from "../../api/assets";
import {
    type DeviceItem,
    type DeviceQuery,
    type DeviceSort,
    devicesExport,
    getDeviceOptions,
    listDevices,
} from "../../api/electronicDevices";
import { dayAfter, isDay } from "../../dates";
import type { AddressValue } from "../../listAddress";
import { ListController } from "../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;
const keys = ["number", "name", "model", "assignee", "location", "type", "manufacturer", "modified"] as const;
/** The pickers the pane filters by, each an id in the address and a text from the options. */
const picks = ["type", "tag", "person", "vendor", "location", "manufacturer"] as const;

/** The filters as the API takes them: the reader's inclusive last day becomes the exclusive next one. */
const request = (f: Filters): Partial<DeviceQuery> => ({
    typeId: f.typeId ?? undefined,
    tagId: f.tagId ?? undefined,
    personId: f.personId ?? undefined,
    vendorId: f.vendorId ?? undefined,
    locationId: f.locationId ?? undefined,
    manufacturerId: f.manufacturerId ?? undefined,
    purchasedFrom: f.from ?? undefined,
    purchasedTo: f.to ? dayAfter(f.to) : undefined,
    incomplete: f.incomplete ?? undefined,
});

export default class extends ListController<Filters, DeviceItem, Row, DeviceSort, FilterKey> {
    protected readonly s = s;
    protected readonly exportHref = s.exportHref;
    protected readonly path = "~/electronic-devices";
    protected readonly defaultSort = "-modified";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as DeviceSort[]);
    protected readonly nouns = ["device", "devices", "No devices"] as const;
    protected readonly failure = "The devices could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: DeviceSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listDevices({ ...q, ...request(f) });
    }

    protected exportUrl({ filters, ...q }: { q?: string; sort: DeviceSort; filters: Filters }) {
        return devicesExport({ ...q, ...request(filters) });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    /** The purchase dates travel as the days the reader chose, both included. */
    protected filtersFrom(query: URLSearchParams): Filters {
        const incomplete = query.get("incomplete");
        const from = query.get("purchasedFrom");
        const to = query.get("purchasedTo");
        return {
            typeId: query.get("typeId"),
            tagId: query.get("tagId"),
            personId: query.get("personId"),
            vendorId: query.get("vendorId"),
            locationId: query.get("locationId"),
            manufacturerId: query.get("manufacturerId"),
            from: isDay(from) ? from : null,
            to: isDay(to) ? to : null,
            incomplete: incomplete === "true" ? true : incomplete === "false" ? false : null,
        };
    }

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        typeId: f.typeId,
        tagId: f.tagId,
        personId: f.personId,
        vendorId: f.vendorId,
        locationId: f.locationId,
        manufacturerId: f.manufacturerId,
        purchasedFrom: f.from,
        purchasedTo: f.to,
        incomplete: f.incomplete == null ? undefined : String(f.incomplete),
    });

    protected without(f: Filters, key: FilterKey): Filters {
        if (key === "range") return { ...f, from: undefined, to: undefined };
        if (key === "incomplete") return { ...f, incomplete: undefined };
        return { ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined };
    }

    protected loadOptions() {
        for (const key of [s.types, s.tags, s.people, s.vendors, s.locations, s.manufacturers])
            this.store.set(key, []);
        getDeviceOptions()
            .then((o) => {
                const lists: Record<(typeof picks)[number], Option[]> = {
                    type: o.types,
                    tag: o.tags,
                    person: o.people,
                    vendor: o.vendors,
                    location: o.locations,
                    manufacturer: o.manufacturers,
                };
                this.store.set(s.types, o.types);
                this.store.set(s.tags, o.tags);
                this.store.set(s.people, o.people);
                this.store.set(s.vendors, o.vendors);
                this.store.set(s.locations, o.locations);
                this.store.set(s.manufacturers, o.manufacturers);
                // A filter the address carried has only its id until the names arrive.
                this.store.update(s.filters, (f) => {
                    const named: Record<string, unknown> = { ...f };
                    for (const key of picks)
                        named[`${key}Text`] ??= lists[key].find((x) => x.id === named[`${key}Id`])?.text;
                    return named as Filters;
                });
            })
            .catch(() => {});
    }

    /** The number and the change time newest first, text A to Z; then the other way. */
    sortBy(key: (typeof keys)[number]) {
        this.sortOn(key, key === "number" || key === "modified");
    }

    setIncomplete(incomplete: Filters["incomplete"]) {
        this.store.set(s.filters.incomplete, incomplete);
    }
}
