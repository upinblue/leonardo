# Publishing the NuGet packages

This guide describes how `UpInBlue.Leonardo.WinUI`, `UpInBlue.Leonardo.Wpf` and the .NET tool
`UpInBlue.Leonardo.Check` are packed and published to nuget.org. Publishing runs in
[`.github/workflows/release.yml`](../.github/workflows/release.yml) with nuget.org Trusted Publishing: there is no
API key. Bastian Wirth starts every release by pushing a tag and approving the run.

## What gets published

| Package | Contents |
| --- | --- |
| `UpInBlue.Leonardo.WinUI` | `lib/net10.0-windows10.0.26100/`: the assembly, its XML documentation, `UpInBlue.Leonardo.WinUI.pri`, and the XAML resources under `UpInBlue.Leonardo.WinUI/Themes/`, so apps can merge `ms-appx:///UpInBlue.Leonardo.WinUI/Themes/Leonardo.xaml`. `README.md`. |
| `UpInBlue.Leonardo.Wpf` | `lib/net10.0-windows7.0/`: the assembly (the XAML dictionaries are compiled into it) and its XML documentation. `README.md`. |
| `UpInBlue.Leonardo.Check` | A .NET tool (package type `DotnetTool`, command `leonardo-check`): `tools/net10.0/any/` with the assembly, its XML documentation, `DotnetToolSettings.xml` and the runtime configuration (`rollForward` `Major`). `README.md`. |

The metadata (version, authors "up in blue", MIT license expression, repository URL, description, tags) is in
[`build/Packaging.props`](../build/Packaging.props), which the two library projects and the check tool import. The
package READMEs are `src/UpInBlue.Leonardo.WinUI/README.md`, `src/UpInBlue.Leonardo.Wpf/README.md` and
`src/UpInBlue.Leonardo.Check/README.md`. No file from `brand/` is packed, and the packages have no icon until
`TRADEMARKS.md` allows one (see [`going-public.md`](going-public.md)).

## 1. Prepare the release

1. Work on an up-to-date `main` with a clean working tree. Every stage that belongs into the release is merged.
2. Check the version in `build/Packaging.props` (`VersionPrefix`). Versions follow semantic versioning; a version that
   was pushed once can never be pushed again, so a fix always gets a new version.
3. In `CHANGELOG.md`, replace `Unreleased` next to the version with today's date (`YYYY-MM-DD`). Merge that change
   through a pull request like any other.

## 2. Build, test and pack

```powershell
dotnet test --solution UpInBlue.Leonardo.slnx
```

```powershell
dotnet run --project src/UpInBlue.Leonardo.Tokens -- check
```

```powershell
Remove-Item artifacts/packages -Recurse -Force -ErrorAction SilentlyContinue
```

```powershell
dotnet pack UpInBlue.Leonardo.slnx -c Release
```

The packages are written to `artifacts/packages/` (ignored by git). Only the two libraries and the check tool are
packed: every project is non-packable by default (`Directory.Build.props`), and `build/Packaging.props` turns packing
on for the projects that import it. To pack only the check tool:

```powershell
dotnet pack src/UpInBlue.Leonardo.Check -c Release
```

## 3. Inspect the packages

List the contents of each package and compare them with the table above:

```powershell
Get-ChildItem artifacts/packages/*.nupkg | ForEach-Object { "== $($_.Name)"; tar -tf $_.FullName }
```

Check in particular:

- the version in the file names is the one in the changelog;
- the WinUI package contains `UpInBlue.Leonardo.WinUI.pri` and the `.xaml` files under
  `lib/net10.0-windows10.0.26100/UpInBlue.Leonardo.WinUI/Themes/`;
- both libraries contain `README.md` and the `.xml` documentation;
- the check tool contains `README.md`, `tools/net10.0/any/UpInBlue.Leonardo.Check.dll` and
  `tools/net10.0/any/DotnetToolSettings.xml` with the command `leonardo-check`;
- no package contains anything from `brand/`, no samples, and no tests.

Try the check tool from the local package before pushing it, in a folder outside the repository:

```powershell
dotnet tool install --tool-path "$env:TEMP\leonardo-check" --add-source artifacts/packages UpInBlue.Leonardo.Check
```

```powershell
& "$env:TEMP\leonardo-check\leonardo-check.exe" --rules
```

## 4. Publish with the release workflow

One-time setup:

1. On nuget.org, open **Trusted Publishing** for the account `uib` and create a policy: repository owner `upinblue`,
   repository `leonardo`, workflow file `release.yml`, environment `nuget`.
2. In the repository settings, create the environment `nuget` with a required reviewer (Bastian Wirth) and the
   variable `NUGET_USER` set to the nuget.org user name (the profile name, not an e-mail address).

For every release, after step 1 is merged:

```powershell
git tag -a v0.1.0 -m "Leonardo 0.1.0"
```

```powershell
git push origin v0.1.0
```

The workflow runs tests and the token check, packs, waits for the approval in the environment `nuget`, and pushes all
three packages. It refuses a tag that does not match `VersionPrefix`. Pushed versions can be unlisted but never
deleted or replaced.

## 5. After publishing

1. Open the three package pages on nuget.org and check the README rendering and the metadata.
2. Optionally, create a GitHub release for the tag with the changelog section as notes.
3. The packages in GitHub Packages of `upinblue` stay private. **Making a package public cannot be undone.**

## Using the packages

```powershell
dotnet add package UpInBlue.Leonardo.WinUI
dotnet add package UpInBlue.Leonardo.Wpf
dotnet tool install --global UpInBlue.Leonardo.Check
```
