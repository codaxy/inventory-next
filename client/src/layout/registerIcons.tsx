/** @jsxImportSource react */
// Plain React JSX, not cx's: HugeIcons ships arbitrary `[tag, attrs]` tuples, which only React's
// `createElement` replays. The views then bind `<Icon name={…} />`, cx's own name → glyph registry.
import { createElement } from "react";
import { Icon } from "cx/widgets";

import { icons, type IconData } from "./icons";

interface IconRenderProps {
    key?: string;
    className?: string;
    style?: Record<string, string | number>;
}

// `Icon` passes no size, so the class the view gives it is the only thing that sets one.
const render =
    (data: IconData) =>
    ({ key, className, style }: IconRenderProps) =>
        createElement(
            "svg",
            { key, className, style, viewBox: "0 0 24 24", fill: "none", "aria-hidden": "true" },
            data.map(([tag, attrs], i) => createElement(tag, { ...attrs, key: attrs.key ?? i })),
        );

for (const [name, data] of Object.entries(icons)) Icon.register(name, render(data));
