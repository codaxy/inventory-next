import { createFunctionalComponent, expr } from "cx/ui";
import { Link, Repeater } from "cx/widgets";

import { formFields } from "../../../components/formFields";
import { recordPage } from "../../../components/recordPage";
import $app from "../../../model";
import Controller from "./Controller";
import m from "./model";

const d = m.device;
const { basicFields, date, label, pick, text } = formFields(
    { draft: d.draft, options: d.options, errors: d.errors, viewing: d.viewing, importance: d.importance },
    "device",
);

/** The device's own card: its type and the type's tags, who made it, its model, serial and warranty. */
const device = (
    <cx>
        <section class="editor-section">
            <h2 class="editor-section-title" text="Device" />
            <div class="editor-grid">
                {pick("Type", "type", "types", { href: (id) => `~/electronic-devices/types/${id}` })}
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
                        <span class="editor-empty" visible={expr(d.tags, (t) => !t?.length)} text="—" />
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
                {pick("Location", "location", "locations", { href: (id) => `~/company/locations/${id}` })}
                {text("URL", "url", 500, { wide: true, url: true })}
            </div>
        </section>
    </cx>
);

/**
 * A device's page: read-only as a row opens it, with Edit, and Duplicate and Delete behind the ⋮;
 * editable at `…/edit`, while creating and when duplicating. A new device can be saved and the next
 * one opened like it, for a batch bought together. Then what is attached: contracts, seats,
 * information.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r: d,
                path: "~/electronic-devices",
                back: "Electronic devices",
                title: "Basic information",
                fields: basicFields(),
                cards: device,
                duplicate: true,
                another: true,
            })}
        </div>
    </cx>
));
