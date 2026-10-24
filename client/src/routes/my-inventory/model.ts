import { createModel } from "cx/ui";

import type {
    InventoryThing,
    InventoryWarning,
    MyInventory,
    Standing,
    ThingKind,
} from "../../api/myInventory";
import type { IconName } from "../../layout/icons";
import { formatDate } from "../../licensing";
import { glyphOf } from "../../typeIcons";

/** Each section's own tint, carried by its cards' icons. */
export type Tone = "device" | "furniture" | "software" | "information";

export interface Card {
    key: string;
    kind: ThingKind;
    id: string;
    icon: IconName;
    tone: Tone;
    /** "Written off", "Sold": still theirs on paper, shown last and pale. */
    gone?: string;
    title: string;
    /** "#100631", under the icon: the number IT asks for, and what tells two alike apart. */
    number?: string;
    subtitle?: string;
    place?: string;
    status?: Standing;
    /** When a subscription expired: "3 Nov 2025". */
    expired?: string;
    statusText?: string;
}

export interface Section {
    key: "devices" | "furniture" | "software" | "licenses" | "information";
    title: string;
    count: number;
    cards: Card[];
    /** "Show all 375", while some are held back. */
    more?: string;
    /** Opened past its first few, and so closable again: "Show fewer". */
    fewer: boolean;
}

export interface Fact {
    label: string;
    value: string;
    number?: string;
    standing?: Standing;
}

export interface Panel {
    open: boolean;
    loading: boolean;
    error?: string;
    kind?: ThingKind;
    icon?: IconName;
    /** "expired" for an activation or a license that has, grey as its card's icon. */
    tone?: Tone | "expired";
    title?: string;
    subtitle?: string;
    facts: Fact[];
    contracts: string[];
    /** `expired`: grey, as its pill on the card is. */
    apps: { name: string; license: string; ends?: string; expired: boolean }[];
}

export interface Notice {
    key: string;
    text: string;
}

export interface Model {
    mine: {
        loaded: boolean;
        error?: string;
        initials?: string;
        /** "Hi, Abigail." */
        greeting?: string;
        /** "3 devices · 1 piece of furniture · 1 license activation" */
        summary?: string;
        notices: Notice[];
        /** "Show 4 more", past the first few notices. */
        moreNotices?: string;
        /** Every notice shown. */
        noticesAll?: boolean;
        /** Only for someone holding enough to need it. */
        searchable: boolean;
        /** Whether the written-off and the sold are shown: off unless the reader turns it on. */
        includeGone: boolean;
        /** How many of their things were written off or sold; no switch without any. */
        goneCount: number;
        /** Everything of theirs went, and the switch hides it. */
        onlyGone: boolean;
        q?: string | null;
        sections: Section[];
        /** No card matches the search. */
        nothingFound: boolean;
        empty: boolean;
        panel: Panel;
    };
    $section: Section;
    $card: Card;
    $notice: Notice;
    $fact: Fact;
    $contract: string;
    $panelApp: Panel["apps"][number];
}

export default createModel<Model>();

/** Cards a section shows before "Show all": four rows of three. */
export const shown = 12;

/** Notices shown before "Show more". */
export const noticesShown = 3;

/** Above this many things, a search. */
export const searchFrom = 20;

/** A thing's icon: its type's, as an admin chose it (`typeIcons.ts`); an activation's (the menu's) or a license's. */
function thingIcon(t: InventoryThing): IconName {
    if (t.kind === "license") return "thingLicense";
    if (t.kind === "seat") return "thingActivation";
    if (t.kind === "information") return "thingInformation";
    return glyphOf(t.kind, t.icon);
}

/** Computers and phones first — what someone thinks of as theirs — then as the server ordered them. */
const ranks: Partial<Record<IconName, number>> = {
    thingLaptop: 0,
    thingDesktop: 1,
    thingPhone: 2,
    thingTablet: 3,
};

/**
 * The tint of a card's icon: grey for what no longer works or is no longer theirs to use — an expired
 * activation or license, anything retired — else its section's. A device's expired warranty is no such
 * thing: it works on.
 */
export const tileTone = (tone: Tone, kind: ThingKind, status?: string, gone?: string) =>
    gone || (kind !== "device" && status === "ended") ? "expired" : tone;

export type Searchable = Card & { search: string };

