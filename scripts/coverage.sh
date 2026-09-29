#!/usr/bin/env bash
# The server's tests with line coverage, failing below the 80% in docs/engineering/testing.md. CI and
# the pre-merge check both run this, so a local pass is CI's pass. Extra arguments go to `dotnet test`
# (CI passes --no-build after its own build step).
set -euo pipefail
cd "$(dirname "$0")/.."

# Reports from an earlier run would be merged into this one's figure.
rm -rf coverage

dotnet tool restore >/dev/null
dotnet test server/Codaxy.Inventory.slnx --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory coverage \
    --settings server/coverlet.runsettings \
    "$@"

dotnet reportgenerator -reports:'coverage/**/coverage.cobertura.xml' \
    -targetdir:coverage/report -reporttypes:TextSummary >/dev/null
# The summary block only; the per-class table beneath it is in coverage/report.
sed '/^$/q' coverage/report/Summary.txt
line=$(grep -m1 'Line coverage:' coverage/report/Summary.txt | grep -oE '[0-9]+(\.[0-9]+)?')
awk -v c="$line" 'BEGIN { exit (c + 0 >= 80) ? 0 : 1 }' \
    || { echo "Line coverage $line% is below the 80% the engineering notes require."; exit 1; }
echo "Line coverage: $line%"
