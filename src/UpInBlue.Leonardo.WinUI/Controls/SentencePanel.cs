using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace UpInBlue.Leonardo.WinUI.Controls;

/// <summary>
/// Lays out the words and phrases of a <see cref="SummaryLine"/> like running text: left to right, wrapping to the next
/// line when they don't fit, each line centered vertically. Used inside the control only.
/// </summary>
internal sealed partial class SentencePanel : Panel
{
    /// <summary>The space between words and between lines: <c>space.xs</c>.</summary>
    private const double Gap = LeonardoTokens.Space.XS;

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0, height = 0, lineWidth = 0, lineHeight = 0;
        foreach (var child in Children)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            var size = child.DesiredSize;
            if (lineWidth > 0 && lineWidth + Gap + size.Width > availableSize.Width)
            {
                width = Math.Max(width, lineWidth);
                height += lineHeight + Gap;
                lineWidth = 0;
                lineHeight = 0;
            }

            lineWidth += (lineWidth > 0 ? Gap : 0) + size.Width;
            lineHeight = Math.Max(lineHeight, size.Height);
        }

        return new Size(Math.Max(width, lineWidth), height + lineHeight);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var line = new List<UIElement>();
        double top = 0, lineWidth = 0, lineHeight = 0;
        foreach (var child in Children)
        {
            var size = child.DesiredSize;
            if (line.Count > 0 && lineWidth + Gap + size.Width > finalSize.Width)
            {
                ArrangeLine(line, top, lineHeight);
                top += lineHeight + Gap;
                line.Clear();
                lineWidth = 0;
                lineHeight = 0;
            }

            line.Add(child);
            lineWidth += (lineWidth > 0 ? Gap : 0) + size.Width;
            lineHeight = Math.Max(lineHeight, size.Height);
        }

        ArrangeLine(line, top, lineHeight);
        return finalSize;
    }

    private static void ArrangeLine(List<UIElement> line, double top, double lineHeight)
    {
        double left = 0;
        foreach (var child in line)
        {
            var size = child.DesiredSize;
            child.Arrange(new Rect(left, top + ((lineHeight - size.Height) / 2), size.Width, size.Height));
            left += size.Width + Gap;
        }
    }
}
