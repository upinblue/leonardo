using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace UpInBlue.Leonardo.Gallery.Helpers;

/// <summary>
/// Finds elements of item templates, which x:Name cannot reach from the page.
/// </summary>
public static class VisualTree
{
    /// <summary>
    /// The first descendant of a type, optionally with a name, depth first.
    /// </summary>
    public static T? FindDescendant<T>(DependencyObject? root, string? name = null)
        where T : FrameworkElement
    {
        if (root is null)
        {
            return null;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match && (name is null || match.Name == name))
            {
                return match;
            }

            if (FindDescendant<T>(child, name) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
