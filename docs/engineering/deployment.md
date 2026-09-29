# Deployment

One image, built by a three-stage Dockerfile: node builds the client into the path the server stage
publishes from, the SDK publishes the server, and the ASP.NET runtime image carries the result. It
runs as `$APP_UID`, not root.

## Publishing

**The image is published by CI, and only once the tests have passed**: the `publish` job in
`ci.yml` needs the server and the client jobs, and runs on a push to `main` or `production` of
Codaxy's own repository — a fork or a copy elsewhere cannot push to Codaxy's registry —
`ghcr.io/codaxy/inventory-next:latest` from `main`, `:stable` from `production`, as the original
tags its image, and every image also carries its commit's sha, so any one can be run again. The
original published before any test ran; here a red build publishes nothing. The name is not the
original's `ghcr.io/codaxy/inventory`, so neither workflow overwrites the other's image and a
deployment switches between the two by changing one line.

**The version is the commit's, stamped when CI builds the image** — never a commit of its own:
`2026.929.1222+eaea98a`, the commit's UTC time as year, month and day, hour and minute, then its
sha. Each part drops its leading zeros (09:05 is `905`): .NET caps a version part at 65535 and SemVer
refuses a leading zero, and in this form the part before `+` is both, and sorts as time does. From
the commit, not the build, so `main` and `production` build one commit as one version. The
Dockerfile's `APP_VERSION` argument carries it; unstamped, it is `dev`. `/api/version` answers it
without a session, and the account menu shows it under Sign out.

## Compose

**`docker compose up` brings up infrastructure only** — PostgreSQL and Mailpit — which is what running
the application from an IDE needs. **`docker compose --profile app up` runs the built image beside
them.**

Ports are deliberately not the original application's, so both stacks can run at once: PostgreSQL on
55432, pgAdmin on 55050, the application on 8090, Mailpit's web interface on 8025 and its relay on
1025.

pgAdmin runs in desktop mode — no sign-in, no master password — and `docker/pgadmin/servers.json`
pre-registers the database so nobody retypes what compose already knows. It, and PostgreSQL's
published port, are conveniences for a laptop and wrong for anything exposed.

## Configuration and secrets

`appsettings.json` carries defaults only, and **the repository holds no credentials.** A setting that
must be supplied is present and empty rather than filled with a placeholder string: a placeholder is a
valid value, so it reaches whatever consumes it and comes back as a parse error instead of "this is
not configured". `ConnectionStrings:PostgreSQL` is checked at startup and names itself when it is
missing.
Google is off until a client id and secret arrive, which in compose is through the environment:

```
GOOGLE_CLIENT_ID=… GOOGLE_CLIENT_SECRET=… docker compose --profile app up
```

**`Dashboard:DisposedLocations` names the locations assets are moved to when written off or sold**,
by name, since names differ between databases; empty by default, and then the dashboard has no section
for seats left on them. Development names the restored database's `Otpisano` and `Prodano`.

**`Dashboard:SubscriptionsExpiredDays`, `SubscriptionsExpiringDays`, `WarrantiesEndedDays` and
`WarrantiesEndingDays` are the dashboard's windows**, each on its own — 45, 15, 45 and 30 in
`appsettings.json`. One below 1 stops the app at start.

`appsettings.Development.json` is committed and holds no secret: the compose connection string,
Mailpit and one-time codes on, so a fresh checkout runs. Development credentials live in user
secrets; see [auth.md](auth.md).

**A deployment behind a reverse proxy sets `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`; the image
does not.** It trusts `X-Forwarded-For` and `X-Forwarded-Proto` from any sender, known-proxy lists
cleared — which the Google redirect URI, the cookie's `Secure` flag and the sign-in rate limit need
behind a proxy, and which lets anyone reaching the port directly spoof the address and the
scheme.

The data protection key ring lives on a named volume at `DataProtection:KeyRingPath`
(`/var/lib/inventory/keys`). The image creates that directory and gives it to the application's user
before the volume is mounted over it — a fresh named volume takes its ownership from the image, and
the container does not run as root.

## Health

Two endpoints, because the two questions have different answers. `/health/live` carries no checks and
says the process is up; it is what compose probes, since restarting the container cannot fix a
database that is down. `/health/ready` runs the database check and is what a load balancer should ask
before sending traffic.

## Logs

Code logs through the framework's `ILogger`, filtered by `Logging:LogLevel`, to two places: the
framework's console logger, so `docker compose logs` works, and the server log. **Request lines are
Development's only**: one combined line per request through the framework's HTTP logging — method,
path, status, duration, no headers and no bodies, because a request body here is a sign-in attempt —
enabled by `appsettings.Development.json`, while `appsettings.json` holds the category at `Warning`.
Elsewhere they drown what the log is for. A setting, not an environment check: a deployment that wants
them sets `Logging__LogLevel__Microsoft.AspNetCore.HttpLogging=Information`.

**One set of levels for every provider.** Serilog's own `MinimumLevel`/`Override` configuration is not
used: it would be a second vocabulary for the same rules. A level for the file alone goes in the
framework's provider section, `Logging:Serilog:LogLevel`.

**The server log is Serilog's file sink**, added as a provider directly rather than through
`AddSerilog`, whose own "everything" filter outranks `Logging:LogLevel`. Nothing outside
`ServerLogSetup` names Serilog. **One rendered compact-JSON object per line**: a newline in a logged
value stays escaped inside its string, so nothing logged can start an entry of its own — plain-text
lines are what make log injection possible. **A file per day**, `server-yyyyMMdd.log`, rolling to
`_001` past 50 MB, deleted after `ServerLog:RetentionDays` (30). **Shared**: the sink appends through
the operating system, so a second writer — a restart overlapping the old process, two instances on one
volume — cannot interleave with it. Unshared, it writes at a position it tracks itself, and two writers
overwrite each other's lines into fragments.

**`ServerLog:Path` is outside `wwwroot`, and startup refuses otherwise.** The files are read only
through the API, behind its own policy; a folder the static-file middleware serves would hand them to
anyone. The image sets it to `/var/lib/inventory/logs`, which it creates and hands to the application's
user, and compose puts that on the `server_logs` volume beside the key ring, so a deploy keeps it; a
checkout writes to `logs/` under the content root.

## Traps

**MSBuild reads environment variables as properties, ignoring case.** A Docker build argument is in
the environment of every `RUN`, so one named `VERSION` becomes `$(Version)` in every project — a
number parses and hides it, anything else fails the publish. The version travels as `APP_VERSION`.

**`dotnet run` is Production without `launchSettings.json`.** The environment comes from there, so a
missing or renamed profile means development settings are never loaded and the application stops at
the connection string it cannot find.

**Both applications migrate the same database at startup.** The connection string in compose points
at this stack's own PostgreSQL, which is empty until something restores into it — it is not the
original's database. Pointing both at one database is the co-existence arrangement, and it needs the
migration histories to be identical; see [co-existence.md](co-existence.md).

**A log folder the process cannot write is an empty server log, not an error.** Serilog's file sink
swallows the failure, so the screen shows nothing and startup succeeds; `logs/` under the image's
`/app` is root's.

**Mailpit accepts everything and delivers nothing.** One-time codes in development are read from its
web interface on 8025, never from a mailbox.
