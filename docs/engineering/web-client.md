# Web client

A CxJS single-page application in TypeScript, built by Vite into the server's `wwwroot`. One
deployable serves both, so there is no separate host and no CORS to configure.

## TypeScript and CxJS

`<cx>` blocks are compiled by Vite through CxJS's JSX runtime (`oxc.jsx.importSource: "cx"`), and
TypeScript only type-checks them:
`npm run typecheck` is a separate step, and CI runs it. **TypeScript takes its JSX types from cx**
(`"jsx": "react-jsx"`, `"jsxImportSource": "cx"`), which declares `<cx>` and gives HTML elements
CxJS's attributes — `class`, `text`, `visible`. Resolved through React's typings instead, every widget
is reported as not a valid JSX component and every `class` as a typo for `className`.

**Bindings are typed accessors, never strings.** Each screen's `model.ts` declares its state as an
interface and default-exports `createModel<Model>()`, imported as `m`; the view binds
`value={m.signin.email}` and the controller reads `this.store.get(m.signin.email)`, both checked. A
string path — `value-bind`, `visible-expr`, `text-tpl`, `bind()` — is unchecked, so a typo renders
blank, and it does not appear in the client. A condition prefers the direct binding, then a helper
(`truthy`, `falsy`, `equal`), then `expr(...accessors, fn)`, lifted out of the JSX and named when it
combines fields. The application-wide state — `url` and `session` — is `src/model.ts`, imported as
`$app`.

**Every `TextField` trims**, set once on the prototype in `src/widgetDefaults.ts` rather than per
field: a value of only spaces then becomes `null`, which `required` counts as empty. A text field's
store key starts absent, never `''` — `''` is a value, so `required` passes on it.

**A field whose store value may be `null` binds through an adapter** in `src/bindings.ts`:
`NumberProp` does not admit `null`, although the widget writes it on clear.

How CxJS code is written here — bindings, forms, windows, lists, styling, and the pitfalls behind
each — is the `cxjs` skill, `.claude/skills/cxjs/SKILL.md`. This file holds the decisions.

## Structure

One folder per screen, mirroring the URL: `index.tsx` is the markup, `Controller.ts` the behaviour,
`model.ts` the state's types and accessor. `model.ts` imports nothing from its folder, so the other two
can both import it. A screen is a `createFunctionalComponent`; the root is a `<cx>` element, because
`startHotAppLoop` takes configuration rather than a component.

**Text goes in `text=` on a self-closing element** wherever nothing else is inside it.
`src/api/*` is one module per resource, over `send` in `src/api/http.ts` — `fetch` with `credentials: "same-origin"` — the session
is a cookie, so nothing attaches a token by hand.

Routing is declarative and the first matching route wins, so order in the JSX is the routing table.
The outermost split is whether there is a session; everything below it can assume the answer.

**The session is resolved once, at the root.** Until it arrives the app shows neither the sign-in
screen nor the application, which is what stops a signed-in person seeing a sign-in form for a moment
on every load.

**`src/layout/navigation.ts` is both the menu and the routing table of the screens in it**: sections
and items as the original's menu has them, flat, with no collapsing. `~/` redirects to the first item —
there is no home screen, as in the original — and an unmatched URL shows a not-found page inside the
shell. `screens` in `routes/index.tsx` maps an item's href to its screen; an item without one routes to
`TodoScreen`, which names the programme step that builds it.

**Icons are HugeIcons' free set** (`@hugeicons/core-free-icons`, MIT), registered by name against cx's
`Icon` in `src/layout/registerIcons.tsx`; a view binds `<Icon name=… />`. One name per use, not per glyph.

## The phone is the hard case

