import { type Config, createFunctionalComponent, expr, falsy, hasValue, truthy } from "cx/ui";
import {
    Button,
    Checkbox,
    DateField,
    Icon,
    Link,
    LinkButton,
    LookupField,
    NumberField,
    Repeater,
    TextArea,
    TextField,
    ValidationGroup,
} from "cx/widgets";

import { dateValue, numberValue } from "../../../bindings";
import { expiryClass } from "../../../licensing";
import { moreActions } from "../../../components/moreActions";
import { listReturn } from "../../../listAddress";
import $app from "../../../model";
import Controller from "./Controller";
import m from "./model";

/** The server's message goes in a line under its field; cx's hover tooltip would hide it. */
const noErrorText = false as unknown as Config;

const l = m.license;
const d = l.draft as any;
const o = l.options as any;
const errors = l.errors as any;
const editing = falsy(l.viewing);

/** The server's message about a field, in a line beneath it. */
const message = (field: string) => (
    <cx>
        <p class="field-message" visible={hasValue(errors[field])} text={errors[field]} />
    </cx>
);

/** A picker over one of the options' lists, bound as id and text. */
const pick = (
    label: string,
    key: string,
    list: string,
    opts: { required?: boolean; wide?: boolean } = {},
) => (
    <cx>
        <div class={{ "editor-wide": !!opts.wide }}>
            <div
                id={`license-${key}-label`}
                class={{ "editor-label": true, "editor-required": opts.required ? editing : false }}
                text={label}
            />
            <LookupField
                id={`license-${key}`}
                value={d[`${key}Id`]}
                text={d[`${key}Text`]}
                options={o[list]}
                required={!!opts.required}
                emptyText="—"
                placeholder="—"
                error={errors[`${key}Id`]}
                errorTooltip={noErrorText}
                inputAttrs={{ "aria-label": label }}
            />
            {message(`${key}Id`)}
        </div>
    </cx>
);

/** A text field, optionally wide. */
const text = (label: string, key: string, max: number, opts: { required?: boolean; wide?: boolean } = {}) => (
    <cx>
        <div class={{ "editor-wide": !!opts.wide }}>
            <div
                class={{ "editor-label": true, "editor-required": opts.required ? editing : false }}
                text={label}
            />
            <TextField
                value={d[key]}
                required={!!opts.required}
                maxLength={max}
                emptyText="—"
                error={errors[key]}
                errorTooltip={noErrorText}
                inputAttrs={{ "aria-label": label }}
            />
            {message(key)}
        </div>
    </cx>
);

/** A yes/no: a checkbox while editing, words while viewing. */
const flag = (label: string, key: string, yes: string, no: string) => (
    <cx>
        <div>
            <div class="editor-label" text={label} />
            <Checkbox visible={editing} value={d[key]} text={yes} />
            <div class="editor-value" visible={l.viewing} text={expr(d[key], (v) => (v ? yes : no))} />
        </div>
    </cx>
);

const noVolumes = expr(l.draft.volumes, (volumes) => !volumes?.length);
const newRow = expr(m.$volume.id, (id) => !id);
const keptRow = expr(m.$volume.id, (id) => !!id);

