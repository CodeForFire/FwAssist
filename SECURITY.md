# Security Policy

## Supported versions

FwAssist is pre-1.0 and moves quickly. Only the **latest release** receives
security fixes; older releases are not patched.

| Version | Supported |
|---------|-----------|
| latest release | :white_check_mark: |
| earlier releases | :x: |

## Reporting a vulnerability

**Please do not report security vulnerabilities through public GitHub issues,
discussions, or pull requests.**

Use [GitHub's private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability)
instead:

1. Go to the repository's **Security** tab.
2. Click **Report a vulnerability**.
3. Describe the issue, including:
   - the affected version (shown at the bottom of the app's home screen)
   - the device and Android version
   - steps to reproduce or a proof of concept
   - the impact you believe it has

Reports are reviewed promptly. Please give maintainers reasonable time to
address the issue before any public disclosure.

## Scope notes

- **The app declares one Android permission, `INTERNET`.** It is used only to
  download the Hydrantenkarte's Kartenpakete from `CodeForFire/lagebuch-geodata`
  when the user asks for them under Einstellungen. A downloaded catalogue or
  pack is hostile input: only https is fetched, the catalogue and every pack are
  read against size caps, a pack is unpacked only when its SHA-256 matches the
  catalogue and every file matches its own `pack.json`, and its files are written
  under fixed names inside the app's own folder. A way past any of those checks
  is in scope.
- **Otherwise the app reads no files** and receives no intents other than its
  own launcher icons. Anything that lets another app or a website make it do
  something — an exported component reachable with crafted extras, for
  example — is in scope.
- **Data stays on the device.** Nothing is sent to the developers; there is no
  server, and a pack download carries nothing about the user or an Einsatz. A
  helper that ever changes that will say so in this file first.
- **Build and release integrity** — the GitHub Actions workflows, the pinned
  actions, the release artifacts and their Sigstore attestations — is in scope.

## Non-security issues

For bugs that are not security-relevant, please use the
[issue tracker](../../issues/new/choose).
