# Changelog

All notable changes to this project are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project is pre-1.0 — breaking changes may occur in any release; see
[Semantic Versioning](https://semver.org/spec/v2.0.0.html) for what that means
once we reach 1.0.

## [Unreleased]

Unreleased entries are not kept in this file. Each change carries its own file under
[`changelog.d/`](changelog.d/), which is assembled into a section here when a release is
cut — see [CONTRIBUTING.md](CONTRIBUTING.md). For the commits themselves, see the
[unreleased changes][Unreleased].

<!-- towncrier release notes start -->

## [0.1.0] - 2026-11-08

The first full release of FwAssist: one Android app with a Stoppuhr, a Buchstabieralphabet, a Schaummittel-Rechner, and references for the RTH-Landeplatz and the Dekon-Platz.

### Added

- [#1](https://github.com/CodeForFire/helfer/pull/1) - An Android app hosting small fire-brigade helpers behind one overview, each with its own optional launcher icon, starting with a Stoppuhr.
- [#27](https://github.com/CodeForFire/helfer/pull/27) - An RTH-Landeplatz helper shows the requirements and steps for preparing a Rettungshubschrauber landing site, separately by day and by night, with drawings of our own (#26).
- [#32](https://github.com/CodeForFire/helfer/pull/32) - A Dekon-Platz helper shows the measures for Dekon-Stufe I, the build order and two layouts for Stufe II, and the facts and passage order for Stufe III, with drawings of our own (#25).
- [#40](https://github.com/CodeForFire/helfer/pull/40) - The Schaummittel-Rechner offers a preset button for a Zumischrate of 0,1 %.
- [#41](https://github.com/CodeForFire/helfer/pull/41) - An INFO button on the home screen opens a new page with the version, the project link, the licence and the copyright.
- [#8](https://github.com/CodeForFire/helfer/pull/8) - A Funkalphabet helper spells typed words for radio traffic with either the old spelling table or DIN 5009:2022.
- [#9](https://github.com/CodeForFire/helfer/pull/9) - A Schaummittel helper works out how much foam the supplies make and how much Wasser and Schaummittel a room needs, and inputs now survive the app being killed through a new helper settings store.

### Changed

- [#33](https://github.com/CodeForFire/helfer/pull/33) - The app is now called FwAssist and has a new launcher icon (#31).
- [#36](https://github.com/CodeForFire/helfer/pull/36) - The Schaummittel-Rechner shows its preset buttons beside the Zumischrate and Verschäumungszahl fields, so a whole calculation fits on one page without scrolling (#35).
- [#38](https://github.com/CodeForFire/helfer/pull/38) - The Funkalphabet is now called Buchstabieralphabet, offers the NATO alphabet, labels the old table BOS and shows the spelled result as one compact line (#37).

### Fixed

- [#11](https://github.com/CodeForFire/helfer/pull/11) - The Funkalphabet keeps the chosen spelling table when Android kills the app in the background (#10).


[Unreleased]: https://github.com/CodeForFire/helfer/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/CodeForFire/helfer/releases/tag/v0.1.0
