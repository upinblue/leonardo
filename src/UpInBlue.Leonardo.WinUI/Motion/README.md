# UpInBlue.Leonardo.WinUI.Motion

The seven motions of the Leonardo design language for WinUI 3. Every value comes from the tokens
(`LeonardoTokens.Motion`); nothing here has its own durations or curves.

| Motion | Helper | Means | Fallback with animations off |
| --- | --- | --- | --- |
| Dive in | `DiveIn.Prepare`, `DiveIn.Start`, `DiveIn.PrepareBack`, `DiveIn.StartBackAsync` | `ConnectedAnimationService`, basic configuration, enter curve, `motion.duration.long`; the rest of the page with `Stagger` | Place name and rest crossfade in |
| Transform | `Transform.Run(source, target, change)` | `ConnectedAnimationService`, basic configuration, move curve, `motion.duration.normal` | Target crossfades in |
| Stagger | `Stagger.Enter(groups, direction)` | On navigation to a view, never at app launch. Composition: translation with the enter curve, opacity linear with `motion.duration.fade`, `motion.stagger.step` apart, at most `motion.stagger.maxGroups` | Groups crossfade in together |
| Reflow | `Reflow.Capture(lists)` then `snapshot.Play()` | Composition: translation from the old place with the move curve, `normal` or `long` | Moved items crossfade |
| Arrive | `Arrive.Run(item, marker, announcement)` | Item fades in; the marker is a short accent bar at the start of the row (`LeonardoArriveMarkerStyle`), and its opacity is a Storyboard: in with `fade`, holds `motion.arrive.hold`, out within `long`; UI Automation notification | Bar appears at once, leaves with `fade` |
| Leave | `Leave.Run(element)`, `Leave.RunAsync(element, remove)` | Composition: translation with the exit curve in `motion.duration.short`, fade during the last `fade` | Fade only |
| Respond | none | The platform's pressed and released states of every control | – |

**Respond** has no helper on purpose. Pressing starts the response, and the platform defines what pressing looks like:
Fluent controls react on press and on release. Never add press animations of your own.

## Rules the helpers follow

- `MotionSettings` wraps `UISettings.AnimationsEnabled`. Call `MotionSettings.Initialize()` on the UI thread at start-up.
  `AnimationsOverride` simulates animations on or off for testing; `Speed` slows all motion down to one tenth for inspection.
- Every helper returns a `MotionRun`. `SkipToEnd()` jumps to the end state. `MotionInterruption.Attach(root)` makes
  every click, tap or key press end all running motion, so motion never blocks.
- Entrances use `motion.duration.normal` or `long`, never `short`: leaving MUST be shorter than arriving, and
  `motion.duration.short` (167 ms) is for exits only.
- Stagger runs when the user navigates to a view. The first view at app launch appears without it, because an entrance
  at launch is decorative: don't call `Stagger.Enter` or `DiveIn.Start` for it.
- The Arrive marker is an accent bar, never a dot: the dot shape is reserved for the `StatusDot`. Put a `Border` with
  `LeonardoArriveMarkerStyle` into the row's leading padding, before the icon, and pass it to `Arrive.Run`:
  `<Border x:Name="ArriveMarker" Style="{StaticResource LeonardoArriveMarkerStyle}" />`.
- Lists animated with `Reflow` or `Leave` turn off their own `ItemContainerTransitions`, so the platform does not animate
  the same change a second time.
- Factories for your own animations: `MotionCurves.Create(compositor, easing)` for Composition,
  `MotionCurves.CreateKeySpline(easing)` and `MotionTiming.CreateDoubleAnimation(...)` for Storyboards, and
  `MotionTiming.Get(duration)` for durations that follow the slow-down.
