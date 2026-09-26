---
name: cxjs
description: Use when writing or debugging CxJS client code in client/ — screens, widgets, bindings, forms, windows, lists, styling, the Vite setup — or when the app white-screens ("Objects are not valid as a React child"), or when CxJS documentation is needed.
---

# CxJS in inventory-next

CxJS is Codaxy's TypeScript UI framework on React: widgets, forms, grids, routing and a central
Store. The client is `client/` — cx 26, Vite 8, Tailwind 4, `cx-theme-variables`.

**Decisions live in `docs/engineering/web-client.md`; this skill is how to write the code.** Where the
two disagree, the document wins and this file is corrected. Adapted from another Codaxy CxJS application's skill; claims
about cx internals were re-checked against the installed cx 26.9.3, and a claim that names a file and
line was read there, not remembered.

## Documentation

Base: https://cxjs.io/docs

- **Every docs page is fetchable as Markdown: append `.md`.** Example:
  https://cxjs.io/docs/intro/what-is-cxjs.md
- **Do not guess slugs** — a wrong one returns an HTML 404. https://cxjs.io/llms.txt links every page;
  `llms-small.txt` (~100KB) and `llms-full.txt` (~500KB) are bulk contexts.
- Verified pages: `intro/functional-components.md`, `intro/data-binding.md`, `intro/store.md`,
  `intro/jsx-syntax.md`, `intro/controllers.md`, `tables/grid.md`, `intro/tailwind-css.md`.
- **The source is installed.** `client/node_modules/cx/src/**/*.tsx` answers a question about a prop
  or a default faster and more reliably than the docs prose.

**Formatting is not your job.** A pre-commit hook runs Prettier on staged client files; never run it
by hand and never argue with its output.

## A screen is a folder

```
routes/sign-in/
  index.tsx       the view — JSX only
  Controller.ts   the Controller — state and every API call
  model.ts        types and pure shaping — interface Model, the accessor, row types, toRow()
  utils.ts        pure functions and constants the screen needs
  columns.tsx     Grid column definitions, once there is a Grid
```

| File | Holds | Never holds |
|---|---|---|
| `index.tsx` | the default-exported `createFunctionalComponent` and its JSX | an API call, a row shape, a column array |
| `Controller.ts` | `onInit`, `store.init`, triggers, every `src/api/*` call, command methods | JSX — needing it means the code belongs in the view |
| `model.ts` | the types, the accessor, and functions that turn DTOs into rows | anything touching the store, the network or JSX |
| `utils.ts` | formatters, error maps, constants | state, JSX, API calls |

`Controller.ts` is `.ts`, not `.tsx`: the extension is the check. **`model.ts` imports nothing from its
own folder** — it is the leaf, so every other file can import it without a cycle. Import direction:
`index.tsx → Controller.ts → utils.ts → model.ts`. Anything two screens need goes in a module under
`src/`, never imported across route folders. One screen per folder; a second screen is a subfolder.

## Bindings are typed accessors, never strings

`bind('a.b')`, `expr('{a} && {b}')`, `tpl('… {x}')` and the `-bind`/`-expr`/`-tpl` attributes are the
legacy API: unchecked, so a typo renders blank and compiles. The client contains none; one appearing in
a diff is a regression.

```ts
// routes/things/model.ts
import { createModel } from "cx/ui";

export interface Row { title: string; done: boolean }
export interface Model {
    things: { rows: Row[]; loaded: boolean; error?: string | null };
    /** Repeater aliases are declared so `recordAlias` and every row binding are typed. */
    $row: Row;
}

export default createModel<Model>();
```

```tsx
import m from "./model";   // always `m`; the application-wide model is `$app` from src/model.ts
```

**Prefer the direct binding, then a helper, then `expr`:**

