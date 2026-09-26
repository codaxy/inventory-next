import { type Config, createFunctionalComponent, expr, falsy, hasValue, truthy } from "cx/ui";
import {
    Button,
    DateField,
    Icon,
    Link,
    LinkButton,
    LookupField,
    NumberField,
    ValidationGroup,
} from "cx/widgets";

import { dateValue, numberValue } from "../../../../bindings";
import { expiryClass } from "../../../../licensing";
import { moreActions } from "../../../../components/moreActions";
import { listReturn } from "../../../../listAddress";
import { externalLink } from "../../../../components/externalLink";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

/** The server's message goes in a line under its field; cx's hover tooltip would hide it. */
const noErrorText = false as unknown as Config;

const a = m.activation;
const v = a.view;
const creating = expr(a.id, (id) => !id);
const shown = hasValue(a.view);
const active = expr(a.view, (view) => !!view?.active);
const ended = expr(a.view, (view) => !!view && !view.active);
const noVolume = expr(a.draft.volumeId, (id) => !id);

/** A label over a value, for the read-only page; absent values show "—". */
const fact = (label: string, value: typeof v.software | typeof v.deactivated) => (
    <cx>
        <div>
            <div class="editor-label" text={label} />
            <div
                class={{ "editor-value": true, "editor-empty": expr(value, (x) => !x) }}
                text={expr(value, (x) => x || "—")}
            />
        </div>
    </cx>
);

