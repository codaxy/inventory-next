import type { Expiry } from "../../api/activations";
import { getLicenseOptions, type LicenseItem, type LicenseSort, listLicenses } from "../../api/licenses";
import { encodeDate } from "../../dates";
import { type AddressValue, oneOf } from "../../listAddress";
import { ListController } from "../../listController";
import m, { type FilterKey, type Filters, type Row, toChips, toRows } from "./model";

const s = m.list;
const expiries = ["expired", "soon", "regular", "none"] as const;
const keys = ["number", "name", "vendor", "value", "purchased", "expires", "modified"] as const;
const day = /^\d{4}-\d{2}-\d{2}$/;

/** The day after, as `YYYY-MM-DD`: the server's `to` is exclusive, the reader's "to" includes the day. */
function dayAfter(value: string) {
    const [y, mo, d] = value.split("-").map(Number);
    return encodeDate(new Date(y, mo - 1, d + 1));
}

export default class extends ListController<Filters, LicenseItem, Row, LicenseSort, FilterKey> {
    protected readonly s = s;
    protected readonly path = "~/licenses";
    protected readonly defaultSort = "-modified";
    protected readonly sorts = keys.flatMap((k) => [k, `-${k}`] as LicenseSort[]);
    protected readonly nouns = ["licence", "licences", "No licences"] as const;
    protected readonly failure = "The licences could not be loaded.";

    protected fetch({
        filters: f,
        ...q
    }: {
        q?: string;
        sort: LicenseSort;
        page: number;
        pageSize: number;
        filters: Filters;
    }) {
        return listLicenses({
            ...q,
            vendorId: f.vendorId ?? undefined,
            purchasedFrom: f.from ?? undefined,
            purchasedTo: f.to ? dayAfter(f.to) : undefined,
            expiry: f.expiry ?? undefined,
            incomplete: f.incomplete ?? undefined,
        });
    }

    protected toRows = toRows;
    protected toChips = toChips;

    /** The purchase dates travel as the days the reader chose, both included. */
    protected filtersFrom(query: URLSearchParams): Filters {
        const incomplete = query.get("incomplete");
        const from = query.get("purchasedFrom");
        const to = query.get("purchasedTo");
        return {
            vendorId: query.get("vendorId"),
            from: from && day.test(from) ? from : null,
            to: to && day.test(to) ? to : null,
            expiry: oneOf(query, "expiry", expiries) as Expiry | "none" | null,
            incomplete: incomplete === "true" ? true : incomplete === "false" ? false : null,
        };
    }

    protected filtersTo = (f: Filters): Record<string, AddressValue> => ({
        vendorId: f.vendorId,
        purchasedFrom: f.from,
        purchasedTo: f.to,
        expiry: f.expiry,
        incomplete: f.incomplete == null ? undefined : String(f.incomplete),
    });

    protected without(f: Filters, key: FilterKey): Filters {
        if (key === "vendor") return { ...f, vendorId: undefined, vendorText: undefined };
        if (key === "range") return { ...f, from: undefined, to: undefined };
        return { ...f, [key]: undefined };
    }

    protected loadOptions() {
        this.store.set(s.vendors, []);
        getLicenseOptions()
            .then((o) => {
                this.store.set(s.vendors, o.vendors);
                this.store.update(s.filters, (f) => ({
                    ...f,
                    vendorText: f.vendorText ?? o.vendors.find((x) => x.id === f.vendorId)?.text,
                }));
            })
            .catch(() => {});
    }

    /** Dates and numbers newest or largest first, text A to Z; then the other way. */
    sortBy(key: (typeof keys)[number]) {
        this.sortOn(key, key !== "name" && key !== "vendor");
    }

    setExpiry(expiry: Filters["expiry"]) {
        this.store.set(s.filters.expiry, expiry);
    }

    setIncomplete(incomplete: Filters["incomplete"]) {
        this.store.set(s.filters.incomplete, incomplete);
    }
}
