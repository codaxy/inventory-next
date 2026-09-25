import { createFunctionalComponent } from "cx/ui";

/** The mark and wordmark, on the dark chrome. The subtitle shows in the sidebar, not the top bar. */
export const Logo = createFunctionalComponent(({ subtitle }: { subtitle?: string }) => (
    <cx>
        <div class="flex items-center gap-2.5">
            <div class="logo-mark" />
            <div>
                <div class="text-sm leading-tight font-bold tracking-tight text-nav-ink" text="Inventory" />
                <div
                    class="text-[10px] font-semibold tracking-wider text-nav-ink-dim uppercase"
                    text={subtitle}
                    visible={!!subtitle}
                />
            </div>
        </div>
    </cx>
));
