import { listLocations, type LocationItem, type LocationSort } from "../../../api/locations";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, LocationItem, Row, LocationSort> {
    protected readonly s = m.list;
    protected readonly path = "~/company/locations";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "city", "-city", "assets", "-assets"] as const;
    protected readonly nouns = ["location", "locations", "No locations"] as const;
    protected readonly failure = "The locations could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: LocationSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listLocations(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** Text A to Z, counts largest first; then the other way. */
    sortBy(key: "name" | "city" | "assets") {
        this.sortOn(key, key === "assets");
    }

    clearSearch() {
        this.clearAll();
    }
}
