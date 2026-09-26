# Persistence

*Reverse-engineered from the code. It describes what the system does, not decisions recorded when they
were taken; correct it where it is wrong rather than working around it.*

PostgreSQL through EF Core and Npgsql. `InventoryContext` exposes one `DbSet` per entity and applies
every `IEntityTypeConfiguration` in the assembly — the entity classes carry no mapping attributes, so
mapping lives in a configuration beside its entity or nowhere. Entities without one
run entirely on convention.

## Naming

Tables and columns are snake_case and unquoted, through `EFCore.NamingConventions`. PostgreSQL folds
unquoted identifiers to lower case, so this is what lets a hand-written `select * from assets` work.

**A table is named after its entity, not after its `DbSet`**: `person`, not `persons`. `DbSet`s are
pluralised mechanically — `Informations`, `Confidentialities` — because that is how they read in C#
and because a mechanical rule needs no arguing about; tables take the CLR name through a loop in
`OnModelCreating`, so none of that reaches the schema.

**Key, index and foreign-key names still follow the `DbSet`**, giving
`fk_activation_assets_asset_id` on a table called `asset`. EF derives those before the loop renames
the table, and every way of correcting them afterwards is worse: clearing them makes EF rebuild them
after the naming convention has run, so they come back as `FK_Activation_Asset_AssetId`, and setting
them by hand bypasses the uniquifying EF does at PostgreSQL's 63-character limit, where the two
foreign keys on `electronic_device_type_electronic_device_tag` then collide. Constraint names are not
worth that, so they are left alone.

**Mostly the `DbSet`, and the loop's order decides which.** Renaming a table re-derives the foreign keys
still named after it, so a constraint named before its principal is renamed carries `assets`, one named
after carries `asset` — `fk_activation_assets_asset_id` beside `fk_information_location_cloud_cloud_id`.
The loop walks entities by class name, the order the schema was built in. EF's own order is by full
name, and walking that renames constraints whenever an entity changes namespace.

**The convention is applied in `InventoryContext.OnConfiguring`, not beside each `UseNpgsql`.** Three
places build a context — the app, the integration factory and the migrations fixture — and one that
configures the model differently from the others builds a model that no longer matches the snapshot.

It is a database convention only: the CLR entities and properties keep their PascalCase names, and
must, because the audit log has recorded them in every row it has ever written.

## Keys

Primary keys are GUIDs, and **every new one is a UUID version 7**: `Guid.CreateVersion7()`, never
`Guid.NewGuid()`. A v7 id begins with its timestamp, so ids arrive in roughly ascending order and an
index on a primary key stays dense; v4 ids scatter across it and the cost grows with the table. It
holds everywhere, not only for rows that are obviously ordered — the exceptions are what leave a
table with two id shapes and no way to tell which is which.

The application allocates the id rather than letting the database default it, which is what lets an
asset and its subtype row share one key before either is inserted.

Besides the primary keys, `Asset.InventoryNumber` carries the only unique constraint in the database.
Names in the codebooks and the directory are not unique: whether two vendors may share one is a product
question nobody has answered, so the schema does not answer it either.

## Migrations

Applied at startup by `Database.Migrate()`, followed by the seeder. There is no separate migration
step in deployment: starting a container migrates the database it points at.

**`Database:MigrateOnStartup` governs both**, and defaults to true. It is a setting rather than a
guess from the environment's name: whether an application brings the database up to date is a
deployment decision, and an application that knows what "Testing" means has a test harness leaking
into it. The integration tests turn it off and build their schema from the model; `MigrationsTests`
is what runs the migrations, against a database of its own.

**The migrations are the original's files, their bodies unchanged** — block-scoped namespaces and
all, because EF Core's generator ignores the file-scoped setting in `.editorconfig` and a converted
history would diverge from what the next `migrations add` writes. The namespace itself follows the
project, `Codaxy.Inventory.App.Persistence.Migrations`, which is what that command generates. Everything
else carried over was converted.

**Their designer files and the model snapshot are not history.** Each carries the CLR type names of
the model, so renaming a namespace has to reach them: the snapshot is what EF diffs against. They name
types as strings, so a stale one still compiles and passes every test. The migration bodies name
nothing but tables and columns.

