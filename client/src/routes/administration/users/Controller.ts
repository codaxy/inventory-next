import { listUsers, type UserItem, type UserSort } from "../../../api/users";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, UserItem, Row, UserSort> {
    protected readonly s = m.list;
    protected readonly path = "~/administration/users";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "email", "-email", "role", "-role"] as const;
    protected readonly failure = "The users could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: UserSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listUsers(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** A to Z; the role in authority order, System Admin first. */
    sortBy(key: "name" | "email" | "role") {
        this.sortOn(key, false);
    }

    clearSearch() {
        this.clearAll();
    }
}
