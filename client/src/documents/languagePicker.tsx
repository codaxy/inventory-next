import type { AccessorChain } from "cx/data";
import { LookupField } from "cx/widgets";

import { documentLanguages, type Language } from "./languages";

/**
 * A document's language, for its header band: the languages it is printed in, each named in itself.
 * `id` names the field, whose closed state is labelled only through `<id>-label`.
 */
export function languagePicker(o: { id: string; value: AccessorChain<Language | undefined> }) {
    return (
        <cx>
            <LookupField
                id={o.id}
                class="document-language"
                value={o.value}
                options={[...documentLanguages]}
                hideClear
            />
            <span id={`${o.id}-label`} class="sr-only" text="Document language" />
        </cx>
    );
}
