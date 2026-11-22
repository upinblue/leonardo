using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using UpInBlue.Leonardo.Gallery.Models;
using UpInBlue.Leonardo.WinUI;
using UpInBlue.Leonardo.WinUI.Motion;

namespace UpInBlue.Leonardo.Gallery.ViewModels;

/// <summary>
/// How the gallery runs motion: following Windows, or simulating animation effects turned off or on.
/// </summary>
public enum AnimationMode
{
    /// <summary>Follow "Animation effects" in Windows Settings.</summary>
    FollowWindows,

    /// <summary>Simulate animation effects turned off: every motion becomes a crossfade.</summary>
    SimulateOff,

    /// <summary>Simulate animation effects turned on, to inspect the motion where Windows has them off.</summary>
    SimulateOn,
}

/// <summary>
/// An easing curve with its token, drawn on the Motion page.
/// </summary>
public sealed record EasingSample(string Name, string Token, MotionEasing Easing, string Use)
{
    /// <summary>The curve in CSS notation, for example "cubic-bezier(0, 0, 0, 1)".</summary>
    public string Value
    {
        get
        {
            var (first, second) = MotionCurves.GetControlPoints(Easing);
            return string.Create(CultureInfo.InvariantCulture, $"cubic-bezier({first.X}, {first.Y}, {second.X}, {second.Y})");
        }
    }

    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Name}, {Value}, {Use}";
}

/// <summary>
/// A timing value of the motion tokens.
/// </summary>
public sealed record TimingSample(string Name, string Token, string ValueText, string Use)
{
    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Name}, {ValueText}, {Use}";
}

/// <summary>
/// A file in the Reflow demo.
/// </summary>
public sealed record DemoFile(string Name, long Size)
{
    /// <summary>The size for display.</summary>
    public string SizeText => Formats.Size(Size);
}

/// <summary>
/// An event in the Arrive and Leave demo.
/// </summary>
public sealed record DemoEvent(string Text, string Meta)
{
    /// <summary>The text a screen reader reads.</summary>
    public string AccessibleName => $"{Text} {Meta}";
}

/// <summary>
/// The Motion page: the settings for inspecting motion, the curves and values, and one demo per motion.
/// </summary>
public sealed partial class MotionViewModel : ObservableObject, IDisposable
{
    private static readonly (string Text, string Meta)[] NewEvents =
    [
        ("Lea Schmid uploaded Delivery note 4471.pdf.", "Hofmann Logistics / Invoices · just now"),
        ("Tim Kraus commented on Cost estimate.xlsx.", "Brenner Industries / Plant expansion · just now"),
        ("Mara Lindqvist shared Tender documents.zip.", "Kessler Construction / Tenders · just now"),
        ("Felix Okafor changed Onboarding checklist.docx.", "Mertens Medical · just now"),
    ];

    private int _nextEvent;

    /// <summary>
    /// Creates the view model with the current motion settings.
    /// </summary>
    public MotionViewModel()
    {
        AnimationModeIndex = MotionSettings.AnimationsOverride switch
        {
            false => (int)AnimationMode.SimulateOff,
            true => (int)AnimationMode.SimulateOn,
            null => (int)AnimationMode.FollowWindows,
        };
        SlowDown = Math.Round(MotionMath.NormalSpeed / MotionSettings.Speed);
        MotionSettings.Changed += OnMotionSettingsChanged;
    }

    /// <summary>The three animation modes, as shown in the radio buttons.</summary>
    public IReadOnlyList<string> AnimationModes => ["Follow Windows", "Simulate turned off", "Simulate turned on"];

    /// <summary>The selected animation mode as an index into <see cref="AnimationModes"/>.</summary>
    [ObservableProperty]
    public partial int AnimationModeIndex { get; set; }

    /// <summary>How many times slower motion runs: 1 (normal) to 10 (one tenth of the speed).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlowDownText))]
    public partial double SlowDown { get; set; }

    /// <summary>The smallest slow-down factor.</summary>
    public double SlowDownMinimum => MotionMath.NormalSpeed;

    /// <summary>The largest slow-down factor: one tenth of the speed.</summary>
    public double SlowDownMaximum => MotionMath.NormalSpeed / MotionMath.MinimumSpeed;

    /// <summary>"Normal speed" or "4 times slower".</summary>
    public string SlowDownText => SlowDown <= 1 ? "Normal speed" : string.Create(CultureInfo.InvariantCulture, $"{SlowDown:0} times slower");

    /// <summary>What Windows says, for example "Animation effects in Windows: Off. The gallery simulates them turned on."</summary>
    public string AnimationState
    {
        get
        {
            var windows = MotionSettings.SystemAnimationsEnabled ? "On" : "Off";
            var effect = MotionSettings.AnimationsEnabled ? "Motion runs." : "Every motion becomes a crossfade.";
            return $"Animation effects in Windows: {windows}. {effect}";
        }
    }

