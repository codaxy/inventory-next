import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, Link, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../bindings";
import { Pager } from "../../components/Pager";
import { completeness, segmented } from "../../components/segmented";
import { sortHeader } from "../../components/sortHeader";
import $app from "../../model";
import { stickyBar } from "../../stickyBar";
import Controller from "./Controller";
import m from "./model";

const s = m.list;
const f = s.filters as any;

const hasChips = isNonEmpty(s.chips);
const chipCount = expr(s.chips, (chips) => String(chips?.length ?? 0));
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** A picker in the filters pane, bound as `<key>Id` and `<key>Text`. */
const filterPick = (label: string, key: string, options: AccessorChain<unknown[]>, placeholder: string) => (
    <cx>
        <div class="list-filter">
            <div class="list-filter-label" id={`devices-${key}-label`} text={label} />
            <LookupField
                id={`devices-${key}`}
                value={f[`${key}Id`]}
                text={f[`${key}Text`]}
                options={options}
                placeholder={placeholder}
                inputAttrs={{ "aria-label": label }}
            />
        </div>
    </cx>
);

/** A cell that may be empty: "—" in the ghost's colour when it is. */
const optional = (value: AccessorChain<string | undefined>) => (
    <cx>
        <span
            class={{ "record-meta": true, "record-blank": expr(value, (v) => !v) }}
            text={expr(value, (v) => v ?? "—")}
        />
    </cx>
);

/** Electronic devices: most recently changed first. Read-only until editing lands: no New yet. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="Electronic devices" />

                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search number, name, model, serial…"
                                showClear
                                inputAttrs={{ "aria-label": "Search devices", enterKeyHint: "search" }}
                            />
                        </div>
                        <Button
                            mod="hollow"
                            class={{ "list-filters-toggle": true, "list-filters-toggle-open": s.filtersOpen }}
                            attrs={{ "aria-controls": "device-filters" }}
                            onClick="toggleFilters"
                        >
                            <Icon name="filters" class="size-4" />
                            <span class="hidden sm:inline" text="Filters" />
                            <span class="list-count" visible={hasChips} text={chipCount} />
                        </Button>
                    </div>

                    <div id="device-filters" class="list-pane" visible={s.filtersOpen}>
                        <div class="list-pane-grid">
                            {filterPick("Type", "type", s.types, "Any type")}
                            {filterPick("Tag", "tag", s.tags, "Any tag")}
                            {filterPick("Assignee", "person", s.people, "Anyone")}
                            {filterPick("Location", "location", s.locations, "Anywhere")}
                            {filterPick("Manufacturer", "manufacturer", s.manufacturers, "Any manufacturer")}
                            {filterPick("Vendor", "vendor", s.vendors, "Any vendor")}
                            <div class="list-filter">
                                <div class="list-filter-label" text="Bought from" />
                                <DateField
                                    value={dateValue(s.filters.from)}
                                    placeholder="Any day"
                                    inputAttrs={{ "aria-label": "Bought from" }}
                                />
                            </div>
                            <div class="list-filter">
                                <div class="list-filter-label" text="Bought to" />
                                <DateField
                                    value={dateValue(s.filters.to)}
                                    placeholder="Any day"
                                    inputAttrs={{ "aria-label": "Bought to" }}
                                />
                            </div>
                            {segmented("Record", completeness, s.filters.incomplete, "setIncomplete")}
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
                    <div class="record-head device-columns">
                        {sortHeader(s.sort, "number", "No.")}
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "model", "Model")}
                        {sortHeader(s.sort, "assignee", "Assignee")}
                        {sortHeader(s.sort, "location", "Location")}
                        {sortHeader(s.sort, "type", "Type")}
                        {sortHeader(s.sort, "manufacturer", "Manufacturer")}
                        <span text="Model code" />
                        <span text="Serial" />
                        {sortHeader(s.sort, "modified", "Changed")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row device-columns"
                            href={expr(m.$row.id, (id) => `~/electronic-devices/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-meta device-number" text={m.$row.number} />
                            <span class="record-title">
                                <span text={m.$row.name} />
                                <span
                                    class="record-flag record-flag-warn"
                                    visible={hasValue(m.$row.incomplete)}
                                    text={m.$row.incomplete}
                                />
                            </span>
                            {optional(m.$row.model)}
                            <span class="record-meta" text={m.$row.assignee} />
                            {optional(m.$row.location)}
                            {optional(m.$row.type)}
                            {optional(m.$row.manufacturer)}
                            {optional(m.$row.modelCode)}
                            {optional(m.$row.serial)}
                            <span class="record-meta device-changed" text={m.$row.modified} />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No devices match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words or filters."
                    />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
