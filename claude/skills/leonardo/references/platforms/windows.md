# Windows: WinUI 3 and WPF

A Windows app behaves like a Windows app. Users bring habits from File Explorer, Outlook, Settings and Office; respect them.

## Research first

Look up in the Windows app design guidance (see `sources.md`) the pages for the patterns you are about to use, typically: navigation basics, command design basics, the specific controls, dialogs and flyouts, title bar, motion ("Motion in Windows"), accessibility, and the Microsoft Writing Style Guide for text.

## Platform conventions to respect

- **Window:** standard caption buttons; a custom title bar only as the guidelines describe it; windows resize freely and work with Snap layouts; sensible minimum sizes.
- **Navigation:** `NavigationView` for top-level areas; a `BreadcrumbBar` where users navigate hierarchies; back navigation where the user dives in.
- **Commands:** `CommandBar` for the commands of the current view, always visible; context menus on right click, Shift+F10 and the Menu key; keyboard accelerators for frequent commands, shown in tooltips and menus.
- **Keyboard:** Tab and Shift+Tab between controls, arrow keys inside lists and grids, F6 between the panes of a window, Esc closes flyouts and dialogs, access keys with Alt where the app is used heavily by keyboard.
- **Dialogs:** `ContentDialog` only for decisions that block the next step. Information and status belong inline, for example in an `InfoBar` or next to the object.
- **Settings:** a settings page inside the app, built from standard controls.
- **Notifications:** only for things the user asked to follow.
- **Touch and pen:** Windows devices may have touch; follow Microsoft's guidance on touch targets.
- **Writing:** follow the Microsoft Writing Style Guide, including sentence-style capitalization.

## Leonardo on Windows: the division of labor

| Layer | What to use |
| --- | --- |
| Ground | `MicaBackdrop` on the window. Nothing custom. |
| Tools | Standard WinUI controls, unchanged: `NavigationView`, `CommandBar`, `ListView`, `TreeView`, `ContentDialog`, menus, flyouts. Acrylic only where Fluent itself uses it. |
| Content | Leonardo resources, text styles and the three Leonardo controls from `UpInBlue.Leonardo.WinUI`. |

Never restyle or rebuild a Fluent control. Leonardo changes exactly one Fluent resource family: the accent ramp (`SystemAccentColor` and its Light1–3 and Dark1–3 steps), so accent fills are brand blue in Light and far in Dark.

## WinUI 3: setup

Merge Leonardo after `XamlControlsResources`, then your app's own resources (from the gallery's `App.xaml`):

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            <ResourceDictionary Source="ms-appx:///UpInBlue.Leonardo.WinUI/Themes/Leonardo.xaml" />
            <ResourceDictionary Source="/Resources/App.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

