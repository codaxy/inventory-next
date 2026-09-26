import { type GroupItem, type GroupSort, informationTags } from "../../../api/informations";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, GroupItem, Row, GroupSort> {
    protected readonly s = m.list;
    protected readonly path = "~/informations/tags";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "information", "-information"] as const;
    protected readonly nouns = ["tag", "tags", "No tags"] as const;
    protected readonly failure = "The tags could not be loaded.";

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
        return informationTags.list(q);
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
