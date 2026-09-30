import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../components/formFields";
import { inventoryNumber } from "../../../components/inventoryNumber";
import { historyAction, moreActions } from "../../../components/moreActions";
import { outlined } from "../../../components/recordPage";
import { listReturn } from "../../../listAddress";
import $app from "../../../model";
import Controller from "./Controller";
import m from "./model";

const f = m.furniture;
const outline = outlined(f.viewing, f.loading);
const { basicInformation, editing, pick, text } = formFields(
    { draft: f.draft, options: f.options, errors: f.errors, viewing: f.viewing, importance: f.importance },
    "furniture",
);

/**
 * A piece of furniture's page: read-only as a row opens it, with Edit, and Duplicate and Delete behind
 * the ⋮; editable at `…/edit`, while creating and when duplicating (`new?from=…`). The asset's basic
 * information, then the furniture's own details.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class={{ "page-top": true, "record-loading": outline }}>
                <div class="page-header">
                    <Link href={listReturn("~/furniture")} url={$app.url} class="editor-back">
                        <Icon name="previous" class="size-4" />
                        <span text="Furniture" />
                    </Link>
                    <div class="editor-heading">
                        <h1 class="page-title">
                            <span text={f.title} />
                            {inventoryNumber(f.number)}
                        </h1>
                        <div class="editor-heading-actions" visible={f.viewing}>
                            <LinkButton
                                mod="primary"
                                href={expr(f.id, (id) => `~/furniture/${id}/edit`)}
                                attrs={{ "aria-label": "Edit", title: "Edit" }}
                            >
                                <Icon name="edit" class="size-4" />
                                <span class="hidden sm:inline" text="Edit" />
                            </LinkButton>
                            {moreActions([
                                historyAction(f.id),
                                {
                                    text: "Duplicate",
                                    icon: "duplicate",
                                    href: expr(f.id, (id) => `~/furniture/new?from=${id}`),
                                },
                                { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                            ])}
                        </div>
                    </div>
                </div>
            </div>

            <div class={{ editor: true, "record-loading": outline }}>
                <div class="editor-alert" visible={hasValue(f.error)}>
                    <span text={f.error} />
                    <Button mod="hollow" text="Reload" onClick="reload" visible={f.stale} />
                </div>

                <ValidationGroup valid={f.valid} visited={f.visited} viewMode={f.viewing}>
                    {basicInformation()}

                    <section class="editor-section">
                        <h2 class="editor-section-title" text="Details" />
                        <div class="editor-grid">
                            {pick("Type", "type", "types", { href: (id) => `~/furniture/types/${id}` })}
                            {text("Model", "model", 300)}
                            {pick("Business entity", "businessEntity", "businessEntities")}
                            {pick("Location", "location", "locations", {
                                href: (id) => `~/company/locations/${id}`,
                            })}
                            {text("URL", "url", 500, { wide: true, url: true })}
                        </div>
                        <div class="editor-actions" visible={editing}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(f.id, (id) =>
                                    id ? `~/furniture/${id}` : listReturn("~/furniture"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={truthy(f.saving)} />
                        </div>
                    </section>
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
