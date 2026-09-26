import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { prose, text } = formFields(
    { draft: r.draft, errors: r.errors, viewing: r.viewing },
    "information-tag",
);

/** An information tag's page: its name and description, then the information of it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/informations/tags",
                back: "Tags",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {prose("Description", "description", 1000)}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
