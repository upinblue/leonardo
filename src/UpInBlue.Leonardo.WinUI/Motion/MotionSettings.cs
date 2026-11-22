using Microsoft.UI.Dispatching;
using Windows.UI.ViewManagement;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Whether Leonardo motion runs, and how fast. Wraps the Windows setting "Animation effects"
/// (<see cref="UISettings.AnimationsEnabled"/>) and adds an override and a slow-down for testing.
/// When animations are off, every helper in this namespace falls back to a crossfade with <c>motion.duration.fade</c>.
/// </summary>
/// <remarks>
/// Call <see cref="Initialize"/> once on the UI thread, for example when the main window is created, so that
/// <see cref="Changed"/> is raised on the UI thread.
/// </remarks>
public static class MotionSettings
{
    private static readonly Lock Gate = new();
    private static UISettings? _uiSettings;
    private static DispatcherQueue? _dispatcher;
    private static bool _systemAnimationsEnabled = true;
    private static bool? _animationsOverride;
    private static double _speed = MotionMath.NormalSpeed;

    /// <summary>
    /// Raised on the UI thread when <see cref="AnimationsEnabled"/> or <see cref="Speed"/> may have changed.
    /// </summary>
    public static event EventHandler? Changed;

    /// <summary>
    /// Whether "Animation effects" is on in Windows Settings.
    /// </summary>
    public static bool SystemAnimationsEnabled
    {
        get
        {
            EnsureInitialized();
            return _systemAnimationsEnabled;
        }
    }

    /// <summary>
    /// An in-app override for testing: true runs the motions, false simulates animation effects turned off,
    /// null (the default) follows Windows.
    /// </summary>
    public static bool? AnimationsOverride
    {
        get => _animationsOverride;
        set
        {
            if (_animationsOverride == value)
            {
                return;
            }

            _animationsOverride = value;
            OnChanged();
        }
    }

    /// <summary>
    /// Whether the helpers run their motion (true) or fall back to a crossfade (false).
    /// </summary>
    public static bool AnimationsEnabled => _animationsOverride ?? SystemAnimationsEnabled;

    /// <summary>
    /// The speed of all Leonardo motion, from <see cref="MotionMath.MinimumSpeed"/> (one tenth, for inspection)
    /// to <see cref="MotionMath.NormalSpeed"/> (the default).
    /// </summary>
    public static double Speed
    {
        get => _speed;
        set
        {
            var speed = MotionMath.ClampSpeed(value);
            if (speed.Equals(_speed))
            {
                return;
            }

            _speed = speed;
            OnChanged();
        }
    }

    /// <summary>
    /// Starts listening to the Windows setting. Call on the UI thread; later calls do nothing.
    /// </summary>
    public static void Initialize() => EnsureInitialized();

    private static void EnsureInitialized()
    {
        lock (Gate)
        {
            if (_uiSettings is not null)
            {
                return;
            }

            _dispatcher = DispatcherQueue.GetForCurrentThread();
            _uiSettings = new UISettings();
            _systemAnimationsEnabled = _uiSettings.AnimationsEnabled;
            _uiSettings.AnimationsEnabledChanged += OnSystemChanged;
        }
    }

    private static void OnSystemChanged(UISettings sender, UISettingsAnimationsEnabledChangedEventArgs args)
    {
        void Refresh()
        {
            var enabled = sender.AnimationsEnabled;
            if (enabled == _systemAnimationsEnabled)
            {
                return;
            }

            _systemAnimationsEnabled = enabled;
            OnChanged();
        }

        if (_dispatcher is null || !_dispatcher.TryEnqueue(Refresh))
        {
            Refresh();
        }
    }

    private static void OnChanged()
    {
        // Motion never outlives a change of the setting: running animations jump to their end state.
        MotionInterruption.SkipAllToEnd();
        Changed?.Invoke(null, EventArgs.Empty);
    }
}
