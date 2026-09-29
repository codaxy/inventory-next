import { createFunctionalComponent } from "cx/ui";

import { countCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";
import { copyButton, copyCell } from "../../../components/copyButton";

const r = m.$row;

/** Software: A to Z, each with how much information is kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide list-fill" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Software",
                noun: "software",
                placeholder: "Search name, license, software…",
                newHref: "~/infrastructure/software/new",
                addText: "Add software",
                href: (id) => `~/infrastructure/software/${id}`,
                columns: "on-volume-columns",
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
                        header: "License",
                        sort: "license",
                        cell: <cx>{copyCell(r.license, "license")}</cx>,
                    },
                    {
                        header: "Software or service",
                        cell: <cx>{copyCell(r.software, "software")}</cx>,
                    },
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
