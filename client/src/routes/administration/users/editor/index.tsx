import { createFunctionalComponent, expr } from "cx/ui";
import { PureContainer } from "cx/widgets";

import { mayEdit, permissions } from "../../../../access";
import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { pick, text } = formFields(
    { draft: r.draft, options: r.options, errors: r.errors, viewing: r.viewing },
    "user",
);

const adding = expr(r.id, (id) => !id);
const added = expr(r.id, (id) => !!id);

/** The role on the reader's own user while editing: shown, not offered — nobody changes their own. */
const ownRole = expr(r.user, r.viewing, (u, viewing) => !!u?.isSelf && !viewing);
const roleOffered = expr(r.user, r.viewing, (u, viewing) => !u?.isSelf || !!viewing);

const transition = (active: boolean) =>
    expr(
        r.user,
        r.viewing,
        $app.url,
        permissions,
        (u, viewing, url, held) =>
            !!u && !!viewing && u.isActive === active && !u.isSelf && u.canManage && mayEdit(url, held),
    );

/**
 * A user's page: who they are and the role they hold. The email is fixed once added, the role cannot
 * be one's own to change, and a user is deactivated rather than deleted.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/administration/users",
                back: "Users",
                deletable: false,
                changeable: r.user.canManage,
                more: [
                    {
                        text: "Deactivate",
                        icon: "deactivate",
                        onClick: "deactivate",
                        visible: transition(true),
                    },
                    {
                        text: "Reactivate",
                        icon: "reactivate",
                        onClick: "reactivate",
                        visible: transition(false),
                    },
                ],
                fields: (
                    <cx>
                        <PureContainer visible={adding}>
                            {text("Email", "email", 250, { required: true, wide: true })}
                        </PureContainer>
                        <div class="editor-wide" visible={added}>
                            <div class="editor-label" text="Email" />
                            <div class="editor-value" text={expr(r.draft.email, (e) => e ?? "")} />
                        </div>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        <PureContainer visible={roleOffered}>
                            {pick("Role", "role", "roles", {
                                required: true,
                                wide: true,
                                view: (
                                    <cx>
                                        <span text={r.draft.roleText} />
                                    </cx>
                                ),
                            })}
                        </PureContainer>
                        <div class="editor-wide" visible={ownRole}>
                            <div class="editor-label" text="Role" />
                            <div class="editor-value" text={r.draft.roleText} />
                            <p class="editor-hint" text="Nobody changes their own role." />
                        </div>
                        <div
                            class="editor-wide"
                            visible={expr(r.user, r.viewing, (u, v) => !!v && u?.isActive === false)}
                        >
                            <div class="editor-label" text="Status" />
                            <div class="editor-value">
                                <span class="record-flag record-flag-ended ml-0" text="Deactivated" />
                            </div>
                        </div>
                    </cx>
                ),
            })}
        </div>
    </cx>
));
