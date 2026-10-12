import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Checkbox, PureContainer, Repeater } from "cx/widgets";

import { mayEdit, permissions } from "../../../../access";
import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import $app from "../../../../model";
import Controller from "./Controller";
import m, { type RoleFlags, summary } from "./model";

const r = m.record;
const g = m.$grant;
const { text, prose } = formFields({ draft: r.draft, errors: r.errors, viewing: r.viewing }, "role");

/** A built-in role keeps its name: the seeder and Reset to default find it by it. */
const nameFixed = expr(r.role, (role) => !!role?.isBuiltIn);
const nameEditable = expr(r.role, (role) => !role?.isBuiltIn);

/** A role action, shown in view to whoever may change this role. */
const action = (when: (role: RoleFlags) => boolean) =>
    expr(
        r.role,
        r.viewing,
        $app.url,
        permissions,
        (role, viewing, url, held) =>
            !!role && !!viewing && role.changeable && when(role) && mayEdit(url, held),
    );

const tick = (slot: "first" | "second") => (
    <cx>
        <Checkbox
            visible={slot === "first" ? true : hasValue(g.second)}
            value={g[slot].on}
            text={g[slot].label}
            disabled={expr(
                g,
                r.viewing,
                (grant, viewing) =>
                    !!viewing ||
                    !grant?.[slot]?.grantable ||
                    // A view stays while its edit is ticked.
                    (slot === "first" && !!grant?.second?.on && grant.second.implies === grant.first.key),
            )}
        />
    </cx>
);

/**
 * A role's page: its name and description, then what it grants, ticked by section. System Admin,
 * the reader's own role and one granting more than theirs are shown, not offered for change.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/administration/roles",
                back: "Roles",
                deletable: false,
                changeable: r.role.changeable,
                more: [
                    {
                        text: "Reset to default",
                        icon: "reactivate",
                        onClick: "reset",
                        visible: action((role) => role.isBuiltIn),
                    },
                    {
                        text: "Delete",
                        icon: "delete",
                        onClick: "remove",
                        danger: true,
                        visible: action((role) => !role.isBuiltIn),
                    },
                ],
                fields: (
                    <cx>
                        <p class="editor-hint editor-wide" visible={hasValue(r.role.why)} text={r.role.why} />
                        <PureContainer visible={nameEditable}>
                            {text("Name", "name", 100, { required: true, wide: true })}
                        </PureContainer>
                        <div class="editor-wide" visible={nameFixed}>
                            <div class="editor-label" text="Name" />
                            <div class="editor-value" text={expr(r.draft.name, (n) => n ?? "")} />
                        </div>
                        {prose("Description", "description", 500)}
                    </cx>
                ),
                cards: (actions) => (
                    <cx>
                        <section class="editor-section">
                            <h2 class="editor-section-title" text="Permissions" />
                            <p
                                class="editor-hint"
                                visible={falsy(r.viewing)}
                                text="Editing includes viewing. You can grant only what your own role grants."
                            />
                            <div class="role-grants">
                                <Repeater records={r.draft.grants} recordAlias={g} keyField="section">
                                    <div class="role-grant">
                                        <span class="role-grant-section" text={g.section} />
                                        <span
                                            class={{
                                                "role-grant-summary": true,
                                                "record-blank": expr(g, (x) => summary(x) === "—"),
                                            }}
                                            visible={r.viewing}
                                            text={expr(g, summary)}
                                        />
                                        <span class="role-grant-ticks" visible={falsy(r.viewing)}>
                                            {tick("first")}
                                            {tick("second")}
                                        </span>
                                    </div>
                                </Repeater>
                            </div>
                            <p
                                class="field-message"
                                visible={hasValue(r.errors.permissions)}
                                text={r.errors.permissions}
                            />
                            {actions}
                        </section>
                    </cx>
                ),
            })}
        </div>
    </cx>
));
