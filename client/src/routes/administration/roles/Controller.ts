import { listRoles, type RoleItem, type RoleSort } from "../../../api/roles";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, RoleItem, Row, RoleSort> {
    protected readonly s = m.list;
    protected readonly path = "~/administration/roles";
    protected readonly defaultSort = "rank";
    protected readonly sorts = ["rank"] as const;
    protected readonly failure = "The roles could not be loaded.";

    protected fetch({ q, page, pageSize }: { q?: string; page: number; pageSize: number }) {
        return listRoles({ q, page, pageSize });
    }

    protected toRows = toRows;
    protected toChips = () => [];
    protected filtersFrom = (): Filters => ({});
    protected filtersTo = () => ({});
    protected without = (f: Filters) => f;

    clearSearch() {
        this.clearAll();
    }
}
