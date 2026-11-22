using Microsoft.UI.Xaml.Controls;

namespace UpInBlue.Leonardo.Gallery.Pages;

/// <summary>
/// What Leonardo is, who owns which layer, and the rule levels.
/// </summary>
public sealed partial class OverviewPage : Page
{
    /// <summary>
    /// Initializes the page.
    /// </summary>
    public OverviewPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The decision rules of the design language, in order.
    /// </summary>
    public IReadOnlyList<string> DecisionRules { get; } =
    [
        "1. Accessibility comes before everything.",
        "2. Usability comes before aesthetics.",
        "3. If the platform has a paradigm, the platform wins.",
        "4. If it has none, Leonardo applies.",
        "5. If Leonardo doesn't cover the case either, the calmer solution wins.",
        "6. If an element can't name its purpose, it's removed.",
        "7. If it's still unclear, ask instead of inventing.",
    ];
}
