import { createModel } from "cx/ui";

import type {
    ActivationDetail,
    DeviceOption,
    Expiry,
    Option,
    VolumeOption,
} from "../../../../api/activations";
import { perUser } from "../../../../api/activations";
import { numberText, volumeText, withNumber } from "../../../../inventoryNumbers";
import { expiryText, formatDate } from "../../../../licensing";

/** The form, as the fields bind it: a pick as its id and text, numbers and dates `null` until set. */
export interface Draft {
    softwareId?: string | null;
    softwareText?: string;
    volumeId?: string | null;
    volumeText?: string;
    personId?: string | null;
    personText?: string;
    deviceId?: string | null;
    deviceText?: string;
    activationDate?: string | null;
    quantity?: number | null;
}

/** An existing activation, as the read-only page shows it. */
export interface View {
    software: string;
    softwareHref: string;
    licenseId: string;
    license: string;
    licenseNumber?: string;
    volume: string;
    seats: string;
    assigneeLabel: string;
    assignee: string;
    /** A device's inventory number. */
    assigneeNumber?: string;
    /** The person's page, or the device's. */
    assigneeHref?: string;
    activated: string;
    deactivated?: string;
    active: boolean;
    vendor?: string;
    vendorHref?: string;
    licenseType?: string;
    expirationModel?: string;
    expiry?: Expiry;
    expiryText?: string;
    location?: string;
    locationHref?: string;
    url?: string;
}

export interface EditorState {
    /** `null` while creating. */
    id: string | null;
    title: string;
    draft: Draft;
    view?: View;
    /** The day it began, for the deactivation's earliest date. */
    activationDate?: string;
    software: Option[];
    people: Option[];
    devices: Option[];
    volumes: Option[];
    /** The chosen volume, with its seats in use. */
    volume?: VolumeOption;
    forPerson: boolean;
    /** The address named the volume — `new?volumeId=…`, from a license's volume — so it and its
     *  software are shown, not asked. */
    fixed: boolean;
    /** Where the form was started from, and where its back link, Cancel and a save return: the
     *  license whose volume it activates, or the activations list. */
    origin: { href: string; text: string };
    /** Why the seats asked for run past the volume's; it warns, it does not refuse. */
    overWarning?: string;
    loading: boolean;
    saving: boolean;
    error?: string;
    errors: {
        volumeId?: string;
        personId?: string;
        deviceId?: string;
        activationDate?: string;
        quantity?: string;
    };
    valid: boolean;
    visited: boolean;
}

export interface Model {
    activation: EditorState;
    $route: { id: string };
}

export default createModel<Model>();

export const toForm = (d: Draft, forPerson: boolean) => ({
    volumeId: d.volumeId ?? null,
    personId: forPerson ? (d.personId ?? null) : null,
    deviceId: forPerson ? null : (d.deviceId ?? null),
    activationDate: d.activationDate ?? null,
    quantity: d.quantity ?? 1,
});

export const volumeOptionText = (v: VolumeOption) => `${volumeText(v)} · ${v.inUse} of ${v.quantity} in use`;

export const deviceText = (d: DeviceOption) =>
    [withNumber(d.text, d.number), d.holder].filter(Boolean).join(" · ");

export function overWarning(volume: VolumeOption | undefined, quantity: number | null | undefined) {
    if (!volume || !quantity) return undefined;
    const free = volume.quantity - volume.inUse;
    return quantity > free
        ? `This takes ${volume.license} past its ${volume.quantity} seats: ${volume.inUse} are in use. It is recorded, not refused.`
        : undefined;
}

export const toView = (a: ActivationDetail): View => ({
    software: a.software.name,
    softwareHref: `~/licenses/software-services/${a.software.id}`,
    licenseId: a.license.id,
    license: a.license.name,
    licenseNumber: numberText(a.license.number),
    volume: `${a.volume.type} · ${a.volume.inUse} of ${a.volume.quantity} in use`,
    seats: a.quantity === 1 ? "1 seat" : `${a.quantity} seats`,
    assigneeLabel: a.volume.typeId === perUser ? "User" : "Device",
    assignee: a.person?.name ?? a.device?.name ?? "—",
    assigneeNumber: a.person ? undefined : numberText(a.device?.number),
    assigneeHref: a.person
        ? `~/company/people/${a.person.id}`
        : a.device
          ? `~/electronic-devices/${a.device.id}`
          : undefined,
    activated: formatDate(a.activationDate)!,
    deactivated: formatDate(a.deactivationDate),
    active: !a.deactivationDate,
    vendor: a.license.vendor ?? undefined,
    vendorHref: `~/company/vendors/${a.license.vendorId}`,
    licenseType: [a.license.type, a.license.model].filter(Boolean).join(" · ") || undefined,
    expirationModel: a.license.expirationModel ?? undefined,
    expiry: a.license.expiry ?? undefined,
    expiryText: a.license.expiry
        ? `${expiryText[a.license.expiry]} · ${formatDate(a.license.expirationDate)}`
        : undefined,
    location: a.license.location ?? undefined,
    locationHref: a.license.locationId ? `~/company/locations/${a.license.locationId}` : undefined,
    url: a.license.url ?? undefined,
});
