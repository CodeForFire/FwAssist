# Common development tasks. Run `make` (or `make help`) for the target list.
#
# These targets only wrap the commands already documented in README.md,
# CONTRIBUTING.md and .github/workflows/ — nothing here is a build system of its
# own. CI calls it only for mutation testing: mutation.yml runs `mutate` and
# `mutate-src`.
#
# The Android head is built in Docker, never on the host: Microsoft.Android.Sdk
# rejects any JDK above 21 (error XA0030) and current distros ship newer ones.
# Everything after the build — emulator, install, logcat — uses the host
# Android SDK, because the build image has no emulator. `build` and `test`
# therefore run against Helfer.Host.slnf, the solution minus that head; `apk`
# builds the head in Docker, and CI builds the full solution.

DOTNET       ?= dotnet
CONFIG       ?= Debug
SLN          := Helfer.sln
SLNF         := Helfer.Host.slnf
ANDROID_PROJ := src/Helfer.App.Android/Helfer.App.Android.csproj
APP_ID       := de.codeforfire.helfer
FILTER       ?=
PROJECT      ?=
VERSION      ?= 0.1.0
# ABI=android-x64 builds `apk` for one ABI only: a third of the size, which is what fits on an
# emulator whose /data is nearly full. Leave it empty for the three-ABI build CI and Play get.
ABI          ?=

# Google Play upload key. Outside the work tree on purpose — signing material is never
# committed, and $(DOCKER_HOME) is a build cache that gets wiped. See docs/releasing.md.
KEYSTORE      ?= $(HOME)/.config/helfer/helfer-upload.p12
KEYSTORE_PASS ?= $(HOME)/.config/helfer/helfer-upload.pass
KEY_ALIAS     ?= helfer-upload
# Derived from VERSION by scripts/android-version-code.sh, which the release workflow uses too.
# Set it only to override that; Google Play burns a versionCode permanently on first upload, so
# a number typed by hand is a mistake that cannot be taken back.
CODE          ?=

ANDROID_HOME ?= $(HOME)/Android/Sdk
ADB          := $(ANDROID_HOME)/platform-tools/adb
EMULATOR_BIN := $(ANDROID_HOME)/emulator/emulator
# The phone the layout is written against: 1080 px at 420 dpi = 411 dp, the width the
# acceptance tests render at. `make emulator-tablet` boots a tablet to check it still holds.
AVD          ?= Medium_Phone_API_35
TABLET_AVD   ?= Tablet_API_35
# The same Dockerfile as Lagebuch's, so IMAGE=lagebuch-android-build reuses an image already built.
IMAGE        ?= helfer-android-build
DOCKER_HOME  ?= $(HOME)/.cache/helfer-android-build
APK          := src/Helfer.App.Android/bin/$(CONFIG)/net10.0-android/$(APP_ID)-Signed.apk
# `aab` always builds Release, whatever CONFIG says — a debug bundle is not uploadable.
AAB          := src/Helfer.App.Android/bin/Release/net10.0-android/$(APP_ID)-Signed.aab
RELEASE_APK  := src/Helfer.App.Android/bin/Release/net10.0-android/$(APP_ID)-Signed.apk

# xunit.v3 launches each test assembly as its own apphost, which fails with
# "You must install .NET to run this application" unless DOTNET_ROOT points at
# the runtime — not the case when the SDK lives outside the system prefix
# (~/.dotnet). Derive it from the dotnet actually on PATH; an existing
# DOTNET_ROOT in the environment wins.
export DOTNET_ROOT ?= $(patsubst %/,%,$(dir $(realpath $(shell command -v $(DOTNET)))))

TEST_FILTER := $(if $(FILTER),--filter $(FILTER),)
# FILTER on its own matches method names only and silently runs nothing for a class name; use
# FILTER=FullyQualifiedName~ClassName, and narrow with PROJECT=tests/Helfer.Core.Tests.
TEST_TARGET := $(if $(PROJECT),$(PROJECT),$(SLNF))

.DEFAULT_GOAL := help

.PHONY: help restore build test mutate mutate-src format format-check codeql ci clean \
        android-image android-image-rebuild apk aab emulator emulator-tablet install run-android \
        logcat uninstall screenshots play-listing-check play-screenshots phone-screenshots

