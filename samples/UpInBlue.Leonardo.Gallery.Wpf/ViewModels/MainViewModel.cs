using System.ComponentModel;
using System.Globalization;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using UpInBlue.Leonardo.Gallery.Wpf.Models;
using UpInBlue.Leonardo.Gallery.Wpf.Services;
using UpInBlue.Leonardo.Wpf;

namespace UpInBlue.Leonardo.Gallery.Wpf.ViewModels;

/// <summary>A choice in the app theme list.</summary>
/// <param name="Value">The theme.</param>
/// <param name="Label">The visible text.</param>
public sealed record ThemeOption(AppTheme Value, string Label);

/// <summary>A theme color, shown with the brush of the active Leonardo dictionary.</summary>
/// <param name="Name">The token name.</param>
/// <param name="Use">What the color means.</param>
/// <param name="BrushKey">The resource key of the brush.</param>
public sealed record ThemeColorItem(string Name, string Use, string BrushKey);

/// <summary>A type role with its text style.</summary>
/// <param name="StyleKey">The resource key of the text style.</param>
/// <param name="Sample">The sample text.</param>
/// <param name="Spec">The values of the role, from the generated tokens.</param>
public sealed record TypeRoleItem(string StyleKey, string Sample, string Spec);

/// <summary>A contract row with formatted values.</summary>
/// <param name="Customer">The customer.</param>
/// <param name="Contract">The contract.</param>
/// <param name="Owner">The person responsible.</param>
/// <param name="AnnualValue">The annual value, formatted.</param>
/// <param name="Renewal">The renewal date, formatted.</param>
public sealed record ContractRow(string Customer, string Contract, string Owner, string AnnualValue, string Renewal);

