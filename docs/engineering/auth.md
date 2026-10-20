# Authentication

Who may sign in and what they may do. The allowed domains set the outer limit, and inside it a user
added by an administrator decides, through the role it holds.

## The session

A cookie, `inventory.session`, issued by the standard cookie handler — not a hand-minted ticket. It is
`HttpOnly`, so no script on the origin can read it, and `SameSite=Lax`, which is the weakest setting
the Google redirect tolerates: the OAuth return is a top-level `GET` that `Strict` would strip the
cookie from. `Lax` still refuses to travel with a cross-site `POST`, and that is what stands in for
anti-forgery on the write endpoints.

Fourteen days, sliding. The API answers `401` rather than redirecting to a sign-in page, because the
client decides what to show.

**The cookie is protected by the data protection key ring**, so the ring has to outlive the
container: `DataProtection:KeyRingPath` on a named volume, and the application name pinned to
`Inventory` so the discriminator does not move with the content root. Lose the ring and every session
ends at once; leave the path unset and the keys land in the container filesystem, where replacing the
container — which is what deploying does — discards them.

The ring is plaintext XML at rest. There is no DPAPI on Linux and no key-encrypting certificate, so
anyone who can read the volume can mint a session for any user. Treat it as a credential: it must not
reach a backup or an image that travels further than the server.

**The server's PDF printer signs in as the caller, with the caller's own cookie**: the session
cookie of the request for the PDF is set on the print's own incognito context, for the server's
local address, and goes with it. The ticket is protected by the key ring, not bound to a host, so a
cookie issued for the public host opens the page on `127.0.0.1`. **Never a path that trusts a local
caller instead**: forwarded headers are trusted from any sender, so anyone reaching the port can
claim to be local. Not a one-time print ticket either: a second sign-in path to build and secure,
where the cookie reuses the one that exists.

## Who may sign in

**`SignInPolicy` applies the domains**, a setting (`SignInSettings.AllowedDomains`, under
Administration → Settings), whichever provider the person came through; empty, any domain — the users
are the allow list, so the domains only narrow who can be added. Read per sign-in, so a change applies
to the next. **A save that would shut out an active System Admin is refused**, naming them: nobody
else could put the domains back but by SQL. Saved as a domain is written in an address — lower-case,
no `@`, once each. One place holds it so a second provider cannot arrive with its own idea of
who is allowed. **Inside it, being an active user decides** (`UserAccess`): both providers end there
once the address is proven.

**Employees sign in as the person they are, not as a user** — no user is made for each person, which
would be a second list of the same people to keep in step. With `SignInSettings.People` on (off by
default, the master switch), a person whose email matches the address, trimmed and whatever the case,
and who may sign in (`Person.CanSignIn`) gets a session of their own: "My inventory", nothing else.
**`UserAccess.MaySignInAsync` is the one answer** — for a code, for Google, and for the cookie on every
request, so turning either off ends a session on the next one. **A person's session holds no
permission**: `CurrentAsync` is a user's access only, so every `/api` endpoint refuses it but one,
`/api/my-equipment`, which answers only about the session's own person — the identity stands for the
permission, and it is on the permission test's short list for that reason. `/api/auth/me` says which
kind is signed in. **A user who is a person too** — the same email — signs in as the user, and sees
their own equipment as well: `UserAccess.EquipmentHolderAsync` answers the endpoint with the person
kept under a user's email whatever that person's `CanSignIn` and the switch, which decide who may sign
in and a user already has; `/api/auth/me` says so (`myEquipment`), and "My inventory" is in their
account menu and in a last section of their menu, "Personal", apart from the work. A user who is no
person is refused it and never shown it. **`CanSignIn` is a decision about that person**, true for
everyone until it is turned off — a leaver, a placeholder holder — and never derived from the setting,
which would leave everyone added while it was off shut out once it is on for no reason anyone made.

**Someone who is not a user is told so only after proving the address** — "ask an administrator" on
verifying a code, `/sign-in?error=no-access` from Google. Requesting a code still answers `204` for
anyone in the domain, and sends the code, so the endpoint names nobody.