help: ## Show this help
	@awk 'BEGIN {FS = ":.*## "} \
	     /^[a-zA-Z0-9_-]+:.*## / {printf "  \033[36m%-22s\033[0m %s\n", $$1, $$2} \
	     /^## / {printf "\n%s\n", substr($$0, 4)}' $(MAKEFILE_LIST)
	@echo ""
	@echo "  Variables: CONFIG=$(CONFIG) AVD=$(AVD) FILTER= PROJECT= VERSION=$(VERSION)"
	@echo "             ANDROID_HOME=$(ANDROID_HOME)"

## .NET (host)

restore: ## Restore NuGet packages
	$(DOTNET) restore $(SLNF)

build: ## Build everything except the Android head
	$(DOTNET) build $(SLNF) -c $(CONFIG)

test: ## Run tests (PROJECT=path and/or FILTER=FullyQualifiedName~Foo to narrow)
	$(DOTNET) test $(TEST_TARGET) -c $(CONFIG) $(TEST_FILTER)

# Projects with a tests/Helfer.<name>.Tests/stryker-config.json, by the part after "Helfer.".
# The acceptance tests are left out: headless UI is too slow to repeat once per mutant.
MUTATE_PROJECTS := Core AppLogic Helpers.Stoppuhr Helpers.Funkalphabet Helpers.Schaummittel Helpers.RthLandeplatz Helpers.DekonPlatz

# Where each project's source lives, for SINCE= below.
mutate_src = $(if $(filter Helpers.%,$(1)),src/Helpers/$(patsubst Helpers.%,%,$(1))/Helfer.$(1),src/Helfer.$(1))

mutate-src: ## Print a mutation project's source directory (PROJECT=...), for mutation.yml
	@echo $(call mutate_src,$(PROJECT))

# SINCE=<committish> is the pull request gate in .github/workflows/mutation.yml
# (SINCE=origin/main): it mutates only the lines of .cs files changed since the
# merge base, working tree included, passed to --mutate as character spans,
# then scores those mutants and fails below BREAK. Stryker's own --since is not
# used: it mutates and scores whole files, and inside a git worktree it finds no
# changes at all.
#
# Without SINCE, the whole project is mutated and fails below the floor in its
# stryker-config.json (thresholds.break).
SINCE ?=
BREAK ?= 80
MUTATE_OUT = $(CURDIR)/StrykerOutput/$(PROJECT)
MUTATE_SCORE = python3 -I scripts/mutation-diff-score.py

mutate: ## Mutation-test one project (PROJECT=Core|AppLogic|Helpers.<Name> [SINCE= BREAK=80])
	@if [ -z "$(filter $(PROJECT),$(MUTATE_PROJECTS))" ]; then \
	  echo "Usage: make mutate PROJECT=<one of: $(MUTATE_PROJECTS)>"; exit 2; fi
	@rm -f $(MUTATE_OUT)/reports/changed-lines.md
	@if [ -z "$(SINCE)" ]; then \
	  (cd tests/Helfer.$(PROJECT).Tests && $(DOTNET) tool run dotnet-stryker --output $(MUTATE_OUT)) || exit $$?; \
	  echo "Report: $(MUTATE_OUT)/reports/mutation-report.html"; \
	else \
	  patterns=$$($(MUTATE_SCORE) patterns --base "$(SINCE)" --project-dir $(call mutate_src,$(PROJECT))) || exit 2; \
	  if [ -z "$$patterns" ]; then \
	    echo "No line in Helfer.$(PROJECT) added or changed since $(SINCE) -- nothing to mutate."; exit 0; fi; \
	  set --; for p in $$patterns; do set -- "$$@" --mutate "**/$$p"; done; \
	  (cd tests/Helfer.$(PROJECT).Tests && $(DOTNET) tool run dotnet-stryker --output $(MUTATE_OUT) --break-at 0 "$$@") || exit $$?; \
	  echo "Report: $(MUTATE_OUT)/reports/mutation-report.html"; \
	  $(MUTATE_SCORE) score --report $(MUTATE_OUT)/reports/mutation-report.json --base "$(SINCE)" \
	    --project-dir $(call mutate_src,$(PROJECT)) --threshold $(BREAK) --summary $(MUTATE_OUT)/reports/changed-lines.md; \
	fi

# whitespace+style only: the analyzer pass is already a build gate here
# (AnalysisMode=All + TreatWarningsAsErrors), and `dotnet format analyzers`
# evaluates test projects differently from the build.
format: ## Apply dotnet format (whitespace + style)
	$(DOTNET) format whitespace $(SLNF)
	$(DOTNET) format style $(SLNF)

format-check: ## Fail if dotnet format would change anything (CI runs this too)
	$(DOTNET) format whitespace $(SLNF) --verify-no-changes
	$(DOTNET) format style $(SLNF) --verify-no-changes

