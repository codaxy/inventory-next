import { Controller } from "cx/ui";

import { ApiError } from "../../api/http";
import { getMyEquipment, getMyEquipmentDetails } from "../../api/myEquipment";
import { formatDate } from "../../licensing";
import m, {
    type Card,
    type Notice,
    type Searchable,
    type Section,
    hello,
    notices,
    noticesShown,
    searchFrom,
    shown,
    sortCards,
    tileTone,
    toCard,
} from "./model";

type Key = Section["key"];

/** The switch is remembered per browser: a convenience, so a failed read or write changes nothing. */
const goneKey = "my-equipment.include-gone";
const remembered = () => {
    try {
        return localStorage.getItem(goneKey) === "1";
    } catch {
        return false;
    }
};

const closedPanel = { open: false, loading: false, facts: [], contracts: [], apps: [] };

export default class extends Controller {
    /** Everything of theirs, held here: the page shows a section's first few, or what the search finds. */
    private all: { key: Key; title: string; cards: Searchable[] }[] = [];
    private expanded = new Set<Key>();
    private notices: Notice[] = [];
    private name = "";
    private opening = 0;
    private onKey = (e: KeyboardEvent) => {
        if (e.key === "Escape" && this.store.get(m.mine.panel.open)) this.close();
    };

    onInit() {
        this.store.set(m.mine, {
            loaded: false,
            notices: [],
            searchable: false,
            includeGone: remembered(),
            goneCount: 0,
            onlyGone: false,
            sections: [],
            nothingFound: false,
            empty: false,
            panel: closedPanel,
        });
        this.addTrigger("search", [m.mine.q], () => this.show());
        this.addTrigger("gone", [m.mine.includeGone], (include) => {
            try {
                localStorage.setItem(goneKey, include ? "1" : "0");
            } catch {
                // Not remembered; it still applies.
            }
            this.show();
        });
        document.addEventListener("keydown", this.onKey);
        this.load();
    }

    onDestroy() {
        document.removeEventListener("keydown", this.onKey);
    }

    async load() {
        try {
            const mine = await getMyEquipment();
            this.name = mine.name;

            this.notices = notices(mine.warnings, mine.devices);
            this.showNotices(false);

            this.all = [
                {
                    key: "devices" as const,
                    title: "Devices",
                    cards: sortCards(mine.devices.map((t) => toCard(t, "device"))),
                },
                {
                    key: "furniture" as const,
                    title: "Furniture",
                    cards: mine.furniture.map((t) => toCard(t, "furniture")),
                },
                {
                    key: "software" as const,
                    title: "License activations",
                    cards: mine.software.map((t) => toCard(t, "software")),
                },
                {
                    key: "licenses" as const,
                    title: "Licenses you hold",
                    cards: mine.licenses.map((t) => toCard(t, "software")),
                },
            ].filter((s) => s.cards.length > 0);
            const total = this.all.reduce((n, s) => n + s.cards.length, 0);
            this.store.set(
                m.mine.goneCount,
                this.all.reduce((n, s) => n + s.cards.filter((c) => c.gone).length, 0),
            );
            this.store.set(m.mine.searchable, total > searchFrom);
            this.store.set(m.mine.empty, total === 0);
            this.show();
            this.store.set(m.mine.loaded, true);
        } catch (error) {
            this.store.set(
                m.mine.error,
                error instanceof ApiError && error.status === 403
                    ? "Nothing is kept under your email address in the register, so there is no equipment of yours to show."
                    : "Your equipment could not be loaded.",
            );
        }
    }

    /** The first few notices, or all of them; "Show fewer" folds them back. */
    showNotices(all: boolean) {
        this.store.set(m.mine.notices, all ? this.notices : this.notices.slice(0, noticesShown));
        const rest = this.notices.length - noticesShown;
        this.store.set(m.mine.noticesAll, all);
        this.store.set(m.mine.moreNotices, rest <= 0 ? undefined : all ? "Show fewer" : `Show ${rest} more`);
    }

    /**
     * "Show all" opens a section, "Show fewer" closes it back to its first few and brings its heading
     * back into view: closed from its foot, the reader would be left far below it.
     */
    toggle(key: Key) {
        if (this.expanded.delete(key)) {
            this.show();
            requestAnimationFrame(() => document.getElementById(`mine-section-${key}`)?.scrollIntoView());
        } else {
            this.expanded.add(key);
            this.show();
        }
    }

