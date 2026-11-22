# Leonardo – the design language of up in blue

This document is the single source of truth for how up in blue products look and behave: Windows apps (WinUI 3, WPF), iOS and macOS apps (SwiftUI), web interfaces, presentations and documents. All concrete values live in `tokens/leonardo.tokens.json`; this document references them by name, for example `space.m` or `motion.easing.enter`.

## Rule levels and decision rules

Every rule carries one of three levels. People and Claude Code read them the same way:

- **MUST:** applies without exception.
- **SHOULD:** applies in the normal case. An exception needs a reason that is recorded in the code or in the review.
- **NEVER:** is forbidden.

If a case is not clearly covered, decide in this order:

1. Accessibility comes before everything.
2. Usability comes before aesthetics. A design that looks better but works worse is wrong.
3. If the platform has a paradigm, the platform wins. Look it up in the platform's official guidelines; do not rely on memory.
4. If it has none, Leonardo applies.
5. If Leonardo does not cover the case either, the calmer solution wins: less color, less motion, fewer frames.
6. If an element cannot name its purpose, it is removed.
7. If it is still unclear, ask instead of inventing.

Leonardo is the handwriting of up in blue, not the blueprint. The blueprint of every screen comes from the platform and from good usability.

## Attitude

Leonardo is the design language of up in blue: the user's work is the content, the app is the tool, and on every platform we behave like a good guest.

Three sources shape it:

- **up in blue.** The name stands for looking up and ahead, with strong, even steps. These steps show in the strong strokes of the logo.
- **Leonardo da Vinci.** Artist and engineer at once. He explained why the sky is blue and derived aerial perspective from it: what is far away becomes bluer and paler. This is the basis of the blue scale.
- **Liberal arts and technology.** Design and technology belong together, as the Bauhaus taught. We build software people enjoy using, not just software that works.

Seven principles carry everything else:

1. **A guest on every platform.** The platform owns the shell. Where Windows, iOS, macOS or the web have a paradigm, the platform wins. A Mac app is a Mac app, not a large iOS app; a Windows app is a Windows app.
2. **Authentically digital.** Nothing imitates paper, leather or binders.
3. **Content before chrome.** Files, metadata and status get the space. Frames and cards must justify their place.
4. **Form follows function.** Whatever has no purpose is removed, even if it is pretty.
5. **Honest materials.** Glass only where there is something to see behind it and the platform provides it.
6. **Motion explains.** Every animation answers where something comes from or where it goes.
7. **Calm is a feature.** Business users spend whole working days in the app. The logo may stand out precisely because everything else is calm.

Review motto: *Fewer frames, more grid. Less effect, more meaning.*

## Brand

The logo is the only typographically special element of up in blue; everything around it stays calm.

### Logo and short form

- **MUST:** The brand name is always written in lowercase: up in blue. This also applies to visible addresses such as github.com/upinblue. The only exception are identifiers in code, which follow the conventions of their language, for example the .NET namespace `UpInBlue.Leonardo`.
- **MUST:** The wordmark "up in blue" in its frame is the standard logo for websites, documents, presentations and print.
- **MUST:** Below the minimum size of the wordmark (120 px wide on screen, 25 mm in print) the short form "uib" is used. Profile pictures and favicons always show the short form.
- **MUST:** A clear space of at least twice the stroke width (about 6 % of the logo width) stays free around the logo.
- **MUST:** Only these combinations are allowed: brand on white or sky, white on depth or brand, depth on light surfaces.
- **MUST:** Only the files in `brand/` are used.
- **NEVER:** rebuild, retype, distort, rotate or recolor the logo, add effects, remove the frame of the wordmark, or place the logo on busy photos.

Construction, in case letters must be added: the letters are drawn, not typeset. Vertical strokes are 14 units wide, horizontal strokes 7. Where a stroke turns a corner, the outer corner has a radius of 14 and the inner corner stays sharp; where a stroke ends, the corner stays square. The frame is 7 units wide. The wordmark is optically corrected: 2 units lower and 1.5 units further right than mathematically centered.

The logo files are not covered by the MIT license; see `TRADEMARKS.md`.

### Blue scale

The scale follows aerial perspective: near is dark and dense, far is light and airy.

| Token | Hex | Use | Contrast |
| --- | --- | --- | --- |
| `palette.depth` | #0A1A33 | Dark surfaces, text | 17.4:1 on white |
| `palette.near` | #0B4A99 | Links and blue text | 8.6:1 on white |
| `palette.brand` | #1B6CE0 | Logo, accent, buttons | 4.9:1 on white |
| `palette.far` | #7FB2F0 | Lines, charts, accent on dark ground | 7.9:1 on depth |
| `palette.haze` | #DCEAFB | Selection, surfaces | surface only |
| `palette.sky` | #F3F7FC | Background | surface only |