```tsx
visible={m.things.loaded}                         // a boolean is a boolean
visible={falsy(m.things.loaded)}                  // inverse
visible={equal($app.session.status, "loading")}   // comparison
text={tpl(m.signin.email, "We sent a code to {0}.")}
text={m.things.title}                              // no bind() wrapper
```

`cx/ui` exports `truthy`, `falsy`, `isTrue`, `isFalse`, `hasValue`, `isEmpty`, `isNonEmpty`,
`lessThan`, `lessThanOrEqual`, `greaterThan`, `greaterThanOrEqual`, `equal`, `notEqual`,
`strictEqual`. Reach for `expr(...accessors, fn)` only when none fits, and **lift a compound condition
out of the JSX and name it** (`const noProvider = expr(…)`), as the sign-in screen does.

**`visible` and `text` do not take a nullable accessor**: `StringProp` and `BooleanProp` admit no
`null`. Bind `visible={hasValue(m.x)}`, and type a value shown as text `x?: string` — absent, not `null`.

**A helper takes an accessor, not a computed value.** `falsy(anyProvider)` where `anyProvider` is an
`expr(…)` is a type error; write the inverse as its own `expr`.

**Controllers are typed too.** `store.get/set/init/update/delete` and `addTrigger` take accessors:
`this.store.get(m.signin.email)` is `string | null | undefined`, not `any`.

**`History.connect` takes a path**, so pass `$app.url.toString()`. An accessor's `toString()` is its
path, which is also what lets a string and a typed binding meet where an API only takes strings.

**Where state lives.** A screen's state is one branch of the store named after it (`m.signin.*`),
reset in `onInit` when the screen can be re-entered. There is no sandboxed per-route `$page` yet; it
arrives with the first screen whose state must survive navigation (cx `Sandbox` keyed by `$app.url`).
**A reusable component** keeps its state in a `PrivateStore` with its inputs as props typed
`StringProp`/`BooleanProp`/`RecordsProp<T>` — a `PrivateStore` isolates, which is what lets two sit on
one screen.

### `expr()` cannot take a component prop

Every argument to `expr()` goes through `Binding.get`, which accepts a path, `{ bind }` or an accessor.
A component prop is whatever the caller passed — often a bare `boolean` — so
`expr(someProp, m.$row.x, fn)` throws at runtime and nothing at build time sees it. Put the prop's
value on the record when shaping rows, and combine two record fields instead.

### A prop that duck-types a config is unsafe with an accessor

An accessor is a Proxy that returns another proxy for **every** property, so any `x.type`,
`x.isComponentType`, `x.bind` on it is truthy. `isSelector()` (`cx/build/data/isSelector.js`) opens
with `if (config.type || config.$type) return false`, so it reads an accessor as "not a selector".
**Field `label` is safe as of cx 26.9** — `Field.init` checks `this.label.isComponentType === true`
(`build/widgets/form/Field.js:89`), which a proxy never equals; older cx treated every accessor label
as a component and white-screened with `ins.scheduleExploreIfVisible is not a function`. For any
other prop that can take either a widget or a value, pass `bind(…)` or check in the browser.

## Functional components must be wrapped

A bare arrow function used as an element inside `<cx>` is handed to React as a React component,
returns cx config objects, and the app white-screens with
`Objects are not valid as a React child (found: object with keys {$type, …})`.

```tsx
export const Panel = ({ children }: { children?: any }) => <cx>…</cx>;              // ❌
export const Panel = createFunctionalComponent(({ children }: { children?: any }) => <cx>…</cx>); // ✅
```

Every screen is one. **The root routes are a `<cx>` element, not a component**, because
`startHotAppLoop` takes configuration. Calling a function that returns JSX is fine — a recursive
helper over a tree works; the danger is passing a function *as an element*.

**Text goes in `text=` on a self-closing element**: `<h1 text="Inventory" />`, not
`<h1>Inventory</h1>`. Elements with real or mixed children stay as they are. **A bare number child
breaks rendering** — `{items.length} items` logs "error during concurrent rendering"; write
`` {`${items.length} items`} ``.

