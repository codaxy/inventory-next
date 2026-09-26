import type { AccessorChain, View } from "cx/data";
import { History } from "cx/ui";

import $app from "./model";

export type AddressValue = string | number | boolean | null | undefined | readonly string[];

/** Each list's last address, so a record's back link returns to the list as it was left. */
const last = new Map<string, string>();

const pathOf = (url: string) => url.split("?")[0];

/** The query of an address: what a list reads its state from. */
export const queryOf = (url: string | null | undefined) =>
    new URLSearchParams((url ?? "").includes("?") ? url!.slice(url!.indexOf("?") + 1) : "");

/**
 * A list's state as a query: empty values and defaults left out, an array as the key repeated, the
 * keys the API's own, so one address serves the screen and the server alike.
 */
export function toQueryString(params: Record<string, AddressValue>): string {
    const search = new URLSearchParams();
    for (const [key, value] of Object.entries(params))
        if (Array.isArray(value)) for (const v of value) search.append(key, v);
        else if (value !== undefined && value !== null && value !== "" && value !== false)
            search.set(key, String(value));
    return search.toString();
}

/**
 * The list's state into the address, replacing the entry rather than adding one — Back leaves the
 * list, it does not step through every filter. The trigger on the address fires inside this call, so a
 * list records `toQueryString(params)` as its own before calling it.
 */
export function writeAddress(store: View, path: string, params: Record<string, AddressValue>): string {
    const query = toQueryString(params);
    const url = query ? `${path}?${query}` : path;
    last.set(path, url);
    if (store.get($app.url) !== url) History.replaceState({}, null, url);
    return query;
}

/** Where a record's back link goes: the list as it was left, or the list itself. */
export const listReturn = (path: string) => last.get(path) ?? path;

/**
 * Calls `apply` with the address's query whenever the address changes to one of this list's that the
 * list did not write itself — a link to the same list, filtered otherwise.
 */
export function followAddress(
    controller: { addTrigger: (name: string, args: AccessorChain<any>[], cb: (url: string) => void) => void },
    path: string,
    written: () => string,
    apply: (query: URLSearchParams) => void,
) {
    controller.addTrigger("address", [$app.url], (url: string) => {
        if (!url || pathOf(url) !== path) return;
        const query = queryOf(url).toString();
        if (query !== written()) apply(queryOf(url));
    });
}

/** A number from the address, or the fallback. */
export function intParam(query: URLSearchParams, key: string, fallback: number) {
    const n = Number(query.get(key));
    return Number.isInteger(n) && n > 0 ? n : fallback;
}

/** One of the allowed values from the address, or null. */
export function oneOf<T extends string>(
    query: URLSearchParams,
    key: string,
    allowed: readonly T[],
): T | null {
    const v = query.get(key);
    return v && (allowed as readonly string[]).includes(v) ? (v as T) : null;
}