    /// <summary>The easing curves.</summary>
    public IReadOnlyList<EasingSample> Curves { get; } =
    [
        new("Enter", "motion.easing.enter", MotionEasing.Enter, "Arrive, dive in"),
        new("Exit", "motion.easing.exit", MotionEasing.Exit, "Leave, always with a fade"),
        new("Move", "motion.easing.move", MotionEasing.Move, "Transform, reflow"),
        new("Fade", "motion.easing.fade", MotionEasing.Fade, "Opacity only"),
    ];

    /// <summary>The durations, offsets and distances.</summary>
    public IReadOnlyList<TimingSample> Timing { get; } =
    [
        new("Fade", "motion.duration.fade", Milliseconds(LeonardoTokens.Motion.DurationFade), "Opacity"),
        new("Short", "motion.duration.short", Milliseconds(LeonardoTokens.Motion.DurationShort), "Leave"),
        new("Normal", "motion.duration.normal", Milliseconds(LeonardoTokens.Motion.DurationNormal), "Stagger, transform, reflow"),
        new("Long", "motion.duration.long", Milliseconds(LeonardoTokens.Motion.DurationLong), "Dive in, long reflows"),
        new("Stagger step", "motion.stagger.step", Milliseconds(LeonardoTokens.Motion.StaggerStep), string.Create(CultureInfo.InvariantCulture, $"Between groups, at most {LeonardoTokens.Motion.StaggerMaxGroups} groups")),
        new("Distance", "motion.distance.*", string.Create(CultureInfo.InvariantCulture, $"{LeonardoTokens.Motion.DistanceEnterMin}–{LeonardoTokens.Motion.DistanceEnterMax} epx"), "How far entering elements travel"),
        new("Arrive hold", "motion.arrive.hold", Milliseconds(LeonardoTokens.Motion.ArriveHold), "How long the marker stays (proposal)"),
    ];

    /// <summary>The folders of the Dive in demo.</summary>
    public IReadOnlyList<string> DemoFolders { get; } = ["Contracts", "Invoices", "Tenders"];

    /// <summary>The folder the Dive in demo is in, or null for the list.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFolderOpen))]
    public partial string? OpenFolder { get; set; }

    /// <summary>Whether the Dive in demo shows a folder.</summary>
    public bool IsFolderOpen => OpenFolder is not null;

    /// <summary>Whether the Transform demo shows the comment panel.</summary>
    [ObservableProperty]
    public partial bool IsCommenting { get; set; }

    /// <summary>The files of the Reflow demo.</summary>
    public ObservableCollection<DemoFile> DemoFiles { get; } =
    [
        new("Price list 2027.xlsx", 88_412),
        new("Framework agreement 2026.docx", 248_310),
        new("Site plan hall 4.pdf", 5_402_331),
        new("Meeting notes November.docx", 42_116),
        new("Invoice 2026-119.pdf", 398_204),
    ];

    /// <summary>The events of the Arrive and Leave demo, newest first.</summary>
    public ObservableCollection<DemoEvent> DemoEvents { get; } =
    [
        new("Jonas Weber commented on Price list 2027.xlsx.", "Hofmann Logistics / Contracts · 8:12 AM"),
        new("Anna Berg changed Service level appendix.docx.", "Hofmann Logistics / Contracts · yesterday"),
    ];

    /// <summary>
    /// Sorts the Reflow demo by name or by size with moves, so that every file keeps its identity.
    /// </summary>
    public void SortFiles(bool bySize)
    {
        var sorted = bySize
            ? DemoFiles.OrderBy(file => file.Size).ToList()
            : DemoFiles.OrderBy(file => file.Name, StringComparer.CurrentCulture).ToList();
        for (var index = 0; index < sorted.Count; index++)
        {
            var current = DemoFiles.IndexOf(sorted[index]);
            if (current != index)
            {
                DemoFiles.Move(current, index);
            }
        }
    }

    /// <summary>
    /// Adds a new event at the top of the Arrive demo.
    /// </summary>
    public DemoEvent AddEvent()
    {
        var (text, meta) = NewEvents[_nextEvent++ % NewEvents.Length];
        var item = new DemoEvent(text, meta);
        DemoEvents.Insert(0, item);
        return item;
    }

    /// <inheritdoc />
    public void Dispose() => MotionSettings.Changed -= OnMotionSettingsChanged;

    partial void OnAnimationModeIndexChanged(int value)
    {
        MotionSettings.AnimationsOverride = (AnimationMode)value switch
        {
            AnimationMode.SimulateOff => false,
            AnimationMode.SimulateOn => true,
            _ => null,
        };
    }

    partial void OnSlowDownChanged(double value)
        => MotionSettings.Speed = MotionMath.NormalSpeed / Math.Max(value, MotionMath.NormalSpeed);

    private void OnMotionSettingsChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(AnimationState));

    private static string Milliseconds(TimeSpan duration)
        => string.Create(CultureInfo.InvariantCulture, $"{duration.TotalMilliseconds:0} ms");
}