## Forms

**Every input is a CxJS field and every form is wrapped in a `ValidationGroup`.** No raw `<input>`,
no hand-written `if (!x) setError(…)` in a controller — declare `required`, `minLength`, `maxLength`,
`onValidate={(v) => message | undefined}` on the field, and let the group report whether the form may
be submitted. Fields: `TextField`, `TextArea`, `NumberField`, `DateField`, `LookupField`, `Checkbox`,
`Radio`, `Switch`, all from `cx/widgets`. **Server rules stay on the server**; client validation only
saves a round trip. **The server's answer about a field goes on that field** — `error={m.form.emailError}` turns it
red and a line beneath it says why, the key absent when there is none (`StringProp` admits no
`null`) and deleted by a trigger on the field's value — never in a banner above the form.

**The submit guard sets `visited`, it does not disable the button:**

```tsx
<ValidationGroup valid={m.form.valid} visited={m.form.visited}>…fields…</ValidationGroup>
<Button
    mod="primary"
    text="Save"
    onClick={(_e, { store, controller }: any) => {
        // Invalid: show every field's error at once and stop. A disabled button with no reason
        // given reads as a broken form.
        if (!store.get(m.form.valid)) return store.set(m.form.visited, true);
        controller.save();
    }}
/>
```

Disabling while a request is in flight is fine. **A one-field form whose only error is visible on
sight disables instead** — sign-in, per `web-client.md`.

**Every picker is a `LookupField`, never a `Select`.** A native `<select>` has no search. `options` is
a data prop, not children; `optionIdField`/`optionTextField` name the fields. It has **no `onChange`**,
so saving on change needs a Save button or a store trigger. Closed, it renders no `<input>`: the
clickable element is `div.cxe-lookupfield-input`.

**`reactOn` decides when a field writes to the store, and it differs per widget**
(`prototype.reactOn` in `build/widgets/form/*.js`): `TextField` `"change input blur enter"` — every
keystroke — `TextArea` `"blur"`, `NumberField` `"enter blur"`. Narrow it when a store trigger watches
the binding, or the trigger fires per character.

### Never initialise a text field

`Field.isEmpty` is `value == null || value === this.emptyValue`, with `emptyValue` `null`
(`build/widgets/form/Field.js:200`). A store value of `''` is therefore **filled**, and a form whose
draft starts at `''` passes `required` on the first click. **A text field's key is absent** — the store
reads `undefined`; the widget itself writes `null` on clear, so the type is `name?: string | null`.
Clear one from code with `store.delete(path)`, never `set(path, "")`. A filter is not initialised
either. `null` is the right initialiser only for a non-text draft empty — an id, a date, a number.

**`NumberField` and `DateField` need an adapter for that `null`**: `NumberProp` and `Prop<string|Date>`
admit no empty case although the runtime handles it. `numberValue(m.draft.amount)` from
`src/bindings.ts`, and `dateValue(m.draft.day)` for a `DateField`.

### Every `TextField` trims

`src/widgetDefaults.ts` sets `TextField.prototype.trim = true` once at startup; cx ships `false`
(`build/widgets/form/TextField.js:73`). **Never write `trim` on a field.** It is not tidiness: with it,
a value of only spaces becomes `null`, which `required` counts as empty. `TextArea` has no `trim` —
prose keeps its whitespace.

### Dates

**Never put a calendar date through `toISOString()`.** cx's default date encoding is `toISOString()`
of a local midnight, so picking 7 August east of UTC stores 6 August 22:00Z. `installDateCulture` in
`src/dates.ts` sets a `YYYY-MM-DD` encoder at startup, with `en-GB` and weeks from Monday; the server
side is `DateOnly` — see `persistence.md`.

### Labels and layout

