import { createFunctionalComponent } from "cx/ui";
import { Button } from "cx/widgets";

import { Logo } from "../components/Logo";
import $app from "../model";

/**
 * The bar that stands in for the sidebar below `lg`: the menu button and the mark. No page title — every
 * screen opens with its own, and a second copy two lines above wastes the scarcest space on a phone.
 */
export const TopBar = createFunctionalComponent(() => (
    <cx>
        <header class="sticky top-0 z-30 flex items-center gap-3 bg-nav-bar px-4 py-3 lg:hidden">
            <Button
                mod={["hollow", "on-dark"]}
                class="nav-toggle"
                attrs={{ "aria-label": "Open navigation" }}
                onClick={(_e: unknown, { store }: any) => store.toggle($app.ui.drawerOpen)}
            >
                {/* Three bars drawn in CSS, so they follow the text colour. */}
                <span class="flex h-4 w-5 flex-col justify-between">
                    <span class="block h-0.5 w-full rounded-full bg-current" />
                    <span class="block h-0.5 w-full rounded-full bg-current" />
                    <span class="block h-0.5 w-full rounded-full bg-current" />
                </span>
            </Button>

            <Logo />
        </header>
    </cx>
));
