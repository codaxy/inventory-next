import { TextField } from "cx/widgets";

/**
 * House defaults for CxJS widgets, set on the prototype once at startup so no field has to repeat
 * them; a field may still opt out, since props resolve through the prototype chain.
 *
 * **`trim` is not tidiness.** `TextField` writes `text || emptyValue`, so with trimming a value of only
 * spaces becomes `null`, which `required` counts as empty. Without it, three spaces satisfy
 * `required`. `TextArea` has no `trim`: prose keeps its whitespace.
 */
export function installWidgetDefaults(): void {
    TextField.prototype.trim = true;
}
