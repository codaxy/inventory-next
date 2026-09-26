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
 * inside another link: a row that is itself a link shows it on the record's page instead. Shown
 * only while `shown` holds where it is given — a record's view, not its form: beside a field being
 * typed into, it opens an address that is not yet saved.
 */
export const externalLink = (
    value: AccessorChain<string | null | undefined>,
    shown?: AccessorChain<boolean | null | undefined>,
) => (
    <cx>
        <a
            class="external-link"
            visible={
                shown ? expr(value, shown, (v, s) => !!s && !!firstUrl(v)) : expr(value, (v) => !!firstUrl(v))
            }
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
