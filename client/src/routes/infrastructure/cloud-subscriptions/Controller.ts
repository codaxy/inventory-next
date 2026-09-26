import { type OnVolumeItem, type Sort, cloudSubscriptions } from "../../../api/infrastructure";
import { ListController } from "../../../listController";
import m, { type Filters, type Row, toRows } from "./model";

export default class extends ListController<Filters, OnVolumeItem, Row, Sort> {
    protected readonly s = m.list;
    protected readonly path = "~/infrastructure/cloud-subscriptions";
    protected readonly defaultSort = "name";
    protected readonly sorts = [
        "name",
        "-name",
        "license",
        "-license",
        "information",
        "-information",
    ] as const;
    protected readonly nouns = [
        "cloud subscription",
        "cloud subscriptions",
        "No cloud subscriptions",
    ] as const;
    protected readonly failure = "The cloud subscriptions could not be loaded.";

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
        return cloudSubscriptions.list(q);
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
