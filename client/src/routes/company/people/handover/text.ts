import type { Language } from "../../../../documents/languages";
import type { HandoverText } from "./model";

/**
 * The sheet's text per language. Serbian is the original's word for word — the company's own legal
 * wording; English is its translation.
 */
export const handoverText: Record<Language, HandoverText> = {
    en: {
        classification: "Internal",
        title: ["List of work equipment", "assigned to the employee"],
        lead: "By signing this statement, I agree that: ",
        terms: [
            "The list of work equipment is complete and contains the equipment I am personally responsible for",
            "The work equipment assigned to me on my own responsibility is the company's property; I may not dispose of it, sell it, or allow anyone not employed by the company to use it",
            "If my employment with the company ends, I will immediately return the work equipment assigned to me to the responsible person or my superior",
            "If the work equipment assigned to me is damaged or lost through my improper conduct and/or negligence, I fully agree to compensate the entire value of the damaged/lost equipment",
        ],
        assets: {
            index: "No.",
            number: "Inventory number",
            name: "Name",
            description: "Description",
            type: "Type",
        },
        noData: "No data",
        seats: {
            title: "Licenses / subscriptions",
            index: "No.",
            software: "Software",
            license: "License",
            device: "On device",
            expires: "Expires",
        },
        closing:
            "This document has been drawn up and signed in 2 (two) copies, one kept by the employee and the other by the company.",
        signatures: {
            place: "Place:",
            name: "Full name:",
            date: "Date:",
            signature: "Signature:",
            controller: "Checked by:",
            responsible: "Supervisor:",
        },
    },
    "sr-Latn-BA": {
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
    },
};
