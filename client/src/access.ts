import { expr } from "cx/ui";

import type { Permission } from "./api/auth";
import { destinationOf, landingFor } from "./layout/navigation";
import $app from "./model";

/**
 * What the signed-in reader may do, as `/api/auth/me` told it. The client is told, not trusted: the
 * server checks every request, and these only keep the screens from offering what it would refuse.
 */
export const permissions = $app.session.user.permissions;

/** An address that adds or changes something: a new record, or a record's editing. */
const writes = (url: string) => {
    const path = url.split("?")[0];
    return path.endsWith("/edit") || /\/new$/.test(path);
};

/** Whether the reader may open the address: its item's view, and its edit where the address writes. */
export function mayOpen(url: string | null | undefined, held: readonly Permission[]): boolean {
    const destination = destinationOf(url);
    if (!destination) return true;

    if (!held.includes(destination.view)) return false;
    return !writes(url ?? "") || (!!destination.edit && held.includes(destination.edit));
}

/** Whether the reader may change what the address belongs to. */
export function mayEdit(url: string | null | undefined, held: readonly Permission[] | null | undefined) {
    const edit = destinationOf(url)?.edit;
    return !!edit && !!held?.includes(edit);
}

/** Whether the reader may change what the page they are on belongs to. */
export const canEdit = expr($app.url, permissions, mayEdit);

/** Whether the reader holds `permission`. */
export const can = (permission: Permission) => expr(permissions, (held) => !!held?.includes(permission));

/** The address is one the reader may not open: it shows as not found. */
export const forbidden = expr($app.url, permissions, (url, held) => !mayOpen(url, held ?? []));

/** Where a person signed in as an employee is: their own equipment, and nothing else. */
export const myEquipment = "~/my-equipment";

/** Whether the session is an employee's, not a user's. */
export const isPerson = expr($app.session.user.kind, (kind) => kind === "person");

/** Whether the session is a user's: anything but an employee's. */
export const isUser = expr($app.session.user.kind, (kind) => kind !== "person");

/** Where `~/` and sign-in land: an employee's own equipment, else the first item the reader can see. */
export const landing = expr($app.session.user, (user) =>
    user?.kind === "person" ? myEquipment : landingFor(user?.permissions ?? []),
);
