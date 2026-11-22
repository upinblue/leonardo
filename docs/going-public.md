# Going public

A checklist for making `upinblue/leonardo` public and publishing the packages on nuget.org. Bastian Wirth decides and
runs every step; nobody else switches the visibility or publishes. Work through the sections in order and tick each
item in the pull request or issue that tracks the release.

## 1. Legal review

- [ ] **`LICENSE`:** MIT for the code. The file adds a paragraph that excludes `brand/` and the names "up in blue" and
      "uib". Confirm the wording, and confirm that "MIT" stays the right SPDX expression for the packages
      (`PackageLicenseExpression` in `build/Packaging.props`). Tools such as GitHub's license detection may not
      recognize the file as MIT because of the added paragraph.
- [x] **Plugin license:** decided in stage 10. The Claude Code plugin in `claude/` is MIT like the code, including the
      copy of the design language in the skill; `brand/` is not part of the plugin and stays excluded.
- [x] **Contributions:** decided in stage 10. `CONTRIBUTING.md` stays "inbound = outbound": contributions are licensed
      under the MIT License of the repository, without a CLA or DCO.
- [ ] **`TRADEMARKS.md`:** confirm the terms for the name and the logos, including whether `brand/` stays in the public
      repository at all, and whether an official package may show the up in blue logo as its icon.
- [ ] **Photo credits in `docs/leonardo-design-language.pdf`:** page 25 credits three photos from Pixabay (Venice by
      han_2, Florence by marmax, Omegna by vco24). Confirm that the Pixabay license allows redistributing them inside
      a PDF in a public repository, and that the credits are complete.
- [ ] **Fonts:** if the Inter font files are added to `samples/web/fonts/`, include the SIL Open Font License text next
      to them.
- [ ] **Third-party content:** confirm that no other third-party images, texts or code are in the repository.

## 2. Remove anything internal

Everything in the repository and its history becomes visible: files, commits, branches, pull requests, issues and
their comments.

- [ ] **Files:** search for internal names, customers, people, product details, hosts and paths. Sample content must
      only use the invented companies and people listed in `CLAUDE.md`.
- [x] **Design language:** the statement about which fonts are installed on the computers at up in blue is gone.
      Since stage 10 the section "Typefaces" says neutrally that Inter is used where it is installed or shipped,
      otherwise the system font. Search once more for other internal IT details.
- [ ] **Working files for Claude Code:** decide whether `CLAUDE.md`, `.claude/settings.json` and `docs/roadmap.md` stay
      public as they are, are rewritten for outside contributors, or are removed.
- [ ] **`CODEOWNERS`:** confirm that the GitHub login in it may be public.
- [ ] **Screenshots:** they stay in the repository (decided in stage 10). Check every image in `docs/screenshots/`
      for real names, data or desktop details.
- [ ] **Secrets:** confirm that no token, key, password or connection string was ever committed, also in old commits
      and in closed branches.
- [ ] **Git history:** commit author names and e-mail addresses become public. Decide whether the history stays, or
      whether the public repository starts from a squashed or fresh history.
- [ ] **Pull requests and branches:** merge or close every open pull request; delete stage branches that are merged.
      Read the pull request descriptions and comments, which become public too.

## 3. Prepare the public repository

- [ ] Update the repository description; it still says "for WinUI 3 and WPF", while Leonardo covers Windows, iOS,
      macOS, the web and presentations. This is a repository setting; Bastian Wirth changes it.
- [ ] Update the status line in `README.md`.
- [ ] Add `SECURITY.md` with how to report vulnerabilities, and turn on private vulnerability reporting.
- [ ] Decide on a code of conduct and issue templates.
- [ ] Protect `main`: pull requests required, no force pushes.
- [ ] Plan to turn on secret scanning and Dependabot alerts right after the switch.

## 4. nuget.org

- [ ] Create a nuget.org organization account for up in blue, owned by at least two people, with two-factor
      authentication for every member.
- [ ] Reserve the ID prefix `UpInBlue.` for that account (nuget.org ID prefix reservation, requested by e-mail to the
      nuget.org team). Do this early: until the prefix is reserved, anyone can publish a package named `UpInBlue.*`
      on nuget.org. Apps that use the GitHub Packages feed are protected by package source mapping (see `README.md`).
- [ ] Package icon: none for now (decided in stage 10). The packages get an icon only after `TRADEMARKS.md` allows
      the up in blue logo for official packages; then add `PackageIcon` in `build/Packaging.props` from a copy that
      is not in `brand/`.
- [ ] Decide whether to sign the packages with an author certificate. nuget.org adds its repository signature anyway.
- [ ] Create an API key that can only push, is limited to the glob `UpInBlue.*`, and expires soon. Keep it in an
      environment variable of the session that pushes, like the GitHub token in [`publishing.md`](publishing.md).
- [ ] Check that the package READMEs render on nuget.org: relative links do not work there; images need absolute URLs
      from a trusted host.
- [ ] Push to `https://api.nuget.org/v3/index.json` with the same `dotnet nuget push` commands as in
      [`publishing.md`](publishing.md) for all three packages (`UpInBlue.Leonardo.WinUI`, `UpInBlue.Leonardo.Wpf`,
      `UpInBlue.Leonardo.Check`), using the nuget.org source and key. Pushed versions can be unlisted, but never
      deleted or replaced.

## 5. Switch the visibility

- [ ] All items above are done or consciously skipped, with the reason recorded.
- [ ] Switch the repository to public in the repository settings (Danger Zone, Change visibility).
- [ ] Decide for each GitHub package whether it becomes public. Changing a package's visibility is separate from the
      repository, and **a public package can never be made private again**.
- [ ] Turn on secret scanning and Dependabot alerts, and check the first results.
- [ ] Announce the release with a link to the changelog.
