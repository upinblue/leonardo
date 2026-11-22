using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UpInBlue.Leonardo.Gallery.Wpf.Helpers;

/// <summary>
/// Attaches resources by key from data, as <c>DynamicResource</c> would, so they follow theme changes.
/// </summary>
public static class ResourceReference
{
    /// <summary>The key of the style of a framework element.</summary>
    public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached(
        "Style",
        typeof(string),
        typeof(ResourceReference),
        new PropertyMetadata(null, (element, e) => ((FrameworkElement)element).SetResourceReference(FrameworkElement.StyleProperty, e.NewValue)));

    /// <summary>The key of the background brush of a border.</summary>
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
        "Background",
        typeof(string),
        typeof(ResourceReference),
        new PropertyMetadata(null, (element, e) => ((Border)element).SetResourceReference(Border.BackgroundProperty, e.NewValue)));

    /// <summary>Gets the style key.</summary>
    /// <param name="element">The element.</param>
    /// <returns>The key.</returns>
    public static string? GetStyle(FrameworkElement element) => (string?)element.GetValue(StyleProperty);

    /// <summary>Sets the style key.</summary>
    /// <param name="element">The element.</param>
    /// <param name="value">The key.</param>
    public static void SetStyle(FrameworkElement element, string? value) => element.SetValue(StyleProperty, value);

    /// <summary>Gets the background brush key.</summary>
    /// <param name="element">The border.</param>
    /// <returns>The key.</returns>
    public static string? GetBackground(Border element) => (string?)element.GetValue(BackgroundProperty);

    /// <summary>Sets the background brush key.</summary>
    /// <param name="element">The border.</param>
    /// <param name="value">The key.</param>
    public static void SetBackground(Border element, string? value) => element.SetValue(BackgroundProperty, value);
}

/// <summary>
/// Shows the color of a solid brush as hex, for example <c>#1B6CE0</c>.
/// </summary>
public sealed class BrushToHexConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is SolidColorBrush { Color: var c } ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : string.Empty;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