The window (from the gallery's `MainWindow`):

```xml
<Window.SystemBackdrop>
    <MicaBackdrop />
</Window.SystemBackdrop>
```

```csharp
// In the window constructor, on the UI thread:
MotionSettings.Initialize();          // follows "Animation effects" in Windows Settings
MotionInterruption.Attach(Root);      // any click, tap or key press ends running motion
```

## WinUI 3: resource keys

All keys start with `Leonardo`. Colors are theme resources (Light, Dark, and HighContrast, which maps every key to a system color); everything else is static.

| Kind | Keys | Use with |
| --- | --- | --- |
| Theme colors | `LeonardoAccentBrush`, `LeonardoOnAccentBrush`, `LeonardoLinkBrush`, `LeonardoSelectionBrush`, `LeonardoSurfaceBrush`, `LeonardoSurfaceAltBrush`, `LeonardoTextPrimaryBrush`, `LeonardoTextSecondaryBrush`, `LeonardoTextTertiaryBrush`, `LeonardoStrokeSubtleBrush`, `LeonardoStrokeStrongBrush`, `LeonardoChartBrush`; each also as `…Color` | `{ThemeResource …}` |
| Chart | `LeonardoChartBrush`: the one blue of a single-series chart (near in Light, far in Dark, `SystemColorHighlightColor` in a contrast theme) | `{ThemeResource …}` on the series, never on text or surfaces |
| Status colors | None in Leonardo. Use Fluent's own: `SystemFillColorCriticalBrush` (broken, overdue), `SystemFillColorCautionBrush` (attention), `SystemFillColorSuccessBrush` (briefly confirmed), as `StatusDot` and `InfoBar` use them. | `{ThemeResource …}` |
| Blue scale | `LeonardoPaletteDepthColor`, `…NearColor`, `…BrandColor`, `…FarColor`, `…HazeColor`, `…SkyColor`, `…WhiteColor` | `{StaticResource …}`, only inside your own theme dictionaries (charts with several series), never directly on an element |
| Spacing (`x:Double`) | `LeonardoSpaceXS` 4, `LeonardoSpaceS` 8, `LeonardoSpaceM` 16, `LeonardoSpaceL` 24, `LeonardoSpaceXL` 40 | `Spacing`, `ColumnSpacing`, `RowSpacing` |
| Spacing (`Thickness`) | `LeonardoSpace{XS,S,M,L,XL}Thickness`, and `…LeftThickness`, `…TopThickness`, `…RightThickness`, `…BottomThickness`, `…HorizontalThickness`, `…VerticalThickness` | `Margin`, `Padding` |
| Radii (`CornerRadius`) | `LeonardoRadiusS`, `LeonardoRadiusM`, `LeonardoRadiusL` | `CornerRadius`; concentric rule: inner = outer − padding |
| Density (`x:Double`) | `LeonardoDensityCompact` 32, `LeonardoDensityStandard` 40, `LeonardoDensityComfortable` 48 | Row heights |
| Sizes (`x:Double`) | `LeonardoSizeIcon` 16, `LeonardoSizeStatusDot` 12 | Icons in list rows and next to labels; the status dot and the status column of a row |
| Text styles | `LeonardoPlaceNameTextStyle`, `LeonardoViewTextStyle`, `LeonardoSectionTextStyle`, `LeonardoBodyTextStyle`, `LeonardoBodyStrongTextStyle`, `LeonardoMetaTextStyle`, and `LeonardoBodyTabularTextStyle`, `LeonardoMetaTabularTextStyle` for numbers in columns | `Style` on `TextBlock` |
| Type values | `Leonardo{PlaceName,View,Section,Body,Meta}{FontFamily,FontSize,LineHeight}` | Only where a text style cannot be used |
| Motion | `LeonardoEaseEnter`, `LeonardoEaseExit`, `LeonardoEaseMove`, `LeonardoEaseFade` (KeySpline strings); `LeonardoDurationFade`, `…Short`, `…Normal`, `…Long` (`x:String` such as `0:0:0.25`, like Fluent's own motion resources, so they convert to `KeyTime`, `Duration` and `BeginTime`); `LeonardoArriveHold`; `LeonardoStaggerStep`, `LeonardoStaggerMaxGroups`; `LeonardoDistanceEnterMin`, `…Max`; `LeonardoParallaxPlaceNameFactor` | Storyboards that the motion helpers do not cover |

The Leonardo packages have no status color resources. Windows draws status in Fluent's colors; only the standalone web and presentations have Leonardo status colors.

Known issue: the Leonardo text styles set `Foreground`. In a contrast theme, text in a selected or hovered `ListView` row then keeps the window text color instead of switching to the highlight text color (Microsoft, "Contrast themes", list items with colored text). Check selected rows in a contrast theme, and until the library changes, use a row text style without `Foreground` where this happens. Say so in the pull request.

In code, the same values are in the generated static class `UpInBlue.Leonardo.WinUI.LeonardoTokens` (`LeonardoTokens.Space.M`, `LeonardoTokens.Motion.DurationNormal`, `LeonardoTokens.Light.Accent` …). Use it only where XAML resources are not reachable; in high contrast, colors come from the system, not from `LeonardoTokens`.

Layout sizes Leonardo does not define (pane widths, column widths) go into the app's own resource dictionary with a name and a comment, never inline. The gallery's `Resources/Gallery.xaml` is the model:

```xml
<!-- Asymmetric balance: navigation fixed, content flexible, details fixed. -->
<GridLength x:Key="GalleryTreePaneWidth">200</GridLength>
<GridLength x:Key="GalleryDetailsPaneWidth">280</GridLength>
```

## Page margin and alignment line

They are two different things. The page margin is the distance from the content edge to the content; the alignment line is the left edge on which text starts.

- **Page margin:** `LeonardoSpaceL` (24), for example `Padding="{StaticResource LeonardoSpaceLHorizontalThickness}"` on the page content.
- **Pages with icon lists** (rows with an icon and maybe a status dot before the text): the alignment line lies at `LeonardoSpaceXL` (40) from the content edge. `PlaceHeader`, headings, column headers and running text start on it (`LeonardoSpaceXLHorizontalThickness`); icons and status dots hang to the left of it. Hover and selection backplates of the rows may reach out to `LeonardoSpaceXS` from the content edge.
- **Pages without such lists** (settings, forms, the overview): the page margin `LeonardoSpaceL` is the alignment line.

The Files page of the gallery shows the arithmetic: the `ListView` has a left padding of `LeonardoSpaceXS`, its items a horizontal padding of `LeonardoSpaceXS`, then the icon (`LeonardoSizeIcon`), the status column (`LeonardoSizeStatusDot`) and `LeonardoSpaceXS` to the name: 4 + 4 + 16 + 12 + 4 = 40.

## WinUI 3: correct and incorrect

### Values come from resources

Correct (gallery, Files page, details pane):

```xml
<ScrollViewer x:Name="DetailsPane" Grid.Column="2" AutomationProperties.Name="Details"
              Background="{ThemeResource LeonardoSurfaceAltBrush}">
    <StackPanel Padding="{StaticResource LeonardoSpaceLThickness}" Spacing="{StaticResource LeonardoSpaceM}">
        <TextBlock AutomationProperties.HeadingLevel="Level2"
                   Style="{StaticResource LeonardoSectionTextStyle}" Text="{x:Bind ViewModel.SelectedEntry.Name, Mode=OneWay}" />
        <TextBlock Style="{StaticResource LeonardoMetaTextStyle}" Text="{x:Bind ViewModel.SelectedEntry.Kind, Mode=OneWay}" />
    </StackPanel>
</ScrollViewer>
```

Incorrect:

```xml
<!-- Literal color, spacing and font size; StaticResource for a color, so it ignores theme changes and high contrast. -->
<ScrollViewer Background="#F3F7FC">
    <StackPanel Padding="24" Spacing="15">
        <TextBlock FontSize="20" FontWeight="SemiBold" Foreground="{StaticResource LeonardoTextPrimaryBrush}" Text="…" />
    </StackPanel>
</ScrollViewer>
```

### Hierarchy from type, not from frames

Correct: a section heading in `LeonardoSectionTextStyle` with white space (`LeonardoSpaceM` between groups, `LeonardoSpaceXL` between place name and content). Incorrect: a `Border` with a background or a line under a heading to create hierarchy; a card around every group. Frames must justify their place, for example a details pane with `LeonardoSurfaceAltBrush`.

### Numbers in columns

Correct: `Style="{StaticResource LeonardoBodyTabularTextStyle}"` for dates, sizes and amounts in a table, right-aligned with their header. Incorrect: `LeonardoBodyTextStyle` for numbers in columns, or a monospace font.

### Accent

Correct: `Style="{StaticResource AccentButtonStyle}"` on the one primary action of a view; the accent ramp makes it brand blue. Checked toggle buttons are selection, not actions: the active phrases of a `SummaryLine` or a checked `ToggleButton` filter may show the accent fill Fluent gives them, several at once, next to the one primary action. Incorrect: `Background="{ThemeResource LeonardoAccentBrush}"` on a `Button`, a heading or a surface; a second `AccentButtonStyle` action in the same view.

### Charts

Correct: a single-series chart draws its series with `{ThemeResource LeonardoChartBrush}`; in a contrast theme it becomes the system highlight color on its own. Incorrect: `LeonardoAccentBrush` or a blue scale color directly on the series, or a status color for data that is not a status.

### Do not restyle Fluent

Incorrect, from any Leonardo app:

```xml
<!-- Never: a new template for a Fluent control, a removed focus visual, a custom glass. -->
<Style TargetType="Button"><Setter Property="Template">…</Setter></Style>
<Button UseSystemFocusVisuals="False" FocusVisualPrimaryThickness="0" />
<Border><Border.Background><AcrylicBrush TintOpacity="0.4" /></Border.Background></Border>
```

Allowed: a style `BasedOn` the Fluent default that sets layout only, as the gallery's list item style does:

```xml
<Style x:Key="GalleryListViewItemStyle" TargetType="ListViewItem" BasedOn="{StaticResource DefaultListViewItemStyle}">
    <Setter Property="MinHeight" Value="{StaticResource LeonardoDensityCompact}" />
</Style>
```

## WinUI 3: the three Leonardo controls

```xml
xmlns:leonardo="using:UpInBlue.Leonardo.WinUI.Controls"
```

Use these instead of building your own versions. Everything else is a Fluent control.

### PlaceHeader

The place name: a meta line, the title (exposed as a level 1 heading) and a content slot for a `SummaryLine`. It sits above the scrolling content; `ScrollSource` points at the `ScrollViewer` or a list that contains one. The header follows the scroll position: the meta line fades, and the title moves up more slowly than the content while it scales to a compact line; scrolling back reverses it. It reserves the shrink distance at the top of the scrolling content itself, so never add that space by hand. With animations off: nothing scales or fades, and the compact title crossfades in. Correct (gallery, Files page):

```xml
<leonardo:PlaceHeader x:Name="PlaceHeader" Grid.Row="2"
                      Margin="{StaticResource LeonardoSpaceXLHorizontalThickness}"
                      ScrollSource="{x:Bind FileList}"
                      Title="{x:Bind ViewModel.Folder.Name}">
    <leonardo:SummaryLine Margin="{StaticResource LeonardoSpaceSTopThickness}"
                          Items="{x:Bind ViewModel.Summary}"
                          Prefix="Since your last visit:" />
</leonardo:PlaceHeader>
<ListView x:Name="FileList" Grid.Row="4" Margin="{StaticResource LeonardoSpaceXLTopThickness}" … />
```

Incorrect: a `TextBlock` with `FontSize="40"` as the page title; a second large title inside a page that already has a `PlaceHeader`; a `PlaceHeader` without `ScrollSource` above long content.

### SummaryLine

What just happened, as one sentence: "Since your last visit: 2 new · 1 checked out · 1 conflict". `Items` holds `SummaryLineItem`s (`Count`, `Kind` New, CheckedOut, Conflict or Custom, `Label` and `PluralLabel` for custom phrases, `IsActive`, `Command`). Each phrase is a toggle filter; phrases with a count of 0 are left out, and the control collapses when all counts are 0. Screen readers read the whole sentence. Incorrect: a row of badges or colored chips, or an `InfoBar` for this information.

### StatusDot

`Kind` is a `StatusKind`. Choose it by meaning, not by the name of the file scenario:

| `StatusKind` | Shape and color | Meaning | Examples |
| --- | --- | --- | --- |
| `New` | Filled dot in the accent color | New since the last visit | A new file, a new approval request |
| `CheckedOut` | Ring in the system caution color (`SystemFillColorCautionBrush`) | Attention, not blocked | Checked out by someone else, a contract that expires soon |
| `Conflict` | Circle with exclamation mark in the system critical color (`SystemFillColorCriticalBrush`) | Something is broken or overdue | A sync conflict, an overdue invoice |

`Description` is required: it is the tooltip and the accessible name, and it says the specific state ("Expires on 3 March"). In rows the dot hangs left of the alignment line, between icon and name, in a column of `LeonardoSizeStatusDot` (see "Page margin and alignment line" for the arithmetic). Correct (gallery, Files page row):

```xml
<Grid Grid.Column="1" Width="{StaticResource LeonardoSizeStatusDot}"
      Margin="{StaticResource LeonardoSpaceXSTopThickness}" VerticalAlignment="Top">
    <leonardo:StatusDot Description="{x:Bind StatusDescription}" Kind="{x:Bind StatusValue}"
                        Visibility="{x:Bind helpers:Show.When(HasStatus)}" />
</Grid>
```

The row itself carries the status in its accessible name (`AutomationProperties.Name="{x:Bind AccessibleName}"`, for example "Price list 2027.xlsx, checked out by Anna Berg"), and the details pane repeats it as text. Incorrect:

```xml
<!-- Color alone, no shape difference, no name, a literal size. -->
<Ellipse Width="8" Height="8" Fill="Orange" />
<leonardo:StatusDot Kind="CheckedOut" />   <!-- Description missing -->
```

## WinUI 3: motion helpers

Namespace `UpInBlue.Leonardo.WinUI.Motion`. Every helper takes its values from the tokens, returns a `MotionRun` that can `SkipToEnd()`, and falls back to a crossfade with `motion.duration.fade` when animations are off in Windows. Never write your own durations or curves.

| Motion | Helper | Gallery example |
| --- | --- | --- |
| Dive in | `DiveIn.Prepare(list, item, elementName)` before navigating; `DiveIn.Start(placeName, rest)` on the target page; `DiveIn.PrepareBack(placeName)` and `await DiveIn.StartBackAsync(list, item, elementName, rest)` for back; `DiveIn.Cancel()` | Files: a folder name flies up and becomes the `PlaceHeader` title |
| Transform | `Transform.Run(source, target, change)` | Approvals: the Approve button becomes the decision panel |
| Stagger | `Stagger.Enter(groups, direction)`, at most `motion.stagger.maxGroups` groups in reading order, only when the user navigates to a view; never at app launch, where the first view appears without it | Files: jumping through the tree |
| Reflow | `var reflow = Reflow.Capture(list); change(); reflow.Play();` | Customers: sorting; Files: delete and undo |
| Arrive | `Arrive.Run(item, marker, announcement)`; the marker is a short accent bar at the start of the row, fully visible for `LeonardoArriveHold`, then fading out; the announcement is a UI Automation notification. Never a dot as marker: the dot shape belongs to `StatusDot`. | Activity: a new event with an accent bar |
| Leave | `await Leave.RunAsync(element, remove)` | Files: a deleted row leaves, the rest reflows |
| Respond | None. The platform's pressed states are the response. | – |

Correct (gallery, Files page, delete):

```csharp
await Leave.RunAsync(container, () =>
{
    var reflow = Reflow.Capture(FileList);
    ViewModel.DeleteFile(entry);
    reflow.Play();
});
```

with the platform's item transitions turned off on that list, so the change is not animated twice:

```xml
<ListView.ItemContainerTransitions>
    <TransitionCollection />
</ListView.ItemContainerTransitions>
```

Incorrect:

```csharp
// Literal duration, linear movement, no fallback when animations are off, cannot be interrupted.
var animation = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(400) };
```

For a custom animation the helpers do not cover, use the factories: `MotionCurves.Create(compositor, MotionEasing.Enter)` for Composition, `MotionCurves.CreateKeySpline(…)` and `MotionTiming.CreateDoubleAnimation(…)` for Storyboards, and check `MotionSettings.AnimationsEnabled`. In XAML Storyboards use `KeySpline="{StaticResource LeonardoEaseEnter}"` and `KeyTime="{StaticResource LeonardoDurationNormal}"`. Entrances use `normal` or `long`; `LeonardoDurationShort` (167 ms) is for exits only, never for an entrance.

## WPF

The platform is WPF with its built-in Fluent theme (`Application.ThemeMode`). `UpInBlue.Leonardo.Wpf` adds the same token resources and text styles as WinUI, and the accent override for the Fluent theme. There are no Leonardo controls and no motion helpers beyond the stagger for WPF; build with standard WPF controls.

### Setup

Do not merge the Leonardo dictionaries by hand. `LeonardoTheme.Apply` sets `ThemeMode`, merges `Leonardo.Light.xaml`, `Leonardo.Dark.xaml` or `Leonardo.HighContrast.xaml`, and swaps them when the Windows app mode or a contrast theme changes (from the WPF sample's `App.xaml.cs`):

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    LeonardoTheme.Apply(this, ThemeMode.System);   // Light, Dark or System; None is not supported
    // …
}
```

### Resource keys and the WPF difference

The keys are the same as in WinUI (`LeonardoSpaceMThickness`, `LeonardoRadiusS`, `LeonardoBodyTextStyle`, `LeonardoTextPrimaryBrush`, `LeonardoChartBrush`, `LeonardoSizeIcon`, `LeonardoSizeStatusDot` …); in a contrast theme `LeonardoChartBrush` is `SystemColors.Highlight`. Like WinUI, the WPF package has no status colors: use the Fluent theme's `SystemFillColorCriticalBrush`, `SystemFillColorCautionBrush` and `SystemFillColorSuccessBrush` with `DynamicResource`. Durations also exist as `LeonardoDuration{Fade,Short,Normal,Long}KeyTime` and `…TimeSpan`. Because the color dictionaries are swapped at run time, colors MUST be `DynamicResource`; spacing, radii and text styles may stay `StaticResource`. Correct (WPF sample):

```xml
<Border Padding="{StaticResource LeonardoSpaceLVerticalThickness}">
    <TextBlock AutomationProperties.HeadingLevel="Level1"
               Style="{StaticResource LeonardoPlaceNameTextStyle}" Text="Foundations" />
</Border>
<Border BorderBrush="{DynamicResource LeonardoStrokeSubtleBrush}"
        CornerRadius="{StaticResource LeonardoRadiusS}" />
<Button Content="Renew contract" Style="{DynamicResource AccentButtonStyle}" />
```

Incorrect: `BorderBrush="{StaticResource LeonardoStrokeSubtleBrush}"` (stays in the old theme after a switch), `Background="#1B6CE0"`, a custom `ControlTemplate` for `Button`, or `FocusVisualStyle="{x:Null}"`.

### Motion in WPF

`LeonardoMotion.AnimationsEnabled` reads the Windows setting "Animation effects" (`SystemParameters.ClientAreaAnimation`) and follows its changes. `LeonardoMotion.Stagger(groups)` lets up to four groups enter in reading order, or crossfades them together when animations are off. Call it when the user navigates to a view, not for the first window at launch. For other animations, `LeonardoMotion.Spline(from, to, begin, duration, controlPoint1, controlPoint2)` builds a key frame animation from token values, for example `LeonardoTokens.Motion.DurationNormal` and `LeonardoTokens.Motion.EaseEnterControlPoint1`. Correct (WPF sample): `LeonardoMotion.Stagger([HeaderGroup, ColorGroup, TypographyGroup, DensityGroup]);`

## Before the pull request (Windows)

- [ ] `App.xaml` merges `Leonardo.xaml` after `XamlControlsResources`; the window uses `MicaBackdrop` and calls `MotionSettings.Initialize()` and `MotionInterruption.Attach(…)`. Check this in an existing app shell too, not only in new pages.
- [ ] Page margin `LeonardoSpaceL`; on pages with icon lists, place name, headings and list text share the alignment line at `LeonardoSpaceXL`.
- [ ] One accent surface for actions per view; checked toggles count as selection. Status in Fluent's status brushes, never in Leonardo colors.
- [ ] Stagger only on navigation, not at launch; `LeonardoDurationShort` only for exits; the Arrive marker is a bar.
- [ ] Light, Dark and a contrast theme (Settings › Accessibility › Contrast themes).
- [ ] Animation effects off (Settings › Accessibility › Visual effects): every motion is a crossfade.
- [ ] Transparency effects off: Mica becomes opaque, the layout stays readable.
- [ ] Text size at 225 % (Settings › Accessibility › Text size): text wraps, nothing clips.
- [ ] Keyboard only, including F6 between panes, and Narrator on the main task.
- [ ] No literal colors, spacing, font sizes, radii, durations or easing curves outside generated files.

## Never on Windows

- Hamburger menus that hide primary navigation on wide windows.
- Custom window chrome that breaks Snap layouts, dragging or caption buttons.
- Mac or iOS patterns such as bottom tab bars, swipe-only actions or sheets sliding from the bottom.
- Restyled Fluent controls, removed focus visuals, custom glass.
