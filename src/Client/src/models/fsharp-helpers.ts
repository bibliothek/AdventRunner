type FCase = "Some" | "None";

export type FOption<T> = {
    Case: FCase;
    Fields: T[];
};

export type FEnum<T> = {
    Case: string;
}

export function None<T>() : FOption<T> {
    return {Case: "None", Fields: []}
}

export function Some<T>(v: T)  : FOption<T> {
    return {Case: "Some", Fields: [v]}
}

export function isSome(option: FOption<any> | undefined): boolean {
    if(!option) {
        return false;
    }
    return option.Case === "Some";
}

export function getSome(option: FOption<any>) {
    return option.Fields[0]
}