- **A `label` renders nothing outside a labels layout.** Only `LabelsTopLayout`/`LabelsLeftLayout`
  read the label slot; a lone `<TextField label="Name" />` is silently unlabelled. Without a layout,
  name the input with `inputAttrs={{ "aria-label": "…" }}` — also the most durable test hook.
- **`ValidationGroup` renders no element**, so a `class` on it styles nothing and its children are
  direct children of whatever encloses it. Wrap it in a `div` for layout.
- **`LabelsTopLayout` renders a `<table>`**; keep buttons outside it.
- **A field's own `visited` latches true** (`Field.js:155`) and resetting the group's `visited` does
  not clear it until the field is recreated. Cosmetic; do not debug it as your own bug.
- **Never `type: "search"` on a `showClear` field.** The browser adds its own clear button, so the
  field shows two ×. `enterKeyHint: "search"` alone gives a phone keyboard its Search key.
- **`viewMode` on a `ValidationGroup` puts every field inside it in view mode.** A multiple
  `LookupField`'s view mode is its records' texts run together with no separator; render them yourself
  (chips) and show the field only while editing. A view-mode field keeps the input's padding, so it sits
  indented from its label unless that goes.
- **A route that matches two addresses keeps its component between them** — `:id` matches `new` and an
  id — so a controller that reads the route in `onInit` never sees the second one. Reopen on a trigger
  on the url.
- **Fields have no `onBlur`.** It is accepted and ignored. Watch the store with `addTrigger` instead.

## Windows

A form that is a **detour** — create something, confirm something irreversible — is a window; a form
that is the screen's ongoing work stays inline. Use `createHotPromiseWindowFactoryWithProps`:

```tsx
import { Controller, createModel } from "cx/ui";
import { Button, Window, createHotPromiseWindowFactoryWithProps } from "cx/widgets";

interface WindowModel { w: { draft: Draft; valid: boolean; visited: boolean } }
const wm = createModel<WindowModel>();

export const showNewThingWindow = createHotPromiseWindowFactoryWithProps<Props, Draft | false>(
    { hot: import.meta.hot },
    ({ initial }: Props) =>
        (resolve) => {
            // Dismissal is the default outcome: Cancel, Escape, the backdrop and browser Back all
            // resolve `false` without a handler each. Only a valid submit replaces it.
            let result: Draft | false = false;

            class WindowController extends Controller {
                onInit() {
                    this.store.init(wm.w, { draft: { ...initial }, valid: true, visited: false });
                }
                submit() {
                    if (!this.store.get(wm.w.valid)) return this.store.set(wm.w.visited, true);
                    result = this.store.get(wm.w.draft);
                    (this.instance as any).dismiss();
                }
            }

            return Window.create(
                <cx>
                    <Window title="New thing" controller={WindowController} modal center autoFocus
                        closeOnEscape dismissOnPopState onDestroy={() => resolve(result)}>
                        …fields…
                        <div putInto="footer">
                            <Button mod="hollow" dismiss text="Cancel" />
                            <Button mod="primary" onClick="submit" text="Create" />
                        </div>
                    </Window>
                </cx>,
            );
        },
);
```

- **A modal window locks the page behind it by itself**: `widgetDefaults.ts` wraps
  `Window.prototype.overlayDidMount`/`overlayWillUnmount` with `lockScroll`. Nothing to add per window;
  a non-modal one is not locked.
- **Back must close the window, not leave the screen.** `dismissOnPopState` only closes it on a
  navigation that has already happened. Take `historyEntry()` from `src/historyEntry.ts` when the
  factory runs and resolve through `release` in `onDestroy`, as the audit log's entry window does.
- **A window that can grow is centred by CSS**, not `center`, which positions it once: `position:
  fixed`, `top`/`left: 50%` and `translate(-50%, -50%)`, all `!important` against cx's inline styles,
  with a `max-height` and a scrolling body — see `_entry-window.scss`.
