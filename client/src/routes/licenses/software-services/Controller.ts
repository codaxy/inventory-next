import {
    getSoftwareServiceOptions,
    listSoftwareServices,
    type SoftwareServiceItem,
    type SoftwareServiceSort,
} from "../../../api/softwareServices";
import type { AddressValue } from "../../../listAddress";
import { ListController } from "../../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;

export default class extends ListController<
    Filters,
    SoftwareServiceItem,
    Row,
    SoftwareServiceSort,
    FilterKey
> {
    protected readonly s = s;
    protected readonly path = "~/licenses/software-services";
    protected readonly defaultSort = "name";
    protected readonly sorts = [
        "name",
        "-name",
        "category",
        "-category",
        "manufacturer",
        "-manufacturer",
        "volumes",
        "-volumes",
    ] as const;
    protected readonly nouns = ["entry", "entries", "None"] as const;
    protected readonly failure = "The list could not be loaded.";

    protected fetch({
        filters,
        ...q
    }: {
        q?: string;
        sort: SoftwareServiceSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listSoftwareServices({
            ...q,
            categoryId: filters.categoryId ?? undefined,
            manufacturerId: filters.manufacturerId ?? undefined,
        });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    protected filtersFrom = (query: URLSearchParams): Filters => ({
        categoryId: query.get("categoryId"),
        manufacturerId: query.get("manufacturerId"),
    });

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        categoryId: f.categoryId,
        manufacturerId: f.manufacturerId,
    });

    protected without = (f: Filters, key: FilterKey): Filters => ({
        ...f,
        [`${key}Id`]: undefined,
        [`${key}Text`]: undefined,
    });

    protected loadOptions() {
        this.store.set(s.categories, []);
        this.store.set(s.manufacturers, []);
        getSoftwareServiceOptions()
            .then((o) => {
                this.store.set(s.categories, o.categories);
                this.store.set(s.manufacturers, o.manufacturers);
                // A filter the address set has only its id; its name comes with the options.
                this.store.update(s.filters, (f) => ({
                    ...f,
                    categoryText: f.categoryText ?? o.categories.find((x) => x.id === f.categoryId)?.text,
                    manufacturerText:
                        f.manufacturerText ?? o.manufacturers.find((x) => x.id === f.manufacturerId)?.text,
                }));
            })
            .catch(() => {});
    }

    sortBy(key: "name" | "category" | "manufacturer" | "volumes") {
        this.sortOn(key);
    }
}
