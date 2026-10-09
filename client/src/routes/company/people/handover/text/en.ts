import type { HandoverText } from "../model";

/** The sheet in English: a translation of the Serbian original. */
export default {
    classification: "Internal",
    title: ["Employee Equipment", "Assignment Record"],
    lead: "By signing this document, I acknowledge and agree that: ",
    terms: [
        "The list below is complete and includes all company equipment for which I am personally responsible.",
        "The equipment assigned to me, for which I am personally responsible, remains the property of the company. I may not transfer or sell it, or allow anyone outside the company to use it.",
        "Upon termination of my employment, I will immediately return all equipment assigned to me to the designated company representative or my supervisor.",
        "If any equipment assigned to me is damaged or lost due to my improper conduct or negligence, I agree to reimburse the company for the full value of the damaged or lost equipment.",
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
        "This document has been prepared and signed in two copies, one kept by the employee and the other by the company.",
    signatures: {
        place: "Place:",
        name: "Full name:",
        date: "Date:",
        signature: "Signature:",
        controller: "Verified by:",
        responsible: "Supervisor:",
    },
} satisfies HandoverText;
