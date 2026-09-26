import type { AccessorChain } from "cx/data";
import { expr, falsy, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, ValidationGroup } from "cx/widgets";

import { listReturn } from "../listAddress";
import $app from "../model";
import type { RecordState } from "../recordController";
import { holdingSections } from "./holdings";
import { moreActions } from "./moreActions";

interface RecordPage {
    r: AccessorChain<RecordState<any>>;
    /** The list, `~/company/vendors`; records are beneath it. */
    path: string;
    /** The back link's text: the list's title. */
    back: string;
    /** The form's fields, in the editor grid. */
    fields: any;
}

/**
 * A record's page in the house shape: the back link and the name, Edit and a ⋮ holding Delete; one
 * card of fields, Cancel and Save at its foot while editing; then, read-only, what is attached to it.
 * Called inside the screen's component, which holds the `RecordController`.
 */
export function recordPage(o: RecordPage) {
    const r = o.r;
    return (
        <cx>
            <div class="page-header">
                <Link href={listReturn(o.path)} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text={o.back} />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={r.title} />
                    <div class="editor-heading-actions" visible={r.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(r.id, (id) => `${o.path}/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
                        {moreActions([{ text: "Delete", icon: "delete", onClick: "remove", danger: true }])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(r.error)}>
                    <span text={r.error} />
                </div>

                <ValidationGroup valid={r.valid} visited={r.visited} viewMode={r.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">{o.fields}</div>
                        <div class="editor-actions" visible={falsy(r.viewing)}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(r.id, (id) => (id ? `${o.path}/${id}` : listReturn(o.path)))}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={truthy(r.saving)} />
                        </div>
                    </section>
                </ValidationGroup>

                {holdingSections(r.sections, r.viewing)}

                <div class="holding-footnote" visible={expr(r.viewing, r.none, (v, none) => !!v && !!none)}>
                    <p text={r.none} />
                </div>
            </div>
        </cx>
    );
}
