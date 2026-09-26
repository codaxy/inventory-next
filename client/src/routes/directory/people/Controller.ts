import { listPeople, type PersonItem, type PersonSort } from "../../../api/people";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, PersonItem, Row, PersonSort> {
    protected readonly s = m.people;
    protected readonly path = "~/directory/people";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "email", "-email", "assets", "-assets"] as const;
    protected readonly nouns = ["person", "people", "No people"] as const;
    protected readonly failure = "The people could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: PersonSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listPeople(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** Text A to Z, the count largest first; then the other way. */
    sortBy(key: "name" | "email" | "assets") {
        this.sortOn(key, key === "assets");
    }

    clearSearch() {
        this.clearAll();
    }
}
