using System.Text.Json.Nodes;

namespace UpInBlue.Leonardo.WinUI.Tests;

/// <summary>
/// Reads values from <c>tokens/leonardo.tokens.json</c>, so that tests check the real tokens.
/// </summary>
internal static class Tokens
{
    private static readonly Lazy<JsonNode> Root = new(() => JsonNode.Parse(File.ReadAllText(Path.Combine(FindRepository(), "tokens", "leonardo.tokens.json")))!);

    public static TimeSpan Duration(string path)
    {
        var value = Value(path);
        var amount = value["value"]!.GetValue<double>();
        return value["unit"]!.GetValue<string>() == "s" ? TimeSpan.FromSeconds(amount) : TimeSpan.FromMilliseconds(amount);
    }

    public static double Dimension(string path) => Value(path)["value"]!.GetValue<double>();

    public static double Number(string path) => Value(path).GetValue<double>();

    public static double[] CubicBezier(string path) => Value(path).AsArray().Select(point => point!.GetValue<double>()).ToArray();

    private static JsonNode Value(string path) => path.Split('.').Aggregate(Root.Value, (node, segment) => node[segment]!)["$value"]!;

    private static string FindRepository()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "UpInBlue.Leonardo.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repository root with UpInBlue.Leonardo.slnx not found.");
    }
}
