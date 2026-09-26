import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Information types: A to Z, each with how much information is of it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Information types",
                noun: "types",
                placeholder: "Search types…",
                newHref: "~/informations/types/new",
                addText: "Add type",
                href: (id) => `~/informations/types/${id}`,
                columns: "information-group-columns",
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
                    { header: "Description", cell: optionalCell(r.description, "record-muted") },
                    {
                        header: "Information",
                        sort: "information",
                        cell: countCell(r.information, r.informationWord),
                    },
                ],
            })}
        </div>
    </cx>
));
