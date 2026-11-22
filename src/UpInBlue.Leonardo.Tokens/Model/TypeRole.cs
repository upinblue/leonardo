namespace UpInBlue.Leonardo.Tokens.Model;

/// <summary>
/// A type role (place name, view, section, body, meta) with its values per platform.
/// A platform is <see langword="null"/> when the role does not exist there.
/// </summary>
internal sealed record TypeRole(
    string Name,
    WindowsTypeStyle? Windows,
    AppleTextStyle? Ios,
    AppleTextStyle? MacOS,
    PresentationTypeStyle? Presentation,
    WebTypeStyle? Web);

/// <summary>
/// Font values on Windows, in effective pixels. <c>Color</c> is the name of a theme color, for example <c>textTertiary</c>, or <see langword="null"/>.
/// </summary>
internal sealed record WindowsTypeStyle(string FontFamily, double FontSize, double LineHeight, int FontWeight, string? Color);

/// <summary>
/// The SwiftUI text style of a role, for example <c>largeTitle</c>.
/// </summary>
internal sealed record AppleTextStyle(string TextStyle, string? Description);

/// <summary>
/// Font values in PowerPoint, in points.
/// </summary>
internal sealed record PresentationTypeStyle(string FontFamily, double FontSizePt, int FontWeight);

/// <summary>
/// Font values for standalone web. The line height is unitless. <c>Color</c> is the name of a theme color, or <see langword="null"/>.
/// </summary>
internal sealed record WebTypeStyle(string FontFamily, double FontSizeRem, double LineHeight, int FontWeight, string? Color);
