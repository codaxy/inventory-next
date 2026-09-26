import { createModel } from "cx/ui";

import type { Expiry } from "../../api/activations";
import type { LicenseItem, LicenseSort, Option } from "../../api/licenses";
import { formatDay } from "../../dates";
import { expiryText, formatDate, formatMoney } from "../../licensing";
import type { PagerState } from "../../paging";

export interface Row {
    id: string;
    number: string;
    name: string;
    /** Present only where the record is marked incomplete: a flag, not a column. */
    incomplete?: string;
    vendor: string;
    value: string;
    purchased: string;
    expiry?: Expiry;
    expiryText?: string;
    modified: string;
}

/** A picked option is its id and its text, as a single `LookupField` binds them. */
export interface Filters {
    vendorId?: string | null;
    vendorText?: string;
    from?: string | null;
    to?: string | null;
    expiry?: Expiry | "none" | null;
    incomplete?: boolean | null;
}

export type FilterKey = "vendor" | "range" | "expiry" | "incomplete";

export interface Chip {
    key: FilterKey;
    text: string;
}

export interface ListState {
    search?: string | null;
    filters: Filters;
    filtersOpen: boolean;
    filtersValid: boolean;
    chips: Chip[];
    vendors: Option[];
    sort: LicenseSort;
    page: number;
    rows: Row[];
    total: number;
    loading: boolean;
    loaded: boolean;
    error?: string;
    pager: PagerState;
    totalText: string;
}

export interface Model {
    list: ListState;
    $row: Row;
    $chip: Chip;
}

export default createModel<Model>();

export const toRows = (items: LicenseItem[]): Row[] =>
    items.map((l) => ({
        id: l.id,
        number: l.number ? `#${l.number}` : "—",
        name: l.name,
        incomplete: l.incomplete ? "Incomplete" : undefined,
        vendor: l.vendor,
        value: formatMoney(l.purchaseValue)!,
        purchased: formatDate(l.purchaseDate)!,
        expiry: l.expiry ?? undefined,
        expiryText: l.expiry ? `${expiryText[l.expiry]} · ${formatDate(l.expirationDate)}` : undefined,
        modified: formatDay(new Date(l.lastModified)),
    }));

const expiryFilterText = { ...expiryText, none: "No expiry date" } as const;

export const toChips = (f: Filters): Chip[] => [
    ...(f.vendorId ? [{ key: "vendor" as const, text: `Vendor: ${f.vendorText ?? "…"}` }] : []),
    ...(f.from || f.to
        ? [
              {
                  key: "range" as const,
                  text: `Bought ${f.from ? `from ${formatDate(f.from)}` : ""}${f.from && f.to ? " " : ""}${f.to ? `to ${formatDate(f.to)}` : ""}`,
              },
          ]
        : []),
    ...(f.expiry ? [{ key: "expiry" as const, text: expiryFilterText[f.expiry] }] : []),
    ...(f.incomplete == null
        ? []
        : [{ key: "incomplete" as const, text: f.incomplete ? "Incomplete" : "Complete" }]),
];
