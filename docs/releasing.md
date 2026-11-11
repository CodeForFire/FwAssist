# Releasing (maintainers)

Pushing a version tag triggers the release workflow
([`.github/workflows/release.yml`](../.github/workflows/release.yml)), which
builds `fwassist-<version>.apk` and publishes it as a GitHub release, with
`SHA256SUMS.txt`, a CycloneDX SBOM and Sigstore attestations. It signs the
`.apk` and the `.aab` with the Google Play upload key and hands the bundle to
Play as a draft, see [Google Play](#google-play).

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
- [ ] `gh attestation verify fwassist-<version>.apk --repo CodeForFire/FwAssist`
      passes, and again with `--predicate-type https://cyclonedx.org/bom`.
- [ ] The release is marked pre-release if and only if the tag carries a suffix.
- [ ] The APK is signed with the upload key, not a debug key:
      `apksigner verify --print-certs fwassist-<version>.apk` names
      `CN=CodeForFire`.
- [ ] The Play Console shows a draft on the expected track — closed testing for
      a prerelease, production for a final release.
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

The APK is **not code-signed** beyond Android's own APK signature, made with
the Play upload key; the README explains the first install.

## Google Play

Google Play has not accepted an APK for a new app since 2021, so the upload is an
**Android App Bundle**. The build produces one: `AndroidPackageFormats` is
`aab;apk` in
[`FwAssist.App.Android.csproj`](../src/FwAssist.App.Android/FwAssist.App.Android.csproj),
and the SDK then runs bundletool over that same signed bundle to extract the
universal `.apk` the GitHub release carries. The sideload APK is therefore a
render of the exact bytes Play receives, not a second, independently built
artefact.

**A tag uploads the bundle itself.** The release workflow signs it with the
upload key and its `play` job hands it to Play as a **draft**. Rolling it out
stays a person's decision, taken in the Console. The credentials are contained:

- The keystore and its password are secrets of the **`google-play`
  Environment**, not of the repository. A job sees them only after a maintainer
  approves that run, and only for `v*` tags. A tag push therefore stops twice
  for approval: once at `android`, which signs, and once at `play`, which
  uploads.
- Play is reached through **Workload Identity Federation**. The job swaps
  GitHub's OIDC token for a Google credential that lives for an hour. No
  service-account key exists anywhere, so none can leak.
- What CI can publish is a draft. Even with both credentials, a release does not
  reach a single device until someone rolls it out in the Console.

Because the sideload `.apk` is extracted from the same signed bundle, it carries
the upload key's certificate too. Up to v0.1.0 every CI build was signed with a
debug key the runner generated fresh, so each APK had a different certificate
and Android refused to install one over the other. From the first release
signed with the upload key on, a sideloaded install updates in place. The last
debug-signed APK has to be uninstalled once; the release body says so.

### One-time setup

The upload key is generated once, on a laptop, and never committed
(`.gitignore` carries `*.p12`, `*.jks`, `*.keystore`, `*.pass` as defence in
depth — the mechanism is that it lives outside the tree). Its path and alias
keep the `helfer` name, like the applicationId:

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

#### Play installs and sideloaded installs

Android installs an update only when it is signed with the same key as the
installed app. The sideload APK on the GitHub release carries the **upload
key**. What Play delivers carries the **app signing key**. Which one that is
gets decided once, at the first upload, and cannot be changed afterwards:

- **Google generates the app signing key** (the Console's default). The two
  keys differ, so a phone cannot move between the GitHub APK and the Play Store
  in place. Switching means uninstalling, which deletes the app's settings and
  the Schaummittel inputs. A leaked upload key cannot sign a Play update.
- **The upload key is also the app signing key** (*Use a different app signing
  key → Export and upload a key from Java keystore*, with Google's PEPK tool).
  Both channels carry the same certificate, so either one updates the other.
  But the key in the `google-play` Environment then signs what Play users
  install too, so a leak reaches Play users as well. Losing it is half
  recoverable: Play keeps its own copy, so an upload-key reset through Play
  support keeps Play updates going. The GitHub APK, however, cannot be signed
  compatibly again, and every sideloaded install has to be uninstalled once.

Choose before step 3 of *The first upload, by hand*. Then record the choice
here, and in the README's install section if users have to uninstall to switch.

Three things can never be changed after the first upload: the package name
(`de.codeforfire.helfer`), the app signing key, and the account type. **Confirm
the package name is final before that first upload.** It stays `helfer` although
the app is now FwAssist, because it already identifies every sideloaded install
(see `AGENTS.md`, *Helpers*).

#### Handing the key and Play access to CI

Do this once, before the first tag that should reach Play. FwAssist shares
Lagebuch's Google Cloud project and its `github` workload identity pool, but has
its own provider and its own service account, so neither app's credential can
publish the other.

1. **Environment.** Under *Settings → Environments*, create `google-play`. Add
   yourself as a *required reviewer*. Under *Deployment branches and tags*, allow
   only the tag pattern `v*`.
2. **Keystore secrets** in that Environment, not in the repository:
   ```bash
   base64 -w0 ~/.config/helfer/helfer-upload.p12 \
     | gh secret set ANDROID_UPLOAD_KEYSTORE_B64 --env google-play --repo CodeForFire/FwAssist
   gh secret set ANDROID_UPLOAD_KEYSTORE_PASS --env google-play --repo CodeForFire/FwAssist \
     < ~/.config/helfer/helfer-upload.pass
   ```
3. **Workload Identity Federation**, in the project that already has the *Google
   Play Android Developer API* enabled for Lagebuch:
   ```bash
   gcloud iam workload-identity-pools providers create-oidc fwassist \
     --location=global --workload-identity-pool=github \
     --issuer-uri=https://token.actions.githubusercontent.com \
     --attribute-mapping=google.subject=assertion.sub,attribute.repository=assertion.repository,attribute.environment=assertion.environment \
     --attribute-condition="assertion.repository == 'CodeForFire/FwAssist' && assertion.environment == 'google-play'"
   gcloud iam service-accounts create fwassist-play
   gcloud iam service-accounts add-iam-policy-binding \
     fwassist-play@<project>.iam.gserviceaccount.com \
     --role=roles/iam.workloadIdentityUser \
     --member="principalSet://iam.googleapis.com/projects/<number>/locations/global/workloadIdentityPools/github/attribute.repository/CodeForFire/FwAssist"
   ```
   The attribute condition is the lock. Only a job of this repository that runs
   in the approved `google-play` Environment gets a token at all.
4. **Play Console** → *Users and permissions* → invite
   `fwassist-play@<project>.iam.gserviceaccount.com`. Give it access to FwAssist
   only, with *Release apps to testing tracks* and *Release to production*.
5. **Environment variables** (*variables*, not secrets, because both are names):
   ```bash
   gh variable set GCP_WIF_PROVIDER --env google-play --repo CodeForFire/FwAssist \
     --body projects/<number>/locations/global/workloadIdentityPools/github/providers/fwassist
   gh variable set GCP_PLAY_SERVICE_ACCOUNT --env google-play --repo CodeForFire/FwAssist \
     --body fwassist-play@<project>.iam.gserviceaccount.com
   ```

#### The first upload, by hand

The Play Developer API only updates an app that already has a bundle; the very
first one goes through the Console.

1. Create the app in the Play Console (default language German, app, free).
2. Fill in the listing and the declarations below — the privacy policy has to be
   live first.
3. Build the first release candidate locally, from its tag, and upload it to
   **internal testing**. When Play App Signing asks for the key, answer as
   decided in *Play installs and sideloaded installs* above:
   ```bash
   git checkout v0.1.1-rc.1
   make aab VERSION=0.1.1-rc.1
   ```
4. From then on, CI uploads. Do not tag that same version again — Play has
   burned its `versionCode`.

### Per release

Push the tag as usual. Approve the `google-play` deployment twice: first for
`android`, then for `play` once the GitHub release exists. A prerelease tag lands
as a draft on closed testing (the API calls that track `alpha`). A final tag
lands as a draft on production. In the Play Console, open the draft, add the
German release notes and roll it out.

Until the app has production access (see *Which track takes what*), a final
tag's `play` job fails: the API refuses production for that app. That is
expected. The GitHub release is already published at that point. Do **not**
promote a closed-testing release instead: that publishes the release
candidate's bundle under its own `versionCode`, not the bundle built from the
final tag. Once production access is granted, re-run the failed `play` job
(*Actions → the run → Re-run failed jobs*). The job uploads the very bundle the
tag built. GitHub allows a re-run for 30 days; after that, build the tag with
`make aab` and upload it by hand (*By hand, when CI cannot*).

The `versionCode` is **derived, not typed** —
[`scripts/android-version-code.sh`](../scripts/android-version-code.sh) computes
it in the release workflow's `version` job, and `make aab` calls the same script,
so the two cannot disagree.

#### By hand, when CI cannot

`make aab` builds the same bundle on a laptop. Build it from the tag, not from
`main`, so the bundle is the code that was released:

```bash
git checkout v0.6.1
make aab VERSION=0.6.1
```

The target prints the `versionCode` before it builds:

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
`de.codeforfire.helfer-Signed.aab` → German release notes → roll out. Never
upload by hand a version the workflow already uploaded: Play refuses the
`versionCode` a second time.

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

- **Privacy policy**: <https://codeforfire.github.io/fwassist/datenschutz/>,
  next to Lagebuch's. It says what this app does: nothing leaves the device.
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
  unzip -q -o -d /tmp/fwassist fwassist-<version>.apk 'lib/*'
  for f in /tmp/fwassist/lib/*/*.so; do
    readelf -lW "$f" | awk -v f="$f" '$1=="LOAD" && $NF!="0x4000" { print "NOT 16K:", f }'
  done
  ```
  If that ever prints something, the offender is a native library shipped by a
  NuGet package and the fix is a package bump — no MSBuild switch realigns
  someone else's `.so`.
- Play's pre-launch report has no crashes on the tested devices.
