import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Cloud subscriptions: A to Z, each with how much information is kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Cloud subscriptions",
                noun: "cloud subscriptions",
                placeholder: "Search name, license, software…",
                newHref: "~/infrastructure/cloud-subscriptions/new",
                newText: "New cloud subscription",
                href: (id) => `~/infrastructure/cloud-subscriptions/${id}`,
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
