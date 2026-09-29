import {
    getProjectOptions,
    listProjects,
    projectsExport,
    type ProjectItem,
    type ProjectSort,
} from "../../../api/projects";
import type { AddressValue } from "../../../listAddress";
import { ListController } from "../../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;

export default class extends ListController<Filters, ProjectItem, Row, ProjectSort, FilterKey> {
    protected readonly s = s;
    protected readonly exportHref = s.exportHref;
    protected readonly path = "~/company/projects";
    protected readonly defaultSort = "name";
    protected readonly sorts = ["name", "-name", "client", "-client", "owner", "-owner"] as const;
    protected readonly failure = "The projects could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: ProjectSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listProjects({ ...q, clientId: f.clientId ?? undefined, personId: f.personId ?? undefined });
    }

    protected exportUrl({ filters: f, ...q }: { q?: string; sort: ProjectSort; filters: Filters }) {
        return projectsExport({ ...q, clientId: f.clientId ?? undefined, personId: f.personId ?? undefined });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    protected filtersFrom = (query: URLSearchParams): Filters => ({
        clientId: query.get("clientId"),
        personId: query.get("personId"),
    });

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        clientId: f.clientId,
        personId: f.personId,
    });

    protected without = (f: Filters, key: FilterKey): Filters => ({
        ...f,
        [`${key}Id`]: undefined,
        [`${key}Text`]: undefined,
    });

    protected loadOptions() {
        this.store.set(s.clients, []);
        this.store.set(s.people, []);
        getProjectOptions()
            .then((o) => {
                this.store.set(s.clients, o.clients);
                this.store.set(s.people, o.people);
                // A filter the address carried has only its id until the names arrive.
                this.store.update(s.filters, (f) => ({
                    ...f,
                    clientText: f.clientText ?? o.clients.find((x) => x.id === f.clientId)?.text,
                    personText: f.personText ?? o.people.find((x) => x.id === f.personId)?.text,
                }));
            })
            .catch(() => {});
    }

    sortBy(key: "name" | "client" | "owner") {
        this.sortOn(key);
    }
}