**The shell is Pulse's.** From `lg` (1024px) a 260px sidebar holds the navigation and, at its foot,
the signed-in person alone; signing out is in the menu that opens above them. Below `lg` the sidebar
becomes a drawer over the content, as wide and never closer than 56px to the far edge. The drawer is
opened by a 44px menu button in a top
bar that carries only the mark. Any tap in the drawer closes it, except one that opens or works the account menu.
**The document scrolls, not the content column**: iPhone Safari collapses its toolbars only when the
document does, so a scrolling `main` keeps the address bar on screen for good. The desktop sidebar and
the phone's top bar are sticky; the top bar's height is `--shell-top`, 0 from `lg`, and anything else
that sticks sits beneath it. **Whatever covers the page locks it** — the open drawer and every modal window — by
refusing gestures: `lockScroll()` in `src/scrollLock.ts` lets a touch or wheel scroll through only
inside a scrollable element of the overlay, or of a cx window or dropdown over it, while it can still
move. The page stays scrollable underneath, so Safari's toolbar keeps its state. Modal windows get it
from `widgetDefaults.ts`, hooked into cx's `overlayDidMount`, so no screen has to ask; the drawer's
list also does not pass its scroll on (`overscroll-behavior: contain`). A screen fills
the content column under a header band (`.page-header`) flush with its top and sides; it never sets
its own outer padding.
Sign-in, outside the shell, is one centred column that stops growing on a wide display.

**Input text is 16px on a touch screen**, 14px elsewhere: iPhone Safari zooms into a focused field
whose text is smaller, and the page stays zoomed after it. **Double-tap zoom is off**
(`touch-action: manipulation`); **pinch zoom is not** — it is how small text is read by those who need
it larger, blocking it fails WCAG's resize-text criterion, and iPhone Safari ignores the block anyway.

Tap targets are at least 44px high wherever the layout is a phone's — the drawer's links included; the
desktop sidebar keeps Pulse's denser rows. The page padding respects `env(safe-area-inset-bottom)`.

## Lists

**One search box, and every other filter in a pane it drops open.** The box is free text, run after a
300ms pause; the *Filters* button beside it carries the active count and opens the pane beneath the bar,
pushing the list down rather than covering it. Every active filter shows as a removable chip under the
bar, so closing the pane hides nothing that is filtering. Filters apply as they change; the pane's
*Done* only closes it.

**A picker searches from seven options**, cx's default, and its list stops at `min(16rem, 40dvh)`
and scrolls inside it.

**The list is one markup at both widths**: a stacked card on a phone, a row of columns with a header
from `md`, laid out by CSS grid areas. Not a `Grid` for desktop beside cards for the phone — two
renderings of every row, drifting apart.

**The toolbar is one row, pinned** — search, *Filters* and a compact previous/next, with the range
beside them from `md` — so nothing needs a scroll back up, and it costs a phone one 44px row. Below
`md` the range is a caption above the list that scrolls away. The order is set in the pane with the
filters and shows as a chip when it is not the default: it changes rarely, and a second line of controls
on every list is too much height to spend on it. `stickyBar()` in `src/stickyBar.ts`
pins it beneath `--shell-top` and publishes its height as `--sticky-bar-height`, which headings in the
list stick beneath. Below `md` it slides away while scrolling down and returns on the first scroll up,
since a bar pinned for good takes a fifth of a phone's screen. With the filter pane open it is not
pinned: the pane is too tall to hold on screen.

**`components/Pager`** also sits under every list, driven by `pager()` in `src/paging.ts`: the range and
the total, previous and next, and from `sm` the first, last and current page with a neighbour each side.
A phone gets "3 / 40" in place of the links. Paging scrolls the page back to the top. In the bar the
pager is compact — bare chevrons drawn at 32px, touched at 44 — and the line it sits on is small type
without borders, so it reads as a caption under the search rather than a second toolbar. Not
infinite scroll: it loses the reader's place and cannot reach page 40 without loading 39.

**Only the latest request writes.** A controller numbers its requests and drops any answer that is not
the newest, or a slow early answer lands over a later one.

**An entry opens in a window, and on a phone the window is the whole screen.** Back closes it:
`historyEntry()` gives the window an entry in the browser history at the same URL, and steps back over
it when the window closes any other way. `dismissOnPopState` alone only closes the window — the Back
that closed it has already left the screen.

## Dates

**British English for every date cx shows** — `Culture.setCulture("en-GB")` and weeks from Monday,
installed with the date encoding by `installDateCulture` in `src/dates.ts`. A `DateField` stores
`YYYY-MM-DD`, bound through `dateValue` in `src/bindings.ts`. A day filter becomes the viewer's own
midnights when the query is built, the end one day on, because the server's `to` is exclusive.

## Theme

**Light only**, on Codaxy's house palette as Pulse uses it: its token names, its primary and
Montserrat, self-hosted through `@fontsource` so no page load reaches a font CDN. A dark mode would be a
second value set for the same tokens.

