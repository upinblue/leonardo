using System.Text.Json;

namespace UpInBlue.Leonardo.Tokens.Tests;

public sealed class RepositoryLayoutTests
{
    [Fact]
    public void TokenFileExistsAndIsValidJson()
    {
        using var document = JsonDocument.Parse(Repository.TokensFile);

        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
    }
}
