import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";

const s = m.people;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** A count under its header from `md`; a phone's card, which has none, keeps the word. */
const count = (value: AccessorChain<string | undefined>, word: AccessorChain<string>) => (
    <cx>
        <span class={{ "record-meta": true, "record-blank": expr(value, (v) => !v) }}>
            <span text={expr(value, (v) => v ?? "—")} />
            <span class="md:hidden" visible={hasValue(value)} text={expr(word, (w) => ` ${w}`)} />
        </span>
    </cx>
);

/** People: A to Z, each with how much they hold. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide" controller={Controller}>
                <h1 class="page-header page-title" text="People" />

                <div class="list-bar" onRef={onBarRef}>
                    <div class="list-toolbar">
                        <div class="list-search">
                            <Icon name="search" class="list-search-icon" />
                            <TextField
                                class="list-search-field"
                                value={s.search}
                                placeholder="Search name or email…"
                                showClear
                                inputAttrs={{ "aria-label": "Search people", enterKeyHint: "search" }}
                            />
                        </div>
                        <LinkButton mod="primary" class="list-new" href="~/directory/people/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New person" />
                            <span class="sr-only sm:hidden" text="New person" />
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
                    <div class="record-head people-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "email", "Email")}
                        {sortHeader(s.sort, "assets", "Assets")}
                        <span text="Seats" />
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row people-columns"
                            href={expr(m.$row.id, (id) => `~/directory/people/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title" text={m.$row.name} />
                            <span class="record-meta" text={m.$row.email} />
                            {count(m.$row.assets, m.$row.assetsWord)}
                            {count(m.$row.seats, m.$row.seatsWord)}
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p class="list-empty-title" text="No people match" />
                    <p class="list-empty-text" text="Try fewer words, or add the person." />
                    <Button mod="hollow" text="Clear search" onClick="clearSearch" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
