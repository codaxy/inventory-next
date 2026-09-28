import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { countCell } from "../../../components/searchList";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { listHeading, listPaging } from "../../../components/listHeading";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../../components/listSkeleton";
import { copyButton, copyCell } from "../../../components/copyButton";

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
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class="page-top">
                    {listHeading({
                        title: "People",
                        addHref: "~/company/people/new",
                        addLabel: "Add person",
                    })}

                    <div class="list-bar">
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
                        </div>
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div
                    class={{ "record-list": true, "record-list-loading": s.loading }}
                    visible={notEmpty}
                    onRef={paging.onRowsRef}
                >
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
                            <span class="record-title record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.name} />
                                    {copyButton(m.$row.name, "name")}
                                </span>
                            </span>
                            {copyCell(m.$row.email, "email")}
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
                    <Pager state={s.pager} onPage={paging.onPage} />
                </div>
            </div>
        </cx>
    );
});
