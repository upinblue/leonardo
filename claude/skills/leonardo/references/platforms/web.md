# Web

There are two cases. First decide which one applies.

## Case 1: inside a Microsoft host (SharePoint, Teams, Outlook)

The host is the platform. Users are inside Microsoft 365 and expect it to behave like Microsoft 365.

- **Research first:** Fluent 2, Fluent UI React and the design guidance for SharePoint Framework or Teams (see `sources.md`).
- **Components:** Fluent UI React components. No custom versions of buttons, menus, dialogs or inputs.
- **Theme:** the host's theme, typeface and colors, including dark mode and high contrast as the host delivers them. Never override them.
- **Leonardo:** only the content layer: type roles mapped to the host's type ramp, the alignment line, the grid, density, status shapes and the motion vocabulary. Where a color is needed, take it from the host theme, not from Leonardo tokens. `leonardo.css` is not used inside a host.

## Case 2: standalone web apps and websites

The browser is the platform. Users expect links, URLs, the back button and their browser settings to work.

- **Research first:** MDN for HTML and CSS behavior, WCAG 2.2 for accessibility, the ARIA Authoring Practices Guide before building any custom widget.
- **Semantics:** real HTML elements first: `button`, `a`, `form`, `label`, `select`, `dialog`, `table`, headings in order, landmarks. ARIA only where no element exists, and then exactly as the Authoring Practices describe.
- **Navigation:** every view has its own URL; the back button works; links navigate, buttons act.
- **Input:** labels on every field; native validation messages or equivalent inline messages; never block paste.
- **Keyboard:** a visible focus indicator (`:focus-visible`), a logical tab order, a skip link on content-heavy pages. After a view changes, move focus to the new place name or announce the change.
- **User preferences:** respect `prefers-color-scheme`, `prefers-reduced-motion`, `prefers-contrast` and `forced-colors`. Never remove the focus outline without a better replacement.
- **Responsive:** relative units (`rem`) for type and layout sizes, so they follow the user's text size; `px` only for hairlines and focus outlines, which should not grow with text. Layouts reflow from phone to wide desktop, with no horizontal scrolling of the page.
- **Typeface:** Inter, titles in Inter Display, self-hosted. Do not load fonts from third-party servers; this keeps visitors' data with us.

## Leonardo on the standalone web

All values come from the generated `leonardo.css` (`generated/web` in upinblue/leonardo). Link it; never copy values out of it. If a deployment needs its own copy, copy the whole file unchanged as part of the build.

```html
<meta name="color-scheme" content="light dark">
<!-- Leonardo values, generated from tokens/leonardo.tokens.json. Never copy values out of this file. -->
<link rel="stylesheet" href="leonardo.css">
<link rel="stylesheet" href="fonts/inter.css">
<link rel="stylesheet" href="app.css">
```

### Custom properties

| Kind | Properties |
| --- | --- |
| Theme colors | `--leonardo-accent`, `--leonardo-on-accent`, `--leonardo-link`, `--leonardo-selection`, `--leonardo-surface`, `--leonardo-surface-alt`, `--leonardo-text-primary`, `--leonardo-text-secondary`, `--leonardo-text-tertiary`, `--leonardo-stroke-subtle`, `--leonardo-stroke-strong` |
| Accent states | `--leonardo-accent-hover`, `--leonardo-accent-pressed`: hover and pressed of accent fills, derived from the accent the way Fluent derives its accent fill states. Only the standalone web has them; other platforms draw these states themselves. |
| Status | `--leonardo-status-critical`, `--leonardo-status-caution`, `--leonardo-status-success`. They mirror the Windows status colors; only the standalone web (and presentations) use them. |
| Chart | `--leonardo-chart`: the one blue of a single-series chart |
| Focus | `--leonardo-focus` with `--leonardo-size-focus-stroke`: the focus outline, only where the browser's ring fails 3:1 |
| Blue scale | `--leonardo-palette-depth`, `-near`, `-brand`, `-far`, `-haze`, `-sky`, `-white` (charts with several series; text and surfaces use the theme colors, accent states use the accent state properties) |
| Spacing | `--leonardo-space-xs`, `-s`, `-m`, `-l`, `-xl` |
| Radii | `--leonardo-radius-s`, `-m`, `-l` |
| Density | `--leonardo-density-compact`, `-standard`, `-comfortable` (row heights) |
| Sizes | `--leonardo-size-icon` (icons in rows and next to labels), `--leonardo-size-status-dot`, `--leonardo-size-focus-stroke` |
| Type roles | `--leonardo-{place-name,view,section,body,meta}-{font-family,font-size,line-height,font-weight}`, and `--leonardo-meta-color` |
| Motion | `--leonardo-ease-enter`, `-exit`, `-move`, `-fade`; `--leonardo-duration-fade`, `-short`, `-normal`, `-long`; `--leonardo-stagger-step`, `--leonardo-stagger-max-groups`, `--leonardo-distance-enter-min`, `-max`, `--leonardo-arrive-hold`, `--leonardo-parallax-place-name-factor` |

