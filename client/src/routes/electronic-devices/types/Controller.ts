import { getTypeOptions, listTypes, type TypeItem, type TypeSort } from "../../../api/electronicDeviceTypes";
import type { AddressValue } from "../../../listAddress";
import { ListController } from "../../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const sorts = ["name", "-name", "tags", "-tags", "devices", "-devices"] as const satisfies TypeSort[];

export default class extends ListController<Filters, TypeItem, Row, TypeSort, FilterKey> {
    protected readonly s = m.types;
    protected readonly path = "~/electronic-devices/types";
    protected readonly defaultSort = "name";
    protected readonly sorts = sorts;
    protected readonly nouns = ["type", "types", "No types"] as const;
    protected readonly failure = "The types could not be loaded.";

    protected fetch({
        filters,
        ...q
    }: {
        q?: string;
        sort: TypeSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listTypes({
            ...q,
            tagIds: filters.tags?.map((t) => t.id),
            holdsLicences: filters.holdsLicences ?? undefined,
        });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    protected filtersFrom(query: URLSearchParams): Filters {
        const holds = query.get("holdsLicences");
        return {
            // The names come with the options; until then a chip reads "Tag".
            tags: query.getAll("tagId").map((id) => ({ id, text: "" })),
            holdsLicences: holds === "true" ? true : holds === "false" ? false : null,
        };
    }

    protected filtersTo(f: Filters): Record<string, AddressValue> {
        return {
            tagId: f.tags?.map((t) => t.id),
            holdsLicences: f.holdsLicences == null ? undefined : String(f.holdsLicences),
        };
    }

    protected without(f: Filters, key: FilterKey): Filters {
        if (key === "holdsLicences") return { ...f, holdsLicences: null };
        const id = key.slice("tag:".length);
        return { ...f, tags: (f.tags ?? []).filter((t) => t.id !== id) };
    }

    protected loadOptions() {
        this.store.set(m.types.tagOptions, []);
        getTypeOptions()
            .then((o) => {
                this.store.set(m.types.tagOptions, o.tags);
                this.store.update(m.types.filters, (f) => ({
                    ...f,
                    tags: f.tags?.map((t) => ({
                        ...t,
                        text: t.text || (o.tags.find((x) => x.id === t.id)?.text ?? ""),
                    })),
                }));
            })
            .catch(() => {});
    }

    sortBy(key: "name" | "tags" | "devices") {
        this.sortOn(key);
    }

    setHoldsLicences(value: boolean | null) {
        this.store.set(m.types.filters.holdsLicences, value);
    }
}