BASE ?= origin/main

codeql: ## Run the PR's CodeQL analysis locally; fail on a result in lines changed since BASE (origin/main)
	scripts/codeql-local.sh "$(BASE)"

ci: ## Reproduce CI's build, test and format legs locally (everything except the Android head)
	$(DOTNET) restore $(SLNF) --locked-mode
	$(DOTNET) build $(SLNF) --configuration Release --no-restore
	$(DOTNET) test $(SLNF) --configuration Release --no-build --verbosity normal
	$(MAKE) format-check

clean: ## Remove build output (bin/ and obj/)
	-$(DOTNET) clean $(SLNF)
	find src tests -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

## Android (built in Docker, run on the host)

android-image: ## Build the Android build image if it is missing
	@docker image inspect $(IMAGE) >/dev/null 2>&1 \
	  || docker build -f docker/android-build.Dockerfile -t $(IMAGE) docker/

android-image-rebuild: ## Rebuild the Android build image unconditionally
	docker build -f docker/android-build.Dockerfile -t $(IMAGE) docker/

# -u keeps bin/ and obj/ owned by the host user — a root-owned build tree breaks
# the next host `make build`, and only root can clear it again. That needs a
# writable HOME, which $(DOCKER_HOME) provides: a Docker named volume would be
# root-owned and unwritable, and a throwaway HOME would regenerate the Android
# debug keystore on every run, so each APK would be signed with a different key
# and `adb install -r` would reject the update. The host NuGet cache is mounted
# inside it so packages carry over from host builds.
apk: android-image ## Build an installable APK in Docker
	@mkdir -p "$(DOCKER_HOME)"
	docker run --rm \
	  -u $$(id -u):$$(id -g) \
	  -e HOME=/home/build -e DOTNET_CLI_HOME=/home/build \
	  -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
	  -v "$(DOCKER_HOME)":/home/build \
	  -v "$$HOME/.nuget":/home/build/.nuget \
	  -v "$$PWD":/src $(IMAGE) \
	  publish $(ANDROID_PROJ) -c $(CONFIG) -f net10.0-android \
	  -p:AndroidPackageFormat=apk -p:EmbedAssembliesIntoApk=true \
	  $(if $(ABI),-p:OnlyAbi=$(ABI))
	@echo "APK: $(APK)"

# The release bundle for Google Play. Unlike `apk` this one is signed with the real upload key,
# so it needs the keystore mounted in — read-only, and as its own mount, because it deliberately
# lives outside both the work tree and $(DOCKER_HOME). The password is passed as file:, never as
# a literal: an -p:AndroidSigningStorePass=<secret> would land in the MSBuild binlog and in `ps`,
# and the env: form is documented as unsupported when the package format is aab.
#
# One publish emits both files: the .aab, and the universal .apk that bundletool extracts from
# that same signed bundle. The .apk is therefore worth installing as a check on the bundle.
aab: android-image ## Build the signed .aab for Google Play (VERSION=x.y.z)
	@test -f "$(KEYSTORE)" \
	  || { echo "No upload keystore at $(KEYSTORE) — see docs/releasing.md, 'Google Play'."; exit 1; }
	@test -f "$(KEYSTORE_PASS)" \
	  || { echo "No keystore password file at $(KEYSTORE_PASS) — see docs/releasing.md."; exit 1; }
	@mkdir -p "$(DOCKER_HOME)"
	@code="$(CODE)"; \
	  if [ -z "$$code" ]; then code=$$(scripts/android-version-code.sh "$(VERSION)") || exit 1; fi; \
	  echo "versionCode $$code   ($(VERSION))"; \
	  docker run --rm \
	    -u $$(id -u):$$(id -g) \
	    -e HOME=/home/build -e DOTNET_CLI_HOME=/home/build \
	    -e DOTNET_NOLOGO=1 -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
	    -v "$(DOCKER_HOME)":/home/build \
	    -v "$$HOME/.nuget":/home/build/.nuget \
	    -v "$$PWD":/src \
	    -v "$(dir $(KEYSTORE))":/keys:ro \
	    -v "$(abspath $(KEYSTORE_PASS))":/keypass/$(notdir $(KEYSTORE_PASS)):ro \
	    $(IMAGE) \
	    publish $(ANDROID_PROJ) -c Release -f net10.0-android \
	    -p:EmbedAssembliesIntoApk=true \
	    -p:ApplicationVersion=$$code \
	    -p:ApplicationDisplayVersion=$(VERSION) -p:Version=$(VERSION) \
	    -p:AndroidSigningKeyStore=/keys/$(notdir $(KEYSTORE)) \
	    -p:AndroidSigningKeyAlias=$(KEY_ALIAS) \
	    -p:AndroidSigningStorePass=file:/keypass/$(notdir $(KEYSTORE_PASS)) \
	    -p:AndroidSigningKeyPass=file:/keypass/$(notdir $(KEYSTORE_PASS))
	@echo "AAB: $(AAB)"
	@echo "APK: $(RELEASE_APK)   (universal, extracted from that bundle)"

