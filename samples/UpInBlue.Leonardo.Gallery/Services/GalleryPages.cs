using UpInBlue.Leonardo.Gallery.Pages;

namespace UpInBlue.Leonardo.Gallery.Services;

/// <summary>
/// A page of the gallery, identified by a tag used in the navigation and on the command line.
/// </summary>
public sealed record GalleryPage(string Tag, Type PageType);

/// <summary>
/// All pages of the gallery.
/// </summary>
public static class GalleryPages
{
    /// <summary>The tag of the settings page.</summary>
    public const string SettingsTag = "settings";

    /// <summary>The pages in navigation order. The settings page comes last.</summary>
    public static IReadOnlyList<GalleryPage> All { get; } =
    [
        new("overview", typeof(OverviewPage)),
        new("color", typeof(ColorPage)),
        new("typography", typeof(TypographyPage)),
        new("geometry", typeof(GeometryPage)),
        new("motion", typeof(MotionPage)),
        new("fallbacks", typeof(FallbacksPage)),
        new("placeheader", typeof(PlaceHeaderPage)),
        new("summaryline", typeof(SummaryLinePage)),
        new("statusdot", typeof(StatusDotPage)),
        new("files", typeof(FilesPage)),
        new("customers", typeof(CustomersPage)),
        new("approvals", typeof(ApprovalsPage)),
        new("keyfigures", typeof(KeyFiguresPage)),
        new("activity", typeof(ActivityPage)),
        new("states", typeof(StatesPage)),
        new(SettingsTag, typeof(SettingsPage)),
    ];

    /// <summary>
    /// Finds a page by its tag.
    /// </summary>
    public static GalleryPage? Find(string? tag) => All.FirstOrDefault(page => string.Equals(page.Tag, tag, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Finds a page by its type.
    /// </summary>
    public static GalleryPage? Find(Type? pageType) => All.FirstOrDefault(page => page.PageType == pageType);
}
