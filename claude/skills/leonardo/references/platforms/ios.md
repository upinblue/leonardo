# iOS: SwiftUI

An iOS app behaves like the apps Apple ships with iPhone and iPad. Users know Mail, Files, Settings and Notes; respect what those apps teach.

## Research first

Read "Designing for iOS" in the Human Interface Guidelines, then the pages for every component and pattern you use, typically: navigation and search, tab bars, toolbars, lists and tables, sheets, menus and context menus, materials, typography, color, SF Symbols, accessibility, and the Apple Style Guide for text. The HIG changes with each iOS release; always read the current version.

## Platform conventions to respect

- **Navigation:** `NavigationStack` with large titles for hierarchies; a tab bar for a few top-level areas; sheets for focused, self-contained tasks.
- **Commands:** toolbar items in the navigation bar or bottom toolbar; context menus on long press; swipe actions in lists for frequent actions, always also reachable another way.
- **Search:** `.searchable` in the navigation bar.
- **Lists:** system list styles; pull to refresh where content can change.
- **Text:** system text styles with Dynamic Type; layouts adapt to the largest sizes.
- **Color and materials:** system colors for status; system materials and system glass in bars and sheets. Never draw custom bars.
- **Icons:** SF Symbols.
- **iPad:** split views and pointer and keyboard support where the app runs on iPad.
- **Writing:** follow the Apple Style Guide and the HIG's guidance on text, including capitalization.

## Leonardo on iOS

- **Place name:** the navigation title in large title style.
- **Colors:** from the generated asset catalog in `generated/apple` (light and dark included); the accent color is brand blue.
- **Type roles:** map to system text styles: view → Title 2, section → Headline, body → Body, meta → Footnote.
- **Spacing and radii:** from the generated Swift values.
- **Motion:** springs without bounce, 0.3 to 0.5 seconds; they take over the velocity of gestures. With Reduce Motion, crossfade.
- **Status:** shapes from `design-language.md` (filled dot, ring, circle with mark), drawn with SF Symbols where possible, with an accessibility label.

## Never on iOS

- Custom navigation bars or tab bars.
- Hamburger menus.
- Windows or Mac patterns such as right-click-only commands, menu bars or tiny click targets.
- Fixed font sizes that ignore Dynamic Type.
