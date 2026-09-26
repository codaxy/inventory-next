import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LinkButton, LookupField, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";

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

const licences = [
    { value: null, text: "Any" },
    { value: true, text: "Holds licences" },
    { value: false, text: "Holds none" },
] as const;

/** Electronic device types: search, a pane of filters, each row opening the type. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide type-list" controller={Controller}>
                <h1 class="page-header page-title" text="Electronic device types" />

                {/* Pinned while the rows scroll, except while the filters are open: the pane is too tall. */}
                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
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
                        <LinkButton mod="primary" class="list-new" href="~/electronic-devices/types/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New type" />
                            <span class="sr-only sm:hidden" text="New type" />
                        </LinkButton>
                    </div>

                    <div id="type-filters" class="list-pane" visible={s.filtersOpen}>
                        <div class="list-pane-grid">
                            <div class="list-filter">
                                <div class="list-filter-label" text="Tags" />
                                <LookupField
                                    records={f.tags}
                                    options={s.tagOptions}
                                    multiple
                                    placeholder="Any tags"
                                    inputAttrs={{ "aria-label": "Tags" }}
                                />
                            </div>

                            <div class="list-filter">
                                <div class="list-filter-label" text="Licences" />
                                <div class="segmented" role="group" aria-label="Licences">
                                    {licences.map((l) => (
                                        <cx>
                                            <Button
                                                mod="hollow"
                                                class={{
                                                    "segmented-item": true,
                                                    "segmented-item-on": expr(
                                                        f.holdsLicences,
                                                        (v) => (v ?? null) === l.value,
                                                    ),
                                                }}
                                                text={l.text}
                                                onClick={(_e: unknown, { controller }: any) =>
                                                    controller.setHoldsLicences(l.value)
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
                        <button type="button" class="chip-clear" onClick="clearFilters" text="Clear all" />
                    </div>

                    <div class="list-results-head">
                        <span class="list-total" text={s.totalText} />
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
                    <div class="record-head type-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        <span text="Description" />
                        {sortHeader(s.sort, "tags", "Tags")}
                        {sortHeader(s.sort, "devices", "Devices")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row type-columns"
                            href={expr(m.$row.id, (id) => `~/electronic-devices/types/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title">
                                <span text={m.$row.name} />
                                <span
                                    class="record-flag"
                                    visible={hasValue(m.$row.licences)}
                                    text={m.$row.licences}
                                />
                            </span>
                            <span
                                class={{
                                    "record-muted": true,
                                    "record-blank": expr(m.$row.description, (d) => !d),
                                }}
                                text={expr(m.$row.description, (d) => d ?? "—")}
                            />
                            <span class="record-meta">
                                <span text={m.$row.tags} />
                                <span
                                    class="record-more"
                                    visible={hasValue(m.$row.more)}
                                    text={m.$row.more}
                                />
                            </span>
                            <span class="record-meta type-devices" text={m.$row.devices} />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="No types match" />
                    <p class="list-empty-text" text="Try fewer words or filters, or make the type." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
