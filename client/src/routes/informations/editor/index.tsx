import { createFunctionalComponent, expr, falsy, hasValue, truthy } from "cx/ui";
import { Button, Icon, Link, LookupField, Repeater, TextField } from "cx/widgets";

import { externalLink } from "../../../components/externalLink";
import { formFields } from "../../../components/formFields";
import { recordPage } from "../../../components/recordPage";
import $app from "../../../model";
import Controller from "./Controller";
import m, { targetsOf } from "./model";

const r = m.record;
const p = m.$place;
const { editing, flag, label, pick, prose, text } = formFields(
    { draft: r.draft, options: r.options, errors: r.errors, viewing: r.viewing },
    "information",
);

const noPlaces = expr(r.draft.places, (places) => !places?.length);
const savedPlace = expr(p.id, (id) => !!id);
const newPlace = expr(p.id, (id) => !id);

/** Where it is kept: a saved place is kept or struck through, a new one filled in — as a license's volumes. */
const places = (
    <cx>
        <section class="editor-section">
            <div class="editor-section-head">
                <h2 class="editor-section-title" text="Where it is kept" />
                <Button mod="hollow" visible={editing} onClick="addPlace">
                    <Icon name="created" class="size-4" />
                    <span text="Add place" />
                </Button>
            </div>
            <p class="editor-empty" visible={noPlaces} text="Not recorded where it is kept." />
            <p
                class="field-message"
                visible={hasValue((r.errors as any).locations)}
                text={(r.errors as any).locations}
            />
            <div class="volume-list">
                <Repeater records={r.draft.places} recordAlias={p} keyField="key">
                    <div
                        class={{ "volume-row": true, "place-row": true, "place-removed": truthy(p.removed) }}
                        visible={savedPlace}
                    >
                        <div class="place-summary">
                            <div class="place-kind" text={p.label} />
                            <div class="place-target">
                                <Link
                                    visible={hasValue(p.href)}
                                    href={p.href}
                                    url={$app.url}
                                    class="editor-link"
                                    text={p.target}
                                />
                                <span visible={falsy(p.href)} text={p.target} />
                                {externalLink(p.external)}
                            </div>
                        </div>
                        <span
                            class="editor-hint volume-removed-note"
                            visible={truthy(p.removed)}
                            text="Removed when you save."
                        />
                        <Button
                            mod="hollow"
                            class="volume-undo"
                            visible={truthy(p.removed)}
                            onClick={(_e: unknown, { store, controller }: any) =>
                                controller.keepPlace(store.get(p.key))
                            }
                            attrs={{ "aria-label": "Keep place", title: "Keep place" }}
                        >
                            <Icon name="reactivate" class="size-4" />
                            <span class="hidden sm:inline" text="Undo" />
                        </Button>
                        <Button
                            mod="hollow"
                            class="volume-remove"
                            visible={expr(r.viewing, p.removed, (v, removed) => !v && !removed)}
                            onClick={(_e: unknown, { store, controller }: any) =>
                                controller.removePlace(store.get(p.key))
                            }
                            attrs={{ "aria-label": "Remove place", title: "Remove place" }}
                        >
                            <Icon name="delete" class="size-4" />
                        </Button>
                    </div>

                    <div class="volume-row volume-new" visible={newPlace}>
                        <span
                            class="sr-only"
                            id={expr(p.key, (k) => `place-${k}-kind-label`)}
                            text="Kind of place"
                        />
                        <span
                            class="sr-only"
                            id={expr(p.key, (k) => `place-${k}-target-label`)}
                            text="Place"
                        />
                        <div class="place-fields">
                            <LookupField
                                id={expr(p.key, (k) => `place-${k}-kind`)}
                                value={p.kindId}
                                text={p.kindText}
                                options={r.options.kinds}
                                required
                                placeholder="Kind of place"
                                inputAttrs={{ "aria-label": "Kind of place" }}
                            />
                            <LookupField
                                id={expr(p.key, (k) => `place-${k}-target`)}
                                visible={expr(p.kindId, (k) => !!k && k !== "url")}
                                value={p.targetId}
                                text={p.targetText}
                                options={expr(r.options, p.kindId, targetsOf)}
                                required
                                placeholder="Which one"
                                inputAttrs={{ "aria-label": "Place" }}
                            />
                            <TextField
                                visible={expr(p.kindId, (k) => k === "url")}
                                value={p.url}
                                required
                                maxLength={500}
                                placeholder="https://…"
                                inputAttrs={{ "aria-label": "Web address", inputMode: "url" }}
                            />
                        </div>
                        <Button
                            mod="hollow"
                            class="volume-remove"
                            onClick={(_e: unknown, { store, controller }: any) =>
                                controller.removePlace(store.get(p.key))
                            }
                            attrs={{ "aria-label": "Remove place", title: "Remove place" }}
                        >
                            <Icon name="delete" class="size-4" />
                        </Button>
                    </div>
                </Repeater>
            </div>
        </section>
    </cx>
);

/** Its tags: a picker while editing, chips — each a link to its tag — while viewing. */
const tags = (
    <cx>
        <div class="editor-wide">
            {label("Tags", false, "information-tags-label")}
            <LookupField
                id="information-tags"
                visible={editing}
                records={r.draft.tags}
                options={r.options.tags}
                multiple
                placeholder="No tags"
                inputAttrs={{ "aria-label": "Tags" }}
            />
            <div class="editor-chips" visible={r.viewing}>
                <Repeater records={r.draft.tags} recordAlias={m.$tag}>
                    <Link
                        class="editor-chip"
                        href={expr(m.$tag.id, (id) => `~/informations/tags/${id}`)}
                        url={$app.url}
                        text={m.$tag.text}
                    />
                </Repeater>
                <span class="editor-empty" visible={expr(r.draft.tags, (t) => !t?.length)} text="—" />
            </div>
        </div>
    </cx>
);

/** A piece of information's page: what it is and who answers for it, its classification, where it is kept. */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            {recordPage({
                r,
                path: "~/informations",
                back: "Information",
                title: "Basic information",
                fields: (
                    <cx>
                        {text("Name", "name", 200, { required: true, wide: true })}
                        {pick("Type", "type", "types", {
                            required: true,
                            href: (id) => `~/informations/types/${id}`,
                        })}
                        {pick("Assignee", "person", "people", {
                            required: true,
                            href: (id) => `~/company/people/${id}`,
                        })}
                        {text("Author", "author", 200)}
                        {pick("Project", "project", "projects", { href: (id) => `~/company/projects/${id}` })}
                        {flag(
                            "Personal information",
                            "personalInformation",
                            "Holds personal information",
                            "No personal information",
                        )}
                        {flag(
                            "Clients' personal information",
                            "clientsPersonalInformation",
                            "Holds clients' personal information",
                            "No clients' personal information",
                        )}
                        {pick("Confidentiality", "confidentiality", "confidentialities")}
                        {pick("Integrity", "integrity", "integrities")}
                        {pick("Availability", "availability", "availabilities")}
                        <div>
                            {label("Importance")}
                            <div class="editor-value" text={r.importance} />
                            <div class="editor-hint" visible={editing} text="From the three above." />
                        </div>
                        {flag("Record", "incomplete", "Marked incomplete", "Complete")}
                        {tags}
                        {prose("Access rights", "accessRights", 1000)}
                        {prose("Description", "description", 1000)}
                        {prose("Note", "note", 1000)}
                    </cx>
                ),
                cards: places,
            })}
        </div>
    </cx>
));
