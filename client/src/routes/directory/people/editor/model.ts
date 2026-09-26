import { createModel } from "cx/ui";

import type { Expiry } from "../../../../api/activations";
import type { Holdings } from "../../../../api/people";
import { expiryText, formatDate } from "../../../../licensing";

/** The form, as the fields bind it: text keys absent until typed. */
export interface PersonDraft {
    name?: string | null;
    email?: string | null;
}

/** One thing attached to the person, in the shape every section's rows share. */
export interface HoldingRow {
    key: string;
    title: string;
    /** "#100893", or where a seat is: beside the title, lighter. */
    note?: string;
    /** The second line: a type and a model, a vendor, a licence. */
    meta?: string;
    /** Its record's page; absent where the kind has no screen yet. */
    href?: string;
    /** A deactivated seat: listed as history. */
    ended?: string;
    expiry?: Expiry;
    expiryText?: string;
}

export interface HoldingSection {
    key: string;
    title: string;
    /** "375", or "2 active · 3 in all" for seats. */
    count: string;
    rows: HoldingRow[];
    /** The list filtered to the person, where the section shows only its first rows. */
    moreHref?: string;
    moreText?: string;
    /** Where there are more and no list to open: that the rest are not shown. */
    limitNote?: string;
}

export interface PersonEditorState {
    /** `null` while creating. */
    id: string | null;
    viewing: boolean;
    title: string;
    draft: PersonDraft;
    sections: HoldingSection[];
    /** "No furniture, information or projects.", absent when every kind has something. */
    none?: string;
    /** For a delete: what they hold, as a phrase, absent when nothing. */
    holds?: string;
    holdingsLoaded: boolean;
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: { name?: string; email?: string };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    person: PersonEditorState;
    $route: { id: string };
    $section: HoldingSection;
    $holding: HoldingRow;
}

export default createModel<Model>();

export const toForm = (draft: PersonDraft) => ({
    name: (draft.name ?? "").trim(),
    email: (draft.email ?? "").trim(),
});

const number = (n: number | null) => (n ? `#${n}` : undefined);
const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;
const plural = (n: number, one: string, many: string) => `${n} ${n === 1 ? one : many}`;

/** "a, b and c", or "a, b or c". */
const list = (parts: string[], last: "and" | "or") =>
    parts.length < 2
        ? (parts[0] ?? "")
        : `${parts.slice(0, -1).join(", ")} ${last} ${parts[parts.length - 1]}`;

interface Kind {
    key: keyof Holdings;
    title: string;
    /** The kind in "No …" and in what a person holds. */
    none: string;
    one: string;
    many: string;
    moreHref?: (person: string) => string;
}

const kinds: Kind[] = [
    {
        key: "devices",
        title: "Electronic devices",
        none: "electronic devices",
        one: "electronic device",
        many: "electronic devices",
        moreHref: (p) => `~/electronic-devices?personId=${p}`,
    },
    {
        key: "furniture",
        title: "Furniture",
        none: "furniture",
        one: "piece of furniture",
        many: "pieces of furniture",
        moreHref: (p) => `~/furniture?personId=${p}`,
    },
    {
        key: "licenses",
        title: "Licences",
        none: "licences",
        one: "licence",
        many: "licences",
        moreHref: (p) => `~/licenses?personId=${p}`,
    },
    {
        key: "seats",
        title: "Seats",
        none: "seats",
        one: "seat",
        many: "seats",
        moreHref: (p) => `~/licenses/activations?personId=${p}`,
    },
    {
        key: "information",
        title: "Information",
        none: "information",
        one: "piece of information",
        many: "pieces of information",
    },
    { key: "projects", title: "Projects", none: "projects", one: "project", many: "projects" },
];

function rowsOf(h: Holdings, key: keyof Holdings): HoldingRow[] {
    switch (key) {
        case "devices":
            return h.devices.items.map((d) => ({
                key: d.id,
                title: d.name,
                note: number(d.number),
                meta: joined(d.type, d.model),
                href: `~/electronic-devices/${d.id}`,
            }));
        case "furniture":
            return h.furniture.items.map((f) => ({
                key: f.id,
                title: f.name,
                note: number(f.number),
                meta: joined(f.type, f.model),
                href: `~/furniture/${f.id}`,
            }));
        case "licenses":
            return h.licenses.items.map((l) => ({
                key: l.id,
                title: l.name,
                note: number(l.number),
                meta: l.vendor,
                href: `~/licenses/${l.id}`,
                expiry: l.expiry ?? undefined,
                expiryText: l.expiry
                    ? `${expiryText[l.expiry]} · ${formatDate(l.expirationDate)}`
                    : undefined,
            }));
        case "seats":
            return h.seats.items.map((s) => ({
                key: s.id,
                title: s.software,
                note: s.device
                    ? joined(`On ${s.device}`, number(s.deviceNumber))
                    : s.quantity > 1
                      ? `${s.quantity} seats`
                      : undefined,
                meta: s.license,
                href: `~/licenses/activations/${s.id}`,
                ended: s.deactivationDate ? `Deactivated ${formatDate(s.deactivationDate)}` : undefined,
            }));
        case "information":
            return h.information.items.map((i) => ({ key: i.id, title: i.name, meta: i.type ?? undefined }));
        case "projects":
            return h.projects.items.map((p) => ({ key: p.id, title: p.name, meta: p.client ?? undefined }));
    }
}

/** The kinds that hold something, as sections; the rest named in one line; what they hold, as a phrase. */
export function toHoldings(h: Holdings, person: string) {
    const sections: HoldingSection[] = [];
    const empty: string[] = [];
    const held: string[] = [];

    for (const kind of kinds) {
        const section = h[kind.key];
        if (section.total === 0) {
            empty.push(kind.none);
            continue;
        }
        held.push(plural(section.total, kind.one, kind.many));
        const more = section.total > section.items.length;
        sections.push({
            key: kind.key,
            title: kind.title,
            count:
                kind.key === "seats" && h.seats.active !== h.seats.total
                    ? `${h.seats.active} active · ${h.seats.total} in all`
                    : String(section.total),
            rows: rowsOf(h, kind.key),
            moreHref: more && kind.moreHref ? kind.moreHref(person) : undefined,
            moreText: more ? `See all ${section.total}` : undefined,
            limitNote:
                more && !kind.moreHref
                    ? `The first ${section.items.length} of ${section.total}, by name.`
                    : undefined,
        });
    }

    return {
        sections,
        none: !sections.length
            ? "Nothing is attached to them."
            : empty.length
              ? `No ${list(empty, "or")}.`
              : undefined,
        holds: held.length ? list(held, "and") : undefined,
    };
}