/**
 * A licence's page: read-only as a row opens it, with Delete, Duplicate and Edit in the header;
 * editable at `…/edit`, while creating and when duplicating (`new?from=…`). Basic and advanced
 * details, then the volumes: an existing one is kept or removed, a new one filled in.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href={listReturn("~/licenses")} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Licences" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title">
                        <span text={l.title} />
                        <span class="page-title-note" visible={hasValue(l.number)} text={l.number} />
                    </h1>
                    <div class="editor-heading-actions" visible={l.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(l.id, (id) => `~/licenses/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
                        {moreActions([
                            {
                                text: "Duplicate",
                                icon: "duplicate",
                                href: expr(l.id, (id) => `~/licenses/new?from=${id}`),
                            },
                            { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                        ])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(l.error)}>
                    <span text={l.error} />
                    <Button mod="hollow" text="Reload" onClick="reload" visible={l.stale} />
                </div>

                <ValidationGroup valid={l.valid} visited={l.visited} viewMode={l.viewing}>
                    <section class="editor-section">
                        <h2 class="editor-section-title" text="Basic information" />
                        <div class="editor-grid">
                            {text("Name", "name", 300, { required: true, wide: true })}
                            {pick("Vendor", "vendor", "vendors", { required: true })}
                            {text("Invoice number", "invoiceNumber", 200)}
                            <div>
                                <div
                                    class={{ "editor-label": true, "editor-required": editing }}
                                    text="Purchase value (BAM, without VAT)"
                                />
                                <NumberField
                                    value={numberValue(l.draft.purchaseValue)}
                                    required
                                    minValue={0}
                                    format="n;2"
                                    error={errors.purchaseValue}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Purchase value", inputMode: "decimal" }}
                                />
                                {message("purchaseValue")}
                            </div>
                            <div>
                                <div
                                    class={{ "editor-label": true, "editor-required": editing }}
                                    text="Purchase date"
                                />
                                <DateField
                                    value={dateValue(l.draft.purchaseDate)}
                                    required
                                    error={errors.purchaseDate}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Purchase date" }}
                                />
                                {message("purchaseDate")}
                            </div>
                            {pick("Assignee", "person", "people", { required: true })}
                            {flag("Record", "incomplete", "Marked incomplete", "Complete")}
                            {pick("Confidentiality", "confidentiality", "confidentialities")}
                            {pick("Integrity", "integrity", "integrities")}
                            {pick("Availability", "availability", "availabilities")}
                            <div>
                                <div class="editor-label" text="Importance" />
                                <div class="editor-value" text={l.importance} />
                                <div class="editor-hint" visible={editing} text="From the three above." />
                            </div>
                            <div class="editor-wide">
                                <div class="editor-label" text="Description" />
                                <TextArea
                                    value={l.draft.description}
                                    emptyText="—"
                                    maxLength={1000}
                                    rows={3}
                                    error={errors.description}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Description" }}
                                />
                                {message("description")}
                            </div>
                        </div>
                    </section>

                    <section class="editor-section">
                        <h2 class="editor-section-title" text="Subscription and details" />
                        <div class="editor-grid">
                            {pick("Licence type", "licenseType", "licenseTypes")}
                            {pick("Licence model", "licenseModel", "licenseModels")}
                            {pick("Expiration model", "expirationModel", "expirationModels")}
                            <div>
                                <div class="editor-label" text="Expires" />
                                <DateField
                                    visible={editing}
                                    value={dateValue(l.draft.expirationDate)}
                                    inputAttrs={{ "aria-label": "Expires" }}
                                />
                                <div class="editor-value" visible={l.viewing}>
                                    <span
                                        visible={hasValue(l.expiryText)}
                                        class={expr(l.expiry, (x) => `status-tag ${expiryClass(x)}`)}
                                        text={l.expiryText}
                                    />
                                    <span class="editor-empty" visible={falsy(l.expiryText)} text="—" />
                                </div>
                            </div>
                            <div>
                                <div class="editor-label" text="Subscription fee" />
                                <NumberField
                                    value={numberValue(l.draft.subscriptionFee)}
                                    minValue={0}
                                    format="n;2"
                                    emptyText="—"
                                    error={errors.subscriptionFee}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Subscription fee", inputMode: "decimal" }}
                                />
                                {message("subscriptionFee")}
                            </div>
                            {pick("Currency", "currency", "currencies")}
                            {pick("Period", "period", "periods")}
                            {flag(
                                "Renewal",
                                "autoRenew",
                                "Renews automatically",
                                "Does not renew automatically",
                            )}
                            {pick("Business entity", "businessEntity", "businessEntities")}
                            {pick("Location", "location", "locations")}
                            {text("Registration number", "registrationNumber", 200)}
                            {text("Key identifier", "keyIdentifier", 500)}
                            {text("Management console URL", "managementConsoleUrl", 500, { wide: true })}
                            {text("URL", "url", 500, { wide: true })}
                        </div>
                    </section>

                    <section class="editor-section">
                        <div class="editor-section-head">
                            <h2 class="editor-section-title" text="Volumes" />
                            <Button mod="hollow" visible={editing} onClick="addVolume">
                                <Icon name="created" class="size-4" />
                                <span text="Add volume" />
                            </Button>
                        </div>
                        <p
                            class="editor-empty"
                            visible={noVolumes}
                            text="No volumes: nothing of this licence can be activated."
                        />
                        <p class="field-message" visible={hasValue(errors.volumes)} text={errors.volumes} />

                        <div class="volume-list">
                            <Repeater records={l.draft.volumes} recordAlias={m.$volume} keyField="key">
                                {/* An existing volume: a line, kept or removed. */}
                                <div
                                    class={{
                                        "volume-row": true,
                                        "volume-kept": true,
                                        "volume-removed": truthy(m.$volume.removed),
                                    }}
                                    visible={keptRow}
                                >
                                    <div class="volume-summary">
                                        <div class="volume-name" text={m.$volume.software} />
                                        <div class="volume-detail" text={m.$volume.detail} />
                                    </div>
                                    <div
                                        class={expr(
                                            m.$volume.load,
                                            (load) => `volume-seats ${load ? `volume-seats-${load}` : ""}`,
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
                                    <div class="volume-links">
                                        <Link
                                            class="editor-link volume-activations"
                                            visible={hasValue(m.$volume.activationsHref)}
                                            href={m.$volume.activationsHref}
                                            url={$app.url}
                                            text={m.$volume.activationsText}
                                        />
                                        <Link
                                            class="editor-link volume-activations"
                                            visible={expr(
                                                l.viewing,
                                                m.$volume.activateHref,
                                                (v, href) => !!v && !!href,
                                            )}
                                            href={m.$volume.activateHref}
                                            url={$app.url}
                                            text="Activate"
                                        />
                                        <span
                                            class="editor-hint"
                                            visible={expr(m.$volume.held, l.viewing, (h, v) => !!h && !v)}
                                            text="In use — it cannot be removed."
                                        />
                                        <span
                                            class="editor-hint volume-removed-note"
                                            visible={truthy(m.$volume.removed)}
                                            text="Removed when you save."
                                        />
                                    </div>
                                    <Button
                                        mod="hollow"
                                        class="volume-undo"
                                        visible={truthy(m.$volume.removed)}
                                        onClick={(_e: unknown, { store, controller }: any) =>
                                            controller.keepVolume(store.get(m.$volume.key))
                                        }
                                        attrs={{ "aria-label": "Keep volume", title: "Keep volume" }}
                                    >
                                        <Icon name="reactivate" class="size-4" />
                                        <span class="hidden sm:inline" text="Undo" />
                                    </Button>
                                    <Button
                                        mod="hollow"
                                        class="volume-remove"
                                        visible={expr(
                                            m.$volume.held,
                                            l.viewing,
                                            m.$volume.removed,
                                            (h, v, r) => !h && !v && !r,
                                        )}
                                        onClick={(_e: unknown, { store, controller }: any) =>
                                            controller.removeVolume(store.get(m.$volume.key))
                                        }
                                        attrs={{ "aria-label": "Remove volume", title: "Remove volume" }}
                                    >
                                        <Icon name="delete" class="size-4" />
                                    </Button>
                                </div>

                                {/* A volume being added: filled in here. */}
                                <div class="volume-row volume-new" visible={newRow}>
                                    <span
                                        class="sr-only"
                                        id={expr(m.$volume.key, (k) => `volume-${k}-software-label`)}
                                        text="Software or service"
                                    />
                                    <span
                                        class="sr-only"
                                        id={expr(m.$volume.key, (k) => `volume-${k}-type-label`)}
                                        text="Assignment type"
                                    />
                                    <div class="volume-fields">
                                        <LookupField
                                            id={expr(m.$volume.key, (k) => `volume-${k}-software`)}
                                            value={m.$volume.softwareId}
                                            text={m.$volume.softwareText}
                                            options={o.software}
                                            required
                                            placeholder="Software or service"
                                            inputAttrs={{ "aria-label": "Software or service" }}
                                        />
                                        <LookupField
                                            id={expr(m.$volume.key, (k) => `volume-${k}-type`)}
                                            value={m.$volume.typeId}
                                            text={m.$volume.typeText}
                                            options={o.volumeTypes}
                                            required
                                            placeholder="Assignment type"
                                            inputAttrs={{ "aria-label": "Assignment type" }}
                                        />
                                        <NumberField
                                            value={numberValue(m.$volume.quantity)}
                                            required
                                            minValue={1}
                                            format="n;0"
                                            placeholder="Seats"
                                            inputAttrs={{ "aria-label": "Seats", inputMode: "numeric" }}
                                        />
                                        <TextField
                                            value={m.$volume.description}
                                            maxLength={1000}
                                            placeholder="Description"
                                            inputAttrs={{ "aria-label": "Volume description" }}
                                        />
                                    </div>
                                    <Button
                                        mod="hollow"
                                        class="volume-remove"
                                        onClick={(_e: unknown, { store, controller }: any) =>
                                            controller.removeVolume(store.get(m.$volume.key))
                                        }
                                        attrs={{ "aria-label": "Remove volume", title: "Remove volume" }}
                                    >
                                        <Icon name="delete" class="size-4" />
                                    </Button>
                                </div>
                            </Repeater>
                        </div>
                    </section>

                    {/* Below the last card, sticky across all three. */}
                    <div class="editor-actions editor-actions-bar" visible={editing}>
                        <LinkButton
                            mod="hollow"
                            text="Cancel"
                            href={expr(l.id, (id) => (id ? `~/licenses/${id}` : listReturn("~/licenses")))}
                        />
                        <Button mod="primary" text="Save" onClick="save" disabled={truthy(l.saving)} />
                    </div>
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
