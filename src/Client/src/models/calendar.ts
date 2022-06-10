export type Calendar = {
    version: string;
    settings: Settings;
    doors: Door[];
    owner: Owner;
};

export type UserData = {
    version: string;
    calendars: Map<number, Calendar>;
    owner: Owner;
}

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
        owner: { name: ""},
        calendars: new Map( [[2021, emptyCalendar()]])
    }
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
};
