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
  settings-snippet.json          Web access to the official design guidance, for a repository's settings
```

The marketplace that lists the plugin is `.claude-plugin/marketplace.json` at the root of upinblue/leonardo; it is named `upinblue`.

`skills/leonardo/references/design-language.md` and `skills/leonardo/references/leonardo.tokens.json` are copies of `docs/design-language.md` and `tokens/leonardo.tokens.json`. The token generator refreshes them (`dotnet run --project src/UpInBlue.Leonardo.Tokens -- generate`); never edit them by hand.

## Install

The repository is private. Claude Code clones it with the Git credentials on your machine and never prompts for them, so make sure Git can read upinblue/leonardo first, for example with `gh auth login`.

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

Commit this to the repository's `.claude/settings.json`. It registers the marketplace for everyone who trusts the folder and turns the plugin on for the repository. The `permissions` block is `settings-snippet.json`: it lets the skill read the official guidance (Microsoft Learn, Fluent 2, Apple Developer, W3C, MDN, Nielsen Norman Group) without a prompt for every page.

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
