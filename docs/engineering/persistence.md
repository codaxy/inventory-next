# Persistence

*Much of this is what the database already held when the application was built, described rather
than decided; correct it where it is wrong rather than working around it.*

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

It is a database convention only: the CLR entities and properties keep their PascalCase names, which
are what the audit log records.

## Keys

Primary keys are GUIDs, and **every new one is a UUID version 7**: `Guid.CreateVersion7()`, never
`Guid.NewGuid()`. A v7 id begins with its timestamp, so ids arrive in roughly ascending order and an
index on a primary key stays dense; v4 ids scatter across it and the cost grows with the table. It
holds everywhere, not only for rows that are obviously ordered — the exceptions are what leave a
table with two id shapes and no way to tell which is which.

The application allocates the id rather than letting the database default it, which is what lets an
asset and its subtype row share one key before either is inserted.

Besides the primary keys, `Asset.InventoryNumber`, a user's email, a role's name, a role's
permission and a setting's key are the only unique constraints in the database.
Names in the codebooks and the company's records are not unique: whether two vendors may share one is a product
question nobody has answered, so the schema does not answer it either. **Device types, tags, software or
services, furniture types, information types and tags, virtual machines, cloud subscriptions, software
entries, and every company record — people, clients, projects, vendors,
manufacturers, locations — are unique by name, whatever the case** — and **people by email too**,
since an email is what a sign-in would be matched against. The application enforces it, as a field
error; no index backs it yet, so two saves at once can still both pass. Both sides are compared
trimmed — older rows carry stray spaces.

## Migrations

Applied at startup by `Database.Migrate()`, followed by the seeder. There is no separate migration
step in deployment: starting a container migrates the database it points at.

**`Database:MigrateOnStartup` governs both**, and defaults to true. It is a setting rather than a
guess from the environment's name: whether an application brings the database up to date is a
deployment decision, and an application that knows what "Testing" means has a test harness leaking
into it. The integration tests turn it off and build their schema from the model; `MigrationsTests`
is what runs the migrations, against a database of its own.

**Migrations are left as `migrations add` writes them** — block-scoped namespaces and all, because
EF Core's generator ignores the file-scoped setting in `.editorconfig` and a converted history would
diverge from what the next `migrations add` writes. The namespace follows the project,
`Codaxy.Inventory.App.Persistence.Migrations`, which is what that command generates.

**Their designer files and the model snapshot are not history.** Each carries the CLR type names of
the model, so renaming a namespace has to reach them: the snapshot is what EF diffs against. They name
types as strings, so a stale one still compiles and passes every test. The migration bodies name
nothing but tables and columns.

**A schema change is a migration added here**, from `server/`:
`dotnet ef migrations add <Name> --project Codaxy.Inventory.App --startup-project Codaxy.Inventory.Web`.
`MigrationsTests` fails until the model and the history agree. **`dotnet ef` is a local tool**, pinned in
`.config/dotnet-tools.json` and installed by `dotnet tool restore`; its version is the EF Core
packages', and moves with them.

## Seeding

`SeedData()` is idempotent per table — each block runs only when its table is empty — and fills the
codebooks and the company's starting records with the values the product expects. It is safe to run against a
populated database, but it will not repair or update a table that already has any row in it. The
emptiness check takes no lock: two instances starting at once against an empty database can both
insert.

**The settings are seeded per key** (`SettingCatalogue.SeedAsync`): every listed setting a database
lacks is inserted with its default from code; one that exists is never changed, and a row nothing
lists any more is left alone.

## Audit log

`AuditLogInterceptor` is a `SaveChangesInterceptor` registered on the context. For every added,
modified or deleted entity implementing `IIdentifiableReadOnly<Guid>`, it writes one `AuditLog` row
holding the entity's class name, its id, the user's email (`system` without one), `Create` /
`Update` / `Delete`, and the complete before/after property set as JSON. `AuditLog` is excluded so the
log cannot log itself. Rows written in one `SaveChanges` share a `TransactionId`.

**One format for the whole log**, which holds rows from 2022 on: indented, every
property in EF's order — the key, then by name — changed or not; `DateOnly` as `yyyy-MM-dd`; non-ASCII
written as it is. `AuditLogInterceptorTests` holds it to that.

**The user is read when the save happens**, through `ICurrentUser`, which the host implements from the
request in progress. The interceptor is a singleton and knows nothing of HTTP; a save outside a
request records `system`.

**What it cannot record**: a link table's rows — `ElectronicDeviceTypeElectronicDeviceTag` and the
like have a composite key and no id, so tagging a type leaves no trace — and
anything the database cascades without EF tracking it. Seeding a fresh database is audited like any
save.

**Reading it, what changed is computed, not stored**: both documents hold every property, so an update's
changes are the properties whose JSON differs. A foreign key's value is named through the EF model —
the key's target entity and the first of its `Name`, `Text`, `Level`, `Status`, `Substatus` or
`Description` — so every logged entity is covered without a map per entity. An asset's subtype row has
no name of its own and shares the asset's id; it is labelled from the asset, or from the asset's last
logged values once the asset is deleted.

**System saves are left out of the log as a whole** — `system`, saves outside a request, which today
is the seeding at startup — unless the reader includes them (`system=true`): they come in bursts
nobody made, and push people's edits off the first page. The user filter only narrows, so `system`
there finds them only alongside it. **A record's
history keeps them** — the log narrowed to one id or one inventory number, or searched for an id —
since a seeded record's creation is a system save, and without it its history would start halfway,
or say the log never saw it.

## Schema changes wanted

What the database lacks and the application works around; each is a migration of its own.

- **A license may name its software or service.** Optional: a license of one product — most are —
  names it, and its volumes can then only be of that software; one without keeps volumes of any. It
  needs a nullable `SoftwareOrServiceId` on `License`; until then a license is attached to software
  only through its volumes.
- **A person's email is unique**, by an index on `lower(trim(email))`. The application refuses a
  duplicate, and two saves at once pass.
- **A user is not linked to the person they are.** `app_user` and `person` both hold a name and an
  email, and nothing ties them; where it matters — an employee's own equipment, the handover sheet's
  signer — they are matched by email. A nullable `PersonId` on `AppUser` would make it a fact.
- **No index serves any predicate.** Primary keys and the unique `inventory_number` are the whole of
  the indexing, and lists filter, sort and page with sequential scans. At the present size — under a
  thousand assets — that costs nothing measurable.

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

**Renaming an entity or a property splits the audit log.** Earlier rows keep the old name, in
`table` and inside their JSON, and the viewer labels by the current model; a rename migration
rewrites them in the same step. A key renamed in place stays where the old name sorted, and an entry
lists its fields in that order.

**The audit log stores every column of every tracked row, in clear.** That includes license key
identifiers and anything in `Information` marked as personal data. It is readable by every signed-in
user — see [auth.md](auth.md).

**`TransactionId` names something that does not exist.** No code opens a transaction, so the value is
a fresh GUID per `SaveChanges`. It groups a save, and reading it as a business transaction is wrong.


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
