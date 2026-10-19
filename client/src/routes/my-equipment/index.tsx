import { createFunctionalComponent } from "cx/ui";
import { Icon } from "cx/widgets";

import { documentTitle } from "../../components/documentTitle";

// Bar widths for the outline's rows, fixed so it does not reshuffle on every render.
const rows = [
    [38, 22, 16],
    [30, 26, 12],
    [44, 18, 20],
    [26, 30, 14],
];

/**
 * An employee's own equipment: where a person signed in lands, and all they see. Not built yet — a
 * notice saying so, over the outline of the list to come.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow">
            <div class="page-top">
                <h1 class="page-header page-title" text="My equipment" />
                {documentTitle("My equipment")}
            </div>

            <div class="todo">
                <div class="todo-badge">
                    <Icon name="todo" class="size-5" />
                </div>
                <div class="min-w-0">
                    <span class="todo-pill" text="Under construction" />
                    <h2 class="todo-title" text="Your equipment will be listed here" />
                    <p
                        class="todo-text"
                        text="The devices, furniture and license seats assigned to you. This page is still being built."
                    />
                </div>
            </div>

            <div class="todo-outline" attrs={{ "aria-hidden": "true" }}>
                <div class="todo-outline-head">
                    <span class="skeleton-bar" style="width: 18%" />
                    <span class="skeleton-bar" style="width: 12%" />
                </div>
                {rows.map((widths) => (
                    <cx>
                        <div class="todo-outline-row">
                            {widths.map((width) => (
                                <cx>
                                    <span class="skeleton-bar" style={`width: ${width}%`} />
                                </cx>
                            ))}
                        </div>
                    </cx>
                ))}
            </div>
        </div>
    </cx>
));
