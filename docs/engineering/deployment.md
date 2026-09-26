# Deployment

One image, built by a three-stage Dockerfile: node builds the client into the path the server stage
publishes from, the SDK publishes the server, and the ASP.NET runtime image carries the result. It
runs as `$APP_UID`, not root.

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

`appsettings.Development.json` is committed and holds no secret: the compose connection string,
Mailpit and one-time codes on, so a fresh checkout runs. Development credentials live in user
secrets; see [auth.md](auth.md).

`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` is set in the image rather than configured in code: it
enables the forwarded-headers middleware for `X-Forwarded-For` and `X-Forwarded-Proto` with the
known-proxy lists cleared, which is what running behind a reverse proxy needs and what running
without one should not have.

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
framework's console logger, so `docker compose logs` works, and the server log. Requests are one
combined line each through the framework's HTTP logging — method, path, status, duration, no headers
and no bodies, because a request body here is a sign-in attempt.

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
anyone. Compose puts it on the `server_logs` volume, beside the key ring, so a deploy keeps it; a
checkout writes to `logs/` under the content root.

## Traps


**`dotnet run` is Production without `launchSettings.json`.** The environment comes from there, so a
missing or renamed profile means development settings are never loaded and the application stops at
the connection string it cannot find.

**Both applications migrate the same database at startup.** The connection string in compose points
at this stack's own PostgreSQL, which is empty until something restores into it — it is not the
original's database. Pointing both at one database is the co-existence arrangement, and it needs the
migration histories to be identical; see [co-existence.md](co-existence.md).

**Mailpit accepts everything and delivers nothing.** One-time codes in development are read from its
web interface on 8025, never from a mailbox.
