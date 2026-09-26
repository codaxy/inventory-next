import type { AccessorChain } from "cx/data";
import { expr } from "cx/ui";

/**
 * A list's column header that sorts: a tap calls the screen controller's `sortBy(key)`, and the mark
 * says whether the column is in force and which way. A function, not a component — it is called while
 * the markup is built, once per column.
 */
export const sortHeader = (sort: AccessorChain<string>, key: string, text: string) => (
    <cx>
        <button
            type="button"
            class="list-sort-header"
            onClick={(_e: unknown, { controller }: any) => controller.sortBy(key)}
        >
            <span text={text} />
            <span
                class="list-sort-mark"
                text={expr(sort, (s) => (s === key ? "▲" : s === `-${key}` ? "▼" : ""))}
            />
        </button>
    </cx>
);
