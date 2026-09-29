import { type OnVolumeItem, type Sort, software } from "../../../api/infrastructure";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, OnVolumeItem, Row, Sort> {
    protected readonly s = m.list;
    protected readonly exportHref = m.list.exportHref;
    protected readonly path = "~/infrastructure/software";
    protected readonly defaultSort = "name";
    protected readonly sorts = [
        "name",
        "-name",
        "license",
        "-license",
        "information",
        "-information",
    ] as const;
    protected readonly failure = "The software could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: Sort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return software.list(q);
    }

    protected exportUrl({ filters: _, ...q }: { q?: string; sort: Sort; filters: Filters }) {
        return software.export(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    sortBy(key: "name" | "license" | "information") {
        this.sortOn(key, key === "information");
    }

    clearSearch() {
        this.clearAll();
    }
}
