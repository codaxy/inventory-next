# Authentication

Sign-in is the whole of the application so far. There is no user table and no roles: who may sign in
is a deployment decision, read from `Auth` in configuration.

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

## Who may sign in

`SignInPolicy` applies three rules in order, whichever provider the person came through:
`AllowedDomains` when it is non-empty, the deny list, then `AllowedUsers` when it is non-empty. One
place holds them so a second provider cannot arrive with its own idea of who is allowed.

**An empty list means no restriction from that rule**, so an instance with nothing configured admits
anyone who can receive email. That is deliberate — an instance already behind an allow list should not
have to name a domain as well — and it is the reason a deployment states at least one of the two.

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
  never that a particular person is not on the list. The first gives someone who mistyped their own
  address something to act on and names nothing; the other two answer questions worth asking, so they
  answer `204` exactly as an accepted address does.

## Development

`appsettings.Development.json` is committed, with one-time codes on and the relay pointed at Mailpit.
It holds no secret, and a checkout that runs and can be signed into is worth more than the convention
of gitignoring that file.

**It does not relax who may sign in.** `AllowedDomains` applies in development as everywhere else, so
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

**There are no roles.** Every signed-in person will be able to do everything, as in the original. The
claims carried are name and email only; anything built on "everyone sees everything" will be revisited
when roles arrive.

**The caller limit counts addresses, not people.** Everyone behind one office address shares a
partition, so the permit count has to allow for however many that is; and everyone on their own
connection gets their own, which an attacker with a pool of addresses also gets.
