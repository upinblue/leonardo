using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.Gallery.Helpers;

/// <summary>
/// Visibility helpers for x:Bind function bindings.
/// </summary>
public static class Show
{
    /// <summary>Visible when the value is true.</summary>
    public static Visibility When(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Visible when the value is false.</summary>
    public static Visibility Unless(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Visible when the text is not empty.</summary>
    public static Visibility WhenText(string? value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Visible when the object exists.</summary>
    public static Visibility WhenSet(object? value) => value is null ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Visible when the object does not exist.</summary>
    public static Visibility UnlessSet(object? value) => value is null ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>"On" or "Off".</summary>
    public static string OnOff(bool value) => value ? "On" : "Off";
}