/// <summary>
/// The view model of the main window.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="settings">The settings of the sample.</param>
    public MainViewModel(GallerySettings settings)
    {
        Settings = settings;
        Settings.PropertyChanged += OnSettingsChanged;
        LeonardoMotion.AnimationsEnabledChanged += (_, _) => OnPropertyChanged(nameof(MotionStatus));
        LeonardoTheme.AppearanceChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsContrastTheme));
            OnPropertyChanged(nameof(ShowsSwatches));
        };
    }

    /// <summary>Gets the settings.</summary>
    public GallerySettings Settings { get; }

    /// <summary>Gets the app theme choices, in the order of the WinUI gallery.</summary>
    public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
    [
        new(AppTheme.Light, "Light"),
        new(AppTheme.Dark, "Dark"),
        new(AppTheme.System, "Use system setting"),
    ];

    /// <summary>Gets or sets the app theme.</summary>
    public AppTheme Theme
    {
        get => Settings.Theme;
        set => Settings.Theme = value;
    }

    /// <summary>Gets or sets a value indicating whether lists use compact rows.</summary>
    public bool IsCompact
    {
        get => Settings.Density == ListDensity.Compact;
        set => SetDensity(value, ListDensity.Compact);
    }

    /// <summary>Gets or sets a value indicating whether lists use standard rows.</summary>
    public bool IsStandard
    {
        get => Settings.Density == ListDensity.Standard;
        set => SetDensity(value, ListDensity.Standard);
    }

    /// <summary>Gets or sets a value indicating whether lists use comfortable rows.</summary>
    public bool IsComfortable
    {
        get => Settings.Density == ListDensity.Comfortable;
        set => SetDensity(value, ListDensity.Comfortable);
    }

    /// <summary>Gets the resource key of the row height for the current density.</summary>
    public string RowHeightKey => Settings.Density switch
    {
        ListDensity.Compact => "LeonardoDensityCompact",
        ListDensity.Comfortable => "LeonardoDensityComfortable",
        _ => "LeonardoDensityStandard",
    };

    /// <summary>Gets a value indicating whether a Windows contrast theme is on.</summary>
    public bool IsContrastTheme => LeonardoTheme.Appearance == LeonardoAppearance.HighContrast;

    /// <summary>Gets a value indicating whether color swatches are shown. In a contrast theme only system colors may appear.</summary>
    public bool ShowsSwatches => !IsContrastTheme;

    /// <summary>Gets a sentence that says how this view enters.</summary>
    public string MotionStatus => (LeonardoMotion.AnimationsOverride, LeonardoMotion.AnimationsEnabled) switch
    {
        (null, true) => "Animation effects are on in Windows, so the groups of this view enter staggered, in reading order.",
        (null, false) => "Animation effects are off in Windows, so the groups of this view crossfade instead of moving.",
        (_, true) => "Animations are turned on for this session, so the groups of this view enter staggered, in reading order.",
        (_, false) => "Animations are turned off for this session, so the groups of this view crossfade instead of moving.",
    };

    /// <summary>Gets the theme colors.</summary>
    public IReadOnlyList<ThemeColorItem> ThemeColors { get; } =
    [
        new("accent", "Act here: primary action, selection", "LeonardoAccentBrush"),
        new("onAccent", "Text and icons on the accent", "LeonardoOnAccentBrush"),
        new("link", "This leads somewhere", "LeonardoLinkBrush"),
        new("selection", "Selected rows", "LeonardoSelectionBrush"),
        new("surface", "Content surface", "LeonardoSurfaceBrush"),
        new("surfaceAlt", "Alternative surface", "LeonardoSurfaceAltBrush"),
        new("textPrimary", "Text", "LeonardoTextPrimaryBrush"),
        new("textSecondary", "Secondary text", "LeonardoTextSecondaryBrush"),
        new("textTertiary", "Meta text", "LeonardoTextTertiaryBrush"),
        new("strokeSubtle", "Dividers", "LeonardoStrokeSubtleBrush"),
        new("strokeStrong", "Boundaries of controls", "LeonardoStrokeStrongBrush"),
        new("statusCritical", "Something is broken", "LeonardoStatusCriticalBrush"),
        new("statusCaution", "Attention, but not blocked", "LeonardoStatusCautionBrush"),
        new("statusSuccess", "Briefly confirmed, then gone", "LeonardoStatusSuccessBrush"),
    ];

    /// <summary>Gets the type roles, with the same sample text as the WinUI gallery.</summary>
    public IReadOnlyList<TypeRoleItem> TypeRoles { get; } =
    [
        new("LeonardoPlaceNameTextStyle", "Place name: Hofmann Logistics", Spec(LeonardoTokens.Typography.PlaceName.FontFamily, LeonardoTokens.Typography.PlaceName.FontSize, LeonardoTokens.Typography.PlaceName.LineHeight, LeonardoTokens.Typography.PlaceName.FontWeight)),
        new("LeonardoViewTextStyle", "View: Contracts", Spec(LeonardoTokens.Typography.View.FontFamily, LeonardoTokens.Typography.View.FontSize, LeonardoTokens.Typography.View.LineHeight, LeonardoTokens.Typography.View.FontWeight)),
        new("LeonardoSectionTextStyle", "Section: Recently changed", Spec(LeonardoTokens.Typography.Section.FontFamily, LeonardoTokens.Typography.Section.FontSize, LeonardoTokens.Typography.Section.LineHeight, LeonardoTokens.Typography.Section.FontWeight)),
        new("LeonardoBodyTextStyle", "Body: The framework agreement with Hofmann Logistics runs until the end of the year. Lea Schmid prepares the renewal.", Spec(LeonardoTokens.Typography.Body.FontFamily, LeonardoTokens.Typography.Body.FontSize, LeonardoTokens.Typography.Body.LineHeight, LeonardoTokens.Typography.Body.FontWeight)),
        new("LeonardoBodyStrongTextStyle", "Emphasis: Renewal due in 14 days", Spec(LeonardoTokens.Typography.Body.FontFamily, LeonardoTokens.Typography.Body.FontSize, LeonardoTokens.Typography.Body.LineHeight, null) + " · Semibold"),
        new("LeonardoMetaTextStyle", "Meta: Modified by Anna Berg · 2 hours ago", Spec(LeonardoTokens.Typography.Meta.FontFamily, LeonardoTokens.Typography.Meta.FontSize, LeonardoTokens.Typography.Meta.LineHeight, LeonardoTokens.Typography.Meta.FontWeight) + " · textTertiary"),
    ];

    /// <summary>Gets the contracts of the list.</summary>
    public IReadOnlyList<ContractRow> Contracts { get; } = SampleData.Contracts
        .Select(contract => new ContractRow(
            contract.Customer,
            contract.Name,
            contract.Owner,
            contract.AnnualValue.ToString("N0", Culture),
            contract.Renewal.ToString("MMM d, yyyy", Culture)))
        .ToList();

    /// <summary>
    /// The Fluent theme mode for the chosen app theme.
    /// </summary>
    /// <param name="theme">The app theme.</param>
    /// <returns>The theme mode.</returns>
    public static ThemeMode ThemeModeOf(AppTheme theme) => theme switch
    {
        AppTheme.Light => ThemeMode.Light,
        AppTheme.Dark => ThemeMode.Dark,
        _ => ThemeMode.System,
    };

    private static string Spec(string family, double size, double lineHeight, ushort? weight)
    {
        var spec = string.Create(Culture, $"{family} · {size}/{lineHeight} epx");
        return weight is { } value ? $"{spec} · {WeightName(value)}" : spec;
    }

    private static string WeightName(ushort weight) => weight switch
    {
        300 => "Light",
        350 => "Semilight",
        400 => "Regular",
        600 => "Semibold",
        _ => weight.ToString(Culture),
    };

    private void SetDensity(bool isChecked, ListDensity density)
    {
        if (isChecked)
        {
            Settings.Density = density;
        }
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(GallerySettings.Theme):
                OnPropertyChanged(nameof(Theme));
                break;
            case nameof(GallerySettings.Density):
                OnPropertyChanged(nameof(IsCompact));
                OnPropertyChanged(nameof(IsStandard));
                OnPropertyChanged(nameof(IsComfortable));
                OnPropertyChanged(nameof(RowHeightKey));
                break;
        }
    }
}
