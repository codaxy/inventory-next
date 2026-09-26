import { createFunctionalComponent, falsy } from "cx/ui";
import { Icon, Link } from "cx/widgets";

import { Logo } from "../components/Logo";
import $app from "../model";
import { AccountMenu } from "./AccountMenu";
import { navigation } from "./navigation";

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
            class="fixed inset-0 z-40 touch-none bg-ink/30 lg:hidden"
            visible={$app.ui.drawerOpen}
            onClick={closeDrawer}
        />

        <aside
            class={{
                "nav-drawer fixed inset-y-0 left-0 z-50 flex h-dvh w-[min(260px,calc(100vw-56px))] shrink-0 flex-col bg-nav": true,
                "transition-transform duration-200 lg:sticky lg:top-0 lg:translate-x-0": true,
                "-translate-x-full": falsy($app.ui.drawerOpen),
                "translate-x-0 shadow-xl lg:shadow-none": $app.ui.drawerOpen,
            }}
            onClick={closeDrawer}
        >
            <div class="px-5 pt-5 pb-2">
                <Logo subtitle="Asset register" />
            </div>

            <nav class="nav-scroll flex-1 overflow-y-auto overscroll-contain px-2 pb-4">
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
                <AccountMenu />
            </div>
        </aside>
    </cx>
));
