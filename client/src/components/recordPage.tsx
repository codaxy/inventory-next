import type { AccessorChain } from "cx/data";
import { expr, falsy, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, Menu, MenuItem, ValidationGroup } from "cx/widgets";

import { listReturn } from "../listAddress";
import $app from "../model";
import type { RecordState } from "../recordController";
import { holdingSections } from "./holdings";
import { historyAction, type MoreAction, moreActions } from "./moreActions";

interface RecordPage {
    r: AccessorChain<RecordState<any>>;
    /** The list, `~/company/vendors`; records are beneath it. */
    path: string;
    /** The back link's text: the list's title. */
    back: string;
    /** The form's fields, in the editor grid. */
    fields: any;
    /** The first card's title, where more cards follow. */
    title?: string;
    /** Further cards of the form; Cancel and Save then sit in a bar of their own below the last. */
    cards?: any;
    /** Actions behind the ⋮ before Delete. */
    more?: MoreAction[];
    /** Duplicate behind the ⋮: `new?from=:id`, which the controller opens as a copy. */
    duplicate?: boolean;
    /** "Save and replicate" behind a chevron on Save, on a new record: a batch entered one by one. */
    another?: boolean;
}

/**
 * A record's page in the house shape: the back link and the name, Edit and a ⋮ holding Delete; one
 * card of fields, Cancel and Save at its foot while editing; then, read-only, what is attached to it.
 * Called inside the screen's component, which holds the `RecordController`.
 */
export function recordPage(o: RecordPage) {
    const r = o.r;
    const commit = (
        <cx>
            <LinkButton
                mod="hollow"
                text="Cancel"
                href={expr(r.id, (id) => (id ? `${o.path}/${id}` : listReturn(o.path)))}
            />
            {o.another ? (
                <cx>
                    {/* A new record saves as Save does; the chevron offers saving it and opening the next like it. */}
                    <div class="save-split" visible={expr(r.id, (id) => !id)}>
                        <Button
                            mod="primary"
                            class="save-split-main"
                            text="Save"
                            onClick="save"
                            disabled={truthy(r.saving)}
                        />
                        <Menu class="save-split-menu">
                            <MenuItem
                                class="save-split-trigger"
                                openOnFocus={false}
                                arrow={false}
                                dropdownOptions={{
                                    placementOrder: "up-left down-left up-right down-right",
                                    offset: 6,
                                    class: "more-dropdown",
                                }}
                            >
                                <Icon name="moreWays" class="size-4" />
                                {/* `MenuItem` passes no attributes through, so its name is text a screen reader reads. */}
                                <span class="sr-only" text="More ways to save" />
                                <Menu putInto="dropdown">
                                    <MenuItem class="more-action" onClick="saveAndAnother" autoClose>
                                        <div class="more-action-body">
                                            <Icon name="duplicate" class="size-4" />
                                            <span text="Save and replicate" />
                                        </div>
                                    </MenuItem>
                                </Menu>
                            </MenuItem>
                        </Menu>
                    </div>
                    <Button
                        mod="primary"
                        visible={hasValue(r.id)}
                        text="Save"
                        onClick="save"
                        disabled={truthy(r.saving)}
                    />
                </cx>
            ) : (
                <cx>
                    <Button mod="primary" text="Save" onClick="save" disabled={truthy(r.saving)} />
                </cx>
            )}
        </cx>
    );
    const actions = (
        <cx>
            <div class="editor-actions" visible={falsy(r.viewing)}>
                {commit}
            </div>
        </cx>
    );
    return (
        <cx>
            <div class="page-header">
                <Link href={listReturn(o.path)} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text={o.back} />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title">
                        <span text={r.title} />
                        <span class="page-title-note" visible={hasValue(r.number)} text={r.number} />
                    </h1>
                    <div class="editor-heading-actions" visible={r.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(r.id, (id) => `${o.path}/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
                        {moreActions([
                            historyAction(r.id),
                            ...(o.duplicate
                                ? [
                                      {
                                          text: "Duplicate",
                                          icon: "duplicate",
                                          href: expr(r.id, (id) => `${o.path}/new?from=${id}`),
                                      } satisfies MoreAction,
                                  ]
                                : []),
                            ...(o.more ?? []),
                            { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                        ])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(r.error)}>
                    <span text={r.error} />
                    <Button mod="hollow" text="Reload" onClick="reload" visible={truthy(r.stale)} />
                </div>

                <ValidationGroup valid={r.valid} visited={r.visited} viewMode={r.viewing}>
                    <section class="editor-section">
                        {o.title ? (
                            <cx>
                                <h2 class="editor-section-title" text={o.title} />
                            </cx>
                        ) : null}
                        <div class="editor-grid">{o.fields}</div>
                        {o.cards ? null : actions}
                    </section>
                    {o.cards ?? null}
                    {o.cards ? (
                        <cx>
                            <div class="editor-actions editor-actions-bar" visible={falsy(r.viewing)}>
                                {commit}
                            </div>
                        </cx>
                    ) : null}
                </ValidationGroup>

                {holdingSections(r.sections, r.viewing)}

                <div class="holding-footnote" visible={expr(r.viewing, r.none, (v, none) => !!v && !!none)}>
                    <p text={r.none} />
                </div>
            </div>
        </cx>
    );
}
