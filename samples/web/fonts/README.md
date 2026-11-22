# Fonts

The web sample uses Inter for text and Inter Display for titles, self-hosted from this folder. `inter.css` declares them.

## Missing files

The font files are not committed yet. They could not be added in stage 6 because Inter was not installed on the machine that built the sample, and nothing was downloaded without approval.

Add these files from the official Inter release (Inter 4.x, https://rsms.me/inter/, SIL Open Font License 1.1):

| File | Family | Weight | Used by |
| --- | --- | --- | --- |
| `Inter-Regular.woff2` | Inter | 400 | body, meta |
| `Inter-SemiBold.woff2` | Inter | 600 | section |
| `InterDisplay-Light.woff2` | Inter Display | 300 | place name, view |
| `LICENSE.txt` | – | – | the SIL Open Font License text from the release |

The release archive contains web fonts under these or similar names; this was not checked, because nothing was downloaded. Keep the file names above, or update the `url()` entries in `inter.css`.

## Until then

`inter.css` lists `local()` sources first. On a machine with Inter installed (as on all up in blue PCs and Macs), the browser uses the installed fonts. Elsewhere it falls back to the generic `sans-serif` of `leonardo.css`, which is Arial on Windows. The layout does not depend on the typeface.
