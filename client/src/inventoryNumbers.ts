/** "#100661": an inventory number as the screens show it. */
export const numberText = (number: number | null | undefined) => (number ? `#${number}` : undefined);

/**
 * A picker's option, "Adobe Illustrator #100661": the one place the number is part of a name's text.
 * Everywhere else it is `inventoryNumber()` beside the name, muted.
 */
export const withNumber = (name: string, number: number | null | undefined) =>
    number ? `${name} #${number}` : name;

/** A volume's option, "Adobe Illustrator #100661 · Per user": its license, then what tells it apart. */
export const volumeText = (v: { license: string; licenseNumber: number | null; designator: string }) =>
    `${withNumber(v.license, v.licenseNumber)} · ${v.designator}`;
