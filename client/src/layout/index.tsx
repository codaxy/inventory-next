import { createFunctionalComponent } from "cx/ui";
import { PureContainer } from "cx/widgets";

import { isPerson, isUser } from "../access";

import Controller from "./Controller";
import { PersonBar } from "./PersonBar";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";

/**
 * The signed-in shell. Below `lg` the sidebar leaves the flow and becomes a drawer behind the top bar;
 * a fixed 260px column would leave a phone 130px of content.
 *
 * The document scrolls, not the content column: iPhone Safari collapses its toolbars only when the
 * document does, so a scrolling `main` leaves the address bar on screen for good. The desktop sidebar
 * is sticky instead, and the phone's top bar fixed, `main` padded by its height: a sticky one iOS
 * Safari dropped for a moment on a hard fling.
 *
 * An employee gets neither sidebar nor drawer: `PersonBar` at every width, the page beneath it.
 *
 * `min-w-0` on the content column is load-bearing: a flex child defaults to `min-width: auto`, so a wide
 * child — a grid, a long unbroken string — would push the column past the viewport instead of scrolling
 * inside it.
 */
export const AppLayout = createFunctionalComponent(({ children }: { children?: any }) => (
    <cx>
        <div class={{ "shell flex min-h-full": true, "shell-person": isPerson }} controller={Controller}>
            <PureContainer visible={isUser}>
                <Sidebar />
            </PureContainer>
            <div class="flex min-w-0 flex-1 flex-col">
                <PureContainer visible={isUser}>
                    <TopBar />
                </PureContainer>
                <PureContainer visible={isPerson}>
                    <PersonBar />
                </PureContainer>
                <main class="flex flex-1 flex-col pt-(--shell-top)">{children}</main>
            </div>
        </div>
    </cx>
));
