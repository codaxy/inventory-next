import { createFunctionalComponent } from "cx/ui";

import { countCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";
import { copyButton, copyCell } from "../../../components/copyButton";

const r = m.$row;

/** Locations: A to Z, each opening its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide list-fill" controller={Controller}>
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
                exportHref: m.list.exportHref,
                cells: [
                    {
                        header: "Name",
                        sort: "name",
                        cell: (
                            <cx>
                                <span class="record-title record-copy">
                                    <span class="record-copy-line">
                                        <span text={r.name} />
                                        {copyButton(r.name, "name")}
                                    </span>
                                </span>
                            </cx>
                        ),
                    },
                    {
                        header: "Address",
                        sort: "city",
                        cell: <cx>{copyCell(r.address, "IP address")}</cx>,
                    },
                    { header: "Room", cell: copyCell(r.room, "room") },
                    {
                        header: "Assets",
                        sort: "assets",
                        numeric: true,
                        cell: countCell(r.assets, r.assetsWord),
                    },
                ],
            })}
        </div>
    </cx>
));
