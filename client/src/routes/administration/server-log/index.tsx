import { createFunctionalComponent, equal, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";
import { levels } from "./utils";

const s = m.serverLog;

const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);
const newestFirst = equal(s.sort, "-time");
const oldestFirst = equal(s.sort, "time");

/** A message or an exception: text, with the server's markers shown as what they stand for. */
const Text = (pieces: typeof m.$row.message) => (
    <cx>
        <Repeater records={pieces} recordAlias={m.$piece}>
            <span
                class={{ "term-marker": m.$piece.marker, "term-newline": m.$piece.newline }}
                text={m.$piece.text}
            />
        </Repeater>
    </cx>
);

/**
 * The server log: one day's entries in a terminal, the day picked from a strip of the days that have
 * a file. The toolbar is the list convention's.
 */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide server-log" controller={Controller}>
                <h1 class="page-header page-title" text="Server log" />

                <div class="list-bar" onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search messages…"
                                showClear
                                inputAttrs={{ "aria-label": "Search the server log", enterKeyHint: "search" }}
                            />
                        </div>
                    </div>

                    <div class="list-results-head">
                        <span class="list-total" text={s.totalText} />
                        <Button
                            mod="hollow"
                            class="list-sort"
                            onClick="refresh"
                            attrs={{ "aria-label": "Refresh", title: "Refresh" }}
                        >
                            <Icon name="refresh" class={{ "size-3.5": true, "term-spin": s.loading }} />
                        </Button>
                        <Button
                            mod="hollow"
                            class="list-sort"
                            onClick="toggleSort"
                            attrs={{ "aria-label": "Change the order" }}
                        >
                            <Icon name="newestFirst" class="size-3.5" visible={newestFirst} />
                            <Icon name="oldestFirst" class="size-3.5" visible={oldestFirst} />
                            <span
                                class="hidden sm:inline"
                                text={expr(s.sort, (sort) =>
                                    sort === "time" ? "Oldest first" : "Newest first",
                                )}
                            />
                        </Button>
                        <div>
                            <Pager
                                state={s.pager}
                                compact
                                onPage={(page, instance) => instance.controller.goTo(page, true)}
                            />
                        </div>
                    </div>
                </div>

                {/* The day and the level, the only two things the log is narrowed by, both always in view. */}
                <div class="log-controls">
                    <div class="log-days" role="group" aria-label="Day">
                        <Repeater records={s.days} recordAlias={m.$day}>
                            <button
                                type="button"
                                class={{
                                    "log-day": true,
                                    "log-day-on": expr(m.$day.day, s.day, (d, sel) => d === sel),
                                }}
                                text={m.$day.text}
                                onClick={(_e: unknown, { store, controller }: any) =>
                                    controller.selectDay(store.get(m.$day.day))
                                }
                            />
                        </Repeater>
                    </div>
                    <div class="segmented log-levels" role="group" aria-label="Level">
                        {levels.map((l) => (
                            <cx>
                                <Button
                                    mod="hollow"
                                    class={{
                                        "segmented-item": true,
                                        "segmented-item-on": expr(s.level, (v) => (v ?? null) === l.value),
                                    }}
                                    text={l.text}
                                    onClick={(_e: unknown, { store }: any) =>
                                        l.value ? store.set(s.level, l.value) : store.delete(s.level)
                                    }
                                />
                            </cx>
                        ))}
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div class={{ term: true, "term-loading": s.loading }} visible={notEmpty}>
                    <div class="term-bar" aria-hidden="true">
                        <span class="term-dot" />
                        <span class="term-dot" />
                        <span class="term-dot" />
                        <span
                            class="term-title"
                            text={expr(s.day, (d) => `server-${d.replaceAll("-", "")}.log`)}
                        />
                    </div>

                    <div class="term-empty" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <div
                            class={{
                                "term-entry": true,
                                "term-info": equal(m.$row.tone, "info"),
                                "term-warn": equal(m.$row.tone, "warn"),
                                "term-error": equal(m.$row.tone, "error"),
                                "term-debug": equal(m.$row.tone, "debug"),
                                "term-raw": equal(m.$row.tone, "raw"),
                            }}
                        >
                            <span class="term-time" text={m.$row.time} />
                            <span class="term-tag" text={m.$row.tag} />
                            <span class="term-category" text={m.$row.category} title={m.$row.categoryFull} />
                            <span class="term-message">
                                {Text(m.$row.message)}
                                <button
                                    type="button"
                                    class="term-toggle"
                                    visible={hasValue(m.$row.exception)}
                                    onClick={(_e: unknown, { store }: any) => store.toggle(m.$row.expanded)}
                                    text={expr(m.$row.expanded, (open) =>
                                        open ? "hide exception" : "show exception",
                                    )}
                                />
                            </span>
                            <pre class="term-exception" visible={m.$row.expanded}>
                                {Text(m.$row.exception as any)}
                            </pre>
                        </div>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="Nothing logged here" />
                    <p class="list-empty-text" text="Try another day, fewer words, or every level." />
                    <Button mod="hollow" text="Clear search and filters" onClick="clearAll" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager
                        state={s.pager}
                        onPage={(page, instance) => instance.controller.goTo(page, true)}
                    />
                </div>
            </div>
        </cx>
    );
});
