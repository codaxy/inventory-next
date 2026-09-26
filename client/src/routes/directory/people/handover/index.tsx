import { createFunctionalComponent, expr, falsy, hasValue } from "cx/ui";
import { Button, Icon, Link, Repeater } from "cx/widgets";

import $app from "../../../../model";
import Controller from "./Controller";
import m from "./model";

const h = m.handover;

/**
 * The equipment a person signs for, as the original printed it — its text word for word, the
 * company's own legal wording in its own language. Only the sheet prints; the header band is the
 * screen's.
 */
export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body page-narrow" controller={Controller}>
            <div class="page-header handover-screen">
                <Link
                    href={expr(h.id, (id) => `~/directory/people/${id}`)}
                    url={$app.url}
                    class="editor-back"
                >
                    <Icon name="previous" class="size-4" />
                    <span text={expr(h.name, (n) => n || "Person")} />
                </Link>
                <div class="editor-heading">
                    <h1 class="page-title" text="Handover sheet" />
                    <div class="editor-heading-actions">
                        <Button mod="primary" onClick="print" disabled={h.loading}>
                            <Icon name="print" class="size-4" />
                            <span text="Print" />
                        </Button>
                    </div>
                </div>
            </div>

            <div class="editor-alert handover-screen" visible={hasValue(h.error)}>
                <span text={h.error} />
            </div>

            <article class="handover" visible={falsy(h.error)}>
                <div class="handover-part">
                    <header class="handover-header">
                        <div text="CODAXY" />
                        <div text="Interno" />
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
                                    h.loading,
                                    (rows, loading) => !loading && !rows?.length,
                                )}
                            >
                                <td colSpan={5} class="handover-empty" text="No data" />
                            </tr>
                        </tbody>
                    </table>
                </div>

                {/* The original's second page: the closing line and the signatures. */}
                <div class="handover-part handover-closing">
                    <p text="Ovaj dokument je napravljen i potpisan u 2 (dva) primjerka, od kojih jedan zadržava zaposleni, a drugi ostaje kompaniji." />
                    <div class="handover-signatures">
                        <div class="handover-line">
                            <span text="Mjesto: " />
                            <span class="handover-blank" />
                        </div>
                        <div class="handover-line">
                            <span text="Potpis: " />
                            <span class="handover-blank" />
                        </div>
                        <div class="handover-line">
                            <span text="Datum: " />
                            <span class="handover-blank" />
                        </div>
                        <div class="handover-line">
                            <span text="Ime i prezime: " />
                            <span class="handover-blank" />
                        </div>
                        <div class="handover-line handover-line-last">
                            <span text="Odgovorno lice ili nadređeni:" />
                            <span class="handover-blank" />
                        </div>
                    </div>
                </div>
            </article>
        </div>
    </cx>
));
