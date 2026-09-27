import type { Expiry } from "./api/activations";
import type { AssetRow, AssetSections, Section } from "./api/assets";
import { expiryText, formatDate } from "./licensing";

/** One thing attached to a record, in the shape every section's rows share. */
export interface HoldingRow {
    key: string;
    title: string;
    /** "#100893", or where a seat is: beside the title, lighter. */
    note?: string;
    /** The second line: a type and a model, a vendor, a license, an owner. */
    meta?: string;
    /** Its record's page. */
    href?: string;
    /** A record that has ended, listed as history: why. */
    ended?: string;
    expiry?: Expiry;
    expiryText?: string;
}

/** A kind of thing attached to a record: its count, its first rows, the way to the rest. */
export interface HoldingSection {
    key: string;
    title: string;
    /** "375", or "2 active · 1 deactivated" for seats. */
    count: string;
    rows: HoldingRow[];
    /** The list filtered to the record, where the section shows only its first rows. */
    moreHref?: string;
    moreText?: string;
    /** Where there are more and no list to open: that the rest are not shown. */
    limitNote?: string;
}

export const plural = (n: number, one: string, many: string) => `${n} ${n === 1 ? one : many}`;

/** "a, b and c", or "a, b or c". */
export const joinAll = (parts: string[], last: "and" | "or") =>
    parts.length < 2
        ? (parts[0] ?? "")
        : `${parts.slice(0, -1).join(", ")} ${last} ${parts[parts.length - 1]}`;

/** A kind of record attached to another, before it is a section: what the page knows of it. */
export interface Kind {
    key: string;
    title: string;
    /** The kind after "No …". */
    none: string;
    one: string;
    many: string;
    total: number;
    rows: HoldingRow[];
    /** The owning list filtered to the record; absent where there is none. */
    moreHref?: string;
    /** In place of the total, as the title's count. */
    count?: string;
    /** How the first rows were chosen, where a section is cut short: "by name" unless said. */
    order?: string;
}

/**
 * The kinds that hold something as sections, "See all" where there are more than the first; the rest
 * named in one line, `missing` saying what they are missing from — "No licenses or information kept
 * here." — or `nothing` when every kind is empty; and what the record holds, as a phrase.
 */
export function toSections(kinds: Kind[], nothing: string, missing: (kinds: string) => string) {
    const sections: HoldingSection[] = [];
    const empty: string[] = [];
    const held: string[] = [];

    for (const kind of kinds) {
        if (kind.total === 0) {
            empty.push(kind.none);
            continue;
        }
        held.push(plural(kind.total, kind.one, kind.many));
        const more = kind.total > kind.rows.length;
        sections.push({
            key: kind.key,
            title: kind.title,
            count: kind.count ?? String(kind.total),
            rows: kind.rows,
            moreHref: more ? kind.moreHref : undefined,
            moreText: more && kind.moreHref ? `See all ${kind.total}` : undefined,
            limitNote:
                more && !kind.moreHref
                    ? `The first ${kind.rows.length} of ${kind.total}, ${kind.order ?? "by name"}.`
                    : undefined,
        });
    }

    return {
        sections,
        none: !sections.length ? nothing : empty.length ? missing(joinAll(empty, "or")) : undefined,
        holds: held.length ? joinAll(held, "and") : undefined,
    };
}

const number = (n: number | null) => (n ? `#${n}` : undefined);
const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;

/** A device, furniture or other asset's row: its name and number, its type and model beneath. */
export const assetRow = (a: AssetRow, href: string): HoldingRow => ({
    key: a.id,
    title: a.name,
    note: number(a.number),
    meta: joined(a.type, a.model),
    href,
});

/** The devices, furniture and licenses attached to a record; "See all" filters each list by `filter`. */
export const assetKinds = (a: AssetSections, filter: string, id: string): Kind[] => [
    {
        key: "devices",
        title: "Electronic devices",
        none: "electronic devices",
        one: "electronic device",
        many: "electronic devices",
        total: a.devices.total,
        rows: a.devices.items.map((d) => assetRow(d, `~/electronic-devices/${d.id}`)),
        moreHref: `~/electronic-devices?${filter}=${id}`,
    },
    {
        key: "furniture",
        title: "Furniture",
        none: "furniture",
        one: "piece of furniture",
        many: "pieces of furniture",
        total: a.furniture.total,
        rows: a.furniture.items.map((f) => assetRow(f, `~/furniture/${f.id}`)),
        moreHref: `~/furniture?${filter}=${id}`,
    },
    {
        key: "licenses",
        title: "Licenses",
        none: "licenses",
        one: "license",
        many: "licenses",
        total: a.licenses.total,
        rows: a.licenses.items.map((l) => ({
            key: l.id,
            title: l.name,
            note: number(l.number),
            meta: l.vendor,
            href: `~/licenses/${l.id}`,
            expiry: l.expiry ?? undefined,
            expiryText: l.expiry ? `${expiryText[l.expiry]} · ${formatDate(l.expirationDate)}` : undefined,
        })),
        moreHref: `~/licenses?${filter}=${id}`,
    },
];

/** Information attached to a record; "See all" filters the information list by `filter`, where it can. */
export const informationKind = (
    s: Section<{ id: string; name: string; type: string | null }>,
    filter?: string,
    id?: string,
): Kind => ({
    key: "information",
    title: "Information",
    none: "information",
    one: "piece of information",
    many: "pieces of information",
    total: s.total,
    rows: s.items.map((i) => ({
        key: i.id,
        title: i.name,
        meta: i.type ?? undefined,
        href: `~/informations/${i.id}`,
    })),
    moreHref: filter && id ? `~/informations?${filter}=${id}` : undefined,
});
