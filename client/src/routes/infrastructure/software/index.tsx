import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Software: A to Z, each with how much information is kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Software",
                noun: "software",
                placeholder: "Search name, license, software…",
                newHref: "~/infrastructure/software/new",
                newText: "New software",
                href: (id) => `~/infrastructure/software/${id}`,
                columns: "on-volume-columns",
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
                        header: "License",
                        sort: "license",
                        cell: (
                            <cx>
                                <span class="record-meta" text={r.license} />
                            </cx>
                        ),
                    },
                    {
                        header: "Software or service",
                        cell: (
                            <cx>
                                <span class="record-meta" text={r.software} />
                            </cx>
                        ),
                    },
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
