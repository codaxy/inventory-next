import { createModel } from "cx/ui";

import type { Option, VolumeLine } from "../../../../api/softwareServices";

/** The form, as the fields bind it: text keys absent until typed, a pick as its id and text. */
export interface Draft {
    name?: string | null;
    categoryId?: string | null;
    categoryText?: string;
    manufacturerId?: string | null;
    manufacturerText?: string;
    url?: string | null;
}

export interface EditorState {
    /** `null` while creating. */
    id: string | null;
    /** Read-only, as a row opens it; editing is `…/:id/edit`, creating opens in it. */
    viewing: boolean;
    title: string;
    draft: Draft;
    /** How many license volumes are of it: what keeps it from being deleted. */
    volumeCount: number;
    /** Its license volumes, each with a link to its license. */
    volumes: VolumeRow[];
    categories: Option[];
    manufacturers: Option[];
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: { name?: string; categoryId?: string; manufacturerId?: string; url?: string };
    valid: boolean;
    visited: boolean;
}

/** A license volume as a row: its license, its type and description, its seats metered, its links. */
export interface VolumeRow {
    id: string;
    licenseHref: string;
    license: string;
    activationsHref?: string;
    activationsText: string;
    activateHref: string;
    activateText: string;
    detail: string;
    seats: string;
    fill: number;
    load?: "full" | "over";
}

export interface Model {
    entry: EditorState;
    $volume: VolumeRow;
    /** What the enclosing `Route` exposes: `new`, or the entry's id. */
    $route: { id: string };
}

export default createModel<Model>();

export const toForm = (d: Draft) => ({
    name: (d.name ?? "").trim(),
    categoryId: d.categoryId ?? null,
    manufacturerId: d.manufacturerId ?? null,
    url: d.url?.trim() || null,
});

export const volumesText = (n: number) =>
    n === 0
        ? "No license volume is of it."
        : n === 1
          ? "1 license volume is of it."
          : `${n} license volumes are of it.`;

export const toVolumeRows = (volumes: VolumeLine[]): VolumeRow[] =>
    volumes.map((v) => ({
        id: v.id,
        licenseHref: `~/licenses/${v.licenseId}`,
        activationsHref: v.activationCount > 0 ? `~/licenses/activations?volumeId=${v.id}` : undefined,
        activationsText: v.activationCount === 1 ? "1 activation" : `${v.activationCount} activations`,
        activateHref: `~/licenses/activations/new?volumeId=${v.id}&from=software`,
        activateText: v.inUse < v.quantity ? "Activate" : "Over-activate",
        license: v.licenseNumber ? `${v.license} #${v.licenseNumber}` : v.license,
        detail: v.description ? `${v.type} · ${v.description}` : v.type,
        seats: `${v.inUse} / ${v.quantity}`,
        fill: v.quantity > 0 ? Math.round((v.inUse / v.quantity) * 100) : 0,
        load: v.inUse > v.quantity ? "over" : v.inUse === v.quantity ? "full" : undefined,
    }));