- **MUST:** Blues come exclusively from this scale.
- **NEVER:** haze or sky as a text color.
- **SHOULD:** Neutrals for text, lines and surfaces carry a hint of blue instead of pure gray (`theme.light.*`, `theme.dark.*`). This is the quiet signature of Leonardo on every platform.

### Typefaces

| Use | Typeface |
| --- | --- |
| Windows apps | Segoe UI Variable (system) |
| iOS and macOS apps | SF Pro (system) |
| Standalone web apps and websites | Inter, titles in Inter Display |
| Web parts and apps inside a host (SharePoint, Teams, Outlook) | The host's typeface |
| Presentations and documents | Inter, titles in Inter Display |
| Logo | Drawn letters, no typeface |

- **MUST:** Native apps use the system typeface of their platform.
- **MUST:** Presentations, documents and standalone web use Inter.
- **MUST:** Web interfaces inside a host follow the host's typeface and design system.
- **NEVER:** Inter in native apps, or system typefaces in presentations.
- **SHOULD:** Customers receive presentations and documents as PDF so the typeface is embedded.

Inter is licensed under the SIL Open Font License and is installed on all Windows PCs and Macs at up in blue.

## Shell and content

The platform owns ground and tools, Leonardo owns the content. This way every app feels at home on its platform and is still recognizable as up in blue.

| Layer | Owned by | Examples |
| --- | --- | --- |
| Tools (closest to the user) | Platform | Command bars, toolbars, menus, menu bar, tab bars, navigation, dialogs, sheets |
| Content | Leonardo | Type roles, alignment line, grid, density, color semantics, status shapes, motion vocabulary |
| Ground | Platform | Window backdrop, sidebar, page background |

- **MUST:** Ground and tools use the components, materials and behavior of the platform unchanged.
- **MUST:** Glass exists only where the platform provides it.
- **MUST:** A multiplatform app is designed per platform: Mac conventions on the Mac, iOS conventions on iOS. Shared code never means shared UI.
- **NEVER:** custom glass, custom material effects or rebuilt system components.

### Platforms and hosts

| Platform | What counts as the platform |
| --- | --- |
| Windows | Windows app design guidelines and Fluent 2, implemented with WinUI 3 or WPF with its Fluent theme |
| iOS | Apple Human Interface Guidelines for iOS, implemented with SwiftUI |
| macOS | Apple Human Interface Guidelines for macOS, implemented with SwiftUI |
| Web inside a Microsoft host (SharePoint, Teams, Outlook) | The host and Fluent 2 for the web, including the host's theme |
| Standalone web | The browser: semantic HTML, native form controls, URLs, the back button, user preferences such as reduced motion and color scheme |
| Presentations | PowerPoint's own tools: layouts, animations, "Morph" |

### Dialects

Every platform speaks Leonardo in its own dialect:

| Leonardo principle | Windows | iOS | macOS | Web | Presentations |
| --- | --- | --- | --- | --- | --- |
| Place name | Large title in the content, shrinks on scroll | Large Title, native | Window title in the toolbar; no second large title | Page heading, large and light | Large, light slide title |
| Dive in | Navigation transition with connected element | Push in the navigation stack | Selection in sidebar and content columns; push inside the detail column | Transition with shared element where supported, otherwise crossfade; every view has its own URL | "Morph" transition |
| Commands | Fluent CommandBar at the top, context menu, keyboard accelerators | Toolbar, context menu, swipe actions | Menu bar with every command and its shortcut, toolbar, context menu | Visible buttons, links for navigation; the browser's context menu stays | – |
| Settings | Settings page in the app | Settings screen in the app or the system Settings app | Settings window, opened with ⌘, | Settings page | – |
| Glass | Acrylic in flyouts and menus | System glass in bars and sheets | System materials in sidebar and toolbar | None, unless the host provides it | None |
| Typeface | Segoe UI Variable | SF Pro | SF Pro | Inter, or the host's typeface | Inter |
| Icons | Fluent icons | SF Symbols | SF Symbols | The host's icons, otherwise one icon family | One icon family per deck |
| Color semantics, alignment line, grid, density | Leonardo | Leonardo | Leonardo | Leonardo | Leonardo |

**MUST:** These tables are binding. If a case is missing, the decision rules at the top of this document apply, starting with the platform's official guidelines.

## Usability

