import { Controller } from "cx/ui";

import type { LogLevel } from "../../../api/serverLog";

import { ApiError } from "../../../api/http";
import { getLogDays, listLogEntries } from "../../../api/serverLog";
import { encodeDate, endOfDay, startOfDay } from "../../../dates";
import { followAddress, intParam, oneOf, queryOf, toQueryString, writeAddress } from "../../../listAddress";
import $app from "../../../model";
import { pager } from "../../../paging";
import m, { toDays, toRows } from "./model";
import { pageSize, searchDelay } from "./utils";

const path = "~/administration/server-log";
const levels = ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"] as const satisfies LogLevel[];

export default class extends Controller {
    /** The search the list reflects; the box runs ahead of it while someone is typing. */
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    /** Only the latest request may write: an older answer arriving late would show stale rows. */
    private request = 0;
    private written = "";
    /** While the address is being applied, the triggers it sets off do not reset the page. */
    private applying = false;

    onInit() {
        const today = encodeDate(new Date());

        this.store.set(m.serverLog.days, toDays([]));
        this.store.set(m.serverLog.rows, []);
        this.store.set(m.serverLog.total, 0);
        this.store.set(m.serverLog.loading, false);
        this.store.set(m.serverLog.loaded, false);
        this.store.delete(m.serverLog.error);
        this.store.set(m.serverLog.pager, pager(1, pageSize, 0));
        this.store.set(m.serverLog.totalText, "");
        this.apply(queryOf(this.store.get($app.url)), today, false);

        this.addTrigger("search", [m.serverLog.search], (value) => {
            if (this.applying || (value ?? null) === this.search) return;
            clearTimeout(this.timer);
            this.timer = setTimeout(() => {
                this.search = this.store.get(m.serverLog.search) ?? null;
                this.goTo(1);
            }, searchDelay);
        });

        this.addTrigger("level", [m.serverLog.level], () => {
            if (!this.applying) this.goTo(1);
        });

        followAddress(
            this,
            path,
            () => this.written,
            (query) => this.apply(query, encodeDate(new Date()), true),
        );

        this.loadDays();
        this.load();
    }

    /**
     * The address's state into the store: the day (today when absent, so a bookmark means "today's
     * log"), the level, the search, the order and the page.
     */
    private apply(query: URLSearchParams, today: string, reload: boolean) {
        this.applying = true;
        const day = query.get("day");
        const q = query.get("q");
        this.search = q;
        if (q) this.store.set(m.serverLog.search, q);
        else this.store.delete(m.serverLog.search);
        this.store.set(m.serverLog.level, oneOf(query, "level", levels));
        this.store.set(m.serverLog.day, day && /^\d{4}-\d{2}-\d{2}$/.test(day) ? day : today);
        this.store.set(m.serverLog.sort, query.get("sort") === "time" ? "time" : "-time");
        this.store.set(m.serverLog.page, intParam(query, "page", 1));
        this.applying = false;
        if (reload) this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    loadDays() {
        getLogDays()
            .then(({ days }) => this.store.set(m.serverLog.days, toDays(days)))
            .catch(() => {});
    }

    goTo(page: number, scroll = false) {
        this.store.set(m.serverLog.page, page);
        this.load();
        if (scroll) window.scrollTo({ top: 0 });
    }

    /** Refresh: the days, which may have gained today's, and the page in view. */
    refresh() {
        this.loadDays();
        this.load();
    }

    selectDay(day: string) {
        this.store.set(m.serverLog.day, day);
        this.goTo(1);
    }

    async load() {
        const request = ++this.request;
        const page = this.store.get(m.serverLog.page);
        const day = this.store.get(m.serverLog.day);

        this.store.set(m.serverLog.loading, true);

        const sort = this.store.get(m.serverLog.sort);
        const address = {
            day: day === encodeDate(new Date()) ? undefined : day,
            level: this.store.get(m.serverLog.level),
            q: this.search?.trim() || undefined,
            sort: sort === "-time" ? undefined : sort,
            page: page > 1 ? page : undefined,
        };
        this.written = toQueryString(address);
        writeAddress(this.store, path, address);

        try {
            const result = await listLogEntries({
                from: startOfDay(day).toISOString(),
                to: endOfDay(day).toISOString(),
                level: this.store.get(m.serverLog.level) ?? undefined,
                q: this.search?.trim() || undefined,
                sort: this.store.get(m.serverLog.sort),
                page,
                pageSize,
            });

            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);

            // The day's log shrank under the reader — a filter changed: its new last page, not an empty one.
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            this.store.set(m.serverLog.rows, toRows(result.items, page));
            this.store.set(m.serverLog.total, result.total);
            this.store.set(m.serverLog.pager, state);
            this.store.set(
                m.serverLog.totalText,
                result.total === 0
                    ? "No entries"
                    : `${state.summary} ${result.total === 1 ? "entry" : "entries"}`,
            );
            this.store.delete(m.serverLog.error);
            this.store.set(m.serverLog.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                m.serverLog.error,
                error instanceof ApiError ? error.message : "The server log could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(m.serverLog.loading, false);
        }
    }

    toggleSort() {
        this.store.update(m.serverLog.sort, (sort) => (sort === "time" ? "-time" : "time"));
        this.goTo(1);
    }

    clearAll() {
        this.store.delete(m.serverLog.search);
        this.store.delete(m.serverLog.level);
    }
}
