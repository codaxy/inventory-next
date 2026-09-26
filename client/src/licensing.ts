import type { Expiry } from "./api/activations";
import { formatDay } from "./dates";

/** A calendar date as the server sends it, `YYYY-MM-DD`, shown as "15 Jan 2026"; read as local, never as UTC. */
export function formatDate(date: string | null | undefined): string | undefined {
    if (!date) return undefined;
    const [y, m, d] = date.split("-").map(Number);
    return formatDay(new Date(y, m - 1, d));
}

const money = new Intl.NumberFormat("en-GB", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** A purchase value; the original kept every one in convertible marks. */
export const formatMoney = (value: number | null | undefined) =>
    value == null ? undefined : `${money.format(value)} BAM`;

export const expiryText: Record<Expiry, string> = {
    expired: "Expired",
    soon: "Expires soon",
    regular: "Current",
};

/** The status tag's modifier: red, amber, green. */
export const expiryClass = (expiry: Expiry | null | undefined) => (expiry ? `status-tag-${expiry}` : "");

/** "Expired 3 Mar 2026", "Expires 15 Jan 2027"; nothing without a date. */
export function expiryLabel(expiry: Expiry | null | undefined, date: string | null | undefined) {
    const day = formatDate(date);
    if (!day) return undefined;
    return expiry === "expired" ? `Expired ${day}` : `Expires ${day}`;
}
