import type { AccessorChain } from "cx/data";
import { expr } from "cx/ui";
import { Button } from "cx/widgets";

/** A list filter as a segmented switch: `setter` names the list controller's method taking the chosen value. */
export const segmented = <T extends string | boolean | null>(
    label: string,
    items: readonly { value: T; text: string }[],
    value: AccessorChain<T | null | undefined>,
    setter: string,
) => (
    <cx>
        <div class="list-filter list-filter-wide">
            <div class="list-filter-label" text={label} />
            <div class="segmented" role="group" aria-label={label}>
                {items.map((item) => (
                    <cx>
                        <Button
                            mod="hollow"
                            class={{
                                "segmented-item": true,
                                "segmented-item-on": expr(value, (v) => (v ?? null) === item.value),
                            }}
                            text={item.text}
                            onClick={(_e: unknown, { controller }: any) => controller[setter](item.value)}
                        />
                    </cx>
                ))}
            </div>
        </div>
    </cx>
);

/** Complete or incomplete, the one filter every asset list has. */
export const completeness = [
    { value: null, text: "Any" },
    { value: false, text: "Complete" },
    { value: true, text: "Incomplete" },
] as const;
