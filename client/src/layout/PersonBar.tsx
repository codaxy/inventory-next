import { createFunctionalComponent } from "cx/ui";

import { Logo } from "../components/Logo";
import { AccountMenu } from "./AccountMenu";

/**
 * An employee's shell, at every width: the mark, and their account menu opening below it. No sidebar
 * and no drawer — their one page needs no navigation, and a column holding a single link reads as
 * something missing.
 */
export const PersonBar = createFunctionalComponent(() => (
    <cx>
        <header class="top-bar person-bar fixed inset-x-0 top-0 z-30 flex items-center justify-between gap-3 bg-nav-bar px-4">
            <Logo />
            <div class="person-bar-account">
                <AccountMenu placement="down" />
            </div>
        </header>
    </cx>
));
