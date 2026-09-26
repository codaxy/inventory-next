import { createModel } from "cx/ui";

import type {
    ActivationDetail,
    DeviceOption,
    Expiry,
    Option,
    VolumeOption,
} from "../../../../api/activations";
import { perUser } from "../../../../api/activations";
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
    licenseId: string;
    license: string;
    volume: string;
    seats: string;
    assigneeLabel: string;
    assignee: string;
    activated: string;
    deactivated?: string;
    active: boolean;
    vendor?: string;
    licenseType?: string;
    expirationModel?: string;
    expiry?: Expiry;
    expiryText?: string;
    location?: string;
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

export const volumeText = (v: VolumeOption) =>
    `${v.license}${v.licenseNumber ? ` #${v.licenseNumber}` : ""} · ${v.type} · ${v.inUse} of ${v.quantity} in use`;

export const deviceText = (d: DeviceOption) =>
    [d.text, d.number ? `#${d.number}` : null, d.holder].filter(Boolean).join(" · ");

export function overWarning(volume: VolumeOption | undefined, quantity: number | null | undefined) {
    if (!volume || !quantity) return undefined;
    const free = volume.quantity - volume.inUse;
    return quantity > free
        ? `This takes ${volume.license} past its ${volume.quantity} seats: ${volume.inUse} are in use. It is recorded, not refused.`
        : undefined;
}

export const toView = (a: ActivationDetail): View => ({
    software: a.software.name,
    licenseId: a.license.id,
    license: a.license.number ? `${a.license.name} #${a.license.number}` : a.license.name,
    volume: `${a.volume.type} · ${a.volume.inUse} of ${a.volume.quantity} in use`,
    seats: a.quantity === 1 ? "1 seat" : `${a.quantity} seats`,
    assigneeLabel: a.volume.typeId === perUser ? "User" : "Device",
    assignee:
        a.person?.name ??
        (a.device ? `${a.device.name}${a.device.number ? ` #${a.device.number}` : ""}` : "—"),
    activated: formatDate(a.activationDate)!,
    deactivated: formatDate(a.deactivationDate),
    active: !a.deactivationDate,
    vendor: a.license.vendor ?? undefined,
    licenseType: [a.license.type, a.license.model].filter(Boolean).join(" · ") || undefined,
    expirationModel: a.license.expirationModel ?? undefined,
    expiry: a.license.expiry ?? undefined,
    expiryText: a.license.expiry
        ? `${expiryText[a.license.expiry]} · ${formatDate(a.license.expirationDate)}`
        : undefined,
    location: a.license.location ?? undefined,
    url: a.license.url ?? undefined,
});
