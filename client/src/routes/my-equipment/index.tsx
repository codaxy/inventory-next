import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Icon, Repeater, Switch, TextField } from "cx/widgets";

import { copyButton } from "../../components/copyButton";
import { documentTitle } from "../../components/documentTitle";
import type Controller from "./Controller";
import ControllerClass from "./Controller";
import { glyphOf } from "../../typeIcons";
import m, { tileTone } from "./model";

const d = m.mine;
const s = m.$section;
const c = m.$card;
const p = d.panel;

/**
 * A pill's class by how it stands. An expired warranty is a regular state — the device works on, maybe
 * for years — so it stays quiet, no fill; an expired subscription is not — the software stops — so it
 * is grey.
 */
const standingClass = (status: string | undefined, kind: string | undefined) =>
    status === "ended" && kind === "device" ? "lapsed" : (status ?? "none");
const standing = (status: string | undefined, kind: string | undefined) =>
    `mine-standing mine-standing-${standingClass(status, kind)}`;
/**
 * A date-only pill in words, for the pointer's hover and a screen reader: "Warranty until Aug 2027",
 * "Subscription expires 3 Nov". An expired pill is words already.
 */
const standingWords = (status: string | undefined, kind: string | undefined, text: string | undefined) => {
    if (!text || status === "ended") return text;
    const what = kind === "device" ? "Warranty" : "Subscription";
    return `${what} ${status === "ok" ? "until" : "expires"} ${text}`;
};

/**
 * A warranty's pill carries a shield — covered, expiring, expired — so its icon says "warranty" with
 * the word; a subscription's an alert while it ends, a clock once it has.
 */
const standingIcon = (status: string | undefined, kind: string | undefined) =>
    kind === "device"
        ? status === "ok"
            ? "covered"
            : status === "soon"
              ? "expiring"
              : "expired"
        : status === "soon"
          ? "attention"
          : "recordHistory";

