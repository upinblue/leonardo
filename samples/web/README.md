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
| `fonts/README.md` | Which font files belong in `fonts/` and why they are missing |

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
- **Keyboard:** a skip link, a visible focus outline on `:focus-visible`, a tab order that follows reading order. After a view changes, focus moves to the place name; when it stays in a navigation, a polite live region announces the new place.
- **Feedback:** delete and upload report what happened next to the place name, with Undo. The message is a `role="status"` live region.
- **Theme:** follows `prefers-color-scheme`; Settings can choose light or dark, which sets `data-theme` on `<html>`.
- **Reduced motion:** `leonardo.css` turns every duration into the short crossfade and every distance into 0. The sample uses only those custom properties, so its motion follows.
- **Forced colors:** system colors only; selection uses `Highlight` and `HighlightText`, panes get a border, status dots keep their text.
- **Storage:** appearance and density are stored in `localStorage` of this browser. Nothing else is stored, and nothing leaves the browser; "uploaded" files exist in the page until it is reloaded.
