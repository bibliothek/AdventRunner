type FCase = "Some" | "None";

export type FOption<T> = {
    case: FCase;
    fields: T[];
};

export type FEnum<T> = {
    case: string;
}

export function None<T>() : FOption<T> {
    return {case: "None", fields: []}
}

export function Some<T>(v: T)  : FOption<T> {
    return {case: "Some", fields: [v]}
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