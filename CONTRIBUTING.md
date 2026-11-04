# Contributing to Helfer

Thanks for your interest in contributing! Helfer is a set of small, offline
pocket tools for fire brigades, in one Android app — every contribution, from a
typo fix to a whole new helper, is welcome.

It shares its stack, look and rules with
[Lagebuch](https://github.com/CodeForFire/lagebuch). If you have worked there,
you already know your way around here.

## Your first contribution

**You do not have to write C#.** The most valuable contribution is often the
idea for a helper, or better German wording on a screen used with gloves on in
the rain. If you are a firefighter rather than a developer, that is where your
judgement beats ours. Propose it under
[Ideen](https://github.com/CodeForFire/helfer/discussions/categories/ideas),
in German or English.

Want to build a helper? [`AGENTS.md`](AGENTS.md#helpers) lists every place a new
helper touches; the Stoppuhr under `src/Helpers/Stoppuhr/` is the pattern to
copy. Open an issue first, so the idea is agreed before the code is written.

## Language

**Issues, pull requests and commit messages are written in English** — title and
body. The UI and the release notes are German; the issue tracker is where
someone who does not speak German has to be able to work.

German terms of art stay German — Stoppuhr, Atemschutz, Trupp, Funkrufname —
because that is what the app says on screen:

> Stoppuhr: ZURÜCKSETZEN while running loses the time

Text you are quoting is never translated: a string as it appears in the UI,
wording you are proposing for a screen, an error message, log output.

**Discussions are different — German and English are both fine there.** Nobody
should have to write English to report that something failed at the last Übung.
Someone will open the issue.

## Development setup

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet), pinned via
  [`global.json`](global.json) (10.x).
- [Docker](https://docs.docker.com/get-docker/), to build the Android app.
  `Microsoft.Android.Sdk` rejects any JDK newer than 21 (error `XA0030`), so
  the APK is built in the image from [`docker/`](docker/) rather than on the
  host.
- Optional: the Android SDK with an emulator (`Medium_Phone_API_35`), to run it.

Build and test everything except the Android head, on any machine:

```bash
make ci            # restore, build, test, format check
make test FILTER=FullyQualifiedName~StoppuhrViewModelTests
```

Build the APK and run it on the emulator:

```bash
make apk                     # all three ABIs, as CI and Play get it
make apk ABI=android-x64     # one ABI, a third of the size, for an emulator
make run-android             # boots the emulator, installs, launches
```

Most of the app is testable without Android at all: view models live in
projects with no Avalonia reference, and every screen renders in the headless
acceptance tests. `make phone-screenshots` writes the screens to
`out/phone-screenshots/`.

## Mutation testing

Stryker.NET mutates the code and re-runs the tests; a mutant nobody notices is
code the tests run without checking. A pull request is held to 80% on the lines
it changes:

```bash
make mutate PROJECT=Helpers.Stoppuhr SINCE=origin/main
```

`PROJECT` is one of `Core`, `AppLogic` or `Helpers.<Name>`. Without `SINCE` the
whole project is mutated against the floor in its `stryker-config.json`.

## Workflow

- **Feature branches only** — open a pull request against `main`; direct
  pushes to `main` are not used.
- Keep pull requests focused: one logical change per PR. A new helper is one
  pull request.
- CI must pass (build + tests) before a PR can be merged.
- Every commit must be **cryptographically signed** and carry a DCO
  `Signed-off-by` trailer — see [Commit conventions](#commit-conventions).

## Commit conventions

Every commit must meet three requirements:

1. a [Conventional Commits](https://www.conventionalcommits.org/) subject line,
2. a DCO `Signed-off-by` trailer — `git commit -s`,
3. a cryptographic signature, SSH or GPG — `git commit -S`.

```bash
git commit -s -S -m "feat(stoppuhr): keep the time when the phone sleeps"
```

Subject line starts with a type prefix — `feat:`, `fix:`, `docs:`, `style:`,
`refactor:`, `perf:`, `test:`, `build:`, `ci:`, or `chore:` — optionally
scoped, e.g. `fix(backgroundjob):`.

The sign-off and the signature are different things, and you need both. The
trailer is a statement about **rights**: it certifies you may submit the change
under the project's licence (see [Licence](#licence)). The signature is a
statement about **identity**: it proves the commit came from you. Neither
implies the other.

### Signing your commits

Set this up once and `git commit -s` is all you ever type again.

**SSH** — recommended, because you already have a key:

```bash
git config --global gpg.format ssh
git config --global user.signingkey ~/.ssh/id_ed25519.pub
git config --global commit.gpgsign true
```

Then upload that same public key to GitHub **a second time**, as a signing key:
*Settings → SSH and GPG keys → New SSH key →* **Key type: Signing Key**. This
is the step people miss. An authentication key does not verify signatures, so
without it your commits are signed perfectly well and still show up as
*Unverified* — and branch protection turns them away.

**GPG** works too if you prefer it or already have a key; follow GitHub's
[Telling Git about your signing key](https://docs.github.com/en/authentication/managing-commit-signature-verification/telling-git-about-your-signing-key).

Check locally before you push:

```bash
git log --show-signature -1
```

and confirm the commit carries a green **Verified** badge once the pull request
is open. That badge, not the local output, is what branch protection reads.

### Fixing a branch you already pushed

If you learn about this after the fact, or a commit slipped through unsigned,
rewrite the branch rather than stacking a commit on top:

```bash
git rebase --signoff --gpg-sign origin/main
git push --force-with-lease
```

That gives every commit on the branch both a fresh signature and a sign-off
trailer. Force-pushing your own pull-request branch is expected here and does
not lose review comments.

## Pull requests

A PR template with a short checklist will guide you:

- Conventional Commits, DCO signed off and cryptographically signed
- `make ci` green locally
- **UI changes**: before/after phone screenshots (`make phone-screenshots`)
- **Changes under `src/`**: a changelog entry as a new file in `changelog.d/`
  — CI checks for this (see [Changelog entries](#changelog-entries))
- No real personal or Einsatz data, in code, fixtures or screenshots

## Changelog entries

`CHANGELOG.md` is assembled, not edited. Each pull request brings its own file
in `changelog.d/`, so two pull requests never touch the same lines and can never
conflict over the changelog:

```
changelog.d/+<slug>.<type>.md
```

`<type>` is one of `added`, `changed`, `deprecated`, `removed`, `fixed` or
`security` — the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
sections. `<slug>` is a short kebab-case description. The leading `+` is
required: it is what keeps every filename unique, so two pull requests cannot
collide even when they describe the same ticket.

Put the entry in the file as plain prose, with no leading `- ` and **no link**:

```markdown
The Stoppuhr keeps running while another helper is open. (#12)
```

A release turns that into one bullet, with a link to the pull request the
fragment arrived in:

```markdown
- [#34](https://github.com/CodeForFire/helfer/pull/34) - The Stoppuhr keeps
  running while another helper is open. (#12)
```

You do not write that link and cannot: while your pull request is open it has no
number yet. `scripts/changelog-prlinks.py` recovers it from git when the release
is cut. An **issue** may still be cited inline, as `(#12)` is above — that is a
different thing from the pull request link, and it stays in the prose.

Two rules about the prose itself:

- **English.** `CHANGELOG.md` is one language throughout, the way the README's
  user half is German for Feuerwehr crews and `SECURITY.md` is English for
  security researchers. German terms of art stay German — Stoppuhr, Atemschutz,
  Übung — because that is what the app says on screen. CI warns when an
  entry looks German, but it is only a hint: it cannot tell a Fachbegriff from a
  sentence, so a reviewer decides.
- **One sentence.** The pull request holds the detail; the entry says what
  changed for the reader. If one sentence genuinely will not hold it, the change
  is probably two entries — which means two pull requests.

Do not edit `CHANGELOG.md` directly — entries land there when a release is cut.

**One entry per pull request**, not one per commit. The entry describes what the
change does for the reader, never the layers it was built in, so a feature split
across helper, shell and UI commits still gets a single file. If your PR
also carries an unrelated fix, fold it into the same entry — and if the two
really do not belong in one entry, they probably do not belong in one pull
request either.

### Cutting a release

A maintainer task, written up in [`docs/releasing.md`](docs/releasing.md).

## Reporting issues

Please use the issue templates. Security vulnerabilities do **not** belong in
public issues — see [`SECURITY.md`](SECURITY.md).

## AI-assisted contributions

Contributions made with coding agents are welcome, provided they follow the
same conventions above; [`AGENTS.md`](AGENTS.md) documents them in the form
agents consume.

## Licence

By contributing, you agree that your contributions will be licensed under the
[MIT licence](LICENSE). Signing off your commits (`git commit -s`) certifies
that you wrote the change or otherwise have the right to submit it under the
project's licence.
