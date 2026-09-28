import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, Link, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../bindings";
import { Pager } from "../../components/Pager";
import { completeness, segmented } from "../../components/segmented";
import { sortHeader } from "../../components/sortHeader";
import { expiryClass } from "../../licensing";
import $app from "../../model";
import { listHeading, listPaging } from "../../components/listHeading";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../components/listSkeleton";
import { copyButton, copyCell } from "../../components/copyButton";

const s = m.list;
const f = s.filters;

const hasChips = isNonEmpty(s.chips);
const chipCount = expr(s.chips, (chips) => String(chips?.length ?? 0));
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

const expiries = [
    { value: null, text: "Any" },
    { value: "expired", text: "Expired" },
    { value: "soon", text: "Expires soon" },
    { value: "regular", text: "Current" },
    { value: "none", text: "No date" },
] as const;

/** Licenses: most recently changed first, each with where its subscription stands. */
export default createFunctionalComponent(() => {
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class={{ "list-top": true, "list-top-static": s.filtersOpen }}>
                    {listHeading({
                        title: "Licenses",
                        exportHref: s.exportHref,
                        addHref: "~/licenses/new",
                        addLabel: "Add license",
                    })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search number, name, vendor, invoice…"
                                    showClear
                                    inputAttrs={{ "aria-label": "Search licenses", enterKeyHint: "search" }}
                                />
                            </div>
                            <Button
                                mod="hollow"
                                class={{
                                    "list-filters-toggle": true,
                                    "list-filters-toggle-open": s.filtersOpen,
                                }}
                                attrs={{ "aria-controls": "license-filters" }}
                                onClick="toggleFilters"
                            >
                                <Icon name="filters" class="size-4" />
                                <span class="hidden sm:inline" text="Filters" />
                                <span class="list-count" visible={hasChips} text={chipCount} />
                            </Button>
                        </div>

                        <div id="license-filters" class="list-pane" visible={s.filtersOpen}>
                            <div class="list-pane-grid">
                                <div class="list-filter">
                                    <div class="list-filter-label" id="licenses-vendor-label" text="Vendor" />
                                    <LookupField
                                        id="licenses-vendor"
                                        value={f.vendorId}
                                        text={f.vendorText}
                                        options={s.vendors}
                                        placeholder="Any vendor"
                                        inputAttrs={{ "aria-label": "Vendor" }}
                                    />
                                </div>
                                <div class="list-filter">
                                    <div
                                        class="list-filter-label"
                                        id="licenses-person-label"
                                        text="Assignee"
                                    />
                                    <LookupField
                                        id="licenses-person"
                                        value={f.personId}
                                        text={f.personText}
                                        options={s.people}
                                        placeholder="Anyone"
                                        inputAttrs={{ "aria-label": "Assignee" }}
                                    />
                                </div>
                                <div class="list-filter">
                                    <div class="list-filter-label" text="Bought from" />
                                    <DateField
                                        value={dateValue(f.from)}
                                        placeholder="Any day"
                                        inputAttrs={{ "aria-label": "Bought from" }}
                                    />
                                </div>
                                <div class="list-filter">
                                    <div class="list-filter-label" text="Bought to" />
                                    <DateField
                                        value={dateValue(f.to)}
                                        placeholder="Any day"
                                        inputAttrs={{ "aria-label": "Bought to" }}
                                    />
                                </div>
                                {segmented("Subscription", expiries, f.expiry, "setExpiry")}
                                {segmented("Record", completeness, f.incomplete, "setIncomplete")}
                            </div>
                            <div class="list-pane-footer">
                                <Button
                                    mod="hollow"
                                    text="Clear filters"
                                    onClick="clearFilters"
                                    visible={hasChips}
                                />
                                <Button mod="primary" text="Done" onClick="closeFilters" />
                            </div>
                        </div>

                        <div class="list-chips" visible={hasChips}>
                            <Repeater records={s.chips} recordAlias={m.$chip}>
                                <button
                                    type="button"
                                    class="chip"
                                    onClick={(_e: unknown, { store, controller }: any) =>
                                        controller.removeFilter(store.get(m.$chip.key))
                                    }
                                >
                                    <span text={m.$chip.text} />
                                    <Icon name="close" class="size-3.5" />
                                    <span class="sr-only" text="Remove filter" />
                                </button>
                            </Repeater>
                            <button
                                type="button"
                                class="chip-clear"
                                onClick="clearFilters"
                                text="Clear all"
                            />
                        </div>
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div
                    class={{ "record-list": true, "record-list-loading": s.loading }}
                    visible={notEmpty}
                    onRef={paging.onRowsRef}
                >
                    <div class="record-head license-columns">
                        {sortHeader(s.sort, "number", "No.")}
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "vendor", "Vendor")}
                        {sortHeader(s.sort, "value", "Value", "record-num")}
                        {sortHeader(s.sort, "purchased", "Bought")}
                        {sortHeader(s.sort, "expires", "Subscription")}
                        {sortHeader(s.sort, "modified", "Changed")}
                    </div>

                    {listSkeleton({
                        columns: "license-columns",
                        cells: 7,
                        numeric: [3],
                        visible: falsy(s.loaded),
                    })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row license-columns"
                            href={expr(m.$row.id, (id) => `~/licenses/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-meta license-number record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.number} />
                                    {copyButton(m.$row.number, "number")}
                                </span>
                            </span>
                            <span class="record-title record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.name} />
                                    <span
                                        class="record-flag record-flag-warn"
                                        visible={hasValue(m.$row.incomplete)}
                                        text={m.$row.incomplete}
                                    />
                                    {copyButton(m.$row.name, "name")}
                                </span>
                            </span>
                            {copyCell(m.$row.vendor, "vendor")}
                            <span class="record-meta record-num license-value" text={m.$row.value} />
                            <span class="record-meta" text={m.$row.purchased} />
                            <span
                                class={{
                                    "record-status": true,
                                    "record-blank": expr(m.$row.expiry, (x) => !x),
                                }}
                            >
                                <span
                                    class={expr(m.$row.expiry, (x) => `status-tag ${expiryClass(x)}`)}
                                    text={expr(m.$row.expiryText, (t) => t ?? "—")}
                                />
                            </span>
                            <span class="record-meta license-changed" text={m.$row.modified} />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No licenses match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words or filters, or add the license."
                    />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={paging.onPage} />
                </div>
            </div>
        </cx>
    );
});
