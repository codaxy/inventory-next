import { createFunctionalComponent, expr, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LinkButton, Repeater, ValidationGroup } from "cx/widgets";

import { formFields } from "../../../../components/formFields";
import { moreActions } from "../../../../components/moreActions";
import { expiryClass } from "../../../../licensing";
import { listReturn } from "../../../../listAddress";
import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const p = m.person;
const h = m.$holding;
const { editing, text } = formFields({ draft: p.draft, errors: p.errors, viewing: p.viewing }, "person");

/** A row's content, whether it links to its record or not. */
const holding = () => (
    <cx>
        <div class="holding-main">
            <span class="holding-title" text={h.title} />
            <span class="record-note" visible={hasValue(h.note)} text={h.note} />
            <span class="record-flag record-flag-ended" visible={hasValue(h.ended)} text={h.ended} />
        </div>
        <div class="holding-meta" visible={hasValue(h.meta)} text={h.meta} />
        <span
            class={expr(h.expiry, (x) => `holding-status status-tag ${expiryClass(x)}`)}
            visible={hasValue(h.expiryText)}
            text={h.expiryText}
        />
    </cx>
);

/**
 * A person's page: their name and email, then — read-only — everything attached to them, a section
 * per kind with its count and its first rows, and "See all" to the list filtered to them.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header">
                <Link href={listReturn("~/directory/people")} url={$app.url} class="editor-back">
                    <Icon name="previous" class="size-4" />
                    <span text="People" />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text={p.title} />
                    <div class="editor-heading-actions" visible={p.viewing}>
                        <LinkButton
                            mod="primary"
                            href={expr(p.id, (id) => `~/directory/people/${id}/edit`)}
                            attrs={{ "aria-label": "Edit", title: "Edit" }}
                        >
                            <Icon name="edit" class="size-4" />
                            <span class="hidden sm:inline" text="Edit" />
                        </LinkButton>
                        {moreActions([
                            {
                                text: "Handover sheet",
                                icon: "print",
                                href: expr(p.id, (id) => `~/directory/people/${id}/handover`),
                            },
                            { text: "Delete", icon: "delete", onClick: "remove", danger: true },
                        ])}
                    </div>
                </div>
            </div>

            <div class="editor">
                <div class="editor-alert" visible={hasValue(p.error)}>
                    <span text={p.error} />
                </div>

                <ValidationGroup valid={p.valid} visited={p.visited} viewMode={p.viewing}>
                    <section class="editor-section">
                        <div class="editor-grid">
                            {text("Name", "name", 200, { required: true })}
                            {text("Email", "email", 200, { required: true })}
                        </div>
                        <div class="editor-actions" visible={editing}>
                            <LinkButton
                                mod="hollow"
                                text="Cancel"
                                href={expr(p.id, (id) =>
                                    id ? `~/directory/people/${id}` : listReturn("~/directory/people"),
                                )}
                            />
                            <Button mod="primary" text="Save" onClick="save" disabled={truthy(p.saving)} />
                        </div>
                    </section>
                </ValidationGroup>

                <Repeater records={p.sections} recordAlias={m.$section} keyField="key">
                    <section class="editor-section holding-section" visible={p.viewing}>
                        <div class="editor-section-head">
                            <h2 class="editor-section-title">
                                <span text={m.$section.title} />
                                <span class="holding-count" text={m.$section.count} />
                            </h2>
                        </div>
                        <div class="holding-list">
                            <Repeater records={m.$section.rows} recordAlias={h} keyField="key">
                                <Link
                                    class={{ "holding-row": true, "holding-row-ended": hasValue(h.ended) }}
                                    visible={hasValue(h.href)}
                                    href={h.href}
                                    url={$app.url}
                                >
                                    {holding()}
                                </Link>
                                <div class="holding-row" visible={expr(h.href, (x) => !x)}>
                                    {holding()}
                                </div>
                            </Repeater>
                        </div>
                        <Link
                            class="editor-link holding-more"
                            visible={hasValue(m.$section.moreHref)}
                            href={m.$section.moreHref}
                            url={$app.url}
                            text={m.$section.moreText}
                        />
                        <p
                            class="editor-hint"
                            visible={hasValue(m.$section.limitNote)}
                            text={m.$section.limitNote}
                        />
                    </section>
                </Repeater>

                <div class="holding-footnote" visible={truthy(p.holdingsLoaded)}>
                    <p visible={hasValue(p.none)} text={p.none} />
                    <p text="Virtual machines, clouds and software have no owner, so none are listed here." />
                </div>
            </div>
        </div>
    </cx>
));
