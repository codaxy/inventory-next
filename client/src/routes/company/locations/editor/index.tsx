import { createFunctionalComponent } from "cx/ui";

import { formFields } from "../../../../components/formFields";
import { recordPage } from "../../../../components/recordPage";
import Controller from "./Controller";
import m from "./model";

const r = m.record;
const { pick, prose, text, whole } = formFields(
    { draft: r.draft, options: r.options, errors: r.errors, viewing: r.viewing },
    "location",
);

/** A location's page: its address, then the assets kept there and the information stored there. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/company/locations",
                back: "Locations",
                fields: (
                    <cx>
                        {text("Name", "name", 50, { required: true, wide: true })}
                        {text("Street", "street", 50, { required: true })}
                        {whole("House number", "houseNumber", { min: 0 })}
                        {text("Postal code", "postalCode", 8)}
                        {pick("City", "city", "cities", { required: true })}
                        {pick("State", "state", "states")}
                        {pick("Country", "country", "countries", { required: true })}
                        {whole("Floor", "floor")}
                        {text("Room", "room", 30)}
                        {prose("Description", "description", 1000)}
                    </cx>
                ),
            })}
        </div>
    </cx>
));
