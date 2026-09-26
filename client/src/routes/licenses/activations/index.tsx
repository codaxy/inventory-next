import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LinkButton, LookupField, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import { expiryClass } from "../../../licensing";
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

/** The volumes the chosen licence and software have, so the picker lists the few that can match. */
const volumeOptions = expr(s.volumes, f.licenseId, f.softwareId, (volumes, license, software) =>
    (volumes ?? [])
        .filter((v) => (!license || v.licenseId === license) && (!software || v.softwareId === software))
        .map((v) => ({ id: v.id, text: v.text })),
);

/** A new activation, of the volume the list is filtered to when it is: that choice is already made. */
const newHref = expr(f.volumeId, (id) =>
    id ? `~/licenses/activations/new?volumeId=${id}` : "~/licenses/activations/new",
);

const statuses = [
    { value: null, text: "Any" },
    { value: "active", text: "Active" },
    { value: "deactivated", text: "Deactivated" },
] as const;

const expiries = [
    { value: null, text: "Any" },
    { value: "expired", text: "Expired" },
    { value: "soon", text: "Expires soon" },
    { value: "regular", text: "Current" },
] as const;

/** A segmented switch over one filter: the controller's setter takes the chosen value. */
const segmented = (
    label: string,
    items: readonly { value: string | null; text: string }[],
    value: typeof f.status | typeof f.expiry,
    setter: "setStatus" | "setExpiry",
) => (
    <cx>
        <div class="list-filter list-filter-wide">
            <div class="list-filter-label" text={label} />
            <div class="segmented" role="group" aria-label={label}>
                {items.map((item) => (
                    <cx>
                        <Button
                            mod="hollow"
                            class={{
                                "segmented-item": true,
                                "segmented-item-on": expr(value, (v) => (v ?? null) === item.value),
                            }}
                            text={item.text}
                            onClick={(_e: unknown, { controller }: any) => controller[setter](item.value)}
                        />
                    </cx>
                ))}
            </div>
        </div>
    </cx>
);

/** Activations: seats of a volume given to a person or a device. Newest first; deactivated ones muted. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="Activations" />

                <div class={{ "list-bar": true, "list-bar-static": s.filtersOpen }} onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search software, licences, people, devices…"
                                showClear
                                inputAttrs={{ "aria-label": "Search activations", enterKeyHint: "search" }}
                            />
                        </div>
                        <Button
                            mod="hollow"
                            class={{ "list-filters-toggle": true, "list-filters-toggle-open": s.filtersOpen }}
                            attrs={{ "aria-controls": "activation-filters" }}
                            onClick="toggleFilters"
                        >
                            <Icon name="filters" class="size-4" />
                            <span class="hidden sm:inline" text="Filters" />
                            <span class="list-count" visible={hasChips} text={chipCount} />
                        </Button>
                        <LinkButton mod="primary" class="list-new" href={newHref}>
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="Activate" />
                            <span class="sr-only sm:hidden" text="New activation" />
                        </LinkButton>
                    </div>

                    <div id="activation-filters" class="list-pane" visible={s.filtersOpen}>
                        <div class="list-pane-grid">
                            <div class="list-filter">
                                <div
                                    class="list-filter-label"
                                    id="licenses-activations-software-or-service-label"
                                    text="Software or service"
                                />
                                <LookupField
                                    id="licenses-activations-software-or-service"
                                    value={f.softwareId}
                                    text={f.softwareText}
                                    options={s.software}
                                    placeholder="Any"
                                    inputAttrs={{ "aria-label": "Software or service" }}
                                />
                            </div>
                            <div class="list-filter">
                                <div
                                    class="list-filter-label"
                                    id="licenses-activations-licence-label"
                                    text="Licence"
                                />
                                <LookupField
                                    id="licenses-activations-licence"
                                    value={f.licenseId}
                                    text={f.licenseText}
                                    options={s.licenses}
                                    placeholder="Any licence"
                                    inputAttrs={{ "aria-label": "Licence" }}
                                />
                            </div>
                            <div class="list-filter">
                                <div
                                    class="list-filter-label"
                                    id="licenses-activations-volume-label"
                                    text="Volume"
                                />
                                <LookupField
                                    id="licenses-activations-volume"
                                    value={f.volumeId}
                                    text={f.volumeText}
                                    options={volumeOptions}
                                    placeholder="Any volume"
                                    inputAttrs={{ "aria-label": "Volume" }}
                                />
                            </div>
                            {segmented("Status", statuses, f.status, "setStatus")}
                            {segmented("Licence expiry", expiries, f.expiry, "setExpiry")}
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
                    <div class="record-head activation-columns">
                        {sortHeader(s.sort, "software", "Software")}
                        {sortHeader(s.sort, "license", "Licence")}
                        {sortHeader(s.sort, "assignee", "User or device")}
                        <span text="Seats" />
                        {sortHeader(s.sort, "activated", "Activated")}
                        {sortHeader(s.sort, "deactivated", "Deactivated")}
                        <span text="Licence expiry" />
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class={{
                                "record-row": true,
                                "activation-columns": true,
                                "record-row-ended": m.$row.ended,
                            }}
                            href={expr(m.$row.id, (id) => `~/licenses/activations/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title">
                                <span text={m.$row.software} />
                                <span
                                    class="record-flag record-flag-ended"
                                    visible={m.$row.ended}
                                    text="Deactivated"
                                />
                            </span>
                            <span class="record-meta" text={m.$row.license} />
                            <span class="record-meta">
                                <span text={m.$row.assignee} />
                                <span
                                    class="record-note"
                                    visible={hasValue(m.$row.assigneeNote)}
                                    text={m.$row.assigneeNote}
                                />
                            </span>
                            <span class="record-meta" text={m.$row.seats} />
                            <span class="record-meta" text={m.$row.activated} />
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-blank": expr(m.$row.deactivated, (d) => !d),
                                }}
                            >
                                {/* The column's header says "Deactivated" from `md`; a phone's card has none. */}
                                <span
                                    class="md:hidden"
                                    visible={hasValue(m.$row.deactivated)}
                                    text="Deactivated "
                                />
                                <span text={expr(m.$row.deactivated, (d) => d ?? "—")} />
                            </span>
                            <span
                                class={{
                                    "record-status": true,
                                    "record-blank": expr(m.$row.expiry, (x) => !x),
                                }}
                            >
                                <span
                                    class={expr(m.$row.expiry, m.$row.ended, (x, ended) =>
                                        ended ? "status-tag" : `status-tag ${expiryClass(x)}`,
                                    )}
                                    text={expr(m.$row.expiryText, (t) => t ?? "—")}
                                />
                            </span>
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="No activations match" />
                    <p class="list-empty-text" text="Try fewer words or filters." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
