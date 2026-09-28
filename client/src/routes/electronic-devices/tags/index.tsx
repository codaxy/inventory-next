import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
import { listHeading, listPaging } from "../../../components/listHeading";
import Controller from "./Controller";
import m from "./model";
import { listSkeleton } from "../../../components/listSkeleton";
import { copyButton, copyCell } from "../../../components/copyButton";

const s = m.tags;
const empty = expr(s.loaded, s.total, s.error, (loaded, total, error) => loaded && total === 0 && !error);
const notEmpty = expr(
    s.loaded,
    s.total,
    s.error,
    (loaded, total, error) => !(loaded && total === 0 && !error),
);

/** Electronic device tags: a searchable list, each row opening the tag's editor. */
export default createFunctionalComponent(() => {
    const paging = listPaging();

    return (
        <cx>
            <div class="page-body page-wide list-fill tag-list" controller={Controller}>
                <div class="page-top">
                    {listHeading({
                        title: "Electronic device tags",
                        addHref: "~/electronic-devices/tags/new",
                        addLabel: "Add tag",
                    })}

                    <div class="list-bar">
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
                        </div>
                    </div>
                </div>

                <div class="list-error" visible={hasValue(s.error)}>
                    <span text={s.error} />
                    <Button mod="hollow" text="Try again" onClick="load" />
                </div>

                <div class={{ "record-list": true, "record-list-loading": s.loading }} visible={notEmpty}>
                    <div class="record-head tag-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        <span text="Description" />
                        {sortHeader(s.sort, "types", "Types")}
                    </div>

                    <div class="list-rows" onRef={paging.onRowsRef}>
                        {listSkeleton({ columns: "tag-columns", cells: 3, visible: falsy(s.loaded) })}

                        <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                            <Link
                                class="record-row tag-columns"
                                href={expr(m.$row.id, (id) => `~/electronic-devices/tags/${id}`)}
                                url={$app.url}
                            >
                                <span class="record-title record-copy">
                                    <span class="record-copy-line">
                                        <span text={m.$row.name} />
                                        {copyButton(m.$row.name, "name")}
                                    </span>
                                </span>
                                {copyCell(m.$row.description, "description", "record-muted")}
                                <span
                                    class={{
                                        "record-meta": true,
                                        "record-blank": expr(m.$row.types, (t) => !t),
                                    }}
                                >
                                    <span text={expr(m.$row.types, (t) => t ?? "—")} />
                                    <span
                                        class="record-more"
                                        visible={hasValue(m.$row.more)}
                                        text={m.$row.more}
                                    />
                                </span>
                            </Link>
                        </Repeater>
                    </div>
                </div>

                <div class="list-empty" visible={empty}>
                    <Icon name="search" class="size-6" />
                    <p
                        class="list-empty-title"
                        text={expr(s.idSearch, (id) => (id ? "No record has this id" : "No tags match"))}
                    />
                    <p
                        class="list-empty-text"
                        visible={falsy(s.idSearch)}
                        text="Try fewer words, or make the tag."
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
