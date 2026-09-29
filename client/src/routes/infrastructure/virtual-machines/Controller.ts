import { type MachineItem, type Sort, virtualMachines } from "../../../api/infrastructure";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, MachineItem, Row, Sort> {
    protected readonly s = m.list;
    protected readonly exportHref = m.list.exportHref;
    protected readonly path = "~/infrastructure/virtual-machines";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "information", "-information"] as const;
    protected readonly failure = "The virtual machines could not be loaded.";

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
        return virtualMachines.list(q);
    }

    protected exportUrl({ filters: _, ...q }: { q?: string; sort: Sort; filters: Filters }) {
        return virtualMachines.export(q);
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
