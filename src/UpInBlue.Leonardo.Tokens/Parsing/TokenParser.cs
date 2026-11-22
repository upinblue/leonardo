using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using UpInBlue.Leonardo.Tokens.Model;

namespace UpInBlue.Leonardo.Tokens.Parsing;

/// <summary>
/// Reads the token file (Design Tokens Community Group format) and resolves references.
/// </summary>
internal static partial class TokenParser
{
    private const string TypographyGroup = "typography";

    private static readonly string[] Platforms = ["windows", "ios", "macos", "presentation", "web"];

    /// <summary>
    /// Parses the token file and resolves all references.
    /// </summary>
    /// <exception cref="TokenException">The file is invalid, a reference is unknown or references form a cycle.</exception>
    public static TokenSet Parse(string json)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Disallow });
        }
        catch (JsonException exception)
        {
            throw new TokenException($"The token file is not valid JSON: {exception.Message}", exception);
        }

        if (root is not JsonObject rootObject)
        {
            throw new TokenException("The token file must contain a JSON object.");
        }

        var rawTokens = new List<RawToken>();
        var groupDescriptions = new Dictionary<string, string>(StringComparer.Ordinal);
        IReadOnlyList<TypeRole> typeRoles = [];

        if (rootObject["$description"] is { } fileDescription)
        {
            groupDescriptions[string.Empty] = ReadString(fileDescription, "$description");
        }

        foreach (var (name, node) in rootObject)
        {
            if (name.StartsWith('$'))
            {
                continue;
            }

            if (name == TypographyGroup)
            {
                typeRoles = ParseTypography(node, groupDescriptions);
                continue;
            }

            Collect(name, node, inheritedType: null, rawTokens, groupDescriptions);
        }

        var tokens = Resolve(rawTokens);
        var tokenSet = new TokenSet(tokens, typeRoles, groupDescriptions);
        ValidateTypeRoleColors(tokenSet);
        return tokenSet;
    }

    private static void Collect(string path, JsonNode? node, string? inheritedType, List<RawToken> rawTokens, Dictionary<string, string> groupDescriptions)
    {
        if (node is not JsonObject group)
        {
            throw new TokenException($"'{path}' must be a token or a group object.");
        }

        var type = group["$type"] is { } typeNode ? ReadString(typeNode, $"{path}.$type") : inheritedType;
        var description = group["$description"] is { } descriptionNode ? ReadString(descriptionNode, $"{path}.$description") : null;

        if (group.ContainsKey("$value"))
        {
            if (type is null)
            {
                throw new TokenException($"Token '{path}' has no $type.");
            }

            if (group.Any(property => !property.Key.StartsWith('$')))
            {
                throw new TokenException($"Token '{path}' must not contain nested groups or tokens.");
            }

            rawTokens.Add(new RawToken(path, ParseType(type, path), group["$value"], description, ReadStatus(group, path), ReadPlatforms(group, path)));
            return;
        }

        if (description is not null)
        {
            groupDescriptions[path] = description;
        }

        foreach (var (name, child) in group)
        {
            if (!name.StartsWith('$'))
            {
                Collect($"{path}.{name}", child, type, rawTokens, groupDescriptions);
            }
        }
    }

    private static List<Token> Resolve(List<RawToken> rawTokens)
    {
        var byPath = rawTokens.ToDictionary(token => token.Path, StringComparer.Ordinal);
        var resolved = new Dictionary<string, object>(StringComparer.Ordinal);

        object ResolveValue(RawToken token, List<string> chain)
        {
            if (resolved.TryGetValue(token.Path, out var known))
            {
                return known;
            }

            object value;
            if (TryGetReference(token.RawValue, out var target))
            {
                if (!byPath.TryGetValue(target, out var targetToken))
                {
                    throw new TokenException($"Token '{token.Path}' references the unknown token '{{{target}}}'.");
                }

                if (chain.Contains(target, StringComparer.Ordinal))
                {
                    throw new TokenException($"Reference cycle: {string.Join(" -> ", chain.Append(target))}.");
                }

                if (targetToken.Type != token.Type)
                {
                    throw new TokenException($"Token '{token.Path}' is of type {token.Type} but references '{target}' of type {targetToken.Type}.");
                }

                chain.Add(target);
                value = ResolveValue(targetToken, chain);
                chain.RemoveAt(chain.Count - 1);
            }
            else
            {
                value = ParseLiteral(token);
            }

            resolved[token.Path] = value;
            return value;
        }

        return rawTokens
            .Select(token =>
            {
                var value = ResolveValue(token, [token.Path]);
                TryGetReference(token.RawValue, out var reference);
                return new Token(token.Path, token.Type, value, token.Description, token.Status, reference, token.Platforms);
            })
            .ToList();
    }

    private static bool TryGetReference(JsonNode? value, out string target)
    {
        target = string.Empty;
        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text))
        {
            var match = ReferencePattern().Match(text);
            if (match.Success)
            {
                target = match.Groups[1].Value;
                return true;
            }
        }

        return false;
    }

    private static object ParseLiteral(RawToken token)
    {
        var path = token.Path;
        var value = token.RawValue;
        switch (token.Type)
        {
            case TokenType.Color:
                var text = ReadString(value, path);
                return RgbColor.TryParse(text, out var color)
                    ? color
                    : throw new TokenException($"Token '{path}' has the invalid color '{text}'. Use #RRGGBB or #RRGGBBAA.");

            case TokenType.Dimension:
            {
                var (amount, unit) = ReadValueWithUnit(value, path);
                return unit is "px" or "rem"
                    ? new Dimension(amount, unit)
                    : throw new TokenException($"Token '{path}' has the unsupported dimension unit '{unit}'. Use px or rem.");
            }

            case TokenType.Duration:
            {
                var (amount, unit) = ReadValueWithUnit(value, path);
                var milliseconds = unit switch
                {
                    "ms" => amount,
                    "s" => amount * 1000,
                    _ => throw new TokenException($"Token '{path}' has the unsupported duration unit '{unit}'. Use ms or s."),
                };
                return milliseconds >= 0
                    ? new TokenDuration(milliseconds)
                    : throw new TokenException($"Token '{path}' has a negative duration.");
            }

            case TokenType.CubicBezier:
                if (value is not JsonArray points || points.Count != 4)
                {
                    throw new TokenException($"Token '{path}' must be an array of four numbers.");
                }

                var curve = new CubicBezier(
                    ReadNumber(points[0], path),
                    ReadNumber(points[1], path),
                    ReadNumber(points[2], path),
                    ReadNumber(points[3], path));
                return curve.X1 is >= 0 and <= 1 && curve.X2 is >= 0 and <= 1
                    ? curve
                    : throw new TokenException($"Token '{path}' has x coordinates outside 0 to 1.");

            case TokenType.Number:
                return ReadNumber(value, path);

            default:
                throw new TokenException($"Token '{path}' has the unsupported type {token.Type}.");
        }
    }

    private static IReadOnlyList<TypeRole> ParseTypography(JsonNode? node, Dictionary<string, string> groupDescriptions)
    {
        if (node is not JsonObject typography)
        {
            throw new TokenException($"'{TypographyGroup}' must be an object.");
        }

        var roles = new List<TypeRole>();
        foreach (var (roleName, roleNode) in typography)
        {
            var rolePath = $"{TypographyGroup}.{roleName}";
            if (roleName == "$description")
            {
                groupDescriptions[TypographyGroup] = ReadString(roleNode, rolePath);
                continue;
            }

            if (roleName.StartsWith('$'))
            {
                continue;
            }

            if (roleNode is not JsonObject role)
            {
                throw new TokenException($"The type role '{rolePath}' must be an object.");
            }

            foreach (var (platform, _) in role)
            {
                if (!platform.StartsWith('$') && !Platforms.Contains(platform, StringComparer.Ordinal))
                {
                    throw new TokenException($"The type role '{rolePath}' has the unknown platform '{platform}'.");
                }
            }

            roles.Add(new TypeRole(
                roleName,
                ReadPlatform(role, "windows", rolePath, (style, path) => new WindowsTypeStyle(
                    ReadString(style["fontFamily"], $"{path}.fontFamily"),
                    ReadPositiveNumber(style["fontSize"], $"{path}.fontSize"),
                    ReadPositiveNumber(style["lineHeight"], $"{path}.lineHeight"),
                    ReadFontWeight(style["fontWeight"], $"{path}.fontWeight"),
                    ReadOptionalString(style["color"], $"{path}.color"))),
                ReadPlatform(role, "ios", rolePath, ReadAppleTextStyle),
                ReadPlatform(role, "macos", rolePath, ReadAppleTextStyle),
                ReadPlatform(role, "presentation", rolePath, (style, path) => new PresentationTypeStyle(
                    ReadString(style["fontFamily"], $"{path}.fontFamily"),
                    ReadPositiveNumber(style["fontSizePt"], $"{path}.fontSizePt"),
                    ReadFontWeight(style["fontWeight"], $"{path}.fontWeight"))),
                ReadPlatform(role, "web", rolePath, (style, path) => new WebTypeStyle(
                    ReadString(style["fontFamily"], $"{path}.fontFamily"),
                    ReadPositiveNumber(style["fontSizeRem"], $"{path}.fontSizeRem"),
                    ReadPositiveNumber(style["lineHeight"], $"{path}.lineHeight"),
                    ReadFontWeight(style["fontWeight"], $"{path}.fontWeight"),
                    ReadOptionalString(style["color"], $"{path}.color")))));
        }

        return roles;
    }

    private static T? ReadPlatform<T>(JsonObject role, string platform, string rolePath, Func<JsonObject, string, T> read)
        where T : class
    {
        var path = $"{rolePath}.{platform}";
        return role[platform] switch
        {
            null => null,
            JsonObject style => read(style, path),
            _ => throw new TokenException($"'{path}' must be an object or null."),
        };
    }

    private static AppleTextStyle ReadAppleTextStyle(JsonObject style, string path)
        => new(
            ReadString(style["textStyle"], $"{path}.textStyle"),
            ReadOptionalString(style["$description"], $"{path}.$description"));

    private static void ValidateTypeRoleColors(TokenSet tokens)
    {
        foreach (var role in tokens.TypeRoles)
        {
            foreach (var color in new[] { role.Windows?.Color, role.Web?.Color })
            {
                if (color is not null && !(tokens.Contains($"theme.light.{color}") && tokens.Contains($"theme.dark.{color}")))
                {
                    throw new TokenException($"The type role '{role.Name}' uses the color '{color}', which is not a theme color in light and dark.");
                }
            }
        }
    }

    private static TokenType ParseType(string type, string path) => type switch
    {
        "color" => TokenType.Color,
        "dimension" => TokenType.Dimension,
        "duration" => TokenType.Duration,
        "cubicBezier" => TokenType.CubicBezier,
        "number" => TokenType.Number,
        _ => throw new TokenException($"Token '{path}' has the unsupported type '{type}'."),
    };

    private static string? ReadStatus(JsonObject token, string path)
        => token["$extensions"] is JsonObject extensions && extensions["status"] is { } status
            ? ReadString(status, $"{path}.$extensions.status")
            : null;

    /// <summary>
    /// Reads <c>$extensions.platforms</c>: a non-empty array of <c>windows</c>, <c>apple</c>, <c>web</c> and <c>presentation</c>.
    /// </summary>
    private static IReadOnlySet<OutputPlatform>? ReadPlatforms(JsonObject token, string path)
    {
        if (token["$extensions"] is not JsonObject extensions || !extensions.ContainsKey("platforms"))
        {
            return null;
        }

        var platformsPath = $"{path}.$extensions.platforms";
        if (extensions["platforms"] is not JsonArray array || array.Count == 0)
        {
            throw new TokenException($"'{platformsPath}' must be a non-empty array.");
        }

        var platforms = new HashSet<OutputPlatform>();
        foreach (var node in array)
        {
            var name = ReadString(node, platformsPath);
            var platform = name switch
            {
                "windows" => OutputPlatform.Windows,
                "apple" => OutputPlatform.Apple,
                "web" => OutputPlatform.Web,
                "presentation" => OutputPlatform.Presentation,
                _ => throw new TokenException($"'{platformsPath}' contains the unknown platform '{name}'. Use windows, apple, web or presentation."),
            };

            if (!platforms.Add(platform))
            {
                throw new TokenException($"'{platformsPath}' names '{name}' twice.");
            }
        }

        return platforms;
    }

    private static (double Value, string Unit) ReadValueWithUnit(JsonNode? node, string path)
    {
        if (node is not JsonObject value)
        {
            throw new TokenException($"Token '{path}' must have a value like {{ \"value\": 16, \"unit\": \"px\" }}.");
        }

        return (ReadNumber(value["value"], $"{path}.value"), ReadString(value["unit"], $"{path}.unit"));
    }

    private static string ReadString(JsonNode? node, string path)
        => node is JsonValue value && value.TryGetValue<string>(out var text)
            ? text
            : throw new TokenException($"'{path}' must be a string.");

    private static string? ReadOptionalString(JsonNode? node, string path)
        => node is null ? null : ReadString(node, path);

    private static double ReadNumber(JsonNode? node, string path)
        => node is JsonValue value && value.GetValueKind() == JsonValueKind.Number && value.TryGetValue<double>(out var number) && double.IsFinite(number)
            ? number
            : throw new TokenException($"'{path}' must be a number.");

    private static double ReadPositiveNumber(JsonNode? node, string path)
    {
        var number = ReadNumber(node, path);
        return number > 0 ? number : throw new TokenException($"'{path}' must be greater than zero.");
    }

    private static int ReadFontWeight(JsonNode? node, string path)
    {
        var number = ReadNumber(node, path);
        return number is >= 1 and <= 1000 && number == Math.Floor(number)
            ? (int)number
            : throw new TokenException($"'{path}' must be a whole number from 1 to 1000.");
    }

    [GeneratedRegex(@"^\{([^{}]+)\}$")]
    private static partial Regex ReferencePattern();

    private sealed record RawToken(string Path, TokenType Type, JsonNode? RawValue, string? Description, string? Status, IReadOnlySet<OutputPlatform>? Platforms);
}
