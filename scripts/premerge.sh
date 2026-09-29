#!/usr/bin/env bash
# What CI checks, run before a branch merges to main (CLAUDE.md, Committing): the formatting always,
# the coverage threshold when the branch touches server/. It checks the tree, whatever the pre-commit
# hook did or did not do.
set -euo pipefail
cd "$(dirname "$0")/.."

# The checks read the working tree; what merges is the commits.
if ! git diff --quiet HEAD; then
    echo "Uncommitted changes: commit them first, so the check sees what will merge."
    exit 1
fi

fail() { echo "premerge: $1"; exit 1; }

dotnet tool restore >/dev/null
dotnet csharpier check server || fail "server formatting — run: dotnet csharpier format server"
(cd client && npm run --silent format:check) || fail "client formatting — run: npm run format (in client/)"

# An error here must not read as "server/ touched" or "untouched": either would be a guess.
base=$(git merge-base main HEAD) || fail "no main branch to compare the branch against"
if git diff --quiet "$base" HEAD -- server/; then
    echo "premerge: server/ untouched, coverage skipped."
else
    scripts/coverage.sh || fail "coverage below the threshold, or a test failed."
fi

echo "premerge: passed."
