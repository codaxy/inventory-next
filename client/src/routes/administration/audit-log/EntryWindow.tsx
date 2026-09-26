import { Controller, createModel, equal, expr, hasValue } from "cx/ui";
import { Button, createHotPromiseWindowFactoryWithProps, Icon, Repeater, Window } from "cx/widgets";

import type { AuditEntry, AuditEntryDetail } from "../../../api/auditLog";
import { formatDateTime } from "../../../dates";
import { historyEntry } from "../../../historyEntry";
import { actionIcon, actionText, describeEntity } from "./model";
import { type FieldRow, type Segment, toFieldRows } from "./utils";

interface Props {
    detail: AuditEntryDetail;
}

/** What the reader asked for on the way out; `false` when they simply closed it. */
export type EntryWindowResult =
    { kind: "history"; entityId: string; label: string } | { kind: "open"; id: string } | false;

interface Related {
    id: string;
    actionText: string;
    actionIcon: string;
    action: string;
    type: string;
    label: string;
}

interface WindowModel {
    w: {
        action: string;
        actionText: string;
        actionIcon: string;
        type: string;
        label: string;
        inventoryNumber?: string;
        email: string;
        time: string;
        isUpdate: boolean;
        showAll: boolean;
        changedCount: number;
        unchangedText: string;
        fields: FieldRow[];
        related: Related[];
    };
    $field: FieldRow;
    $segment: Segment;
    $related: Related;
}

const wm = createModel<WindowModel>();

const toRelated = (entry: AuditEntry): Related => ({
    id: entry.id,
    action: entry.action,
    actionText: actionText[entry.action],
    actionIcon: actionIcon[entry.action],
    ...describeEntity(entry),
});

/**
 * What changed, and on request the rest: an update's unchanged fields, the empty ones of a record
 * created or deleted.
 */
const fieldShown = expr(wm.w.showAll, wm.$field.changed, (all, changed) => all || changed);

/** A value's segments; what changed is marked, the rest reads as ordinary text. */
const Value = (side: "before" | "after") => (
    <cx>
        <span
            class="entry-value-empty"
            visible={side === "before" ? wm.$field.beforeEmpty : wm.$field.afterEmpty}
            text="—"
        />
        <Repeater records={side === "before" ? wm.$field.before : wm.$field.after} recordAlias={wm.$segment}>
            <span class={{ [`entry-mark-${side}`]: wm.$segment.mark }} text={wm.$segment.text} />
        </Repeater>
        <span
            class="entry-value-hint"
            visible={hasValue(side === "before" ? wm.$field.beforeHint : wm.$field.afterHint)}
            text={side === "before" ? wm.$field.beforeHint : wm.$field.afterHint}
        />
    </cx>
);

