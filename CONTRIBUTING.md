# Contributing to Leonardo

Thank you for helping. Leonardo is the design language of up in blue, so changes to how things look or behave are
design decisions first and code second. This guide says how to make both.

## Sources of truth

1. [`docs/design-language.md`](docs/design-language.md): what things look like and how they behave. Rules are marked
   MUST, SHOULD or NEVER.
2. [`tokens/leonardo.tokens.json`](tokens/leonardo.tokens.json): every concrete value.

If code contradicts the design language, the design language wins. If a case is not covered, follow the decision rules
at the top of the design language. If it is still unclear, open an issue and ask instead of inventing a design
decision. Bastian Wirth (up in blue) decides.

## Before you start

- Requirements: Windows 11 and the .NET 10 SDK (see `global.json`). The Apple sample (stage 6b) needs a Mac with the
  current stable Xcode.
- Build, test and check the generated files:

  ```powershell
  dotnet build UpInBlue.Leonardo.slnx
  ```

  ```powershell
  dotnet test --solution UpInBlue.Leonardo.slnx
  ```

  ```powershell
  dotnet run --project src/UpInBlue.Leonardo.Tokens -- check
  ```

- Check UI code for literal design values with the check tool:

  ```powershell
  dotnet run --project src/UpInBlue.Leonardo.Check -- src samples
  ```

- Warnings are errors. A change is ready only when the build has no warnings and all tests pass.
- There is no CI. Build and test locally before you open a pull request.

## Rules for the libraries

1. **Only what Leonardo defines beyond Fluent.** Never restyle or reimplement platform controls. Leonardo adds token
   resources, text styles, the accent ramp, motion helpers and the controls `PlaceHeader`, `SummaryLine` and `StatusDot`.
2. **No literal design values.** Colors, spacing, radii, font sizes, durations and easing curves come from token
   resources or generated constants.
3. **No custom glass or material effects.** The window backdrop is Mica.
4. **Fallbacks are part of every feature.** Every animation respects the system animation setting. Every view works
   without transparency and in high contrast.
5. **Accessibility.** Status visuals have accessible names, everything is reachable by keyboard, focus visuals are never
   removed or restyled, and text contrast is at least 4.5:1.
6. **Generated files are never edited by hand.** Change the tokens and run
   `dotnet run --project src/UpInBlue.Leonardo.Tokens -- generate`. This also refreshes the copies in
   `claude/skills/leonardo/references/`.
7. **Public API.** XML documentation on all public types and members. Namespaces start with `UpInBlue.Leonardo`.

## Changing a token

1. Edit `tokens/leonardo.tokens.json`. A value that is not decided yet gets `"status": "proposal"`.
2. Run the generator with `generate` and commit the generated files together with the token change.
3. In the pull request, list every proposal that becomes visible, so it can be confirmed.

## Designing UI

The gallery and the samples are designed with the Claude Code skill in `claude/skills/leonardo`, whether you use Claude
Code or not. Before designing a page or pattern, read the platform's official guidance listed in
`claude/skills/leonardo/references/sources.md` and name the sources you read in the pull request.

Sample content is invented. Use the fictional companies Hofmann Logistics, Brenner Industries, Lakeside Utilities,
Kessler Construction and Mertens Medical and the fictional people Anna Berg, Jonas Weber, Lea Schmid, Tim Kraus, Mara
Lindqvist and Felix Okafor. Never use real customers, people or internal product details.

## Brand files

The files in `brand/` and the names "up in blue" and "uib" are not covered by the MIT License; see
[`TRADEMARKS.md`](TRADEMARKS.md). Never modify the files in `brand/`.

## Branches, commits and pull requests

- One topic per branch and per pull request into `main`. Roadmap stages use `stage/NN-short-name`.
- Never commit to `main` directly. Bastian Wirth reviews and merges.
- Commit messages are in the imperative mood, with a subject line of at most 72 characters.
- A pull request describes:
  - the goal,
  - the acceptance criteria as a checklist with the result of each,
  - the design sources read,
  - screenshots of new or changed pages in light and dark mode,
  - open questions.
- Add a line to `CHANGELOG.md` under the next version for every change users notice.

## Language and naming

- English everywhere: code, comments, commit messages, pull requests, documentation and app UI.
- In prose, the brand name is always lowercase: up in blue.
- Resource keys start with `Leonardo`, CSS custom properties with `--leonardo-`, and Swift values live in the namespace
  enum `Leonardo`.

## License

By contributing, you agree that your contribution is licensed under the [MIT License](LICENSE) of this repository
("inbound = outbound"). There is no separate contributor license agreement and no sign-off requirement.
