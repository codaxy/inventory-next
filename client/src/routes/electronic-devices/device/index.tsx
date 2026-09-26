import { createFunctionalComponent, expr, hasValue } from "cx/ui";
import { Icon, Link, Repeater, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../components/formFields";
import { holdingSections } from "../../../components/holdings";
import { listReturn } from "../../../listAddress";
import $app from "../../../model";
import Controller from "./Controller";
import m from "./model";

const d = m.device;
const { basicInformation, date, label, pick, text } = formFields(
    { draft: d.draft, errors: d.errors, viewing: d.viewing, importance: d.importance },
    "device",
);

/**
 * A device's page, read-only: the asset's basic information, the device's own details — its type's
 * tags as chips, each a link — and what is attached to it: maintenance contracts, seats, information.
 * Editing arrives with its own phase; until then nothing is offered that cannot be done.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href={listReturn("~/electronic-devices")} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="Electronic devices" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title">
                        <span text={d.title} />
                        <span class="page-title-note" visible={hasValue(d.number)} text={d.number} />
                    </h1>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(d.error)}>
                    <span text={d.error} />
                </div>

                <ValidationGroup viewMode={d.viewing}>
                    {basicInformation()}

                    <section class="editor-section">
                        <h2 class="editor-section-title" text="Device" />
                        <div class="editor-grid">
                            {pick("Type", "type", "types", {
                                href: (id) => `~/electronic-devices/types/${id}`,
                            })}
                            <div>
                                {label("Tags")}
                                <div class="editor-chips">
                                    <Repeater records={d.tags} recordAlias={m.$tag}>
                                        <Link
                                            class="editor-chip"
                                            href={expr(m.$tag.id, (id) => `~/electronic-devices/tags/${id}`)}
                                            url={$app.url}
                                            text={m.$tag.text}
                                        />
                                    </Repeater>
                                    <span
                                        class="editor-empty"
                                        visible={expr(d.tags, (t) => !t?.length)}
                                        text="—"
                                    />
                                </div>
                            </div>
                            {pick("Manufacturer", "manufacturer", "manufacturers", {
                                href: (id) => `~/company/manufacturers/${id}`,
                            })}
                            {date("Manufactured", "manufacturingDate")}
                            {text("Model name", "modelName", 200)}
                            {text("Model code", "modelCode", 200)}
                            {text("Serial number", "serialNumber", 200)}
                            {text("Warranty number", "warrantyNumber", 200)}
                            {date("Warranty expires", "warrantyExpirationDate")}
                            {pick("Business entity", "businessEntity", "businessEntities")}
                            {pick("Location", "location", "locations", {
                                href: (id) => `~/company/locations/${id}`,
                            })}
                            {text("URL", "url", 500, { wide: true, url: true })}
                        </div>
                    </section>
                </ValidationGroup>

                {holdingSections(d.sections, d.viewing)}

                <div class="holding-footnote" visible={hasValue(d.none)}>
                    <p text={d.none} />
                </div>
            </div>
        </div>
    </cx>
));
