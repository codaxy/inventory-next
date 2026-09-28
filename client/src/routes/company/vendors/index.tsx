import { createFunctionalComponent } from "cx/ui";

import { countCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";
import { copyButton, copyCell } from "../../../components/copyButton";

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
                                <span class="record-title record-copy">
                                    <span class="record-copy-line">
                                        <span text={r.name} />
                                        {copyButton(r.name, "name")}
                                    </span>
                                </span>
                            </cx>
                        ),
                    },
                    { header: "Contact", cell: copyCell(r.contact, "contact") },
                    { header: "Email", cell: copyCell(r.email, "email") },
                    { header: "Phone", cell: copyCell(r.phone, "phone") },
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
