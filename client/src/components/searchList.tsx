import type { AccessorChain } from "cx/data";
import { expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, TextField } from "cx/widgets";

import $app from "../model";
import type { PagerState } from "../paging";
import { stickyBar } from "../stickyBar";
import { Pager } from "./Pager";
import { sortHeader } from "./sortHeader";

/** What a searchable list's markup binds; `ListController` keeps it. */
export interface SearchListState {
    search?: string | null;
    sort: string;
    rows: unknown[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
    /** The search that ran is one id: an empty answer then says no record has it. */
    idSearch?: boolean;
}

export interface Column {
    header: string;
    /** The sort key a tap on the header orders by; absent for a column that does not sort. */
    sort?: string;
    /** The cell, bound to the row's alias. */
    cell: any;
}

interface SearchList {
    s: AccessorChain<SearchListState>;
    /** The row alias the cells bind, declared in the screen's model. */
    row: AccessorChain<{ id: string }>;
    title: string;
    /** "clients": what the search box says it searches, and how the empty list names them. */
    noun: string;
    placeholder: string;
    newHref: string;
    /** "Add vendor": the button reads "Add", and this is its name for a screen reader and its tooltip. */
    addText: string;
    /** The row's record page, from its id. */
    href: (id: string) => string;
    /** The screen's class that lays the columns out, in `_records.scss`. */
    columns: string;
    cells: Column[];
}

/**
 * A list that is searched, not filtered — the directory's shape: the pinned bar with search, New and
 * the compact pager, sortable columns, a row opening its record, and the empty and error states.
 * Called inside the screen's component, which holds the `ListController`.
 */
export function searchList(o: SearchList) {
    const onBarRef = stickyBar();
    const s = o.s;
    const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
    const notEmpty = expr(
        s.loaded,
        s.total,
        s.error,
        (loaded, total, error) => !(loaded && total === 0 && !error),
    );

    return (
        <cx>
            <h1 class="page-header page-title" text={o.title} />

            <div class="list-bar" onRef={onBarRef}>
                <div class="list-toolbar">
                    <div class="list-search">
                        <Icon name="search" class="list-search-icon" />
                        <TextField
                            class="list-search-field"
                            value={s.search}
                            placeholder={o.placeholder}
                            showClear
                            inputAttrs={{ "aria-label": `Search ${o.noun}`, enterKeyHint: "search" }}
                        />
                    </div>
                    <LinkButton
                        mod="primary"
                        class="list-new"
                        href={o.newHref}
                        attrs={{ "aria-label": o.addText, title: o.addText }}
                    >
                        <Icon name="created" class="size-4" />
                        <span class="hidden sm:inline" text="Add" />
                    </LinkButton>
                </div>

                <div class="list-results-head">
                    <span class="list-total" text={s.totalText} />
                    <div>
                        <Pager state={s.pager} compact onPage={(page, i) => i.controller.goTo(page, true)} />
                    </div>
                </div>
            </div>

            <div class="list-error" visible={hasValue(s.error)}>
                <span text={s.error} />
                <Button mod="hollow" text="Try again" onClick="load" />
            </div>

            <div class={{ "record-list": true, "record-list-loading": s.loading }} visible={notEmpty}>
                <div class={`record-head ${o.columns}`}>
                    {o.cells.map((c) =>
                        c.sort ? (
                            sortHeader(s.sort as any, c.sort, c.header)
                        ) : (
                            <cx>
                                <span text={c.header} />
                            </cx>
                        ),
                    )}
                </div>

                <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                <Repeater records={s.rows as any} recordAlias={o.row} keyField="id">
                    <Link class={`record-row ${o.columns}`} href={expr(o.row.id, o.href)} url={$app.url}>
                        {o.cells.map((c) => c.cell)}
                    </Link>
                </Repeater>
            </div>

            <div class="list-empty" visible={empty}>
                <Icon name="search" class="size-6" />
                <p
                    class="list-empty-title"
                    text={expr(s.idSearch, (id) => (id ? "No record has this id" : `No ${o.noun} match`))}
                />
                <p
                    class="list-empty-text"
                    visible={falsy(s.idSearch)}
                    text={`Try fewer words, or add one.`}
                />
                <Button mod="hollow" text="Clear search" onClick="clearSearch" />
            </div>

            <div visible={expr(s.total, (t) => t > 0)}>
                <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
            </div>
        </cx>
    );
}

/** A cell that may be empty: "—" in the ghost's colour when it is. */
export const optionalCell = (value: AccessorChain<string | undefined>, cls = "record-meta") => (
    <cx>
        <span
            class={{ [cls]: true, "record-blank": expr(value, (v) => !v) }}
            text={expr(value, (v) => v ?? "—")}
        />
    </cx>
);

/** A count under its header from `md`; a phone's card, which has none, keeps the word. */
export const countCell = (value: AccessorChain<string | undefined>, word: AccessorChain<string>) => (
    <cx>
        <span class={{ "record-meta": true, "record-blank": expr(value, (v) => !v) }}>
            <span text={expr(value, (v) => v ?? "—")} />
            <span class="md:hidden" visible={hasValue(value)} text={expr(word, (w) => ` ${w}`)} />
        </span>
    </cx>
);

/** A row's count and its word: absent under its header when zero, the word for a phone. */
export const counted = (n: number, one: string, many: string) => ({
    value: n ? String(n) : undefined,
    word: n === 1 ? one : many,
});
