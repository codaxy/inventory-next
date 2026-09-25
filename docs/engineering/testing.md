# Testing

## The threshold

**Line coverage is at least 80%, and CI fails below it.** The original application had no gate at all
— its workflow published an image on every push and never ran a test — so a number that is not
enforced is the same as no number.

What the percentage is measured over decides whether it means anything:

- **Excluded: generated code.** Migrations, their designer files and the model snapshot are thousands
  of lines nobody wrote and nobody can meaningfully test. Counting them would put the figure above 80
  on its own.
- **Excluded: composition.** Startup, dependency registration and configuration binding. They are
  exercised by every integration test that boots the host, and asserting on them tests the framework.
- **Excluded: the persistence carried over from the original.** The entity classes, the context and
  the seed data are declarations, frozen by [co-existence.md](co-existence.md), and `MigrationsTests`
  proves the whole schema they describe rather than a line at a time.
- **Included: everything else.**

The exclusions live in `server/coverlet.runsettings`, so the number CI enforces and the number a
local run prints are the same one.

**Integration tests count.** Most of the coverage comes from tests that boot the application against a
real PostgreSQL container and drive it through HTTP, because that is what exercises paging,
projection and the mapping between models — the places this application is most likely to be wrong.
Unit tests are for logic that has a shape of its own: seat counts, expiry, allocation.

## Formatting

CSharpier formats the server and Prettier the client, both pinned and both checked in CI, so
formatting is never a review comment. **A pre-commit hook applies them**, through husky and
lint-staged at the repository root, to staged files only and with exactly the globs CI checks — a
hook broader than CI rewrites files CI never looks at, and a narrower one lets a commit fail CI.
Prettier runs from the client's own install, so its version is pinned once. EF's migrations are
excluded — the next `migrations add` would undo it.

## Naming

A test is named as a sentence, with underscores: `Refuses_a_code_once_it_has_expired`. A failing test
is read in a list of failures, where a sentence says what broke and `RefusesACodeOnceItHasExpired`
has to be deciphered first.

## What the percentage cannot be allowed to hide

A percentage rewards testing what is easy to test. These are covered whatever the number says:

- **Sign-in.** The original replaced it wholesale with a test handler, so token minting, validation
  and the allowed-user rules were exercised by nothing end to end. This application mints its own
  tokens; the path is covered here.
- **Every state transition endpoint**, in both directions, including the one that undoes.
- **Every paged list**, at the boundaries: the first page, the last, past the last, and the page size
  refused as too large.
- **The schema match.** `MigrationsTests` runs the copied history and compares the result against the
  model. It is not coverage, it is the thing that catches the copy drifting from the original — see
  [co-existence.md](co-existence.md).
