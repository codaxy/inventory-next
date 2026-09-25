# Inventory Next

## Before anything else

**Read all of `docs/engineering/` before the first answer** — not the one file that looks relevant. It
is short, it is what the work is judged against, and skipping it produces work that contradicts a
decision already paid for. A one-line instruction and a screenshot count: small work is where the
reading gets skipped and where a recorded rule gets broken unnoticed.

## Terseness

**Say it once, in as few words as carry it.** A rule gets a sentence; a second only if the reason is
not obvious. No paragraph where a clause will do, no example where the rule is plain, no restating
what the diff, the code, or the sentence above already says. Cut the motivation, the contrast and the
recap.

**Terse is not incomplete.** A gap, a deviation, an unmet criterion, anything that would surprise a
reviewer: state it — in a line.

**Write what is, never what changed.** Anything defined against what it replaced — "a setting rather
than an environment check", "no longer a placeholder", "this used to be in the controller" — reads as
nonsense to everyone who arrives after, which is everyone. A code comment is where this hides best,
because it is written in the moment the change is made and nothing ever rereads it against a fresh
pair of eyes. Say what the thing does and why it is not obvious; the diff already holds what moved,
and it holds it better.

The exception is a rejected alternative or a trap — what not to reach for, and what breaks. Those are
still true for someone who arrived today, and the test is exactly that.

This governs everything written: the code and its comments, `docs/engineering/`, the plans, commit
messages, and what is said in the conversation.

## Branches

**Every change happens on a branch. No exemptions** — plan or no plan, prototype, one-line fix,
typo in a document. `main` is where work arrives, not where it is done.

**Check before the first edit:** `git rev-parse --abbrev-ref HEAD`. If it says `main`, branch first.
Noticing late is no reason to continue: `git checkout -b` carries uncommitted changes.

**The mistake comes on the second request, not the first.** Shipping merges and leaves you on `main`,
where the next instruction reads as a continuation. It is new work. Branch again.

A branch lives until its work ships, and is deleted once merged.

## The process

Every piece of work goes through three phases, in order — **analyse, plan, execute** — and then,
usually, a fourth: **polish**.

### 1. Analyse

We work the problem out in conversation before anything is written. The `docs/engineering/` files for
the area are authoritative — if the code contradicts one, say so rather than quietly following either.

Push back when you disagree and say what you would do instead: an analysis that only confirms what I
proposed has not happened. It ends when we agree what to do, why, and what we are deliberately not
doing. Nothing but `TODO.md` changes meanwhile.

### 2. Plan

When the analysis settles, write it up as an enumerated plan in `docs/plans/local/` — the next free
number, named after the work: `0004-fixing-this-and-that.md`. Work too large for one branch gets a
programme in `docs/plans/global/` as well, and a local plan per step.

A plan states what will change, why, and what was ruled out and for what reason. Big enough work
is broken into phases, each of which leaves the app working. Scale the plan to the work: a small
change gets a short plan, not no plan.

**The plan is the spec the implementation follows.** If it turns out wrong while executing,
change the plan and say so — do not quietly drift from it.

### 3. Execute

Work the plan, adjusting **the code and `docs/engineering/` together, in the same turn**. Durable
knowledge only survives in `docs/engineering/`; the plan is thrown away.

Done means: the work is finished and verified, the tests pass, every decision that changed is
reflected in `docs/engineering/`, and the `TODO.md` entry, where there is one, is struck
through. Report what was left undone rather than narrowing the scope silently.

### 4. Polish

Once the work is in front of us, things surface that could not have been seen from the plan — a
value that is wrong in practice, a case nobody thought of, a rule only visible on screen.

**Polish never goes back into the plan** — it recorded what we decided beforehand and is about to be
thrown away.

**Polish worth keeping goes into `docs/engineering/`**, by the same test as everything else there:
would it help someone building this from scratch land it first time? The value that turned out wrong,
the constraint discovered by violating it, the trap that only appears at the end. This is the easiest
knowledge to lose, because by the time it appears the work feels finished. A wording tweak or a nudge
to a margin needs no record.

### Skipping the plan

Two ways past it. **Prototyping**, where the point is to see how something looks: a screenshot settles
in one exchange what a paragraph argues about for three. And a **direct instruction** — "just do
this", "fix that" — small or settled enough that writing it down first would only slow it.

