using Microsoft.UI.Xaml;

namespace UpInBlue.Leonardo.Gallery.Helpers;

/// <summary>
/// Visibility helpers for x:Bind function bindings. They take bools and strings only: x:Bind does not call a function
/// again when an object argument changes to null, so a check for null belongs in a bool property of the view model.
/// </summary>
public static class Show
{
    /// <summary>Visible when the value is true.</summary>
    public static Visibility When(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Visible when the value is false.</summary>
    public static Visibility Unless(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Visible when the text is not empty.</summary>
    public static Visibility WhenText(string? value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>"On" or "Off".</summary>
    public static string OnOff(bool value) => value ? "On" : "Off";
}
