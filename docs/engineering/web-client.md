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
and items as the original's menu has them, flat, with no collapsing — except that its directory is
**Company**, and first: the people and organisations everything else is assigned to. `~/` lands on the
**Dashboard**, the first item and in no section. An unmatched URL shows a not-found page inside the
shell. `screens` in `routes/index.tsx` maps an item's href to its screen; an item without one routes
to `TodoScreen`, which names the programme step that builds it.

**The Dashboard is one screen**, after Pulse's home: a lede, one card "Worth a look" holding a tile
per rule in [domain.md](domain.md) — its count and what it counts — and beneath it the selected tile's
rows as `holdingSections`, restyled to the card: an uppercase label with the count bold beside it,
rows edge to edge. Tiles are divided by hairlines and tinted on hover; the selected one keeps the tint
with the accent along its foot, and the first with anything is selected. An empty tile is dimmed and
inert. Red for over-allocated seats and seats on disposed devices, wrong whenever there are any; amber
for what ends soon; ink for the rest, whose weight depends. Two tiles to a row on a phone, half the
count from 34rem, never all in one — that reads as a strip — so the page is `page-narrow`. From 34rem
a tile stacks label, 19px bold number and line as subgrid rows of its row, so numbers align whichever
label wraps, and label and line each hold two lines, so every row of tiles is as tall as the fullest —
never a stubby row under a full one. On a phone a tile is label and medium-weight number on one line:
the line goes, the list's title carries the window, the header goes, the lede says it, and the gutter
is 1rem — so the list starts on the first screen.

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
that sticks sits beneath it. **Above the page is the chrome's navy, below it the page colour**: a bounce past the top
shows the root's background, the top bar's navy on a phone and the sidebar's from `lg`, so the header
stretches rather than splitting from the status bar; past the bottom, navy read as a stray band, so on the
page's last screen the root takes the page colour (`page-end`, set by `pageEnd.ts` from the shell's
size and the scroll). The shell carries the page colour itself, so the root shows nowhere else. The root alone is not enough: Safari painted the page colour above the bar when a
fast scroll hit the top, so the bar also carries a screen of its navy above itself, which moves with it. **Whatever covers the page locks it** — the open drawer and every modal window — by
refusing gestures: `lockScroll()` in `src/scrollLock.ts` lets a touch or wheel scroll through only
inside a scrollable element of the overlay, or of a cx window or dropdown over it, while it can still
move. The page stays scrollable underneath, so Safari's toolbar keeps its state. Modal windows get it
from `widgetDefaults.ts`, hooked into cx's `overlayDidMount`, so no screen has to ask; the drawer's
list also does not pass its scroll on (`overscroll-behavior: contain`, below `lg` only — the desktop
sidebar would swallow the wheel). A screen fills
the content column under a header band (`.page-header`) flush with its top and sides; it never sets
its own outer padding. **The page's head is pinned on every page** beneath `--shell-top` wherever the
document scrolls, on a phone as on the desktop: one block, `page-top`, opens every screen — the header
band, and on a list its toolbar and chips. The block spans the page's padding, so nothing in it pulls
into the padding with negative margins; what sticks beneath it reads its height from
`--page-top-height` (`pageTop()`).

**A page's width is set by its kind, not by the display** — `page-wide` (96rem, Tailwind's `2xl`) for
lists and logs, `page-narrow` (52rem) for a record's page, in `_shell.scss`. Header band, pinned bars
and content share the one left-aligned column, so the search is never wider than what it searches;
a list's columns are capped so a spare width stays in the page, not between the cells. Past the column
the page fills the viewport's height with the primary glow and dot grid of sign-in, fading in from
its left: a bare canvas reads as unfinished. The server log follows the same rule — no pane is special.
Sign-in, outside the shell, is one centred column that stops growing on a wide display. Its footer
is "Built by" and Codaxy's wordmark on the text's baseline — the letters stand at 67 of its 80, so
it is shifted down, not centred.

**Input text is 16px on a touch screen**, 14px elsewhere: iPhone Safari zooms into a focused field
whose text is smaller, and the page stays zoomed after it. **Double-tap zoom is off**
(`touch-action: manipulation`); **pinch zoom is not** — it is how small text is read by those who need
it larger, blocking it fails WCAG's resize-text criterion, and iPhone Safari ignores the block anyway.

