import type { AccessorChain } from "cx/data";
import type { BooleanProp } from "cx/ui";
import { Icon, Toast } from "cx/widgets";

import { download } from "../download";

interface DownloadButton {
    /** The file's URL; nothing happens while it is empty. */
    href: AccessorChain<string | undefined>;
    /** What the URL becomes as it is fetched: an export's, named in the viewer's zone. */
    prepare?: (url: string) => string;
    /** "Excel", "PDF": the button's text from `sm`, an icon alone below. */
    text: string;
    /** Its name for a screen reader. */
    label: string;
    /** Its tooltip. */
    title: string;
    /** What it fetches, as its messages name it: "spreadsheet", "PDF". */
    noun: string;
    visible?: BooleanProp;
}

/** How long "Ready" stays, and how long a quick download goes without a spinner. */
const doneFor = 2000;
const busyAfter = 200;

/**
 * A header band's download — a list's Excel, the handover sheet's PDF: fetches the file and saves it,
 * saying so on the button — "Preparing…" with a spinner once it takes longer than a blink, "Ready"
 * with a check for a moment after, however quick; a failure is a toast saying why. The state lives on
 * the element (`data-state`), as the copy button keeps its own, so no screen's model carries it.
 * Disabled while it runs, so a double click fetches once.
 */
export function downloadButton(o: DownloadButton) {
    const said = {
        busy: `Preparing the ${o.noun}…`,
        done: `The ${o.noun} is ready.`,
        failed: "Not downloaded.",
    };

    return (
        <cx>
            <button
                type="button"
                class="header-action"
                visible={o.visible}
                attrs={{ "aria-label": o.label, title: o.title }}
                onClick={(e, instance) => {
                    const button = e.currentTarget as HTMLButtonElement;
                    const url = instance.store.get(o.href);
                    if (!url || button.disabled) return;
                    const status = button.querySelector<HTMLElement>(".download-status")!;
                    const show = (state?: keyof typeof said) => {
                        if (state) button.dataset.state = state;
                        else delete button.dataset.state;
                        status.textContent = state ? said[state] : "";
                    };

                    button.disabled = true;
                    const busy = setTimeout(() => show("busy"), busyAfter);
                    download(o.prepare ? o.prepare(url) : url).then(
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
                                message: `The ${o.noun} could not be downloaded: ${error.message}`,
                                timeout: 6000,
                            }).open(instance.store);
                        },
                    );
                }}
            >
                <Icon name="download" class="download-idle size-4" />
                <Icon name="working" class="download-busy size-4" />
                <Icon name="copied" class="download-done size-4" />
                <span class="hidden sm:inline">
                    <span class="download-idle" text={o.text} />
                    <span class="download-busy" text="Preparing…" />
                    <span class="download-done" text="Ready" />
                </span>
                <span class="sr-only download-status" attrs={{ role: "status" }} />
            </button>
        </cx>
    );
}
