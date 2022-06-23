type FCase = "Some" | "None";

export type FOption<T> = {
    case: FCase;
    fields: T[];
};

export function None<T>() : FOption<T> {
    return {case: "None", fields: []}
}

export function isSome(option: FOption<any>): boolean {
    if(!option) {
        return false;
    }
    return option.case === "Some";
}

export function getSome(option: FOption<any>) {
    return option.fields[0]
}