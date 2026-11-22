# Leonardo web sample

The Files scenario of the Leonardo gallery as one standalone web page: plain HTML and CSS, a small script, no framework, no build step. Content is invented.

## Run it

Open `index.html` in a browser. Nothing else is needed; the page works from `file://` and from any static web server.

To serve it, serve the repository root, so that the relative link to `generated/web` resolves, and open `/samples/web/`.

## Files

| File | Purpose |
| --- | --- |
| `index.html` | The page: banner, folder navigation, main content, details, Settings |
| `files.css` | Layout and styles of the sample. Every design value comes from a `--leonardo-*` custom property |
| `files.js` | Sample data, hash routing, rendering, delete and undo, upload, settings |
| `fonts/inter.css` | `@font-face` rules for Inter and Inter Display, self-hosted |
| `fonts/README.md` | Which Inter release the font files come from, and how to update them |
| `fonts/*.woff2`, `fonts/LICENSE.txt` | Inter 4.1 and Inter Display 4.1 in the three weights the type roles use, with the SIL Open Font License |

## Leonardo values stay in sync

`index.html` links the generated stylesheet directly:

```html
<link rel="stylesheet" href="../../generated/web/leonardo.css">
```

There is no copy to keep up to date. When the tokens change, run the generator (`dotnet run --project src/UpInBlue.Leonardo.Tokens -- generate`) and reload the page.

If the sample is ever deployed on its own, copy `generated/web/leonardo.css` next to `index.html` as part of that deployment and change the link. Never edit the copy.

## Views and URLs

Every view has its own URL, so the back button, bookmarks and shared links work:

| URL | View |
| --- | --- |
| `#/files` | Customers: all customer folders |
| `#/files/hofmann-logistics/contracts` | A folder |
| `#/files/hofmann-logistics/contracts?file=price-list-2027-xlsx` | A folder with the details of one file |
| `#/settings` | Settings: appearance and list density |

Unknown addresses show "Page not found" or "Folder not found"; a file that is no longer there shows "File not found" in the details.

## Behavior

- **Semantics:** landmarks (`header`, `nav` "App", `nav` "Folders", `nav` "Breadcrumb", `main`, `aside` "Details"), a real `table` for the list, links to navigate, buttons to act, native radio buttons in Settings.
- **Folder navigation:** nested lists of links with disclosure buttons, following the APG disclosure navigation pattern. It is not an ARIA tree: Tab moves through it, Enter or Space opens and closes a folder.
- **Keyboard:** a skip link, a visible focus indicator, a tab order that follows reading order. After a view changes, focus moves to the place name; when it stays in a navigation, a polite live region announces the new place.
- **Focus indicator:** the browser's own ring where it reaches 3:1. Edge and Chrome draw it in #101010 in both color schemes, which passes on the light surfaces and fails on the dark ones, so the dark theme replaces it with `--leonardo-focus`, `--leonardo-size-focus-stroke` wide. Forced colors keep the browser's ring.
- **Layout:** page margin `--leonardo-space-l`. The folder view puts its alignment line at `--leonardo-space-xl`: breadcrumb, place name, summary, column headers and file names start there, status dots (`--leonardo-size-status-dot`) hang to the left of it, and row backplates reach to `--leonardo-space-xs` from the content edge. Settings and "not found" use the page margin as their alignment line.
- **Feedback:** delete and upload report what happened next to the place name, with Undo. The message is a `role="status"` live region.
- **Motion:** navigating to another folder or view staggers its groups (breadcrumb, place name, summary, content); the first view after loading appears without motion. Entrances use `--leonardo-duration-normal`, `--leonardo-duration-short` is used only for exits (a leaving row, a collapsing chevron). Restored and uploaded rows arrive with a short accent bar at the start of the row, which fades out after `--leonardo-arrive-hold`. Accent buttons use `--leonardo-accent-hover` and `--leonardo-accent-pressed`.
- **Place name:** while the page scrolls, it moves slower than the content (`--leonardo-parallax-place-name-factor`) and shrinks toward the size of the view role. It moves down only as far as it has shrunk, so it never overlaps the breadcrumb, the summary or the commands.
- **Theme:** follows `prefers-color-scheme`; Settings can choose light or dark, which sets `data-theme` on `<html>`.
- **Reduced motion:** `leonardo.css` turns every duration into the short crossfade and every distance into 0. The sample uses only those custom properties, so its motion follows. The chevron changes state without turning, and the place name neither moves nor shrinks.
- **Forced colors:** system colors only; selection uses `Highlight` and `HighlightText`, panes get a border, status dots keep their text, the Arrive bar uses `Highlight` (`HighlightText` in the selected row).
- **Icons:** the sample shows no file or folder icons; only the folder chevrons, at `--leonardo-size-icon`. Which icon family the standalone web uses is still open.
- **Storage:** appearance and density are stored in `localStorage` of this browser. Nothing else is stored, and nothing leaves the browser; "uploaded" files exist in the page until it is reloaded.
