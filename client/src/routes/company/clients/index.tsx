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
import { copyButton } from "../../../components/copyButton";

const s = m.clients;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** A count under its header from `md`; a phone's card, which has none, keeps the word. */
/** Clients: A to Z, each with its projects. */
export default createFunctionalComponent(() => {
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill" controller={Controller}>
                <div class="list-top">
                    {listHeading({
                        title: "Clients",
                        addHref: "~/company/clients/new",
                        addLabel: "Add client",
                    })}

                    <div class="list-bar">
                        <div class="list-toolbar">
                            <div class="list-search">
                                <Icon name="search" class="list-search-icon" />
                                <TextField
                                    class="list-search-field"
                                    value={s.search}
                                    placeholder="Search clients…"
                                    showClear
                                    inputAttrs={{ "aria-label": "Search clients", enterKeyHint: "search" }}
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
                    <div class="record-head client-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        {sortHeader(s.sort, "projects", "Projects", "record-num")}
                    </div>

                    {listSkeleton({
                        columns: "client-columns",
                        cells: 2,
                        numeric: [1],
                        visible: falsy(s.loaded),
                    })}

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row client-columns"
                            href={expr(m.$row.id, (id) => `~/company/clients/${id}`)}
                            url={$app.url}
                        >
                            <span class="record-title record-copy">
                                <span class="record-copy-line">
                                    <span text={m.$row.name} />
                                    {copyButton(m.$row.name, "name")}
                                </span>
                            </span>
                            {countCell(m.$row.projects, m.$row.projectsWord)}
                        </Link>
                    </Repeater>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No clients match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words, or add the client."
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
