using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using UpInBlue.Leonardo.WinUI.Motion;
using Windows.Foundation;

namespace UpInBlue.Leonardo.Gallery.Helpers;

/// <summary>
/// Draws easing curves for x:Bind function bindings: time runs to the right, progress upwards.
/// </summary>
public static class Curves
{
    /// <summary>
    /// The curve of an easing as a geometry in a square of <c>GalleryCurveSize</c>.
    /// </summary>
    public static Geometry Geometry(MotionEasing easing)
    {
        var size = (double)Application.Current.Resources["GalleryCurveSize"];
        var (first, second) = MotionCurves.GetControlPoints(easing);
        var figure = new PathFigure { StartPoint = new Point(0, size), IsFilled = false };
        figure.Segments.Add(new BezierSegment
        {
            Point1 = new Point(first.X * size, size - (first.Y * size)),
            Point2 = new Point(second.X * size, size - (second.Y * size)),
            Point3 = new Point(size, 0),
        });
        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// The two axes: progress on the left, time at the bottom.
    /// </summary>
    public static Geometry Axes()
    {
        var size = (double)Application.Current.Resources["GalleryCurveSize"];
        var figure = new PathFigure { StartPoint = new Point(0, 0), IsFilled = false };
        figure.Segments.Add(new LineSegment { Point = new Point(0, size) });
        figure.Segments.Add(new LineSegment { Point = new Point(size, size) });
        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }
}
