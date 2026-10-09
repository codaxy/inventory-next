import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Icon, Link, Repeater } from "cx/widgets";

import { downloadButton } from "../../../../components/downloadButton";
import { languagePicker } from "../../../../documents/languagePicker";
import $app from "../../../../model";
import { sheetFit } from "../../../../sheetFit";
import Controller from "./Controller";
import m from "./model";

const h = m.handover;

/**
 * The equipment a person signs for, as the original printed it, in the language chosen (`text.ts`).
 * Only the sheet prints; the header band is the screen's, in the UI's language. On screen it is the printed page itself, fitted to the column (`sheetFit`); the server
 * prints this same page for the PDF, waiting on `data-print`.
 */
export default createFunctionalComponent(() => {
    const onColumnRef = sheetFit();

    return (
        <cx>
            <div class="page-body page-narrow" controller={Controller} attrs={{ "data-print": h.print }}>
                <div class="page-top handover-screen">
                    <div class="page-header">
                        <Link
                            href={expr(h.id, (id) => `~/company/people/${id}`)}
                            url={$app.url}
                            class="editor-back"
                        >
                            <Icon name="previous" class="size-4" />
                            <span text={expr(h.name, (n) => n || "Person")} />
                        </Link>
                        <div class="editor-heading">
                            <h1 class="page-title" text="Handover sheet" />
                            <div class="editor-heading-actions">
                                {languagePicker({
                                    id: "company-people-handover-language",
                                    value: h.language,
                                })}
                                {downloadButton({
                                    href: h.pdfHref,
                                    visible: hasValue(h.pdfHref),
                                    text: "PDF",
                                    label: "Download as PDF",
                                    title: "Download the handover sheet as a PDF",
                                    noun: "PDF",
                                })}
                            </div>
                        </div>
                    </div>
                </div>

                <div class="editor-alert handover-screen" visible={hasValue(h.error)}>
                    <span text={h.error} />
                </div>

                <div class="handover-fit" onRef={onColumnRef} visible={falsy(h.error)}>
                    <style text={h.pageNumbers} />
                    <article class="handover" attrs={{ lang: h.language }}>
                        <div class="handover-part">
                            <header class="handover-header">
                                <div class="handover-logo" attrs={{ role: "img", "aria-label": "Codaxy" }} />
                                <div class="handover-class" text={h.text.classification} />
                            </header>
                            <h2 class="handover-title">
                                <span text={h.text.title[0]} />
                                <br />
                                <span text={h.text.title[1]} />
                            </h2>
                            <p class="handover-lead" text={h.text.lead} />
                            <ul class="handover-terms">
                                <Repeater records={h.text.terms} recordAlias={m.$term}>
                                    <li text={m.$term} />
                                </Repeater>
                            </ul>
                            <table class="handover-table">
                                <thead>
                                    <tr>
                                        <th text={h.text.assets.index} />
                                        <th text={h.text.assets.number} />
                                        <th text={h.text.assets.name} />
                                        <th text={h.text.assets.description} />
                                        <th text={h.text.assets.type} />
                                    </tr>
                                </thead>
                                <tbody>
                                    <Repeater records={h.rows} recordAlias={m.$row}>
                                        <tr>
                                            <td text={m.$row.index} />
                                            <td text={m.$row.number} />
                                            <td text={m.$row.name} />
                                            <td text={m.$row.description} />
                                            <td text={m.$row.type} />
                                        </tr>
                                    </Repeater>
                                    <tr
                                        visible={expr(
                                            h.rows,
                                            h.print,
                                            (rows, print) => print === "ready" && !rows?.length,
                                        )}
                                    >
                                        <td colSpan={5} class="handover-empty" text={h.text.noData} />
                                    </tr>
                                </tbody>
                            </table>

                            <div visible={expr(h.seats, (seats) => !!seats?.length)}>
                                <h3 class="handover-subtitle" text={h.text.seats.title} />
                                <table class="handover-table handover-seats">
                                    <thead>
                                        <tr>
                                            <th text={h.text.seats.index} />
                                            <th text={h.text.seats.software} />
                                            <th text={h.text.seats.license} />
                                            <th text={h.text.seats.device} />
                                            <th text={h.text.seats.expires} />
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <Repeater records={h.seats} recordAlias={m.$seat}>
                                            <tr>
                                                <td text={m.$seat.index} />
                                                <td text={m.$seat.software} />
                                                <td>
                                                    <span text={m.$seat.license} />{" "}
                                                    <span
                                                        class="handover-number"
                                                        text={m.$seat.licenseNumber}
                                                    />
                                                </td>
                                                <td>
                                                    <span text={m.$seat.device} />{" "}
                                                    <span
                                                        class="handover-number"
                                                        text={m.$seat.deviceNumber}
                                                    />
                                                </td>
                                                <td text={m.$seat.expires} />
                                            </tr>
                                        </Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </div>

                        {/* The closing line and the signatures, printed together, never split. */}
                        <div class="handover-part handover-closing">
                            <p text={h.text.closing} />
                            <div class="handover-signatures">
                                {line(h.text.signatures.place, h.place)}
                                {line(h.text.signatures.name, h.name)}
                                {line(h.text.signatures.date, h.date)}
                                {line(h.text.signatures.signature)}
                                {line(h.text.signatures.controller, h.controller)}
                                {line(h.text.signatures.responsible)}
                            </div>
                        </div>
                    </article>
                </div>
            </div>
        </cx>
    );
});

/** A signature line: its label, and what the sheet fills in, if anything, written on the line. */
function line(label: typeof h.name, value?: typeof h.name) {
    return (
        <cx>
            <div class="handover-line">
                <span text={label} />
                <span class="handover-blank" text={value ?? ""} />
            </div>
        </cx>
    );
}
