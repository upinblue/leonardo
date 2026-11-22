namespace UpInBlue.Leonardo.Gallery.Models;

/// <summary>
/// Invented sample content: fictional companies and people only. Dates are fixed so screenshots stay stable.
/// </summary>
public static class SampleData
{
    /// <summary>The reference "now" of the sample data.</summary>
    public static readonly DateTimeOffset Now = new(2026, 11, 20, 14, 30, 0, TimeSpan.FromHours(1));

    private const string Word = "";
    private const string Sheet = "";
    private const string Pdf = "";
    private const string Picture = "";

    /// <summary>
    /// The folder tree of the Files scenario.
    /// </summary>
    public static IReadOnlyList<FolderNode> Folders()
    {
        const string Root = "Customers";
        var hofmann = $"{Root} / Hofmann Logistics";
        var brenner = $"{Root} / Brenner Industries";
        var kessler = $"{Root} / Kessler Construction";

        return
        [
            new FolderNode("Hofmann Logistics", Root,
                [
                    new FolderNode("Contracts", hofmann, files:
                    [
                        new("Framework agreement 2026.docx", "Word document", Word, At(18, 10, 42), "Lea Schmid", 248_310, $"{hofmann} / Contracts"),
                        new("Rahmenvertragsverlängerungsvereinbarung 2027.docx", "Word document", Word, At(19, 16, 5), "Anna Berg", 194_880, $"{hofmann} / Contracts"),
                        new("Price list 2027.xlsx", "Excel workbook", Sheet, At(17, 9, 12), "Jonas Weber", 88_412, $"{hofmann} / Contracts"),
                        new("Signed framework agreement 2025.pdf", "PDF document", Pdf, At(3, 11, 30), "Tim Kraus", 1_284_002, $"{hofmann} / Contracts"),
                        new("Service level appendix.docx", "Word document", Word, At(12, 14, 18), "Lea Schmid", 61_740, $"{hofmann} / Contracts"),
                    ]),
                    new FolderNode("Invoices", hofmann, files:
                    [
                        new("Invoice 2026-118.pdf", "PDF document", Pdf, At(15, 8, 55), "Mara Lindqvist", 402_118, $"{hofmann} / Invoices"),
                        new("Invoice 2026-119.pdf", "PDF document", Pdf, At(20, 8, 40), "Mara Lindqvist", 398_204, $"{hofmann} / Invoices"),
                    ]),
                ])
            {
                IsExpanded = true,
            },
            new FolderNode("Brenner Industries", Root,
                [
                    new FolderNode("Plant expansion", brenner, files:
                    [
                        new("Site plan hall 4.pdf", "PDF document", Pdf, At(14, 13, 20), "Felix Okafor", 5_402_331, $"{brenner} / Plant expansion"),
                        new("Cost estimate.xlsx", "Excel workbook", Sheet, At(16, 17, 2), "Jonas Weber", 120_554, $"{brenner} / Plant expansion"),
                        new("Photo foundation.jpg", "JPEG image", Picture, At(11, 10, 15), "Felix Okafor", 3_120_448, $"{brenner} / Plant expansion"),
                    ]),
                ]),
            new FolderNode("Lakeside Utilities", Root, files:
                [
                    new("Meeting notes November.docx", "Word document", Word, At(19, 15, 48), "Anna Berg", 42_116, $"{Root} / Lakeside Utilities"),
                ]),
            new FolderNode("Kessler Construction", Root,
                [
                    new FolderNode("Tenders", kessler),
                ]),
            new FolderNode("Mertens Medical", Root, files:
                [
                    new("Onboarding checklist.docx", "Word document", Word, At(9, 9, 0), "Tim Kraus", 36_002, $"{Root} / Mertens Medical"),
                    new("Data processing agreement.pdf", "PDF document", Pdf, At(10, 16, 41), "Lea Schmid", 860_271, $"{Root} / Mertens Medical"),
                ]),
        ];
    }

    private static DateTimeOffset At(int day, int hour, int minute) => new(2026, 11, day, hour, minute, 0, TimeSpan.FromHours(1));
}
