import type { AccessorChain } from "cx/data";
import { expr } from "cx/ui";
import { Icon } from "cx/widgets";

const copiedFor = 1500;

/**
 * A small copy button after a list cell's value, shown while the pointer is over the cell
 * (`record-copy` in `_records.scss`): a row is a link, so its text cannot be selected, and the value
 * is otherwise a record's page away. Absent when the cell is empty or shows "—". A touch screen has
 * no hover, so it never shows there.
 *
 * `inline` puts it in the line after the value, moving what follows aside. `overlay` lays it over
 * what follows instead, taking no room, for a value followed by more text in its own cell that should
 * hold still: chosen where it is used, since laid past a value that fills its cell it would cover the
 * next column.
 *
 * Not a `<button>`: the row is an `<a>`, which may not hold one. It stops its click itself, or the
 * row's `Link` would open the record as well. Not focusable either — the row is the keyboard's stop,
 * and its record shows every value.
 */
export const copyButton = (
    value: AccessorChain<string | undefined>,
    what: string,
    placement: "inline" | "overlay" = "inline",
) => (
    <cx>
        <span
            class={placement === "overlay" ? "copy-button copy-button-overlay" : "copy-button"}
            visible={expr(value, (v) => !!v && v !== "—")}
            attrs={{ role: "button", "aria-label": `Copy ${what}`, title: `Copy ${what}` }}
            onClick={(e, instance) => {
                e.preventDefault();
                e.stopPropagation();
                const button = e.currentTarget as HTMLElement;
                navigator.clipboard.writeText(instance.store.get(value) ?? "").then(() => {
                    button.classList.add("copy-button-done");
                    setTimeout(() => button.classList.remove("copy-button-done"), copiedFor);
                });
            }}
        >
            <Icon name="copy" class="copy-button-copy size-3.5" />
            <Icon name="copied" class="copy-button-copied size-3.5" />
        </span>
    </cx>
);

/**
 * A text cell whose value can be copied: "—" in the ghost's colour when empty, and then no button.
 * `what` names the value for the button: "Copy serial number".
 */
export const copyCell = (value: AccessorChain<string | undefined>, what: string, cls = "record-meta") => (
    <cx>
        <span class={{ [cls]: true, "record-copy": true, "record-blank": expr(value, (v) => !v) }}>
            <span class="record-copy-line">
                <span text={expr(value, (v) => v ?? "—")} />
                {copyButton(value, what)}
            </span>
        </span>
    </cx>
);
