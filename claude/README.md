# Leonardo plugin for Claude Code

The plugin `leonardo` gives Claude Code the skill `leonardo`. With it, Claude Code designs, revises and reviews the UI of up in blue apps: it identifies the platform, reads the platform's official guidelines before designing, applies usability first and then the Leonardo design language, and names the sources it used.

| Mode | Example request |
| --- | --- |
| Build | "Add a page that lists the open approvals." |
| Revise | "This settings view looks dated. Bring it in line with Leonardo." |
| Review | "Review the customer form for usability and accessibility." |

Platforms: Windows (WinUI 3, WPF), iOS and macOS (SwiftUI), the web (standalone, SharePoint, Teams) and PowerPoint.

Claude Code uses the skill on its own when a task is about UI. You can also call it directly with `/leonardo:leonardo`.

## Contents

```
claude/
  .claude-plugin/plugin.json     The plugin manifest
  skills/leonardo/SKILL.md       The skill: workflow, order of priority, when to ask
  skills/leonardo/references/    Modes, platforms, usability, official sources, and copies of the
                                 design language and the tokens
  hooks/hooks.json               PostToolUse hook: runs the Leonardo check after Claude edits UI code
  hooks/leonardo-check.sh        Finds and runs the check tool
  settings-snippet.json          Web access to the official design guidance, for a repository's settings
```

The marketplace that lists the plugin is `.claude-plugin/marketplace.json` at the root of upinblue/leonardo; it is named `upinblue`.

`skills/leonardo/references/design-language.md` and `skills/leonardo/references/leonardo.tokens.json` are copies of `docs/design-language.md` and `tokens/leonardo.tokens.json`. The token generator refreshes them (`dotnet run --project src/UpInBlue.Leonardo.Tokens -- generate`); never edit them by hand.

## Install

### For yourself

In Claude Code:

```
/plugin marketplace add upinblue/leonardo
/plugin install leonardo@upinblue
```

Or from a terminal:

```
claude plugin marketplace add upinblue/leonardo
claude plugin install leonardo@upinblue --scope user
```

Updates: third-party marketplaces do not update on their own by default. Run `claude plugin update leonardo@upinblue`, or turn on auto-update for the marketplace `upinblue` in `/plugin` under Marketplaces. A new plugin version arrives when `version` in `.claude-plugin/plugin.json` changes.

### For a team repository

Commit this to the repository's `.claude/settings.json`. It registers the marketplace for everyone who trusts the folder and turns the plugin on for the repository. The `permissions` block is `settings-snippet.json`: it lets the skill read the official guidance (Microsoft Learn, Fluent 2, the Fluent UI React docs and storybooks, Apple Developer, W3C, MDN, Nielsen Norman Group) without a prompt for every page.

```json
{
  "extraKnownMarketplaces": {
    "upinblue": {
      "source": { "source": "github", "repo": "upinblue/leonardo" }
    }
  },
  "enabledPlugins": {
    "leonardo@upinblue": true
  },
  "permissions": {
    "allow": [
      "WebSearch",
      "WebFetch(domain:learn.microsoft.com)",
      "…the rest of settings-snippet.json"
    ]
  }
}
```

If the repository already has a `.claude/settings.json`, merge the keys; append the entries of `settings-snippet.json` to its `permissions.allow` list.

Enabling a plugin in project settings does not download it. Each person runs once per clone:

```
claude plugin install leonardo@upinblue --scope project
```

Until then, `/plugin` shows the plugin under Errors as not installed.

### Try it without installing

From a clone of upinblue/leonardo:

```
claude --plugin-dir ./claude
```

This loads the plugin for one session. Check it with `claude plugin validate ./claude` and, for the marketplace, `claude plugin validate .`.

## What to expect

- Before designing, the skill reads the platform's official pages and lists them with URL and date. Without web access it says so and marks platform decisions as unverified.
- Values come from the Leonardo resources of the platform: `UpInBlue.Leonardo.WinUI` and `UpInBlue.Leonardo.Wpf` on Windows, `generated/apple` on iOS and macOS, `generated/web/leonardo.css` on the web. If a repository has none yet, the skill adds them from upinblue/leonardo and notes the version.
- Reviews are written as Markdown reports in the format of `skills/leonardo/references/modes/review.md`, by default to `docs/reviews/`.
- When the platform and Leonardo conflict and the order of priority does not settle it, the skill asks instead of deciding.

## Check tool and hook

The plugin has a `PostToolUse` hook. After Claude Code writes or edits a `.xaml`, `.cs`, `.swift`, `.css`, `.scss`, `.html` or `.tsx` file, the hook runs the check tool `leonardo-check` on it. Findings in the lines Claude just changed go back to Claude, which then fixes them:

```
Leonardo check: 1 problem in Pages/FilesPage.xaml where you just changed it.
Pages/FilesPage.xaml:317: LEO001 literal-color: Background="#F3F7FC" is a literal color.
    Suggestion: Use a Leonardo color resource such as LeonardoSurfaceBrush or LeonardoSurfaceAltBrush, ...
```

The rules (`leonardo-check --rules`): literal colors, font sizes, corner radii, durations, easing curves and spacing (in Swift `.padding`, `spacing:` and `EdgeInsets`; in CSS and SCSS `margin`, `padding` and `gap`; `0` and 1px hairlines are allowed); custom glass; removed or restyled focus visuals; bouncing springs; transitions without a reduced-motion rule; clickable `div` elements. Generated files, build output and test code (folders such as `tests` or `*Tests`) are skipped. A justified exception is a comment `leonardo-check: ignore LEO001 <reason>` on the line or the line above; the reason is required.

### Install the tool

The hook needs the .NET 10 runtime or later and the tool. It looks for, in this order:

1. the environment variable `LEONARDO_CHECK`, set to `UpInBlue.Leonardo.Check.dll` or an executable;
2. `leonardo-check` on the `PATH`, installed as a .NET tool;
3. in a clone of upinblue/leonardo, the tool built from `src/UpInBlue.Leonardo.Check` (build the solution once).

Without the tool, the hook prints one line, "Leonardo check skipped: leonardo-check is not on the PATH …", and lets Claude Code continue; it never blocks an edit.

The tool is the .NET tool package `UpInBlue.Leonardo.Check` on nuget.org:

```
dotnet tool install --global UpInBlue.Leonardo.Check
```

Update with `dotnet tool update --global UpInBlue.Leonardo.Check`. To use a build from a clone instead:

```
dotnet pack src/UpInBlue.Leonardo.Check -c Release
dotnet tool install --global --add-source artifacts/packages UpInBlue.Leonardo.Check
```

### Requirements on Windows

The hook is a Bash script. Claude Code on Windows runs hooks with Git Bash, so the hook assumes Git for Windows is installed (Claude Code itself needs it on Windows). The script uses only `bash`, `grep` and `dotnet`.

### Run it yourself

```
leonardo-check src samples        # check folders; exit code 1 if there are findings
leonardo-check Pages/Files.xaml   # check one file
```

`/hooks` in Claude Code shows the hook and where it comes from. To turn all hooks off, set `disableAllHooks` in your settings.

## License

The plugin is MIT licensed, like the code of upinblue/leonardo (`license` in `.claude-plugin/plugin.json`), including the copy of the design language in the skill. The names "up in blue" and "uib" and the logos in `brand/` are not covered by it, and nothing from `brand/` is part of the plugin; see `TRADEMARKS.md` in the repository.
