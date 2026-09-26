import type { Option } from "../../api/assets";
import {
    getInformationOptions,
    type InformationItem,
    type InformationQuery,
    type InformationSort,
    informationExport,
    listInformation,
} from "../../api/informations";
import type { AddressValue } from "../../listAddress";
import { ListController } from "../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;
const keys = ["name", "type", "assignee", "author", "project"] as const;
/** The pickers the pane filters by, each an id in the address and a text from the options. */
const picks = ["type", "person", "project", "tag", "location"] as const;

const request = (f: Filters): Partial<InformationQuery> => ({
    typeId: f.typeId ?? undefined,
    personId: f.personId ?? undefined,
    projectId: f.projectId ?? undefined,
    tagId: f.tagId ?? undefined,
    locationId: f.locationId ?? undefined,
    incomplete: f.incomplete ?? undefined,
});

export default class extends ListController<Filters, InformationItem, Row, InformationSort, FilterKey> {
    protected readonly s = s;
    protected readonly exportHref = s.exportHref;
    protected readonly path = "~/informations";
    protected readonly defaultSort = "name";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as InformationSort[]);
    protected readonly nouns = ["piece of information", "pieces of information", "No information"] as const;
    protected readonly failure = "The information could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: InformationSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listInformation({ ...q, ...request(f) });
    }

    protected exportUrl({ filters, ...q }: { q?: string; sort: InformationSort; filters: Filters }) {
        return informationExport({ ...q, ...request(filters) });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    protected filtersFrom(query: URLSearchParams): Filters {
        const incomplete = query.get("incomplete");
        return {
            typeId: query.get("typeId"),
            personId: query.get("personId"),
            projectId: query.get("projectId"),
            tagId: query.get("tagId"),
            locationId: query.get("locationId"),
            incomplete: incomplete === "true" ? true : incomplete === "false" ? false : null,
        };
    }

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        typeId: f.typeId,
        personId: f.personId,
        projectId: f.projectId,
        tagId: f.tagId,
        locationId: f.locationId,
        incomplete: f.incomplete == null ? undefined : String(f.incomplete),
    });

    protected without(f: Filters, key: FilterKey): Filters {
        if (key === "incomplete") return { ...f, incomplete: undefined };
        return { ...f, [`${key}Id`]: undefined, [`${key}Text`]: undefined };
    }

    protected loadOptions() {
        for (const key of [s.types, s.people, s.projects, s.tags, s.locations]) this.store.set(key, []);
        getInformationOptions()
            .then((o) => {
                const lists: Record<(typeof picks)[number], Option[]> = {
                    type: o.types,
                    person: o.people,
                    project: o.projects,
                    tag: o.tags,
                    location: o.locations,
                };
                this.store.set(s.types, o.types);
                this.store.set(s.people, o.people);
                this.store.set(s.projects, o.projects);
                this.store.set(s.tags, o.tags);
                this.store.set(s.locations, o.locations);
                // A filter the address carried has only its id until the names arrive.
                this.store.update(s.filters, (f) => {
                    const named: Record<string, unknown> = { ...f };
                    for (const key of picks)
                        named[`${key}Text`] ??= lists[key].find((x) => x.id === named[`${key}Id`])?.text;
                    return named as Filters;
                });
            })
            .catch(() => {});
    }

    sortBy(key: (typeof keys)[number]) {
        this.sortOn(key);
    }

    setIncomplete(incomplete: Filters["incomplete"]) {
        this.store.set(s.filters.incomplete, incomplete);
    }
}
