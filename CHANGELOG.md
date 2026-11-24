# Changelog

All notable changes to Leonardo are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). The version applies to the NuGet packages
`UpInBlue.Leonardo.WinUI`, `UpInBlue.Leonardo.Wpf` and `UpInBlue.Leonardo.Check` and to the generated files in
`generated/`.

While the major version is 0, any minor version may change resource keys, APIs or token values.

## [Unreleased]

### Changed

- `PlaceHeader` follows the scroll position instead of a timed shrink animation (#17). The meta line fades instead of
  being covered, the title scales continuously to the compact line, and scrolling back reverses every step. The header
  now reserves the shrink distance at the top of the scrolling content and no longer corrects the scroll offset.
- `PlaceHeaderMath` has new members for the scroll-linked values; `Threshold`, `ParallaxLift`, `NextIsCompact` and
  `CompensatedOffset` are removed.

## [0.1.1] - 2026-11-23

First version on nuget.org. The repository is public.

### Changed

- The packages are published to nuget.org with Trusted Publishing (`.github/workflows/release.yml`); the READMEs
  describe installation from nuget.org.
- Added `SECURITY.md`. Removed the working files `docs/roadmap.md` and `.claude/settings.json`.

## [0.1.0] - 2026-11-22

The first version of Leonardo as tools.

### Added

- Design language in `docs/design-language.md` and its visual version as PDF.
- Design tokens in `tokens/leonardo.tokens.json` and the C# token generator `UpInBlue.Leonardo.Tokens` with the
  commands `generate` and `check`. It produces values for WinUI 3, WPF, iOS and macOS (asset catalog and Swift), the web
  (`leonardo.css`) and PowerPoint.
- `UpInBlue.Leonardo.WinUI`, a library for WinUI 3 on the Windows App SDK:
  - token resources (`Leonardo…`), the accent ramp for Fluent (`SystemAccentColor…`) and text styles for the type
    roles, merged from `ms-appx:///UpInBlue.Leonardo.WinUI/Themes/Leonardo.xaml`;
  - motion helpers `DiveIn`, `Transform`, `Stagger`, `Reflow`, `Arrive` and `Leave`, with `MotionSettings` for the
    system animation setting and `MotionInterruption` so that motion never blocks input;
  - the controls `PlaceHeader`, `SummaryLine` and `StatusDot`.
- `UpInBlue.Leonardo.Wpf`, resources for WPF on the built-in Fluent theme: token dictionaries for light, dark and
  contrast themes, text styles, the accent override, `LeonardoTheme.Apply` and `LeonardoMotion`.
- The WinUI 3 gallery app with the foundation pages, the motion page, a page per control and four scenarios with
  invented content.
- A WPF sample window with colors, typography, a density switch and a staggered entrance.
- A standalone web sample with the Files scenario: semantic HTML, keyboard support, light and dark, reduced motion and
  forced colors.
- The Claude Code plugin with the skill `leonardo`, which designs, revises and reviews UI on every platform, in the
  marketplace `upinblue`.
- The check tool `leonardo-check`, packed as the .NET tool `UpInBlue.Leonardo.Check`. It finds literal colors, font
  sizes, spacing, radii, durations and easing curves, custom glass, removed focus visuals, bouncing springs, missing
  reduced-motion rules and clickable `div` elements in XAML, C#, Swift, CSS, SCSS, HTML and TSX. The plugin runs it as
  a `PostToolUse` hook, which prints a one-line warning and never blocks when the tool is not installed.
- Tokens: `theme.*.chart` for single-series charts on every platform (`LeonardoChartBrush`, `Leonardo.Colors.chart`,
  `--leonardo-chart`); `size.icon` and `size.statusDot` (`LeonardoSizeIcon`, `LeonardoSizeStatusDot`,
  `Leonardo.Size`, `--leonardo-size-*`); for the standalone web only, `theme.*.focus`, `size.focusStroke`,
  `theme.*.accentHover` and `theme.*.accentPressed`. Swift also has `Leonardo.Motion.arriveHold`.
- Tokens can be limited to some outputs with `$extensions.platforms`.
- NuGet packaging for both libraries and the check tool, the publishing guide `docs/publishing.md` and the checklist
  `docs/going-public.md`.

### Changed

Decisions made before the first release (stage 10), compared with the previews on the stage branches:

- All token proposals are confirmed; no token is a proposal any more.
- The status colors (`theme.*.status*`) exist only for the standalone web and presentations. WinUI, WPF, C#, Swift and
  the asset catalog no longer contain them: Windows uses Fluent's status brushes, iOS and macOS use system colors.
- In WinUI, `LeonardoStatusDotSize` is replaced by `LeonardoSizeStatusDot`, and the Arrive marker is a short accent bar.
- Design language:
  - the page margin is `space.l`; on pages with icon lists the alignment line lies at `space.xl`;
  - `motion.duration.short` (167 ms) is for exits only; entrances start at `motion.duration.normal`;
  - stagger happens on navigation to a view, never at app launch;
  - checked toggle buttons count as selection, not as the one accent surface for actions;
  - the Arrive marker is a short accent bar; the dot shape is reserved for the status dot;
  - on the standalone web, a browser focus ring with less than 3:1 contrast is replaced by the focus tokens;
  - Inter is described neutrally: used where it is installed or shipped, otherwise the system font;
  - Apple density: iOS keeps the system row heights; on macOS, compact is the system's table row.

### Notes

- There is no Apple sample app yet; it is stage 6b and needs a Mac. SwiftUI apps use the files in `generated/apple`,
  added to the app target (the generated colors are looked up in the main bundle).
- The packages are published to nuget.org with Trusted Publishing (see `docs/publishing.md`).

[0.1.1]: https://github.com/upinblue/leonardo/releases/tag/v0.1.1
[0.1.0]: https://github.com/upinblue/leonardo/releases/tag/v0.1.0
