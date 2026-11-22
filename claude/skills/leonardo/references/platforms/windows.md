# Windows: WinUI 3 and WPF

A Windows app behaves like a Windows app. Users bring habits from File Explorer, Outlook, Settings and Office; respect them.

## Research first

Look up in the Windows app design guidance (see `sources.md`) the pages for the patterns you are about to use, typically: navigation basics, command design basics, the specific controls, dialogs and flyouts, title bar, motion, accessibility, and the Microsoft Writing Style Guide for text.

## Platform conventions to respect

- **Window:** standard caption buttons; a custom title bar only as the guidelines describe it; windows resize freely and work with Snap layouts; sensible minimum sizes.
- **Navigation:** `NavigationView` for top-level areas; a breadcrumb or address bar where users navigate hierarchies; back navigation where the user dives in.
- **Commands:** `CommandBar` for the commands of the current view, always visible; context menus on right click, Shift+F10 and the Menu key; keyboard accelerators for frequent commands, shown in tooltips and menus.
- **Keyboard:** Tab and Shift+Tab between controls, arrow keys inside lists and grids, F6 between the panes of a window, Esc closes flyouts and dialogs, access keys with Alt where the app is used heavily by keyboard.
- **Dialogs:** `ContentDialog` only for decisions that block the next step. Information and status belong inline, for example in an `InfoBar`.
- **Settings:** a settings page inside the app, built from standard controls.
- **Notifications:** only for things the user asked to follow.
- **Touch and pen:** Windows devices may have touch; follow Microsoft's guidance on touch targets.
- **Writing:** follow the Microsoft Writing Style Guide, including its capitalization rules.

## Leonardo on Windows

- **Ground:** `MicaBackdrop`. Nothing custom.
- **Tools:** standard WinUI controls. Acrylic only where Fluent uses it.
- **Content:** Leonardo resources from `UpInBlue.Leonardo.WinUI`: merge `Themes/Leonardo.xaml` after `XamlControlsResources`; use the text styles `LeonardoPlaceNameTextStyle`, `LeonardoViewTextStyle`, `LeonardoSectionTextStyle`, `LeonardoBodyTextStyle`, `LeonardoBodyStrongTextStyle`, `LeonardoMetaTextStyle`; spacing, radii, colors and motion only through `Leonardo…` resources.
- **Controls Leonardo adds:** `PlaceHeader`, `SummaryLine`, `StatusDot`. Use them instead of building your own versions.
- **Motion:** the helpers in `UpInBlue.Leonardo.WinUI.Motion` for the seven motions. They already respect the system animation setting.

Until these packages exist in a repository, use the token values from `leonardo.tokens.json` as XAML resources and the standard WinUI controls.

## WPF

- The platform is WPF with its built-in Fluent theme (`ThemeMode`).
- Leonardo comes from `UpInBlue.Leonardo.Wpf`: colors, spacing, type roles and motion values as resources. There are no Leonardo controls for WPF; build with standard WPF controls.
- Call `LeonardoTheme.Apply(app, ThemeMode.System)` (or `Light`, `Dark`) in `App.OnStartup` instead of setting `ThemeMode` yourself. It merges `Themes/Leonardo.Light.xaml`, `Leonardo.Dark.xaml` or `Leonardo.HighContrast.xaml`, gives the Fluent controls the Leonardo accent, and follows theme and contrast changes.
- Reference Leonardo colors with `DynamicResource`, so they follow the theme. The text styles have the same names as in WinUI, for example `LeonardoBodyTextStyle`.
- Motion: `LeonardoMotion.Stagger` for the entrance of a view; it respects the system animation setting. Build other motions from the `Leonardo…` easing (`KeySpline`) and duration (`Duration`, `KeyTime`, `TimeSpan`) resources, and check `LeonardoMotion.AnimationsEnabled` first.
- The same conventions as above apply where WPF offers the equivalent.

## Never on Windows

- Hamburger menus that hide primary navigation on wide windows.
- Custom window chrome that breaks Snap layouts, dragging or caption buttons.
- Mac or iOS patterns such as bottom tab bars, swipe-only actions or sheets sliding from the bottom.