- **Never pass the store — pass values.** `await showNewThingWindow({ initial })`. With no store the
  factory creates a fresh one per open, which is the only thing that makes the window's state die with
  it. Hand it `this.store` and the `w` branch outlives the dialog; `store.init` is a no-op on a path
  that holds a value, so the next open silently shows the last draft.
- **A window that must not write may not import from `src/api/`.** It resolves a draft; the caller's
  controller saves it. That import list, not the window's logic, is what makes "Cancel writes nothing"
  true.
- The module is reachable **only through the factory** — exporting the `Window` config as well breaks
  hot reload of an open dialog. Window chrome is themed from `theme.ts` (`window*` variables).

## Context menus

`openContextMenu(e, <cx><Menu>…</Menu></cx>, instance)` from `cx/widgets`. **Pass `instance`** — it
gives the menu the record's store inside a `Repeater` or `Grid` row; without it handlers read blank.
A `MenuItem` needs `autoClose` or the menu stays open. It calls `preventDefault`/`stopPropagation`
itself, suppressing the browser's own menu wherever it is attached — attach narrowly. **Never the only
way to do something**: right-click is absent on a phone, and the phone is the hard case here.

## Dropdown menus

A `MenuItem` with a `<Menu putInto="dropdown">` child is a click-to-open menu with Enter, Escape and
focus-out closing built in. **Keep its dropdown `inline`** (the default): portaled with
`dropdownOptions={{ inline: false }}`, Enter opens it but focus cannot move into it, so it is
unreachable by keyboard. `openOnFocus={false}` stops Tab from popping it open. Content is a `div`, not
a bare `span` — the theme pads `.cxb-menuitem > span`.

## Lists: `Repeater`, not `ContentResolver`

```tsx
<Repeater records={m.things.rows} recordAlias={m.$row}>
    <div text={m.$row.title} />
</Repeater>
```

`ContentResolver` **remounts its whole subtree** when a param changes; put anything edited in its
params and every input inside is recreated — uncommitted text lost, focus gone, extra blur commits.
Use it only when the *widget type* depends on the value. Derived data is computed in the controller
and stored, not computed in `onResolve`.

**A record handler takes the alias as an accessor**, typed against the model, so a wrong alias is a
compile error rather than `Cannot read properties of undefined` one call away from the mistake:

```tsx
const onRecord =
    <T,>(fn: (c: ThingsController, record: T) => void, alias: AccessorChain<T>) =>
    (_e: unknown, instance: any) =>
        fn(instance.controller as ThingsController, instance.store.get(alias));
```

**An inactive `Tab`'s content is destroyed, not hidden.** What was typed survives only because the
click that switches tabs blurs the field first. Anything that switches tabs without a click drops
uncommitted input silently.

## Styling

Decisions and tokens are in `web-client.md` → *Theme*. In practice:

1. **A colour or shadow is a token** in `src/tailwind.css`. Never a hex value anywhere else.
2. **A CxJS widget is styled through `src/theme.ts`** first — the full variable list is
   `node_modules/cx-theme-variables/build/presets/default.js`; the docs do not have it.
3. **Then one partial per component in `src/scss/`**, `@forward`ed from `src/scss/index.scss`, for
   what no variable expresses. **No `@apply` in SCSS** — it reaches the browser verbatim.
4. **Tailwind utilities in markup** win over both, because the SCSS loads in `@layer components`.

**The theme's variables themselves are set inline on `<html>`** by `renderThemeVariables`, so a
stylesheet overrides one (`--cx-input-font-size` in a media query, say) only with `!important`.

**The theme's variable sheet is injected at runtime, so it wins specificity ties**, and its selectors
are often two classes deep (`.cxb-button.cxm-hollow`). Read the real rule from
`node_modules/cx-theme-variables/dist/widgets.css` rather than guessing. **`padding` cannot resize a
`Button`** — its height is computed from the line-height, padding and border variables; change those
in `theme.ts`, or set `height: auto` at `.cxb-button` specificity for a one-off.

