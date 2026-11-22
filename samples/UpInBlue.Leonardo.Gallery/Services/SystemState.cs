using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.System;
using Windows.UI.ViewManagement;

namespace UpInBlue.Leonardo.Gallery.Services;

/// <summary>
/// The live state of the Windows settings Leonardo falls back on: transparency, animations,
/// contrast themes and text size. Changes are raised on the UI thread.
/// </summary>
public sealed partial class SystemState : ObservableObject, IDisposable
{
    private readonly UISettings _uiSettings = new();
    private readonly ThemeSettings _themeSettings;
    private readonly DispatcherQueue _dispatcher;

    /// <summary>
    /// Starts listening to the Windows settings for a window. Call on the UI thread.
    /// </summary>
    /// <param name="windowId">The window whose contrast theme is read.</param>
    public SystemState(WindowId windowId)
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _themeSettings = ThemeSettings.CreateForWindowId(windowId);
        Refresh();
        _uiSettings.AdvancedEffectsEnabledChanged += OnChanged;
        _uiSettings.AnimationsEnabledChanged += OnChanged;
        _uiSettings.TextScaleFactorChanged += OnChanged;
        _themeSettings.Changed += OnThemeChanged;
    }

    /// <summary>Whether transparency effects are on.</summary>
    [ObservableProperty]
    public partial bool TransparencyEnabled { get; private set; }

    /// <summary>Whether animation effects are on.</summary>
    [ObservableProperty]
    public partial bool AnimationsEnabled { get; private set; }

    /// <summary>Whether a contrast theme is on.</summary>
    [ObservableProperty]
    public partial bool HighContrast { get; private set; }

    /// <summary>The name of the contrast theme, for example "High Contrast Black".</summary>
    [ObservableProperty]
    public partial string HighContrastScheme { get; private set; } = string.Empty;

    /// <summary>The text size set in Windows, where 1 is 100 %.</summary>
    [ObservableProperty]
    public partial double TextScaleFactor { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        _uiSettings.AdvancedEffectsEnabledChanged -= OnChanged;
        _uiSettings.AnimationsEnabledChanged -= OnChanged;
        _uiSettings.TextScaleFactorChanged -= OnChanged;
        _themeSettings.Changed -= OnThemeChanged;
    }

    private void OnChanged(object sender, object args) => _dispatcher.TryEnqueue(Refresh);

    private void OnChanged(UISettings sender, UISettingsAnimationsEnabledChangedEventArgs args) => _dispatcher.TryEnqueue(Refresh);

    private void OnThemeChanged(ThemeSettings sender, object args) => _dispatcher.TryEnqueue(Refresh);

    private void Refresh()
    {
        TransparencyEnabled = _uiSettings.AdvancedEffectsEnabled;
        AnimationsEnabled = _uiSettings.AnimationsEnabled;
        HighContrast = _themeSettings.HighContrast;
        HighContrastScheme = _themeSettings.HighContrast ? _themeSettings.HighContrastScheme : string.Empty;
        TextScaleFactor = _uiSettings.TextScaleFactor;
    }
}
