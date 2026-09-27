import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { countCell } from "../../../components/searchList";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { stickyBar } from "../../../stickyBar";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../../components/listSkeleton";

const s = m.people;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** A count under its header from `md`; a phone's card, which has none, keeps the word. */
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
                        <LinkButton
                            mod="primary"
                            class="list-new"
                            attrs={{ "aria-label": "Add person", title: "Add person" }}
                            href="~/company/people/new"
                        >
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="Add" />
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
                        {sortHeader(s.sort, "assets", "Assets", "record-num")}
                        <span class="record-num" text="Seats" />
                    </div>

                    {listSkeleton({
                        columns: "people-columns",
                        cells: 4,
                        numeric: [2, 3],
                        visible: falsy(s.loaded),
                    })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row people-columns"
                            href={expr(m.$row.id, (id) => `~/company/people/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title" text={m.$row.name} />
                            <span class="record-meta" text={m.$row.email} />
                            {countCell(m.$row.assets, m.$row.assetsWord)}
                            {countCell(m.$row.seats, m.$row.seatsWord)}
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No people match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words, or add the person."
                    />
                    <Button mod="hollow" text="Clear search" onClick="clearSearch" />
                </div>

                <div visible={expr(s.total, (t) => t > 0)}>
                    <Pager state={s.pager} onPage={(page, i) => i.controller.goTo(page, true)} />
                </div>
            </div>
        </cx>
    );
});
