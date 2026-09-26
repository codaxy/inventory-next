import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Locations: A to Z, each opening its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Locations",
                noun: "locations",
                placeholder: "Search name, street, city…",
                newHref: "~/company/locations/new",
                addText: "Add location",
                href: (id) => `~/company/locations/${id}`,
                columns: "location-columns",
                cells: [
                    {
                        header: "Name",
                        sort: "name",
                        cell: (
                            <cx>
                                <span class="record-title" text={r.name} />
                            </cx>
                        ),
                    },
                    {
                        header: "Address",
                        sort: "city",
                        cell: (
                            <cx>
                                <span class="record-meta" text={r.address} />
                            </cx>
                        ),
                    },
                    { header: "Room", cell: optionalCell(r.room) },
                    { header: "Assets", sort: "assets", cell: countCell(r.assets, r.assetsWord) },
                ],
            })}
        </div>
    </cx>
));
