# Agent instructions

## Language

Issues, pull requests and commit messages are written in **English** — title and
body. This is the collaboration surface, and keeping it English is what lets
someone who does not speak German read the backlog and work here.

German terms of art stay German: Stoppuhr, Atemschutz, Einsatz, Übung, Trupp,
Funkrufname. They are what the app says on screen and what a Kommandant says out
loud, so translating them makes the text describe a product that does not exist.
`Stoppuhr: ZURÜCKSETZEN while running loses the time` is exactly right. Quoted UI strings, wording proposed for a
screen, and log output are reproduced verbatim — never translated.

A pull request title becomes the squash-merge commit subject, so this is one
rule in both places; see [Git commits](#git-commits).

What is English is the workbench, not the product:

| English | German |
|---|---|
| issues, pull requests, commit messages | the UI and every string in it |
| `CHANGELOG.md`, the code and its comments | the German half of `README.md` |
| `CONTRIBUTING.md`, `AGENTS.md`, `SECURITY.md` | the GitHub release body |
| the bug report and feature request forms | the issue chooser, `docs/play/listing-de-DE.md` |

**Discussions are deliberately exempt — German and English are both fine there.**
A Kommandant arriving from the README or the issue chooser is routed to
Discussions in German, which is what
[`.github/ISSUE_TEMPLATE/config.yml`](.github/ISSUE_TEMPLATE/config.yml) is for.
Feedback from an Übung is worth more than the language it arrives in. Turning a
German discussion into an English issue is a maintainer's job, not a hurdle put
in front of the person who found the bug.

## Pull requests

Every PR that touches the UI must include screenshots. The flow is:

1. Render the affected view(s) to PNG at phone size using the headless Skia
   harness in `tests/Helfer.Acceptance.Tests` (`PhoneRender` +
   `Window.CaptureRenderedFrame()` — `UseHeadlessDrawing = false` so embedded
   fonts rasterize). `make phone-screenshots` renders `DemoFlowRenderTests`
   into `out/phone-screenshots/`.
2. Attach the PNGs directly with `gh`'s `--attach` flag — no manual pasting
   needed. It uploads to `github.com/user-attachments/assets` (the same
   pipeline the web UI's drag-and-drop uses) and rewrites any
   `![alt](./path.png)` reference in the body to point at the uploaded asset:
   - New PR: `gh pr create --attach './before.png#Before' --attach './after.png#After'`
   - Existing PR body: `gh pr edit <number> --attach './screenshot.png#Alt text'`
   - PR comment: `gh pr comment <number> --attach './screenshot.png#Alt text'`

   If `gh` refuses the upload, drag the PNGs into the pull request's
   description in the web UI instead; it is the same pipeline.
3. Link each image to its own asset URL. GitHub renders a bare
   `![alt](https://github.com/user-attachments/assets/<id>)` with a
   click-through link to `private-user-images.githubusercontent.com` whose
   token expires five minutes after the page loads, so opening a screenshot
   full size later shows a 404. Wrapped as `[![alt](U)](U)`, the link stays on
   `github.com/user-attachments/assets/<id>`, which signs a fresh URL on
   every click:

   ```bash
   gh pr view <number> --json body --jq .body \
     | perl -0pe 's{(?<!\[)!\[([^\]]*)\]\((https://github\.com/user-attachments/assets/[0-9a-f-]+)\)}{[![$1]($2)]($2)}g' \
     > body.md && gh pr edit <number> --body-file body.md
   ```

Copilot reviews every push (ruleset "Copilot review for default branch").
`.github/workflows/copilot-approve.yml` approves as `github-actions[bot]` when
that review has no inline comments, and withdraws the approval when a later one
does. Treat Copilot's comments like any reviewer's: fix them or answer why not.

For UI changes, provide a before/after pair. The render harness files are
diagnostic-only and should not be committed unless they double as a real test.

The README's screenshots come from one such test, `DemoFlowRenderTests`:
regenerate them with `make screenshots` whenever a view they show changes, and
the Play listing's with `make play-screenshots`. A new helper adds its page to
that test.

## Helpers

The app is one Play listing with one helper per job. Each helper is its own
module, so it can be split into a standalone app later by packaging alone, and
each can have its own launcher icon on the phone's home screen.

**Adding a helper** touches exactly these places:

1. `src/Helpers/<Name>/Helfer.Helpers.<Name>/`: the rules and view models,
   plus a `sealed class <Name>Module : IHelperModule`. Its `Id` is lower-case
   ASCII and permanent. It names the launcher alias, and renaming it removes
   the icon from every home screen it was placed on.
2. `src/Helpers/<Name>/Helfer.Helpers.<Name>.Ui/`: the views, and a
   `<Name>Views.Register(ViewRegistry)`.
3. `CompositionRoot.Modules` and `CompositionRoot.RegisterViews`, and the
   project reference in `Helfer.Composition.csproj`.
4. An `<activity-alias>` named `de.codeforfire.helfer.launcher.<id>` in
   `src/Helfer.App.Android/AndroidManifest.xml`, with its icon as
   `Resources/drawable/ic_helper_<id>.xml`, `ic_helper_<id>_foreground.xml` and
   `drawable-anydpi-v26/ic_helper_<id>.xml` (copy the Stoppuhr's three files).
5. `tests/Helfer.Helpers.<Name>.Tests/` with a `stryker-config.json`, and the
   project in `MUTATE_PROJECTS` (Makefile) and in both lists in
   `.github/workflows/mutation.yml`.
6. The helper's page in `DemoFlowRenderTests`, and its line in the README and
   in `docs/play/listing-de-DE.md`.

`HelperRegistrationTests` fails when the module list, the views and the
manifest disagree. `AutomationPropertiesTests` walks every helper's page.

A helper's view model is created once per process and kept while the user
visits other helpers, so a running timer keeps running. A helper whose state
must survive the process being killed persists it itself.

## Mobile

There is no desktop build. Every screen is designed for a phone in portrait
first and checked on a tablet second (`make emulator-tablet`).

- **Gloves and one hand.** Every control is at least 48 dp (`ControlHeight` in
  `Theme/Tokens.axaml`). The primary action sits at the bottom, in thumb reach.
- **Daylight.** Use the token colours, and never carry meaning by colour alone.
- **No hover, no right click, no keyboard shortcuts.** Nothing may depend on
  them. A long press is not discoverable either; use a visible button.
- **The soft keyboard** shrinks the window (`adjustResize`). A form scrolls, so
  the field being typed into stays visible.
- **Offline.** A helper works without a network unless that is its purpose,
  and then it says so on screen.
- **Back.** The arrow in the title bar goes to the overview. The system Back
  gesture returns to the overview from a helper opened there, and leaves the
  app from a helper opened by its own icon (`ShellViewModel.HandleSystemBack`).
  The app's own icon always lands on the overview.
- **Process death.** Android kills a background app without warning. State the
  user would miss is persisted, never only held in a view model.
- **Time** comes from the clock, never from counting ticks: a phone that sleeps
  through a minute must show the right time when it wakes (`StopwatchState`).
- **Check it on a device.** The headless tests cannot see Activity lifecycle
  problems. Before a release, run `make apk ABI=android-x64`, install it on the
  phone emulator and go through Back, relaunch and the launcher icons by hand.

## Git commits

All commits must be:
- Semantic / Conventional Commits: subject starts with `feat:`, `fix:`,
  `docs:`, `style:`, `refactor:`, `perf:`, `test:`, `build:`, `ci:`, `chore:`
  (optionally scoped, e.g. `fix(backgroundjob):`)
- DCO signed-off: always pass `-s` to `git commit`
- Cryptographically signed, SSH or GPG: always pass `-S`, or set
  `commit.gpgsign = true`
- Written in English, subject and body — a squash-merge makes the pull request
  title the commit subject, so it is one rule; see [Language](#language)

The last two are project requirements, not local conventions — `main`'s branch
protection rejects a pull request carrying an unsigned commit, and the DCO
check fails one missing a `Signed-off-by` trailer. Neither can be merged past.
[`CONTRIBUTING.md`](CONTRIBUTING.md#signing-your-commits) has the setup and the
recovery recipe for a branch already pushed.

Signing in the sandbox: SSH signing (`gpg.format = ssh`) needs the SSH key,
which the command sandbox blocks — inside the sandbox `git commit` silently
produces an unsigned commit despite `commit.gpgsign = true`, and you find out
only when the merge is blocked. Run signing commits with the sandbox disabled
and verify before pushing with:

```bash
git cat-file commit HEAD | grep gpgsig
```

That check covers SSH signatures too: git stores them in the same `gpgsig`
header, so the absence of that header means the commit is unsigned whichever
format you use.

## Git push

NEVER push directly to `master` or `main`. Always use a feature branch and
open a PR. Absolute rule, no exceptions.

## GitHub Actions workflows

- Pin third-party actions to a full 40-char commit SHA, never a movable tag
  (`uses: owner/action@<sha> # vX.Y.Z`). First-party internal reusable
  workflows may follow repo convention.
- Always pin to the latest release: resolve with
  `gh api repos/<owner>/<action>/releases/latest --jq .tag_name`, then
  `gh api repos/<owner>/<action>/commits/<tag> --jq .sha`. That resolves an
  annotated tag to its commit; `git/refs/tags/<tag>` would hand back the tag
  object's SHA instead.
- Every repo needs `.github/dependabot.yml` covering the `github-actions`
  ecosystem; add it if missing.
- `zizmor.yml` audits every workflow and composite action, and its findings
  arrive as code scanning alerts. Values from `needs`, `inputs` or
  `github.event` reach a `run:` through `env:`, never as `${{ }}` inside the
  script. A checkout sets `persist-credentials: false` unless the job pushes.
  A finding accepted on purpose gets an inline `# zizmor: ignore[<audit>]`
  with the reason in a comment beside it. Run it locally with `zizmor .`.

## Avalonia docs MCP

`.mcp.json` configures Avalonia's free, hosted
[Build MCP](https://docs.avaloniaui.net/tools/ai-tools/build-mcp) as
`avalonia-docs`. Claude Code asks once to approve it, as it does for every
project-scoped server. Use it — `lookup_avalonia_api`,
`search_avalonia_docs`, `get_avalonia_expert_rules` — before writing XAML or
reaching for a control API: this app is on Avalonia 12, much of what a model
remembers is Avalonia 11, and a deprecated member only fails a full rebuild
(see [Static analysis](#static-analysis)).

Avalonia's DevTools and Parcel MCP servers are deliberately not configured:
they need a paid Avalonia Plus licence, which a contributor cannot be expected
to have. For looking at a rendered view, the headless harness under
[Pull requests](#pull-requests) does the job.

## Architecture

These sections hold the standards you would otherwise have to ask for with an
"act as a senior .NET architect / security engineer" prompt. They are the
defaults for every change. Where the codebase already solves a problem one way,
use that way. Consistency beats a locally cleverer idea.

Dependencies point one way, and nothing may invert them:

- `Core` references nothing. It is Lagebuch's `Domain`: values and rules any
  helper may share, such as `Formatting`.
- `AppLogic` holds the shell's view models and the service seams (`ITicker`,
  `ILauncherIcons`, `IHelperModule`) and has **no Avalonia reference**, which
  is what makes it testable without a UI.
- `Ui` holds the Avalonia `App`, the theme and the shell's views. It is a plain
  `net10.0` library so the headless tests can load it.
- A helper is two projects under `src/Helpers/<Name>/`: `Helfer.Helpers.<Name>`
  (its rules and view models; references `Core` and `AppLogic` only) and
  `Helfer.Helpers.<Name>.Ui` (its views; references the first and `Ui`).
  **A helper never references another helper.** What two helpers share moves
  down into `Core`, `AppLogic` or `Ui`.
- `Composition` is the one project that knows every helper.
- `App.Android` is a thin host.

A project reference that breaks this order needs a reason in the PR.

- **View models** live in `src/Helfer.AppLogic/ViewModels/`, or in the
  helper's own project.
  - Write them as `sealed partial` classes over CommunityToolkit.Mvvm's
    `ObservableObject`, using `[ObservableProperty]` and `[RelayCommand]`.
  - They never touch `Dispatcher.UIThread`. Drive timers through `ITicker`. A
    helper that has to marshal from a background thread copies Lagebuch's
    `IUiDispatcher` seam into `AppLogic` rather than inventing another.
- **No DI container.** Wiring is by hand, through constructor injection, in
  `src/Helfer.Composition/CompositionRoot.cs` and `MainActivity.cs`. A helper
  gets its platform services through `HelperServices`.
  
  An optional dependency is a nullable constructor parameter with a default,
  e.g. `uiDispatcher ?? new ImmediateUiDispatcher()`. Do not introduce
  `ServiceCollection`.
- **Time is injected** as a `TimeProvider`. Never read `DateTime.Now` or
  `DateTimeOffset.Now` directly; `MainActivity` passes `TimeProvider.System`.
- **Types:**
  - Non-view classes are `sealed`.
  - Domain values are `sealed record`s.
  - Persisted state comes back through a `Rehydrate` factory, not public
    setters.
- **Smallest change that fits.**
  - Extend the existing pattern before inventing an abstraction.
  - Add an interface only when there is a second implementation, and a test
    double counts as one.
  - No speculative generality, and no drive-by refactors in a feature PR.

## C# and .NET

The analyzers already enforce formatting and naming. The rules below are about
what they cannot see.

- **Nullability is real.**
  - Never add `#nullable disable`.
  - Never use `!` to silence a warning. Fix the flow, or annotate it with
    `[MaybeNullWhen]` or `[NotNullWhen]`.
  - Public entry points start with `ArgumentNullException.ThrowIfNull`.
- **Failure has three shapes here.** Pick one of them; do not bring in a
  `Result<T>` library.
  - `bool TryX(..., out T)` with `[MaybeNullWhen(false)]`.
  - A nullable return whose `null` is documented.
  - A typed exception, such as `CertificateChangedException`.
- **Failures surface, they are not logged.** There is no logging framework.
  - A failure becomes a bound German status or error property on the view
    model, as `SettingsViewModel.Error` does.
  - A `catch` that ends in neither that nor a rethrow is a bug.
- **Async:**
  - No `async void` outside event handlers.
  - No `.Result` or `.Wait()`. The one audited exception is the Exit hook in
    `Program.cs`.
  - Write deliberate fire-and-forget as `_ = Task`, and only where the failure
    surfaces somewhere else.
  - No `ConfigureAwait(false)`: CA2007 is off because continuations must stay
    on the UI context.
  - New I/O or network APIs take `CancellationToken cancellationToken =
    default` and pass it on.
- **Culture:**
  - Anything the user reads is formatted through the fixed `de-DE` culture in
    `src/Helfer.Core/Formatting.cs`.
  - Anything persisted or sent over the wire uses `InvariantCulture`.
  - Keys and identifiers compare with `StringComparison.Ordinal`.
- **Dispose deterministically.** Use `using` or `await using` for every
  `IDisposable` you own — never a manual `.Dispose()` in a `finally`; see
  *Static analysis* below.
- **Packages:**
  - Versions live in `Directory.Packages.props` (central package management).
    Never put a `Version` on a `PackageReference` in a `.csproj`.
  - Only `Helfer.Acceptance.Tests` keeps its own nested props file (the
    xunit.v3 stack). The Android head's packages live in the root file on
    purpose, so Dependabot's Avalonia group moves `Avalonia.Android` with
    `Avalonia`: Lagebuch's separate file left it on 12.1.1, which hangs.
  - Every project except the Android head commits a `packages.lock.json`, and
    CI restores with `--locked-mode`. A package change commits every lock file
    `dotnet restore --force-evaluate` rewrites; CI fails with `NU1004`
    otherwise.
  - A Dependabot NuGet bump needs that by hand. Dependabot rewrites only the
    lock file of the project that references the package directly, while
    every project that gets it through a project reference pins it as a
    `CentralTransitive` entry too (lagebuch #642). Run `dotnet restore Helfer.sln
    --force-evaluate` on its branch and push the result as a signed,
    signed-off commit.
  - A new dependency must earn its place in the PR description: it is
    maintained, it is licence-compatible, and its transitive tree is small.

## Avalonia UI

Use the `avalonia-docs` MCP described above before reaching for an API from
memory. The rules below are the ones this app has learned the hard way.

- **Compiled bindings are on by default.**
  - Every `.axaml` declares `x:DataType`.
  - `x:CompileBindings="False"` needs a comment saying why.
- **Code-behind is view-only.** It may handle focus and forward events to
  commands. No state and no decisions live in `.axaml.cs`; anything a test
  should cover belongs in the view model.
- **UI text is German and inline.** There are no `.resx` files.
  - Keep wording short and imperative. It is read on a phone, outdoors, often
    with gloves on.
  - Button labels are upper case, as in Lagebuch: `START`, `ZURÜCKSETZEN`.
- **Accessibility:** every icon-only or otherwise text-less control gets an
  `AutomationProperties.Name`. `AutomationPropertiesTests` holds the line.
- **Layout traps that have shipped before:**
  - **Content-sized columns over virtualized lists.** A column with
    `MaxWidth` and `HorizontalAlignment="Center"` that contains a
    virtualizing list changes width while the user scrolls. Use `Stretch`
    instead.
  - **`Transparent` gradient stops.** A stop at `Transparent` fades through
    grey. Fade to a zero-alpha stop of the same hue, as `SignalFadedColor`
    in `Theme/Tokens.axaml` does.
  - **Ragged `Auto` columns.** Row `Grid`s inside an `ItemsControl` do not
    share `Auto` widths. Use `Grid.IsSharedSizeScope` with a
    `SharedSizeGroup`.
- **Platform APIs stay in the head.** Anything Android-specific goes behind a
  service interface in `AppLogic`, implemented in `App.Android` and by a test
  double, the way `ILauncherIcons` is. Views never call Android.

## Security

**Threat model.** Today nothing crosses a trust boundary: the app declares no
permission, reads no file and receives no intent beyond its own launcher
icons. That changes the moment a helper imports a file, shares data or talks to
the network, and a helper that does so brings the rules below with it. Treat
any such input as hostile. Data a helper records about people at an Einsatz is
personal data, so confidentiality is a requirement, not a nice-to-have.

- **Permissions.** Every `<uses-permission>` in the manifest needs a reason in
  the PR and a line in the Play listing's data-safety answers. "Keine
  Berechtigungen" is part of what the listing promises today.
- **SQL.**
  - Values are always bound as `$name` parameters.
  - An identifier may be interpolated only from a compile-time schema
    constant, and the site carries a `CA2100` justification that says so.
  - Nothing that came from input ever reaches the SQL text.
- **Paths.** Beyond the `Path.Join` and sanitiser rules under
  [Static analysis](#static-analysis), a path built from outside input must be
  resolved with `Path.GetFullPath` and checked to start with the root plus a
  separator. `AttachmentTempPaths` shows the pattern.
- **Size.** Every input that crosses the trust boundary has a cap, enforced on
  both ends; Lagebuch checks an attachment against `IncidentFile.MaxSizeBytes`
  in both the view model and the host.
- **Deserialization.**
  - Use System.Text.Json only, with one shared `JsonSerializerOptions` per
    format.
  - Polymorphism goes through a closed `[JsonDerivedType]` allowlist, as on
    Lagebuch's `SyncCommand`.
  - Never use `BinaryFormatter`, Newtonsoft `TypeNameHandling`, or
    `Type.GetType` on input.
- **Network.** None today. A helper that adds it uses TLS only, never accepts
  any certificate (not even in a debug build), and takes anything secret from
  `RandomNumberGenerator`, never `Random`. Lagebuch's sync is the reference.
- **Launching things.**
  - A URL is validated before it is opened, and only its schemes are
    allowlisted (copy Lagebuch's `HttpUrlValidator` when the first link
    arrives).
  - A process started with `UseShellExecute = false` quotes its argument.
  - Nothing from input is ever concatenated into a command line.
- **No hand-rolled crypto.** Use BCL primitives only. Compare secret material
  with `CryptographicOperations.FixedTimeEquals`.
- **Personal data stays out of diagnostics.**
  - No names, phone numbers or Einsatz content in trace output.
  - Test fixtures and screenshots use fictional data.
  - Real Einsatz data is never committed.

## Tests

- **Tooling:**
  - xUnit with plain `Assert`, and handwritten test doubles. There is no
    mocking library and no FluentAssertions; do not add either.
  - Headless UI tests use `[AvaloniaFact]` in `Helfer.Acceptance.Tests`.
- **Naming:** a test's name is a sentence in snake_case stating the behaviour,
  e.g. `Join_prompt_focuses_the_host_field_not_the_name_field`.
- **Bug fixes:** write the failing test first.
  - A security fix gets a test that replays the attack and shows it fail: the
    traversal name, the oversized body, the wrong PIN.
- **Time and waiting:** control time with `FakeTimeProvider` or a fixed
  `IClock`. A real `Task.Delay` or sleep in a test is a flake waiting to
  happen.

## Before you call it done

Before saying a task is finished, and again before opening a pull request,
review your whole diff through each of the following lenses. This review is the
standing replacement for the persona prompts.

**Architect**
- Does every project reference still point the way
  [Architecture](#architecture) says?
- Did I reuse the existing pattern, or invent a parallel one?
- Is every new type, interface and parameter needed by this change, not by an
  imagined future one?
- Is the diff as small as the change allows?

**Security**
- Does new input cross a trust boundary? If so, is it validated, size-capped,
  parameterized and path-contained?
- Can a failure surface to the user without leaking personal data or crashing
  the app mid-Einsatz?
- Did I add a permission, or weaken a sanitiser, even temporarily?

**UI** (when any `.axaml` changed)
- Does the view declare `x:DataType`, and do text-less controls carry
  automation names?
- Is the wording German, short, and readable with gloves on in daylight?
- Does it work at 412 dp wide, and on a tablet?
- Is there a before/after screenshot pair ready for the PR?

**Evidence**
- Run a clean build with `make clean ci`. An incremental build hides XAML
  deprecations. `make ci` covers everything but the Android head; build that
  with `make apk` when the head, the manifest or a resource changed.
- Run `make codeql` when the diff touches `.cs` code. A green `make clean ci`
  says nothing about CodeQL; see [Static analysis](#static-analysis).
- When a test fails, search the log for `Test Run Failed`. The tail of the log
  says `0 Error(s)` even when a test assembly failed.
- "Done" means you saw the output, not that you expect it to pass.

## Static analysis

Two tools police this code and they overlap: the build runs the .NET analyzers
at `AnalysisMode=All` plus StyleCop with `TreatWarningsAsErrors`, and CodeQL
runs the `security-and-quality` suite on every pull request. The build sees
`[SuppressMessage]` attributes and `.editorconfig` severities; CodeQL does not.
A decision recorded only in code therefore comes back as a CodeQL alert unless
it is recorded in `.github/codeql/codeql-config.yml` too.

Write code that trips neither:

- **Join paths with `Path.Join`, never `Path.Combine`.** `Path.Combine` returns
  its later argument alone once that argument turns out to be rooted, silently
  discarding the directory you meant to stay inside; `Path.Join` always
  concatenates. This is defence in depth, not a substitute for sanitising: a
  name that crosses the trust boundary — a content provider's `DISPLAY_NAME`,
  anything read back out of an imported file — still goes through a sanitiser
  first (Lagebuch's `SafeFileName.Sanitize` is the one to copy).
- **Give every `catch (Exception)` a per-member `[SuppressMessage("Design",
  "CA1031", Justification = "...")]`** that says where the failure surfaces —
  a bound error string, a documented `null` contract, an event. The build
  rejects the catch without one; the justification is also what makes CodeQL's
  duplicate finding defensible instead of an open alert nobody can explain.
- **Never dispose a local by hand — put it in `using`, even when it only
  exists on some paths** (`cs/missed-using-statement`). CA2000 is satisfied by
  a `finally { x?.Dispose(); }`, so the build never objects; CodeQL does, and
  this shape has been flagged on review again and again:

  ```csharp
  FileStream? handle = null;
  try { if (OperatingSystem.IsWindows()) handle = new FileStream(…); … }
  finally { handle?.Dispose(); }
  ```

  `using` accepts `null`, so a conditional resource is still one line:
  `using var handle = OperatingSystem.IsWindows() ? new FileStream(…) : null;`.
  When the resource has to be released *before* other cleanup in a `finally`
  runs — unlock a file, then delete its directory — a `using var` declaration
  is wrong, because it disposes at the end of the method, after the `finally`.
  Use a `using (…) { … }` block inside the `try`, so its scope ends first.
  If the block never reads the object — it only has to exist, as a file lock
  does — give `using` the bare expression, `using (cond ? new X(…) : null)`;
  naming a variable nobody reads trips `cs/useless-assignment-to-local`.
  The one exception is a method that has to hand a disposable to its caller:
  CA2000 and this query pull in opposite directions there, so return plain
  data and build the object where it is used (see `HostIdentity`, #503).
- **Leave no dead locals.** `IDE0059` is a warning here and warnings are
  errors. Use `_` for a value you do not need, including inside a tuple
  deconstruction or an `out var` call.
- **Never compare `double` or `float` with `==` or `!=`** (`cs/equality-on-floats`).
  This bites hardest in UI tests, where it is tempting to find a control by a
  layout value — `OfType<StackPanel>().First(p => p.MaxWidth == 720)`. Find
  controls by `x:Name` instead, adding the name to the `.axaml` if it has none;
  that is what `OpenErrorBanner` and `RecentFilesHint` are for. Where a number
  really is the subject, assert on it with a tolerance — xUnit's
  `Assert.Equal(expected, actual, precision: 0)`, or `Math.Abs(a - b) <= 1.0` —
  and never with `Assert.Equal(0, someDouble)`, which is an exact comparison
  wearing a method call.
- **Never type-test a value against a type it already is** (`cs/useless-type-test`).
  Neither the compiler nor the analyzers warn, so `make clean ci` passes and
  the pull request's code scanning blocks it instead. Avalonia makes this easy
  to miss, because the hierarchy is deep: every `Visual` is a `StyledElement`
  and therefore an `ILogical`, so `visual is ILogical logical` is always true
  (#555). Pass the value directly — `root.IsLogicalAncestorOf(visual)`. The
  quick proof is to delete the test and build: if it still compiles, the test
  was useless.
- **Never call an API the toolchain marks obsolete.** When the warning names
  the replacement, use it — `TextBox.Watermark` → `PlaceholderText`. On the C#
  side `TreatWarningsAsErrors` already fails the build (CS0612/0618/0619), but
  Avalonia's XAML compiler logs through MSBuild and slipped past it, so
  `Directory.Build.props` escalates `AVLN5001` separately. Note that a XAML
  deprecation only surfaces on a **full** rebuild — an incremental build skips
  the XAML compile, which is how a deprecated `Watermark` once shipped
  unnoticed. If a deprecation ever genuinely has to stand, suppress that one
  occurrence with a written justification; never widen `NoWarn`.
  A `PART_` name in a control template is not an obsolete member —
  `TextBox /template/ TextBlock#PART_Watermark` stays exactly as it is.

These queries came back on Lagebuch pull request after pull request in
September and October 2026: 69 review comments across 22 PRs, every one of
which had passed `make clean ci`. Where a Roslyn rule covers part of a query, `.editorconfig`
now raises it into the build, so that part fails locally. The rest is shape to
avoid:

| Query | Shape that trips it | Write instead | Build catches |
|---|---|---|---|
| `cs/useless-upcast`, `cs/useless-cast-to-self` | `(IFoo)foo`, or `(Guid?)null` inside a tuple, to steer type inference | Give the type its own place: `IFoo key = foo;`, `Assert.Equal<(Kind, Guid?)>((Kind.A, null), …)`, `Select<TIn, TOut>(…)` | `IDE0004`, the plain casts only |
| `cs/loss-of-precision` | `14 * depth` or `tile * 256` handed to a `float`/`double`/`long` | Widen an operand first: `14f * depth`, `tile * 256.0`, `(long)a * b` | — |
| `cs/constant-condition` | A `switch` arm or `?.` that the preceding code already decided, e.g. `null =>` after a non-null pattern | Drop the arm or the `?`; cover the remaining cases with `_ =>` | `CA1508`, in part |
| `cs/reference-equality-with-object` | `b.DataContext == row`: both sides typed `object` | `ReferenceEquals(b.DataContext, row)` | — |
| `cs/missed-readonly-modifier` | A field assigned only in the constructor | `readonly` | `IDE0044` |
| `cs/empty-catch-block` | `catch (X) { }` | A comment inside the block saying why the failure may vanish, or the failure surfacing as [above](#c-and-net) | — |
| `cs/catch-nullreferenceexception` | `catch (NullReferenceException)` | Check for `null` before the call | — |
| `cs/complex-block` | Four loops or `if`s with bodies one after another in one block | Pull the repeated loop into a helper | — |

**Run `make codeql` before pushing a branch that touches `.cs` code.** It runs
the pull request's CodeQL analysis on this machine: the same CodeQL bundle (read
from the action commit `codeql.yml` pins), the same `security-and-quality`
queries, the same build and the same exclusions. It fails when a result lands
on a line the branch changed, which is what the ruleset would block. A first
run takes about ten minutes; the bundle is cached under `~/.cache/helfer-codeql`.
`scripts/codeql-results.py` is shared with CI. It drops every result in generated
code under `obj/` before upload, because `paths-ignore` cannot. Generated code
therefore never opens an alert and never needs an exclusion.

When a new CodeQL alert appears, never leave it open and never dismiss it
without a written reason:

1. **Fix it in code** if it is real. This is the default.
2. **If the query duplicates a rule the build already governs** — a CA or SA
   rule the repository has deliberately configured — exclude it by id in
   `.github/codeql/codeql-config.yml`, with a comment naming that rule and why
   the build's handling is the finer-grained one. Never exclude a query
   carrying a security severity.
3. **Only for a site-specific false positive**, dismiss that single alert with
   a comment explaining it. Prefer 1 and 2: a dismissal lives outside the
   repository, so no reviewer and no future reader ever sees the reasoning.

A `code_scanning` ruleset rule blocks a pull request that introduces a new
alert, so this is enforced rather than advisory.

Note that `paths` and `paths-ignore` do **not** work in the CodeQL config here:
GitHub restricts them to interpreted languages or `build-mode: none`, and this
is C# under `build-mode: manual`. Scope by rule id, or by the build target in
`codeql.yml`.

## Repository hygiene

Never commit `docs/superpowers/` — local tooling artifacts only. If tracked,
remove them and add to `.gitignore`.

## Changelog entries

Any PR touching `src/` must add **exactly one** changelog entry, as its own
file in `changelog.d/`, named `+<slug>.<type>.md`, where `<type>` is one of
`added`, `changed`, `deprecated`, `removed`, `fixed` or `security`. The file
holds the entry text as plain prose, with no leading `- `; reference issues
inline (`(#302)`). CI enforces that an entry exists — the `changelog entry` job
runs `towncrier check` — so do it in the same PR, not as a follow-up.

The entry is **English** and **one sentence**. `CHANGELOG.md` is one language
throughout, the way the README's user half is German for Feuerwehr crews and
`SECURITY.md` is English for security researchers; German terms of art stay
German — Stoppuhr, Atemschutz, Übung — because that is what the app says on
screen. The same job warns when an entry looks German, but only warns: it
cannot tell a Fachbegriff from a German sentence.

**Never write a pull request link into the fragment.** The rendered bullet
carries one — `- [#334](…/pull/334) - The PDF's Aufgaben section marks …` — but
it is recovered from git by `scripts/changelog-prlinks.py` when the release is
cut, because the number does not exist while the PR is open. An *issue*
reference inline is a different thing and stays in the prose.

One entry, not one per commit. A pull request is one change as far as the
release notes are concerned, and the entry describes what it does for the
reader, never the layers it was built in: a feature split across domain,
persistence and UI commits is still a single entry. A PR that also carries an
unrelated fix folds that into the same entry rather than adding a second file —
if the two really do not belong together, they are two pull requests.

Never edit `CHANGELOG.md` directly. It is assembled from `changelog.d/` by
`towncrier build` when a release is cut. Editing it from a feature branch is
what used to make every other open PR conflict, which is the whole reason for
the split. The leading `+` in the filename is required: it keeps filenames
unique so two PRs citing the same ticket cannot collide.

## Lagebuch

This repository started as a copy of [Lagebuch](https://github.com/CodeForFire/lagebuch)'s
stack, theme, workflows and rules, and is allowed to drift from it. A change to
the shared look (`Theme/Tokens.axaml`, `Theme/Styles.axaml`) or to a rule in
this file is worth proposing in both repositories, so the two apps keep reading
as one family. A reference written `lagebuch #123` points at that repository.