Units: type roles are in `rem`, so they follow the user's text size; spacing, radii, densities, sizes and motion distances are in `px`, the unit of the tokens. Font stacks fall back to `sans-serif` where Inter is not available.

Dark mode is built in: `prefers-color-scheme: dark` switches the theme colors, and `data-theme="dark"` or `data-theme="light"` on `<html>` overrides the system choice. Reduced motion is built in too: under `prefers-reduced-motion: reduce`, `leonardo.css` turns every distance and the stagger step into 0, the parallax factor into 1, and every movement duration and curve into the fade duration and curve. Motion that uses only these properties follows the user's setting without extra rules.

Sizes Leonardo does not define (pane widths, breakpoints) become named custom properties of the app, defined from Leonardo values where possible, with a comment (from the web sample's `files.css`):

```css
:root {
    /* Layout dimensions of the three areas. Not Leonardo tokens. */
    --files-nav-width: 16rem;
    --files-details-width: 20rem;
    /* List density, chosen in Settings. Standard is the default. */
    --files-row-height: var(--leonardo-density-standard);
}
```

### Page margin and alignment line

The page margin is `--leonardo-space-l`. On pages with list rows that have icons or status dots, the alignment line lies at `--leonardo-space-xl` from the content edge: the place name, headings, column headers and the list text start there, icons and status dots hang to the left of it, and row backplates (hover, selection) may reach out to `--leonardo-space-xs` from the content edge. Pages without such lists use the page margin as their alignment line.

## Correct and incorrect

### Values

Correct (web sample, type roles and buttons):

```css
.role-place-name {
    font-family: var(--leonardo-place-name-font-family);
    font-size: var(--leonardo-place-name-font-size);
    line-height: var(--leonardo-place-name-line-height);
    font-weight: var(--leonardo-place-name-font-weight);
}

.button {
    padding: var(--leonardo-space-xs) var(--leonardo-space-m);
    border: 1px solid var(--leonardo-stroke-strong);
    border-radius: var(--leonardo-radius-s);
    background: var(--leonardo-surface);
    color: var(--leonardo-text-primary);
}
```

Incorrect:

```css
h1 { font-size: 40px; font-weight: 300; color: #0A1A33; }   /* px ignores the user's text size; literal color */
.button { padding: 5px 15px; border-radius: 6px; background: #1b6ce0; }
.list { gap: 12px; margin-block-end: 2rem; }                 /* spacing not from --leonardo-space-* */
```

`0` and 1px hairlines (also `margin: -1px` in the visually hidden pattern) are not design values; everything else in `margin`, `padding` and `gap` comes from `--leonardo-space-*`, alone or in `calc()`.

### Accent and selection

Correct: one accent fill for the one primary action of a view, with the Leonardo accent states.

```css
.button-primary { background: var(--leonardo-accent); color: var(--leonardo-on-accent); }
.button-primary:hover { background: var(--leonardo-accent-hover); }
.button-primary:active { background: var(--leonardo-accent-pressed); transition: none; }
```

Checked toggle buttons (`aria-pressed="true"`), such as the active filter phrases of a summary line, are selection, not actions. Several may be checked at once, and the rule of one accent surface does not apply to them. Incorrect: two accent-filled actions in one view; hover and pressed colors mixed by hand or taken from the blue scale.

### Focus

Keep the browser's focus ring. Where it has less than 3:1 contrast against its background (for example Edge's dark ring on the dark surface), replace it on `:focus-visible` with the focus tokens. This is the only place where Leonardo restyles a focus visual.

```css
:focus-visible {
    outline: var(--leonardo-size-focus-stroke) solid var(--leonardo-focus);
    outline-offset: var(--leonardo-space-xs);
}
```

In forced colors the browser draws the outline in system colors. Incorrect: `button:focus { outline: none; }` with nothing in its place, or an outline in the accent or a text color instead of `--leonardo-focus`.

### Motion

Correct (web sample, Dive in and Stagger, Leave): only Leonardo curves and durations, opacity faster than movement, exits shorter than entrances.

