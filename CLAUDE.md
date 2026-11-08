# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@AGENTS.md

`AGENTS.md`, imported above, is the rulebook. This file adds only the commands and how the pieces meet at runtime.

## Commands

The `Makefile` wraps `dotnet` and needs a POSIX shell. Where `make` is missing (a plain Windows host), run the `dotnet` lines directly. The build-and-test workflow does not call `make`, but `mutation.yml` runs `make mutate` and `make mutate-src`, so changing those targets affects CI.

| Task | Command |
|---|---|
| Build, without the Android head | `make build` or `dotnet build Helfer.Host.slnf` |
| All tests | `make test` or `dotnet test Helfer.Host.slnf` |
| One test project | `make test PROJECT=tests/Helfer.Core.Tests` |
| One test class | `make test FILTER=FullyQualifiedName~StopwatchStateTests` or `dotnet test tests/Helfer.Helpers.Stoppuhr.Tests --filter FullyQualifiedName~StopwatchStateTests` |
| Format, then verify | `make format`, `make format-check` |
| Full local gate | `make clean ci` |
| CodeQL on changed lines | `make codeql` (`BASE=origin/main`) |
| Mutation test one project | `make mutate PROJECT=Helpers.Stoppuhr`; add `SINCE=origin/main` for the pull request gate (changed lines only, fails below `BREAK=80`) |
| Android APK, in Docker | `make apk`, or `make apk ABI=android-x64` for an emulator |
| Run on the emulator | `make run-android ABI=android-x64`, then `make logcat` |
| PR screenshots | `make phone-screenshots`, written to `out/phone-screenshots/` |
| README and Play screenshots | `make screenshots`, `make play-screenshots` |

- A bare `FILTER=Foo` matches method names only and silently runs nothing for a class name. Use `FullyQualifiedName~Foo`.
- Build against `Helfer.Host.slnf`, not `Helfer.sln`. The full solution includes the Android head, which only builds in the Docker image (`Microsoft.Android.Sdk` rejects JDKs above 21, `XA0030`).
- `make ci` restores with `--locked-mode`, so a package change has to commit the rewritten `packages.lock.json` files.

## How a helper reaches the screen

The app is one Activity showing one `ShellViewModel`. The home screen, the settings and every helper are pages of that shell.

1. `MainActivity.OnCreate` builds the platform services into a `HelperServices` and calls `CompositionRoot.CreateShell`. The shell is held in a **static** field, so it outlives Activity recreation; that is what keeps a running Stoppuhr alive through a configuration change. It does not survive process death (see Mobile in `AGENTS.md`).
2. `CompositionRoot.Modules` is the list of `IHelperModule`s, in home screen order. A module supplies plain data (`Id`, `Title`, `Summary`, SVG `IconPathData`) and a `CreateViewModel` factory, which keeps `AppLogic` and the helper's logic project free of Avalonia.
3. `ShellViewModel.TryShow` calls `CreateViewModel` once per helper per process and caches the result by `Id`. `Current` then changes to that view model.
4. `Helfer.Ui.ViewRegistry` maps the view model's exact type to a view factory. Each helper registers its own through `<Name>Views.Register`, because views live in one assembly per helper and are not found by naming convention.

Launcher icons are `<activity-alias>` entries pointing at `MainActivity`. `LauncherAlias.HelperIdOf` derives the helper `Id` from the alias class name in the intent, and `MainActivity.Route` calls `ShellViewModel.OpenFromLauncher` with it. An `Id` no module claims lands on the home screen. `OpenFromLauncher` also sets the flag that makes system Back leave the app.
