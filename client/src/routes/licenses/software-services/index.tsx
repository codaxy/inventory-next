import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LookupField, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { listHeading, listPaging } from "../../../components/listHeading";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../../components/listSkeleton";
import { copyButton, copyCell } from "../../../components/copyButton";

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

/** Software and services: what a license's volume is of. Search, filters, each row opening the entry. */
export default createFunctionalComponent(() => {
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class="page-top">
                    {listHeading({
                        title: "Software & services",
                        addHref: "~/licenses/software-services/new",
                        addLabel: "Add software or service",
                    })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search software and services…"
                                    showClear
                                    inputAttrs={{
                                        "aria-label": "Search software and services",
                                        enterKeyHint: "search",
                                    }}
                                />
                            </div>
                            <Button
                                mod="hollow"
                                class={{
                                    "list-filters-toggle": true,
                                    "list-filters-toggle-open": s.filtersOpen,
                                }}
                                attrs={{ "aria-controls": "software-filters" }}
                                onClick="toggleFilters"
                            >
                                <Icon name="filters" class="size-4" />
                                <span class="hidden sm:inline" text="Filters" />
                                <span class="list-count" visible={hasChips} text={chipCount} />
                            </Button>
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

                <div id="software-filters" class="list-pane" visible={s.filtersOpen}>
                    <div class="list-pane-grid">
                        <div class="list-filter">
                            <div
                                class="list-filter-label"
                                id="licenses-software-services-category-label"
                                text="Category"
                            />
                            <LookupField
                                id="licenses-software-services-category"
                                value={f.categoryId}
                                text={f.categoryText}
                                options={s.categories}
                                placeholder="Any category"
                                inputAttrs={{ "aria-label": "Category" }}
                            />
                        </div>
                        <div class="list-filter">
                            <div
                                class="list-filter-label"
                                id="licenses-software-services-manufacturer-label"
                                text="Manufacturer"
                            />
                            <LookupField
                                id="licenses-software-services-manufacturer"
                                value={f.manufacturerId}
                                text={f.manufacturerText}
                                options={s.manufacturers}
                                placeholder="Any manufacturer"
                                inputAttrs={{ "aria-label": "Manufacturer" }}
                            />
                        </div>
                    </div>
                    <div class="list-pane-footer">
                        <Button mod="hollow" text="Clear filters" onClick="clearFilters" visible={hasChips} />
                        <Button mod="primary" text="Done" onClick="closeFilters" />
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
                    <div class="record-head software-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "category", "Category")}
                        {sortHeader(s.sort, "manufacturer", "Manufacturer")}
                        <span text="URL" />
                        {sortHeader(s.sort, "volumes", "Volumes", "record-num")}
                    </div>

                    {listSkeleton({
                        columns: "software-columns",
                        cells: 5,
                        numeric: [4],
                        visible: falsy(s.loaded),
                    })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row software-columns"
                            href={expr(m.$row.id, (id) => `~/licenses/software-services/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.name} />
                                    {copyButton(m.$row.name, "name")}
                                </span>
                            </span>
                            {copyCell(m.$row.category, "category")}
                            {copyCell(m.$row.manufacturer, "manufacturer")}
                            {copyCell(m.$row.url, "URL")}
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-num": true,
                                    "record-blank": expr(m.$row.volumes, (v) => !v),
                                }}
                                text={expr(m.$row.volumes, (v) => v ?? "—")}
                            />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "Nothing matches"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words or filters, or add the entry."
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
