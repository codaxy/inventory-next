import { Dropdown, TextField, Window } from "cx/widgets";

import { lockScroll } from "./scrollLock";

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

    // A dropdown follows its field when the page scrolls, and is not closed by it. cx closes one once
    // its field has moved 50px, and on a phone the page moves further than that whenever Safari lifts
    // a focused search box above the keyboard: the list closed the moment one tapped into it.
    Dropdown.prototype.closeOnScrollDistance = Infinity;

    // A modal window locks the page behind it, every one of them without being asked: the document
    // scrolls, so otherwise a drag scrolls the page under the window, and on a phone Safari's toolbar
    // resizes it as it goes. Gestures are refused, the page is not frozen — see `scrollLock.ts`.
    const didMount = Window.prototype.overlayDidMount;
    const willUnmount = Window.prototype.overlayWillUnmount;

    Window.prototype.overlayDidMount = function (instance, component) {
        didMount.call(this, instance, component);
        if (this.modal) component.releaseScroll = lockScroll(component.el);
    };

    Window.prototype.overlayWillUnmount = function (instance, component) {
        component.releaseScroll?.();
        willUnmount.call(this, instance, component);
    };
}
