import type { BooleanProp } from "cx/ui";

// Bar widths per row, fixed so the outline does not reshuffle on every render; a cell takes its
// row's width shifted by its column, so neighbouring bars differ. A right-aligned cell shrinks to its
// content, where a percentage is of nothing, so its bar is a fixed width.
const widths = [62, 44, 78, 52, 70, 38, 58, 48];

interface Props {
    /** The list's columns class, the one its head and rows carry. */
    columns: string;
    /** How many cells a row has. */
    cells: number;
    /** The cells, from 0, whose values are right-aligned. */
    numeric?: number[];
    visible: BooleanProp;
}

/**
 * A list's first load: eight rows in its own columns, a bar in each cell, under its real head. A
 * refetch does not use it — the rows already shown stay, dimmed.
 */
export const listSkeleton = ({ columns, cells, numeric = [], visible }: Props) => (
    <cx>
        <div visible={visible} attrs={{ role: "status" }}>
            <span class="sr-only" text="Loading…" />
            {widths.map((width, row) => (
                <cx>
                    <div
                        class={`record-row record-row-skeleton ${columns}`}
                        attrs={{ "aria-hidden": "true" }}
                    >
                        {Array.from({ length: cells }, (_, cell) => (
                            <cx>
                                <span class={numeric.includes(cell) ? "record-num" : undefined}>
                                    <span
                                        class="skeleton-bar"
                                        style={
                                            numeric.includes(cell)
                                                ? "width: 3rem"
                                                : `width: ${widths[(row + cell * 3) % widths.length]}%`
                                        }
                                    />
                                </span>
                            </cx>
                        ))}
                    </div>
                </cx>
            ))}
        </div>
    </cx>
);