Usability is what makes software good to work with. Leonardo makes it recognizable; usability makes it useful. These rules apply on every platform and are interpreted through the platform's conventions.

### Orientation

- **MUST:** Every view answers three questions at a glance: Where am I? What can I do here? What just happened?
- **MUST:** The same action has the same name and the same place throughout the app.
- **SHOULD:** Recognition over recall: show recent items, sensible defaults and suggestions instead of making people remember.

### Feedback and status

- **MUST:** Direct manipulation responds within about 0.1 seconds.
- **MUST:** Waits longer than about one second show progress. Waits longer than about ten seconds show progress with an estimate or a way to cancel, and never block the rest of the app.
- **MUST:** Loading, saving, syncing and errors are visible where they happen.

### Errors and safety

- **MUST:** Undo comes before confirmation. Destructive actions can be undone where technically possible.
- **MUST:** A confirmation is used only when an action cannot be undone. It names the consequence and uses a specific verb, for example "Delete 3 files", never "OK".
- **MUST:** Error messages say what happened, why, and what to do next, in plain language, next to where the problem is.
- **NEVER:** error codes without explanation, or messages that blame the user.

### Forms and input

- **MUST:** Every field has a visible label. A placeholder is never the label.
- **MUST:** Validation happens when a field is left or the form is submitted, shown at the field, and entered data is never lost on an error.
- **MUST:** Required or optional is marked consistently, by marking the exception.
- **NEVER:** disabled controls without a way to learn why they are disabled.

### Empty, loading and error states

- **MUST:** An empty state explains what will appear here and offers the first action.
- **SHOULD:** Loading shows the structure of what is coming instead of a lone spinner, where the platform supports it.

### Keyboard and input devices

- **MUST:** The platform's standard shortcuts work: copy, paste, undo, find, select all, close, settings.
- **MUST:** On macOS every command is reachable from the menu bar and shows its shortcut there.
- **SHOULD:** Frequent commands have shortcuts; their keys follow platform conventions.

### Writing

- **MUST:** Buttons use specific verbs that describe the result.
- **MUST:** Capitalization, punctuation and terminology follow the platform's writing guidelines. They differ between Microsoft and Apple; look them up.
- **SHOULD:** Short sentences, no jargon, the user's words rather than the system's.
- **MUST:** Layouts survive long German words and translations: text wraps instead of being clipped.

### Interruptions

- **NEVER:** modal dialogs for information that can be shown inline.
- **NEVER:** notifications or badges for things the user did not ask to follow.

## Typography as architecture

Type replaces frames. Leonardo defines the roles, the platform provides the typeface:

| Role | Windows (Segoe UI Variable) | iOS (SF Pro) | macOS (SF Pro) | Standalone web (Inter) | Presentation (Inter) |
| --- | --- | --- | --- | --- | --- |
| `placeName` | Display 40/52 epx, Light | Large Title | Window title (system) | Inter Display 2.5 rem / 1.2, Light | Inter Display 40 pt, Light |
| `view` | Display 28/36 epx, Semilight | Title 2 | Title 2 | Inter Display 1.75 rem / 1.25, Light | – |
| `section` | Text 20/28 epx, Semibold | Headline | Headline | Inter 1.25 rem / 1.4, Semibold | Inter 24 pt, Semibold |
| `body` | Text 14/20 epx, Regular | Body | Body | Inter 1 rem / 1.5, Regular | Inter 18 pt, Regular |
| `meta` | Small 12/16 epx, tertiary text color | Footnote | Caption | Inter 0.8125 rem / 1.4 | Inter 12 pt |

On iOS and macOS the role maps to a system text style, and the system decides the size. Inside a Microsoft host, web interfaces use the host's type ramp and map the roles to it.

- **MUST:** Only the roles in this table exist. Emphasis is the body role in semibold, never a separate size.
- **MUST:** Hierarchy comes from size, weight and white space.
- **MUST:** At most three sizes per area. List and details pane count separately.
- **MUST:** iOS supports Dynamic Type; roles scale with the user's text size. On the web, sizes are set in rem so they follow the user's browser settings.
- **MUST:** Numbers in columns use tabular figures so they align vertically.
- **SHOULD:** The place name sits large and light at the top, like a station sign, and shrinks to a compact line on scroll. On iOS the Large Title does this.
- **SHOULD:** On Windows, view headings run past the right edge when there are more views than fit. This shows without explanation that there is more.
- **NEVER:** lines or boxes to create hierarchy.
- **NEVER:** lowercase as a stylistic device. The only exception is the brand name.

## Grid and geometry

