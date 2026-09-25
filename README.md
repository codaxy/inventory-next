# Inventory Next

A rewrite of Codaxy.Inventory against the same PostgreSQL database — paged, phone-first, and honest
about what an endpoint may change. Both applications run until this one reaches parity; see
[docs/engineering/co-existence.md](docs/engineering/co-existence.md).

So far it signs you in and does nothing else.

## Running it

```
docker compose up -d                          # PostgreSQL and Mailpit
cd client && npm install && npm start         # watches and serves the bundles, leave it running
cd ../server && dotnet run --project Codaxy.Inventory
```

Open **http://localhost:5080** — the server's own origin, in development as in production. The
application is served from there and only its bundles come from the watcher on 8765, so an edit is
live without a rebuild and the session cookie belongs to the same origin it will in production.

`appsettings.Development.json` is committed and points at the compose defaults. Sign in with an
address in one of `Auth:AllowedDomains` — `codaxy.com` out of the box — and the code arrives in
Mailpit at http://localhost:8025, whether or not the mailbox exists.

## Google sign-in in development

The button appears only once a client id and secret are configured, and they never go in a settings
file — the secret manager keeps them outside the repository:

```
cd server/Codaxy.Inventory
dotnet user-secrets set "Auth:Google:ClientId" "your-id.apps.googleusercontent.com"
dotnet user-secrets set "Auth:Google:ClientSecret" "your-secret"
```

Restart the server; `GET /api/auth/options` reports `"google": true` and the sign-in screen offers it.

The Google client needs the matching redirect URI registered, and **which one depends on how you are
running the client**, because the handler builds it from the host that asked:

- `http://localhost:5080/signin-google` — server serving the built client, the command above.
- `http://localhost:8765/signin-google` — webpack dev server proxying to it, `npm start`.

Register both and either loop works. `dotnet user-secrets list` shows what is set, and `dotnet
user-secrets clear` removes it.

`npm run build` writes to `client/dist` and is only needed for a production-like run; the image does
it. To run the image instead:

```
docker compose --profile app up --build     # http://localhost:8090
```

## Formatting

CSharpier formats the server, Prettier the client, and CI fails on either being out of shape:

```
dotnet tool restore && dotnet csharpier format server
cd client && npm run format
```

EF's migrations are excluded — `.csharpierignore` — because the next `migrations add` would undo it.

## Tests

```
cd server && dotnet test Codaxy.Inventory.slnx
```

The integration tests start PostgreSQL and Mailpit through Testcontainers, so Docker has to be
running. Coverage must stay at or above 80%; see [docs/engineering/testing.md](docs/engineering/testing.md).
