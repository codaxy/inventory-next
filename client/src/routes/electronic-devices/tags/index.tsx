import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";

const s = m.tags;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** A column header that sorts, marking the column and direction in force. */
const SortHeader = (key: "name" | "types", text: string) => (
    <cx>
        <button
            type="button"
            class="list-sort-header"
            onClick={(_e: unknown, { controller }: any) => controller.sortBy(key)}
        >
            <span text={text} />
            <span
                class="list-sort-mark"
                text={expr(s.sort, (sort) => (sort === key ? "▲" : sort === `-${key}` ? "▼" : ""))}
            />
        </button>
    </cx>
);

/** Electronic device tags: a searchable list, each row opening the tag's editor. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body tag-list" controller={Controller}>
                <h1 class="page-header page-title" text="Electronic device tags" />

                <div class="list-bar" onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search tags…"
                                showClear
                                inputAttrs={{ "aria-label": "Search tags", enterKeyHint: "search" }}
                            />
                        </div>
                        <Button mod="primary" class="list-new" onClick="create">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New tag" />
                            <span class="sr-only sm:hidden" text="New tag" />
                        </Button>
                    </div>

                    <div class="list-results-head">
                        <span class="list-total" text={s.totalText} />
                        <div visible={expr(s.total, (t) => t > 0)}>
                            <Pager
                                state={s.pager}
                                compact
                                onPage={(page, i) => i.controller.goTo(page, true)}
                            />
                        </div>
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div class={{ "record-list": true, "record-list-loading": s.loading }} visible={notEmpty}>
                    <div class="record-head tag-columns">
                        {SortHeader("name", "Name")}
                        <span text="Description" />
                        {SortHeader("types", "Types")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <button
                            type="button"
                            class="record-row tag-columns"
                            onClick={(_e: unknown, { store, controller }: any) =>
                                controller.open(store.get(m.$row.id))
                            }
                        >
                            <span class="record-title" text={m.$row.name} />
                            <span class="record-muted" text={expr(m.$row.description, (d) => d ?? "—")} />
                            <span class="record-meta" text={m.$row.types} />
                        </button>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="No tags match" />
                    <p class="list-empty-text" text="Try fewer words, or make the tag." />
                    <Button mod="hollow" text="Clear search" onClick="clearSearch" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