Everything sits on a base grid of 4; layout spacing jumps in steps of 8. Values are epx on Windows and pt on iOS; on Windows they match the Fluent grid.

| Token | Value | Use |
| --- | --- | --- |
| `space.xs` | 4 | Icon to label |
| `space.s` | 8 | Within a group |
| `space.m` | 16 | Between groups |
| `space.l` | 24 | Page margins, details pane |
| `space.xl` | 40 | Between place name and content |

- **MUST:** Spacing comes exclusively from this table.
- **MUST:** Shared alignment line. Place name, column headers and the first text column start on the same left edge. Icons and status dots hang to the left of it.
- **MUST:** Concentric radii. Inner radius = outer radius − padding. A card with `radius.l` and padding `space.s` contains elements with `radius.m`. The logo follows the same rule.
- **MUST:** Basic shapes carry meaning. Circle = person or state (avatars, presence, sync status). Rounded square = object (files, folders, libraries). Capsule = filter, and buttons only where the platform makes them capsule-shaped.
- **SHOULD:** Asymmetric balance. Navigation fixed, content flexible, details fixed; the weight sits on the left, where the eye starts.
- **SHOULD:** Lists offer three densities: `density.compact` 32, `density.standard` 40, `density.comfortable` 48 row height. Standard is the default; the choice is stored per user.
- **NEVER:** spacing by eye or radii that contradict the concentric rule.

## Color as signal

The surface is cool-neutral, and every color that appears means exactly one thing:

| Color | Meaning | Examples | Never for |
| --- | --- | --- | --- |
| Accent (`theme.*.accent`: brand, on dark ground far) | Act here | Primary action, selection | Headings, surfaces, decoration |
| Link (`theme.*.link`) | This leads somewhere | Links in running text | Buttons |
| Red (system color) | Something is broken | Sync conflict, upload failed | Delete buttons at rest |
| Yellow (system color) | Attention, but not blocked | Checked out by another person | Highlights |
| Green (system color) | Briefly confirmed, then gone | Upload finished | Permanent states |
| Cool neutrals | Everything else | Text, lines, surfaces, app icons | – |

- **MUST:** The accent is brand blue, on dark ground far. On Windows it is the app accent color, on iOS and macOS the app's accent color, on standalone web the CSS accent. Inside a Microsoft host the host's theme decides.
- **MUST:** At most one accent surface per view.
- **MUST:** Status colors come from the platform: Fluent colors on Windows, system colors on iOS and macOS, the host's colors inside a host. Standalone web uses the `theme.*.status*` tokens, which mirror the Windows status colors.
- **MUST:** Every status carries shape and text in addition to color, for example a circle plus tooltip.
- **MUST:** Focus visuals and elements the platform draws itself stay unchanged.
- **SHOULD:** Success is confirmed only briefly; the green disappears afterwards.
- **SHOULD:** File type icons stay colorful because they are content. Everything around them stays neutral.
- **NEVER:** color as decoration, for headings or for large surfaces.

## Motion as grammar

Motion explains where something comes from and where it goes. Every motion is a word from this vocabulary; whatever cannot be expressed in one of them is not animated. This applies to apps and presentations alike.

### Vocabulary

| Motion | Meaning | Sequence |
| --- | --- | --- |
| Dive in | I go one level deeper | The chosen element flies to its new place and becomes the place name or slide title. The rest follows staggered. Going back reverses the motion exactly. |
| Transform | This thing becomes that | An element changes shape, size or content instead of disappearing and reappearing. A button becomes the panel it opens. |
| Stagger | These belong together, in this order | The groups of a view or slide enter shortly after each other, in reading order. |
| Reflow | The order has changed | An element pushes others aside in an animated way; gaps close, sorted rows glide into place. |
| Arrive | Something new is here | New items appear in their place with a short accent marker. |
| Leave | This is gone | An element accelerates out and fades while doing so. |
| Respond | I heard you | Visible reaction at the moment of pressing and releasing. |

### Tempo and easing

Arriving decelerates strongly, leaving accelerates. This relationship is the signature of Leonardo motion.

| | Windows | iOS and macOS | Web | Presentation (PowerPoint) |
| --- | --- | --- | --- | --- |
| Arrive, dive in | `motion.easing.enter` cubic-bezier(0, 0, 0, 1); 167, 250 or 333 ms depending on distance | Spring without bounce, 0.3–0.5 s | Same curve and durations as Windows | Smooth end fully applied; 0.3 s |
| Leave | `motion.easing.exit` cubic-bezier(1, 0, 1, 1); 167 ms, always with fade | Spring without bounce, shorter than arrive | Same as Windows | Smooth start fully applied; 0.2 s, with fade |
| Transform, reflow | `motion.easing.move` cubic-bezier(0.55, 0.55, 0, 1); 250 or 333 ms | Spring without bounce | Same as Windows | "Morph" transition, 0.5 s |
| Opacity | linear, 83 ms | System default | linear, 83 ms | "Fade", 0.1 s |
| Stagger | 50 ms offset per group | 50 ms offset per group | 50 ms offset per group | "With previous", delay in steps of 0.05 s |

