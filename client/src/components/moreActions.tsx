import { type AccessorChain, getSelector } from "cx/data";
import { type BooleanProp, expr, type StringProp } from "cx/ui";
import { Icon, Link, Menu, MenuItem } from "cx/widgets";

import type { IconName } from "../layout/icons";
import { can, canEdit } from "../access";
import $app from "../model";

export interface MoreAction {
    text: string;
    icon: IconName;
    /** The screen controller's method to call; a button only acts. */
    onClick?: string;
    /** Where it goes, for an action that navigates: a link, so it takes a middle click. */
    href?: StringProp;
    /** Red and last: it cannot be undone. */
    danger?: boolean;
    /** It changes the record, so only whoever may edit its section is offered it. */
    edits?: boolean;
    visible?: BooleanProp;
}

/**
 * A record's changes: the audit log narrowed to its id. First behind the ⋮ on every record's page. The
 * log holds saves, never who looked, so no activity feed's name; a verb first, like the actions beside it.
 */
export const historyAction = (id: AccessorChain<string | null | undefined>): MoreAction => ({
    text: "View history",
    icon: "history",
    href: expr(id, (id) => `~/administration/audit-log?entityId=${id}`),
    visible: can("audit_log.view"),
});

/** Delete, last and red: offered to whoever may edit. */
export const deleteAction: MoreAction = {
    text: "Delete",
    icon: "delete",
    onClick: "remove",
    danger: true,
    edits: true,
};

/** Whether the reader is offered the action. */
const offered = (action: MoreAction): BooleanProp => (action.edits ? canEdit : (action.visible ?? true));

/** Whether any of the actions is offered: a ⋮ with nothing behind it is not shown at all. */
const anyOffered = (actions: MoreAction[]) => {
    const selectors = actions.map((action) => getSelector(offered(action)));
    return (data: unknown) => selectors.some((offer) => !!offer(data));
};

/**
 * The record's secondary actions behind a ⋮ beside its primary one — Duplicate, Delete — so the header
 * keeps one button and what destroys is a click deeper. A cx `MenuItem` dropdown, kept inline so the
 * keyboard reaches it; a function, not a component, called while the header is built.
 */
export const moreActions = (actions: MoreAction[]) => (
    <cx>
        <Menu class="more-menu" visible={anyOffered(actions)}>
            <MenuItem
                class="more-trigger"
                openOnFocus={false}
                arrow={false}
                dropdownOptions={{
                    placementOrder: "down-left down-right up-left up-right",
                    offset: 6,
                    class: "more-dropdown",
                }}
            >
                <Icon name="more" class="size-5" />
                {/* `MenuItem` passes no attributes through, so its name is text a screen reader reads. */}
                <span class="sr-only" text="More actions" />
                <Menu putInto="dropdown">
                    {actions.map((action) => (
                        <cx>
                            <MenuItem
                                class={{ "more-action": true, "more-action-danger": !!action.danger }}
                                visible={offered(action)}
                                onClick={action.onClick}
                                autoClose
                            >
                                {action.href ? (
                                    <cx>
                                        <Link class="more-action-body" href={action.href} url={$app.url}>
                                            <Icon name={action.icon} class="size-4" />
                                            <span text={action.text} />
                                        </Link>
                                    </cx>
                                ) : (
                                    <cx>
                                        <div class="more-action-body">
                                            <Icon name={action.icon} class="size-4" />
                                            <span text={action.text} />
                                        </div>
                                    </cx>
                                )}
                            </MenuItem>
                        </cx>
                    ))}
                </Menu>
            </MenuItem>
        </Menu>
    </cx>
);
