# UpInBlue.Leonardo.WinUI.Controls

The three controls Leonardo adds to WinUI. Everything else is a Fluent control, unchanged. The default styles are in
`Themes/Generic.xaml` and need the resources of `Themes/Leonardo.xaml`, which apps merge after `XamlControlsResources`.

```xml
xmlns:leonardo="using:UpInBlue.Leonardo.WinUI.Controls"
```

## PlaceHeader

The place name of a view: meta line, title (level 1 heading), and a content slot for a `SummaryLine`.

```xml
<leonardo:PlaceHeader Title="Contracts" Meta="Hofmann Logistics" ScrollSource="{x:Bind FileList}">
    <leonardo:SummaryLine Items="{x:Bind ViewModel.Summary}" Prefix="Since your last visit:" />
</leonardo:PlaceHeader>
<ListView x:Name="FileList" Grid.Row="1" … />
```

- Put it above the scrolling content and set `ScrollSource` to the `ScrollViewer` or an element that contains one.
- While the content scrolls, the title moves up at `motion.parallax.placeNameFactor` of the content speed. Further
  down, the header shrinks to a compact line (section role), and the scroll offset is corrected so the content does not
  jump. At the top it grows again. With animations off: no parallax, no animation.
- `TitleElement` is the element that shows the title now, for `DiveIn`.

## SummaryLine

What just happened, as one sentence: "Since your last visit: 2 new · 1 checked out · 1 conflict".

- `Items` holds `SummaryLineItem`s with `Count`, `Kind` (New, CheckedOut, Conflict, Custom), `Label` and
  `PluralLabel` for custom phrases, `IsActive`, and `Command` with `CommandParameter` (the item itself when null).
- Every phrase is a standard `ToggleButton`. Toggling, by pointer, keyboard or UI Automation, sets `IsActive`, then runs `Command`.
- Several phrases can be active at once. Checked toggle buttons count as selection, not as actions, so the rule of one
  accent surface per view does not apply to them. The control never turns a phrase off by itself; the app decides how
  active filters combine, usually by showing what matches any of them.
- Phrases with a count of 0 are left out; the control collapses when all counts are 0.
- Screen readers read the whole sentence as the name of a group ("…: 2 new, 1 checked out, 1 conflict"). Changes are
  announced politely.
- The sentence logic is in `SummaryLineText`, which is unit tested.

## StatusDot

Status next to an item: `Kind` New (accent dot), CheckedOut (ring in Fluent's caution color) or Conflict (circle
with an exclamation mark in Fluent's critical color).

- The kinds map by meaning: CheckedOut means attention, for example a file someone else holds or a request that
  waits; Conflict means broken or overdue. Say the actual state in `Description`.
- The dot shape is reserved for the status dot. The Arrive marker is an accent bar, see `Motion/README.md`.

- `Description` is required: it is the tooltip and the accessible name, for example "Checked out by Anna Berg".
- The dot is `LeonardoSizeStatusDot` (`size.statusDot`, 12) in diameter. In rows, it hangs between the icon and the
  name, in a column as wide as the dot: list offset `space.xs` 4 + item padding `space.xs` 4 + icon
  `LeonardoSizeIcon` 16 + dot 12 + `space.xs` 4 = 40, the alignment line at `space.xl`. That keeps it clear of the
  list's selection indicator.
- The dot is not focusable. Put the status into the row's accessible name and show it in the details as text.
