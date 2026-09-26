import { type Config, createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import {
    Button,
    Checkbox,
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
import m, { devicesText } from "./model";

/** The server's message goes in a line under its field; cx's hover tooltip would hide it. */
const noErrorText = false as unknown as Config;

const t = m.type;

/**
 * A type's page: read-only as a row opens it, Delete and Edit in the header beside its name; editable
 * at `…/edit` and while creating, Cancel and Save in the card's footer. One form for both, switched by
 * the group's `viewMode`.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href="~/electronic-devices/types" url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Types" />
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
                            href={expr(t.id, (id) => `~/electronic-devices/types/${id}/edit`)}
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
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Name" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(t.errors.name)}
                                    text={t.errors.name}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Licences" />
                                <Checkbox
                                    visible={falsy(t.viewing)}
                                    value={t.draft.holdsLicences}
                                    text="Its devices can hold licences"
                                />
                                <div
                                    class="editor-value"
                                    visible={t.viewing}
                                    text={expr(t.draft.holdsLicences, (h) =>
                                        h ? "Its devices can hold licences" : "Its devices hold no licences",
                                    )}
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
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Description" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(t.errors.description)}
                                    text={t.errors.description}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Tags" />
                                <LookupField
                                    visible={falsy(t.viewing)}
                                    records={t.draft.tags}
                                    options={t.tagOptions}
                                    multiple
                                    placeholder="No tags"
                                    emptyText="No tags"
                                    error={t.errors.tagIds}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Tags" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(t.errors.tagIds)}
                                    text={t.errors.tagIds}
                                />
                                {/* cx shows a multiple lookup's view as one run of text; these read as the list they are. */}
                                <div class="editor-chips" visible={t.viewing}>
                                    <Repeater records={t.draft.tags} recordAlias={m.$tag}>
                                        <Link
                                            class="editor-chip"
                                            href={expr(m.$tag.id, (id) => `~/electronic-devices/tags/${id}`)}
                                            url={$app.url}
                                            text={m.$tag.text}
                                        />
                                    </Repeater>
                                    <span
                                        class="editor-empty"
                                        visible={expr(t.draft.tags, (tags) => !tags?.length)}
                                        text="No tags"
                                    />
                                </div>
                                <div
                                    class="editor-hint"
                                    visible={falsy(t.viewing)}
                                    text="A device of this type shows these tags."
                                />
                            </div>
                            <div class="editor-wide" visible={expr(t.viewing, t.loading, (v, l) => v && !l)}>
                                <div class="editor-label" text="Devices" />
                                <div
                                    class="editor-value"
                                    text={expr(t.deviceCount, (n) => devicesText(n ?? 0))}
                                />
                            </div>
                        </div>
                        {/* The form's commit, the card's footer; it sticks to the viewport's foot while a long form scrolls. */}
                        <div class="editor-actions" visible={falsy(t.viewing)}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(t.id, (id) =>
                                    id ? `~/electronic-devices/types/${id}` : "~/electronic-devices/types",
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
