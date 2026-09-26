import { createModel } from "cx/ui";

import type { Option } from "../../../../api/assets";
import type { InCountry, LocationDetail, LocationForm } from "../../../../api/locations";
import { text } from "../../../../assets";
import { assetKinds, informationKind, toSections } from "../../../../holdings";
import type { RecordState } from "../../../../recordController";

export interface LocationDraft {
    name?: string | null;
    description?: string | null;
    countryId?: string | null;
    countryText?: string;
    stateId?: string | null;
    stateText?: string;
    cityId?: string | null;
    cityText?: string;
    postalCode?: string | null;
    street?: string | null;
    houseNumber?: number | null;
    floor?: number | null;
    room?: string | null;
}

export interface LocationState extends RecordState<LocationDraft> {
    options: {
        countries: Option[];
        /** Every city and state, and those of the chosen country, which the pickers offer. */
        allCities: InCountry[];
        allStates: InCountry[];
        cities: InCountry[];
        states: InCountry[];
    };
}

export interface Model {
    record: LocationState;
    $route: { id: string };
}

export default createModel<Model>();

export const toDraft = (l: LocationDetail): LocationDraft => ({
    name: l.name,
    description: l.description,
    countryId: l.country.code,
    countryText: l.country.name,
    stateId: l.state?.id,
    stateText: l.state?.name,
    cityId: l.city.id,
    cityText: l.city.name,
    postalCode: l.postalCode,
    street: l.street,
    houseNumber: l.houseNumber,
    floor: l.floor,
    room: l.room,
});

/** The country picker binds `countryId`, as every picker binds `<key>Id`; the API calls it the code. */
export const toForm = (d: LocationDraft): LocationForm => ({
    name: (d.name ?? "").trim(),
    description: text(d.description),
    countryCode: d.countryId ?? null,
    stateId: d.stateId ?? null,
    cityId: d.cityId ?? null,
    postalCode: text(d.postalCode),
    street: text(d.street),
    houseNumber: d.houseNumber ?? null,
    floor: d.floor ?? null,
    room: text(d.room),
});

export const toAttached = (l: LocationDetail) =>
    toSections(
        [...assetKinds(l.assets, "locationId", l.id), informationKind(l.information)],
        "Nothing is kept here.",
        (kinds) => `No ${kinds} kept here.`,
    );
