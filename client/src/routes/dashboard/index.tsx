import { type BooleanProp, createFunctionalComponent, expr, falsy, hasValue, truthy } from "cx/ui";
import { Button, Repeater } from "cx/widgets";

import { holdingSections } from "../../components/holdings";
import Controller from "./Controller";
import m from "./model";

const d = m.dashboard;
const t = m.$tile;

// Label and line widths per tile, fixed so the outline does not reshuffle on every render.
const outline = [
    [72, 56],
    [80, 48],
    [58, 70],
    [66, 60],
    [70, 52],
    [76, 44],
    [60, 50],
    [52, 64],
];

/**
 * The first load: the same card, grid and list card, with bars for the text — eight tiles, as a
 * deployment with disposal locations has — so the counts arrive without anything moving.
 */
const skeleton = (visible: BooleanProp) => (
    <cx>
        <div class="dash-tiles-frame" visible={visible} attrs={{ role: "status" }}>
            <span class="sr-only" text="Loading…" />
            <div class="dash-card" attrs={{ "aria-hidden": "true" }}>
                <div class="dash-card-header">
                    <span class="dash-label" text="Worth a look" />
                </div>
                <div class="dash-tiles" style="--dash-half: 4">
                    {outline.map(([label, line]) => (
                        <cx>
                            <div class="dash-tile dash-tile-skeleton">
                                <span class="dash-label">
                                    <span class="skeleton-bar" style={`width: ${label}%`} />
                                </span>
                                <span class="dash-tile-value">
                                    <span class="skeleton-bar dash-value-bar" />
                                </span>
                                <span class="dash-tile-sub">
                                    <span class="skeleton-bar" style={`width: ${line}%`} />
                                </span>
                            </div>
                        </cx>
                    ))}
                </div>
            </div>
            <div class="dash-detail" attrs={{ "aria-hidden": "true" }}>
                <section class="editor-section holding-section">
                    <div class="editor-section-head">
                        <span class="editor-section-title">
                            <span class="skeleton-bar" style="width: 14rem" />
                        </span>
                    </div>
                    <div class="holding-list">
                        {[46, 60, 38, 52].map((width) => (
                            <cx>
                                <div class="holding-row">
                                    <div class="holding-main">
                                        <span class="skeleton-bar" style={`width: ${width}%`} />
                                    </div>
                                    <div class="holding-meta">
                                        <span class="skeleton-bar" style={`width: ${width - 16}%`} />
                                    </div>
                                </div>
                            </cx>
                        ))}
                    </div>
                </section>
            </div>
        </div>
    </cx>
);

/** A tile per rule, and the chosen tile's rows in one card beneath, so the page is read at a glance. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <h1 class="page-header page-title" text="Dashboard" />
            <p
                class="page-lede"
                text="What has run out, what is about to, and whatever is quietly slipping."
            />

            <div class="editor-alert" visible={hasValue(d.error)}>
                <span text={d.error} />
                <Button mod="hollow" text="Try again" onClick="load" />
            </div>

            {skeleton(falsy(d.loaded))}

            <div
                class="dash-tiles-frame"
                visible={expr(d.loaded, d.error, (loaded, error) => loaded && !error)}
            >
                <div class="dash-card">
                    <div class="dash-card-header">
                        <span class="dash-label" text="Worth a look" />
                    </div>
                    <div
                        class="dash-tiles"
                        style={expr(d.tiles, (tiles) => `--dash-half: ${Math.ceil(tiles.length / 2)}`)}
                    >
                        <Repeater records={d.tiles} recordAlias={t} keyField="key">
                            <button
                                type="button"
                                class={{
                                    "dash-tile": true,
                                    "dash-tile-alert": expr(
                                        t.tone,
                                        t.empty,
                                        (tone, empty) => tone === "alert" && !empty,
                                    ),
                                    "dash-tile-warn": expr(
                                        t.tone,
                                        t.empty,
                                        (tone, empty) => tone === "warn" && !empty,
                                    ),
                                    "dash-tile-empty": truthy(t.empty),
                                    "dash-tile-on": expr(
                                        t.key,
                                        d.selected,
                                        (key, selected) => key === selected,
                                    ),
                                }}
                                disabled={truthy(t.empty)}
                                onClick={(_e: unknown, { store, controller }: any) =>
                                    controller.select(store.get(t.key))
                                }
                            >
                                <span class="dash-label" text={t.label} />
                                <span class="dash-tile-value" text={t.value} />
                                <span class="dash-tile-sub" text={t.sub} />
                            </button>
                        </Repeater>
                    </div>
                </div>

                <div class="dash-detail">{holdingSections(d.detail, d.loaded)}</div>
            </div>
        </div>
    </cx>
));
