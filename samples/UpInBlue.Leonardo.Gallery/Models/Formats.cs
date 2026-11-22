using System.Globalization;

namespace UpInBlue.Leonardo.Gallery.Models;

/// <summary>
/// Formatting of dates, sizes and amounts. The gallery UI is English, so formats are en-US.
/// </summary>
public static class Formats
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>"Nov 18, 2026".</summary>
    public static string Date(DateTimeOffset value) => value.ToString("MMM d, yyyy", Culture);

    /// <summary>"Nov 18, 2026, 10:42 AM".</summary>
    public static string DateTime(DateTimeOffset value) => value.ToString("MMM d, yyyy, h:mm tt", Culture);

    /// <summary>"10:42 AM".</summary>
    public static string Time(DateTimeOffset value) => value.ToString("h:mm tt", Culture);

    /// <summary>"1.2 MB", "860 KB" or "–" for folders.</summary>
    public static string Size(long? bytes) => bytes switch
    {
        null => "–",
        < 1024 => string.Create(Culture, $"{bytes} bytes"),
        < 1024 * 1024 => string.Create(Culture, $"{bytes / 1024.0:0} KB"),
        _ => string.Create(Culture, $"{bytes / (1024.0 * 1024.0):0.0} MB"),
    };

    /// <summary>"EUR 12,480.00".</summary>
    public static string Amount(decimal value) => string.Create(Culture, $"EUR {value:N2}");

    /// <summary>"248".</summary>
    public static string Number(int value) => value.ToString("N0", Culture);
}
