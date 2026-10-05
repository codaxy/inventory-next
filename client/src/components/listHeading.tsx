import type { AccessorChain } from "cx/data";
import type { StringProp } from "cx/ui";
import { Icon, LinkButton, Toast } from "cx/widgets";

import { download, inViewerZone } from "../download";

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
                        {o.exportHref && exportButton(o.exportHref)}
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

/** How long "Ready" stays, and how long a quick download goes without a spinner. */
const doneFor = 2000;
const busyAfter = 200;

const said = {
    busy: "Preparing the spreadsheet…",
    done: "The spreadsheet is ready.",
    failed: "Not downloaded.",
};

/**
 * Excel: fetches the list's spreadsheet and saves it, saying so on the button — "Preparing…" with a
 * spinner once it takes longer than a blink, "Ready" with a check for a moment after, however
 * quick; a failure is a toast saying why. The state lives on the element (`data-state`), as the copy
 * button keeps its own, so no list's model carries it. Disabled while it runs, so a double click
 * fetches once.
 */
function exportButton(href: AccessorChain<string | undefined>) {
    return (
        <cx>
            <button
                type="button"
                class="list-export"
                attrs={{
                    "aria-label": "Download as Excel",
                    title: "Download what the list shows, every page, as Excel",
                }}
                onClick={(e, instance) => {
                    const button = e.currentTarget as HTMLButtonElement;
                    const url = instance.store.get(href);
                    if (!url || button.disabled) return;
                    const status = button.querySelector<HTMLElement>(".list-export-status")!;
                    const show = (state?: keyof typeof said) => {
                        if (state) button.dataset.state = state;
                        else delete button.dataset.state;
                        status.textContent = state ? said[state] : "";
                    };

                    button.disabled = true;
                    const busy = setTimeout(() => show("busy"), busyAfter);
                    download(inViewerZone(url)).then(
                        () => {
                            clearTimeout(busy);
                            show("done");
                            setTimeout(() => {
                                show();
                                button.disabled = false;
                            }, doneFor);
                        },
                        (error: Error) => {
                            clearTimeout(busy);
                            show();
                            status.textContent = said.failed;
                            button.disabled = false;
                            Toast.create({
                                message: `The spreadsheet could not be downloaded: ${error.message}`,
                                timeout: 6000,
                            }).open(instance.store);
                        },
                    );
                }}
            >
                <Icon name="download" class="list-export-idle size-4" />
                <Icon name="working" class="list-export-busy size-4" />
                <Icon name="copied" class="list-export-done size-4" />
                <span class="hidden sm:inline">
                    <span class="list-export-idle" text="Excel" />
                    <span class="list-export-busy" text="Preparing…" />
                    <span class="list-export-done" text="Ready" />
                </span>
                <span class="sr-only list-export-status" attrs={{ role: "status" }} />
            </button>
        </cx>
    );
}
