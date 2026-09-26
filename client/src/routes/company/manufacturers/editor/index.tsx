import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { text } = formFields({ draft: r.draft, errors: r.errors, viewing: r.viewing }, "manufacturer");

/** A manufacturer's page: the name and site, then their devices and their software and services. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/company/manufacturers",
                back: "Manufacturers",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {text("Website", "url", 500, { wide: true, url: true })}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
