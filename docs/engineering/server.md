# Server

Two projects. **`Codaxy.Inventory.Web` is the host**: `Program.cs`, `Setup/`, `wwwroot/`, settings,
launch profiles, and `Auth/` — who may sign in and how is the host's business, beside the cookie and
Google handlers `Setup/` configures. **`Codaxy.Inventory.App` is the domain**: the menu's items, and the
persistence they share. `Web` references `App`, never the reverse, so hosting and authentication stay
out of the domain.

Each project's namespaces start with its name — `Codaxy.Inventory.App.Licenses`,
`Codaxy.Inventory.Web.Auth` — so a namespace says which project holds it. Moving code between the two
changes its namespace; for an entity, the migrations have to follow (see Traps).

## Ruled out

- **A `Persistence` project of its own.** The context declares a `DbSet` per entity and the constraint
  names derive from them (see [persistence.md](persistence.md)), so it must see the entities, which the
  items also own: a cycle, or an interface repeating every `DbSet` to break it.
- **Repositories.** The context already is one; a wrapper either passes `IQueryable` through or
  reinvents paging, filtering and sorting in front of it.
- **A mediator, handler interfaces, a mapping library.** An endpoint takes the context and the services
  it needs; mapping is written out where a reader sees what an endpoint exposes.

## One folder per menu item

**The domain is organised as vertical slices, and the slice is the use case**:
`Activations/Deactivate/` cuts through one screen top to bottom — endpoint, request and response, its
own logic — and sits in the folder of the menu item it belongs to. What is not a slice is named as
such: `Shared/`, `Persistence/`, and the host in `Web`. Taken from the pattern: the cut by feature. Not
taken: boundaries between items enforced by tests, since review holds them; data access or a contracts
layer per slice, since the schema is one graph and the context is shared.

`App` mirrors the client's menu: a folder per section, and in it a folder per item —
`Licenses/Activations`, `Company/Locations` — so a screen has the same path on both sides. An item in
no section, the Dashboard, is a folder at the top.

```
Licenses/
  Endpoints.cs                    the section's items
  Activations/
    Activation.cs                 the item's entities and their configurations, at its root
    Endpoints.cs                  its group and the use cases mapped into it
    List/  Create/  Deactivate/   one folder per use case, beside them
```

An item owns one or two entities, so it holds them directly; a `Domain/`–`Persistence/` pair would be
two folders of one file each. A configuration exists only where convention is not enough.

A use case holds a static `Endpoint` — a class cannot take its folder's name — with its request and
response models and any logic only it needs. What several use cases of an item share sits at the item's
root. A folder exists once there is something in it: none is created ahead of its screen, so
`Administration/` appears with the first of its screens. `Auth/`, in `Web`, shows the use-case shape, and `MapAuth` is
the one call `Program.cs` makes for it.

**A configuration option belongs to the code that reads it**; `Setup/` binds it. **Options say what is wrong with
them** — `Problem`, null when valid — and `Setup/` validates by it at start (`ValidateBy`), so the
reason is written beside the rule and a new rule brings its own message: never a fixed message at the
binding, which goes stale once a second rule can fail.

