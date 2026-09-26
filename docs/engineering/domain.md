# Domain

*Reverse-engineered from the code. It describes what the system does, not decisions recorded when they
were taken; correct it where it is wrong rather than working around it.*

The system tracks assets through their life-cycle, plus the information, licences and contracts
attached to them.

## Asset and its subtypes

`Asset` carries everything common to a tracked thing: inventory number, name, type, substatus,
location, holder (`Person`), vendor, business entity, purchase date and value, invoice number, the
CIA triad and importance, an `Incomplete` flag and `LastModified`.

There are three specialisations — licence, furniture and electronic device — each a **separate table
sharing the asset's primary key**, in a one-to-one owned by `Asset` and cascade-deleted with it. The
subtype's `AssetId` *is* its id, so creating one means creating both rows in a single `SaveChanges`,
and `api/electronicdevices/{id}` and `api/assets/{id}` address the same entity by the same GUID.

Every table has a reader. A fourth specialisation means a table, an API and a screen, not a table
alone — the schema once carried five more that nothing reached, and they were dropped rather than
left to imply features that did not exist.

`AssetType` groups under `AssetCategory`; `AssetSubstatus` groups under `AssetStatus`, and an asset
points at the substatus only, so its status is reached through it.

**Tags attach to device types, never to devices**: a device shows the tags of its type. A tag names a
kind of thing — `Mobile`, `HasData` — and the link table is the whole of the relation.

**A device type says whether its devices hold licences** (`HoldLicences`): only a device of such a type
is offered when an activation is assigned to a device. **A type a device uses is not deleted** — the
device's foreign key does not cascade — while a tag goes with its links.

The client picks the asset type by **name** when creating (`loadAssetType("Electronic Device")`), so
the seeded type names are part of the contract between client and server, not free text.

## Entities that are not assets

`Information` (with its types, tags and locations), `SoftwareOrService`, `Volume`, `Activation` and
`MaintenanceContract` are first-class entities with their own ids. None has an inventory number and
none touches `Sequence`.

`VirtualMachine`, `Cloud` and `Software` are the infrastructure: a virtual machine is a name and an
address, and a cloud or a software entry belongs to a `Volume`.

Licence seats are modelled by three of them: a `Volume` is a quantity of a `SoftwareOrService` bought
under a licence, and an `Activation` assigns one seat of a volume to a person, a device (an asset) or
both. `MaintenanceContract` hangs off an asset, optionally.

**An activation is never edited.** Once created, the only thing that changes is whether it is
deactivated: two transitions, `deactivate` with a date on or after the activation's and `reactivate`
clearing it — each refused from the wrong state — and the page is read-only for an existing record.
Reactivating discards the date the deactivation held, so the pair is the whole of its lifecycle. A
wrong activation is deleted and made again.

**An activation's assignee follows the volume's type**: a person for a per-user volume, otherwise an
electronic device whose type holds licences. **Seats past a volume's quantity warn, they are not
refused** — the original only warned, and over-allocation is recorded rather than prevented. Seats in
use are the sum of the quantities of the volume's active activations.

**A subscription is expired before its date, expires soon within fifteen days of it, and is current
after**; a licence without a date has no status. A licence expiring today is still valid today.
Today is the server's UTC date. Auto-renewal is a recorded flag and changes none of it.

**Importance is computed from the three weights** of confidentiality, integrity and availability —
3–4 Low, 5–7 Medium, 8–9 High — and is absent unless all three are chosen. The server computes it on
every save; the original's client did, and sent the result.

**What stands on a volume keeps it**: its activations, clouds and software entries cascade with it, so
a volume holding any is not removed from its licence, and a licence with such a volume — or with a
maintenance contract — is not deleted. A software or service a volume is of is not deleted either. An
existing volume is kept as it is or removed; editing one is not offered, as in the original.

## Codebooks

The lookup tables — countries, cities, states, currencies, periods, confidentiality, integrity,
availability, importance, licence classes/categories/types/models, business entities — are small lists
of coded values, seeded (see [persistence.md](persistence.md)) and given no screen of their own. They
are referenced by GUID everywhere except where the client resolves one by its text, as with importance
levels.

## The directory

`Person`, `Client`, `Project`, `Vendor`, `Manufacturer` and `Location` are records kept up to date by
hand — a vendor has a VAT number and contacts, a project an owner and a client, a location an address —
which assets, activations and information point at. They are not codebooks, although persons,
locations, manufacturers and vendors are seeded with a starting set.

## Inventory numbers

A single `Sequence` row holds the next asset inventory number. Only the three services that create
assets — licence, furniture, electronic device — read it, stamp the asset and increment it. The number
is `int?` and nullable: assets created outside those paths have none.

## Traps

**Inventory number allocation is not concurrency-safe; the unique index is what saves it.** The
services read the `Sequence` row, increment a **local copy** with `Interlocked.Increment`, assign it
back and save. The interlocked call is on a stack variable and protects nothing, and there is no
transaction or row lock, so two concurrent creates still reach for the same number. The unique index
on `Asset.InventoryNumber` turns that into a unique violation, which nothing maps, so the loser gets a
500 carrying the Npgsql message. That is deliberate: a rare loud failure in place of a duplicate that
cannot be repaired afterwards. Nothing in the codebase opens a transaction at all — an asset and its
subtype row are consistent only because they save together.

**`Sequence` must already contain a row.** The create paths throw `ItemNotFoundException` when it is
empty rather than starting at 1.

**Deleting an asset cascades to its subtype row**, by configuration rather than by anything visible at
the call site. Maintenance contracts are *not* part of that cascade: the foreign key is `ON DELETE
RESTRICT`, so deleting an asset that has one fails outright. The client deleting an asset's contracts
first is therefore required, not a nicety — and nothing in the delete path says so.