/**
 * An activation. Creating is a form — software, then its volume, then who or what holds the seats.
 * An existing one is never edited, only deactivated or reactivated, or deleted: a read-only page with
 * those actions in its header and the licence it draws on beside it.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link
                    href={expr(a.origin, a.id, (o, id) =>
                        !id && o ? o.href : listReturn("~/licenses/activations"),
                    )}
                    url={$app.url}
                    class="editor-back"
                >
                    <Icon name="previous" class="size-4" />
                    <span text={expr(a.origin, a.id, (o, id) => (!id && o ? o.text : "Activations"))} />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={a.title} />
                    <div class="editor-heading-actions" visible={shown}>
                        <Button
                            mod="primary"
                            visible={active}
                            onClick="deactivate"
                            attrs={{ "aria-label": "Deactivate", title: "Deactivate" }}
                        >
                            <Icon name="deactivate" class="size-4" />
                            <span class="hidden sm:inline" text="Deactivate" />
                        </Button>
                        <Button
                            mod="primary"
                            visible={ended}
                            onClick="reactivate"
                            attrs={{ "aria-label": "Reactivate", title: "Reactivate" }}
                        >
                            <Icon name="reactivate" class="size-4" />
                            <span class="hidden sm:inline" text="Reactivate" />
                        </Button>
                        {moreActions([{ text: "Delete", icon: "delete", onClick: "remove", danger: true }])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(a.error)}>
                    <span text={a.error} />
                </div>

                {/* An existing activation: facts, not fields. */}
                <div visible={shown}>
                    <section class="editor-section">
                        <h2 class="editor-section-title" text="Activation" />
                        <div class="editor-grid">
                            {fact("Software or service", v.software)}
                            <div>
                                <div class="editor-label" text="Licence" />
                                <Link
                                    class="editor-value editor-link"
                                    href={expr(v.licenseId, (id) => `~/licenses/${id}`)}
                                    url={$app.url}
                                    text={v.license}
                                />
                            </div>
                            {fact("Volume", v.volume)}
                            <div>
                                <div class="editor-label" text={v.assigneeLabel} />
                                <div class="editor-value" text={v.assignee} />
                            </div>
                            {fact("Seats", v.seats)}
                            {fact("Activated", v.activated)}
                            {fact("Deactivated", v.deactivated)}
                        </div>
                    </section>
                    <section class="editor-section">
                        <h2 class="editor-section-title" text="The licence" />
                        <div class="editor-grid">
                            <div>
                                <div class="editor-label" text="Expiry" />
                                <div class="editor-value">
                                    <span
                                        visible={hasValue(v.expiryText)}
                                        class={expr(v.expiry, (x) => `status-tag ${expiryClass(x)}`)}
                                        text={v.expiryText}
                                    />
                                    <span class="editor-empty" visible={falsy(v.expiryText)} text="—" />
                                </div>
                            </div>
                            {fact("Vendor", v.vendor)}
                            {fact("Type and model", v.licenseType)}
                            {fact("Expiration model", v.expirationModel)}
                            {fact("Location", v.location)}
                            <div>
                                <div class="editor-label" text="URL" />
                                <div class="editor-url">
                                    <div
                                        class={{
                                            "editor-value": true,
                                            "editor-empty": expr(v.url, (x) => !x),
                                        }}
                                        text={expr(v.url, (x) => x || "—")}
                                    />
                                    {externalLink(v.url)}
                                </div>
                            </div>
                        </div>
                    </section>
                </div>

                {/* A new activation: the form. */}
                <ValidationGroup valid={a.valid} visited={a.visited}>
                    <section class="editor-section" visible={creating}>
                        <div class="editor-grid">
                            <div class="editor-wide">
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(a.fixed) }}
                                    id="licenses-activations-editor-software-or-service-label"
                                    text="Software or service"
                                />
                                <LookupField
                                    viewMode={a.fixed}
                                    id="licenses-activations-editor-software-or-service"
                                    value={a.draft.softwareId}
                                    text={a.draft.softwareText}
                                    options={a.software}
                                    required
                                    placeholder="Choose what is activated"
                                    inputAttrs={{ "aria-label": "Software or service" }}
                                />
                            </div>
                            <div class="editor-wide">
                                <div
                                    class={{ "editor-label": true, "editor-required": falsy(a.fixed) }}
                                    id="licenses-activations-editor-volume-label"
                                    text="Volume"
                                />
                                <LookupField
                                    viewMode={a.fixed}
                                    id="licenses-activations-editor-volume"
                                    value={a.draft.volumeId}
                                    text={a.draft.volumeText}
                                    options={a.volumes}
                                    required
                                    disabled={falsy(a.draft.softwareId)}
                                    placeholder="Choose the licence's volume"
                                    error={a.errors.volumeId}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Volume" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(a.errors.volumeId)}
                                    text={a.errors.volumeId}
                                />
                            </div>
                            <div class="editor-wide" visible={a.forPerson}>
                                <div
                                    class="editor-label editor-required"
                                    id="licenses-activations-editor-user-label"
                                    text="User"
                                />
                                <LookupField
                                    id="licenses-activations-editor-user"
                                    value={a.draft.personId}
                                    text={a.draft.personText}
                                    options={a.people}
                                    required={a.forPerson}
                                    disabled={noVolume}
                                    placeholder="Choose who the seats are for"
                                    error={a.errors.personId}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "User" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(a.errors.personId)}
                                    text={a.errors.personId}
                                />
                            </div>
                            <div class="editor-wide" visible={falsy(a.forPerson)}>
                                <div
                                    class="editor-label editor-required"
                                    id="licenses-activations-editor-device-label"
                                    text="Device"
                                />
                                <LookupField
                                    id="licenses-activations-editor-device"
                                    value={a.draft.deviceId}
                                    text={a.draft.deviceText}
                                    options={a.devices}
                                    required={falsy(a.forPerson)}
                                    placeholder="Choose the device"
                                    error={a.errors.deviceId}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Device" }}
                                />
                                <p
                                    class="field-message"
                                    visible={hasValue(a.errors.deviceId)}
                                    text={a.errors.deviceId}
                                />
                                <div class="editor-hint" text="Only devices of a type that holds licences." />
                            </div>
                            <div>
                                <div class="editor-label editor-required" text="Activated on" />
                                <DateField
                                    value={dateValue(a.draft.activationDate)}
                                    required
                                    error={a.errors.activationDate}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Activated on" }}
                                />
                            </div>
                            <div>
                                <div class="editor-label editor-required" text="Seats" />
                                <NumberField
                                    value={numberValue(a.draft.quantity)}
                                    required
                                    minValue={1}
                                    format="n;0"
                                    error={a.errors.quantity}
                                    errorTooltip={noErrorText}
                                    inputAttrs={{ "aria-label": "Seats", inputMode: "numeric" }}
                                />
                            </div>
                            <div class="editor-wide editor-warning" visible={truthy(a.overWarning)}>
                                <span text={a.overWarning} />
                            </div>
                        </div>
                        <div class="editor-actions">
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(a.origin, (o) => o?.href ?? listReturn("~/licenses/activations"))}
                            />
                            <Button mod="primary" text="Activate" onClick="save" disabled={a.saving} />
                        </div>
                    </section>
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