**The sidebar and top bar are dark chrome with tokens of their own**, `nav-*`: two navy tones so the
two read as a frame, and the active item a solid primary fill rather than a wash. The `ink`, `line` and
`hover` tokens are tuned for white and fail on navy, so nothing in the chrome uses them.

**The logo tile is violet**, `brand`, wherever it appears: the chrome is Pulse's, and the mark is what
tells the two applications apart at a glance.

**Every colour and shadow is a token in `src/tailwind.css`**, in `@theme static`, and nothing else in the client
writes one. Each text token clears AA (4.5:1) on both the card and the page; field and button borders
clear 3:1 on the card. The house values for `ink-faint`, `line-strong` and `warn` fail that, so those values are
darker here. **Text in a status colour uses its `-text` token**, which equals the fill where the fill
passes and is darker where it does not: `warn` is 3.4:1 as text, `warn-text` 5.1:1. A status's `-wash` is a
background its `-text` clears 4.5:1 on; `-mark` highlights the words a change touched, under `ink`.

**Two layers, in this order.** `src/theme.ts` maps CxJS's theme variables onto the tokens and is
applied by `renderThemeVariables` at startup — colours, type and sizes of widgets belong there. The
full list of variables is `cx-theme-variables/build/presets/default.js`; the docs do not have it. Then
one partial per component in `src/scss/`, for what no variable expresses. The theme's SCSS and ours
load inside `@layer components`, so a Tailwind utility in markup wins over both.

**An invalid field is its red border**, never a tinted fill: CxJS's pink wash is overridden in
`src/scss/_fields.scss`. A message beneath it is for what the field cannot show — the server
refusing the address — never for "required" or "not a valid address", which say what the red
border and an empty field already do. Sign-in's button is disabled until the address is
well-formed, and empty is not an error.

**Controls are 44px by padding, not by a density preset.** The largest preset stops at 40px, so
`theme.ts` takes `densityComfortable`'s 24px line and sets 9px vertical padding on inputs and buttons.
`densityCompact` (32px) is desktop sizing and wrong here.

## Build

`npm run build` is `vite build`: hashed bundles and an `index.html` naming them in `client/dist`,
which the image copies into the server's `wwwroot`.

**The application is always served by the server, on its own origin**, in development as in
production. In development a plugin in `vite.config.ts` writes the server a copy of `index.html`
whose scripts point at the dev server on `https://localhost:8765`; the modules stay in its memory,
so the page comes from the server and the scripts from Vite.

**Both are served over TLS in development**, with the ASP.NET development certificate that `npm
start` exports for Vite. One certificate, so one thing to trust. It is not decoration: cookie
attributes depend on the scheme, so an http development loop exercises different rules from
production and the difference surfaces as a failure in one browser and not another. An https page
cannot load scripts over http either, so the dev server has no choice once the server has one.

That is the reason for the arrangement rather than the browser being handed to Vite: the session
cookie is issued for, and confined to, the origin that serves the application. Point the browser at
the dev server instead and the cookie belongs to a node process, the origin differs from production,
and the dev server's own exposure becomes part of the authenticated surface. **`server.origin` and
`cors`** exist because of it: asset URLs have to be absolute, and every module request crosses from
the server's origin to Vite's.

**Hot replacement is Vite's plus `startHotAppLoop`**, which swaps the running application and keeps
the store and the current route rather than reloading the page. The entry passes
`{ hot: import.meta.hot }` and also calls `import.meta.hot.accept()` itself; without the second,
every edit is a full reload and a lost session.

## No Razor shell

`index.html` is written by the build and served as a static file. Razor earns its place when the HTML
has to be built per request — resolving hashed bundle names, choosing between development and
production script URLs, or passing a server-side value into the page — and none of those apply: the
build writes the names, the development plugin decides the origin, and what the client needs to know it asks
`/api/auth/options` for.

**A Content-Security-Policy with a nonce would change that**, since a nonce is new per request and
must appear in both the header and every script tag. Script hashes in the header are the alternative
that a static shell supports. Worth deciding before there are screens, not after.

## Traps

**A component used inside `<cx>` must be a `createFunctionalComponent`.** A bare arrow function is
handed to React as a React component, returns CxJS configuration, and the application white-screens
with `Objects are not valid as a React child`.

