import { createFunctionalComponent, expr, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../../components/formFields";
import { moreActions } from "../../../../components/moreActions";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const t = m.type;
const { editing, label, prose, text } = formFields(
    { draft: t.draft, errors: t.errors, viewing: t.viewing },
    "furniture-type",
);

/**
 * A furniture type's page: read-only as a row opens it, with the furniture of the type as a link to
 * the list filtered to it; editable at `…/edit` and while creating.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href={listReturn("~/furniture/types")} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Types" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={t.title} />
                    <div class="editor-heading-actions" visible={t.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(t.id, (id) => `~/furniture/types/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
                        {moreActions([{ text: "Delete", icon: "delete", onClick: "remove", danger: true }])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(t.error)}>
                    <span text={t.error} />
                </div>

                <ValidationGroup valid={t.valid} visited={t.visited} viewMode={t.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            {text("Name", "name", 100, { required: true, wide: true })}
                            {prose("Description", "description", 1000)}
                            <div class="editor-wide" visible={t.viewing}>
                                {label("Furniture of this type")}
                                <div class="editor-value">
                                    <Link
                                        class="editor-link"
                                        visible={hasValue(t.furnitureText)}
                                        href={t.furnitureHref}
                                        url={$app.url}
                                        text={t.furnitureText}
                                    />
                                    <span
                                        class="editor-empty"
                                        visible={expr(t.furnitureText, (x) => !x)}
                                        text="—"
                                    />
                                </div>
                            </div>
                        </div>
                        <div class="editor-actions" visible={editing}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(t.id, (id) =>
                                    id ? `~/furniture/types/${id}` : listReturn("~/furniture/types"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={t.saving} />
                        </div>
                    </section>
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
