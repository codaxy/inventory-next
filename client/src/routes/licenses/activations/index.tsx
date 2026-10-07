import { createFunctionalComponent, expr, falsy, hasValue, isNonEmpty } from "cx/ui";
import { Button, Icon, Link, LookupField, Repeater, TextField } from "cx/widgets";

import { inventoryNumber } from "../../../components/inventoryNumber";
import { Pager } from "../../../components/Pager";
import { volumeText, withNumber } from "../../../inventoryNumbers";
import { sortHeader } from "../../../components/sortHeader";
import { expiryClass } from "../../../licensing";
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

/** The volumes the chosen license and software have, so the picker lists the few that can match. */
const volumeOptions = expr(s.volumes, f.licenseId, f.softwareId, (volumes, license, software) =>
    (volumes ?? [])
        .filter((v) => (!license || v.licenseId === license) && (!software || v.softwareId === software))
        .map((v) => ({ id: v.id, text: volumeText(v) })),
);

/** The licenses with a volume of the chosen software, every one while none is chosen. */
const licenseOptions = expr(s.licenses, s.volumes, f.softwareId, (licenses, volumes, software) =>
    (licenses ?? [])
        .filter(
            (l) =>
                !software || (volumes ?? []).some((v) => v.licenseId === l.id && v.softwareId === software),
        )
        .map((l) => ({ id: l.id, text: withNumber(l.name, l.number) })),
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
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class="page-top">
                    {listHeading({
                        title: "Activations",
                        exportHref: s.exportHref,
                        addHref: newHref,
                        addText: "Activate",
                        addLabel: "Activate a seat",
                    })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search software, licenses, people, devices…"
                                    showClear
                                    inputAttrs={{
                                        "aria-label": "Search activations",
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
                                attrs={{ "aria-controls": "activation-filters" }}
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
                                    <span>
                                        <span text={m.$chip.text} />
                                        {inventoryNumber(m.$chip.number)}
                                        <span visible={hasValue(m.$chip.rest)} text={m.$chip.rest} />
                                    </span>
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
                                id="licenses-activations-license-label"
                                text="License"
                            />
                            <LookupField
                                id="licenses-activations-license"
                                value={f.licenseId}
                                text={f.licenseText}
                                options={licenseOptions}
                                placeholder="Any license"
                                inputAttrs={{ "aria-label": "License" }}
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
                        <div class="list-filter">
                            <div
                                class="list-filter-label"
                                id="licenses-activations-person-label"
                                text="Held by"
                            />
                            <LookupField
                                id="licenses-activations-person"
                                value={f.personId}
                                text={f.personText}
                                options={s.people}
                                placeholder="Anyone"
                                inputAttrs={{ "aria-label": "Held by" }}
                            />
                        </div>
                        {segmented("Status", statuses, f.status, "setStatus")}
                        {segmented("License expiry", expiries, f.expiry, "setExpiry")}
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

                <div class={{ "record-list": true, "record-list-loading": s.loading }} visible={notEmpty}>
                    <div class="record-head activation-columns">
                        {sortHeader(s.sort, "software", "Software")}
                        {sortHeader(s.sort, "license", "License")}
                        {sortHeader(s.sort, "assignee", "Assigned to")}
                        <span class="record-num" text="Seats" />
                        {sortHeader(s.sort, "activated", "Activated")}
                        {sortHeader(s.sort, "deactivated", "Deactivated")}
                        <span text="License expiry" />
                    </div>

                    <div class="list-rows" onRef={paging.onRowsRef}>
                        {listSkeleton({
                            columns: "activation-columns",
                            cells: 7,
                            numeric: [3],
                            visible: falsy(s.loaded),
                        })}

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
                                <span class="record-title record-copy">
                                    <span class="record-copy-line">
                                        <span text={m.$row.software} />
                                        <span
                                            class="record-flag record-flag-ended"
                                            visible={m.$row.ended}
                                            text="Deactivated"
                                        />
                                        {copyButton(m.$row.software, "software")}
                                    </span>
                                </span>
                                {copyCell(m.$row.license, "license")}
                                <span class="record-meta">
                                    <span class="assignee-lines">
                                        <span class="record-copy record-copy-line">
                                            <span>
                                                {/* The column holds people and devices; the menu's mark says which. */}
                                                <span
                                                    class="assignee-kind"
                                                    visible={m.$row.forDevice}
                                                    attrs={{
                                                        role: "img",
                                                        "aria-label": "Device",
                                                        title: "Device",
                                                    }}
                                                >
                                                    <Icon name="electronicDevices" class="size-3" />
                                                </span>
                                                <span
                                                    class="assignee-kind"
                                                    visible={falsy(m.$row.forDevice)}
                                                    attrs={{
                                                        role: "img",
                                                        "aria-label": "User",
                                                        title: "User",
                                                    }}
                                                >
                                                    <Icon name="people" class="size-3.5" />
                                                </span>
                                                <span text={m.$row.assignee} />
                                            </span>
                                            {copyButton(m.$row.assignee, "assignee")}
                                        </span>
                                        <span
                                            class="record-copy activation-device"
                                            visible={m.$row.forDevice}
                                        >
                                            <span class="activation-device-who">
                                                <span
                                                    class="activation-device-number"
                                                    visible={hasValue(m.$row.deviceNumber)}
                                                    text={m.$row.deviceNumber}
                                                />
                                                {copyButton(m.$row.deviceNumberCopy, "inventory number")}
                                                <span
                                                    class="activation-device-holder"
                                                    visible={hasValue(m.$row.deviceHolder)}
                                                    text={m.$row.deviceHolder}
                                                />
                                            </span>
                                            <span
                                                class="activation-device-where"
                                                visible={hasValue(m.$row.deviceLocation)}
                                                text={m.$row.deviceLocation}
                                            />
                                        </span>
                                    </span>
                                </span>
                                <span class="record-meta record-num" text={m.$row.seats} />
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
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) =>
                            id ? "No record has this id" : "No activations match",
                        )}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words or filters."
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
