# Testing

## The threshold

**Line coverage is at least 80%, checked before a branch merges and again in CI.** The original
application had no gate at all — its workflow published an image on every push and never ran a test
— so a number that is not enforced is the same as no number. **One script computes it,
`scripts/coverage.sh`**, and both run it: the pre-merge check (`scripts/premerge.sh`) on any branch
touching `server/`, and CI's server job. CI alone notices too late — shipping pushes before it runs.
A branch that cannot touch the figure skips it: it measures the server alone.

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

The exclusions live in `server/coverlet.runsettings`. **Not coverlet's `Threshold` property**: it
judges each test project alone, and the 80% is over the unit and integration projects together.

**Integration tests count.** Most of the coverage comes from tests that boot the application against a
real PostgreSQL container and drive it through HTTP, because that is what exercises paging,
projection and the mapping between models — the places this application is most likely to be wrong.
Unit tests are for logic that has a shape of its own: seat counts, expiry, allocation.

**The test projects divide by what a test needs to run, not by what it covers.** The unit project
references `App` and `Web` but needs nothing — no Docker, no host — so it always runs; without the
host-testing package it cannot boot the application. The integration project references `Web`, starts
PostgreSQL through Docker and drives the host over HTTP.

## Isolation

**Every factory that starts the application gives it scratch folders** — the key ring, and the server
log through `UseScratchServerLog()`. The log's default is `logs` under the content root, which is the
source tree: a test host left on it writes into the developer's own log, beside a running `dotnet run`.

**A fixture another extends keeps its ids per instance, not static.** Two fixtures run in parallel,
each against its own database; a static field set in `InitializeAsync` is overwritten by whichever
starts last, and the other's tests then read ids from the wrong database — passing or failing by
the order they happened to start in.

**The PDF test prints through a real browser** — `ChromiumPrintingTests`, on Kestrel at a real port,
against the built client in `wwwroot` — and runs only where `INVENTORY_TEST_CHROMIUM` names a
Chromium binary and the client is built there; CI does both. Not `Pdf__ChromiumPath`: every fixture
reads the environment, and the others are written against a server without a browser. Elsewhere the
PDF endpoint is tested against a recording printer. A local run skips the browser's lines, so its
figure is below CI's, never above.

## Formatting

CSharpier formats the server and Prettier the client, both pinned and both checked — before every
merge by `scripts/premerge.sh` and in CI, with the same commands — so formatting is never a review
comment. **The check is over the tree, not a record of the hook**: a hook not installed, skipped or
unstable in one pass is caught all the same, and fixed by running the formatter and committing —
never by redoing the hook's job by hand. **A pre-commit hook applies them**, which usually makes
the check a formality: Husky.Net, a dotnet
tool beside CSharpier, so the repository root carries no `package.json`. It runs on staged files
only, with exactly the globs CI checks — a hook broader than CI rewrites files CI never looks at,
and a narrower one lets a commit fail CI — then re-stages them. Prettier runs from the client's own
install, so its version is pinned once. The server's restore installs the hook through a target in
`Codaxy.Inventory.Web.csproj`; `HUSKY=0` turns that off in the image build and CI, which have no
repository to hook.

**The hook formats moved files too**: Husky.Net's own `${staged}` lists only added and modified
files, so a file renamed — or renamed and edited — in a commit went through unformatted and failed
CI, as every file a folder rename touched did. `task-runner.json` defines `${staged-with-renames}`
(`--diff-filter=ACMR`) and every task uses it. **Prettier is not always stable in one pass** on a
long SCSS value — a wide `grid-template-columns` — and the hook runs it once: write such a value
across lines as Prettier's second pass has it, or CI's check disagrees with the committed file.

**Re-staging adds whole files.** A file committed with only some of its hunks staged goes in with all
of them once the hook has touched it; lint-staged stashes the rest first, Husky.Net does not. EF's migrations are
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
