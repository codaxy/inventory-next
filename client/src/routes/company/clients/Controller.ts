import { type ClientItem, type ClientSort, listClients } from "../../../api/clients";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, ClientItem, Row, ClientSort> {
    protected readonly s = m.clients;
    protected readonly path = "~/company/clients";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "projects", "-projects"] as const;
    protected readonly nouns = ["client", "clients", "No clients"] as const;
    protected readonly failure = "The clients could not be loaded.";

    protected fetch({
        filters: _,
        ...q
    }: {
        q?: string;
        sort: ClientSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listClients(q);
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    /** The name A to Z, the count largest first; then the other way. */
    sortBy(key: "name" | "projects") {
        this.sortOn(key, key === "projects");
    }

    clearSearch() {
        this.clearAll();
    }
}