**The branch is not one of the things skipped** — see [Branches](#branches). Without a plan nobody has
agreed what "finished" looks like, and `main` is not where that is discovered. Work that survives is
not finished by virtue of being liked; it still goes through the rules below.

## The layers

| Layer | Holds | Changes when |
| ----- | ----- | ------------ |
| `CLAUDE.md` | how work is done | the process itself changes |
| `docs/engineering/` | what was decided about the system, and why | a decision changes |
| `.claude/skills/` | how a technology is written here, and its pitfalls | a pitfall is learned or a claim goes stale |
| `docs/plans/` | the working plan for a piece of work | every session — disposable |
| the code | the system | continuously |

**Before touching `client/`, load the `cxjs` skill** (`.claude/skills/cxjs/`). Where it and
`docs/engineering/web-client.md` disagree, the document wins and the skill is corrected.

**When a change touches more than one layer, the most general changes first** — this file, then
`docs/engineering/`, then the code. Specific statements derive from general ones; the other order
means writing them twice, and the second pass is the one that gets skipped.

**This file changes only when the process changes**, never when the product does: a new feature, a
new permission, a new `docs/engineering/` file or a defect folded in all leave it alone.

## `docs/engineering/`

The project's durable memory: one file per subject, holding the decisions, conventions and constraints
that shape it — enough for a competent architect to build a system of this kind, not to reproduce this
one file for file. No numbering, no dates, no defect records; history lives in git.
`docs/engineering/README.md` indexes the files and states their shape.

**The test for what belongs: would an ordinary feature or tweak invalidate it?** If yes it is
inventory — a table count, a roster of entities, a map of every file — and it belongs in the
code, where it is already true by construction and cannot rot. If changing it would itself be a
design decision — a permission, a value on a scale, a component of the vocabulary, a rule the
tests enforce — it belongs here, exact values included. Rare, deliberate change is what makes a
statement worth recording, not what disqualifies it.

**Amend, never append.** Change the section that already owns the topic; never describe the new state
beside the old. If a change makes a sentence untrue, delete it — a decision that lands only in the code
leaves the document actively wrong, and the next agent undoes what was just decided.

**Write what is, never what changed** — see [Terseness](#terseness). A rename leaves no contrast
behind, and a decision is not explained by the decision it replaced.

**Fold defects in as lessons, do not record them as events.** When you fix a bug, the durable
part is what would stop it being written again: put that in the *Traps* section of the file that
owns the area, in a sentence or two. Never "fixed in `abc1234`", never a symptom-cause-fix
narrative, never a file per defect.

**Add a file when a subject has no owner** rather than grafting it onto an unrelated one, and index it
in `docs/engineering/README.md` in the same turn.

## `docs/plans/`

Enumerated plans, numbered from `0001` and named after the work. Two folders, each numbered
independently:

- **`docs/plans/global/` is committed, and holds programmes.** One file per body of work too large
  for a single branch: what it is for, the steps in the abstract, their order and the reason for it.
  It says what each step is, never how — the step decides that when its turn comes, knowing things the
  programme could not. Its number is the same in every clone, so it can be referred to by it.
- **`docs/plans/local/` is gitignored, and holds the working plans.** One per piece of work, one
  branch each — including each step of a programme. Numbering is per clone and means nothing to
  anyone else.

**Strike a programme's step through when it ships** — `~~**Its title.**~~`, leaving the reason it is
there legible, and in place: the order is the whole point of the file, so nothing moves. Where a
programme turns out wrong mid-flight, amend it and say so, by the same rule that governs any plan.

Both are **disposable**: anything worth keeping lands in `docs/engineering/` before the work is
finished, and the plan is deleted when its work ships — a programme when its last step does. A
committed plan is deleted the same way; git holds whatever history is wanted, and a folder of finished
plans is a second, rotting account of a system `docs/engineering/` already describes.

## `TODO.md`

**Only if the repository has one** — this project may not, and nothing here creates it. Where there
is no `TODO.md`, every rule below and every reference to it elsewhere in this file simply does not
apply.

A scratch list, not a plan. **One line per entry, extremely terse** — a title, not a description.
Anything needing explanation belongs in a plan, in `docs/engineering/`, or in the commit that does
the work; an entry here only has to be recognisable to someone who already knows what it means.

Prefix defect entries with `Bug:`, and strike entries through with `~~…~~` once they are done.

**Striking an entry through also moves it up**, to the *end* of the done block — directly above the
first thing still outstanding, with everything already done left where it is. Done work collects at the
top in the order it was finished, and what is left stays one unbroken list, which is the only thing the
file is for.

## Committing

**Never commit until I tell you to.** Finish the work, leave it in the working tree and say what is
there; I read the diff before it becomes a commit, and a commit is the one thing that puts it out of
easy reach. Staging is fine. This holds however finished the work looks and however many times I have
said yes before.

When I say something is ready to commit, **bring the documentation up to date first, in the same
commit.**

- **`docs/engineering/`** — every decision this work changed or introduced is reflected, and any
  sentence it made untrue is gone rather than merely outnumbered.

Check it before committing, not after: documentation that lands a commit later does not land, and
neither file is worth having if it cannot be trusted.

**"Ship it" means: commit if it is not already committed, merge to `main`, then push** — the whole way
home in one word, which is what makes branching cheap enough to be compulsory. The checks above come
first; shipping is the whole sequence, not a way of skipping to the end of it.

**It leaves you on `main`, so the next change — a correction to what just shipped included —
starts by branching again.**

## Pushing

**Never push unless I explicitly tell you to.** Committing locally is fine; `git push`, opening a
PR, or anything else that publishes work waits for my say-so every time. Prior permission to push
does not carry over to the next push.

**"Ship it" is that instruction**, and it behaves like any other: it authorises the push in front
of us and no later one.

## Commit messages

**Never sign commits as Claude.** No `Co-Authored-By: Claude` trailer, no "Generated with Claude
Code" line, no AI attribution of any kind — in commit messages, PR descriptions or contributor
lists. Commits are authored solely by the user.

**Follow [Conventional Commits](https://www.conventionalcommits.org/).** The subject is
`type(scope): summary` — imperative, lower-case after the colon, no full stop, 50 characters or
fewer in all. Then a blank line, then a body wrapped at 70 characters.

- **type** — `feat` (user-visible capability), `fix` (defect), `refactor` (no behavioural change),
  `perf`, `test`, `docs`, `build` (dependencies, project files, compose), `ci`, `chore`.
- **scope** — the area touched, named after its `docs/engineering/` file where one owns it
  (`auth`, `persistence`, `web-client`…); omit it when the change spans the repository.
- **breaking change** — `!` before the colon, and a `BREAKING CHANGE:` footer saying what breaks.

**A subject and a handful of body lines.** The reader is a teammate who knows this codebase, .NET
and GitHub: omit what the diff shows, never omit what it cannot — a behavioural change, a migration,
anything surprising, anything left undone. See [Terseness](#terseness).
