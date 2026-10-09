import type { HandoverText } from "../model";

/** The sheet in Serbian: the original's word for word — the company's own legal wording. */
export default {
    classification: "Interno",
    title: ["Spisak sredstava za rad koje", "duži zaposleni"],
    lead: "Potpisivanjem ove izjave, slažem se da: ",
    terms: [
        "Lista sredstava za rad je kompletna i sadrži radna sredstva za koje lično odgovaram",
        "Sredstva za rad koja zadužujem na sopstvenu odgovornost su vlasništvo kompanije i ne smijem da ih otuđim, prodam ili omogućim njihovu upotrebu nezaposlenima u kompaniji",
        "Ukoliko mi prestane radni odnos sa kompanijom, odmah ću, odgovornoj osobi ili nadređenom, vratiti sredstva za rad koja lično zadužujem",
        "Ako nastane oštećenje ili gubljenje sredstava za rad koje lično zadužujem, a nastalo je mojim pogrešnim postupanjem i/ili nemarom u potpunosti se slažem da nadoknadim cjelokupnu vrijednost oštećenih/izgubljenih sredstva za rad",
    ],
    assets: {
        index: "R. Br",
        number: "Inventarni broj",
        name: "Naziv",
        description: "Opis",
        type: "Tip",
    },
    noData: "Nema podataka",
    seats: {
        title: "Licence / pretplate",
        index: "R. Br",
        software: "Softver",
        license: "Licenca",
        device: "Na uređaju",
        expires: "Ističe",
    },
    closing:
        "Ovaj dokument je napravljen i potpisan u 2 (dva) primjerka, od kojih jedan zadržava zaposleni, a drugi ostaje kompaniji.",
    signatures: {
        place: "Mjesto:",
        name: "Ime i prezime:",
        date: "Datum:",
        signature: "Potpis:",
        controller: "Kontrolor:",
        responsible: "Odgovorno lice ili nadređeni:",
    },
} satisfies HandoverText;
