/** One page of a list, as every list endpoint answers: the window and the filtered total. */
export interface Page<T> {
    items: T[];
    total: number;
}

export interface PagerLink {
    /** Absent on a gap. */
    page?: number;
    current: boolean;
    text: string;
}

/** What the pager shows, derived once per answer so the view binds plain values. */
export interface PagerState {
    page: number;
    pageCount: number;
    summary: string;
    /** The short form for a phone: "3 / 40". */
    position: string;
    hasPrevious: boolean;
    hasNext: boolean;
    links: PagerLink[];
}

const number = new Intl.NumberFormat("en-GB");

/**
 * The first, the last, and the current page with a neighbour each side; a gap wherever pages are
 * skipped. At most seven links, so the row never wraps at the width it is shown at.
 */
export function pager(page: number, pageSize: number, total: number): PagerState {
    const pageCount = Math.max(1, Math.ceil(total / pageSize));
    const from = total === 0 ? 0 : (page - 1) * pageSize + 1;
    const to = Math.min(total, page * pageSize);

    const shown = new Set([1, pageCount, page - 1, page, page + 1].filter((p) => p >= 1 && p <= pageCount));
    const links: PagerLink[] = [];
    let previous = 0;

    for (const p of [...shown].sort((a, b) => a - b)) {
        if (p - previous === 2) links.push({ page: p - 1, current: false, text: String(p - 1) });
        else if (p - previous > 2) links.push({ current: false, text: "…" });

        links.push({ page: p, current: p === page, text: String(p) });
        previous = p;
    }

    return {
        page,
        pageCount,
        summary:
            total === 0
                ? "No results"
                : `${number.format(from)}–${number.format(to)} of ${number.format(total)}`,
        position: `${page} / ${pageCount}`,
        hasPrevious: page > 1,
        hasNext: page < pageCount,
        links,
    };
}
