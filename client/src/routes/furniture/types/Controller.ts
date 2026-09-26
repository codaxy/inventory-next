import {
    type FurnitureTypeItem,
    type FurnitureTypeSort,
    listFurnitureTypes,
} from "../../../api/furnitureTypes";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, FurnitureTypeItem, Row, FurnitureTypeSort> {
    protected readonly s = m.types;
    protected readonly path = "~/furniture/types";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "furniture", "-furniture"] as const;
    protected readonly nouns = ["type", "types", "No types"] as const;
    protected readonly failure = "The types could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: FurnitureTypeSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listFurnitureTypes(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** The name A to Z, the count largest first; then the other way. */
    sortBy(key: "name" | "furniture") {
        this.sortOn(key, key === "furniture");
    }

    clearSearch() {
        this.clearAll();
    }
}
