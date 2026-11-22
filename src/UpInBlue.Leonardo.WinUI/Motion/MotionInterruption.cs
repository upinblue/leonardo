using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace UpInBlue.Leonardo.WinUI.Motion;

/// <summary>
/// Motion never blocks: clicking, tapping or pressing a key while something moves jumps every running Leonardo motion
/// to its end state. The input itself is not handled and reaches its target as usual.
/// </summary>
public static class MotionInterruption
{
    private static readonly PointerEventHandler PointerPressed = (_, _) => SkipAllToEnd();
    private static readonly KeyEventHandler KeyDown = (_, _) => SkipAllToEnd();

    /// <summary>
    /// Listens to pointer and key presses anywhere below an element, usually the root of the window content.
    /// </summary>
    public static void Attach(UIElement root)
    {
        ArgumentNullException.ThrowIfNull(root);
        root.AddHandler(UIElement.PointerPressedEvent, PointerPressed, handledEventsToo: true);
        root.AddHandler(UIElement.KeyDownEvent, KeyDown, handledEventsToo: true);
    }

    /// <summary>
    /// Stops listening.
    /// </summary>
    public static void Detach(UIElement root)
    {
        ArgumentNullException.ThrowIfNull(root);
        root.RemoveHandler(UIElement.PointerPressedEvent, PointerPressed);
        root.RemoveHandler(UIElement.KeyDownEvent, KeyDown);
    }

    /// <summary>
    /// Jumps every running Leonardo motion to its end state.
    /// </summary>
    public static void SkipAllToEnd() => MotionRun.SkipAllToEnd();
}
