# UpInBlue.Leonardo.Check

`leonardo-check` finds literal design values and common accessibility mistakes in the UI code of up in blue apps. It
follows Leonardo, the design language of up in blue: colors, font sizes, spacing, radii, durations and easing curves
come from Leonardo tokens and resources, glass only where the platform provides it, focus visuals stay, motion
respects the system setting, and interactive elements are real controls.

It checks `.xaml`, `.cs`, `.swift`, `.css`, `.scss`, `.html` and `.tsx` files and skips generated files, build output,
hidden folders and test code. The Claude Code plugin `leonardo` runs it as a hook after every edit of a UI file.

## Install

The package is published to GitHub Packages of the organization `upinblue`. Reading it needs a GitHub personal access
token (classic) with the scope `read:packages` in the environment variable `GITHUB_PACKAGES_TOKEN`, and the source
`upinblue` in a `nuget.config` (see the README of github.com/upinblue/leonardo).

```
dotnet tool install --global UpInBlue.Leonardo.Check --add-source https://nuget.pkg.github.com/upinblue/index.json
```

The tool needs the .NET 10 runtime or later.

## Use

```
leonardo-check src samples        # check folders; exit code 1 if there are findings
leonardo-check Pages/Files.xaml   # check one file
leonardo-check --rules            # list the rules
leonardo-check --hook             # Claude Code PostToolUse hook: the event JSON on standard input
```

Every finding names the file, the line, the rule and a suggestion:

```
Pages/FilesPage.xaml:317: LEO001 literal-color: Background="#F3F7FC" is a literal color.
    Suggestion: Use a Leonardo color resource such as LeonardoSurfaceBrush or LeonardoSurfaceAltBrush, ...
```

A justified exception is a comment `leonardo-check: ignore LEO001 <reason>` on the line or the line above. The reason
is required.

## License

MIT. The names "up in blue" and "uib" and the up in blue logos are not covered by the license.
