import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LinkButton, LookupField, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { stickyBar } from "../../../stickyBar";
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

/** Software and services: what a licence's volume is of. Search, filters, each row opening the entry. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="Software & services" />

                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
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
                            class={{ "list-filters-toggle": true, "list-filters-toggle-open": s.filtersOpen }}
                            attrs={{ "aria-controls": "software-filters" }}
                            onClick="toggleFilters"
                        >
                            <Icon name="filters" class="size-4" />
                            <span class="hidden sm:inline" text="Filters" />
                            <span class="list-count" visible={hasChips} text={chipCount} />
                        </Button>
                        <LinkButton mod="primary" class="list-new" href="~/licenses/software-services/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New" />
                            <span class="sr-only sm:hidden" text="New software or service" />
                        </LinkButton>
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
                    <div class="record-head software-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "category", "Category")}
                        {sortHeader(s.sort, "manufacturer", "Manufacturer")}
                        <span text="URL" />
                        {sortHeader(s.sort, "volumes", "Volumes")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row software-columns"
                            href={expr(m.$row.id, (id) => `~/licenses/software-services/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title" text={m.$row.name} />
                            <span class="record-meta" text={m.$row.category} />
                            <span class="record-meta" text={m.$row.manufacturer} />
                            <span
                                class={{ "record-meta": true, "record-blank": expr(m.$row.url, (u) => !u) }}
                                text={expr(m.$row.url, (u) => u ?? "—")}
                            />
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-blank": expr(m.$row.volumes, (v) => !v),
                                }}
                                text={expr(m.$row.volumes, (v) => v ?? "—")}
                            />
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="Nothing matches" />
                    <p class="list-empty-text" text="Try fewer words or filters, or add the entry." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
