import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";

const s = m.types;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** Furniture types: a searchable list, each row opening the type's page. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="Furniture types" />

                <div class="list-bar" onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search types…"
                                showClear
                                inputAttrs={{ "aria-label": "Search types", enterKeyHint: "search" }}
                            />
                        </div>
                        <LinkButton mod="primary" class="list-new" href="~/furniture/types/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New type" />
                            <span class="sr-only sm:hidden" text="New type" />
                        </LinkButton>
                    </div>

                    <div class="list-results-head">
                        <span class="list-total" text={s.totalText} />
                        <div>
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
                    <div class="record-head furniture-type-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        <span text="Description" />
                        {sortHeader(s.sort, "furniture", "Furniture")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row furniture-type-columns"
                            href={expr(m.$row.id, (id) => `~/furniture/types/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title" text={m.$row.name} />
                            <span
                                class={{
                                    "record-muted": true,
                                    "record-blank": expr(m.$row.description, (d) => !d),
                                }}
                                text={expr(m.$row.description, (d) => d ?? "—")}
                            />
                            <span
                                class={{
                                    "record-meta": true,
                                    "record-blank": expr(m.$row.furniture, (f) => !f),
                                }}
                            >
                                <span text={expr(m.$row.furniture, (f) => f ?? "—")} />
                                {/* The column's header says "Furniture" from `md`; a phone's card has none. */}
                                <span
                                    class="md:hidden"
                                    visible={hasValue(m.$row.furniture)}
                                    text={expr(m.$row.furnitureWord, (w) => ` ${w}`)}
                                />
                            </span>
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="No types match" />
                    <p class="list-empty-text" text="Try fewer words, or make the type." />
                    <Button mod="hollow" text="Clear search" onClick="clearSearch" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