**What an administrator decides is a setting in the database, not configuration**: a `setting` row
per key — `group.name`, the group a card of the settings screen — holding its value as `jsonb`, so
the audit log names the setting that changed. **A setting is a `SettingDefinition<T>` beside the code
that reads it** — `DashboardSettings`, `DocumentSettings` — with its default and its `Problem`, and
`Shared/Settings/SettingCatalogue` lists them in the screen's order. A reader takes
`SettingStore.LoadAsync` once per request, uncached, so a change applies on the next one. **A stored
value that cannot be used** — not its type, null, or with a problem: an edit by hand, since nothing
else saves one — **reads as its default and is logged**, never stopping a request or the start.
**A group is saved whole** — `PUT /api/administration/settings/{group}` with every setting of it, keyed
`group.name` — or, with any problem, not at all, the problems under their keys; a value is tidied
before it is checked (trimmed, a language's canonical tag). A check that needs the database — the
domains' System Admin — is the endpoint's. Configuration is not read for a setting: one value, one source. Not a row per group with a column per
setting: a migration for every new one, and an audit entry naming the group rather than the setting.

## Where an entity lives

**With the item that shows it**, and a lookup with no screen of its own with the item that uses it:
countries, cities and states in `Company/Locations`, currencies and periods in `Licenses/Licenses`.

**`Shared/` holds what several items use and none can own**, each as a folder of things that belong
together — never a bin sorted by kind:

- **`Shared/Assets`** — `Asset` and its categories, statuses and types, `Sequence`, maintenance
  contracts, business entities: `Asset` is the base of devices, furniture and licenses alike.
- **`Shared/Classification`** — confidentiality, integrity, availability and importance, which assets
  and information always carry as a set.
- **`Shared/Access`** — the permission catalogue and the policy a section's endpoints carry.
- **`Shared/Volumes`** — `Volume` and its type. Licenses, software and services, cloud subscriptions, software and
  activations all point at a volume.

**`Persistence/`** holds the context, the migrations, the seed data, the audit log and its interceptor,
and `IIdentifiable`. It is neither an item nor shared domain.

Any item may read another's entities and hold a foreign key to them; only the owner writes them.
**A link table belongs to both sides of its many-to-many**: a tag's editor writes the tag–type pairs,
and so will a type's. The
schema is one graph, so an entity referencing another item's is a foreign key, not a breach.
What an item's *code* reaches into is review's to hold; nothing enforces it.

## Naming

**"License", never "licence"** — in what a person reads, URLs, DTOs and their JSON, folders,
identifiers, tests and these documents, as the menu has it. Entity members and columns are the
exception until a migration renames them: `ElectronicDeviceType.HoldLicences` and `hold_licences`.

**No folder takes the name of a class it would shadow.** A namespace wins every lookup from inside
`Codaxy.Inventory.App`, and renaming an entity splits the audit log (see
[persistence.md](persistence.md)). Where the plain name is taken, a
folder takes the mechanical plural its `DbSet` already uses: `Furnitures/`, `Informations/`,
`Infrastructure/Softwares/`, `Administration/AuditLogs/`.

## Lists

**Every list pages, filters and sorts in the database**, with one shape. `page` counts from 1 and
`pageSize` defaults to 25, at most 100, both in the query; the answer is `{ items, total }`, the total
counted over the filtered rows. A page below 1 or a size outside 1–100 is a 400 validation problem; a
page past the last is empty with the real total, since the list can shrink under its reader.
`Shared/Paging` holds the convention: `Paging.Read` validates, `ToPageAsync` counts and then reads the
window.

**One page is not paged at all, an employee's own equipment** (`MyEquipment/Get`): one person's
things are a few hundred at most, across four kinds of record no single query pages, so the page takes
them whole and shortens and searches them itself. Every list pages in the database.

**Offset paging with a count**, not keyset: a screen shows "page 3 of 40" and jumps to a page, which
keyset cannot, and at this data's size the count is free. Not a page of `pageSize + 1` rows either — it
knows only whether a next page exists, so the pager can say neither where the reader is nor how far is
left.

**The order ends in a unique key** — `ThenBy(a => a.Id)` after whatever the reader sorted by. Rows
sharing a sort value otherwise come back in any order, and one appears on two pages or on none.

**Free text is `q`**: split on whitespace, every term must match, each against any of the searched
columns, by `ILIKE` with `%`, `_` and `\` escaped so they match themselves. **A term that is a GUID matches ids, exactly** —
the record's own and those of what it points at directly (a piece of furniture's assignee, vendor,
location, type), never a many-to-many link — so a pasted id finds its record in any list. **A
leading `#` is dropped**, so an inventory number typed as the screens show it matches. **An id filter
is read as text**: one that is not an id matches nothing and answers an empty page, where binding a
`Guid` would refuse the whole request with a 400. Other filters are named
parameters, exact unless their name says otherwise, ANDed. A range is `from` inclusive and `to`
exclusive, so adjacent ranges neither overlap nor leave a gap.

**The menu's endpoints are one `/api` group that requires a session**, mapped by `MapInventoryApi`
beside `MapAuth`, and each section's behind its permissions (see [auth.md](auth.md)). Each level of the menu lists only its children, in the menu's order:
`MapInventoryApi` the sections and the Dashboard, a section's `Endpoints` its items, and an item's
`Endpoints` its group beneath `/api`, named after its URL, and its use cases. A new use case touches
only its item, and takes its section's permission from its method: a read `view`, a write `edit`.

**An asset is written through `Shared/Assets/AssetWrites`**: the inventory number taken from
`Sequence`, the asset type found by its seeded name on the server, the importance computed, and
`LastModified` set by the server to the microsecond PostgreSQL keeps. **Every asset's form carries the
asset's fields flat beside its own and implements `IAssetForm`**, so one `Validate`, `CheckAsync`
(every id exists), `ApplyAsync`, `CheckUnchanged` and `OptionsAsync` (the pickers) serve each subtype;
the subtype's writes call them first, then do their own. Not a nested asset object on the wire: the
interface shares the code without changing the API.

**An asset's update is checked against the `lastModified` it was loaded with**: the body echoes it,
and one that no longer matches is a 409 — someone saved since, and their edit is not overwritten.

**A state change is its own endpoint** — `POST …/{id}/deactivate`, `…/reactivate` — never a `PUT` of a
wide model: it carries only what the change takes, and answers 409 from the wrong state.

**A list's spreadsheet is its own query, every row**: `GET …/export` takes exactly the list's
parameters, and the list and its export share one function that filters and orders (`Rows`), so the
file holds what the screen shows, all pages of it. **File names and headers are fixed** — the sheets
and formulas that read them depend on them — with " - Filtered" added when a search or a filter narrowed
the rows, so a partial list is never taken for the whole; the sort does not count. Columns a list
gained later follow them — an activation's device number, holder and location. Served as the response
to that request, not as a handle to a file cached for thirty seconds: a handle is for a bearer token,
which cannot ride a download link, and the session cookie does.
**ClosedXML writes it, through `Shared/Export/Spreadsheet`**: one row type per list, a property a
column only when `[XLColumn]` heads it; one sheet named after the list, holding an Excel table —
header pinned, a filter on every column, widths fitted to the content, which needs no font in the
image — with dates as dates and money
to two decimals, so the file sorts and sums. **An instant is written in the viewer's time zone**,
since a cell holds none: the request names it, `tz=Europe/Belgrade` — the browser's IANA name — and
the column, a `DateTimeOffset`, takes that zone's local time, daylight saving included. **Its header
names the zone by its abbreviations, standard time first** — "Last Modified (CET/CEST)" — which the
browser sends as `tzLabel`, since .NET on Linux knows only long names: at most 24 of letters, digits
and `+ - : /`. Without one, the header gives the zone's offsets in January and July, "GMT+1/GMT+2";
without a zone, "UTC". `Shared/Export/ExportZone` reads both as `Spreadsheet` writes, so no export
declares them; an unknown zone or a refused label is a 400. Never one offset for the column: it is
wrong for every row on the other side of a clock change. Not the browser's offset, which is wrong for an
instant on the other side of a clock change; not a zone per deployment, which offices in several
zones share; not text with an offset, which Excel no longer reads as a date. Not ClosedXML's
`InsertTable` over the rows: it writes a `DateOnly` as text. Not CodeReports, which sizes a column by its header's padded width
and throws under a culture it does not ship; not EPPlus, licensed commercially.
**A list without an established file** — the company's records, the infrastructure — takes the list's
columns, then the record's own fields, names resolved and counts kept; its file is `<List>.Export`,
the list's name without spaces.

**Mail is `Shared/Email/IEmailSender`**, one message to every recipient — a failure then sends to none
rather than some — plain text, with an HTML version where there is one, implemented in `Web` over SMTP
(`Smtp:*`). **Every email has one layout, `Shared/Email/EmailLayout`** — a sign-in code's as an expiry
alert's — and reads as a notification, not a newsletter: one white card on a grey page under a thin
bar in Inventory's violet, the mark and a heading that says what happened, then the content and the
sender beneath — no dark banner, imagery or call to action, which read as an advertisement and get
skipped; the heading is the greeting. **The page, the card and their text are coloured together**, a
light set and a dark one by `prefers-color-scheme`, in the stylesheet and never inline: a client that
follows a dark theme for the text but keeps its own white behind it showed white on white, and an
inline colour cannot be undone in a dark theme. Every name with a page links to it, and so does a page
named in the text — "Administration → Settings"; the plain text says the same, a page's address on a
line of its own. **The settings screen describes the relay through `IEmailRelay`** — host, port,
sender, the account it signs in as, never the password — and sends a test to the signed-in
administrator only, never to an address typed in, so the button mails nobody else. A failed test
answers 200 with what the relay said: it is the answer asked for, not a failed request. **The SMTP
settings stay configuration**: Gmail sends only as the account that signs in, so the sender belongs
with the account, and a password in the database would need encrypting and keeping out of the audit
log, which stores every value in clear.

**Expiry alerts are a check, not a schedule** (`Shared/ExpiryAlerts`): a hosted worker in `Web` runs
`ExpiryAlertCheck.SendDueAsync` at start, every `ExpiryAlerts:Interval` and as soon as the alert settings
are saved (`ExpiryAlertRequests`) — recipients just named hear at once — which reads the settings and
the dates as they are, sends one email listing whatever is due and not yet sent, and records it once
the send succeeds; a failure is logged and the next check tries again. Not an alert row stored per
record: every change of a date would have to keep it in step, and records that exist would need one
each. One email per check, not per record: a first check can find dozens. A single instance is assumed
— two would both send.

**A PDF is the client's own page, printed by the server in headless Chromium** — the handover
sheet's, `GET …/people/{id}/handover.pdf?tz=…&lang=…` — through `Shared/Printing/IPagePrinter`,
implemented in `Web` by PuppeteerSharp. Chromium opens the page on the server's own address, signed in
as the caller (see [auth.md](auth.md)), its clock in `tz`, its language `lang` — passed on as the
page's own `?lang=`, one the server does not print in a 400, none the default language setting — and prints once the page sets `data-print` to
`ready` — the page's print rules and A4 `@page` are the file's. One browser for the process, opened on
the first print and closed after `Pdf:IdleClose`; a fresh incognito context per print; two prints at
once, more wait; a page not ready within `Pdf:Timeout`, or a browser that does not start, is a 500 that says so. Every request to
anything but this machine is aborted, so nothing the page links to is fetched. **The printer exists
only where `Pdf:ChromiumPath` names a browser**: without one it is not registered, the PDF endpoint is
a 404 and the sheet offers no download. Not a PDF library laying the sheet out again: two layouts of
one document drift. Not the browser's print dialog: not a file, and clumsy on a phone.

**The document may move to the server**: a Razor component rendered to one self-contained HTML page,
its text a `.resx` per language, shown on screen in a frame and printed by handing Chromium that HTML
(`SetContent`). That drops the caller's cookie on the print, the server's own address and
`data-print`, and lets a test read the rendered document, which no test can now; it costs moving the
sheet, its styles and its text out of the client, the larger change while the client's page works.

**What every printed document shares is `Shared/Documents`**: its settings (`DocumentSettings`: the
place, empty for a hand, and the default language, English by default), the languages documents are printed in (`DocumentLanguages`, which
must match the client's list, and which matches a tag whatever its case and writes it as listed —
`sr-latn-ba` is `sr-Latn-BA` — as BCP 47 has it), and `GET /api/documents/settings` — the default
language and the place — so no document's answer repeats them. A document's file name stays English whatever its
language: it is named for the reader saving it.

**A delete the database would refuse is a 409 that says what holds the record** — "113 devices are of
this type" — checked before the save, not left to surface as a foreign-key 500.

**What is attached to a record is counted and sampled by one rule** — `Shared/Assets/AssetHoldings`:
the devices, furniture and licenses of any asset predicate, and any other kind through
`SectionAsync`, each its total and its first ten by name. **A name unique by the application** goes
through `Shared/Validation/Unique`, trimmed and case-folded on both sides.

**A page that gathers other items' records has an endpoint of its own** — a person's holdings, per
kind the total and the first rows, in one request — rather than a list endpoint per kind called from
the page: one round trip, and a kind without a list endpoint yet is served all the same. "See all"
is the owning item's list, filtered.

**Shared helpers for every item**: `Shared/Paging` (the window and the page), `Shared/Search`
(`FreeText`, the free-text terms), `Shared/Validation` (`MiniValidator`, whose problem keys are the
JSON's field names, so a message lands under the field it names).

## Traps

**A new child added to a tracked parent's collection with its key already set is taken for an
existing row**, and EF saves it as an update that matches nothing — a concurrency exception. Add it
to the context as well as to the collection.

**Order on the entity, then project.** EF cannot translate an `OrderBy` over the members of a record
built in a `Select`; a list sorts its query and projects last.

**An unknown `/api` path must be a 404.** The shell's fallback answers every unmatched path with the
page and a 200, so without the `/api` fallback before it a missing endpoint reaches the client as HTML,
which it fails to parse and reports as a generic failure.

**Moving an entity changes its namespace, and two things have to follow.** The table-renaming loop in
`InventoryContext` walks entities by class name — walked in EF's full-name order, a move renames
constraints; `MigrationsTests` fails on that. The model snapshot and the migration designer files name
types as strings and follow only by search and replace: **a stale string passes every test**, so grep
for the old namespace after a move. Designer strings naming entities dropped long ago keep their old
namespace.

**`App` uses the plain SDK, so it declares the implicit usings `Sdk.Web` would add** — `Http`,
`Routing`, `Builder`, `Logging` and the rest. Without them every endpoint fails on `IResult` and
`HttpContext`.

**Where the server listens is not where Chromium can reach it**: `IServerAddressesFeature` gives
`http://+:8080` or `http://0.0.0.0:48010`, so a wildcard host becomes the loopback address, and plain
HTTP is preferred where it listens on both.

**`dotnet ef` needs both projects**: `--project Codaxy.Inventory.App --startup-project
Codaxy.Inventory.Web`. The migrations live with the context in `App`; the design package and the
configuration live in `Web`.
