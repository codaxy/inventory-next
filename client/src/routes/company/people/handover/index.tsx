import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Icon, Link, Repeater } from "cx/widgets";

import { downloadButton } from "../../../../components/downloadButton";
import $app from "../../../../model";
import { sheetFit } from "../../../../sheetFit";
import Controller from "./Controller";
import m from "./model";

const h = m.handover;

/**
 * The equipment a person signs for, as the original printed it — its text word for word, the
 * company's own legal wording in its own language. Only the sheet prints; the header band is the
 * screen's. On screen it is the printed page itself, fitted to the column (`sheetFit`); the server
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
                    <article class="handover">
                        <div class="handover-part">
                            <header class="handover-header">
                                <div class="handover-logo" attrs={{ role: "img", "aria-label": "Codaxy" }} />
                                <div class="handover-class" text="Interno" />
                            </header>
                            <h2 class="handover-title">
                                <span text="Spisak sredstava za rad koje" />
                                <br />
                                <span text="duži zaposleni" />
                            </h2>
                            <p class="handover-lead" text="Potpisivanjem ove izjave, slažem se da: " />
                            <ul class="handover-terms">
                                <li text="Lista sredstava za rad je kompletna i sadrži radna sredstva za koje lično odgovaram" />
                                <li text="Sredstva za rad koja zadužujem na sopstvenu odgovornost su vlasništvo kompanije i ne smijem da ih otuđim, prodam ili omogućim njihovu upotrebu nezaposlenima u kompaniji" />
                                <li text="Ukoliko mi prestane radni odnos sa kompanijom, odmah ću, odgovornoj osobi ili nadređenom, vratiti sredstva za rad koja lično zadužujem" />
                                <li text="Ako nastane oštećenje ili gubljenje sredstava za rad koje lično zadužujem, a nastalo je mojim pogrešnim postupanjem i/ili nemarom u potpunosti se slažem da nadoknadim cjelokupnu vrijednost oštećenih/izgubljenih sredstva za rad" />
                            </ul>
                            <table class="handover-table">
                                <thead>
                                    <tr>
                                        <th text="R. Br" />
                                        <th text="Inventarni broj" />
                                        <th text="Naziv" />
                                        <th text="Opis" />
                                        <th text="Tip" />
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
                                        <td colSpan={5} class="handover-empty" text="No data" />
                                    </tr>
                                </tbody>
                            </table>

                            <div visible={expr(h.seats, (seats) => !!seats?.length)}>
                                <h3 class="handover-subtitle" text="Licence / pretplate" />
                                <table class="handover-table handover-seats">
                                    <thead>
                                        <tr>
                                            <th text="R. Br" />
                                            <th text="Softver" />
                                            <th text="Licenca" />
                                            <th text="Na uređaju" />
                                            <th text="Ističe" />
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
                            <p text="Ovaj dokument je napravljen i potpisan u 2 (dva) primjerka, od kojih jedan zadržava zaposleni, a drugi ostaje kompaniji." />
                            <div class="handover-signatures">
                                {line("Mjesto:", h.place)}
                                {line("Potpis:")}
                                {line("Datum:", h.date)}
                                {line("Ime i prezime:", h.name)}
                                {line("Kontrolor:", h.controller)}
                                {line("Odgovorno lice ili nadređeni:")}
                            </div>
                        </div>
                    </article>
                </div>
            </div>
        </cx>
    );
});

/** A signature line: its label, and what the sheet fills in, if anything, written on the line. */
function line(label: string, value?: typeof h.name) {
    return (
        <cx>
            <div class="handover-line">
                <span text={label} />
                <span class="handover-blank" text={value ?? ""} />
            </div>
        </cx>
    );
}
