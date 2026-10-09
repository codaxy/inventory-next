import type { HandoverText } from "../model";

/** The sheet in English: a translation of the Serbian original. */
export default {
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
} satisfies HandoverText;
