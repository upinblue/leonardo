# iOS: SwiftUI

An iOS app behaves like the apps Apple ships with iPhone and iPad. Users know Mail, Files, Settings and Notes; respect what those apps teach.

## Research first

Read "Designing for iOS" in the Human Interface Guidelines, then the pages for every component and pattern you use, typically: tab bars, toolbars, sidebars (iPad), lists and tables, sheets, menus and context menus, search, materials, motion, typography, color, SF Symbols, accessibility, and the Apple Style Guide for text. The HIG changes with each iOS release (for example the Liquid Glass guidance for bars and controls); always read the current version and note the page's change-log date.

## Platform conventions to respect

- **Navigation:** `NavigationStack` with large titles for hierarchies; a `TabView` tab bar for a few top-level sections (never for actions); sheets for focused, self-contained tasks. Swipe back works everywhere.
- **Commands:** toolbar items in the navigation bar or bottom toolbar, one prominent primary action at most; context menus on long press; swipe actions in lists for frequent actions, always also reachable another way (a context menu or an edit mode).
- **Search:** `.searchable` in the navigation bar or as a search tab.
- **Lists:** system list styles; pull to refresh where content can change.
- **Text:** system text styles with Dynamic Type; layouts adapt up to the accessibility sizes instead of truncating (stack instead of columns).
- **Color and materials:** system colors for status; system materials and Liquid Glass come with the system bars and sheets. Never put glass into the content layer, and never draw custom bars.
- **Touch targets:** at least 44 × 44 pt.
- **Icons:** SF Symbols.
- **iPad:** the tab bar can become a sidebar (`.tabViewStyle(.sidebarAdaptable)`); support pointer, keyboard shortcuts (`.keyboardShortcut`) and multitasking. iPadOS has a menu bar: the app's SwiftUI `Commands` appear there, so give frequent commands a menu item and a shortcut (see "The menu bar" in the HIG).
- **Writing:** follow the Apple Style Guide and the HIG's guidance on text, including title-style capitalization for button and menu labels where Apple uses it.

## Leonardo on iOS

Leonardo values come only from `generated/apple` in upinblue/leonardo: add `Leonardo.xcassets` and `LeonardoTokens.swift` to the app target. Never type a Leonardo value by hand.

**App target only.** The generated colors are `Color("Leonardo/Accent")` and so on, without a `bundle:` argument, so SwiftUI looks them up in the main bundle. The catalog must be compiled into the app itself; inside a Swift package module the colors are not found, and the package's `AccentColor` does not tint the app.

| Leonardo | iOS |
| --- | --- |
| Place name | The navigation title in large title style (`.navigationTitle` with `.navigationBarTitleDisplayMode(.large)`). The system shrinks it on scroll. |
| Type roles | `Leonardo.TextStyle`: placeName → `.largeTitle` (used by the navigation bar), view → `.title2`, section → `.headline`, body → `.body`, meta → `.footnote`. Emphasis is `.body` with `.fontWeight(.semibold)`. |
| Colors | `Leonardo.Colors.textPrimary`, `.textSecondary`, `.textTertiary`, `.surface`, `.surfaceAlt`, `.selection`, `.link`, `.strokeSubtle`, `.strokeStrong`, `.accent`, `.onAccent`, `.chart` (asset catalog `Leonardo/…`, light and dark). The catalog's `AccentColor` makes the app's tint brand blue in light and far in dark. Most text can simply use the system's primary and secondary styles. |
| Charts | A single series in `Leonardo.Colors.chart` (Swift Charts: `.foregroundStyle(Leonardo.Colors.chart)`). |
| Status | System colors, as the design language requires: `.red` broken or overdue, `.yellow` attention, `.green` briefly confirmed. The asset catalog has no Leonardo status colors. Always with a shape and text. |
| Spacing | `Leonardo.Space.xs` 4, `.s` 8, `.m` 16, `.l` 24, `.xl` 40 |
| Radii | `Leonardo.Radius.s`, `.m`, `.l` (concentric: inner = outer − padding) |
| Sizes | `Leonardo.Size.icon` 16 and `Leonardo.Size.statusDot` 12, as base values of `@ScaledMetric(relativeTo: .body)`, so icons and dots grow with Dynamic Type. |
| Density | System row heights, at least 44 pt. The Leonardo densities do not apply on iOS: no density setting, no fixed row heights. |
| Page margin, alignment line | Inside system lists and forms the system insets win. For content you lay out yourself, the page margin is `Leonardo.Space.l`; next to lists with icons, text starts at `Leonardo.Space.xl`. |
| Motion | Springs without bounce: entrances `.spring(duration: Leonardo.Motion.Spring.durationMax, bounce: Leonardo.Motion.Spring.bounce)`, exits `durationMin`, so leaving stays shorter than arriving. Stagger with `Leonardo.Motion.staggerStep`, at most `Leonardo.Motion.staggerMaxGroups` groups, only when the user navigates to a view, never at launch. Arrive: a short accent bar at the start of the row, visible for `Leonardo.Motion.arriveHold`, then fading; never a dot, which is the status shape. With Reduce Motion, `.opacity` transitions only. |

