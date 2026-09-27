import { createFunctionalComponent, expr, falsy, hasValue, truthy } from "cx/ui";
import { Button, Repeater } from "cx/widgets";

import { holdingSections } from "../../components/holdings";
import Controller from "./Controller";
import m from "./model";

const d = m.dashboard;
const t = m.$tile;

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

            <div class="list-loading" visible={falsy(d.loaded)} text="Loading…" />

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