/**
 * An employee's own equipment, made for them rather than for IT: a greeting, what needs attention in
 * plain sentences, and their things as cards by section — what each is, its make and model, an icon of
 * it, and how its warranty or subscription stands. The codes IT asks for wait in a panel a card opens.
 * Someone holding hundreds gets a section's first few and "Show all", and a search.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body mine" controller={ControllerClass}>
            {documentTitle("My equipment")}

            <div class="editor-alert" visible={hasValue(d.error)}>
                <span text={d.error} />
            </div>

            <header class="mine-hero" visible={truthy(d.loaded)}>
                <span class="mine-avatar" text={d.initials} aria-hidden="true" />
                <div>
                    <h1 class="mine-greeting" text={d.greeting} />
                    <p class="mine-summary" visible={hasValue(d.summary)} text={d.summary} />
                </div>
            </header>

            <section
                class="mine-notice"
                visible={expr(d.notices, (n) => n.length > 0)}
                attrs={{ "aria-label": "Ending soon" }}
            >
                <Icon name="attention" class="mine-notice-icon" />
                <div class="mine-notice-body">
                    <ul>
                        <Repeater records={d.notices} recordAlias={m.$notice} keyField="key">
                            <li text={m.$notice.text} />
                        </Repeater>
                    </ul>
                    <button
                        type="button"
                        class="mine-link"
                        visible={hasValue(d.moreNotices)}
                        text={d.moreNotices}
                        onClick={(_e: unknown, { store, controller }: any) =>
                            (controller as Controller).showNotices(!store.get(d.noticesAll))
                        }
                    />
                </div>
            </section>

            <div
                class="mine-tools"
                visible={expr(d.searchable, d.goneCount, (searchable, gone) => !!searchable || gone > 0)}
            >
                <div class="list-search mine-search" visible={truthy(d.searchable)}>
                    <Icon name="search" class="list-search-icon" />
                    <TextField
                        class="list-search-field"
                        value={d.q}
                        placeholder="Find something of yours…"
                        showClear
                        inputAttrs={{ "aria-label": "Search your equipment", enterKeyHint: "search" }}
                    />
                </div>
                {/* Off at first: what sits at a disposal location is theirs on paper only. Said plainly, not by
                    the locations' names — those are the data's, and the cards' pills carry them. */}
                <Switch
                    class="mine-gone-switch"
                    visible={expr(d.goneCount, (n) => n > 0)}
                    value={d.includeGone}
                    text={expr(d.goneCount, (n) => `Include retired (${n})`)}
                />
            </div>

            <Repeater records={d.sections} recordAlias={s} keyField="key">
                <section class="mine-section" attrs={{ id: expr(s.key, (k) => `mine-section-${k}`) }}>
                    <h2 class="mine-section-title">
                        <span text={s.title} />
                        <span class="mine-section-count" text={expr(s.count, String)} />
                        <button
                            type="button"
                            class="mine-link mine-section-fewer"
                            visible={truthy(s.fewer)}
                            text="Show fewer"
                            onClick={(_e: unknown, { store, controller }: any) =>
                                (controller as Controller).toggle(store.get(s.key))
                            }
                        />
                    </h2>
                    <div class="mine-cards">
                        <Repeater records={s.cards} recordAlias={c} keyField="key">
                            <button
                                type="button"
                                class="mine-card"
                                onClick={(_e: unknown, { store, controller }: any) =>
                                    (controller as Controller).open(store.get(c))
                                }
                            >
                                <span class="mine-card-side">
                                    <span
                                        class={expr(
                                            c.tone,
                                            c.kind,
                                            c.status,
                                            c.gone,
                                            (tone, kind, status, gone) =>
                                                `mine-thing mine-thing-${tileTone(tone, kind, status, gone)}`,
                                        )}
                                    >
                                        <Icon name={c.icon} />
                                    </span>
                                    <span
                                        class="mine-card-number"
                                        visible={hasValue(c.number)}
                                        text={c.number}
                                    />
                                </span>
                                <span class="mine-card-body">
                                    <span class="mine-card-title" text={c.title} />
                                    <span
                                        class="mine-card-subtitle"
                                        visible={hasValue(c.subtitle)}
                                        text={c.subtitle}
                                    />
                                    <span class="mine-apps" visible={expr(c.apps, (a) => a.length > 0)}>
                                        <Repeater records={c.apps} recordAlias={m.$app} keyField="key">
                                            <span
                                                class={expr(
                                                    m.$app.status,
                                                    (st) => `mine-app mine-app-${st ?? "ok"}`,
                                                )}
                                                text={m.$app.name}
                                                attrs={{
                                                    title: expr(m.$app.status, m.$app.statusText, (st, t) =>
                                                        standingWords(st, "seat", t),
                                                    ),
                                                }}
                                            />
                                        </Repeater>
                                    </span>
                                    <span
                                        class="mine-standing mine-standing-gone"
                                        visible={hasValue(c.gone)}
                                        text={c.gone}
                                    />
                                    <span
                                        class={expr(c.status, c.kind, standing)}
                                        visible={hasValue(c.statusText)}
                                        attrs={{
                                            title: expr(c.status, c.kind, c.statusText, standingWords),
                                            "aria-label": expr(c.status, c.kind, c.statusText, standingWords),
                                        }}
                                    >
                                        <Icon name={expr(c.status, c.kind, standingIcon)} />
                                        <span text={c.statusText} />
                                    </span>
                                </span>
                            </button>
                        </Repeater>
                    </div>
                    <button
                        type="button"
                        class="mine-show-all"
                        visible={expr(s.more, s.fewer, (more, fewer) => !!more || !!fewer)}
                        text={expr(s.more, (more) => more ?? "Show fewer")}
                        onClick={(_e: unknown, { store, controller }: any) =>
                            (controller as Controller).toggle(store.get(s.key))
                        }
                    />
                </section>
            </Repeater>

            <p class="mine-quiet" visible={truthy(d.nothingFound)} text="Nothing of yours matches that." />
            <p
                class="mine-quiet"
                visible={truthy(d.onlyGone)}
                text="Everything assigned to you is retired. Turn on Include retired to see it."
            />
            <div class="mine-empty" visible={truthy(d.empty)}>
                <span class="mine-thing mine-thing-device">
                    <Icon name="myEquipment" />
                </span>
                <p class="mine-empty-title" text="Nothing is assigned to you yet." />
                <p class="mine-quiet" text="Once IT hands you something, it shows up here." />
            </div>

            {/* The details beside the page, over it on a phone; the backdrop, Escape and × close it. */}
            <div class="mine-backdrop" visible={truthy(p.open)} onClick="close" />
            <aside
                class={{ "mine-panel": true, "mine-panel-open": truthy(p.open) }}
                attrs={{
                    role: "dialog",
                    "aria-modal": "true",
                    "aria-labelledby": "mine-panel-title",
                    "aria-hidden": expr(p.open, (o) => (o ? "false" : "true")),
                }}
            >
                <button
                    type="button"
                    class="mine-panel-close"
                    onClick="close"
                    attrs={{ "aria-label": "Close" }}
                >
                    <Icon name="close" class="size-5" />
                </button>
                <div class="mine-panel-head">
                    <span
                        class={expr(p.tone, (t) => `mine-thing mine-thing-large mine-thing-${t ?? "device"}`)}
                    >
                        <Icon name={expr(p.icon, (i) => i ?? glyphOf("device", null))} />
                    </span>
                    <h2 id="mine-panel-title" class="mine-panel-title" text={p.title} />
                    <p class="mine-card-subtitle" visible={hasValue(p.subtitle)} text={p.subtitle} />
                </div>

                <p class="mine-quiet" visible={truthy(p.loading)} text="Loading…" />
                <p class="field-message" visible={hasValue(p.error)} text={p.error} />

                <div class="mine-panel-part" visible={expr(p.facts, (f) => f.length > 0)}>
                    <h3 class="mine-panel-label" text="For the help desk" />
                    <dl class="mine-facts">
                        <Repeater records={p.facts} recordAlias={m.$fact}>
                            <div class="mine-fact">
                                <dt text={m.$fact.label} />
                                <dd>
                                    {/* A date as the card's pill has it, in full: one place says how it
                                        stands and when. */}
                                    <span
                                        class={expr(m.$fact.standing, p.kind, (st, kind) =>
                                            st ? `mine-fact-date-${standingClass(st, kind)}` : "",
                                        )}
                                        text={m.$fact.value}
                                    />
                                    <span
                                        class="inventory-number"
                                        visible={hasValue(m.$fact.number)}
                                        text={m.$fact.number}
                                    />
                                    <span visible={truthy(m.$fact.copy)} class="mine-copy">
                                        {copyButton(m.$fact.value, "value")}
                                    </span>
                                </dd>
                            </div>
                        </Repeater>
                    </dl>
                </div>

                <div class="mine-panel-part" visible={expr(p.apps, (a) => a.length > 0)}>
                    <h3 class="mine-panel-label" text="Apps on it" />
                    <Repeater records={p.apps} recordAlias={m.$panelApp}>
                        <div
                            class={{
                                "mine-panel-app": true,
                                "mine-panel-app-expired": truthy(m.$panelApp.expired),
                            }}
                        >
                            <span class="mine-thing mine-thing-small mine-thing-software">
                                <Icon name="thingApp" />
                            </span>
                            <div>
                                <p class="mine-panel-app-name" text={m.$panelApp.name} />
                                <p class="mine-quiet" text={m.$panelApp.license} />
                                <p
                                    class="mine-quiet"
                                    visible={hasValue(m.$panelApp.ends)}
                                    text={m.$panelApp.ends}
                                />
                            </div>
                        </div>
                    </Repeater>
                </div>

                <div class="mine-panel-part" visible={expr(p.contracts, (x) => x.length > 0)}>
                    <h3 class="mine-panel-label" text="Who to call" />
                    <Repeater records={p.contracts} recordAlias={m.$contract}>
                        <p class="mine-contract" text={m.$contract} />
                    </Repeater>
                </div>
            </aside>
        </div>
    </cx>
));