```css
.is-entering {
    animation:
        files-fade-in var(--leonardo-duration-fade) var(--leonardo-ease-fade) both,
        files-slide-in var(--leonardo-duration-normal) var(--leonardo-ease-enter) both;
    animation-delay: calc(var(--leonardo-stagger-step) * var(--files-group, 0));
}

.file-table tbody tr.is-leaving {
    animation:
        files-fade-out var(--leonardo-duration-short) var(--leonardo-ease-fade) both,
        files-slide-out var(--leonardo-duration-short) var(--leonardo-ease-exit) both;
}

/* Respond: the reaction starts on press, without a transition. */
.button:active { background: var(--leonardo-selection); transition: none; }
```

Entrances use `--leonardo-duration-normal` or `-long`; `--leonardo-duration-short` is for exits only. Stagger runs when the user navigates to a view (a hash change), never on the first load of the page: the first view appears without it.

Arrive: a new row gets a short accent bar at its start, fully visible for `--leonardo-arrive-hold`, then fading out. The dot shape is reserved for the status dot.

```css
.row.is-arriving::before {
    content: "";
    position: absolute;
    inset-block: var(--leonardo-space-xs);
    inset-inline-start: 0;
    inline-size: var(--leonardo-space-xs);
    border-radius: var(--leonardo-radius-s);
    background: var(--leonardo-accent);
    animation: arrive-fade var(--leonardo-duration-fade) var(--leonardo-ease-fade) var(--leonardo-arrive-hold) both;
}

@keyframes arrive-fade { from { opacity: 1; } to { opacity: 0; } }
```

The row itself needs `position: relative`, and the new item is announced in a polite live region.

A transition with a literal duration or curve needs its own `@media (prefers-reduced-motion: reduce)` rule; better, use the custom properties. Incorrect: `transition: transform 0.4s ease-in-out;`, `animation: bounce 1s infinite;`, a staggered entrance on page load, an entrance in `--leonardo-duration-short`.

### Charts

Correct: a single-series chart draws its series in `var(--leonardo-chart)`, and in `forced-colors: active` in `Highlight`. Incorrect: `var(--leonardo-accent)` or a status color for data that is not a status.

### Status

Correct (web sample): a shape plus text; only the dot carries the status color, from `--leonardo-status-*`, at `--leonardo-size-status-dot`.

```html
<span class="status-dot status-caution" aria-hidden="true"></span>
<span class="status-text">Checked out by Anna Berg</span>
```

Incorrect: a red row background, a colored word with no shape, or a dot with no text.

### Semantics

Correct: `<a href="#/files/hofmann-logistics/contracts">Contracts</a>` to navigate, `<button type="button">Upload files</button>` to act, a `<table>` for tabular file lists, nested lists of links with disclosure buttons (`aria-expanded`) for a folder navigation, following the APG disclosure navigation pattern.

Incorrect: `<div class="row" onclick="open(id)">`, `<span class="link" onclick="…">`, `role="tree"` for something that is a navigation, or a view that changes without changing the URL.

### Forced colors

Correct (web sample): system colors only, selection with `Highlight` and `HighlightText`, panes get a border, status dots keep their text.

```css
@media (forced-colors: active) {
    .details { border: 1px solid CanvasText; }
    .file-table tbody tr.is-current td { forced-color-adjust: none; background: Highlight; color: HighlightText; }
}
```

## Before the pull request (web)

- [ ] Keyboard-only walkthrough of the main task, with a visible focus at every step.
- [ ] Light and dark, `prefers-reduced-motion: reduce`, `forced-colors: active` (emulate them in the browser's developer tools).
- [ ] The focus ring has at least 3:1 contrast on every surface in both themes; where the browser's does not, the focus tokens replace it.
- [ ] Page margin `--leonardo-space-l`; on pages with icon lists, text shares the alignment line at `--leonardo-space-xl`.
- [ ] One accent fill for actions per view, with `--leonardo-accent-hover` and `-pressed`; no stagger on first load.
- [ ] Browser zoom at 200 % and a narrow window: nothing clips, no horizontal page scroll.
- [ ] Every view has its own URL; back and forward work.
- [ ] No literal colors, font sizes, spacing, radii, durations or curves outside `leonardo.css`.

## Never on the web

- Overriding the browser's context menu, scroll behavior or text selection without a strong reason.
- Clickable `div` elements instead of buttons or links.
- Placeholder text as the only label.
- Views without their own URL in apps where people share or bookmark views.
- Fonts from third-party servers.