export function toCard(t: InventoryThing, tone: Tone): Searchable {
    return {
        key: `${t.kind}-${t.id}`,
        kind: t.kind,
        id: t.id,
        icon: thingIcon(t),
        tone,
        gone: t.gone ?? undefined,
        title: t.title,
        // A seat's number is its license's, not a thing of theirs.
        number: t.kind !== "seat" && t.number ? `#${t.number}` : undefined,
        subtitle: t.subtitle ?? undefined,
        place: t.place ?? undefined,
        status: t.status ?? undefined,
        expired: t.expired ?? undefined,
        statusText: t.statusText ?? undefined,
        search: [t.title, t.subtitle, t.place, t.name, t.number, t.serial]
            .filter(Boolean)
            .join(" ")
            .toLowerCase(),
    };
}

/** What they have before what went; among each, computers and phones first. */
export const sortCards = (cards: Searchable[]) =>
    [...cards].sort(
        (a, b) => Number(!!a.gone) - Number(!!b.gone) || (ranks[a.icon] ?? 9) - (ranks[b.icon] ?? 9),
    );

const count = (n: number, one: string, many: string) => `${n} ${n === 1 ? one : many}`;

/**
 * The greeting, and what they hold by kind — counted as the page shows it, so the written-off and the
 * sold count only while they are shown.
 */
export function hello(name: string, counts: Partial<Record<Section["key"], number>>) {
    const first = name.split(" ")[0] || name;
    const { devices = 0, furniture = 0, software = 0, licenses = 0, information = 0 } = counts;
    const parts = [
        devices ? count(devices, "device", "devices") : undefined,
        furniture ? count(furniture, "piece of furniture", "pieces of furniture") : undefined,
        software ? count(software, "license activation", "license activations") : undefined,
        licenses ? count(licenses, "license", "licenses") : undefined,
        information ? count(information, "piece of information", "pieces of information") : undefined,
    ].filter(Boolean);
    return {
        initials: name
            .split(/\s+/)
            .filter(Boolean)
            .slice(0, 2)
            .map((w) => w[0]!.toUpperCase())
            .join(""),
        greeting: `Hi, ${first}.`,
        summary: parts.length ? parts.join(" · ") : undefined,
    };
}

/** "Laptop" reads "laptop" mid-sentence; "MFP" or "HUB" stays as it is. */
const lower = (s: string) => (/^[A-Z][a-z]/.test(s) ? s[0]!.toLowerCase() + s.slice(1) : s);

/** "laptops", "switches", "mice": enough English for the kinds of device recorded. */
const plural = (s: string) =>
    /mouse$/i.test(s) ? s.replace(/ouse$/i, "ice") : /(s|x|ch|sh)$/i.test(s) ? `${s}es` : `${s}s`;

/**
 * What ends soon, in plain sentences: "Your laptop's warranty expires on 3 Nov 2026." A device is
 * named by what it is, as on its card; a subscription by the software it serves. Several alike ending
 * the same day are one sentence — "The warranties on 2 of your wireless access points expire on
 * 3 Nov 2026.", "2 Rider subscriptions expire on 8 Feb 2027." — not the same sentence twice. What has
 * ended is no news here: its card says so.
 */
export function notices(warnings: InventoryWarning[], devices: InventoryThing[]): Notice[] {
    const grouped = new Map<string, { w: InventoryWarning; what?: string; n: number }>();
    for (const w of warnings) {
        const device = w.kind === "warranty" ? devices.find((d) => d.id === w.id) : undefined;
        const what =
            w.kind === "subscription" ? (w.uses ?? w.name) : device?.type ? lower(device.title) : undefined;
        const key = what ? `${w.kind}|${what}|${w.date}` : `${w.kind}-${w.id}`;
        const group = grouped.get(key);
        if (group) group.n++;
        else grouped.set(key, { w, what, n: 1 });
    }
    return [...grouped.entries()].map(([key, { w, what, n }]) => {
        const day = formatDate(w.date);
        let text: string;
        if (w.kind === "subscription")
            text =
                n > 1
                    ? `${n} ${what} subscriptions expire on ${day}.`
                    : `${w.uses ? `The ${w.uses} subscription` : `Your ${w.name} subscription`} expires on ${day}.`;
        else if (n > 1) text = `The warranties on ${n} of your ${plural(what!)} expire on ${day}.`;
        else text = `${what ? `Your ${what}'s` : `${w.name}'s`} warranty expires on ${day}.`;
        return { key, text };
    });
}
