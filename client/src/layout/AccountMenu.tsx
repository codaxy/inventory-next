import { createFunctionalComponent, expr, hasValue, History } from "cx/ui";
import { Icon, Menu, MenuItem } from "cx/widgets";

import { myEquipment } from "../access";
import $app from "../model";

const userWithEquipment = expr(
    $app.session.user.kind,
    $app.session.user.myEquipment,
    (kind, has) => kind === "user" && !!has,
);

/** Initials for the avatar, from the name or, failing that, the address. */
function initials(name: string | undefined): string {
    const parts = (name ?? "").split(/[\s.@_-]+/).filter(Boolean);
    return `${parts[0]?.[0] ?? "?"}${parts[1]?.[0] ?? ""}`.toUpperCase();
}

const displayName = expr($app.session.user.name, $app.session.user.email, (name, email) => name || email);

/**
 * The signed-in person and their role, at the foot of the navigation; clicking opens a menu above it
 * that holds the address they signed in with, their own equipment for a user who is a person too,
 * signing out, and beneath it the build the server runs. A cx `MenuItem` dropdown rather than a
 * hand-built popover: it brings Enter, Escape, arrow keys and closing when focus leaves.
 *
 * `openOnFocus` is off so tabbing through the navigation does not pop the menu open.
 */
export const AccountMenu = createFunctionalComponent(
    ({ placement = "up" }: { placement?: "up" | "down" }) => (
        <cx>
            <Menu class="account-menu">
                <MenuItem
                    class="account-trigger"
                    openOnFocus={false}
                    placement={placement}
                    dropdownOptions={{
                        matchWidth: true,
                        placementOrder: placement === "up" ? "up down" : "down up",
                        offset: 6,
                        class: "account-dropdown",
                    }}
                >
                    <div class="flex items-center gap-2.5">
                        <div
                            class="grid size-8 shrink-0 place-items-center rounded-full bg-nav-avatar text-[11px] font-bold text-nav-avatar-ink"
                            text={expr($app.session.user.name, $app.session.user.email, (name, email) =>
                                initials(name || email),
                            )}
                        />
                        <div class="min-w-0 flex-1">
                            <div class="truncate text-[13px] font-semibold text-nav-ink" text={displayName} />
                            <div
                                class="truncate text-[11px] text-nav-ink-dim"
                                text={expr($app.session.user, (u) => u?.role ?? "Employee")}
                            />
                        </div>
                        {/* Pointing the way the menu opens: up from the sidebar's foot, down from the bar. */}
                        <Icon
                            name={placement === "up" ? "accountMenu" : "accountMenuDown"}
                            class="account-chevron"
                        />
                    </div>

                    <Menu putInto="dropdown">
                        {/* Not a MenuItem, as the version below. */}
                        <div class="account-email" text={$app.session.user.email} />
                        {/* A user's own equipment, where they are a person too: theirs, so with their
                            name rather than in the work's menu alone. An employee is on it already. */}
                        <MenuItem
                            class="account-action"
                            visible={userWithEquipment}
                            onClick={() => History.pushState({}, null, myEquipment)}
                            autoClose
                        >
                            <div class="flex items-center gap-2.5">
                                <Icon name="myEquipment" class="nav-icon" />
                                <span text="My inventory" />
                            </div>
                        </MenuItem>
                        <MenuItem class="account-action account-sign-out" onClick="onSignOut" autoClose>
                            <div class="flex items-center gap-2.5">
                                <Icon name="signOut" class="nav-icon" />
                                <span text="Sign out" />
                            </div>
                        </MenuItem>
                        {/* Not a MenuItem: nothing to do with it, so neither a click nor the arrow keys land on it. */}
                        <div class="account-version" text={$app.version} visible={hasValue($app.version)} />
                    </Menu>
                </MenuItem>
            </Menu>
        </cx>
    ),
);
