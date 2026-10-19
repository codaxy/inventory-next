import type { AccessorChain } from "cx/data";
import type { Controller } from "cx/ui";

import { guessIcon, type IconKind } from "./typeIcons";

type Store = Controller["store"];

/** What a type editor binds that the icon's guess reads and writes. */
export interface TypeIconFields {
    /** The type's id: none while creating. */
    id: AccessorChain<string | null>;
    name: AccessorChain<string | null | undefined>;
    icon: AccessorChain<string | null | undefined>;
    /** Whether the name has focus, bound to its field with `trackFocus`. */
    nameFocused: AccessorChain<boolean | undefined>;
    /** The server's message for the icon, gone once the icon changes. */
    error: AccessorChain<string | undefined>;
}

/**
 * A type editor's icon, picked or guessed: one for both type editors. Leaving the name of a type being
 * created, changed since it was last left, sets the icon the name suggests, over one picked before —
 * the icon is just below, so the change is seen and undone in a click. A name left unchanged guesses
 * nothing, so tabbing through it keeps a pick; a name that suggests nothing leaves the icon as it is.
 * An existing type's icon changes only when picked.
 */
export class TypeIconGuess {
    /** The name the icon was last guessed from. */
    private guessedFrom?: string;

    constructor(
        private kind: IconKind,
        private fields: TypeIconFields,
    ) {}

    /**
     * Guesses whenever the name is left — cx's `onBlur` is declared but never called — and drops the
     * icon's message once the icon changes.
     */
    watch(controller: Controller) {
        controller.addTrigger("icon-guess", [this.fields.nameFocused], (focused) => {
            if (focused === false) this.guess(controller.store);
        });
        controller.addTrigger("icon-edited", [this.fields.icon], () =>
            controller.store.delete(this.fields.error),
        );
    }

    /** A record opened, or a new one started: its name not yet guessed from. */
    reset() {
        this.guessedFrom = undefined;
    }

    pick(store: Store, key: string | null) {
        store.set(this.fields.icon, key);
    }

    private guess(store: Store) {
        if (store.get(this.fields.id)) return;
        const name = (store.get(this.fields.name) ?? "").trim();
        if (name === this.guessedFrom) return;
        this.guessedFrom = name;
        const guess = guessIcon(this.kind, name);
        if (guess !== undefined) store.set(this.fields.icon, guess);
    }
}
