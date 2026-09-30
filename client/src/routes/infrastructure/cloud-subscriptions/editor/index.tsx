import { createFunctionalComponent, expr } from "cx/ui";
import { Link } from "cx/widgets";

import $app from "../../../../model";

import { formFields } from "../../../../components/formFields";
import { inventoryNumber } from "../../../../components/inventoryNumber";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { label, pick, text } = formFields(
    { draft: r.draft, options: r.options, errors: r.errors, viewing: r.viewing },
    "cloud-subscriptions",
);

/** A cloud subscription's page, then the information kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/infrastructure/cloud-subscriptions",
                back: "Cloud subscriptions",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {pick("Volume", "volume", "volumes", {
                            required: true,
                            wide: true,
                            view: (
                                <cx>
                                    <span text={r.licenseText} />
                                    {inventoryNumber(r.licenseNumber)}
                                    <span text={expr(r.designator, (d) => ` · ${d ?? ""}`)} />
                                </cx>
                            ),
                        })}
                        <div class="editor-wide" visible={r.viewing}>
                            {label("License")}
                            <div class="editor-value">
                                <Link
                                    class="editor-link"
                                    href={r.licenseHref}
                                    url={$app.url}
                                    text={r.licenseText}
                                />
                                {inventoryNumber(r.licenseNumber)}
                            </div>
                        </div>
                        {text("Management URL", "managementUrl", 500, { wide: true, url: true })}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
