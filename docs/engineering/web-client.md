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

**A field whose store value may be `null` binds through an adapter** in `src/bindings.ts`:
`NumberProp` does not admit `null`, although the widget writes it on clear.

## Structure

One folder per screen, mirroring the URL: `index.tsx` is the markup, `Controller.ts` the behaviour,
`model.ts` the state's types and accessor. `model.ts` imports nothing from its folder, so the other two
can both import it. A screen is a `createFunctionalComponent`; the root is a `<cx>` element, because
`startHotAppLoop` takes configuration rather than a component.

**Text goes in `text=` on a self-closing element** wherever nothing else is inside it.
`src/api/*` is one module per resource, over `fetch` with `credentials: "same-origin"` — the session
is a cookie, so nothing attaches a token by hand.

Routing is declarative and the first matching route wins, so order in the JSX is the routing table.
The outermost split is whether there is a session; everything below it can assume the answer.

**The session is resolved once, at the root.** Until it arrives the app shows neither the sign-in
screen nor the application, which is what stops a signed-in person seeing a sign-in form for a moment
on every load.

## The phone is the hard case

Every screen is a single column that stops growing on a wide display, rather than a wide layout
squeezed into a narrow one. Tap targets are at least 44px high, and the page padding respects
`env(safe-area-inset-bottom)`.

## Theme

**Dark only**, from Pulse (`cx-pulse`): its navy chrome extended into a full token set under its token
names, its primary and Montserrat, self-hosted through `@fontsource` so no page load reaches a font CDN.
A light mode would be a second value set for the same tokens.

**Every colour and shadow is a token in `src/tailwind.css`**, in `@theme static`, and nothing else in the client
writes one. Each text token clears AA (4.5:1) on both the card and the page; field and button borders
clear 3:1. **A colour that fails as text on navy gets a `-text` sibling** rather than a darker page:
`primary` is a fill that white reads on at 5.7:1 but is 2.9:1 as text, so links and focus use
`primary-text`; `danger` likewise.

**Two layers, in this order.** `src/theme.ts` maps CxJS's theme variables onto the tokens and is
applied by `renderThemeVariables` at startup — colours, type and sizes of widgets belong there. The
full list of variables is `cx-theme-variables/build/presets/default.js`; the docs do not have it. Then
one partial per component in `src/scss/`, for what no variable expresses. The theme's SCSS and ours
load inside `@layer components`, so a Tailwind utility in markup wins over both.

**Controls are 44px by padding, not by a density preset.** The largest preset stops at 40px, so
`theme.ts` takes `densityComfortable`'s 24px line and sets 9px vertical padding on inputs and buttons.
Pulse's `densityCompact` (32px) is desktop sizing and wrong here.

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

**`ValidationGroup` renders no element of its own**, so a `class` on it styles nothing. Wrap it in a
`div` for layout.

**CxJS's `Link` never leaves the application.** It calls `preventDefault` and pushes the href through
the client router for any local URL, so a link to a server endpoint — starting an OAuth flow, say —
routes to a page that does not exist and lands back where it started, with no request made. A plain
anchor is what leaves.

**A CxJS layout is an imported widget, not a string.** `layout={{ type: "vbox" }}` compiles, reaches
the browser, and throws `Invalid widget type` at render — the screen is simply blank. Anything this
simple belongs in CSS anyway.

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
