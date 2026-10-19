import type { AccessorChain } from "cx/data";
import { expr, type StringProp, Widget, type WidgetConfig } from "cx/ui";

import { destinationOf } from "../layout/navigation";
import $app from "../model";

interface DocumentTitleConfig extends WidgetConfig {
    /** What the page is; blank leaves the application's name alone. */
    text?: StringProp;
}

/** Names the browser's tab after the page while it is rendered: "Activations · Inventory". */
class DocumentTitle extends Widget<DocumentTitleConfig> {
    declareData(...args: any[]) {
        super.declareData({ text: undefined }, ...args);
    }

    prepareData(context: any, instance: any) {
        super.prepareData(context, instance);
        const text = instance.data.text;
        document.title = text ? `${text} · Inventory` : "Inventory";
    }

    render() {
        return null;
    }
}

/** The tab's title, beside the heading it repeats. */
export const documentTitle = (text: StringProp) => (
    <cx>
        <DocumentTitle text={text} />
    </cx>
);

/**
 * A record's tab: its name and number, "Edit …" while editing, and its menu item's title until the
 * name arrives.
 */
export const recordTitle = (
    title: AccessorChain<string | undefined>,
    number?: AccessorChain<string | undefined>,
) => {
    const name = (url: string | undefined, t: string | undefined, n?: string) => {
        if (!t) return destinationOf(url)?.title ?? "";
        const named = n ? `${t} ${n}` : t;
        return (url ?? "").split("?")[0].endsWith("/edit") ? `Edit ${named}` : named;
    };
    return documentTitle(number ? expr($app.url, title, number, name) : expr($app.url, title, name));
};
