import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../../components/formFields";
import { holdingSections } from "../../../../components/holdings";
import { historyAction, moreActions } from "../../../../components/moreActions";
import { outlined } from "../../../../components/recordPage";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const c = m.client;
const outline = outlined(c.viewing, c.loading);
const { editing, text } = formFields({ draft: c.draft, errors: c.errors, viewing: c.viewing }, "client");
const noProjects = expr(c.loaded, c.projectCount, c.viewing, (loaded, n, v) => !!loaded && n === 0 && !!v);

/** A client's page: its name, then — read-only — its projects, each leading to its page. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class={{ "page-top": true, "record-loading": outline }}>
                <div class="page-header">
                    <Link href={listReturn("~/company/clients")} url={$app.url} class="editor-back">
                        <Icon name="previous" class="size-4" />
                        <span text="Clients" />
                    </Link>
                    <div class="editor-heading">
                        <h1 class="page-title" text={c.title} />
                        <div class="editor-heading-actions" visible={c.viewing}>
                            <LinkButton
                                mod="primary"
                                href={expr(c.id, (id) => `~/company/clients/${id}/edit`)}
                                attrs={{ "aria-label": "Edit", title: "Edit" }}
                            >
                                <Icon name="edit" class="size-4" />
                                <span class="hidden sm:inline" text="Edit" />
                            </LinkButton>
                            {moreActions([
                                historyAction(c.id),
                                { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                            ])}
                        </div>
                    </div>
                </div>
            </div>

            <div class={{ editor: true, "record-loading": outline }}>
                <div class="editor-alert" visible={hasValue(c.error)}>
                    <span text={c.error} />
                </div>

                <ValidationGroup valid={c.valid} visited={c.visited} viewMode={c.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            {text("Name", "name", 200, { required: true, wide: true })}
                        </div>
                        <div class="editor-actions" visible={editing}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(c.id, (id) =>
                                    id ? `~/company/clients/${id}` : listReturn("~/company/clients"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={truthy(c.saving)} />
                        </div>
                    </section>
                </ValidationGroup>

                {holdingSections(c.sections, c.viewing)}

                <div class="holding-footnote" visible={noProjects}>
                    <p text="It has no projects." />
                </div>
            </div>
        </div>
    </cx>
));
