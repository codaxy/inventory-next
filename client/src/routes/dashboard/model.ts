import { createModel } from "cx/ui";

import type { Dashboard } from "../../api/dashboard";
import { type HoldingSection, type Kind, plural, toSections } from "../../holdings";
import { expiryLabel, formatDate } from "../../licensing";

/** One rule, as a tile: its number, what the number counts, and its rows for the card beneath. */
export type Tone = "alert" | "warn";

export interface Tile {
    key: string;
    label: string;
    value: string;
    sub: string;
    /** How its number reads when it holds anything: red, wrong whenever there are any; amber, due soon. */
    tone?: Tone;
    /** Nothing to show: the number is muted and the tile opens nothing. */
    empty: boolean;
    section?: HoldingSection;
}

export interface DashboardState {
    loaded: boolean;
    error?: string;
    tiles: Tile[];
    selected?: string;
    /** The selected tile's rows: none, or one section. */
    detail: HoldingSection[];
}

export interface Model {
    dashboard: DashboardState;
    $tile: Tile;
}

export default createModel<Model>();

const number = (n: number | null) => (n ? `#${n}` : undefined);
const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;
const seats = (inUse: number, quantity: number) => `${inUse} / ${quantity} in use`;

const recordHref = {
    device: (id: string) => `~/electronic-devices/${id}`,
    furniture: (id: string) => `~/furniture/${id}`,
    license: (id: string) => `~/licenses/${id}`,
    information: (id: string) => `~/informations/${id}`,
};
const recordKind = {
    device: "Device",
    furniture: "Furniture",
    license: "License",
    information: "Information",
};

interface Rule {
    kind: Kind;
    label: string;
    tone?: Tone;
    /** The tile's number, where it is not the section's total. */
    value?: number;
    sub: string;
}

