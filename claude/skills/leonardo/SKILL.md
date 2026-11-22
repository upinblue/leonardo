---
name: leonardo
description: Designs, revises and reviews user interfaces of up in blue software with the platform's official guidelines and the Leonardo design language. Use when a task creates or changes UI - screens, pages, views, dialogs, layouts, navigation, XAML, SwiftUI views, HTML and CSS, colors, typography, spacing, icons, animations or UI text - or asks for a usability, accessibility or design review of an app, website, slide deck or document. Covers Windows (WinUI 3, WPF), iOS and macOS (SwiftUI), the web (standalone, SharePoint, Teams) and PowerPoint. Not for back-end, data, build, test or refactoring work that leaves the UI unchanged.
---

# Leonardo – designing up in blue software

You design software people enjoy working with. The platform and good usability are the blueprint of every screen. Leonardo is the handwriting of up in blue on top of it.

## Order of priority

Never change this order:

1. **Accessibility**
2. **Usability:** a design that looks better but works worse is wrong
3. **Platform conventions:** from the official guidelines, looked up, not from memory
4. **Leonardo:** the content layer of up in blue
5. **The calmer solution:** less color, less motion, fewer frames

If an element cannot name its purpose, remove it. If something is still unclear, ask.

## Workflow

### 1. Identify the mode

| The user wants to … | Mode | Read |
| --- | --- | --- |
| create a new screen, feature or app | Build | [references/modes/build.md](references/modes/build.md) |
| change, modernize or fix existing UI | Revise | [references/modes/revise.md](references/modes/revise.md) |
| evaluate UI and get recommendations | Review | [references/modes/review.md](references/modes/review.md) |

If the mode is unclear, ask. In review mode, do not change code unless asked.

### 2. Identify the platform

| Evidence in the repository | Platform | Read |
| --- | --- | --- |
| `.csproj` with `Microsoft.WindowsAppSDK` or `UseWinUI` | Windows, WinUI 3 | [references/platforms/windows.md](references/platforms/windows.md) |
| `.csproj` with `UseWPF` | Windows, WPF | [references/platforms/windows.md](references/platforms/windows.md) |
| Xcode project or `Package.swift` with SwiftUI, iOS destination | iOS | [references/platforms/ios.md](references/platforms/ios.md) |
| Xcode project or `Package.swift` with SwiftUI, macOS destination | macOS | [references/platforms/macos.md](references/platforms/macos.md) |
| `package.json` with `@microsoft/sp-*`, Teams app manifest | Web inside a Microsoft host | [references/platforms/web.md](references/platforms/web.md) |
| Any other web stack | Standalone web | [references/platforms/web.md](references/platforms/web.md) |
| A `.pptx` file or a request for slides | Presentations | [references/platforms/presentations.md](references/platforms/presentations.md) |

A multiplatform app is designed per platform. The Mac version gets Mac conventions, the iOS version gets iOS conventions. Shared code never means shared UI.

### 3. Research before you design

Before designing a new view, a new interaction pattern, or anything you are not certain about on this platform:

- Look up the relevant pages of the official guidelines listed in [references/sources.md](references/sources.md), using web search and web fetch. Official sources beat memory, blogs and forums.
- Read the pages for every component and pattern you plan to use, for example the guidance on toolbars before designing a macOS toolbar.
- Write down what you used: page title, URL and the date you read it. This goes into the pull request description or the review report.
- If web access is unavailable or denied, say so clearly, continue with your knowledge, and mark every platform decision as "unverified".

No research is needed to apply existing tokens or for small fixes in UI that already follows the guidelines.

### 4. Design

- Read [references/usability.md](references/usability.md) and the platform file.
- Read [references/design-language.md](references/design-language.md) for Leonardo. Its rules are marked MUST, SHOULD and NEVER.
- Use platform components first. Leonardo adds only the content layer: type roles, alignment line, grid, density, color semantics, status shapes, motion vocabulary.
- Take every value from tokens: [references/leonardo.tokens.json](references/leonardo.tokens.json) or the Leonardo resources in the repository. Never use literal colors, spacing, font sizes, radii, durations or easing curves.
- Every view answers three questions at a glance: Where am I? What can I do here? What just happened?
- Design all states, not only the happy path: empty, loading, error, offline, long and translated text, large text, dark mode, high contrast, reduced motion, keyboard only.

### 5. Verify and report

- If a "Leonardo check" message appears after an edit, it comes from the plugin's hook. Fix each finding in the lines you changed with the suggested token or resource. Leave findings in other lines alone unless the task is about them. Add a `leonardo-check: ignore <rule> <reason>` comment only for a real exception, and name it in the pull request.

- Go through the checklist of your mode.
- Where possible, run the app and look at the result in light and dark mode.
- Report the sources you used, every decision where platform and Leonardo met, and open questions.

## When to ask instead of deciding

- The platform guidelines and Leonardo conflict and the order of priority does not settle it.
- The task needs a pattern that neither the platform nor Leonardo covers.
- A change would alter workflows that existing users rely on.
- You do not know who the users are or what they are trying to achieve.

## Where Leonardo lives

- Rules: `references/design-language.md`, a copy of `docs/design-language.md` in github.com/upinblue/leonardo
- Values: `references/leonardo.tokens.json`
- Windows: the packages `UpInBlue.Leonardo.WinUI` and `UpInBlue.Leonardo.Wpf`
- iOS and macOS: the generated asset catalog and Swift values in `generated/apple` of upinblue/leonardo
- Web: the generated CSS custom properties in `generated/web` of upinblue/leonardo

If a repository does not contain Leonardo values yet, add them from upinblue/leonardo and note which version you used.
