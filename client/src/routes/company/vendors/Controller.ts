import { listVendors, type VendorItem, type VendorSort } from "../../../api/vendors";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, VendorItem, Row, VendorSort> {
    protected readonly s = m.list;
    protected readonly path = "~/company/vendors";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "assets", "-assets"] as const;
    protected readonly nouns = ["vendor", "vendors", "No vendors"] as const;
    protected readonly failure = "The vendors could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: VendorSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listVendors(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** Text A to Z, counts largest first; then the other way. */
    sortBy(key: "name" | "assets") {
        this.sortOn(key, key === "assets");
    }

    clearSearch() {
        this.clearAll();
    }
}