emulator-tablet: ## Boot the tablet emulator (TABLET_AVD=name), to check a layout still holds there
	$(MAKE) emulator AVD=$(TABLET_AVD)

emulator: ## Boot the emulator (AVD=name) and wait for it
	@test -x "$(EMULATOR_BIN)" \
	  || { echo "No emulator at $(EMULATOR_BIN) — set ANDROID_HOME=/path/to/sdk"; exit 1; }
	@if $(ADB) devices | grep -q "device$$"; then \
	  echo "A device is already attached — skipping launch."; \
	else \
	  "$(EMULATOR_BIN)" -list-avds | grep -qx "$(AVD)" || { \
	    echo "AVD '$(AVD)' not found. Available: $$("$(EMULATOR_BIN)" -list-avds | tr '\n' ' ')"; \
	    echo "Create one with:"; \
	    echo "  $(ANDROID_HOME)/cmdline-tools/latest/bin/android sdk install system-images/android-36/google_apis/x86_64"; \
	    echo "  $(ANDROID_HOME)/cmdline-tools/latest/bin/android emulator create --profile=$(AVD)"; \
	    echo "  ('android emulator create --list-profiles' shows other device profiles)"; \
	    exit 1; \
	  }; \
	  echo "Booting $(AVD)..."; \
	  "$(EMULATOR_BIN)" -avd $(AVD) >/dev/null 2>&1 & \
	  $(ADB) wait-for-device; \
	  until [ "$$($(ADB) shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" = "1" ]; do sleep 2; done; \
	  echo "$(AVD) booted."; \
	fi

install: apk ## Install the APK on the attached device/emulator
	@$(ADB) install -r $(APK) || { \
	  echo "If this failed with INSTALL_FAILED_UPDATE_INCOMPATIBLE, the installed"; \
	  echo "copy was signed with a different debug key — run 'make uninstall' first."; \
	  exit 1; \
	}

run-android: emulator install ## Boot the emulator, install and launch the app
	$(ADB) shell am start -n $(APP_ID)/de.codeforfire.helfer.MainActivity

logcat: ## Follow logcat for the running app
	@pid=$$($(ADB) shell pidof -s $(APP_ID) 2>/dev/null | tr -d '\r'); \
	if [ -z "$$pid" ]; then \
	  echo "$(APP_ID) is not running — start it with 'make run-android'."; exit 1; \
	fi; \
	$(ADB) logcat --pid=$$pid

uninstall: ## Remove the app from the attached device/emulator
	$(ADB) uninstall $(APP_ID)

## Screenshots and the Play listing

# Writes into docs/: the set the README shows, at the phone size the layout is written for.
screenshots: ## Regenerate docs/screenshots/*.png (DemoFlowRenderTests with RENDER_OUT)
	RENDER_OUT=$(CURDIR)/docs/screenshots $(DOTNET) test tests/Helfer.Acceptance.Tests -c $(CONFIG) \
	  --filter FullyQualifiedName~DemoFlowRenderTests

# Writes outside docs/ on purpose: these are for a pull request's before/after pair, not for the
# repository.
phone-screenshots: ## Render the phone screens to out/phone-screenshots (412x915, for a PR)
	RENDER_OUT=$(CURDIR)/out/phone-screenshots $(DOTNET) test tests/Helfer.Acceptance.Tests -c $(CONFIG) \
	  --filter FullyQualifiedName~DemoFlowRenderTests

# Google Play rejects a phone screenshot that is not exactly 9:16 (or 16:9).
play-screenshots: ## Regenerate docs/play/screenshots/*.png at 9:16 for the Play listing
	RENDER_OUT=$(CURDIR)/docs/play/screenshots RENDER_WIDTH=540 RENDER_HEIGHT=960 \
	  $(DOTNET) test tests/Helfer.Acceptance.Tests -c $(CONFIG) \
	  --filter FullyQualifiedName~DemoFlowRenderTests

play-listing-check: ## Check the Play Store listing text against the Console's limits
	scripts/check-play-listing.sh
