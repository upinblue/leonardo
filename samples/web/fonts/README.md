# Fonts

The web sample uses Inter for text and Inter Display for titles, self-hosted from this folder. `inter.css` declares them. No font is loaded from a third-party server.

## Source

| | |
| --- | --- |
| Release | Inter 4.1 (tag `v4.1`, published 2024-11-16) |
| Archive | https://github.com/rsms/inter/releases/download/v4.1/Inter-4.1.zip |
| Release page | https://github.com/rsms/inter/releases/tag/v4.1 |
| License | SIL Open Font License 1.1, see `LICENSE.txt` |

The files are copied unchanged from the archive. The web fonts come from its `web/` folder, the license from its root.

| File | Path in the archive | Family | Weight | Used by |
| --- | --- | --- | --- | --- |
| `Inter-Regular.woff2` | `web/Inter-Regular.woff2` | Inter | 400 | body, meta |
| `Inter-SemiBold.woff2` | `web/Inter-SemiBold.woff2` | Inter | 600 | section |
| `InterDisplay-Light.woff2` | `web/InterDisplay-Light.woff2` | Inter Display | 300 | place name, view |
| `LICENSE.txt` | `LICENSE.txt` | – | – | the license of the fonts |

Only the three weights the type roles use are shipped. If a role ever needs another weight, take it from the same release, add an `@font-face` rule to `inter.css` and a row to the table above.

## Updating

Download the archive of a newer stable release from https://github.com/rsms/inter/releases, copy the same files from its `web/` folder and its `LICENSE.txt`, and update the release and archive rows above. Check that the file names in `inter.css` still match.

## Fallback

The fonts are always loaded from this folder, never from an installed copy, so every machine renders the same release. If a file cannot be loaded, the browser falls back to the generic `sans-serif` of `leonardo.css`. The layout does not depend on the typeface.