**Controls are 44px** — the phone's tap target. Never adopt a density preset that shrinks them.

**A dropdown is not closed by scrolling here**: `Dropdown.prototype.closeOnScrollDistance` is
`Infinity` in `widgetDefaults.ts`, since the phone keyboard scrolls the page under a focused search box.
Never set it back per field.

**A `LookupField`'s height cap goes on the list itself**, `.cxe-lookupfield-scroll-container >
:first-child`, with its own `overflow-y: auto`. cx sizes and places the dropdown from that element's
height (`onMeasureNaturalContentSize`), so a cap on `div.cxb-dropdown` or
`div.cxe-lookupfield-dropdown` leaves cx sizing the box to all the room on screen: empty space under
the list, or a box opened upward and pinned to the top of the screen. The house cap is in
`_fields.scss`. **The search box appears from 7 options** (`minOptionsForSearchField`).

**Grid DOM:** each row is its own `<tbody class="cxe-grid-data cxs-…">` around one `tr`; state
classes land on the `tbody`. **Grid selects on `mousedown`**, not `click`.

**`Button`'s `pressed` does not produce `aria-pressed`**; use a bound class for a segmented state.

## Vite setup (do not regress)

- JSX runs through cx's runtime: `oxc.jsx.importSource: "cx"` in `vite.config.ts`,
  `"jsxImportSource": "cx"` in `tsconfig.json`. TypeScript only type-checks.
- **Import from `cx/ui`, `cx/widgets`, `cx/data`, `cx/util`** (and `cx/charts`, `cx/svg`). The `cx`
  root has no exports.
- The entry calls `startHotAppLoop({ hot: import.meta.hot }, el, store, Routes)` **and**
  `import.meta.hot.accept()` — both, or every edit is a full reload.
- **The server serves the page** on `https://localhost:5443`; Vite on 8765 only serves modules, and in
  development a plugin writes the server a stub `index.html` pointing at it. Browse 5443, never 8765.
- `vite.config.ts` runs for `build` too: anything only the dev server needs (the certificate) is read
  when `command === "serve"`.

## Debugging a white screen

1. The error is in the **browser** console; Vite's terminal stays clean.
2. An unwrapped functional component — the message names the element's keys.
3. An import from a subpath that does not exist (`cx` root).
4. A prop that duck-types a config given an accessor (see above).
5. **A `controller` and a `visible` it sets on the same element deadlock**: `visible` starts false, the
   element never renders, `onInit` never runs. Put the controller outside, the gate inside.

## Other gotchas

- **Setting `input.value` programmatically does not reach the store.** A headless check clicks and
  types real keys and presses Tab; assigning `.value` and dispatching events leaves the binding
  untouched, so a working form looks broken.
- **`innerText` returns text as rendered** — `text-transform: uppercase` reads back uppercase. Match
  case-insensitively or assert on DOM structure.
- **`addTrigger` callbacks run after the commit that fired them**; a trigger that clears a message
  swallows one set in the same cycle.
- **A route's parameters are read through its record, `$route`**, declared in the screen's model
  like any alias (`$route: { id: string }`, then `m.$route.id`). Never through `Route`'s `params`: it
  reads `params.bind`, which an accessor answers with another accessor, so the values land at
  `<path>.bind`.
- **CxJS's `Link` never leaves the application** — it routes any local href through the client router.
  A link to a server endpoint (an OAuth start) is a plain `<a>`.
- **A layout is an imported widget, not a string**: `layout={{ type: "vbox" }}` throws
  `Invalid widget type` at render. Anything that simple is CSS.

## Verifying a screen

A CxJS screen can return 200 and paint nothing, which no build or type check catches. After a change,
load it in a real browser at phone width (390×844) and desktop, and assert on the **DOM** — the
element exists, the control measures 44px, `.cxs-error` appears on an empty required field — not on a
screenshot alone.
