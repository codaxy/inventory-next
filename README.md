# Inventory Next

A rewrite of Codaxy.Inventory against the same PostgreSQL database — paged, phone-first, and honest
about what an endpoint may change. Both applications run until this one reaches parity; see
[docs/engineering/co-existence.md](docs/engineering/co-existence.md).

So far it signs you in and does nothing else.

## Running it

```
dotnet dev-certs https --trust                # once per machine
docker compose up -d                          # PostgreSQL and Mailpit
cd client && npm install && npm start         # watches and serves the bundles, leave it running
cd ../server && dotnet run --project Codaxy.Inventory.Web
```

Open **https://localhost:5443** — the server's own origin, in development as in production. The
application is served from there and only its modules come from the Vite dev server on 8765, so an edit is
live without a rebuild and the session cookie belongs to the same origin it will in production.

Development runs over TLS for the same reason: cookies behave differently without it, and a
difference between development and production shows up as a browser-specific failure nobody can
reproduce. `npm start` exports the ASP.NET development certificate for Vite, so both are
served with the one certificate the trust command above installed.

`appsettings.Development.json` is committed and points at the compose defaults. Sign in with an
address in one of `Auth:AllowedDomains` — `codaxy.com` out of the box — and the code arrives in
Mailpit at http://localhost:8025, whether or not the mailbox exists.

## Google sign-in in development

The button appears only once a client id and secret are configured, and they never go in a settings
file — the secret manager keeps them outside the repository:

```
cd server/Codaxy.Inventory.Web
dotnet user-secrets set "Auth:Google:ClientId" "your-id.apps.googleusercontent.com"
dotnet user-secrets set "Auth:Google:ClientSecret" "your-secret"
```

Restart the server; `GET /api/auth/options` reports `"google": true` and the sign-in screen offers it.

The Google client needs `https://localhost:5443/signin-google` registered as a redirect URI. Without
it the flow reaches Google and comes back as `Error 400: redirect_uri_mismatch` — the application is
fine, the console entry is missing. Only that one URI is needed: the page is served by the server on
5443 whether or not the dev server is running. `dotnet user-secrets list` shows what is set, and `dotnet
user-secrets clear` removes it.

`npm run build` writes to `client/dist` and is only needed for a production-like run; the image does
it. To run the image instead:

```
docker compose --profile app up --build     # http://localhost:8090
```

## Formatting

A pre-commit hook formats what is staged — CSharpier the server, Prettier the client — with the scope
CI checks, so nobody runs either by hand. It is [Husky.Net](https://alirezanet.github.io/Husky.Net/),
a dotnet tool, and the server's first restore installs it: `git config core.hooksPath` then prints
`.husky`. A clone that never builds the server installs it with `dotnet tool restore && dotnet husky
install`. Prettier runs from the client's own install, so `npm install` in `client` has to have run.

To format everything at once anyway:

```
dotnet csharpier format server
cd client && npm run format
```

EF's migrations are excluded — `.csharpierignore` — because the next `migrations add` would undo it.

## Tests

```
cd server && dotnet test Codaxy.Inventory.slnx
```

The integration tests start PostgreSQL and Mailpit through Testcontainers, so Docker has to be
running. Coverage must stay at or above 80%; see [docs/engineering/testing.md](docs/engineering/testing.md).
