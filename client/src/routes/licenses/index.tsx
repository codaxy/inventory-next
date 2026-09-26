import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, Link, LinkButton, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../bindings";
import { Pager } from "../../components/Pager";
import { completeness, segmented } from "../../components/segmented";
import { sortHeader } from "../../components/sortHeader";
import { expiryClass } from "../../licensing";
import $app from "../../model";
import { stickyBar } from "../../stickyBar";
import Controller from "./Controller";
import m from "./model";

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

/** Licences: most recently changed first, each with where its subscription stands. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="Licences" />

                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search number, name, vendor, invoice…"
                                showClear
                                inputAttrs={{ "aria-label": "Search licences", enterKeyHint: "search" }}
                            />
                        </div>
                        <Button
                            mod="hollow"
                            class={{ "list-filters-toggle": true, "list-filters-toggle-open": s.filtersOpen }}
                            attrs={{ "aria-controls": "license-filters" }}
                            onClick="toggleFilters"
                        >
                            <Icon name="filters" class="size-4" />
                            <span class="hidden sm:inline" text="Filters" />
                            <span class="list-count" visible={hasChips} text={chipCount} />
                        </Button>
                        <LinkButton mod="primary" class="list-new" href="~/licenses/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New licence" />
                            <span class="sr-only sm:hidden" text="New licence" />
                        </LinkButton>
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
                                <div class="list-filter-label" id="licenses-person-label" text="Assignee" />
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
                        <button type="button" class="chip-clear" onClick="clearFilters" text="Clear all" />
                    </div>

                    <div class="list-results-head">
                        <span class="list-total" text={s.totalText} />
                        {/* A plain anchor: cx's Link would route it inside the app instead of downloading. */}
                        <a
                            class="list-export"
                            href={s.exportHref}
                            download
                            attrs={{ title: "Download what the list shows, every page, as Excel" }}
                        >
                            <Icon name="download" class="size-3.5" />
                            <span text="Excel" />
                        </a>
                        <div>
                            <Pager
                                state={s.pager}
                                compact
                                onPage={(page, i) => i.controller.goTo(page, true)}
                            />
                        </div>
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div class={{ "record-list": true, "record-list-loading": s.loading }} visible={notEmpty}>
                    <div class="record-head license-columns">
                        {sortHeader(s.sort, "number", "No.")}
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "vendor", "Vendor")}
                        {sortHeader(s.sort, "value", "Value")}
                        {sortHeader(s.sort, "purchased", "Bought")}
                        {sortHeader(s.sort, "expires", "Subscription")}
                        {sortHeader(s.sort, "modified", "Changed")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row license-columns"
                            href={expr(m.$row.id, (id) => `~/licenses/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-meta license-number" text={m.$row.number} />
                            <span class="record-title">
                                <span text={m.$row.name} />
                                <span
                                    class="record-flag record-flag-warn"
                                    visible={hasValue(m.$row.incomplete)}
                                    text={m.$row.incomplete}
                                />
                            </span>
                            <span class="record-meta" text={m.$row.vendor} />
                            <span class="record-meta license-value" text={m.$row.value} />
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
                    <p class="list-empty-title" text="No licences match" />
                    <p class="list-empty-text" text="Try fewer words or filters, or add the licence." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
