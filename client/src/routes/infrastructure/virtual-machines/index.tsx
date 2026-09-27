import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Virtual machines: A to Z, each with how much information is kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Virtual machines",
                noun: "virtual machines",
                placeholder: "Search name or address…",
                newHref: "~/infrastructure/virtual-machines/new",
                addText: "Add virtual machine",
                href: (id) => `~/infrastructure/virtual-machines/${id}`,
                columns: "machine-columns",
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
                    { header: "IP address", cell: optionalCell(r.address) },
                    {
                        header: "Information",
                        sort: "information",
                        cell: countCell(r.information, r.informationWord),
                        numeric: true,
                    },
                ],
            })}
        </div>
    </cx>
));
