import { type Config, createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import {
    Button,
    Icon,
    Link,
    LinkButton,
    LookupField,
    Repeater,
    TextField,
    ValidationGroup,
} from "cx/widgets";

import { moreActions } from "../../../../components/moreActions";
import { listReturn } from "../../../../listAddress";
import { externalLink } from "../../../../components/externalLink";
import $app from "../../../../model";
import Controller from "./Controller";
import m, { volumesText } from "./model";

/** The server's message goes in a line under its field; cx's hover tooltip would hide it. */
const noErrorText = false as unknown as Config;

const e = m.entry;

/**
 * A software or service's page: read-only as a row opens it, Delete and Edit in the header; editable
 * at `…/edit` and while creating, Cancel and Save in the card's footer.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href={listReturn("~/licenses/software-services")} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Software & services" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={e.title} />
                    <div class="editor-heading-actions" visible={e.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(e.id, (id) => `~/licenses/software-services/${id}/edit`)}
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
                <div class="editor-alert" visible={hasValue(e.error)}>
                    <span text={e.error} />
                </div>

                <ValidationGroup valid={e.valid} visited={e.visited} viewMode={e.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            <div class="editor-wide">
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(e.viewing) }}
                                    text="Name"
                                />
                                <TextField
                                    value={e.draft.name}
                                    required
                                    maxLength={200}
                                    error={e.errors.name}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Name" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(e.errors.name)}
                                    text={e.errors.name}
                                />
                            </div>
                            <div>
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(e.viewing) }}
                                    id="licenses-software-services-editor-category-label"
                                    text="Category"
                                />
                                <LookupField
                                    id="licenses-software-services-editor-category"
                                    value={e.draft.categoryId}
                                    text={e.draft.categoryText}
                                    options={e.categories}
                                    required
                                    placeholder="Choose a category"
                                    error={e.errors.categoryId}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Category" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(e.errors.categoryId)}
                                    text={e.errors.categoryId}
                                />
                            </div>
                            <div>
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(e.viewing) }}
                                    id="licenses-software-services-editor-manufacturer-label"
                                    text="Manufacturer"
                                />
                                <LookupField
                                    id="licenses-software-services-editor-manufacturer"
                                    value={e.draft.manufacturerId}
                                    text={e.draft.manufacturerText}
                                    options={e.manufacturers}
                                    required
                                    placeholder="Choose a manufacturer"
                                    error={e.errors.manufacturerId}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Manufacturer" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(e.errors.manufacturerId)}
                                    text={e.errors.manufacturerId}
                                />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="URL" />
                                <div class="editor-url">
                                    <TextField
                                        value={e.draft.url}
                                        maxLength={500}
                                        emptyText="—"
                                        error={e.errors.url}
                                        errorTooltip={noErrorText}
                                        inputAttrs={{ "aria-label": "URL", inputMode: "url" }}
                                    />
                                    {externalLink(e.draft.url)}
                                </div>
                                <p
                                    class="field-message"
                                    visible={hasValue(e.errors.url)}
                                    text={e.errors.url}
                                />
                            </div>
                            <div class="editor-wide" visible={expr(e.viewing, e.loading, (v, l) => v && !l)}>
                                <div class="editor-label" text="Licence volumes" />
                                <div
                                    class="editor-value"
                                    visible={expr(e.volumes, (v) => !v?.length)}
                                    text={expr(e.volumeCount, (n) => volumesText(n ?? 0))}
                                />
                                {/* Each a link to its licence, read in the same parts as on the licence's page. */}
                                <div class="volume-list">
                                    <Repeater records={e.volumes} recordAlias={m.$volume} keyField="id">
                                        <Link
                                            class="volume-row volume-kept volume-link"
                                            href={m.$volume.href}
                                            url={$app.url}
                                        >
                                            <div class="volume-summary">
                                                <div class="volume-name" text={m.$volume.license} />
                                                <div class="volume-detail" text={m.$volume.detail} />
                                            </div>
                                            <div
                                                class={expr(
                                                    m.$volume.load,
                                                    (load) =>
                                                        `volume-seats ${load ? `volume-seats-${load}` : ""}`,
                                                )}
                                            >
                                                <div class="volume-seats-figure">
                                                    <span text={m.$volume.seats} />
                                                    <span class="volume-seats-label" text="in use" />
                                                </div>
                                                <div class="volume-meter" aria-hidden="true">
                                                    <div
                                                        class="volume-meter-fill"
                                                        style={expr(
                                                            m.$volume.fill,
                                                            (f) => `width: ${Math.min(f ?? 0, 100)}%`,
                                                        )}
                                                    />
                                                </div>
                                            </div>
                                        </Link>
                                    </Repeater>
                                </div>
                            </div>
                        </div>
                        <div class="editor-actions" visible={falsy(e.viewing)}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(e.id, (id) =>
                                    id
                                        ? `~/licenses/software-services/${id}`
                                        : listReturn("~/licenses/software-services"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={e.saving} />
                        </div>
                    </section>
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
