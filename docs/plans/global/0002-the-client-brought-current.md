# 0002 — The client, brought current

Bring the client up to the CxJS the rest of Codaxy writes, with `Pulse.WebClient` in `cx-pulse` as the
reference, while it has one screen. Each step is a branch and a local plan of its own. It runs inside
step 1 of [0001](0001-the-new-application.md): the skeleton is what every later screen copies, so this
lands before a second screen exists.

## Why

**Bindings are string paths** — `value-bind`, `visible-expr`, `text-tpl` — and a string is unchecked:
a typo renders blank and compiles. cx 26.8 ships typed accessor models and calls the string API
legacy.

**The theme is Aquamarine 18**, a fixed stylesheet restyled by out-guessing its selectors.
`cx-theme-variables` sets the same things through variables mapped onto design tokens, once.

## Decided

- **Dark only, built from Pulse.** Pulse's navy chrome (`#0e1424`, `#151d33`, its `nav-ink` greys)
  extended into a full token set under Pulse's token names; its primary and Montserrat, self-hosted
  through `@fontsource`. Where a Pulse colour fails AA on navy as text — the primary, the danger red —
  a lighter `-text` token sits beside it. A light mode would be a second token set, not a rewrite.
- **Pulse's `densityCompact` is not adopted.** It sizes controls at 32px for a desktop application;
  here a tap target is at least 44px.
- **Tailwind 4 is adopted**, through PostCSS under webpack. The tokens live in its `@theme`.
- **Vite is out of scope.** The webpack loop in [web-client.md](../../engineering/web-client.md)
  stays.
- **Pulse's rules are taken where they apply and restated in `web-client.md`**, not copied: its
  CxJS skill carries patterns that are Pulse's alone.

## The steps

1. ~~**CxJS 26 and typed models.**~~ `cx`, `cx-react` and the babel preset to 26; a `model.ts` per screen
   exporting `createModel<Model>()`; typed controllers; `createFunctionalComponent`; no string binding
   left. Every screen renders as it did.
2. **The theme.** `cx-theme-variables` in place of Aquamarine, `theme.ts` mapping its variables onto
   the dark tokens, Tailwind 4, Montserrat, and the sign-in screen rebuilt on them — no colour written
   outside the token set.

## Why that order

**Typed models first** because they change no behaviour and no pixel: a regression in that step is a
binding, never a colour. **The theme second**, so the restyle is written once, in the binding form
that stays.
