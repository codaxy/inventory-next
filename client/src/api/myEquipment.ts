import { send } from "./http";

/** Something of the employee's ending soon, by the dashboard's windows ahead. */
export interface EquipmentWarning {
    kind: "warranty" | "subscription";
    id: string;
    name: string;
    date: string;
    uses: string | null;
}

export type ThingKind = "device" | "furniture" | "license" | "seat";

/** How a warranty or a subscription stands, where it says something. */
export type Standing = "ok" | "soon" | "ended";

/** One thing of theirs, as a card: what it is, then its make and model. */
export interface EquipmentThing {
    kind: ThingKind;
    id: string;
    /** The type as recorded, to pick an icon by. */
    type: string | null;
    title: string;
    subtitle: string | null;
    /** The codes that name it, for the search; the card leaves them to its details. */
    name: string;
    number: number | null;
    serial: string | null;
    status: Standing | null;
    statusText: string | null;
    /** Written off or sold: where it went. */
    gone: string | null;
    /** Its type's icon key; none, the generic one. */
    icon: string | null;
    /** Where a device or a piece of furniture is, while their things are in more than one place. */
    place: string | null;
    /** When a subscription expired, for the pill's words: "3 Nov 2025". */
    expired: string | null;
}

export interface MyEquipment {
    name: string;
    warnings: EquipmentWarning[];
    devices: EquipmentThing[];
    furniture: EquipmentThing[];
    software: EquipmentThing[];
    /** Licenses held in their name: mostly a holding account's. */
    licenses: EquipmentThing[];
}

/** The signed-in employee's page, whole; refused to anyone else. */
export const getMyEquipment = () => send<MyEquipment>("/api/my-equipment");

export interface EquipmentFact {
    label: string;
    value: string;
    /** The inventory number of what the value names, shown muted beside it. */
    number: number | null;
    /** On the date a warranty or a subscription expires on, how it stands: shown as a pill, in full. */
    standing: Standing | null;
}

/** Who to call about a device: a maintenance contract on it. */
export interface EquipmentContract {
    vendor: string;
    number: string | null;
    contact: string | null;
    phone: string | null;
    email: string | null;
    ends: string | null;
}

/** An app on a device, with its license. */
export interface EquipmentDetailApp {
    name: string;
    license: string;
    licenseNumber: number | null;
    ends: string | null;
}

export interface EquipmentDetails {
    facts: EquipmentFact[];
    contracts: EquipmentContract[];
    apps: EquipmentDetailApp[];
}

/** One thing of theirs, as much as they need to tell IT or a repair shop about it. */
export const getMyEquipmentDetails = (kind: ThingKind, id: string) =>
    send<EquipmentDetails>(`/api/my-equipment/items/${kind}/${id}`);
