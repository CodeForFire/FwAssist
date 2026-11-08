# Releasing (maintainers)

Pushing a version tag triggers the release workflow
([`.github/workflows/release.yml`](../.github/workflows/release.yml)), which
builds `helfer-<version>.apk` and publishes it as a GitHub release, with
`SHA256SUMS.txt`, a CycloneDX SBOM and Sigstore attestations. It also builds a
`.aab`, kept as a build artefact for inspection only: it is signed with the CI
debug key. The bundle Google Play gets is built on a laptop with the upload
key, see [Google Play](#google-play).

## Cutting a release

`CHANGELOG.md` is assembled from `changelog.d/`, never edited by hand, so a
release starts by folding the fragments into a version section:

```bash
pip install -r .github/requirements-changelog.txt
python scripts/changelog-prlinks.py --check   # rehearse; writes nothing
towncrier build --draft --version 0.6.0       # preview; writes nothing

python scripts/changelog-prlinks.py           # prepends each entry's PR link
towncrier build --version 0.6.0               # writes the section, consumes the fragments
```

Every bullet opens with a link to the pull request the entry arrived in. Nobody
writes that link by hand — the number does not exist while the pull request that
carries the fragment is still open — so
[`scripts/changelog-prlinks.py`](../scripts/changelog-prlinks.py) recovers it
first: after a squash-merge exactly one commit adds a given fragment, and its
subject ends in `(#NNN)`. The script rewrites each fragment in place, which is
safe because `towncrier build` consumes them in the next command, and it is
idempotent, so rehearsing with `--draft` and then building for real does not
prefix the link twice.

**If it cannot resolve a fragment it says so and exits non-zero — stop there.**
A fragment that is not committed yet, or one whose commit subject carries no
`(#NNN)`, has no link to find; commit it, or add the link by hand, rather than
cutting a release with one entry silently unlinked.

`title_format` in [`towncrier.toml`](../towncrier.toml) renders the heading as
`## [0.6.0] - <Datum>`, and the version in it has to match the tag exactly minus
the `v` — that is what the workflow looks for. A few things go with the build,
none of which towncrier does for you:

- **The summary paragraph** under the new heading, if the release deserves one.
  Towncrier writes the entries and nothing else.
  It is English like the rest of the file — the German summary a Kommandant
  reads is the GitHub release body, not this.
- **The link references at the bottom of `CHANGELOG.md`.** Repoint
  `[Unreleased]` at `compare/v<new>...HEAD` and add a `[<new>]:
  compare/v<previous>...v<new>` line above the existing ones (the first
  release links `releases/tag/v<new>` instead). A missing line renders the
  version heading as plain text instead of a diff link.
- **The screenshots.** Once everything for the release has merged, run
  `make screenshots play-screenshots` on the release branch and commit what
  changed. A pull request re-renders only the views it touches.
- **The milestone.** Close the milestone the release corresponds to, and move
  anything still open in it to the next one before closing — an issue that
  silently loses its milestone is an issue nobody plans again. Nothing in CI
  touches milestones.

Open that as its own pull request. Pushing the tag once it merges is what
triggers the release:

```bash
git tag -s v0.6.0 -m "FwAssist v0.6.0 — <Zusammenfassung>" && git push origin v0.6.0
```

The release body links to the version's section rather than inlining it, and a
final release whose version has no section is refused by the workflow's `version`
job, before any build minutes are spent — so a forgotten `towncrier build` costs
seconds rather than a release with no notes. Prereleases (`v0.6.0-beta.1`) are
exempt: they are cut from work in progress, before that version's section is
assembled.

## After the tag: check the release

- [ ] `SHA256SUMS.txt` is attached and lists the `.apk` and the SBOM under their
      bare names. `sha256sum -c SHA256SUMS.txt` has to pass in a folder holding
      nothing but the downloads.
- [ ] `gh attestation verify helfer-<version>.apk --repo CodeForFire/helfer`
      passes, and again with `--predicate-type https://cyclonedx.org/bom`.
- [ ] The release is marked pre-release if and only if the tag carries a suffix.
- [ ] The APK installs over the previous release on a phone, every helper's
      launcher icon opens its helper, and Back and relaunch work. The headless
      tests cannot see the Activity lifecycle.

## Betas and release candidates

A tag with a suffix — `v0.6.0-beta.1`, `v0.6.0-rc.2` — publishes as a GitHub
**pre-release**, so the previous full release stays the "Latest" download. The
suffix stays in the file name and the version shown on the home screen.

The Android `versionCode` is the hard part: Play accepts one integer, and
refuses any upload that does not raise it — to *any* track, permanently. So it
is derived in the `version` job, never typed:

```
MAJOR*10000000 + MINOR*100000 + PATCH*1000 + offset
offset:  alpha.N -> 100+N    beta.N -> 200+N    rc.N -> 300+N    final -> 999
```

| tag | `versionCode` |
|---|---|
| `v0.6.0-beta.1` | 600201 |
| `v0.6.0-rc.1` | 600301 |
| `v0.6.0` | 600999 |
| `v1.0.0` | 10000999 |

[`scripts/android-version-code.sh`](../scripts/android-version-code.sh) is the
source of truth, shared with Lagebuch; `--self-test` covers every row. A
prerelease sorts below its final release. An unrecognised suffix fails the
build: a scheme nobody can order is worse than a rejected tag.

The APK is **not code-signed** beyond Android's own APK signature; the README
explains the first install.

## Google Play

Google Play has not accepted an APK for a new app since 2021, so the upload is an
**Android App Bundle**. The build produces one: `AndroidPackageFormats` is
`aab;apk` in
[`Helfer.App.Android.csproj`](../src/Helfer.App.Android/Helfer.App.Android.csproj),
and the SDK then runs bundletool over that same signed bundle to extract a
universal `.apk`. The `.apk` that `make aab` leaves beside the bundle is
therefore a render of the exact bytes Play receives, and installing it on a
phone is the check on the bundle. The GitHub release's `.apk` is not that
check: CI builds it separately from the same tag, signed with another key.

**Submitting is a manual step at release time, and deliberately so.**
Automating it would mean keeping the upload keystore and its password in this
repository's secrets; a credential that can publish to every installed device is
not worth saving a few minutes a handful of times a year.

### One-time setup

The upload key is generated once, on a laptop, and never committed
(`.gitignore` carries `*.p12`, `*.jks`, `*.keystore`, `*.pass` as defence in
depth — the mechanism is that it lives outside the tree):

```bash
mkdir -p ~/.config/helfer && chmod 700 ~/.config/helfer
keytool -genkeypair -v \
  -keystore ~/.config/helfer/helfer-upload.p12 -storetype PKCS12 \
  -alias helfer-upload -keyalg RSA -keysize 4096 -validity 10000 \
  -dname "CN=CodeForFire, O=CodeForFire, C=DE"
printf '%s' '<password>' > ~/.config/helfer/helfer-upload.pass
chmod 600 ~/.config/helfer/helfer-upload.pass
```

`-validity 10000` is about 27 years, past Play's 2033 minimum. Use one password
for store and key so a single file serves both properties. **Back the keystore
up off the laptop, encrypted.**

Under Play App Signing, Google holds the key that actually signs what users
install; this one only authorises uploads. Losing it is recoverable through Play
support, which is precisely why Play App Signing is the right choice for a
project with one maintainer — but recovery is a support round-trip, not a
convenience.

Three things can never be changed after the first upload: the package name
(`de.codeforfire.helfer`), the app signing key, and the account type. **The
package name has to be final before that first upload** — while the app's name
is still a working title, so is this one.

### Per release

Build from the tag, not from `main`, so the bundle is the code that was released:

```bash
git checkout v0.6.1
make aab VERSION=0.6.1
```

The `versionCode` is **derived, not typed** —
[`scripts/android-version-code.sh`](../scripts/android-version-code.sh) computes
it from `VERSION`, and the release workflow calls the same script, so the two
cannot disagree. The target prints the number before it builds:

```
versionCode 601999   (0.6.1)
```

`CODE=` overrides it, and should stay unused. Play burns a `versionCode`
permanently on first sight — a number typed by hand is a mistake that cannot be
taken back, which is the whole reason the script exists.

The keystore path, password file and alias are overridable (`KEYSTORE=`,
`KEYSTORE_PASS=`, `KEY_ALIAS=`) but default to the locations above. The target
refuses to run without a keystore and a password file.

Then, in the Play Console: pick the track → *Create new release* → upload
`de.codeforfire.helfer-Signed.aab` → German release notes → roll out.

**Which track takes what**

*Production is full releases only*: a version that reaches the Play Store is
one that exists as a tag and a GitHub release.

A **prerelease may go to internal or closed testing**, and for this project it
has to be able to. A *personal* Play developer account cannot reach production
with a new app until a closed test has run **12 testers opted in for 14
continuous days** — once per app, which is one reason the helpers share one app, and
seeding that gate with a release candidate is exactly what a release candidate
is for — `v0.6.1-rc.1` gives `601301`, which stays below `0.6.1`'s `601999`, so
the real release still installs over it as an upgrade.

And note the ratchet once more: a build uploaded to *any* track burns its
`versionCode` forever, so even a throwaway test upload uses the derived number.

### The listing itself

Everything the Console asks for lives in `docs/play/`, so the listing is
reproducible instead of being retyped from memory each time:

| | Where | Regenerate |
|---|---|---|
| Text — name, short and full description, reviewer notes | [`docs/play/listing-de-DE.md`](play/listing-de-DE.md) | by hand; `make play-listing-check` |
| Icon, 512x512, 32-bit | `docs/play/icon-512.png` | yes |
| Feature graphic, 1024x500, no alpha | `docs/play/feature-graphic-1024x500.png` | not yet |
| Phone screenshots, 9:16 | `docs/play/screenshots/*.png` | `make play-screenshots` |

`make play-listing-check` counts characters against the Console's limits — it
truncates silently rather than warning — and greps the text for the wording in
*What the listing must not say* below. Run it before pasting.

### The declarations, frozen

These are answered once and must be answered the same way every time;
re-deciding them per release is how a Data-safety section drifts away from what
the app does.

- **Privacy policy**: still to be written, and Play requires one. It belongs on
  codeforfire.github.io next to Lagebuch's, and says what this app does: nothing
  leaves the device.
- **Data safety — no data collected, no data shared.** No account, no server, no
  network permission, no telemetry, no crash reporting, no ads or analytics
  SDKs. *Data deletion: everything is app-private and removed on uninstall.*
- **Content rating** (IARC): Utility; no violence, sexual content, language,
  substances, gambling or horror; no interaction between users; no location.
  Expected outcome USK 0 / PEGI 3.
- **Target audience**: 18 and over. Do not tick a younger bracket — it pulls the
  app into Families policy for no benefit.
- **Ads**: none. **Account deletion URL**: not applicable, there are no accounts.
  **News / health / financial / government app**: no to all.
- **Category**: Tools.

A helper that adds a permission, network access or stored personal data changes
these answers. Update this list in the same pull request.

### What the listing must not say

Play restricts apps that imply official or governmental status, and apps that
present themselves as emergency services. FwAssist is a volunteer project; it does
not alert, dispatch or replace anything.

- No coat of arms, Feuerwehr emblem or BOS insignia in the icon or graphics.
- Not *amtlich*, *offiziell*, *behördlich*, *Leitstelle*, *112* or *Notruf* in
  the name or descriptions, and no named Wehr.
- No safety claims. A timer is a timer; describe it as one, not as something
  that keeps anybody safe.

Keep the disclaimer in the description: FwAssist steht in keiner Verbindung zu
einer Behörde oder Leitstelle und ersetzt keine Ausbildung.

### After the upload

- The bundle's manifest carries the expected build number:
  `java -jar bundletool.jar dump manifest --bundle <file>.aab | head -1`
  (bundletool ships with the workload, under
  `~/.dotnet/packs/Microsoft.Android.Sdk.Linux/*/tools/`).
- The native libraries are still 16 KB-aligned, which Play requires of anything
  targeting Android 15+. The app ships 32- and 64-bit ABIs; check them all:
  ```bash
  unzip -q -o -d /tmp/helfer helfer-<version>.apk 'lib/*'
  for f in /tmp/helfer/lib/*/*.so; do
    readelf -lW "$f" | awk -v f="$f" '$1=="LOAD" && $NF!="0x4000" { print "NOT 16K:", f }'
  done
  ```
  If that ever prints something, the offender is a native library shipped by a
  NuGet package and the fix is a package bump — no MSBuild switch realigns
  someone else's `.so`.
- Play's pre-launch report has no crashes on the tested devices.
