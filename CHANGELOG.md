# Changelog

All notable changes to Leonardo are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). The version applies to the NuGet packages
`UpInBlue.Leonardo.WinUI` and `UpInBlue.Leonardo.Wpf` and to the generated files in `generated/`.

While the major version is 0, any minor version may change resource keys, APIs or token values.

## [0.1.0] - Unreleased

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
- The Claude Code plugin with the skill `leonardo`, which designs, revises and reviews UI on every platform.
- NuGet packaging for both libraries, the publishing guide `docs/publishing.md` and the checklist
  `docs/going-public.md`.

### Notes

- Some token values are proposals and may change before 1.0: the corner radii, the surface, text and stroke colors of
  both themes, the link and selection colors of the dark theme, `motion.parallax.placeNameFactor` and
  `motion.arrive.hold`. They are marked `"status": "proposal"`
  in the tokens and `/* proposal */` in `leonardo.css`.
- There is no Apple sample app yet. SwiftUI apps use the files in `generated/apple`.

[0.1.0]: https://github.com/upinblue/leonardo/releases/tag/v0.1.0
