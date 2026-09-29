import { createFunctionalComponent } from "cx/ui";

import { countCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";
import { copyButton, copyCell } from "../../../components/copyButton";

const r = m.$row;

/** Manufacturers: A to Z, each opening its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide list-fill" controller={Controller}>
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
                    { header: "URL", cell: copyCell(r.url, "URL", "record-muted") },
                    {
                        header: "Devices",
                        sort: "devices",
                        numeric: true,
                        cell: countCell(r.devices, r.devicesWord),
                    },
                    {
                        header: "Software",
                        sort: "software",
                        numeric: true,
                        cell: countCell(r.software, r.softwareWord),
                    },
                ],
            })}
        </div>
    </cx>
));
