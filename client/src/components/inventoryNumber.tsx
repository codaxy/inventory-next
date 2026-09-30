import type { AccessorChain } from "cx/data";
import { hasValue } from "cx/ui";

/**
 * An inventory number after a name, muted so it never reads as part of the name; nothing when there
 * is none. The number is bound as `numberText()` gives it.
 */
export const inventoryNumber = (number: AccessorChain<string | undefined>) => (
    <cx>
        <span class="inventory-number" visible={hasValue(number)} text={number} />
    </cx>
);
