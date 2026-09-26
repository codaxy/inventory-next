import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Manufacturers: A to Z, each opening its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Manufacturers",
                noun: "manufacturers",
                placeholder: "Search name or URL…",
                newHref: "~/company/manufacturers/new",
                addText: "Add manufacturer",
                href: (id) => `~/company/manufacturers/${id}`,
                columns: "manufacturer-columns",
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
                    { header: "URL", cell: optionalCell(r.url, "record-muted") },
                    { header: "Devices", sort: "devices", cell: countCell(r.devices, r.devicesWord) },
                    { header: "Software", sort: "software", cell: countCell(r.software, r.softwareWord) },
                ],
            })}
        </div>
    </cx>
));
