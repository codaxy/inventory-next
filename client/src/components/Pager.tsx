import type { AccessorChain } from "cx/data";
import { createFunctionalComponent, createModel, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Repeater } from "cx/widgets";

import type { PagerLink, PagerState } from "../paging";

const $ = createModel<{ $link: PagerLink }>();

interface Props {
    state: AccessorChain<PagerState>;
    /** Called with the page to show. */
    onPage: (page: number, instance: any) => void;
}

/**
 * Previous and next either side of where the reader is. Page links from `sm` up; a phone gets the
 * position instead, which does not wrap at any width.
 */
export const Pager = createFunctionalComponent(({ state, onPage }: Props) => (
    <cx>
        <nav class="pager" attrs={{ "aria-label": "Pages" }}>
            <span class="pager-summary" text={state.summary} />

            <div class="pager-controls">
                <Button
                    mod="hollow"
                    class="pager-step"
                    disabled={falsy(state.hasPrevious)}
                    attrs={{ "aria-label": "Previous page" }}
                    onClick={(_e: unknown, instance: any) =>
                        onPage(instance.store.get(state.page) - 1, instance)
                    }
                >
                    <Icon name="previous" class="size-4" />
                </Button>

                <span class="pager-position sm:hidden" text={state.position} />

                <div class="hidden items-center gap-1 sm:flex">
                    <Repeater records={state.links} recordAlias={$.$link}>
                        <Button
                            mod="hollow"
                            class={{ "pager-link": true, "pager-link-current": $.$link.current }}
                            visible={hasValue($.$link.page)}
                            text={$.$link.text}
                            onClick={(_e: unknown, instance: any) =>
                                onPage(instance.store.get($.$link.page), instance)
                            }
                        />
                        <span class="pager-gap" visible={expr($.$link.page, (p) => p == null)} text="…" />
                    </Repeater>
                </div>

                <Button
                    mod="hollow"
                    class="pager-step"
                    disabled={falsy(state.hasNext)}
                    attrs={{ "aria-label": "Next page" }}
                    onClick={(_e: unknown, instance: any) =>
                        onPage(instance.store.get(state.page) + 1, instance)
                    }
                >
                    <Icon name="next" class="size-4" />
                </Button>
            </div>
        </nav>
    </cx>
));