Tap targets are at least 44px high on a touch screen — the drawer's links included; the
desktop sidebar keeps Pulse's denser rows. The page padding respects `env(safe-area-inset-bottom)`.

## Lists

**One search box, and every other filter in a pane it drops open** — or inline, where a screen has a
single filter: the server log's level switch sits beside its day strip, since a pane for one switch is
a card of empty space. The box is free text, run after a
300ms pause; the *Filters* button beside it carries the active count and opens the pane beneath the pinned block,
scrolling with the page and pushing the list down rather than covering it. Every active filter shows as a removable chip under the
bar, pinned with it, so closing the pane hides nothing that is filtering. Filters apply as they change; the pane's
*Done* only closes it. **A switch in the pane has its row to itself at every width** (`list-filter-wide`):
sharing it, a picker slides up beside the switch on a wide screen, and the pane reads in a different
order from one width to the next.

**A picker searches from seven options**, cx's default, and cx sizes its list to the room on screen
and scrolls it.

**The toolbar's parts are shared** — `list-*` in `_list.scss`: the pinned block and its heading, the bar,
search, Filters and its pane, chips, and the error and empty states. A screen's rows are its own. **A search for an id that matches
nothing says "No record has this id"**, without the usual advice to try fewer words, which does not
apply to an id; any other search keeps its list's own wording. `ListController` knows which it ran
(`idSearch`).

**A screen loads as its own outline**: its real layout with a `skeleton-bar` for each text
(`_skeleton.scss`), so the data arrives without anything moving — shown at once, since there is no
flash to hide. A list's first load is `listSkeleton()`: eight rows in the list's columns under its
real head, one bar per cell, a phone's row its first two. The dashboard and the audit log outline
their own layouts. **A refetch keeps the rows it has, dimmed**, never the outline. The bars shimmer
slowly, and stand still under reduced motion.

**A cell says only what its column's header does not**: "6 Aug 2024" under Deactivated, not
"Deactivated 6 Aug 2024" — but a phone's card, which has no headers, keeps the word.

**A quantity is right-aligned, a label that is a number is not**: counts, seats and money — and their
headers — sit right in tabular figures from `md` (`record-num`; `countCell` carries it, a `searchList`
column says `numeric`), so a column reads by size. Inventory, invoice, serial and phone numbers stay
left: nobody compares their size.

**The list is one markup at both widths**: a stacked card on a phone, a row of columns with a header
from `md`, laid out by CSS grid areas. Not a `Grid` for desktop beside cards for the phone — two
renderings of every row, drifting apart. **A wide list's columns are shares (`fr`), not fixed widths**:
fixed widths that overflow the page shrink every column alike, and the name — which needs the room —
is cut first. Where the columns do not fit below `xl`, the least scanned go until then — furniture's
vendor and change time; a device's manufacturer, model code, serial number and change time — and
wait for the record's page.

**Every list fills its page** (`list-fill`) — the lists of records and both logs. Where its columns
show, the document holds still: the rows scroll inside the card — the server log's terminal — under
its pinned column header, and the pager stays at the window's foot. A card squeezed under 10rem in a
short window lets the document scroll after all rather than hide the rows. Below `md` the document
scrolls, for Safari's toolbars. **With the filter pane open the page scrolls at every width**, as a
phone's does: the pane pushes the list down, and in a page of fixed height that pushes the rows off
it. Not a pane covering the rows instead: it needs capping to the room left, which moves as chips
wrap, and a scrim to keep a tap from opening a record behind it.

**A list's pinned block holds its header band, toolbar and chips**; the pane opens beneath it,
since held on screen it would be too tall. The band carries the title and, at its far end, the list's actions (`listHeading()`) — Excel, Add, the server
log's Refresh — as a record's page carries its own, icons only on a phone; the toolbar is search and
*Filters*. On a phone it stays rather than sliding away: it is the whole of the list's controls; the
audit log's day headings stick beneath it. **There is no caption line**: the pager under the list gives the
range, the total and the steps — on a phone only at the list's end, which is the price of a toolbar
that never moves. **The order is a column header's** (`sortHeader`), the audit log's Time included; a
phone, which shows no header, keeps the default or the address's.

