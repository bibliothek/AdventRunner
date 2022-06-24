import {
    FOption,
    None
} from "./fsharp-helpers";

export type Calendar = {
    version: string;
    settings: Settings;
    doors: Door[];
    owner: Owner;
};

export type UserData = {
    version: string;
    calendars: { [key: number]: Calendar };
    owner: Owner;
    displayName: FOption<string>;
    latestPeriod: number;
};

export type SharedLinkPostRequest = { period: number };
export type SharedLinkResponse = {calendar: Calendar; period: number; displayName: FOption<string>}

export function emptyCalendar(): Calendar {
    return {
        version: "",
        settings: { distanceFactor: 1 },
        doors: [],
        owner: { name: "" },
    };
}

export function emptyUserData(): UserData {
    return {
        version: "",
        owner: { name: "" },
        calendars: { 0: emptyCalendar() },
        latestPeriod: 0,
        displayName: None<string>()
    };
}

export type Owner = {
    name: string;
};

export type Door = {
    day: number;
    distance: number;
    state: DoorState;
};

export type DoorState = {
    case: DoorStateCase;
};

export type DoorStateCase = "Closed" | "Open" | "Done" | "Failed";

export type Settings = {
    distanceFactor: number;
    sharedLinkId?: FOption<string>;
};
