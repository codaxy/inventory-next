import { createModel } from "cx/ui";

import type { Option } from "../../../api/assets";
import type {
    InformationDetail,
    InformationForm,
    InformationOptions,
    LocationKind,
    LocationRow,
} from "../../../api/informations";
import { firstUrl } from "../../../components/externalLink";
import { text } from "../../../assets";
import type { RecordState } from "../../../recordController";

/** Where a piece of information is kept, as the form holds it: a saved one kept or removed, a new one filled in. */
export interface PlaceRow {
    key: string;
    /** A saved one's id; absent for one being added. */
    id?: string;
    kindId?: LocationKind | null;
    kindText?: string;
    targetId?: string | null;
    targetText?: string;
    url?: string | null;
    /** A saved one, as shown: "Electronic device", and what. */
    label?: string;
    target?: string;
    /** Its page, or the address itself. */
    href?: string;
    external?: string;
    removed?: boolean;
}

export interface InformationDraft {
    name?: string | null;
    typeId?: string | null;
    typeText?: string;
    personId?: string | null;
    personText?: string;
    author?: string | null;
    accessRights?: string | null;
    personalInformation: boolean;
    clientsPersonalInformation: boolean;
    incomplete: boolean;
    description?: string | null;
    note?: string | null;
    confidentialityId?: string | null;
    confidentialityText?: string;
    integrityId?: string | null;
    integrityText?: string;
    availabilityId?: string | null;
    availabilityText?: string;
    projectId?: string | null;
    projectText?: string;
    tags: Option[];
    places: PlaceRow[];
}

export interface InformationState extends RecordState<InformationDraft> {
    options: InformationOptions & { kinds: { id: LocationKind; text: string }[] };
    importance: string;
}

export interface Model {
    record: InformationState;
    $route: { id: string };
    $place: PlaceRow;
    $tag: Option;
}

export default createModel<Model>();

export const kinds: { id: LocationKind; text: string; list?: keyof InformationOptions }[] = [
    { id: "device", text: "Electronic device", list: "devices" },
    { id: "virtualMachine", text: "Virtual machine", list: "virtualMachines" },
    { id: "software", text: "Software", list: "software" },
    { id: "cloudSubscription", text: "Cloud subscription", list: "cloudSubscriptions" },
    { id: "location", text: "Physical location", list: "locations" },
    { id: "url", text: "Web address" },
];

export const emptyOptions: InformationState["options"] = {
    types: [],
    people: [],
    projects: [],
    tags: [],
    confidentialities: [],
    integrities: [],
    availabilities: [],
    devices: [],
    virtualMachines: [],
    software: [],
    cloudSubscriptions: [],
    locations: [],
    kinds: kinds.map((k) => ({ id: k.id, text: k.text })),
};

let next = 0;
export const placeKey = () => `p${++next}`;

/** A saved place's page: a device's, a location's — the machines, software and cloud subscriptions ahead of their screens. */
const pageOf: Record<LocationKind, ((id: string) => string) | undefined> = {
    device: (id) => `~/electronic-devices/${id}`,
    virtualMachine: (id) => `~/infrastructure/virtual-machines/${id}`,
    software: (id) => `~/infrastructure/software/${id}`,
    cloudSubscription: (id) => `~/infrastructure/cloud-subscriptions/${id}`,
    location: (id) => `~/company/locations/${id}`,
    url: undefined,
};

const toPlace = (l: LocationRow): PlaceRow => ({
    key: placeKey(),
    id: l.id,
    label: kinds.find((k) => k.id === l.kind)?.text,
    target: l.number ? `${l.target} · #${l.number}` : l.target,
    href: l.targetId ? pageOf[l.kind]?.(l.targetId) : undefined,
    external: l.kind === "url" ? firstUrl(l.target) : undefined,
});

const pick = (ref: { id: string; name: string } | null, key: string) =>
    ref ? { [`${key}Id`]: ref.id, [`${key}Text`]: ref.name } : {};

export const toDraft = (i: InformationDetail): InformationDraft => ({
    name: i.name,
    author: i.author,
    accessRights: i.accessRights,
    personalInformation: i.personalInformation,
    clientsPersonalInformation: i.clientsPersonalInformation,
    incomplete: i.incomplete,
    description: i.description,
    note: i.note,
    ...pick(i.type, "type"),
    ...pick(i.person, "person"),
    ...pick(i.confidentiality, "confidentiality"),
    ...pick(i.integrity, "integrity"),
    ...pick(i.availability, "availability"),
    ...pick(i.project, "project"),
    tags: i.tags.map((t) => ({ id: t.id, text: t.name })),
    places: i.locations.map(toPlace),
});

export const toForm = (d: InformationDraft): InformationForm => ({
    name: (d.name ?? "").trim(),
    typeId: d.typeId ?? null,
    personId: d.personId ?? null,
    author: text(d.author),
    accessRights: text(d.accessRights),
    personalInformation: !!d.personalInformation,
    clientsPersonalInformation: !!d.clientsPersonalInformation,
    incomplete: !!d.incomplete,
    description: text(d.description),
    note: text(d.note),
    confidentialityId: d.confidentialityId ?? null,
    integrityId: d.integrityId ?? null,
    availabilityId: d.availabilityId ?? null,
    projectId: d.projectId ?? null,
    tagIds: (d.tags ?? []).map((t) => t.id),
    locations: (d.places ?? [])
        .filter((p) => !p.removed)
        .map((p) =>
            p.id
                ? { id: p.id }
                : p.kindId === "url"
                  ? { kind: "url", url: text(p.url) }
                  : { kind: p.kindId ?? null, targetId: p.targetId ?? null },
        ),
});

/** The pickers a new place's target offers: its kind's list. */
export const targetsOf = (
    o: InformationState["options"] | undefined,
    kind: LocationKind | null | undefined,
) => {
    const list = kinds.find((k) => k.id === kind)?.list;
    return list && o ? (o[list] as Option[]) : [];
};
