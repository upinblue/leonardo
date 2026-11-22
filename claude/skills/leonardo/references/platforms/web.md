# Web

There are two cases. First decide which one applies.

## Case 1: inside a Microsoft host (SharePoint, Teams, Outlook)

The host is the platform. Users are inside Microsoft 365 and expect it to behave like Microsoft 365.

- **Research first:** Fluent 2, Fluent UI React and the design guidance for SharePoint Framework or Teams (see `sources.md`).
- **Components:** Fluent UI React components. No custom versions of buttons, menus, dialogs or inputs.
- **Theme:** the host's theme, typeface and colors, including dark mode and high contrast as the host delivers them. Never override them.
- **Leonardo:** only the content layer: type roles mapped to the host's type ramp, the alignment line, the grid, density, status shapes and the motion vocabulary. Where a color is needed, take it from the host theme, not from Leonardo tokens.

## Case 2: standalone web apps and websites

The browser is the platform. Users expect links, URLs, the back button and their browser settings to work.

- **Research first:** MDN for HTML and CSS behavior, WCAG 2.2 for accessibility, the ARIA Authoring Practices Guide before building any custom widget.
- **Semantics:** real HTML elements first: `button`, `a`, `form`, `label`, `select`, `dialog`, headings in order, landmarks. ARIA only where no element exists, and then exactly as the Authoring Practices describe.
- **Navigation:** every view has its own URL; the back button works; links navigate, buttons act.
- **Input:** labels on every field; native validation messages or equivalent inline messages; never block paste.
- **Keyboard:** a visible focus indicator (`:focus-visible`), a logical tab order, a skip link on content-heavy pages.
- **User preferences:** respect `prefers-color-scheme`, `prefers-reduced-motion`, `prefers-contrast` and `forced-colors`. Never remove the focus outline without a better replacement.
- **Responsive:** relative units (`rem`), layouts that reflow from phone to wide desktop, no horizontal scrolling of the page.
- **Typeface:** Inter, titles in Inter Display, self-hosted. Do not load fonts from third-party servers; this keeps visitors' data with us.
- **Leonardo:** colors, spacing, radii, type roles and motion from the generated `leonardo.css` custom properties in `generated/web`. Status colors from the `--leonardo-status-*` properties.
- **Motion:** CSS transitions with the Leonardo curves and durations; with `prefers-reduced-motion: reduce`, crossfade only.

## Never on the web

- Overriding the browser's context menu, scroll behavior or text selection without a strong reason.
- Clickable `div` elements instead of buttons or links.
- Placeholder text as the only label.
- Views without their own URL in apps where people share or bookmark views.
