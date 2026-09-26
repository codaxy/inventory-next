import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import {
    Button,
    Icon,
    Link,
    LinkButton,
    LookupField,
    Repeater,
    TextArea,
    TextField,
    ValidationGroup,
} from "cx/widgets";

import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const t = m.tag;

/**
 * A tag's page: read-only as a row opens it, Delete and Edit in the header beside its name; editable
 * at `…/edit` and while creating, Cancel and Save pinned where the form ends. One form for both,
 * switched by the group's `viewMode`.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href="~/electronic-devices/tags" url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Tags" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={t.title} />

                    {/* The record's own actions, beside its name; a phone shows the icons, named for readers. */}
                    <div class="editor-heading-actions" visible={t.viewing}>
                        <Button
                            mod="hollow"
                            class="editor-delete"
                            onClick="remove"
                            attrs={{ "aria-label": "Delete", title: "Delete" }}
                        >
                            <Icon name="delete" class="size-4" />
                            <span class="hidden sm:inline" text="Delete" />
                        </Button>
                        <LinkButton
                            mod="primary"
                            href={expr(t.id, (id) => `~/electronic-devices/tags/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
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
                            <div class="editor-wide">
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(t.viewing) }}
                                    text="Name"
                                />
                                <TextField
                                    value={t.draft.name}
                                    required
                                    maxLength={100}
                                    error={t.errors.name}
                                    inputAttrs={{ "aria-label": "Name" }}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Description" />
                                <TextArea
                                    value={t.draft.description}
                                    emptyText="—"
                                    maxLength={1000}
                                    rows={3}
                                    error={t.errors.description}
                                    inputAttrs={{ "aria-label": "Description" }}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Types with this tag" />
                                <LookupField
                                    visible={falsy(t.viewing)}
                                    records={t.draft.types}
                                    options={t.typeOptions}
                                    multiple
                                    placeholder="No types"
                                    emptyText="No types"
                                    error={t.errors.typeIds}
                                    inputAttrs={{ "aria-label": "Types with this tag" }}
                                />
                                {/* cx shows a multiple lookup's view as one run of text; these read as the list they are. */}
                                <div class="editor-chips" visible={t.viewing}>
                                    <Repeater records={t.draft.types} recordAlias={m.$type}>
                                        <Link
                                            class="editor-chip"
                                            href={expr(
                                                m.$type.id,
                                                (id) => `~/electronic-devices/types/${id}`,
                                            )}
                                            url={$app.url}
                                            text={m.$type.text}
                                        />
                                    </Repeater>
                                    <span
                                        class="editor-empty"
                                        visible={expr(t.draft.types, (types) => !types?.length)}
                                        text="No types"
                                    />
                                </div>
                                <div
                                    class="editor-hint"
                                    visible={expr(
                                        t.viewing,
                                        t.draft.types,
                                        (v, types) => !v && types?.length > 0,
                                    )}
                                    text="A device of one of these types shows the tag."
                                />
                            </div>
                        </div>
                    </section>
                </ValidationGroup>
            </div>

            {/* The form's commit, where the form ends; pinned so a long form keeps it in reach. */}
            <div class="editor-actions" visible={falsy(t.viewing)}>
                <div class="editor-actions-row">
                    <div class="editor-actions-end">
                        <LinkButton
                            mod="hollow"
                            text="Cancel"
                            href={expr(t.id, (id) =>
                                id ? `~/electronic-devices/tags/${id}` : "~/electronic-devices/tags",
                            )}
                        />
                        <Button mod="primary" text="Save" onClick="save" disabled={t.saving} />
                    </div>
                </div>
            </div>
        </div>
    </cx>
));
