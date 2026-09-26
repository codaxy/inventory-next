import type { Option } from "../../api/assets";
import {
    type FurnitureItem,
    type FurnitureQuery,
    type FurnitureSort,
    furnitureExport,
    getFurnitureOptions,
    listFurniture,
} from "../../api/furniture";
import { dayAfter, isDay } from "../../dates";
import type { AddressValue } from "../../listAddress";
import { ListController } from "../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;
const keys = ["number", "name", "assignee", "location", "type", "vendor", "value", "modified"] as const;
/** The pickers the pane filters by, each an id in the address and a text from the options. */
const picks = ["type", "vendor", "person", "location"] as const;

/** The filters as the API takes them: the reader's inclusive last day becomes the exclusive next one. */
const request = (f: Filters): Partial<FurnitureQuery> => ({
    typeId: f.typeId ?? undefined,
    vendorId: f.vendorId ?? undefined,
    personId: f.personId ?? undefined,
    locationId: f.locationId ?? undefined,
    purchasedFrom: f.from ?? undefined,
    purchasedTo: f.to ? dayAfter(f.to) : undefined,
    incomplete: f.incomplete ?? undefined,
});

export default class extends ListController<Filters, FurnitureItem, Row, FurnitureSort, FilterKey> {
    protected readonly s = s;
    protected readonly exportHref = s.exportHref;
    protected readonly path = "~/furniture";
    protected readonly defaultSort = "-modified";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as FurnitureSort[]);
    protected readonly nouns = ["piece of furniture", "pieces of furniture", "No furniture"] as const;
    protected readonly failure = "The furniture could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: FurnitureSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listFurniture({ ...q, ...request(f) });
    }

    protected exportUrl({ filters, ...q }: { q?: string; sort: FurnitureSort; filters: Filters }) {
        return furnitureExport({ ...q, ...request(filters) });
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
            vendorId: query.get("vendorId"),
            personId: query.get("personId"),
            locationId: query.get("locationId"),
            from: isDay(from) ? from : null,
            to: isDay(to) ? to : null,
            incomplete: incomplete === "true" ? true : incomplete === "false" ? false : null,
        };
    }

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        typeId: f.typeId,
        vendorId: f.vendorId,
        personId: f.personId,
        locationId: f.locationId,
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
        for (const key of [s.types, s.vendors, s.people, s.locations]) this.store.set(key, []);
        getFurnitureOptions()
            .then((o) => {
                const lists: Record<(typeof picks)[number], Option[]> = {
                    type: o.types,
                    vendor: o.vendors,
                    person: o.people,
                    location: o.locations,
                };
                this.store.set(s.types, o.types);
                this.store.set(s.vendors, o.vendors);
                this.store.set(s.people, o.people);
                this.store.set(s.locations, o.locations);
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

    /** Numbers, value and time largest or newest first, text A to Z; then the other way. */
    sortBy(key: (typeof keys)[number]) {
        this.sortOn(key, key === "number" || key === "value" || key === "modified");
    }

    setIncomplete(incomplete: Filters["incomplete"]) {
        this.store.set(s.filters.incomplete, incomplete);
    }
}
