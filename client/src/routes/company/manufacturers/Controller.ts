import { listManufacturers, type ManufacturerItem, type ManufacturerSort } from "../../../api/manufacturers";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, ManufacturerItem, Row, ManufacturerSort> {
    protected readonly s = m.list;
    protected readonly path = "~/company/manufacturers";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "devices", "-devices", "software", "-software"] as const;
    protected readonly failure = "The manufacturers could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: ManufacturerSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listManufacturers(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** Text A to Z, counts largest first; then the other way. */
    sortBy(key: "name" | "devices" | "software") {
        this.sortOn(key, key === "devices" || key === "software");
    }

    clearSearch() {
        this.clearAll();
    }
}
