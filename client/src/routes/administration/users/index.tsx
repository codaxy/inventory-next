import { createFunctionalComponent } from "cx/ui";

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
                                <span class="record-title">
                                    <span text={r.name} />
                                    <span
                                        class="record-flag record-flag-ended"
                                        visible={r.ended}
                                        text="Deactivated"
                                    />
                                </span>
                            </cx>
                        ),
                    },
                    {
                        header: "Email",
                        sort: "email",
                        cell: (
                            <cx>
                                <span class="record-meta" text={r.email} />
                            </cx>
                        ),
                    },
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
