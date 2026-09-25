# 0003 — Vite, and the CxJS skill

Move the client's build to Vite, the bundler Codaxy's CxJS template and `cx-pulse` use, and give
agents a CxJS skill adapted from Pulse's. Each step is a branch and a local plan of its own.

## Why

**Webpack and babel are the last of the old toolchain.** Vite 8 compiles CxJS's JSX itself, so
babel, the loaders and the PostCSS configuration go, and the build is the one the rest of Codaxy's
CxJS code is written against.

**The CxJS pitfalls are learned once or paid for repeatedly.** Pulse's skill holds the ones that cost
time there — typed accessors as labels, the empty-field rule, windows and stores, lists — and most
hold here unchanged.

## Decided

- **The server keeps serving the page from a static `index.html`** — no Razor shell, unlike Pulse.
  Vite writes the production page into `dist`; in development a plugin writes a stub into `wwwroot`
  that loads from the dev server, as webpack's `writeToDisk` does now.
- **Ports, TLS and the certificate stay**: 5443 and 8765, the ASP.NET development certificate, read
  only when serving.
- **The skill is committed**, at `.claude/skills/cxjs/`, and `CLAUDE.md` says to load it before
  touching `client/`. It is adapted, not copied: Pulse's decision numbers, grids, diagrams, icons and
  sign-in harness do not come across.
- **`widgetDefaults.ts` comes across; `dates.ts` and the smoke harness wait** for the first date field
  and for more than one screen.

## The steps

1. ~~**Vite.**~~ Webpack, babel and PostCSS out; Vite, `@tailwindcss/vite` in; the development stub;
   `web-client.md`'s build section and traps rewritten. Nothing renders differently.
2. **The skill.** `.claude/skills/cxjs/SKILL.md`, the `.gitignore` exception for it, the `CLAUDE.md`
   line, and `widgetDefaults.ts`.

## Why that order

**Vite first** so the skill describes the build that exists rather than one about to be replaced.