    /**
     * The page from what they hold: written off and sold only while the switch shows them — in the
     * sections, their counts and the greeting's — then cut by the search and the sections' lengths.
     */
    private show() {
        const includeGone = this.store.get(m.mine.includeGone);
        const held = this.all.map((s) => ({
            ...s,
            cards: includeGone ? s.cards : s.cards.filter((c) => !c.gone),
        }));
        const said = hello(this.name, Object.fromEntries(held.map((s) => [s.key, s.cards.length])));
        this.store.set(m.mine.initials, said.initials);
        this.store.set(m.mine.greeting, said.greeting);
        this.store.set(m.mine.summary, said.summary);
        const holding = held.reduce((n, s) => n + s.cards.length, 0);
        this.store.set(m.mine.onlyGone, holding === 0 && this.store.get(m.mine.goneCount) > 0);

        const terms = (this.store.get(m.mine.q) ?? "").toLowerCase().split(/\s+/).filter(Boolean);
        const sections: Section[] = [];
        for (const s of held) {
            const found = s.cards.filter((c) => terms.every((t) => c.search.includes(t)));
            if (!found.length) continue;
            const short = found.length <= shown + 3;
            const whole = terms.length > 0 || this.expanded.has(s.key) || short;
            sections.push({
                key: s.key,
                title: s.title,
                count: found.length,
                cards: whole ? found : found.slice(0, shown),
                more: whole ? undefined : `Show all ${found.length}`,
                fewer: terms.length === 0 && this.expanded.has(s.key) && !short,
            });
        }
        this.store.set(m.mine.sections, sections);
        this.store.set(m.mine.nothingFound, terms.length > 0 && sections.length === 0);
    }

    /** A card opens its details beside the page: the codes IT asks for, and whom to call. */
    async open(card: Card) {
        const opening = ++this.opening;
        this.store.set(m.mine.panel, {
            ...closedPanel,
            open: true,
            loading: true,
            kind: card.kind,
            icon: card.icon,
            tone: tileTone(card.tone, card.kind, card.status, card.gone),
            title: card.title,
            subtitle: card.subtitle,
        });
        // Into the panel, for a keyboard and a screen reader; Escape brings them back out.
        requestAnimationFrame(() => document.querySelector<HTMLElement>(".mine-panel-close")?.focus());
        try {
            const details = await getMyEquipmentDetails(card.kind, card.id);
            // A second card opened meanwhile owns the panel now.
            if (opening !== this.opening) return;
            this.store.set(
                m.mine.panel.facts,
                details.facts.map((f) => ({
                    label: f.label,
                    value: f.value,
                    number: f.number ? `#${f.number}` : undefined,
                    copy: f.copy,
                    standing: f.standing ?? undefined,
                })),
            );
            this.store.set(
                m.mine.panel.contracts,
                details.contracts.map((c) =>
                    [
                        c.vendor,
                        c.number ? `contract ${c.number}` : undefined,
                        c.contact,
                        c.phone,
                        c.email,
                        c.ends ? `until ${formatDate(c.ends)}` : undefined,
                    ]
                        .filter(Boolean)
                        .join(" · "),
                ),
            );
            // "YYYY-MM-DD" here, as the dates come: past it, a subscription "expired".
            const today = new Date().toLocaleDateString("sv");
            this.store.set(
                m.mine.panel.apps,
                details.apps.map((a) => ({
                    name: a.name,
                    license: a.licenseNumber ? `${a.license} #${a.licenseNumber}` : a.license,
                    ends: a.ends
                        ? `Subscription ${a.ends < today ? "expired" : "expires"} ${formatDate(a.ends)}`
                        : undefined,
                    expired: !!a.ends && a.ends < today,
                })),
            );
        } catch {
            if (opening === this.opening)
                this.store.set(m.mine.panel.error, "The details could not be loaded.");
        } finally {
            if (opening === this.opening) this.store.set(m.mine.panel.loading, false);
        }
    }

    close() {
        this.store.set(m.mine.panel.open, false);
    }
}
