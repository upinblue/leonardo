# macOS: SwiftUI

A Mac app is a Mac app, not a large iOS app. Mac users work with keyboard and pointer, several windows and the menu bar. They expect Finder, Mail and Notes behavior.

## Research first

Read "Designing for macOS" in the Human Interface Guidelines, then the pages for every component and pattern you use, typically: the menu bar, menus, windows, toolbars, sidebars, split views, settings, keyboards, pointing devices, drag and drop, materials, typography, accessibility, and the Apple Style Guide for text. Read the current version; it changes with each macOS release.

## Platform conventions to respect

- **Menu bar:** every command of the app is in the menu bar, in the standard menus (app menu, File, Edit, View, Window, Help) where it belongs, with its keyboard shortcut. Implement it with SwiftUI `Commands`.
- **Settings:** a separate Settings window, opened with Command-comma, implemented with the `Settings` scene.
- **Windows:** resizable windows, multiple windows where documents or items can be opened side by side, state restoration, sensible minimum sizes.
- **Structure:** `NavigationSplitView` with a sidebar for top-level areas and sources; content and detail columns for lists and items.
- **Toolbar:** standard toolbar for the most frequent commands of the window; customizable where the app is used heavily.
- **Pointer and keyboard:** hover feedback, tooltips (`.help`), context menus on right click, multiple selection with Command and Shift, drag and drop between windows and apps, full keyboard access.
- **Sheets and dialogs:** attached to the window they belong to.
- **Undo:** Edit menu with undo and redo for every change the user makes.
- **Writing:** follow the Apple Style Guide and the HIG's guidance on menus and labels, including capitalization, which differs from Windows.

## Leonardo on macOS

- **Place name:** the window title in the toolbar. No second large title inside the content.
- **Colors:** from the generated asset catalog in `generated/apple`; the accent color is brand blue.
- **Type roles:** map to system text styles: view → Title 2, section → Headline, body → Body, meta → Caption. Mac text is smaller than iOS text; do not copy iOS sizes.
- **Density:** Mac lists are dense. Use the compact or standard density.
- **Spacing, radii and the alignment line:** as everywhere, from the generated Swift values.
- **Motion:** shorter and rarer than on iOS. Windows, sidebars and toolbars animate the way the system animates them. With Reduce Motion, crossfade.

## Never on macOS

- Bottom tab bars, swipe-only actions, large touch-sized controls or full-screen modal flows taken from iOS.
- Commands that exist only as buttons in the content and not in the menu bar.
- Settings inside the main window instead of a Settings window.
- Windows patterns such as a ribbon or a command bar inside the content.
