import { createFunctionalComponent, equal, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../../bindings";
import { Pager } from "../../../components/Pager";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";
import { inventoryNumberPattern } from "./utils";

const s = m.auditLog;
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
const newestFirst = equal(s.sort, "-time");
const oldestFirst = equal(s.sort, "time");

const actions = [
    { value: null, text: "All" },
    { value: "Create", text: "Created" },
    { value: "Update", text: "Updated" },
    { value: "Delete", text: "Deleted" },
] as const;

/**
 * The audit log: one search box, with every other filter in a pane it drops open. What is filtered
 * stays visible as chips, so the pane can close without hiding it.
 */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body audit-log" controller={Controller}>
                <h1 class="page-header page-title" text="Audit log" />

                {/* Pinned while the rows scroll, except while the filters are open: the pane is too tall. */}
                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
                    <div class="audit-toolbar">
                        <div class="audit-search">
                            <Icon name="search" class="audit-search-icon" />
                            <TextField
                                class="audit-search-field"
                                value={s.search}
                                placeholder="Search changes, people, records…"
                                showClear
                                inputAttrs={{
                                    "aria-label": "Search the audit log",
                                    type: "search",
                                    enterKeyHint: "search",
                                }}
                            />
                        </div>
                        <Button
                            mod="hollow"
                            class={{
                                "audit-filters-toggle": true,
                                "audit-filters-toggle-open": s.filtersOpen,
                            }}
                            attrs={{ "aria-controls": "audit-filters" }}
                            onClick="toggleFilters"
                        >
                            <Icon name="filters" class="size-4" />
                            <span class="hidden sm:inline" text="Filters" />
                            <span class="audit-count" visible={hasChips} text={chipCount} />
                        </Button>
                    </div>

                    <div id="audit-filters" class="audit-pane" visible={s.filtersOpen}>
                        <div class="audit-pane-grid">
                            <div class="audit-filter audit-filter-wide">
                                <div class="audit-filter-label" text="Change" />
                                <div class="segmented" role="group" aria-label="Change">
                                    {actions.map((a) => (
                                        <cx>
                                            <Button
                                                mod="hollow"
                                                class={{
                                                    "segmented-item": true,
                                                    "segmented-item-on": expr(
                                                        f.action,
                                                        (v) => (v ?? null) === a.value,
                                                    ),
                                                }}
                                                text={a.text}
                                                onClick={(_e: unknown, { controller }: any) =>
                                                    controller.setAction(a.value)
                                                }
                                            />
                                        </cx>
                                    ))}
                                </div>
                            </div>

                            <div class="audit-filter">
                                <div class="audit-filter-label" text="Record type" />
                                <LookupField
                                    value={f.table}
                                    options={s.tables}
                                    placeholder="Any type"
                                    inputAttrs={{ "aria-label": "Record type" }}
                                />
                            </div>

                            <div class="audit-filter">
                                <div class="audit-filter-label" text="Changed by" />
                                <LookupField
                                    value={f.email}
                                    options={s.emails}
                                    placeholder="Anyone"
                                    inputAttrs={{ "aria-label": "Changed by" }}
                                />
                            </div>

                            <div class="audit-filter">
                                <div class="audit-filter-label" text="From" />
                                <DateField
                                    value={dateValue(f.from)}
                                    placeholder="Any day"
                                    inputAttrs={{ "aria-label": "From" }}
                                />
                            </div>

                            <div class="audit-filter">
                                <div class="audit-filter-label" text="To" />
                                <DateField
                                    value={dateValue(f.to)}
                                    placeholder="Any day"
                                    inputAttrs={{ "aria-label": "To" }}
                                />
                            </div>

                            <div class="audit-filter">
                                <div class="audit-filter-label" text="Inventory number" />
                                <TextField
                                    value={f.inventoryNumber}
                                    placeholder="e.g. 100893"
                                    inputAttrs={{ "aria-label": "Inventory number", inputMode: "numeric" }}
                                    onValidate={(v: string | null) =>
                                        !v || inventoryNumberPattern.test(v) ? undefined : "Digits only."
                                    }
                                />
                            </div>
                        </div>

                        <div class="audit-pane-footer">
                            <Button
                                mod="hollow"
                                text="Clear filters"
                                onClick="clearFilters"
                                visible={hasChips}
                            />
                            <Button mod="primary" text="Done" onClick="closeFilters" />
                        </div>
                    </div>

                    <div class="audit-chips" visible={hasChips}>
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

                    <div class="audit-results-head">
                        <span class="audit-total" text={s.totalText} />
                        <Button
                            mod="hollow"
                            class="audit-sort"
                            onClick="toggleSort"
                            attrs={{ "aria-label": "Change the order" }}
                        >
                            <Icon name="newestFirst" class="size-3.5" visible={newestFirst} />
                            <Icon name="oldestFirst" class="size-3.5" visible={oldestFirst} />
                            <span
                                class="hidden sm:inline"
                                text={expr(s.sort, (sort) =>
                                    sort === "time" ? "Oldest first" : "Newest first",
                                )}
                            />
                        </Button>
                        <div visible={expr(s.total, (t) => t > 0)}>
                            <Pager
                                state={s.pager}
                                compact
                                onPage={(page, instance) => instance.controller.goTo(page, true)}
                            />
                        </div>
                    </div>
                </div>

                <div class="audit-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div class={{ "audit-list": true, "audit-list-loading": s.loading }} visible={notEmpty}>
                    <div class="audit-list-head" aria-hidden="true">
                        <span text="Time" />
                        <span text="Change" />
                        <span text="Record" />
                        <span text="Fields" />
                        <span text="By" />
                    </div>

                    <div class="audit-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <div
                            class="audit-day"
                            visible={hasValue(m.$row.dayHeading)}
                            text={m.$row.dayHeading}
                        />
                        <button
                            type="button"
                            class="audit-row"
                            onClick={(_e: unknown, { store, controller }: any) =>
                                controller.openEntry(store.get(m.$row))
                            }
                        >
                            <span class="audit-time" text={m.$row.time} />
                            <span
                                class={{
                                    "audit-action": true,
                                    "audit-action-create": equal(m.$row.action, "Create"),
                                    "audit-action-update": equal(m.$row.action, "Update"),
                                    "audit-action-delete": equal(m.$row.action, "Delete"),
                                }}
                            >
                                <Icon name={m.$row.actionIcon} class="size-3.5" />
                                <span text={m.$row.actionText} />
                            </span>
                            <span class="audit-record">
                                <span class="audit-type" text={m.$row.type} />
                                <span class="audit-label" text={m.$row.label} />
                                <span
                                    class="audit-number"
                                    visible={hasValue(m.$row.inventoryNumber)}
                                    text={m.$row.inventoryNumber}
                                />
                            </span>
                            <span class="audit-summary" text={m.$row.summary} />
                            <span class="audit-user" text={m.$row.email} />
                        </button>
                    </Repeater>
                </div>

                <div class="audit-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="audit-empty-title" text="No changes match" />
                    <p class="audit-empty-text" text="Try fewer words, or loosen a filter." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager
                        state={s.pager}
                        onPage={(page, instance) => instance.controller.goTo(page, true)}
                    />
                </div>
            </div>
        </cx>
    );
});
