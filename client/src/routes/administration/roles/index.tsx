import { createFunctionalComponent, expr } from "cx/ui";

import { countCell, searchList } from "../../../components/searchList";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;

/** Roles in authority order: System Admin, the other built-in roles, then the rest by name. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-wide list-fill" controller={Controller}>
            {searchList({
                s: m.list,
                row: r,
                title: "Roles",
                noun: "roles",
                placeholder: "Search name, description…",
                newHref: "~/administration/roles/new",
                addText: "Add role",
                href: (id) => `~/administration/roles/${id}`,
                columns: "role-columns",
                cells: [
                    {
                        header: "Name",
                        cell: (
                            <cx>
                                <span class="record-title">
                                    <span text={r.name} />
                                    <span class="record-flag" visible={r.builtIn} text="Built-in" />
                                </span>
                            </cx>
                        ),
                    },
                    {
                        header: "Description",
                        cell: (
                            <cx>
                                <span
                                    class={{
                                        "record-meta": true,
                                        "record-blank": expr(r.description, (d) => !d),
                                    }}
                                    text={expr(r.description, (d) => d ?? "—")}
                                />
                            </cx>
                        ),
                    },
                    { header: "Users", numeric: true, cell: countCell(r.users, r.usersWord) },
                ],
            })}
        </div>
    </cx>
));
