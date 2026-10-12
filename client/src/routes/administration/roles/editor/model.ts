import { createModel } from "cx/ui";

import type { Permission } from "../../../../api/auth";
import type { CatalogueEntry, RoleDetail, RoleForm } from "../../../../api/roles";
import type { RecordState } from "../../../../recordController";

/** One permission's tick. */
export interface Slot {
    key: Permission;
    /** "View", "Edit", "Manage". */
    label: string;
    on: boolean;
    /** The reader holds it: nobody grants what they do not hold. */
    grantable: boolean;
    /** The view an edit cannot be held without. */
    implies: Permission | null;
}

/** A section's ticks: its view, and its edit where it has one. */
export interface Grant {
    section: string;
    first: Slot;
    second?: Slot;
}

export interface RoleDraft {
    name?: string | null;
    description?: string | null;
    grants: Grant[];
}

/** What the server says the reader may do to the role, as loaded. */
export interface RoleFlags {
    isBuiltIn: boolean;
    isSystemAdmin: boolean;
    /** Neither System Admin, nor the reader's own, nor one granting more than theirs. */
    changeable: boolean;
    /** Why it is not, or none. */
    why?: string;
}

export interface RoleState extends RecordState<RoleDraft> {
    /** Absent while adding. */
    role?: RoleFlags;
}

export interface Model {
    record: RoleState;
    $route: { id: string };
    $grant: Grant;
}

export default createModel<Model>();

/** The catalogue as one row per section, ticked as `held` says. */
export function grantsOf(catalogue: CatalogueEntry[], held: readonly Permission[]): Grant[] {
    const grants: Grant[] = [];
    for (const entry of catalogue) {
        const slot: Slot = {
            key: entry.key,
            label: entry.name,
            on: held.includes(entry.key),
            grantable: entry.grantable,
            implies: entry.implies,
        };
        const grant = grants.find((g) => g.section === entry.section);
        if (grant) grant.second = slot;
        else grants.push({ section: entry.section, first: slot });
    }
    return grants;
}

/** An edit ticked takes its view with it. */
export function withImplied(grants: Grant[]): Grant[] | null {
    const broken = grants.some((g) => g.second?.on && g.second.implies === g.first.key && !g.first.on);
    return broken
        ? grants.map((g) =>
              g.second?.on && g.second.implies === g.first.key
                  ? { ...g, first: { ...g.first, on: true } }
                  : g,
          )
        : null;
}

/** What a section's row says in view: "View, Edit", or a dash. */
export const summary = (g: Grant | undefined) =>
    [g?.first, g?.second]
        .filter((s) => s?.on)
        .map((s) => s!.label)
        .join(", ") || "—";

export const toDraft = (r: RoleDetail, catalogue: CatalogueEntry[]): RoleDraft => ({
    name: r.name,
    description: r.description,
    grants: grantsOf(catalogue, r.permissions),
});

export const toForm = (d: RoleDraft): RoleForm => ({
    name: (d.name ?? "").trim(),
    description: (d.description ?? "").trim() || null,
    permissions: (d.grants ?? [])
        .flatMap((g) => [g.first, g.second])
        .filter((s): s is Slot => !!s?.on)
        .map((s) => s.key),
});

export function flagsOf(r: RoleDetail): RoleFlags {
    const why = r.isSystemAdmin
        ? "System Admin grants every permission, including ones added later; nothing about it changes."
        : r.isOwn
          ? "You hold this role, so you cannot change it."
          : !r.canManage
            ? "This role grants more than yours, so you cannot change it."
            : undefined;
    return { isBuiltIn: r.isBuiltIn, isSystemAdmin: r.isSystemAdmin, changeable: !why, why };
}
