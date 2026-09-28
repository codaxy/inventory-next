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

const s = m.types;
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

const licenses = [
    { value: null, text: "Any" },
    { value: true, text: "Holds licenses" },
    { value: false, text: "Holds none" },
] as const;

/** Electronic device types: search, a pane of filters, each row opening the type. */
export default createFunctionalComponent(() => {
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill type-list" controller={Controller}>
                <div class={{ "list-top": true, "list-top-static": s.filtersOpen }}>
                    {listHeading({
                        title: "Electronic device types",
                        addHref: "~/electronic-devices/types/new",
                        addLabel: "Add type",
                    })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search types…"
                                    showClear
                                    inputAttrs={{ "aria-label": "Search types", enterKeyHint: "search" }}
                                />
                            </div>
                            <Button
                                mod="hollow"
                                class={{
                                    "list-filters-toggle": true,
                                    "list-filters-toggle-open": s.filtersOpen,
                                }}
                                attrs={{ "aria-controls": "type-filters" }}
                                onClick="toggleFilters"
                            >
                                <Icon name="filters" class="size-4" />
                                <span class="hidden sm:inline" text="Filters" />
                                <span class="list-count" visible={hasChips} text={chipCount} />
                            </Button>
                        </div>

                        <div id="type-filters" class="list-pane" visible={s.filtersOpen}>
                            <div class="list-pane-grid">
                                <div class="list-filter">
                                    <div
                                        class="list-filter-label"
                                        id="electronic-devices-types-tags-label"
                                        text="Tags"
                                    />
                                    <LookupField
                                        id="electronic-devices-types-tags"
                                        records={f.tags}
                                        options={s.tagOptions}
                                        multiple
                                        placeholder="Any tags"
                                        inputAttrs={{ "aria-label": "Tags" }}
                                    />
                                </div>

                                <div class="list-filter">
                                    <div class="list-filter-label" text="Licenses" />
                                    <div class="segmented" role="group" aria-label="Licenses">
                                        {licenses.map((l) => (
                                            <cx>
                                                <Button
                                                    mod="hollow"
                                                    class={{
                                                        "segmented-item": true,
                                                        "segmented-item-on": expr(
                                                            f.holdsLicenses,
                                                            (v) => (v ?? null) === l.value,
                                                        ),
                                                    }}
                                                    text={l.text}
                                                    onClick={(_e: unknown, { controller }: any) =>
                                                        controller.setHoldsLicenses(l.value)
                                                    }
                                                />
                                            </cx>
                                        ))}
                                    </div>
                                </div>
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
                    <div class="record-head type-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        <span text="Description" />
                        {sortHeader(s.sort, "tags", "Tags")}
                        {sortHeader(s.sort, "devices", "Devices", "record-num")}
                    </div>

                    {listSkeleton({
                        columns: "type-columns",
                        cells: 4,
                        numeric: [3],
                        visible: falsy(s.loaded),
                    })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row type-columns"
                            href={expr(m.$row.id, (id) => `~/electronic-devices/types/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.name} />
                                    <span
                                        class="record-flag"
                                        visible={hasValue(m.$row.licenses)}
                                        text={m.$row.licenses}
                                    />
                                    {copyButton(m.$row.name, "name")}
                                </span>
                            </span>
                            {copyCell(m.$row.description, "description", "record-muted")}
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-blank": expr(m.$row.tags, (t) => !t),
                                }}
                            >
                                <span text={expr(m.$row.tags, (t) => t ?? "—")} />
                                <span
                                    class="record-more"
                                    visible={hasValue(m.$row.more)}
                                    text={m.$row.more}
                                />
                            </span>
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-num": true,
                                    "record-blank": expr(m.$row.devices, (d) => !d),
                                }}
                                text={expr(m.$row.devices, (d) => d ?? "—")}
                            />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No types match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words or filters, or make the type."
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