**`components/Pager`** sits under every list, driven by `pager()` in `src/paging.ts`: the range and
the total, previous and next, and from `sm` the first, last and current page with a neighbour each side.
A phone gets "3 / 40" in place of the links. Paging scrolls the page back to the top, and a list's
rows inside their card (`listPaging()`). **A list shows 20 per page** (`pageSize` in `paging.ts`), so
a page fits a 1440px-tall display without scrolling; with Windows scaling at 125% it still scrolls.
The server log's lines are denser, and it shows 25, always newest first: a log is read from what just
happened, so it offers no order. **The pager is a caption, not a toolbar**: steps and page numbers
are text drawn at 32px and touched at 44, with no border or fill; the current page is a small pill of
the accent, as the menu marks where the reader is. It goes when nothing matches. Not infinite scroll:
it loses the reader's place and cannot reach page 40 without loading 39.

**Every list is deep-linkable: its whole state is in the address** — the search, every filter, the
sort and the page, as query parameters named as the API names them, defaults left out, so a plain list
is a plain URL and any view of one can be linked, bookmarked, reloaded and returned to. A picker's
filter travels as its id, never its name, which is looked up once the options arrive; a typed filter
travels as typed. The list reads the address when it opens and whenever the address changes under it
— a link to the same list, filtered otherwise — and writes it after every change, the search after
its pause, **replacing the history entry, never adding one**: Back leaves the list rather than
stepping through every filter. A record's back link, Cancel, and the return after saving a new record
or deleting one go to the list as it was left (`listReturn`). **Filters nested in one another stay consistent**: a narrower one the broader contradicts is cleared —
a volume when a license or software it does not belong to is chosen, or arrives in the address — and
its picker lists only what the broader ones allow. Filters side by side that can exclude each
other — a license and a software, one covering the other when the license has a volume of it — keep
whichever was changed last and clear the other, so neither picker has to be narrowed to reach a
choice outside the other. Nothing is locked or filled in: the narrower filter's chip already names
what it implies. Links into a list use the same parameters and **name the narrowest record they mean** — a volume's
activations link by `volumeId`, not by its license and software, which a license of five Rider volumes
shares five ways — and say how many they lead to ("2 activations"). A form opened on a choice the address already makes — `activations/new?volumeId=…` — shows it,
and what follows from it, as text rather than asking again: the volume and its software fixed, and
the next field the one its type calls for, a user or a device. The activations list filtered to one volume
opens such a form from its Activate button too. Such a form returns where it was started: from a volume on
its license's page (`from=license`) or its software's (`from=software`), its back link names that page
and Cancel and a save go back to it; from the list, to the list as it was left. A volume always offers its shortcut, and says
what it leads to: "Activate" while a seat is free, "Over-activate" once none is — past the quantity a
seat is allowed with a warning, not refused. `ListController` in `src/listController.ts` holds all of this
with the search's pause, the chips, the sort and the latest-request rule; a list declares its path,
its filters to and from the address, and its fetch. Not a history entry per change: Back would step
through every filter click before leaving.

**A list that has a spreadsheet offers it in its header band** — "Excel", before Add — as a
plain anchor to the export of its current request, filters and all: cx's `Link` would route it inside
the application instead of downloading.

**Only the latest request writes.** A controller numbers its requests and drops any answer that is not
the newest, or a slow early answer lands over a later one.

**An entry opens in a window, and on a phone the window is the whole screen.** Back closes it:
`historyEntry()` gives the window an entry in the browser history at the same URL, and steps back over
it when the window closes any other way. `dismissOnPopState` alone only closes the window — the Back
that closed it has already left the screen.

## Editors

