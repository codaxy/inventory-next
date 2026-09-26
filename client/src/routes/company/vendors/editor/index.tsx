import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { text } = formFields({ draft: r.draft, errors: r.errors, viewing: r.viewing }, "vendor");

/** A vendor's page: who they are and how to reach them, then what was bought from them. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/company/vendors",
                back: "Vendors",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {text("Contact person", "contactPerson", 200)}
                        {text("Email", "email", 200)}
                        {text("Phone", "phone", 200)}
                        {text("Mobile phone", "mobilePhone", 200)}
                        {text("Address", "location", 200, { wide: true })}
                        {text("Registration number", "registrationNumber", 200)}
                        {text("VAT number", "vatNumber", 200)}
                        {text("Website", "web", 500, { wide: true, url: true })}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
