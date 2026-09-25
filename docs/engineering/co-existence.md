# Co-existence

Two applications run against one database: this one and the original `Codaxy.Inventory`, in the
`inventory` repository. Both are reachable for as long as it takes this one to reach parity, after
which the original is deleted and everything here stops being conditional.

## What that freezes

**The schema cannot change while both run.** No migration is added here until the original is gone —
not a column, not an index. A change would have to land in both migration histories identically, and
nothing would enforce that.

**The CLR property names are part of the database.** The audit log records them in every row it has
ever written, so an entity renamed here writes a second vocabulary into one log with nothing to tell
the two apart. Entity and property names match the original exactly; the API's own models are free.

**Ids are UUID v7 here, and the original mints its own differently.** Both are time-ordered GUIDs in
the same `uuid` column, so neither application has to care which wrote a row: there is nothing to keep
in step, and nothing of the original's key generation comes across.

**Inventory numbers are allocated the same way** — read the single `Sequence` row, stamp the asset,
increment. It races, and the unique index on `inventory_number` is what turns a collision into a
failure rather than a duplicate. Two mechanisms on one counter would disagree where one does not.

## What is not shared

**Sessions.** This application mints its own tokens; a user signing in here is not signed in there.
Sharing them would mean inheriting the original's hand-minted ASOS ticket and its unmaintained
validation package, which is one of the things this rewrite exists to leave behind.

**Code.** Nothing is referenced or linked across the two repositories. The migrations, the migration
tests and the history-naming SQL are copied, and `MigrationsTests` is what proves the copy still
describes the same schema — in the original it guards a schema that moves, here it guards a copy that
must not.

## Migrating and seeding

Both applications migrate and seed at startup, as the original always did — `Database:MigrateOnStartup`,
true by default. With the history identical
on both sides `Migrate()` is a no-op from either, and EF serialises concurrent calls, so neither
waits on the other beyond that. `SeedData()` fills a table only when it is empty, so whichever starts
first seeds and the other skips.

The one hole, and it is the original's too: the emptiness check takes no lock, so two applications
starting at the same instant against an empty database can both insert. It costs a duplicate codebook
in a fresh environment, once.

## Deferred schema changes

Things the database wants and cannot have until the original is gone. This list is the input to the
first migration after deprecation, not a backlog to act on.

- **No index serves any predicate.** Primary keys and the unique `inventory_number` are the whole of
  the indexing. The original filtered in the browser so there was nothing to serve; this application
  filters, sorts and pages in the database, and does it with sequential scans. At the present size —
  under a thousand assets — that costs nothing measurable, which is why it waits.
