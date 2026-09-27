import { send } from "./http";
import type { Section } from "./assets";

export interface SubscriptionRow {
    id: string;
    number: number | null;
    name: string;
    vendor: string | null;
    expirationDate: string;
    autoRenew: boolean;
    /** Seats of its volumes still active: live software on a lapsed license. */
    activeSeats: number;
}

export interface VolumeRow {
    id: string;
    licenseId: string;
    license: string;
    software: string;
    quantity: number;
    inUse: number;
}

export interface WarrantyRow {
    id: string;
    number: number | null;
    name: string;
    holder: string | null;
    ends: string;
}

export interface DisposedSeatRow {
    id: string;
    software: string;
    deviceId: string;
    device: string;
    deviceNumber: number | null;
    location: string;
}

export interface IncompleteRow {
    id: string;
    kind: "device" | "furniture" | "license" | "information";
    number: number | null;
    name: string;
}

export interface Dashboard {
    expired: Section<SubscriptionRow>;
    expiringSoon: Section<SubscriptionRow>;
    overAllocated: Section<VolumeRow>;
    unused: Section<VolumeRow>;
    /** The free seats of every unused volume, not only the first. */
    unusedSeats: number;
    warrantiesEnded: Section<WarrantyRow>;
    warranties: Section<WarrantyRow>;
    /** Absent where no disposal location is configured. */
    disposedSeats: Section<DisposedSeatRow> | null;
    incomplete: Section<IncompleteRow>;
    subscriptionsExpiredDays: number;
    subscriptionsExpiringDays: number;
    warrantiesEndedDays: number;
    warrantiesEndingDays: number;
}

export const getDashboard = () => send<Dashboard>("/api/dashboard");
