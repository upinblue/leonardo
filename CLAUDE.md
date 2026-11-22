# CLAUDE.md – UpInBlue.Leonardo

## What this repository is

Leonardo is the design language of up in blue. This repository turns it into tools:

- a token generator that produces values for WinUI 3, WPF, iOS and macOS (SwiftUI), the web and PowerPoint,
- a WinUI 3 library and a WPF resource package,
- a WinUI 3 gallery app and small sample apps for iOS, macOS and the web,
- a Claude Code plugin with the skill `leonardo`, which designs, revises and reviews the UI of up in blue apps on every platform.

Leonardo never replaces the platform. Every app is first a good Windows, iOS, macOS or web app, with good usability; Leonardo is the up in blue handwriting on top.

## Sources of truth, in this order

1. `docs/design-language.md` – what things look like and how they behave. Rules are marked MUST, SHOULD or NEVER.
2. `tokens/leonardo.tokens.json` – every concrete value.
3. `docs/roadmap.md` – the stages, their scope and acceptance criteria.

If code contradicts the design language, the design language wins. If something is unclear, follow the last decision rule of the design language: stop and ask Bastian Wirth instead of inventing a design decision.

## Designing UI in this repository

The gallery and sample apps are designed with the skill in `claude/skills/leonardo`. Follow it, including its research step: before designing a page or pattern, read the platform's official guidance listed in `claude/skills/leonardo/references/sources.md` and name the sources in the pull request. `.claude/settings.json` allows web access to these sources.

## Repository

- GitHub: `upinblue/leonardo`, **private**. Never change the visibility.
- Maintainer: Bastian Wirth (up in blue). For `CODEOWNERS`, read the GitHub login with `gh api user --jq .login` and confirm it with Bastian before committing.
- Structure:

```
.claude/settings.json                Web access to official design guidance
brand/                               Logo files. Not MIT licensed, see TRADEMARKS.md. Never modify.
docs/                                design-language.md, roadmap.md, leonardo-design-language.pdf
tokens/                              leonardo.tokens.json
generated/apple/                     Asset catalog and Swift values for iOS and macOS (committed)
generated/web/                       leonardo.css with custom properties (committed)
generated/presentation/              Values for PowerPoint (committed)
src/UpInBlue.Leonardo.Tokens/        C# token generator (console app)
src/UpInBlue.Leonardo.WinUI/         WinUI 3 library
src/UpInBlue.Leonardo.Wpf/           WPF resource package
samples/UpInBlue.Leonardo.Gallery/   WinUI 3 gallery app
samples/UpInBlue.Leonardo.Gallery.Wpf/  Small WPF sample (stage 5)
samples/apple/                       SwiftUI multiplatform sample for iOS and macOS (stage 6)
samples/web/                         Standalone web sample (stage 6)
tests/UpInBlue.Leonardo.Tokens.Tests/
claude/                              Claude Code plugin; the skill lives in claude/skills/leonardo
```

`claude/skills/leonardo/references/design-language.md` and `leonardo.tokens.json` are copies. They are refreshed by the generator; never edit them by hand.

## Tech stack

- .NET 10, C# latest, nullable reference types enabled, warnings treated as errors.
- WinUI 3 with the latest stable Windows App SDK at the time the stage is implemented. Pin all package versions centrally in `Directory.Packages.props`.
- Minimum Windows version: Windows 11 (`TargetPlatformMinVersion` 10.0.22000.0).
- Gallery app: unpackaged (`WindowsPackageType` None) and Windows App SDK self-contained.
- WPF: .NET 10 with the built-in Fluent theme (`ThemeMode`).
- Gallery architecture: MVVM with CommunityToolkit.Mvvm.
- Apple sample: Swift and SwiftUI, one multiplatform app for iOS and macOS, built with the current stable Xcode. This stage runs on a Mac.
- Web sample: plain HTML and CSS, minimal JavaScript, no framework.
- Tests: xUnit.
- Token generator: C# only. No Node.js, no Style Dictionary.
- No GitHub Actions. Build and test locally.

## Hard rules for the library

1. **Only what Leonardo defines beyond Fluent.** Never restyle or reimplement WinUI controls such as Button, ListView, CommandBar, NavigationView, dialogs or menus. Leonardo adds exactly: token resources, text styles for the type roles, the accent ramp and content-layer neutrals, motion helpers, and three controls (`PlaceHeader`, `SummaryLine`, `StatusDot`).
2. **No literal design values.** Colors, spacing, radii, font sizes, durations and easing curves always come from generated token resources or constants. Literal values are only allowed inside generated files.
3. **No custom glass or material effects.** The window backdrop is `MicaBackdrop`. Acrylic only where Fluent itself uses it.
4. **Fallbacks are part of every feature.** Every animation respects the system animation setting. Every view works without transparency and in high contrast, where only system colors are used.
5. **Accessibility.** Status visuals have accessible names. Everything is reachable by keyboard. Focus visuals are never removed or restyled. Text contrast is at least 4.5:1.
6. **Generated files are never edited by hand.** Change the tokens and regenerate.
7. **Public API.** XML documentation on all public types and members. Namespaces start with `UpInBlue.Leonardo`.

## Naming

- Packages and namespaces: `UpInBlue.Leonardo.Tokens`, `UpInBlue.Leonardo.WinUI`, `UpInBlue.Leonardo.Wpf`.
- In prose, the brand name is always lowercase: up in blue.
- Design terms: place name → `PlaceHeader`, summary line → `SummaryLine`, status dot → `StatusDot`. Motions: `DiveIn`, `Transform`, `Stagger`, `Reflow`, `Arrive`, `Leave`, `Respond`.
- Resource keys start with `Leonardo`, for example `LeonardoAccentBrush`, `LeonardoSpaceM`, `LeonardoRadiusL`, `LeonardoPlaceNameTextStyle`, `LeonardoEaseEnter`, `LeonardoDurationNormal`.
- CSS custom properties start with `--leonardo-`, for example `--leonardo-accent`, `--leonardo-space-m`.
- Swift: a namespace enum `Leonardo` with nested types, for example `Leonardo.Space.m`; colors in the asset catalog named `Leonardo/Accent`, `Leonardo/TextPrimary`.

## Language

English everywhere: code, comments, commit messages, pull requests, README, app UI and sample content.

## Workflow

- One stage = one branch `stage/NN-short-name` = one pull request into `main`. Exception: stage 0 creates the repository and pushes the initial commit to `main`.
- Never commit to `main` after stage 0. Never merge pull requests yourself; Bastian reviews and merges.
- Before opening a pull request: everything builds and all tests pass locally without warnings, and the affected apps start.
- Pull request description: goal of the stage, the acceptance criteria from the roadmap as a checklist with the result of each, the design sources read, screenshots of new pages in light and dark mode, and open questions.
- Token values marked `"status": "proposal"` may be used, but every proposal that becomes visible must be listed in the pull request for confirmation.
- Commit messages: imperative mood, subject line of at most 72 characters.

## Sample content

Invented content only. No real customers, people or internal product details. Use these fictional companies: Hofmann Logistics, Brenner Industries, Lakeside Utilities, Kessler Construction, Mertens Medical. Use these fictional people: Anna Berg, Jonas Weber, Lea Schmid, Tim Kraus, Mara Lindqvist, Felix Okafor.
