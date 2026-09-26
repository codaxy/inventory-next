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
layer per slice, since the schema is one frozen graph and the context is shared.

`App` mirrors the client's menu: a folder per section, and in it a folder per item —
`Licenses/Activations`, `Directory/Locations` — so a screen has the same path on both sides.

```
Licenses/
  Activations/
    Activation.cs                 the item's entities and their configurations, at its root
    List/  Create/  Deactivate/   one folder per use case, beside them
```

An item owns one or two entities, so it holds them directly; a `Domain/`–`Persistence/` pair would be
two folders of one file each. A configuration exists only where convention is not enough.

A use case holds a static `Endpoint` — a class cannot take its folder's name — with its request and
response models and any logic only it needs. What several use cases of an item share sits at the item's
root. A folder exists once there is something in it: none is created ahead of its screen, so
`Administration/` appears with the first of its screens. `Auth/`, in `Web`, shows the use-case shape, and `MapAuth` is
the one call `Program.cs` makes for it.

**A setting belongs to the code that reads it**; `Setup/` binds it.

## Where an entity lives

**With the item that shows it**, and a lookup with no screen of its own with the item that uses it:
countries, cities and states in `Directory/Locations`, currencies and periods in `Licenses/Licenses`.

**`Shared/` holds what several items use and none can own**, each as a folder of things that belong
together — never a bin sorted by kind:

- **`Shared/Assets`** — `Asset` and its categories, statuses and types, `Sequence`, maintenance
  contracts, business entities: `Asset` is the base of devices, furniture and licences alike.
- **`Shared/Classification`** — confidentiality, integrity, availability and importance, which assets
  and information always carry as a set.
- **`Shared/Volumes`** — `Volume` and its type. Licences, software and services, clouds, software and
  activations all point at a volume.

**`Persistence/`** holds the context, the migrations, the seed data, the audit log and its interceptor,
and `IIdentifiable`. It is neither an item nor shared domain.

Any item may read another's entities and hold a foreign key to them; only the owner writes them.
**A link table belongs to both sides of its many-to-many**: a tag's editor writes the tag–type pairs,
and so will a type's. The
schema is one graph and frozen, so an entity referencing another item's is a foreign key, not a breach.
What an item's *code* reaches into is review's to hold; nothing enforces it.

## Naming

**No folder takes the name of a class it would shadow.** A namespace wins every lookup from inside
`Codaxy.Inventory.App`, and entity names are frozen by the audit log. Where the plain name is taken, a
folder takes the mechanical plural its `DbSet` already uses: `Furnitures/`, `Informations/`,
`Infrastructure/Softwares/`, `Administration/AuditLogs/`.

## Lists

**Every list pages, filters and sorts in the database**, with one shape. `page` counts from 1 and
`pageSize` defaults to 25, at most 100, both in the query; the answer is `{ items, total }`, the total
counted over the filtered rows. A page below 1 or a size outside 1–100 is a 400 validation problem; a
page past the last is empty with the real total, since the list can shrink under its reader.
`Shared/Paging` holds the convention: `Paging.Read` validates, `ToPageAsync` counts and then reads the
window.

**Offset paging with a count**, not keyset: a screen shows "page 3 of 40" and jumps to a page, which
keyset cannot, and at this data's size the count is free. Not a page of `pageSize + 1` rows either — it
knows only whether a next page exists, so the pager can say neither where the reader is nor how far is
left.

**The order ends in a unique key** — `ThenBy(a => a.Id)` after whatever the reader sorted by. Rows
sharing a sort value otherwise come back in any order, and one appears on two pages or on none.

**Free text is `q`**: split on whitespace, every term must match, each against any of the searched
columns, by `ILIKE` with `%`, `_` and `\` escaped so they match themselves. Other filters are named
parameters, exact unless their name says otherwise, ANDed. A range is `from` inclusive and `to`
exclusive, so adjacent ranges neither overlap nor leave a gap.

**The menu's endpoints are one `/api` group that requires a session**, mapped by `MapInventoryApi`
beside `MapAuth`, and each item a group beneath it named after its URL. **An item whose readers will
narrow has its own named policy now**, defined in the host — the server log's `ServerLog`, today any
session — so restricting it to a role is one line, not a search for every endpoint it covers.

**An asset is written through `Shared/Assets/AssetWrites`**: the inventory number taken from
`Sequence`, the asset type found by its seeded name on the server, the importance computed, and
`LastModified` set by the server to the microsecond PostgreSQL keeps.

**An asset's update is checked against the `lastModified` it was loaded with**: the body echoes it,
and one that no longer matches is a 409 — someone saved since, and their edit is not overwritten.

**A state change is its own endpoint** — `POST …/{id}/deactivate`, `…/reactivate` — never a `PUT` of a
wide model: it carries only what the change takes, and answers 409 from the wrong state.

**A list's spreadsheet is its own query, every row**: `GET …/export` takes exactly the list's
parameters, and the list and its export share one function that filters and orders (`Rows`), so the
file holds what the screen shows, all pages of it. Written as the original wrote them —
CodeReports, one row type per list, its `[TableColumn]` headers and file names kept — but served as
the response to that request, not as the original's handle to a file cached for thirty seconds: the
handle existed because its bearer token could not ride a download link, and the session cookie does.
CodeReports reads its texts by the thread's culture and throws on one it does not ship, the
invariant culture of a container among them, so `Shared/Export/Excel` writes under English.

**A delete the database would refuse is a 409 that says what holds the record** — "113 devices are of
this type" — checked before the save, not left to surface as a foreign-key 500.

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

**`Directory/` shadows `System.IO.Directory`** inside `Codaxy.Inventory.App`: `Directory.Exists` there
resolves to the menu section and fails to compile. Write `System.IO.Directory`. The tests and `Web` sit
outside that namespace and are unaffected.

**`App` uses the plain SDK, so it declares the implicit usings `Sdk.Web` would add** — `Http`,
`Routing`, `Builder`, `Logging` and the rest. Without them every endpoint fails on `IResult` and
`HttpContext`.

**`dotnet ef` needs both projects**: `--project Codaxy.Inventory.App --startup-project
Codaxy.Inventory.Web`. The migrations live with the context in `App`; the design package and the
configuration live in `Web`.
