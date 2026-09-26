import { createFunctionalComponent } from "cx/ui";

import Controller from "./Controller";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";

/**
 * The signed-in shell. Below `lg` the sidebar leaves the flow and becomes a drawer behind the top bar;
 * a fixed 260px column would leave a phone 130px of content.
 *
 * The document scrolls, not the content column: iPhone Safari collapses its toolbars only when the
 * document does, so a scrolling `main` leaves the address bar on screen for good. The desktop sidebar
 * and the phone's top bar are sticky instead.
 *
 * `min-w-0` on the content column is load-bearing: a flex child defaults to `min-width: auto`, so a wide
 * child — a grid, a long unbroken string — would push the column past the viewport instead of scrolling
 * inside it.
 */
export const AppLayout = createFunctionalComponent(({ children }: { children?: any }) => (
    <cx>
        <div class="flex min-h-full" controller={Controller}>
            <Sidebar />
            <div class="flex min-w-0 flex-1 flex-col">
                <TopBar />
                <main class="flex flex-1 flex-col">{children}</main>
            </div>
        </div>
    </cx>
));
