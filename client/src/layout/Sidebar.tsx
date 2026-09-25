import { createFunctionalComponent, expr, falsy } from "cx/ui";
import { Button, Icon, Link } from "cx/widgets";

import { Logo } from "../components/Logo";
import $app from "../model";
import { navigation } from "./navigation";

/** Initials for the avatar, from the name or, failing that, the address. */
function initials(name: string | undefined): string {
    const parts = (name ?? "").split(/[\s.@_-]+/).filter(Boolean);
    return `${parts[0]?.[0] ?? "?"}${parts[1]?.[0] ?? ""}`.toUpperCase();
}

const closeDrawer = (_e: unknown, { store }: any) => store.set($app.ui.drawerOpen, false);

/**
 * The navigation: a column from `lg` up, a drawer over the content below it. One component, so the two
 * cannot drift apart. Any tap inside it, a link included, closes the drawer.
 *
 * The menu is static, so it is built here rather than repeated from the store — which is also what
 * lets `match` differ per item: it is widget configuration, not a bindable prop.
 */
export const Sidebar = createFunctionalComponent(() => (
    <cx>
        {/* A sibling, not a wrapper, so the drawer slides over the content. Tapping it dismisses. */}
        <div
            class="fixed inset-0 z-40 bg-ink/30 lg:hidden"
            visible={$app.ui.drawerOpen}
            onClick={closeDrawer}
        />

        <aside
            class={{
                "fixed inset-y-0 left-0 z-50 flex h-dvh w-[220px] shrink-0 flex-col bg-nav": true,
                "transition-transform duration-200 lg:sticky lg:top-0 lg:translate-x-0": true,
                "-translate-x-full": falsy($app.ui.drawerOpen),
                "translate-x-0 shadow-xl lg:shadow-none": $app.ui.drawerOpen,
            }}
            onClick={closeDrawer}
        >
            <div class="px-5 pt-5 pb-2">
                <Logo subtitle="Asset register" />
            </div>

            <nav class="nav-scroll flex-1 overflow-y-auto px-2 pb-4">
                {navigation.map((section) => (
                    <cx>
                        <div>
                            <div class="nav-section" text={section.title} visible={!!section.title} />
                            {section.items.map((item) => (
                                <cx>
                                    <Link
                                        href={item.href}
                                        url={$app.url}
                                        class="nav-link"
                                        activeClass="nav-link-active"
                                        match={item.exact ? "equal" : "subroute"}
                                    >
                                        <Icon name={item.icon} class="nav-icon" />
                                        <span text={item.label} />
                                    </Link>
                                </cx>
                            ))}
                        </div>
                    </cx>
                ))}
            </nav>

            <div class="nav-footer">
                <div class="flex items-center gap-2.5">
                    <div
                        class="grid size-8 shrink-0 place-items-center rounded-full bg-nav-avatar text-[11px] font-bold text-nav-avatar-ink"
                        text={expr($app.session.user.name, $app.session.user.email, (name, email) =>
                            initials(name || email),
                        )}
                    />
                    <div class="min-w-0 flex-1">
                        <div
                            class="truncate text-[13px] font-semibold text-nav-ink"
                            text={expr(
                                $app.session.user.name,
                                $app.session.user.email,
                                (name, email) => name || email,
                            )}
                        />
                        <div
                            class="truncate text-[11px] text-nav-ink-dim"
                            text={$app.session.user.email}
                            visible={expr(
                                $app.session.user.name,
                                $app.session.user.email,
                                (name, email) => !!name && name != email,
                            )}
                        />
                    </div>
                </div>

                <div class="mt-2 flex">
                    <Button
                        mod={["hollow", "on-dark", "danger"]}
                        class="flex-1"
                        onClick="onSignOut"
                        text="Sign out"
                    />
                </div>
            </div>
        </aside>
    </cx>
));
