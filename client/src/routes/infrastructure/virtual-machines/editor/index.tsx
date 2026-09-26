import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { text } = formFields({ draft: r.draft, errors: r.errors, viewing: r.viewing }, "virtual-machines");

/** A virtual machine's page, then the information kept on it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/infrastructure/virtual-machines",
                back: "Virtual machines",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {text("IP address", "ipAddress", 100)}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
