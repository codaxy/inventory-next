import { Controller, History } from "cx/ui";

import { listTags } from "../../../api/electronicDeviceTags";
import { ApiError } from "../../../api/http";
import { pager } from "../../../paging";
import m, { toRows } from "./model";

const pageSize = 25;
const searchDelay = 300;

export default class extends Controller {
    private search: string | null = null;
    private timer?: ReturnType<typeof setTimeout>;
    private request = 0;

    onInit() {
        this.store.delete(m.tags.search);
        this.store.set(m.tags.sort, "name");
        this.store.set(m.tags.page, 1);
        this.store.set(m.tags.rows, []);
        this.store.set(m.tags.total, 0);
        this.store.set(m.tags.loading, false);
        this.store.set(m.tags.loaded, false);
        this.store.delete(m.tags.error);
        this.store.set(m.tags.pager, pager(1, pageSize, 0));
        this.store.set(m.tags.totalText, "");
        this.search = null;

        this.addTrigger("search", [m.tags.search], () => {
            clearTimeout(this.timer);
            this.timer = setTimeout(() => {
                this.search = this.store.get(m.tags.search) ?? null;
                this.goTo(1);
            }, searchDelay);
        });

        this.load();
    }

    onDestroy() {
        clearTimeout(this.timer);
    }

    goTo(page: number, scroll = false) {
        this.store.set(m.tags.page, page);
        this.load();
        if (scroll) window.scrollTo({ top: 0 });
    }

    async load() {
        const request = ++this.request;
        const page = this.store.get(m.tags.page);
        this.store.set(m.tags.loading, true);

        try {
            const result = await listTags({
                q: this.search?.trim() || undefined,
                sort: this.store.get(m.tags.sort),
                page,
                pageSize,
            });
            if (request !== this.request) return;

            const state = pager(page, pageSize, result.total);
            if (result.items.length === 0 && result.total > 0) return this.goTo(state.pageCount);

            this.store.set(m.tags.rows, toRows(result.items));
            this.store.set(m.tags.total, result.total);
            this.store.set(m.tags.pager, state);
            this.store.set(
                m.tags.totalText,
                result.total === 0 ? "No tags" : `${state.summary} ${result.total === 1 ? "tag" : "tags"}`,
            );
            this.store.delete(m.tags.error);
            this.store.set(m.tags.loaded, true);
        } catch (error) {
            if (request !== this.request) return;
            this.store.set(
                m.tags.error,
                error instanceof ApiError ? error.message : "The tags could not be loaded.",
            );
        } finally {
            if (request === this.request) this.store.set(m.tags.loading, false);
        }
    }

    /** A column header: its key ascending, then descending, then back. */
    sortBy(key: "name" | "types") {
        this.store.update(m.tags.sort, (sort) => (sort === key ? `-${key}` : key) as typeof sort);
        this.goTo(1);
    }

    open(id: string) {
        History.pushState({}, null, `~/electronic-devices/tags/${id}`);
    }

    create() {
        History.pushState({}, null, "~/electronic-devices/tags/new");
    }

    clearSearch() {
        this.store.delete(m.tags.search);
    }
}
