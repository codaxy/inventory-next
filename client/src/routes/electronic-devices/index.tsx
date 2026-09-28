import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, Link, LinkButton, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../bindings";
import { Pager } from "../../components/Pager";
import { completeness, segmented } from "../../components/segmented";
import { sortHeader } from "../../components/sortHeader";
import $app from "../../model";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../components/listSkeleton";
import { copyButton, copyCell } from "../../components/copyButton";

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

/** Electronic devices: most recently changed first. */
export default createFunctionalComponent(() => {
    // From `md` the rows scroll inside the card (`list-fill`), so paging returns the card to its top too.
    let rowsEl: HTMLElement | null = null;
    const goTo = (page: number, instance: any) => {
        instance.controller.goTo(page, true);
        rowsEl?.scrollTo({ top: 0 });
    };

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class={{ "list-top": true, "list-top-static": s.filtersOpen }}>
                    <div class="page-header">
                        <div class="list-heading">
                            <h1 class="page-title" text="Electronic devices" />
                            <div class="list-heading-actions">
                                {/* A plain anchor: cx's Link would route it inside the app instead of downloading. */}
                                <a
                                    class="list-export"
                                    href={s.exportHref}
                                    download
                                    attrs={{
                                        "aria-label": "Download as Excel",
                                        title: "Download what the list shows, every page, as Excel",
                                    }}
                                >
                                    <Icon name="download" class="size-4" />
                                    <span class="hidden sm:inline" text="Excel" />
                                </a>
                                <LinkButton
                                    mod="primary"
                                    class="list-new"
                                    attrs={{ "aria-label": "Add device", title: "Add device" }}
                                    href="~/electronic-devices/new"
                                >
                                    <Icon name="created" class="size-4" />
                                    <span class="hidden sm:inline" text="Add" />
                                </LinkButton>
                            </div>
                        </div>
                    </div>

                    <div class="list-bar">
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
                                class={{
                                    "list-filters-toggle": true,
                                    "list-filters-toggle-open": s.filtersOpen,
                                }}
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
                                {filterPick(
                                    "Manufacturer",
                                    "manufacturer",
                                    s.manufacturers,
                                    "Any manufacturer",
                                )}
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
                    onRef={(el: HTMLElement | null) => (rowsEl = el)}
                >
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

                    {listSkeleton({ columns: "device-columns", cells: 10, visible: falsy(s.loaded) })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row device-columns"
                            href={expr(m.$row.id, (id) => `~/electronic-devices/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-meta device-number record-copy">
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
                            {copyCell(m.$row.model, "model")}
                            {copyCell(m.$row.assignee, "assignee")}
                            {copyCell(m.$row.location, "location")}
                            {copyCell(m.$row.type, "type")}
                            {copyCell(m.$row.manufacturer, "manufacturer")}
                            {copyCell(m.$row.modelCode, "model code")}
                            {copyCell(m.$row.serial, "serial number")}
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
                        text="Try fewer words or filters, or add the device."
                    />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={goTo} />
                </div>
            </div>
        </cx>
    );
});
