# macOS: SwiftUI

A Mac app is a Mac app, not a large iOS app. Mac users work with keyboard and pointer, several windows and the menu bar. They expect Finder, Mail and Notes behavior.

## Research first

Read "Designing for macOS" in the Human Interface Guidelines, then the pages for every component and pattern you use, typically: the menu bar, menus, windows, toolbars, sidebars, split views, settings, keyboards, pointing devices, drag and drop, materials, typography, accessibility, and the Apple Style Guide for text. Read the current version and note the page's change-log date; it changes with each macOS release.

## Platform conventions to respect

- **Menu bar:** every command of the app is in the menu bar, in the standard menus (app menu, File, Edit, Format, View, app-specific menus, Window, Help) where it belongs, with its keyboard shortcut. Every toolbar item is also a menu command. Unavailable items are disabled, not hidden. Implement it with SwiftUI `Commands` (`CommandGroup`, `CommandMenu`, and the built-in `SidebarCommands`, `ToolbarCommands`, `TextEditingCommands`).
- **Settings:** a separate Settings window from the `Settings` scene, opened from the app menu and with Command-comma. Panes in a `TabView`; the window title names the pane. No settings button in the main window's toolbar.
- **Windows:** resizable windows, multiple windows where documents or items can be opened side by side, full screen, state restoration, sensible minimum sizes.
- **Structure:** `NavigationSplitView` with a sidebar for top-level areas and sources; content and detail columns for lists and items. The sidebar can be shown and hidden from the View menu.
- **Toolbar:** standard toolbar for the most frequent commands of the window; customizable where the app is used heavily. Short window titles; never the app name as title.
- **Pointer and keyboard:** hover feedback, tooltips (`.help`), context menus on right click (`.contextMenu`), multiple selection with Command and Shift, drag and drop between windows and apps, full keyboard access.
- **Sheets and dialogs:** attached to the window they belong to.
- **Undo:** Edit menu with undo and redo for every change the user makes (`UndoManager`).
- **Text:** macOS has no Dynamic Type. Use the system text styles at their Mac sizes; the default body size is 13 pt. Do not copy iOS sizes.
- **Writing:** follow the Apple Style Guide and the HIG's guidance on menus and labels, including title-style capitalization in menus and an ellipsis on commands that open another window.

## Leonardo on macOS

Leonardo values come only from `generated/apple` in upinblue/leonardo: add `Leonardo.xcassets` and `LeonardoTokens.swift` to the Mac app target. `LeonardoTokens.swift` already contains the Mac differences under `#if os(macOS)`.

**App target only.** The generated colors are `Color("Leonardo/…")` without a `bundle:` argument, so they are looked up in the main bundle. In a Swift package module they are not found.

| Leonardo | macOS |
| --- | --- |
| Place name | The window title in the toolbar (`.navigationTitle`). No second large title inside the content; `Leonardo.TextStyle` has no `placeName` on macOS for this reason. |
| Type roles | view → `.title2`, section → `.headline`, body → `.body`, meta → `.caption` (iOS uses `.footnote`). Emphasis is `.body` with `.fontWeight(.semibold)`. |
| Colors | `Leonardo.Colors.…` from the asset catalog, as on iOS, including `.chart` for a single-series chart. Respect the user's system accent color where the system uses it (sidebar icons, selection); the app's `AccentColor` is brand blue in light and far in dark. |
| Status | System colors, as the design language requires: `.red` broken or overdue, `.yellow` attention, `.green` briefly confirmed, always with shape and text. The asset catalog has no Leonardo status colors. |
| Density | Three densities, standard by default, the choice stored per user: compact is the system's default table row height (set no row height), standard is `Leonardo.Density.standard` (40), comfortable is `Leonardo.Density.comfortable` (48). For a `List`, for example `.environment(\.defaultMinListRowHeight, Leonardo.Density.standard)`. |
| Sizes | `Leonardo.Size.icon` (16) for icons in rows and next to labels, `Leonardo.Size.statusDot` (12) for the status dot. |
| Spacing, radii | `Leonardo.Space`, `Leonardo.Radius`, as everywhere. |
| Page margin, alignment line | System sidebars, tables and forms keep their insets. For content you lay out yourself, the page margin is `Leonardo.Space.l`; next to lists with icons, text starts at `Leonardo.Space.xl`. |
| Motion | Shorter and rarer than on iOS. Windows, sidebars, toolbars and sheets animate the way the system animates them. Content motion: springs without bounce, entrances `.spring(duration: Leonardo.Motion.Spring.durationMax, bounce: Leonardo.Motion.Spring.bounce)`, exits `durationMin`. Stagger only on navigation to a view, never when a window opens at launch. Arrive: a short accent bar at the start of the row, held for `Leonardo.Motion.arriveHold`. With Reduce Motion, `.opacity` only. |

### Examples

These examples are written against the generated `LeonardoTokens.swift` and Apple's documentation. There is no Apple sample in upinblue/leonardo yet, so they are not taken from a running app.

One app, two platforms: the scene declares what only the Mac has.

```swift
@main
struct FilesApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
        #if os(macOS)
        .commands {
            SidebarCommands()
            ToolbarCommands()
            CommandMenu("File Actions") {
                Button("Upload Files…") { upload() }
                    .keyboardShortcut("u", modifiers: [.command, .shift])
            }
        }
        #endif

        #if os(macOS)
        Settings {
            SettingsView()   // TabView with one tab per pane
        }
        #endif
    }
}
```

The content of a Mac window: sidebar, list and details; the window title is the place name.

```swift
NavigationSplitView {
    List(folders, selection: $selectedFolder) { folder in
        Label(folder.name, systemImage: "folder")
    }
} content: {
    Table(files, selection: $selectedFiles) { … }   // multiple selection with Command and Shift
        .contextMenu(forSelectionType: File.ID.self) { ids in … }
        .navigationTitle(selectedFolder?.name ?? "Files")
} detail: {
    FileDetails(files: selectedFiles)
        .padding(Leonardo.Space.l)
}
```

Incorrect on the Mac:

```swift
TabView { … }                                        // a tab bar for the top-level areas instead of a sidebar in a NavigationSplitView
Text("Contracts").font(.largeTitle)                  // a second large title below the window title
.toolbar { Button("Settings") { showSettings = true } }   // settings in the main window instead of the Settings scene
Button("Delete") { delete() }                        // a command that exists only in the content, not in the menu bar
List { … }.environment(\.defaultMinListRowHeight, 30)   // a literal row height; compact means the system's own row
FileDetails(files: selectedFiles).padding(20)        // literal padding instead of Leonardo.Space.l
```

## Before the pull request (macOS)

- [ ] Every command is in the menu bar with its shortcut; if the app has settings, Command-comma opens the Settings window.
- [ ] Full keyboard access works for the main task; tooltips on toolbar items.
- [ ] Light and dark, Increase Contrast, Reduce Transparency, Reduce Motion.
- [ ] Undo and redo for every user change.
- [ ] No iOS patterns (bottom tab bar, swipe-only actions, full-screen modal flows).
- [ ] Lists offer compact (system row), standard and comfortable; standard is the default.
- [ ] No literal colors, font sizes, spacing, radii or animation durations; springs have no bounce.

## Never on macOS

- Bottom tab bars, swipe-only actions, large touch-sized controls or full-screen modal flows taken from iOS.
- Commands that exist only as buttons in the content and not in the menu bar.
- Settings inside the main window instead of a Settings window.
- Windows patterns such as a ribbon or a command bar inside the content.