**There is no allow list in configuration**: the users are the allow list and deactivating is the
deny list, both kept in the application. A deployment moving from one names its addresses in
`Auth:SeedUsers` instead.

## Users, roles and permissions

**The code checks a permission, never a role; a role is a named set of permissions; a user holds
one.** The catalogue is `Shared/Access/Permissions`, **coarse, as the application is used**:
`inventory.view` and `inventory.edit` — every section and the dashboard; nobody views or edits part of
the inventory and not the rest — then `users.manage`, `roles.manage`, `settings.manage`,
`audit_log.view` and `server_log.view`, in the menu's order. **A permission nothing checks is not in it.** Each names what a screen groups it
under, so a view and its edit are ticked side by side. **The audit log is apart from the inventory**:
it holds who changed what, and deleted records whole, which seeing every record does not.

Not per section: the per-section split was built first and dropped — it served nobody, and every
record page that shows another section's records would have had to be trimmed for it.

**A section's endpoints carry the inventory's permissions by method** — `RequirePermissions(view,
edit)` on the section's group: a `GET` reads and needs `view`, any other method writes and needs `edit`, and a write
in a section without `edit` stops the start. Administration's items carry their own, and the Users
Roles and Settings items need `users.manage`, `roles.manage` and `settings.manage` to read as well,
since who has access, and to what, is as much the job as changing it.
The host registers a policy per catalogue entry and none for anything else, so a key outside it fails
the request. **A test enumerates every `/api` endpoint** and fails on one without a catalogue
permission, apart from `/api/version`, `/api/auth/*` and the documents' settings.

**Roles are rows**: `role` and `role_permission`, which has an id of its own so the audit log records a
permission granted or revoked. Three are built in and found by name, which therefore never changes:
**System Admin** grants every permission, including ones added later — the seeder keeps its rows in
step with the catalogue so a screen shows the truth, and nothing about it changes; **Admin** views and
edits the inventory, reads the audit log and manages users; **Supervisor** views the inventory, and
not the audit log. A built-in role missing from a database is seeded with its defaults; one that exists
is left as it is, so its permissions are an administrator's to change. A stored permission the
catalogue no longer has is dropped from every role — but for the per-section keys of before
(`Permissions.Legacy`): a built-in role still holding one goes back to its defaults, since nobody
could edit one then, and a custom role's become `inventory.view` and `inventory.edit`. That
translation is deletable once every database has started with it.

**Roles are edited under Administration → Roles, with `roles.manage`**, which only System Admin holds
by default. A role is a name, a description and its permissions; a new one starts with none. **An
`edit` comes with its `view`** (`Permissions.Implied`): an editor cannot load the record it
edits, so a role saved with one and without the other is refused. A built-in role keeps its name and
offers **Reset to default** — its description and permissions back to what a new database gets; a
custom role is deleted once nobody holds it. **The editor's own limits** are the users' subset rule
turned on roles — a permission is granted only by someone holding it, and a role granting more than
its editor holds is out of their reach — and **nobody edits, resets or deletes the role they hold**:
the subset rule stops them granting themselves more, not taking `roles.manage` off their own role or
changing what everyone else holding it may do. System Admin has no editor. The roles list and the
users' role picker share one order: System Admin, the other built-in roles, then the rest by name
(`BuiltInRoles.Rank`).

**A user is a row, `app_user`** — not `user`, which PostgreSQL reserves, so hand-written SQL against it
would fail unquoted. Its email is stored trimmed and lower-case, unique, and fixed once added; its name
is typed by whoever adds it, never taken from a provider, since it would then depend on which way
someone first signed in; the application shows that name, the account menu included. A user is never
deleted, only deactivated.

**The user is read from the database on every request, uncached**: `OnValidatePrincipal` signs out a
cookie whose user is deactivated or gone, and the permission handler reads the role for the request,
so either change applies on the next one. One query, shared by both through the scoped `UserAccess`.
`/api/auth/me` answers the name, the role and its permissions, for the screens to follow; the server
checks every request regardless.

