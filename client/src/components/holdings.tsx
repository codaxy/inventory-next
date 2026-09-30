import type { AccessorChain } from "cx/data";
import { createModel, expr, hasValue } from "cx/ui";
import { Link, Repeater } from "cx/widgets";

import type { HoldingRow, HoldingSection } from "../holdings";
import { expiryClass } from "../licensing";
import $app from "../model";
import { inventoryNumber } from "./inventoryNumber";

const hm = createModel<{ $section: HoldingSection; $holding: HoldingRow }>();
const h = hm.$holding;

/** A row's content, whether it links to its record or not. */
const holding = () => (
    <cx>
        <div class="holding-main">
            <span class="holding-title" text={h.title} />
            <span class="record-note" visible={hasValue(h.note)} text={h.note} />
            <span class="record-flag record-flag-ended" visible={hasValue(h.ended)} text={h.ended} />
        </div>
        <div class="holding-meta" visible={hasValue(h.meta)}>
            <span text={h.meta} />
            {inventoryNumber(h.metaNumber)}
            <span visible={hasValue(h.metaRest)} text={h.metaRest} />
        </div>
        <span
            class={expr(h.expiry, (x) => `holding-status status-tag ${expiryClass(x)}`)}
            visible={hasValue(h.expiryText)}
            text={h.expiryText}
        />
    </cx>
);

/**
 * What is attached to a record, read-only: a card per kind, its count in the title, its first rows,
 * and "See all" to the list filtered to the record.
 */
export const holdingSections = (
    sections: AccessorChain<HoldingSection[]>,
    visible: AccessorChain<boolean>,
) => (
    <cx>
        <Repeater records={sections} recordAlias={hm.$section} keyField="key">
            <section class="editor-section holding-section" visible={visible}>
                <div class="editor-section-head">
                    <h2 class="editor-section-title">
                        <span text={hm.$section.title} />
                        <span class="holding-count" text={hm.$section.count} />
                    </h2>
                </div>
                <div class="holding-list">
                    <Repeater records={hm.$section.rows} recordAlias={h} keyField="key">
                        <Link
                            class={{ "holding-row": true, "holding-row-ended": hasValue(h.ended) }}
                            visible={hasValue(h.href)}
                            href={h.href}
                            url={$app.url}
                        >
                            {holding()}
                        </Link>
                        <div class="holding-row" visible={expr(h.href, (x) => !x)}>
                            {holding()}
                        </div>
                    </Repeater>
                </div>
                <Link
                    class="editor-link holding-more"
                    visible={hasValue(hm.$section.moreHref)}
                    href={hm.$section.moreHref}
                    url={$app.url}
                    text={hm.$section.moreText}
                />
                <p
                    class="editor-hint"
                    visible={hasValue(hm.$section.limitNote)}
                    text={hm.$section.limitNote}
                />
            </section>
        </Repeater>
    </cx>
);
