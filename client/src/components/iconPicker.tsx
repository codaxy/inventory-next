import type { AccessorChain } from "cx/data";
import { expr, falsy, truthy } from "cx/ui";
import { Icon } from "cx/widgets";

import { genericIcon, glyphOf, type IconKind, iconsOf } from "../typeIcons";

/**
 * Focus, once what is named has rendered — the chosen icon on opening, the trigger on closing: cx
 * renders a frame or two after the store changes.
 */
function focusLater(selector: string, frames = 10) {
    requestAnimationFrame(() => {
        const element = document.querySelector<HTMLElement>(selector);
        if (element && element.offsetParent !== null) element.focus();
        else if (frames > 0) focusLater(selector, frames - 1);
    });
}

/**
 * A type's icon, for an employee's equipment to show its things by: a button showing the chosen glyph,
 * which opens the generic one and the kind's set as a grid — glyphs alone, named for a screen reader
 * only, behaving as one radio group. Picking one, clicking outside or Escape closes it. Read-only, the
 * chosen glyph. `open` holds whether the grid shows; `onPick` is the controller's method, given the
 * key or `null` for the generic one — the editor notes the choice was made, so a later guess from the
 * name leaves it alone.
 */
export const iconPicker = (
    kind: IconKind,
    value: AccessorChain<string | null | undefined>,
    viewing: AccessorChain<boolean>,
    open: AccessorChain<boolean | undefined>,
    onPick: string,
) => {
    const choices = [genericIcon[kind], ...iconsOf(kind)];
    const label = (key: string | null | undefined) =>
        choices.find((c) => c.key === (key ?? ""))?.label ?? genericIcon[kind].label;
    const glyph = expr(value, (key) => glyphOf(kind, key));
    const trigger = `#${kind}-icon-trigger`;

    const close = (store: any) => {
        store.set(open, false);
        focusLater(trigger);
    };

    return (
        <cx>
            <div class="editor-wide">
                <div class="editor-label" id={`${kind}-icon-label`} text="Icon" />
                <div
                    class="editor-value type-icon-chosen"
                    visible={viewing}
                    attrs={{ "aria-label": expr(value, label) }}
                >
                    <Icon name={glyph} class="type-icon-glyph" />
                </div>
                <div
                    class="type-icon-field"
                    visible={falsy(viewing)}
                    onKeyDown={(e: any, { store }: any) => {
                        if (e.key === "Escape" && store.get(open)) {
                            e.stopPropagation();
                            close(store);
                        }
                    }}
                >
                    <button
                        type="button"
                        id={`${kind}-icon-trigger`}
                        class={{ "type-icon-trigger": true, "type-icon-trigger-open": truthy(open) }}
                        attrs={{
                            "aria-haspopup": "true",
                            "aria-expanded": expr(open, (o) => (o ? "true" : "false")),
                            "aria-label": expr(value, (key) => `Icon: ${label(key)}`),
                        }}
                        onClick={(_e: unknown, { store }: any) => {
                            const opening = !store.get(open);
                            store.set(open, opening);
                            if (opening) focusLater(`.type-icon-popover .type-icon-on`);
                        }}
                    >
                        <Icon name={glyph} class="type-icon-glyph" />
                        <Icon name="moreWays" class="type-icon-chevron" />
                    </button>

                    {/* Clear, behind the grid: a click anywhere else closes it. */}
                    <div
                        class="type-icon-backdrop"
                        visible={truthy(open)}
                        onClick={(_e: unknown, { store }: any) => close(store)}
                    />
                    <div class="type-icon-popover" visible={truthy(open)}>
                        <div
                            class="type-icons"
                            attrs={{ role: "radiogroup", "aria-labelledby": `${kind}-icon-label` }}
                        >
                            {choices.map((choice) => (
                                <cx>
                                    <button
                                        type="button"
                                        class={{
                                            "type-icon": true,
                                            "type-icon-on": expr(value, (key) => (key ?? "") === choice.key),
                                        }}
                                        attrs={{
                                            role: "radio",
                                            "aria-checked": expr(value, (key) =>
                                                (key ?? "") === choice.key ? "true" : "false",
                                            ),
                                            "aria-label": choice.label,
                                        }}
                                        onClick={(_e: unknown, { store, controller }: any) => {
                                            controller[onPick](choice.key || null);
                                            close(store);
                                        }}
                                    >
                                        <Icon name={choice.glyph} />
                                    </button>
                                </cx>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </cx>
    );
};
