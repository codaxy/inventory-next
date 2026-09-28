import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../../components/formFields";
import { holdingSections } from "../../../../components/holdings";
import { historyAction, moreActions } from "../../../../components/moreActions";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const p = m.person;
const { editing, text } = formFields({ draft: p.draft, errors: p.errors, viewing: p.viewing }, "person");

/**
 * A person's page: their name and email, then — read-only — everything attached to them, a section
 * per kind with its count and its first rows, and "See all" to the list filtered to them.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-top">
                <div class="page-header">
                    <Link href={listReturn("~/company/people")} url={$app.url} class="editor-back">
                        <Icon name="previous" class="size-4" />
                        <span text="People" />
                    </Link>
                    <div class="editor-heading">
                        <h1 class="page-title" text={p.title} />
                        <div class="editor-heading-actions" visible={p.viewing}>
                            <LinkButton
                                mod="primary"
                                href={expr(p.id, (id) => `~/company/people/${id}/edit`)}
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
                                { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                            ])}
                        </div>
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(p.error)}>
                    <span text={p.error} />
                </div>

                <ValidationGroup valid={p.valid} visited={p.visited} viewMode={p.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            {text("Name", "name", 200, { required: true })}
                            {text("Email", "email", 200, { required: true })}
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
