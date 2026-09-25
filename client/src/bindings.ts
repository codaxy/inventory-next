import type { AccessorChain } from "cx/data";
import type { NumberProp } from "cx/ui";

/**
 * A `NumberField`'s `value` for a field that may hold `null`, which the widget writes on clear.
 * `NumberProp` does not admit `null`, so the accessor is widened here rather than at each call site.
 */
export function numberValue(accessor: AccessorChain<number | null | undefined>): NumberProp {
    return accessor as unknown as NumberProp;
}
