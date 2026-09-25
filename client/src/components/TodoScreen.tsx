import { createFunctionalComponent } from "cx/ui";
import { Icon } from "cx/widgets";

interface Props {
    title: string;
    /** The programme step that builds the screen. */
    step: number;
}

// Bar widths for the skeleton rows, fixed so the placeholder does not reshuffle on every render.
const rows = [
    [38, 22, 16],
    [30, 26, 12],
    [44, 18, 20],
    [26, 30, 14],
    [36, 20, 18],
];

/** A routed screen that is not built yet: a page of its own, saying so, over the outline of a list. */
export const TodoScreen = createFunctionalComponent(({ title, step }: Props) => (
    <cx>
        <div class="page-body">
            <h1 class="page-header page-title" text={title} />

            <div class="todo">
                <div class="todo-badge">
                    <Icon name="todo" class="size-5" />
                </div>
                <div class="min-w-0">
                    <span class="todo-pill" text="To do" />
                    <h2 class="todo-title" text="This screen is not built yet" />
                    <p
                        class="todo-text"
                        text={`It arrives with step ${step} of the rebuild. Until then, the original application has it.`}
                    />
                </div>
            </div>

            <div class="skeleton" attrs={{ "aria-hidden": "true" }}>
                <div class="skeleton-head">
                    <span style="width: 18%" />
                    <span style="width: 12%" />
                </div>
                {rows.map((widths) => (
                    <cx>
                        <div class="skeleton-row">
                            {widths.map((width) => (
                                <cx>
                                    <span style={`width: ${width}%`} />
                                </cx>
                            ))}
                        </div>
                    </cx>
                ))}
            </div>
        </div>
    </cx>
));
