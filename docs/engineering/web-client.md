# Web client

A CxJS single-page application in TypeScript, built by webpack into the server's `wwwroot`. One
deployable serves both, so there is no separate host and no CORS to configure.

## TypeScript and CxJS

CxJS ships its own typings, and `<cx>` blocks are compiled by `babel-preset-cx-env` rather than by
TypeScript. Two consequences:

- **`tsc` only type-checks; babel emits.** `npm run typecheck` is a separate step, and CI runs it.
- **The `<cx>` element is declared in `src/jsx.d.ts`, as an augmentation of React's JSX namespace**,
  not a global one. React's typings are what TypeScript resolves `JSX` from — `cx-react` pulls them
  in — and an augmentation of the global namespace does not merge with them. A `Property 'cx' does
  not exist` error means that file stopped being included.

Markup inside a `<cx>` block is not meaningfully type-checked: attributes are binding strings. The
types earn their place in controllers and the API layer, which is where the logic is.

## Structure

One folder per screen, mirroring the URL: `index.tsx` is the markup, `Controller.ts` the behaviour.
`src/api/*` is one module per resource, over `fetch` with `credentials: "same-origin"` — the session
is a cookie, so nothing attaches a token by hand.

Routing is declarative and the first matching route wins, so order in the JSX is the routing table.
The outermost split is whether there is a session; everything below it can assume the answer.

**The session is resolved once, at the root.** Until it arrives the app shows neither the sign-in
screen nor the application, which is what stops a signed-in person seeing a sign-in form for a moment
on every load.

## The phone is the hard case

Every screen is a single column that stops growing on a wide display, rather than a wide layout
squeezed into a narrow one. Tap targets are at least 44px high, which the default widget sizing does
not guarantee, and the page padding respects `env(safe-area-inset-bottom)`.

## Build

`npm run build` writes hashed bundles into `client/dist`, which the image copies into the server's
`wwwroot`.

**The application is always served by the server, on its own origin**, in development as in
production. In development the watcher writes only the shell to `wwwroot` — bundles stay in its
memory and the shell's `publicPath` points at `https://localhost:8765/` — so the page comes from the
server and the scripts come from the watcher.

**Both are served over TLS in development**, with the ASP.NET development certificate that `npm
start` exports for the watcher. One certificate, so one thing to trust. It is not decoration: cookie
attributes depend on the scheme, so an http development loop exercises different rules from
production and the difference surfaces as a failure in one browser and not another. An https page
cannot load scripts over http either, so the watcher has no choice once the server has one.

That is the reason for the arrangement rather than the browser being handed to the dev server: the
session cookie is issued for, and confined to, the origin that serves the application. Point the
browser at the dev server instead and the cookie belongs to a node process, the origin differs from
production, and the dev server's own exposure becomes part of the authenticated surface.

**Hot replacement is `devServer.hot` plus `startHotAppLoop`**, which swaps the running application and
keeps the store and the current route rather than reloading the page. Losing either half turns every
edit into a full reload and a lost session.

## No Razor shell

`index.html` is written by the build and served as a static file. Razor earns its place when the HTML
has to be built per request — resolving hashed bundle names, choosing between development and
production script URLs, or passing a server-side value into the page — and none of those apply: the
plugin writes the names, `publicPath` decides the origin, and what the client needs to know it asks
`/api/auth/options` for.

**A Content-Security-Policy with a nonce would change that**, since a nonce is new per request and
must appear in both the header and every script tag. Script hashes in the header are the alternative
that a static shell supports. Worth deciding before there are screens, not after.

## Traps

**CxJS's `Link` never leaves the application.** It calls `preventDefault` and pushes the href through
the client router for any local URL, so a link to a server endpoint — starting an OAuth flow, say —
routes to a page that does not exist and lands back where it started, with no request made. A plain
anchor is what leaves.

**A CxJS layout is an imported widget, not a string.** `layout={{ type: "vbox" }}` compiles, reaches
the browser, and throws `Invalid widget type` at render — the screen is simply blank. Anything this
simple belongs in CSS anyway.

**The theme ships plain `.css` as well as the `.scss` written here**, so the build needs a rule for
both. With only the `.scss` rule the build still succeeds and the theme is quietly missing.

**Only `index.html` is written to `wwwroot` in development**, through `devMiddleware.writeToDisk`.
Letting the whole build land there leaves bundles the server will serve in place of the watcher's,
and an edit then appears to do nothing.

**Both ports are stated twice, in files that do not know about each other.** 8765 is
`devServer.port` and the `publicPath` in `webpack.config.js`; 5443 is `applicationUrl` in
`launchSettings.json` and what the browser is told to open. Change one half and the failure is
silent — the page loads from the server and asks for bundles nobody is serving.

**A shell written in development points at `https://localhost:8765`.** Running the server in
Production against that same `wwwroot` serves a page asking for a watcher that is not there; build
into `dist` and let the image copy it.
