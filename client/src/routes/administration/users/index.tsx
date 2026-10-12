import { createFunctionalComponent, expr } from "cx/ui";
import { Link, Repeater } from "cx/widgets";

import { copyButton, copyCell } from "../../../components/copyButton";
import { searchList } from "../../../components/searchList";
import $app from "../../../model";
import Controller from "./Controller";
import m from "./model";

const r = m.$row;
const c = m.$configured;

/** Who may sign in: the users, then the addresses configuration lets in that are not users yet. */
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

            <section class="configured-users" visible={expr(m.configured, (list) => (list?.length ?? 0) > 0)}>
                <h2 class="configured-users-title" text="Allowed by configuration, not added yet" />
                <p
                    class="configured-users-note"
                    text="These addresses sign in as Admin through Auth:AllowedUsers. Add each as a user before the setting is removed."
                />
                <Repeater records={m.configured} recordAlias={c} keyField="email">
                    <div class="configured-user">
                        <span class="configured-user-email" text={c.email} />
                        <Link class="editor-link" href={c.addHref} url={$app.url} text="Add" />
                    </div>
                </Repeater>
            </section>
        </div>
    </cx>
));
