# 0001 — The new application

Rebuild `Codaxy.Inventory` on the same database: same behaviour for the user, different plumbing
underneath, and a client that works on a phone. Each step is a branch and a local plan of its own;
this file holds only what they are and why they come in this order.

## Why

The original cannot be fixed where it stands. Every list endpoint returns its whole table and every
grid sorts and filters in the browser, so paging is not a change to one endpoint but to all of them
and to every screen at once. The client is pre-TypeScript CxJS built on holding whole tables, and its
own notes already say not to invest in it structurally. And the API's shape misleads: an update model
wide enough to edit anything is what an endpoint offers, whatever the domain intends — deactivating an
activation is a `PUT` of an update model on an entity that is immutable by design.

Rebuilding beside it, rather than inside it, buys the one thing an in-place rewrite cannot: both
applications running at once, so the new one can be compared against the old and abandoned if it is
wrong.

## Not negotiable

- **No migration is added until the original is deleted.** Shortcomings in the schema are recorded in
  [co-existence.md](../../engineering/co-existence.md) and left alone.
- **Paging, filtering and sorting happen in the database**, on every list, from the first one.
- **The phone is the hard case**, not an afterthought — a layout that works there is what each screen
  is designed against.
- **Line coverage is at least 80% and CI enforces it**, from the first step rather than as a late
  push — see [testing.md](../../engineering/testing.md).
- **Behaviour matches the original** unless the difference is forced by the above. Better features
  come after parity, not during it.

## The steps

1. **The walking skeleton.** One paged list, one editor, sign-in, the mobile layout, the migration and
   schema-match tests, and a deployable image. Every later step copies its shape, so it is worth
   getting slowly.
2. **Electronic devices.** The largest list and the richest editor: if paging, filtering and the
   phone layout survive this, they survive everything.
3. ~~**Licences, volumes and activations.**~~ The only area with behaviour rather than shape — seat
   counts, activation and its reversal, subscription expiry.
4. **Furniture, the infrastructure — virtual machines, clouds, software — and the remaining asset
   types.** Shapes already proven by step 2.
5. **Information and its types, tags and locations.**
6. **The directory.** Many screens, one shape.
7. **The audit log, the log viewer and Excel export.** The read-only corners, and the ones whose
   behaviour is least worth changing. The audit log was built ahead of the rest, at the user's
   request, and is the first list: step 1's paging convention is the one it set, with a search
   modernised rather than kept at parity. The log viewer followed it, reading a daily JSON-lines file
   the application now writes.
8. **Deprecation.** Delete the original, then the first migration: the deferred schema changes, and
   whatever roles turn out to need.

## Why that order

**The skeleton alone** until it is right, because eight steps copy it. A paging convention settled in
step 1 is written once; settled in step 4 it is rewritten three times.

**Electronic devices before the rest** because it is the worst case for every decision the skeleton
made. Proving them on a small directory screen proves nothing.

**Activations after devices** because they depend on assets, and because they are the only place where
the domain has rules rather than fields — the place to get the transition endpoints right.

**The directory late** despite being easy: they are the least risky and the most repetitive, so they are
what to do when the shape is settled and the work is mechanical.

**Deprecation last and as one act.** Both applications stay reachable until every step above has
landed; a half-migrated user base means neither application is authoritative.

## Ruled out

**Sharing code with the original.** Nothing is referenced or linked across the repositories; the
migrations are copied and `MigrationsTests` proves the copy. A shared library would re-couple the two
and make deleting the original a refactor instead of a deletion.

**Fixing the inventory-number race first.** A database sequence is the only allocation that works for
two applications at once, but it changes the numbering the original produces and only one person uses
either application at a time. It is the new application's to fix after deprecation.

**Sharing sign-in.** It would mean keeping the hand-minted ASOS ticket and its unmaintained validation
package, which is one of the things this exists to leave behind. Signing in twice while both run is
cheaper.

**Schema changes to suit the new application.** See [Not negotiable](#not-negotiable).

## Done when

The original is deleted, and nothing in this repository is phrased as a comparison with it.