### Examples

These examples are written against the generated `LeonardoTokens.swift` and Apple's documentation. There is no Apple sample in upinblue/leonardo yet, so they are not taken from a running app.

Type roles and spacing:

```swift
VStack(alignment: .leading, spacing: Leonardo.Space.s) {
    Text(file.name)
        .font(.system(Leonardo.TextStyle.section))
    Text("Modified by \(file.modifiedBy)")
        .font(.system(Leonardo.TextStyle.meta))
        .foregroundStyle(Leonardo.Colors.textTertiary)
}
.padding(.horizontal, Leonardo.Space.l)
```

Incorrect:

```swift
Text(file.name).font(.system(size: 20, weight: .semibold))           // fixed size ignores Dynamic Type
Text(file.modifiedBy).foregroundColor(Color(red: 0.4, green: 0.45, blue: 0.5))   // literal color
VStack(spacing: 12) { … }                                             // spacing not from Leonardo.Space
Text(file.name).padding(15)                                           // literal padding
List { … }.environment(\.defaultMinListRowHeight, Leonardo.Density.compact)   // densities do not apply on iOS
```

`0` and a 1 pt hairline are not design values; every other number in `.padding`, `spacing:` and `EdgeInsets` comes from `Leonardo.Space`.

Motion that respects Reduce Motion: keep the spring, and with Reduce Motion take the distance away (`.opacity` instead of `.move`), not the meaning. Entrances take longer than exits.

```swift
@Environment(\.accessibilityReduceMotion) private var reduceMotion

let arrive = Animation.spring(duration: Leonardo.Motion.Spring.durationMax, bounce: Leonardo.Motion.Spring.bounce)
let leave = Animation.spring(duration: Leonardo.Motion.Spring.durationMin, bounce: Leonardo.Motion.Spring.bounce)

var arriveTransition: AnyTransition {
    reduceMotion ? .opacity : .move(edge: .trailing).combined(with: .opacity)
}
```

Incorrect: `.easeInOut(duration: 0.4)` (literal duration), `.spring(response: 0.6, dampingFraction: 0.5)` (bounce), `.interpolatingSpring(stiffness:damping:)` with overshoot, `.animation(.linear(duration: 1).repeatForever())` outside real progress, a staggered entrance of the first screen at launch.

Status, shape plus text plus label. Choose the shape by meaning: a filled dot in the accent color for new, a ring in `.yellow` for attention (checked out, expires soon), a filled circle with exclamation mark in `.red` for broken or overdue.

```swift
@ScaledMetric(relativeTo: .body) private var dotSize = Leonardo.Size.statusDot

Label {
    Text("Checked out by Anna Berg")
} icon: {
    Image(systemName: "circle")          // ring = attention; "circle.fill" = new; "exclamationmark.circle.fill" = broken or overdue
        .resizable()
        .frame(width: dotSize, height: dotSize)
        .foregroundStyle(.yellow)
}
.accessibilityElement(children: .combine)
```

## Before the pull request (iOS)

- [ ] Light and dark, Increase Contrast, Reduce Transparency, Reduce Motion.
- [ ] Dynamic Type up to the largest accessibility size: nothing truncates or overlaps.
- [ ] VoiceOver on the main task: every element has a label; status is read as text.
- [ ] Every swipe action is also reachable without swiping.
- [ ] No literal colors, font sizes, spacing, radii or animation durations; springs have no bounce.
- [ ] Lists keep the system row heights; status in system colors with shape and text; no stagger at launch.

## Never on iOS

- Custom navigation bars or tab bars, or glass in the content layer.
- Hamburger menus.
- Windows or Mac patterns such as right-click-only commands, menu bars or tiny click targets.
- Fixed font sizes that ignore Dynamic Type.
- Bouncing springs in content.
