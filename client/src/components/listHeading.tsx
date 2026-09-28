import type { Prop, StringProp } from "cx/ui";
import { Icon, LinkButton } from "cx/widgets";

interface ListHeading {
    title: string;
    /** What else the list does, before Excel: the server log's Refresh. */
    actions?: any;
    /** The spreadsheet of what the list selects, for a list that has one. */
    exportHref?: Prop<string | undefined>;
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
                        {o.exportHref && (
                            // A plain anchor: cx's Link would route it inside the app instead of downloading.
                            <a
                                class="list-export"
                                href={o.exportHref}
                                download
                                attrs={{
                                    "aria-label": "Download as Excel",
                                    title: "Download what the list shows, every page, as Excel",
                                }}
                            >
                                <Icon name="download" class="size-4" />
                                <span class="hidden sm:inline" text="Excel" />
                            </a>
                        )}
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
 * from `md` they scroll inside it. `onRowsRef` goes on the `record-list`, `onPage` on both pagers.
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