**Every entity has a page of its own**, never a window, **and a row opens it read-only**:
`~/<item>/:id` shows the record, **its primary action in the header beside its name and the rest behind
a ⋮** (`moreActions` in `components/`, a borderless square button of the controls' height with a hover tint and a focus ring): Edit visible — or Deactivate, for a record never edited — and
View history, Duplicate and Delete a click deeper, Delete last and red, so what destroys is never one mis-click
away; icons only on a phone, named for screen readers; editing is `~/<item>/:id/edit`, Cancel and Save in the card's footer; `new` opens in editing, there being nothing to show yet. A
record's actions go where the eye starts, a form's commit where the form finishes. Not everyone will be allowed to edit, and
a record should not change because someone clicked into it. Cancel and a successful Save of an edit
return to the read-only page; a new record's Save and Cancel return to the list. Both modes are one form, switched by the `ValidationGroup`'s `viewMode`, which every
field inside it follows. The routes come after the menu's own, since a menu item's href can share the
prefix.

The page is the header band with a back link — blue at weight 500, light enough that the name leads; grey read as
disabled — 44px to a
finger by padding its margin takes back — and the record's name — as tall in every mode as with
its actions, so switching mode never moves the page — and the form as a card in the narrow column. While editing, the card ends in a footer — the card's own white under a rule,
buttons at the end; tinted, it takes the page's colour and reads as a hole in the card — sticky at the viewport's foot, so a long form keeps Save in reach and a short one does not
float a full-width bar over empty canvas. **A form of several cards commits in a bar of its own
below the last** (`editor-actions-bar`): a footer inside one card sticks only while that card is on
screen. A view-mode value reads at the input's size whatever the field — cx sets text fields'
larger than pickers'. In view mode a field is text lined up with its label, and a list of values — the types on a tag —
is chips, each a link to its record.

**Every asset's page opens with the same "Basic information"** — the asset's own fields, then the
subtype's sections. `formFields()` in `components/formFields.tsx` makes an editor's fields over its
state (picker, text, prose, money, date, yes/no, each labelled above with the server's message
beneath) and `basicInformation()` from them; `src/assets.ts` holds the asset's draft, its load from a
detail and its form, and the importance as the server will compute it. A subtype's model extends the
asset's draft and form and adds its own fields.

**A record's page shows everything attached to it** — `holdingSections()` in
`components/holdings.tsx`, over `HoldingSection`s from `src/holdings.ts` — read-only after its own
fields: on a person's page a card per kind that holds something, on a client's its projects. A card
has its count in the title and its first ten rows, each a link to its record — a device's and a
project's too, addressed ahead of their screens — and "See all N" to the owning list filtered to the
record (`personId`, `clientId`), or a line saying only the first are shown where there is no list yet. The kinds with nothing are one line beneath, not empty cards. A
seat on a device they hold says which device. On the activations list the filter reads "Held by":
theirs by name and those on their devices.

**View history leads every record's ⋮** (`historyAction`): the audit log filtered to the
record's id, which also takes an asset's own row, since a device, a piece of furniture or a license
shares its asset's id. The address carries the id only; the chip names the record from its first
entry. A verb first, as beside it — "Change history" reads as an order to change it — and not
"activity": the log holds saves, never who looked. A record the log has
never seen says so — the log begins on 5 December 2022 — rather than advising fewer search words.

**A record page and a searched list are made, not copied.** `RecordController` in
`src/recordController.ts` holds the page's life — the address naming record and mode, the
unsaved-changes guard, save, and a delete refused with what holds the record — and `recordPage()` in
`components/recordPage.tsx` its markup; `searchList()` in `components/searchList.tsx` is a list with
no filter pane. A screen declares its API, draft, fields and columns;
a record checked for an edit made meanwhile names its `lastModified` (a refused save offers Reload), one
that can be copied names what a copy keeps (`new?from=:id`, Duplicate behind the ⋮), and a new asset
bought in batches offers **"Save and replicate"** behind a chevron on Save — Save saves, the
chevron offers saving and opening the next as its copy — one control at every width. **Whatever names a record with a page
is a link to it in view** — a picker (`pick(…, { href })`), a fact on a read-only page — the assignee, vendor, location, type, manufacturer; a codebook value, which has no page,
stays text. **A name that titles something else is not that link**: a volume has no page of its own, and its row
is titled by its software's name on its license's page and by its license's on its software's —
either, as a link, would read as opening the volume. The title is text under the menu's icon for what
it names, the item's own mark saying which record it is, and the row's links carry the way on — its
activations, Activate, "View software" or "View license" — a muted dot between each. Away from its
license, a line under the section's label says so — "License volumes cannot be managed here, only in
the license editor", pointing to "View license" for a volume's own license and to Licenses, a link,
for adding one to another license or a new one. The section is on the page only: the form, which
changes none of it, leaves it out. The city and state a location offers are the chosen country's; a
form whose only choice is made — one country, its one city — starts with it.

**A printable document is a page of its own with its own print styles** — the handover sheet at
`~/company/people/:id/handover`: the original's text word for word, a Print button, and under
`@media print` the shell removed (`display: none`, not hidden — hidden, it keeps its room and
squeezes the sheet), the table's header repeating on each page, and the signatures on a page of their
own.

**A license's volume is read in parts, not as a sentence**: its software, then its type and
description muted beneath, and its seats — "15 / 35 in use" over a meter, primary while seats are
free, green when every one is used — a bought seat is meant to be — and red only past the quantity — in a column of their own, where the eye scans for them; on a
phone the seats take a line beneath the name.

**Removing a saved part of a record waits for the save** — a license's volume, a place where
information is kept: the existing one is struck
through, marked "Removed when you save", and has an Undo, so the reader sees what the save will take
and can take it back; one added in the same edit goes at once, as nothing is lost.

**Every web address has an open button** — `externalLink` in `components/`, a small icon anchor
right after the value, opening it in a new tab — whether the address is a URL field or sits in a
description. **In view only**: beside a field being typed into, it would open an address not yet saved. Never inside another link: a row that is itself a link shows it on its record's page.

**Anything that goes somewhere is a link**, an anchor with an address — a row, a chip naming another
record, a back link, and the New, Edit and Cancel buttons (`LinkButton`) — so it opens in a new tab,
takes a middle click and can be copied. A button only acts: Save, Delete.

**A question has answers that say what they do** — `confirm()` in `components/confirm.tsx`: "Keep" and
"Delete tag", never "No" and "Yes", the action last and red when it cannot be undone, the focus on
declining so Enter never deletes. Without an action it is a notice with one button. Deleting asks first
and says what goes with it; a record something still uses says so instead of asking and then refusing; leaving an edit with
changes asks "Keep editing" or "Discard changes".

**The route's id is read through `$route`**, declared in the editor's model, and **the address, not the
mount, says which record and mode are open**: `new` and an id match one route, so saving a new record
would keep the page and its controller, which therefore reopens on every change of `$app.url`. The server's field
errors land under their fields through `fieldErrors`, as a line beneath the field (`field-message`) with
cx's hover tooltip off, since a tooltip hides the one thing the reader needs; **unsaved changes ask before leaving**, in editing
only —
`guardLeaving` in `src/leaveGuard.ts`, cx's navigation confirmation for in-app links and the browser's
prompt for a reload or a closed tab. Browser Back leaves without asking: cx cannot hold a navigation the
browser has already made. A save or a delete releases the guard before it navigates.

**A row is a link, so its text cannot be selected; the values worth copying have a copy button
instead** (`copyButton` and `copyCell` in `components/`) on every text value of every list — names,
numbers, codes, people, places, descriptions, addresses; not counts, dates, money, status tags or a
"+N" list, whose text is not the value. Shown only while the pointer is over the cell, after any badge,
and never on a touch screen, which has no hover. It copies the value as shown and ticks for 1.5s.
Under a mouse the value truncates before its badges, so a long name never hides one. Not text selection: a link is dragged, not
selected, and a row that selects on a click stops opening its record on one. Not a `<button>`, which
an `<a>` may not hold; it stops its own click so the row does not open as well.

**A list of records** — `_records.scss` — is a card per row on a phone and columns from `md`, a header
on `raised` — the page's own tint dissolves the card's top edge — that sorts on a tap (`sortHeader`), a row that opens its record, and an **Add** button in the header band — "+ Add" whatever the list,
since its title names what is added, and "Add vendor" as its accessible name; activations keep
"Activate", the verb for a seat, named "Activate a seat". A list cut short — the first three types on a tag, the first fields of an audit
change — ends in a muted `+N` pill (`record-more`), so the count never reads as another name. **An empty cell is "—"**, far lighter than a
value (`record-blank`, `ink-ghost` against values in `ink-soft`: a thin dash is judged by weight, not
colour, so anything short of a ghost reads as one more value), never words like "No tags" that read as one more value; on a phone's card the
line goes. A yes/no that
most rows answer no — a type holding licenses — is a flag beside the name (`record-flag`), not a column
of "No". A subscription's status is a tag (`status-tag`) — red once
expired, amber within the fortnight, green after, grey on a record that has ended — and always says
which in words, never by colour alone. A record that has ended — a deactivated activation — stays
listed as history (`record-row-ended`): the row on the others' white, its text in
`line-strong` — below AA on purpose, there to be found rather than scanned — its name no longer bold, a
"Deactivated" flag saying why, and its license's expiry kept but grey, read rather than signalled.
**A count that holds ended records names its parts** — "3 active · 1 deactivated", a part at zero left
out — never a part beside the whole: "3 active · 4 in all" reads as two counts that overlap.

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

**The server log is a terminal on a palette of its own**, `term-*`, as the chrome has `nav-*`: every
text colour clears 4.5:1 on both its tones, and the type is the system monospace stack — no font to
download. A marker the server put in place of a character that would have acted (`⟨ESC⟩`,
`⟨U+202E⟩`) is set apart in `term-mark`, and a line break inside a message carries a `⏎`, so an
injection attempt reads as one.

**The logo tile is violet**, `brand`, wherever it appears: the chrome is Pulse's, and the mark is what
tells the two applications apart at a glance. The favicon is the same tile, an SVG inline in `index.html` — the one place the
colour and the glyph are written out rather than taken from `tailwind.css` and `_brand.scss`, since a
browser reads it before any stylesheet. **A home screen ignores an SVG icon**, so the tile is also a PNG in `client/public`:
`apple-touch-icon.png` (180px) for iPhone and 192 and 512px ones in `manifest.json` for Android —
full bleed, since the platform rounds the corners and iOS turns transparency black. The manifest's
`display` is `browser`: standalone would drop Safari's address bar and, with it, pull-to-refresh. **Codaxy's wordmark is codaxy.com's
white logo as a mask** in `_brand.scss`, its two tones kept (`axy` at 55%), its colour a token.

**Every colour and shadow is a token in `src/tailwind.css`**, in `@theme static`, and nothing else in the client
writes one. Each text token clears AA (4.5:1) on both the card and the page, but `ink-ghost`, which marks an
absence and never content, and `ink-label`, the uppercase label of a card, which clears it on the card
only and is used nowhere else. **Field and button edges are `line-field`, 1.8:1**, under WCAG's 3:1 for
non-text contrast by choice: at 3:1 every field shouted. The white fill on the tinted page carries the
rest, and a field's edge darkens to `line-strong` (3.2) under the pointer. The house values for
`ink-faint`, `line-strong` and `warn` fail AA, so those values are darker here. **Text in a status colour uses its `-text` token**, which equals the fill where the fill
passes and is darker where it does not: `warn` is 3.4:1 as text, `warn-text` 5.1:1. A status's `-wash` is a
background its `-text` clears 4.5:1 on; `-mark` highlights the words a change touched, under `ink`.

**Windows are themed in `theme.ts`**: the preset's header takes the accent colour and its footer has
no top padding, which a coloured footer shows as buttons flush to its top edge; both are set there.
**A modal dims and blurs the page behind it** — ink at 55% and a 3px blur, fading in over 70ms, in
`_surfaces.scss`: cx's grey wash leaves the page legible enough to compete. No theme variable covers
the backdrop.

**Two layers, in this order.** `src/theme.ts` maps CxJS's theme variables onto the tokens and is
applied by `renderThemeVariables` at startup — colours, type and sizes of widgets belong there. The
full list of variables is `cx-theme-variables/build/presets/default.js`; the docs do not have it. Then
one partial per component in `src/scss/`, for what no variable expresses. The theme's SCSS and ours
load inside `@layer components`, so a Tailwind utility in markup wins over both.

**A card's padding is never smaller below its content than above it**: lighter at the foot, the
content reads as sinking in it.

**An invalid field is its red border**, never a tinted fill: CxJS's pink wash is overridden in
`src/scss/_fields.scss`. A message beneath it is for what the field cannot show — the server
refusing the address — never for "required" or "not a valid address", which say what the red
border and an empty field already do. Sign-in's button is disabled until the address is
well-formed, and empty is not an error.

**Controls are 36px under a mouse and 44px on a touch screen, by padding, not by a density preset.**
`theme.ts` takes `densityComfortable`'s 24px line with 5px vertical padding on inputs and buttons;
`(pointer: coarse)` raises it to 9px in `_surfaces.scss`, which also sets `--control` — the height
every hand-built control takes. The breakpoint is the pointer, not the width: a narrow desktop window
has a mouse. Grid rows keep 9px at every size — a row is read, not touched. `densityCompact` (32px)
is too dense even under a mouse.

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

**Every response says what a browser may keep** (`CachingSetup`): `/api` is `no-store`; the shell
and other static files are `no-cache`, revalidated on every load so a deploy takes effect at once; the
hashed bundles under `/assets` are kept a year, `immutable`, since their names change with their
content. Left unsaid, a response with only a `Last-Modified` is cached for a guessed lifetime — a
browser then answers an API URL with whatever it once got there, the shell from a build that lacked
the route, until its cache is emptied by hand.

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

**A right-aligned cell shrinks to its content**, so a skeleton bar sized in percent inside one is
zero wide; its bar has a fixed width. **A list's first load also sets it loading**, whose dimming is
for a refetch; the outline undoes it with `:has()`.

**Beyond the page a browser paints only the root's background.** A shadow or pseudo-element reaching
past the page's end is not drawn in a bounce there — Chrome shows the root's colour — so two colours
at the two ends mean swapping the root's by scroll position. Watch the shell's size, not the body's:
the body is the window's height and the page overflows it.

**A scroll lock that outlives its overlay freezes the page until a reload**: it listens to the whole
document and refuses every gesture outside the overlay. A window mounted twice without unmounting — a
hot reload can — overwrites its first release, so `lockScroll` also releases itself once its overlay
has left the document, and a window's re-mount releases the lock it already holds.

**What a form fills in for itself is where it starts, not an edit**: a value the address preselects
once the options arrive must be counted into the unsaved-changes baseline, or Cancel asks to discard
changes nobody made.

**A list cell's `display` belongs to its columns**: `*-columns` show and hide cells by width through
`display`, so a rule restyling a cell's `display` — flex for its content — shows a hidden column or
loses to them. Lay the content out inside the cell.

**Read the address from `$app.url`, never `window.location`**: cx updates the store as it navigates
and moves the browser's address only once the new page has rendered, so a controller opening on a link
reads the page it came from in `window.location` — a duplicate opened empty for that.

**A `var()` naming a token that does not exist voids the whole declaration**, silently: `border: 1px
solid var(--color-typo)` computes to no border, not to a border of some default colour. A name
carried over from Pulse is the likely typo — check `tailwind.css` before reaching for `!important`.

**Free text is often a URL, which has nowhere to break**: anything showing a typed value in a flex or
grid cell sets `overflow-wrap: anywhere`, or one long description widens the page on a phone — and a
check against records with short text never sees it.

**Never cap or scroll the list inside a picker's `.cxe-lookupfield-scroll-container`.** cx treats that
container as the scrolling element: it cancels every wheel on it at either end, so the page does not
scroll instead, and loads further options when it nears the bottom. A `max-height` and `overflow` on
the list inside leave the container never scrolling — at both ends at once — so no wheel scrolls the
list, in any browser, and the options past the first page never load.

**A closed `LookupField` ignores `inputAttrs`**: its only name is `aria-labelledby="<id>-label"`,
pointing at a label cx renders only in a labels layout. Give the field an `id` and the visible label
beside it the matching `<id>-label`; in a `Repeater`, bind both to the row's key.

**Text the original emptied is `""`, not `null`.** Its forms saved a cleared description as an empty
string, so a list that shows "—" for a missing value tests for blank (`||`), not for `null` (`??`), or
those rows show nothing at all.

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

**A menu item is lit by `isCurrent`, not by cx's `match`**: an item stays lit on a record under its
address — `~/licenses/:id` — but not on another item nested under it — `~/licenses/activations`.
`equal` darkens the first, `subroute` lights the second; `match` is widget configuration, so it cannot
vary per item in a `Repeater` either.

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
