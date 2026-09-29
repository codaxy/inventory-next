import { createFunctionalComponent, expr } from "cx/ui";
import { Icon, Menu, MenuItem } from "cx/widgets";

import $app from "../model";

/** Initials for the avatar, from the name or, failing that, the address. */
function initials(name: string | undefined): string {
    const parts = (name ?? "").split(/[\s.@_-]+/).filter(Boolean);
    return `${parts[0]?.[0] ?? "?"}${parts[1]?.[0] ?? ""}`.toUpperCase();
}

const displayName = expr($app.session.user.name, $app.session.user.email, (name, email) => name || email);
const showEmail = expr(
    $app.session.user.name,
    $app.session.user.email,
    (name, email) => !!name && name != email,
);

/**
 * The signed-in person, at the foot of the navigation; clicking opens a menu above it that holds
 * signing out. A cx `MenuItem` dropdown rather than a hand-built popover: it brings Enter, Escape,
 * arrow keys and closing when focus leaves.
 *
 * `openOnFocus` is off so tabbing through the navigation does not pop the menu open.
 */
export const AccountMenu = createFunctionalComponent(() => (
    <cx>
        <Menu class="account-menu">
            <MenuItem
                class="account-trigger"
                openOnFocus={false}
                placement="up"
                dropdownOptions={{
                    matchWidth: true,
                    placementOrder: "up down",
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
                            text={$app.session.user.email}
                            visible={showEmail}
                        />
                    </div>
                    <Icon name="accountMenu" class="account-chevron" />
                </div>

                <Menu putInto="dropdown">
                    <MenuItem class="account-action account-sign-out" onClick="onSignOut" autoClose>
                        <div class="flex items-center gap-2.5">
                            <Icon name="signOut" class="nav-icon" />
                            <span text="Sign out" />
                        </div>
                    </MenuItem>
                </Menu>
            </MenuItem>
        </Menu>
    </cx>
));
