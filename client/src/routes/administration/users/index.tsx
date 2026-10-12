import { createFunctionalComponent } from "cx/ui";

import { copyButton, copyCell } from "../../../components/copyButton";
import { searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Who may sign in: every user added, the deactivated kept as history. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide list-fill" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Users",
                noun: "users",
                placeholder: "Search name, email, role…",
                newHref: "~/administration/users/new",
                addText: "Add user",
                href: (id) => `~/administration/users/${id}`,
                columns: "user-columns",
                ended: r.ended,
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
                                        <span
                                            class="record-flag record-flag-ended"
                                            visible={r.ended}
                                            text="Deactivated"
                                        />
                                    </span>
                                </span>
                            </cx>
                        ),
                    },
                    { header: "Email", sort: "email", cell: copyCell(r.email, "email") },
                    {
                        header: "Role",
                        sort: "role",
                        cell: (
                            <cx>
                                <span class="record-meta" text={r.role} />
                            </cx>
                        ),
                    },
                ],
            })}
        </div>
    </cx>
));
