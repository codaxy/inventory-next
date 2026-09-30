import { createModel } from "cx/ui";

import type { Holdings } from "../../../../api/people";
import { assetKinds, type HoldingSection, informationKind, toSections } from "../../../../holdings";
import { formatDate } from "../../../../licensing";

/** The form, as the fields bind it: text keys absent until typed. */
export interface PersonDraft {
    name?: string | null;
    email?: string | null;
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
}

export default createModel<Model>();

export const toForm = (draft: PersonDraft) => ({
    name: (draft.name ?? "").trim(),
    email: (draft.email ?? "").trim(),
});

const number = (n: number | null) => (n ? `#${n}` : undefined);
const joined = (...parts: (string | null | undefined)[]) => parts.filter(Boolean).join(" · ") || undefined;

/** Everything attached to a person, as sections; what they hold as a phrase, for a delete. */
export function toHoldings(h: Holdings, person: string) {
    return toSections(
        [
            ...assetKinds(h, "personId", person),
            {
                key: "seats",
                title: "Seats",
                none: "seats",
                one: "seat",
                many: "seats",
                total: h.seats.total,
                count:
                    h.seats.active !== h.seats.total
                        ? joined(
                              h.seats.active ? `${h.seats.active} active` : undefined,
                              `${h.seats.total - h.seats.active} deactivated`,
                          )
                        : undefined,
                rows: h.seats.items.map((s) => ({
                    key: s.id,
                    title: s.software,
                    note: s.device
                        ? joined(`On ${s.device}`, number(s.deviceNumber))
                        : s.quantity > 1
                          ? `${s.quantity} seats`
                          : undefined,
                    meta: s.license,
                    metaNumber: number(s.licenseNumber),
                    href: `~/licenses/activations/${s.id}`,
                    ended: s.deactivationDate ? `Deactivated ${formatDate(s.deactivationDate)}` : undefined,
                })),
                moreHref: `~/licenses/activations?personId=${person}`,
            },
            informationKind(h.information, "personId", person),
            {
                key: "projects",
                title: "Projects",
                none: "projects",
                one: "project",
                many: "projects",
                total: h.projects.total,
                rows: h.projects.items.map((p) => ({
                    key: p.id,
                    title: p.name,
                    meta: p.client ?? undefined,
                    href: `~/company/projects/${p.id}`,
                })),
                moreHref: `~/company/projects?personId=${person}`,
            },
        ],
        "Nothing is attached to them.",
        (kinds) => `They hold no ${kinds}.`,
    );
}
