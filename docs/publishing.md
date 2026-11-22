# Publishing the NuGet packages

This guide packs `UpInBlue.Leonardo.WinUI` and `UpInBlue.Leonardo.Wpf` and publishes them to GitHub Packages of the
organization `upinblue`. Nothing in this repository publishes automatically. Bastian Wirth runs these steps by hand.

Publishing to nuget.org is not covered here; it comes after the repository is public, see
[`going-public.md`](going-public.md).

## What gets published

| Package | Contents |
| --- | --- |
| `UpInBlue.Leonardo.WinUI` | `lib/net10.0-windows10.0.26100/`: the assembly, its XML documentation, `UpInBlue.Leonardo.WinUI.pri`, and the XAML resources under `UpInBlue.Leonardo.WinUI/Themes/`, so apps can merge `ms-appx:///UpInBlue.Leonardo.WinUI/Themes/Leonardo.xaml`. `README.md`. |
| `UpInBlue.Leonardo.Wpf` | `lib/net10.0-windows7.0/`: the assembly (the XAML dictionaries are compiled into it) and its XML documentation. `README.md`. |

The metadata (version, authors "up in blue", MIT license expression, repository URL, description, tags) is in
[`build/Packaging.props`](../build/Packaging.props), which both library projects import. The package READMEs are
`src/UpInBlue.Leonardo.WinUI/README.md` and `src/UpInBlue.Leonardo.Wpf/README.md`. No file from `brand/` is packed,
and the packages have no icon.

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

The packages are written to `artifacts/packages/` (ignored by git). Only the two libraries are packed: every project
is non-packable by default (`Directory.Build.props`), and `build/Packaging.props` turns packing on for the libraries.

## 3. Inspect the packages

List the contents of each package and compare them with the table above:

```powershell
Get-ChildItem artifacts/packages/*.nupkg | ForEach-Object { "== $($_.Name)"; tar -tf $_.FullName }
```

Check in particular:

- the version in the file names is the one in the changelog;
- the WinUI package contains `UpInBlue.Leonardo.WinUI.pri` and the `.xaml` files under
  `lib/net10.0-windows10.0.26100/UpInBlue.Leonardo.WinUI/Themes/`;
- both packages contain `README.md` and the `.xml` documentation;
- no package contains anything from `brand/`, no samples, and no tests.

## 4. Push to GitHub Packages

GitHub Packages accepts only a personal access token (classic) for NuGet. Create one at
<https://github.com/settings/tokens> with the scopes `write:packages` and `read:packages`, with a short expiry, and
authorize it for the organization `upinblue` if the organization requires single sign-on.

Never write the token into a file, a script or a command. Put it into an environment variable of the current
PowerShell 7 session only; `-MaskInput` keeps it off the screen and out of the history:

```powershell
$env:GITHUB_PACKAGES_TOKEN = Read-Host -MaskInput "GitHub token"
```

Push both packages:

```powershell
dotnet nuget push artifacts/packages/UpInBlue.Leonardo.WinUI.0.1.0.nupkg --source https://nuget.pkg.github.com/upinblue/index.json --api-key $env:GITHUB_PACKAGES_TOKEN
```

```powershell
dotnet nuget push artifacts/packages/UpInBlue.Leonardo.Wpf.0.1.0.nupkg --source https://nuget.pkg.github.com/upinblue/index.json --api-key $env:GITHUB_PACKAGES_TOKEN
```

Then remove the token from the session:

```powershell
Remove-Item Env:GITHUB_PACKAGES_TOKEN
```

## 5. After publishing

1. Open <https://github.com/orgs/upinblue/packages>. Both packages are listed and linked to the repository
   `upinblue/leonardo` (from `RepositoryUrl` in `build/Packaging.props`).
2. New packages are private and inherit the access permissions of the linked repository. Leave them private.
   **Making a package public cannot be undone**; that is part of [going public](going-public.md).
3. Tag the release on the commit that was packed and push the tag:

   ```powershell
   git tag -a v0.1.0 -m "Leonardo 0.1.0"
   ```

   ```powershell
   git push origin v0.1.0
   ```

4. Optionally, create a GitHub release for the tag with the changelog section as notes.

## Using the packages

Apps restore the packages with a `nuget.config` that maps `UpInBlue.*` to the up in blue feed and reads a token with
the scope `read:packages` from the environment variable `GITHUB_PACKAGES_TOKEN`. The README shows the file.
