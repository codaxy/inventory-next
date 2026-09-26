import { createFunctionalComponent, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LookupField, TextArea, TextField, ValidationGroup } from "cx/widgets";

import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const t = m.tag;

/**
 * A tag's editor, a page of its own: its words and the types that carry it. Save and Cancel stay
 * in reach at the bottom of the screen; Delete appears once the tag exists.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body" controller={Controller}>
            <div class="page-header">
                <Link href="~/electronic-devices/tags" url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Tags" />
                </Link>
                <h1 class="page-title" text={t.title} />
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(t.error)}>
                    <span text={t.error} />
                </div>

                <ValidationGroup valid={t.valid} visited={t.visited}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            <div class="editor-wide">
                                <div class="editor-label editor-required" text="Name" />
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
                                    maxLength={1000}
                                    rows={3}
                                    error={t.errors.description}
                                    inputAttrs={{ "aria-label": "Description" }}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Types with this tag" />
                                <LookupField
                                    records={t.draft.types}
                                    options={t.typeOptions}
                                    multiple
                                    placeholder="No types"
                                    error={t.errors.typeIds}
                                    inputAttrs={{ "aria-label": "Types with this tag" }}
                                />
                                <div
                                    class="editor-hint"
                                    visible={isNonEmpty(t.draft.types)}
                                    text="A device of one of these types shows the tag."
                                />
                            </div>
                        </div>
                    </section>
                </ValidationGroup>
            </div>

            <div class="editor-actions">
                <div class="editor-actions-row">
                    <Button
                        mod="hollow"
                        class="editor-delete"
                        text="Delete"
                        onClick="remove"
                        visible={hasValue(t.id)}
                    />
                    <div class="editor-actions-end">
                        <Button mod="hollow" text="Cancel" onClick="cancel" />
                        <Button mod="primary" text="Save" onClick="save" disabled={t.saving} />
                    </div>
                </div>
            </div>
        </div>
    </cx>
));