export const showEntryWindow = createHotPromiseWindowFactoryWithProps<Props, EntryWindowResult>(
    { hot: import.meta.hot },
    ({ detail }: Props) =>
        (resolve) => {
            // Closing is the default outcome: Escape, the backdrop and browser Back all resolve
            // `false`. Only the two actions below replace it.
            let result: EntryWindowResult = false;
            const { entry } = detail;
            const entity = describeEntity(entry);
            const fields = toFieldRows(detail);
            const changedCount = fields.filter((f) => f.changed).length;
            const unchanged = fields.length - changedCount;
            const backEntry = historyEntry();

            class WindowController extends Controller {
                onInit() {
                    this.store.init(wm.w, {
                        action: entry.action,
                        actionText: actionText[entry.action],
                        actionIcon: actionIcon[entry.action],
                        ...entity,
                        email: entry.email,
                        time: formatDateTime(new Date(entry.time)),
                        isUpdate: entry.action === "Update",
                        showAll: false,
                        changedCount,
                        unchangedText: `Show ${unchanged} ${entry.action === "Update" ? "unchanged" : "empty"} ${unchanged === 1 ? "field" : "fields"}`,
                        fields,
                        related: detail.related.map(toRelated),
                    });
                }

                history() {
                    result = { kind: "history", entityId: entry.entityId, label: entity.label };
                    (this.instance as any).dismiss();
                }

                open(id: string) {
                    result = { kind: "open", id };
                    (this.instance as any).dismiss();
                }
            }

            return Window.create(
                <cx>
                    <Window
                        class="entry-window"
                        bodyClass="entry-body"
                        controller={WindowController}
                        modal
                        center
                        closable={false}
                        closeOnEscape
                        dismissOnPopState
                        onDestroy={() => backEntry.release(() => resolve(result))}
                    >
                        <div putInto="header" class="entry-header">
                            <div class="min-w-0 flex-1">
                                <div class="entry-kicker">
                                    <span
                                        class={{
                                            "audit-action": true,
                                            "audit-action-create": equal(wm.w.action, "Create"),
                                            "audit-action-update": equal(wm.w.action, "Update"),
                                            "audit-action-delete": equal(wm.w.action, "Delete"),
                                        }}
                                    >
                                        <Icon name={wm.w.actionIcon} class="size-3.5" />
                                        <span text={wm.w.actionText} />
                                    </span>
                                    <span text={wm.w.type} />
                                </div>
                                <h2 class="entry-title">
                                    <span text={wm.w.label} />
                                    <span
                                        class="entry-number"
                                        visible={hasValue(wm.w.inventoryNumber)}
                                        text={wm.w.inventoryNumber}
                                    />
                                </h2>
                                <p class="entry-meta">
                                    <span text={wm.w.email} />
                                    <span aria-hidden="true" text=" · " />
                                    <span text={wm.w.time} />
                                </p>
                            </div>
                            <Button
                                mod="hollow"
                                class="entry-close"
                                dismiss
                                attrs={{ "aria-label": "Close" }}
                            >
                                <Icon name="close" class="size-5" />
                            </Button>
                        </div>

                        <div class="entry-fields-head" visible={wm.w.isUpdate}>
                            <span class="entry-col-before" text="Before" />
                            <span class="entry-col-after" text="After" />
                        </div>

                        <div class="entry-fields">
                            <Repeater records={wm.w.fields} recordAlias={wm.$field}>
                                <div
                                    class={{
                                        "entry-field": true,
                                        "entry-field-changed": wm.$field.changed,
                                        "entry-field-update": wm.w.isUpdate,
                                        "entry-field-whole": expr(
                                            wm.$field.changed,
                                            wm.$field.diffed,
                                            (c, d) => c && !d,
                                        ),
                                    }}
                                    visible={fieldShown}
                                >
                                    <div class="entry-field-name" text={wm.$field.name} />
                                    <div
                                        class="entry-before"
                                        visible={expr(wm.w.action, (a) => a !== "Create")}
                                    >
                                        {Value("before")}
                                    </div>
                                    <div
                                        class="entry-after"
                                        visible={expr(
                                            wm.w.action,
                                            wm.$field.changed,
                                            (a, changed) => a === "Create" || (a === "Update" && changed),
                                        )}
                                    >
                                        {Value("after")}
                                    </div>
                                </div>
                            </Repeater>
                        </div>

                        <div
                            class="entry-toggle"
                            visible={expr(wm.w.fields, (f) => f.some((x) => !x.changed))}
                        >
                            <button
                                type="button"
                                class="entry-toggle-button"
                                onClick={(_e: unknown, { store }: any) => store.toggle(wm.w.showAll)}
                                text={expr(wm.w.showAll, wm.w.unchangedText, (all, text) =>
                                    all ? "Show only what changed" : text,
                                )}
                            />
                        </div>

                        <section class="entry-related" visible={expr(wm.w.related, (r) => r.length > 0)}>
                            <h3 text="Saved together with" />
                            <Repeater records={wm.w.related} recordAlias={wm.$related}>
                                <button
                                    type="button"
                                    class="entry-related-item"
                                    onClick={(_e: unknown, { store, controller }: any) =>
                                        controller.open(store.get(wm.$related.id))
                                    }
                                >
                                    <span
                                        class={{
                                            "audit-action": true,
                                            "audit-action-create": equal(wm.$related.action, "Create"),
                                            "audit-action-update": equal(wm.$related.action, "Update"),
                                            "audit-action-delete": equal(wm.$related.action, "Delete"),
                                        }}
                                    >
                                        <Icon name={wm.$related.actionIcon} class="size-3.5" />
                                        <span text={wm.$related.actionText} />
                                    </span>
                                    <span class="entry-related-text">
                                        <span class="text-ink-muted" text={wm.$related.type} />
                                        <span text={wm.$related.label} />
                                    </span>
                                    <Icon name="next" class="size-4 shrink-0 text-ink-faint" />
                                </button>
                            </Repeater>
                        </section>

                        <div putInto="footer" class="entry-footer">
                            <Button mod="hollow" onClick="history">
                                <Icon name="recordHistory" class="size-4" />
                                <span text="History of this record" />
                            </Button>
                            <Button mod="primary" dismiss text="Close" />
                        </div>
                    </Window>
                </cx>,
            );
        },
);
