import { listTags, type TagItem } from "../../../api/electronicDeviceTags";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, type TagSort, toRows } from "./model";

export default class extends ListController<Filters, TagItem, Row, TagSort> {
    protected readonly s = m.tags;
    protected readonly path = "~/electronic-devices/tags";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "types", "-types"] as const;
    protected readonly nouns = ["tag", "tags", "No tags"] as const;
    protected readonly failure = "The tags could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: TagSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listTags(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    sortBy(key: "name" | "types") {
        this.sortOn(key);
    }

    clearSearch() {
        this.clearAll();
    }
}
