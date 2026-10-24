import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, Switch, ValidationGroup } from "cx/widgets";

import { recordTitle } from "../../../../components/documentTitle";
import { formFields } from "../../../../components/formFields";
import { holdingSections } from "../../../../components/holdings";
import { historyAction, deleteAction, moreActions } from "../../../../components/moreActions";
import { outlined } from "../../../../components/recordPage";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";
import { can, canEdit, permissions } from "../../../../access";

const p = m.person;
const outline = outlined(p.viewing, p.loading);
const { editing, text } = formFields({ draft: p.draft, errors: p.errors, viewing: p.viewing }, "person");

const mayOpenSettings = can("settings.manage");
const mayNotOpenSettings = expr(permissions, (held) => !held?.includes("settings.manage"));

/**
 * Whether the person may sign in to see their own inventory: a decision about them, shown and changed
 * as it is stored whatever the global switch says — so someone can be left out ahead of time. A plain
 * "Yes" or "No"; under a "Yes", while employees cannot sign in at all, a note says so — a fact about
 * now, never a promise it will apply.
 */
function signInField() {
    return (
        <cx>
            <div class="editor-wide">
                <div class="editor-label" text="Can sign in" />
                <div
                    class="editor-value"
                    visible={p.viewing}
                    text={expr(p.draft.canSignIn, (can) => (can === false ? "No" : "Yes"))}
                />
                <div visible={editing}>
                    <Switch value={p.draft.canSignIn} text="To see their own inventory" />
                </div>
                <p
                    class="editor-hint"
                    visible={expr(p.signInOpen, p.draft.canSignIn, (open, can) => !open && can !== false)}
                >
                    <span text="Note: employee sign-in is turned off globally, under " />
                    {/* A link for whoever may open the settings; for anyone else it would lead nowhere. */}
                    <Link
                        href="~/administration/settings"
                        url={$app.url}
                        visible={mayOpenSettings}
                        text="Administration → Settings → Sign-in"
                    />
                    <span visible={mayNotOpenSettings} text="Administration → Settings → Sign-in" />
                    <span text="." />
                </p>
            </div>
        </cx>
    );
}

/**
 * A person's page: their name and email, then — read-only — everything attached to them, a section
 * per kind with its count and its first rows, and "See all" to the list filtered to them.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class={{ "page-top": true, "record-loading": outline }}>
                <div class="page-header">
                    <Link href={listReturn("~/company/people")} url={$app.url} class="editor-back">
                        <Icon name="previous" class="size-4" />
                        <span text="People" />
                    </Link>
                    <div class="editor-heading">
                        <h1 class="page-title" text={p.title} />
                        {recordTitle(p.title)}
                        <div class="editor-heading-actions" visible={p.viewing}>
                            <LinkButton
                                mod="primary"
                                href={expr(p.id, (id) => `~/company/people/${id}/edit`)}
                                visible={canEdit}
                                attrs={{ "aria-label": "Edit", title: "Edit" }}
                            >
                                <Icon name="edit" class="size-4" />
                                <span class="hidden sm:inline" text="Edit" />
                            </LinkButton>
                            {moreActions([
                                historyAction(p.id),
                                {
                                    text: "Handover sheet",
                                    icon: "print",
                                    href: expr(p.id, (id) => `~/company/people/${id}/handover`),
                                },
                                deleteAction,
                            ])}
                        </div>
                    </div>
                </div>
            </div>

            <div class={{ editor: true, "record-loading": outline }}>
                <div class="editor-alert" visible={hasValue(p.error)}>
                    <span text={p.error} />
                </div>

                <ValidationGroup valid={p.valid} visited={p.visited} viewMode={p.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            {text("Name", "name", 200, { required: true })}
                            {text("Email", "email", 200, { required: true })}
                            {signInField()}
                        </div>
                        <div class="editor-actions" visible={editing}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(p.id, (id) =>
                                    id ? `~/company/people/${id}` : listReturn("~/company/people"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={truthy(p.saving)} />
                        </div>
                    </section>
                </ValidationGroup>

                {holdingSections(p.sections, p.viewing)}

                <div class="holding-footnote" visible={truthy(p.holdingsLoaded)}>
                    <p visible={hasValue(p.none)} text={p.none} />
                    <p text="Virtual machines, cloud subscriptions and software have no owner, so none are listed here." />
                </div>
            </div>
        </div>
    </cx>
));
