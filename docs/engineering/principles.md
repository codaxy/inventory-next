# Principles

What this application is built to be, as against the one it replaces. Each of these is here because
it was got wrong first.

**Take a dependency when it earns its place, not before.** A library added for what it might be
needed for later brings its configuration, its idioms and its version with it, and the code written
around it is written around a decision nobody made. The framework is usually enough for the thing in
front of you, and the moment something genuinely needs more is the moment to choose.

**Inherit nothing unexamined.** Code carried across from elsewhere arrives with the reasoning that
produced it missing. It is a liability until someone can say why it is right here, and copying it is
not that.

**A setting that must be supplied is absent, not filled in.** A placeholder is a valid value: it
travels into whatever consumes it and comes back as a parse error about a typo that does not exist.
Leave it empty and check it where the application starts, so it names itself when it is missing.

**A checkout runs.** Defaults that hold no secret belong in the repository, however strong the habit
of ignoring the file they live in — a sample that has to be copied is a sample that will be copied
wrong. Credentials go somewhere they cannot be committed.

**Let configuration decide what exists, rather than a flag describing it.** A feature that is on
because its settings are present cannot be half-configured; a feature with a flag beside its settings
can be on with nothing behind it, and the failure surfaces at whichever service it calls.

**Generated code is left as it is generated.** Conventions apply to what is written by hand; imposing
them on what a tool rewrites buys one tidy diff and a conflict every time the tool runs again. Where
the two meet, the boundary is worth stating out loud.

**An interface offers exactly what its job needs.** What a request model carries is what a caller may
change, and what an endpoint advertises is what someone will eventually do with it. Width that is not
needed today is permission granted by accident.

**A rule that has exceptions is two rules.** "Time-ordered ids, except where ordering does not
matter" leaves a table holding both kinds and nobody able to tell them apart, and the same shape
appears wherever a convention is allowed a sensible-sounding escape. Either it holds everywhere or it
is not the rule.

**Settle it by running it.** A claim about how a framework behaves, what a tool generates, or what a
round trip does to a value is cheaper to test than to argue, and the test is usually a few minutes.
Most of the rules above were written after the opposite turned out to be true.
