import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, TextField } from "cx/widgets";

import { Pager } from "../../../components/Pager";
import { sortHeader } from "../../../components/sortHeader";
import $app from "../../../model";
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

/** Electronic device tags: a searchable list, each row opening the tag's editor. */
export default createFunctionalComponent(() => {
    const onBarRef = stickyBar();

    return (
        <cx>
            <div class="page-body page-wide tag-list" controller={Controller}>
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
                        <LinkButton mod="primary" class="list-new" href="~/electronic-devices/tags/new">
                            <Icon name="created" class="size-4" />
                            <span class="hidden sm:inline" text="New tag" />
                            <span class="sr-only sm:hidden" text="New tag" />
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
                    <div class="record-head tag-columns">
                        {sortHeader(s.sort, "name", "Name")}
                        <span text="Description" />
                        {sortHeader(s.sort, "types", "Types")}
                    </div>

                    <div class="list-loading" visible={falsy(s.loaded)} text="Loading…" />

                    <Repeater records={s.rows} recordAlias={m.$row} keyField="id">
                        <Link
                            class="record-row tag-columns"
                            href={expr(m.$row.id, (id) => `~/electronic-devices/tags/${id}`)}
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
                            <span class="record-meta">
                                <span text={m.$row.types} />
                                <span
                                    class="record-more"
                                    visible={hasValue(m.$row.more)}
                                    text={m.$row.more}
                                />
                            </span>
                        </Link>
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