function rules(d: Dashboard): Rule[] {
    const withSeats = d.expired.items.filter((l) => l.activeSeats).length;
    const overBy = d.overAllocated.items.reduce((sum, v) => sum + v.inUse - v.quantity, 0);

    return [
        {
            label: "Subscriptions expired",
            sub: withSeats
                ? `${withSeats} with seats still in use`
                : `In the last ${d.subscriptionsExpiredDays} days`,
            kind: {
                key: "expired",
                title: `Subscriptions expired in the last ${d.subscriptionsExpiredDays} days`,
                none: "subscriptions expired lately",
                one: "license",
                many: "licenses",
                total: d.expired.total,
                order: "those with seats in use first",
                rows: d.expired.items.map((l) => ({
                    key: l.id,
                    title: l.name,
                    note: number(l.number),
                    meta: joined(
                        l.vendor,
                        l.activeSeats ? `${plural(l.activeSeats, "seat", "seats")} still in use` : undefined,
                        l.autoRenew ? "Set to renew: its date may not have been updated" : undefined,
                    ),
                    href: `~/licenses/${l.id}`,
                    expiry: "expired",
                    expiryText: expiryLabel("expired", l.expirationDate),
                })),
            },
        },
        {
            label: "Subscriptions expiring",
            tone: "warn",
            sub: `Within ${d.subscriptionsExpiringDays} days`,
            kind: {
                key: "soon",
                title: `Subscriptions expiring within ${d.subscriptionsExpiringDays} days`,
                none: "subscriptions expiring soon",
                one: "license",
                many: "licenses",
                total: d.expiringSoon.total,
                order: "soonest first",
                rows: d.expiringSoon.items.map((l) => ({
                    key: l.id,
                    title: l.name,
                    note: number(l.number),
                    meta: joined(l.vendor, l.autoRenew ? "Set to renew" : undefined),
                    href: `~/licenses/${l.id}`,
                    expiry: "soon",
                    expiryText: expiryLabel("soon", l.expirationDate),
                })),
            },
        },
        {
            label: "Over-allocated",
            tone: "alert" as const,
            sub: overBy
                ? `${plural(overBy, "seat", "seats")} past what was bought`
                : "Seats past what was bought",
            kind: {
                key: "over",
                title: "Over-allocated volumes",
                none: "over-allocated volumes",
                one: "volume",
                many: "volumes",
                total: d.overAllocated.total,
                order: "most over first",
                rows: d.overAllocated.items.map((v) => ({
                    key: v.id,
                    title: v.software,
                    note: `${v.inUse - v.quantity} over`,
                    meta: joined(v.license, seats(v.inUse, v.quantity)),
                    href: `~/licenses/${v.licenseId}`,
                })),
            },
        },
        ...(d.disposedSeats
            ? [
                  {
                      label: "Disposed devices",
                      tone: "alert" as const,
                      sub: "Seats still active on them",
                      kind: {
                          key: "disposed",
                          title: "Seats on devices written off or sold",
                          none: "seats on disposed devices",
                          one: "seat",
                          many: "seats",
                          total: d.disposedSeats.total,
                          rows: d.disposedSeats.items.map((s) => ({
                              key: s.id,
                              title: s.software,
                              note: joined(`On ${s.device}`, number(s.deviceNumber)),
                              meta: s.location,
                              href: `~/licenses/activations/${s.id}`,
                          })),
                      },
                  },
              ]
            : []),
        {
            label: "Warranties ended",
            sub: `In the last ${d.warrantiesEndedDays} days`,
            kind: {
                key: "warrantiesEnded",
                title: `Warranties ended in the last ${d.warrantiesEndedDays} days`,
                none: "warranties ended lately",
                one: "device",
                many: "devices",
                total: d.warrantiesEnded.total,
                order: "latest first",
                rows: d.warrantiesEnded.items.map((w) => ({
                    key: w.id,
                    title: w.name,
                    note: number(w.number),
                    meta: joined(w.holder, `Warranty ended ${formatDate(w.ends)}`),
                    href: `~/electronic-devices/${w.id}`,
                })),
            },
        },
        {
            label: "Warranties ending",
            tone: "warn",
            sub: `Within ${d.warrantiesEndingDays} days`,
            kind: {
                key: "warranties",
                title: `Warranties ending within ${d.warrantiesEndingDays} days`,
                none: "warranties ending",
                one: "device",
                many: "devices",
                total: d.warranties.total,
                order: "soonest first",
                rows: d.warranties.items.map((w) => ({
                    key: w.id,
                    title: w.name,
                    note: number(w.number),
                    meta: joined(w.holder, `Warranty ends ${formatDate(w.ends)}`),
                    href: `~/electronic-devices/${w.id}`,
                })),
            },
        },
        {
            label: "Unused seats",
            value: d.unusedSeats,
            sub: `On ${plural(d.unused.total, "volume", "volumes")}`,
            kind: {
                key: "unused",
                title: "Volumes with unused seats",
                none: "unused seats",
                one: "volume",
                many: "volumes",
                total: d.unused.total,
                order: "most free first",
                rows: d.unused.items.map((v) => ({
                    key: v.id,
                    title: v.software,
                    note: `${v.quantity - v.inUse} free`,
                    meta: joined(v.license, seats(v.inUse, v.quantity)),
                    href: `~/licenses/${v.licenseId}`,
                })),
            },
        },
        {
            label: "Incomplete",
            sub: "Records left unfinished",
            kind: {
                key: "incomplete",
                title: "Incomplete records",
                none: "incomplete records",
                one: "record",
                many: "records",
                total: d.incomplete.total,
                rows: d.incomplete.items.map((r) => ({
                    key: r.id,
                    title: r.name,
                    note: number(r.number),
                    meta: recordKind[r.kind],
                    href: recordHref[r.kind](r.id),
                })),
            },
        },
    ];
}

/** A tile per rule, each holding the section its card shows when chosen. */
export function toDashboard(d: Dashboard): Tile[] {
    const all = rules(d);
    const { sections } = toSections(
        all.map((r) => r.kind),
        "",
        () => "",
    );
    const byKey = new Map(sections.map((s) => [s.key, s]));

    return all.map((r) => ({
        key: r.kind.key,
        label: r.label,
        value: String(r.value ?? r.kind.total),
        sub: r.sub,
        tone: r.tone,
        empty: r.kind.total === 0,
        section: byKey.get(r.kind.key),
    }));
}

/** The tile to open first: the first that has anything. */
export const firstTile = (tiles: Tile[]) => tiles.find((t) => !t.empty)?.key;
