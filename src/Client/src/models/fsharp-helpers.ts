type FCase = "Some" | "None";

export type FOption<T> = {
    case: FCase;
    fields: T[];
};

export function IsSome(option: FOption<any>): boolean {
    return option.case === "Some";
}