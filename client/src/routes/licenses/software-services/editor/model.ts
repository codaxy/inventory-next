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
    /** How many licence volumes are of it: what keeps it from being deleted. */
    volumeCount: number;
    /** Its licence volumes, each a link to its licence. */
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

/** A licence volume as a row: its licence, its type and description, its seats metered. */
export interface VolumeRow {
    id: string;
    href: string;
    license: string;
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
        ? "No licence volume is of it."
        : n === 1
          ? "1 licence volume is of it."
          : `${n} licence volumes are of it.`;

export const toVolumeRows = (volumes: VolumeLine[]): VolumeRow[] =>
    volumes.map((v) => ({
        id: v.id,
        href: `~/licenses/${v.licenseId}`,
        license: v.licenseNumber ? `${v.license} #${v.licenseNumber}` : v.license,
        detail: v.description ? `${v.type} · ${v.description}` : v.type,
        seats: `${v.inUse} / ${v.quantity}`,
        fill: v.quantity > 0 ? Math.round((v.inUse / v.quantity) * 100) : 0,
        load: v.inUse > v.quantity ? "over" : v.inUse === v.quantity ? "full" : undefined,
    }));