**Bootstrap is `Auth:SeedUsers`** — email, name and a role's name — creating each user that does not
exist at startup and never changing one that does; a role it does not know stops the start. **Empty in
`appsettings.json`**: a named person there would be every deployment's System Admin. **A start with no
active System Admin stops**, naming the setting. Development seeds one, so a checkout runs.

**Nobody grants what they do not hold** — the subset rule: a user is added, given a role or changed only
if everything the role grants is the manager's own, and managed at all only if their current role's is.
So nobody acts on someone more powerful, and System Admin is touched only by System Admin. **Nobody
changes their own user but its name**, and the last active System Admin keeps the role and stays
active. Checked on the server; the screens offer only what passes. The checks take no lock: two System
Admins demoting each other at once can both pass. The way back is SQL — set a row of `app_user` to
System Admin's `role_id` and `is_active`.

## Google

Enabled by the presence of `Auth:Google:ClientId` and `ClientSecret`, not by a flag. A handler with no
credentials would advertise a provider that fails at Google rather than one that is simply not
offered, and the repository holds no secrets, so the default state is off.

The OAuth hop lands in a separate `External` cookie; `/auth/google/callback` reads it, applies the
policy, and only then issues the session. The correlation cookie that carries the flow across is
`SameSite=Lax`, which the top-level redirect back satisfies. The alternative — letting the handler sign the person in
directly — would mean revoking a session already granted.

## One-time codes

`Auth:OneTimeCode:Enabled` turns them on, because they need no secret and would otherwise always be
available. Six digits, ten minutes, emailed through SMTP.

Deliberately the simplest thing that works, and temporary:

- **Codes live in memory**, so a restart invalidates every outstanding one and a second instance
  cannot verify a code the first issued. `IOneTimeCodeStore` exists so that decision can be retaken
  without touching the endpoints.
- **Any failed attempt burns the code**, not just a correct one. That is what stops six digits being
  guessed; the cost is that a mistyped code means asking for another, and the cooldown means waiting
  for it.
- **Requesting a code says only that the domain is refused**, never which domain would be accepted and
  never that a particular person is not a user. The first gives someone who mistyped their own
  address something to act on and names nothing; the other two answer questions worth asking, so they
  answer `204` exactly as a user's address does.

## Development

`appsettings.Development.json` is committed, with one-time codes on, the relay pointed at Mailpit and
`developer@codaxy.com` seeded as System Admin. It holds no secret, and a checkout that runs and can be signed into is worth more than the convention
of gitignoring that file.

**It does not relax who may sign in.** The domain and the users apply in development as everywhere else, so
the rules being exercised are the rules that run in production — a development configuration that lets
anyone in is a configuration nobody has tested.

**Google credentials in development go in user secrets**, never in a settings file:

```
dotnet user-secrets set "Auth:Google:ClientId" "…"
dotnet user-secrets set "Auth:Google:ClientSecret" "…"
```

Nothing else about the application is configured that way, and nothing secret may be added to the
committed file — the moment something is, it is in the history.

## Rate limits

Two, because they stop different things.

**Per caller**, on both one-time-code endpoints: `RateLimit:SignIn`, ten attempts in five minutes,
partitioned by the caller's address so exhausting the permits locks out nobody else. A rejected
request answers `429` with `Retry-After`. Nothing else in the application is limited.

**Per address**: `Auth:OneTimeCode:Cooldown`, a minute between codes for one email whoever asks for
them. The caller limit alone would leave someone's mailbox open to anyone willing to change address.
Consuming a code clears the cooldown, so signing out and back in does not wait.

Both are configured rather than compiled in, because the right numbers depend on how many people sit
behind one address — which the application cannot know.

## Traps

**The caller limit counts addresses, not people.** Everyone behind one office address shares a
partition, so the permit count has to allow for however many that is; and everyone on their own
connection gets their own, which an attacker with a pool of addresses also gets.