**The history is copied from the original application and does not grow here** until that application
is deleted — see [co-existence.md](co-existence.md). A migration added on one side only would leave
the two describing different schemas with nothing to say which is right.

## Seeding

`SeedData()` is idempotent per table — each block runs only when its table is empty — and fills the
codebooks and a starting directory with the values the product expects. It is safe to run against a
populated database, but it will not repair or update a table that already has any row in it.

## Audit log

`AuditLogInterceptor` is a `SaveChangesInterceptor` registered on the context. For every added,
modified or deleted entity implementing `IIdentifiableReadOnly<Guid>`, it writes one `AuditLog` row
holding the table name, the entity id, the signed-in user's email (`"system"` when there is no HTTP
context), the action, and the complete before/after property set as JSON. `AuditLog` is excluded so
the log cannot log itself.

Rows written in one `SaveChanges` share a `TransactionId`.

**Reading it, what changed is computed, not stored**: both documents hold every property, so an update's
changes are the properties whose JSON differs. A foreign key's value is named through the EF model —
the key's target entity and the first of its `Name`, `Text`, `Level`, `Status`, `Substatus` or
`Description` — so every logged entity is covered without a map per entity. An asset's subtype row has
no name of its own and shares the asset's id; it is labelled from the asset, or from the asset's last
logged values once the asset is deleted.

## Traps

**An explicit `ToTable` or `HasColumnName` overrides the convention silently**, leaving one table in
PascalCase among snake_case neighbours with no error to say so — only a schema comparison finds it.
Nothing needs one now; adding one means spelling out what the convention would have produced anyway.

**The migrations history table is not covered by any migration.** EF reads it to decide what to apply,
so the convention renaming its columns locks out every database written before the convention landed.
`MigrationsHistoryNaming` renames them before `Migrate()`, and the rehearsal script repeats the same
SQL because `dotnet ef` does not go through startup. Both are deletable once no database predates the
naming migration.

**Seeded ids are mostly not stable.** Around 300 seeded rows take `Guid.NewGuid()` and a dozen take a
literal GUID, so most codebook ids differ between every database that was ever seeded separately.
Nothing may hard-code one, and data cannot be moved between environments by id. The client already
works around it by looking codebook entries up by text.

**The audit log stores every column of every tracked row, in clear.** That includes licence key
identifiers and anything in `Information` marked as personal data. It is readable by every signed-in
user — see [auth.md](auth.md).

**`TransactionId` names something that does not exist.** No code opens a transaction, so the value is
a fresh GUID per `SaveChanges`. It groups a save, and reading it as a business transaction is wrong.

**The interceptor captures `HttpContext` in its constructor**, so it attributes writes correctly only
on the request scope that resolved it; anything saving outside a request records `"system"`. Its
synchronous `SavingChanges` blocks on the async path with `.Result`.

**An instant must be `DateTimeOffset`.** It maps to `timestamp with time zone` and keeps its offset
all the way to the browser, which is why `AuditLog.TimeCreated` and `Asset.LastModified` display
correctly. A plain `DateTime` reaches the client with no zone and is read there as local time —
behind by the viewer's offset.

**A calendar date is `DateOnly` and a `date` column.** Purchase, activation, manufacturing,
expiration and due dates are the same day in every timezone. Given a `DateTime` they are not: the
client sends local midnight with its offset, `System.Text.Json` applies it, and a UTC server stores
the day before at 22:00 or 23:00 — then re-saving the record walks it back again. `DateOnly` has
nowhere for an offset to be applied, which is the whole of the fix; a server or container timezone
setting is not.

**Migrations that declare no store type resolve it when they are applied**, which is how the same
history produced `timestamp without time zone` through the app and `timestamp with time zone` through
`dotnet ef`. A migration whose column type matters states it, or the two diverge and only
`MigrationsTests` notices.

**`JsonTextWriter` throws on a type it does not recognise**, rather than falling back to `ToString`.
`TrackedChanges` converts `DateOnly` by hand for that reason; anything newer than `DateTime` reaching
an audited entity needs the same, or every save of that entity fails.
