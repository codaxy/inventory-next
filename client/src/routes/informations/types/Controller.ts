import { type GroupItem, type GroupSort, informationTypes } from "../../../api/informations";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, GroupItem, Row, GroupSort> {
    protected readonly s = m.list;
    protected readonly path = "~/informations/types";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "information", "-information"] as const;
    protected readonly failure = "The types could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: GroupSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return informationTypes.list(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    sortBy(key: "name" | "information") {
        this.sortOn(key, key === "information");
    }

    clearSearch() {
        this.clearAll();
    }
}
