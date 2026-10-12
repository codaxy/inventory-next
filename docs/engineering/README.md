# Engineering notes

What was decided about this application and why — enough for someone building a system of this kind to
land it first time. One file per subject. Decisions, conventions, constraints and traps only: no
history, no defect records, no inventories that an ordinary feature would invalidate.

The database predates the application — it holds rows from 2022 on — and `domain.md` and
`persistence.md` describe much of it as found, and say so; where one and the database disagree, the
database is what runs.

Build and run instructions live in the root `README.md`; the process for doing work lives in
`CLAUDE.md`.

| File | Subject |
| ---- | ------- |
| [principles.md](principles.md) | What this application is built to be |
| [domain.md](domain.md) | `Asset` and its one-to-one subtypes, the entities that are not assets, codebooks, the company, inventory numbers |
| [persistence.md](persistence.md) | EF Core and Npgsql, UUID v7 keys, migrate-and-seed at startup, the audit log, schema changes wanted |
| [server.md](server.md) | The `Web` host and the `App` domain, one folder per menu item, where an entity lives, naming, the list convention |
| [auth.md](auth.md) | The session cookie, who may sign in, Google, one-time codes |
| [web-client.md](web-client.md) | CxJS with TypeScript, the route folder convention, the phone-first layout, the build |
| [deployment.md](deployment.md) | The image, compose and its profiles, ports, where credentials come from |
| [testing.md](testing.md) | The coverage threshold and what it is measured over, and what it may not hide |
