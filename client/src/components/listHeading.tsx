import type { AccessorChain } from "cx/data";
import type { StringProp } from "cx/ui";
import { Icon, LinkButton } from "cx/widgets";

import { inViewerZone } from "../download";
import { downloadButton } from "./downloadButton";

interface ListHeading {
    title: string;
    /** What else the list does, before Excel: the server log's Refresh. */
    actions?: any;
    /** The spreadsheet of what the list selects, for a list that has one. */
    exportHref?: AccessorChain<string | undefined>;
    /** Where Add leads, for a list whose records are added here. */
    addHref?: StringProp;
    /** "Add"; "Activate" for a seat. */
    addText?: string;
    /** "Add vendor": the button's name for a screen reader and its tooltip. */
    addLabel?: string;
}

/**
 * A `list-fill` list's header band: the title, and its actions — Excel and Add — at its far end, icons
 * only on a phone.
 * It opens the list's `list-top` block, the toolbar after it.
 */
export function listHeading(o: ListHeading) {
    return (
        <cx>
            <div class="page-header">
                <div class="list-heading">
                    <h1 class="page-title" text={o.title} />
                    <div class="list-heading-actions">
                        {o.actions}
                        {o.exportHref &&
                            downloadButton({
                                href: o.exportHref,
                                prepare: inViewerZone,
                                text: "Excel",
                                label: "Download as Excel",
                                title: "Download what the list shows, every page, as Excel",
                                noun: "spreadsheet",
                            })}
                        {o.addHref && (
                            <LinkButton
                                mod="primary"
                                class="list-new"
                                attrs={{ "aria-label": o.addLabel, title: o.addLabel }}
                                href={o.addHref}
                            >
                                <Icon name="created" class="size-4" />
                                <span class="hidden sm:inline" text={o.addText ?? "Add"} />
                            </LinkButton>
                        )}
                    </div>
                </div>
            </div>
        </cx>
    );
}

/**
 * Paging for a `list-fill` list: the controller's page, and the card's rows back to their top, since
 * from `md` they scroll inside it. `onRowsRef` goes on the card's `list-rows`, `onPage` on both pagers.
 */
export function listPaging() {
    let rows: HTMLElement | null = null;
    return {
        onRowsRef: (el: HTMLElement | null) => {
            rows = el;
        },
        onPage: (page: number, instance: any) => {
            instance.controller.goTo(page, true);
            rows?.scrollTo({ top: 0 });
        },
    };
}