On macOS, motion is used more sparingly than on iOS: windows, sidebars and toolbars animate the way the system animates them, and content motion stays short.

### Rules

- **MUST:** Arriving decelerates, leaving accelerates. Movement is never linear; only opacity is linear.
- **MUST:** Leaving is shorter than arriving.
- **MUST:** Opacity is faster than movement. The content is visible at once while it still glides into place.
- **MUST:** Direction is fixed. Forward comes from the chosen element or from the right; back reverses exactly that.
- **MUST:** Responding starts on press, not after release. The release feedback starts immediately, even if the triggered action takes longer. What pressing looks like is defined by the platform.
- **MUST:** Motion never blocks. Every animation can be interrupted; clicking on quickly jumps to the end state. On iOS the animation takes over the velocity of the gesture.
- **MUST:** If animations are turned off or reduced in the system (on the web: `prefers-reduced-motion`), every motion becomes a short crossfade. The meaning stays, the distance goes.
- **MUST:** Every motion is implemented with the means of its medium: Fluent curves and navigation transitions on Windows, SwiftUI springs on iOS and macOS, CSS transitions on the web, animations and "Morph" in PowerPoint.
- **SHOULD:** Short distances. On entering, an element travels at most 24 to 40 epx (`motion.distance.*`); on slides at most a few percent of the slide width.
- **SHOULD:** Stagger sparingly. At most four groups, in reading order.
- **SHOULD:** The only depth effect: the place name scrolls slightly slower than the content before it shrinks (`motion.parallax.placeNameFactor`). When morphing in presentations, the slide title moves less far than the content.
- **NEVER:** parallax outside this exception, bounce in content, 3D rotations, light or texture effects, endless loops except for real progress, decorative animations on launch.

### Presentations in detail

- **Dive in and transform** are built with the "Morph" transition. An element that becomes the title on the next slide carries the same name on both slides in the selection pane, prefixed with `!!`, so PowerPoint matches it reliably.
- **Stagger and arrive** use only "Fade", optionally with a short motion path.
- **Leave** uses "Fade" as exit effect.
- **Slide transitions** are "Morph", "Fade" or none.
- **NEVER:** effects that fly in from outside the slide, spin, bounce or jump at the end.

## Accessibility and honest fallbacks

The fallback is not an emergency but the second half of the design. If a view looks good opaque and without motion, the glass was never load-bearing, and that is exactly how it should be.

- **MUST:** Every view works and looks good without transparency, without animations, in high contrast mode and with enlarged text.
- **MUST:** System settings are respected: on Windows transparency and animation effects; on iOS and macOS "Reduce Transparency", "Reduce Motion" and "Increase Contrast"; on the web `prefers-reduced-motion`, `prefers-color-scheme`, `prefers-contrast` and `forced-colors`.
- **MUST:** Text reaches at least 4.5:1 contrast (WCAG AA); on glass against the least favorable background that can be behind it.
- **MUST:** Everything is reachable by keyboard; the focus visual is never removed.

How views behave in each situation:

| Situation | What happens |
| --- | --- |
| Transparency effects off in Windows | Mica and glass become opaque surfaces in the layer's colors. The layering stays recognizable through brightness and a fine edge. |
| High contrast theme active (Windows), forced colors (web) | System colors only. Glass becomes an opaque surface with a border; status dots get a symbol and text. |
| Increase Contrast (iOS, macOS) | System colors and materials adapt. Leonardo colors keep at least the contrast required above. |
| Animation effects off | Every motion becomes a short crossfade (see Motion). |
| Text size increased in Windows | Layout wraps instead of clipping. The place name does not grow beyond its role. |
| Keyboard only | Windows: F6 moves between the areas of a window. macOS: full keyboard access and the menu bar. Web: a visible focus indicator and a logical tab order. Every command is reachable without a pointer. |
| Screen reader (Narrator, VoiceOver, browser screen readers) | Status dots have names, for example "Checked out by Anna Berg". "Arrive" is announced as a live announcement. |

**SHOULD:** The tint of glass is measured by legibility, not by beauty.
