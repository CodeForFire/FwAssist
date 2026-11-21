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

## [0.2.1] - 2026-11-21

The Hydrantenkarte, sharpened after its first use: pinching zooms the map again, the chosen Hydrant carries a red arrow and stays on the map when zoomed out, and the Richtungspfeil announces the arrival with a tone, a vibration and AM HYDRANTEN in large letters. FwAssist now also asks for the vibration permission, which Android grants at install without a dialog.

### Added

- [#73](https://github.com/CodeForFire/FwAssist/pull/73) - At the hydrant the Richtungspfeil plays a short tone and vibrates once, as the phone's silent setting allows, and shows AM HYDRANTEN in large letters above a smaller arrow that still guides the last metres (#70).

### Changed

- [#72](https://github.com/CodeForFire/FwAssist/pull/72) - The Hydrantenkarte marks the chosen Hydrant with a red arrow and a thicker frame, and keeps it on the map when zoomed out too far to show the others (#69).

### Fixed

- [#71](https://github.com/CodeForFire/FwAssist/pull/71) - Pinching the Hydrantenkarte zooms in and out around the fingers instead of throwing the map far off the Kartenpaket (#68).


## [0.2.0] - 2026-11-20

The Hydrantenkarte: the Hydranten around the Einsatzstelle on an offline OpenStreetMap map, the nearest ones with their distance and B-Schläuche, a Richtungspfeil and a Mein Standort button, with the Kartenpakete downloaded under Einstellungen. FwAssist now asks for two permissions, Internet for the Kartenpakete and location for the arrow and the button. The helpers can also be sorted in Einstellungen.

### Added

- [#61](https://github.com/CodeForFire/FwAssist/pull/61) - FwAssist can now install Lagebuch's offline Kartenpakete and draw their OpenStreetMap map with its Hydranten and other Wasserentnahmestellen, the groundwork for the Hydrantenkarte (#60).
- [#62](https://github.com/CodeForFire/FwAssist/pull/62) - The new Hydrantenkarte shows the Hydranten on an offline OpenStreetMap map, lets the user pin the Einsatzstelle, and lists the nearest Hydranten with their straight-line distance and the B-Schläuche each takes, with the Kartenpakete it needs downloaded under Einstellungen (#60).
- [#63](https://github.com/CodeForFire/FwAssist/pull/63) - The Hydrantenkarte gains a Richtungspfeil that points from the phone's live position to the chosen hydrant, turned by the compass and with the distance still to go, asking for the location permission only when it is first opened (#60).
- [#65](https://github.com/CodeForFire/FwAssist/pull/65) - The Hydrantenkarte gains a Mein Standort button above the zoom buttons that centres the map on the phone's position once, marks it with a blue dot and switches the GPS off again (#60).
- [#66](https://github.com/CodeForFire/FwAssist/pull/66) - Einstellungen can sort the helpers by dragging each one by its grip to a new place, and the overview shows them in that order, kept across restarts (#39).

### Changed

- [#47](https://github.com/CodeForFire/FwAssist/pull/47) - The repository moved to CodeForFire/FwAssist and release downloads are now named `fwassist-<version>.apk`, while the Android package stays `de.codeforfire.helfer` so installed copies keep updating.

### Fixed

- [#46](https://github.com/CodeForFire/FwAssist/pull/46) - RTH-Landeplatz now shows TAG or NACHT with its timeframe at the top of the page, and both stay visible while scrolling (#44).
- [#64](https://github.com/CodeForFire/FwAssist/pull/64) - Google Play no longer hides FwAssist from devices without location hardware, such as Wi-Fi-only tablets, which the location permission had made it require.

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


[Unreleased]: https://github.com/CodeForFire/FwAssist/compare/v0.2.1...HEAD
[0.2.1]: https://github.com/CodeForFire/FwAssist/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/CodeForFire/FwAssist/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/CodeForFire/helfer/releases/tag/v0.1.0
