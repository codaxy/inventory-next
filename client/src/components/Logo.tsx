import { createFunctionalComponent, hasValue } from "cx/ui";

import $app from "../model";

/**
 * The mark and wordmark, on the dark chrome. In the sidebar the build the server runs sits under the
 * name, as an application signs itself; in monospace, since it is a value, not a tagline. The top bar
 * shows the name alone.
 */
export const Logo = createFunctionalComponent(({ version }: { version?: boolean }) => (
    <cx>
        <div class="flex items-center gap-2.5">
            <div class="logo-mark" />
            <div>
                <div class="text-sm leading-tight font-bold tracking-tight text-nav-ink" text="Inventory" />
                <div
                    class="font-mono text-[10.5px] text-nav-ink-dim select-all"
                    text={$app.version}
                    visible={version ? hasValue($app.version) : false}
                />
            </div>
        </div>
    </cx>
));
