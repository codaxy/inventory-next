import { createFunctionalComponent, equal, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, DateField, Icon, LookupField, Repeater, TextField } from "cx/widgets";

import { dateValue } from "../../../bindings";
import { listHeading, listPaging } from "../../../components/listHeading";
import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import { pageTop } from "../../../pageTop";
import Controller from "./Controller";
import m, { type Filters } from "./model";
import { inventoryNumberPattern, recordIdPattern } from "./utils";

const s = m.auditLog;
const f = s.filters;

const hasChips = isNonEmpty(s.chips);
const chipCount = expr(s.chips, (chips) => String(chips?.length ?? 0));
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
// One record's history and nothing narrowing it: an empty answer means the log never saw a change.
const recordOnly = (f: Filters | undefined, q: string | null | undefined) =>
    !!f?.entityId &&
    recordIdPattern.test(f.entityId.trim()) &&
    !q &&
    !f.action &&
    !f.table &&
    !f.email &&
    !f.from &&
    !f.to &&
    !f.inventoryNumber;
const emptyTitle = expr(s.idSearch, s.filters, s.search, (id, f, q) =>
    recordOnly(f, q)
        ? "No changes recorded for this record"
        : id
          ? "No record has this id"
          : "No changes match",
);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

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
    const onTopRef = pageTop();
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide audit-log list-fill" controller={Controller}>
                <div class="page-top" onRef={onTopRef}>
                    {listHeading({ title: "Audit log" })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search changes, people, records…"
                                    showClear
                                    inputAttrs={{
                                        "aria-label": "Search the audit log",
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
                                attrs={{ "aria-controls": "audit-filters" }}
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

                <div id="audit-filters" class="list-pane" visible={s.filtersOpen}>
                    <div class="list-pane-grid">
                        <div class="list-filter list-filter-wide">
                            <div class="list-filter-label" text="Change" />
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

                        <div class="list-filter">
                            <div
                                class="list-filter-label"
                                id="administration-audit-log-record-type-label"
                                text="Record type"
                            />
                            <LookupField
                                id="administration-audit-log-record-type"
                                value={f.table}
                                options={s.tables}
                                placeholder="Any type"
                                inputAttrs={{ "aria-label": "Record type" }}
                            />
                        </div>

                        <div class="list-filter">
                            <div class="list-filter-label" text="Record id" />
                            <TextField
                                value={f.entityId}
                                placeholder="Paste an id"
                                inputAttrs={{
                                    "aria-label": "Record id",
                                    spellCheck: false,
                                    autoComplete: "off",
                                }}
                                onValidate={(v: string | null) =>
                                    !v || recordIdPattern.test(v.trim()) ? undefined : "Not a record id."
                                }
                            />
                        </div>

                        <div class="list-filter">
                            <div class="list-filter-label" text="Inventory number" />
                            <TextField
                                value={f.inventoryNumber}
                                placeholder="e.g. 100893"
                                inputAttrs={{
                                    "aria-label": "Inventory number",
                                    inputMode: "numeric",
                                }}
                                onValidate={(v: string | null) =>
                                    !v || inventoryNumberPattern.test(v) ? undefined : "Digits only."
                                }
                            />
                        </div>

                        <div class="list-filter">
                            <div
                                class="list-filter-label"
                                id="administration-audit-log-changed-by-label"
                                text="Changed by"
                            />
                            <LookupField
                                id="administration-audit-log-changed-by"
                                value={f.email}
                                options={s.emails}
                                placeholder="Anyone"
                                inputAttrs={{ "aria-label": "Changed by" }}
                            />
                        </div>

                        <div class="list-filter">
                            <div class="list-filter-label" text="From" />
                            <DateField
                                value={dateValue(f.from)}
                                placeholder="Any day"
                                inputAttrs={{ "aria-label": "From" }}
                            />
                        </div>

                        <div class="list-filter">
                            <div class="list-filter-label" text="To" />
                            <DateField
                                value={dateValue(f.to)}
                                placeholder="Any day"
                                inputAttrs={{ "aria-label": "To" }}
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

                <div class={{ "audit-list": true, "audit-list-loading": s.loading }} visible={notEmpty}>
                    <div class="audit-list-head">
                        {sortHeader(s.sort, "time", "Time")}
                        <span text="Change" />
                        <span text="Record" />
                        <span text="Fields" />
                        <span text="By" />
                    </div>

                    <div class="list-rows" onRef={paging.onRowsRef}>
                        {/* The first load: rows in the log's own areas, a bar in each. */}
                        <div visible={falsy(s.loaded)} attrs={{ role: "status" }}>
                            <span class="sr-only" text="Loading…" />
                            {[58, 42, 70, 50, 64, 38, 54, 46].map((width) => (
                                <cx>
                                    <div
                                        class="audit-row audit-row-skeleton"
                                        attrs={{ "aria-hidden": "true" }}
                                    >
                                        <span class="audit-time">
                                            <span class="skeleton-bar" style="width: 3rem" />
                                        </span>
                                        <span class="audit-action audit-action-skeleton" />
                                        <span class="audit-record">
                                            <span class="skeleton-bar" style={`width: ${width}%`} />
                                        </span>
                                        <span class="audit-summary">
                                            <span class="skeleton-bar" style={`width: ${100 - width}%`} />
                                        </span>
                                        <span class="audit-user">
                                            <span class="skeleton-bar" style="width: 9rem" />
                                        </span>
                                    </div>
                                </cx>
                            ))}
                        </div>

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
                                <span class="audit-summary">
                                    <span text={m.$row.summary} />
                                    <span
                                        class="record-more"
                                        visible={hasValue(m.$row.more)}
                                        text={m.$row.more}
                                    />
                                </span>
                                <span class="audit-user" text={m.$row.email} />
                            </button>
                        </Repeater>
                    </div>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text={emptyTitle} />
                    {/* The log began with its migration; what nobody has changed since has no entry. */}
                    <p
                        class="list-empty-text"
                        visible={expr(s.filters, s.search, recordOnly)}
                        text="The log begins on 5 December 2022, and this record has not changed since."
                    />
                    <p
                        class="list-empty-text"
                        visible={expr(
                            s.idSearch,
                            s.filters,
                            s.search,
                            (id, f, q) => !id && !recordOnly(f, q),
                        )}
                        text="Try fewer words, or loosen a filter."
                    />
                    <Button
                        mod="hollow"
                        text={expr(s.filters, s.search, (f, q) =>
                            recordOnly(f, q) ? "Show every change" : "Clear search and filters",
                        )}
                        onClick="clearAll"
                    />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={paging.onPage} />
                </div>
            </div>
        </cx>
    );
});
