import { createFunctionalComponent } from "cx/ui";

import { countCell, optionalCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Vendors: A to Z, each opening its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Vendors",
                noun: "vendors",
                placeholder: "Search name, contact, email, VAT…",
                newHref: "~/company/vendors/new",
                addText: "Add vendor",
                href: (id) => `~/company/vendors/${id}`,
                columns: "vendor-columns",
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
                    { header: "Contact", cell: optionalCell(r.contact) },
                    { header: "Email", cell: optionalCell(r.email) },
                    { header: "Phone", cell: optionalCell(r.phone) },
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
