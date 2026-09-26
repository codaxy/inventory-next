import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { pick, text } = formFields(
    { draft: r.draft, options: r.options, errors: r.errors, viewing: r.viewing },
    "project",
);

/** A project's page: its client and who leads it, each a link, then the information of it. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/company/projects",
                back: "Projects",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {pick("Client", "client", "clients", {
                            required: true,
                            href: (id) => `~/company/clients/${id}`,
                        })}
                        {pick("Led by", "owner", "people", {
                            required: true,
                            href: (id) => `~/company/people/${id}`,
                        })}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