**A validation problem's `title` says nothing.** ASP.NET's is always "One or more validation errors
occurred."; `src/api/` shows the first message in `errors`, so a request model's `ErrorMessage` is
text a person reads.

**A code is text, not a number.** A `NumberField` groups digits by culture — `665,355` — and
drops a leading zero; a `TextField` with `inputMode: "numeric"` and `autoComplete: "one-time-code"`
keeps both and lets the phone offer the code from the message.

**`ValidationGroup` renders no element of its own**, so a `class` on it styles nothing. Wrap it in a
`div` for layout.

**CxJS's `Link` never leaves the application.** It calls `preventDefault` and pushes the href through
the client router for any local URL, so a link to a server endpoint — starting an OAuth flow, say —
routes to a page that does not exist and lands back where it started, with no request made. A plain
anchor is what leaves.

**`Link`'s `match` is widget configuration, not a bindable prop**, so a `Repeater` cannot vary it per
item. The menu is static, so it is built in JSX from `navigation.ts`, and an item whose href prefixes a
sibling's (`~/furniture`, `~/furniture/types`) matches `equal` where the rest match `subroute`.

**A CxJS layout is an imported widget, not a string.** `layout={{ type: "vbox" }}` compiles, reaches
the browser, and throws `Invalid widget type` at render — the screen is simply blank. Anything this
simple belongs in CSS anyway.

**cx's `center` places a window once, for the height it opens at.** A window that grows afterwards
runs off the bottom of the screen and scrolls the page behind it. A window whose content changes height
is centred by CSS instead — fixed, translated by half, capped at the viewport — so only its body
scrolls.

**A cx dropdown closes once its field moves 50px**, and with the document scrolling, Safari moves it
further than that whenever it lifts a focused search box above the keyboard — the list closed the
moment one tapped into it. `widgetDefaults.ts` sets `closeOnScrollDistance` to `Infinity`: a dropdown
follows its field instead, and a tap outside or Escape still closes it.

**Neither `overflow: hidden` nor a fixed body is a scroll lock on an iPhone.** `overflow: hidden` jumps
the page to the top and Safari still scrolls it on a drag; a body fixed at its offset holds, but makes
the document unscrollable, and Safari expands its collapsed toolbar in answer. Refuse the gestures
instead.

**A stuck element's `offsetTop` moves with it.** Asking whether the reader has scrolled past a sticky
bar by comparing against its `offsetTop` is never true once it sticks; measure its resting position
once, before it does.

**A token used only from SCSS needs `@theme static`.** A plain `@theme` emits only the variables some
utility references, so `var(--color-…)` in a partial resolves to nothing — a transparent background,
not an error.

**No `@apply` in a `.scss` file.** Sass compiles before Tailwind sees it, so `@apply` reaches the
browser verbatim and is ignored without a warning.

**The theme's variable sheet is injected at runtime, so it wins specificity ties** with anything
bundled, and its selectors are often two classes deep: `.cxb-button.cxm-hollow` beats
`.my-button`. Set the variable; where a selector is unavoidable, read the real rule from
`cx-theme-variables/dist/widgets.css` rather than guessing.

**`padding` cannot resize a `Button`.** `.cxb-button` sets an explicit height from its own line height,
padding and border variables; change those in `theme.ts`.

**Only `index.html` is written to `wwwroot` in development.** A build landing there leaves bundles
the server serves in place of Vite's, and an edit then appears to do nothing.

**The ports are stated in two places that do not know about each other.** 8765 is `DEV_SERVER` and
`server.port` in `vite.config.ts`; 5443 is `applicationUrl` in `launchSettings.json` and what the
browser is told to open. Change one half and the failure is silent — the page loads from the server
and asks for modules nobody is serving.

**A shell written in development points at `https://localhost:8765`.** Running the server in
Production against that same `wwwroot` serves a page asking for a dev server that is not there; build
into `dist` and let the image copy it.

**`vite.config.ts` is evaluated in both modes**, so the certificate is read only when `command` is
`serve`. Read unconditionally, it fails `npm run build` wherever `npm start` never exported it — CI
and the image.

**`dotnet dev-certs https --export-path` will not create the folder it exports into**, so `prestart`
creates `.certs` first. Without it `npm start` fails on every fresh clone and works on any machine
where the folder once existed.
