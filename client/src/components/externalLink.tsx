import type { AccessorChain } from "cx/data";
import { expr } from "cx/ui";
import { Icon } from "cx/widgets";

/** The first web address in a text: the whole of a URL field, or one inside a description. */
export function firstUrl(text: string | null | undefined): string | undefined {
    return text?.match(/https?:\/\/[^\s]+/)?.[0];
}

/**
 * A small icon link beside a value that holds a web address, opening it in a new tab — a plain anchor,
 * since cx's `Link` would route it inside the application. Absent when the value holds none. Never
 * inside another link: a row that is itself a link shows it on the record's page instead.
 */
export const externalLink = (value: AccessorChain<string | null | undefined>) => (
    <cx>
        <a
            class="external-link"
            visible={expr(value, (v) => !!firstUrl(v))}
            href={expr(value, (v) => firstUrl(v) ?? "")}
            target="_blank"
            rel="noopener noreferrer"
            title={expr(value, (v) => `Open ${firstUrl(v) ?? ""} in a new tab`)}
        >
            <Icon name="external" class="size-4" />
            <span class="sr-only" text="Open in a new tab" />
        </a>
    </cx>
);
