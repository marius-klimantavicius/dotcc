using System.Text.Json;

namespace Managed.PGlite.ProductTests;

/// <summary>Shared exact assertions over the unchanged native SQL inventory.
/// JsonDocument uses no reflection and works in the normally trimmed consumer.</summary>
internal sealed class SqlCases : IDisposable
{
    internal static readonly string[] Groups = ["write", "reopen", "reopen-again"];
    private readonly JsonDocument document;
    private SqlCases(JsonDocument document) { this.document = document; }
    internal static SqlCases Load(string path)
    {
        var result = new SqlCases(JsonDocument.Parse(File.ReadAllBytes(path)));
        try
        {
            if (result.document.RootElement.EnumerateObject().Count() != 3 ||
                Groups.Sum(group => result.Cases(group).GetArrayLength()) != 21)
                throw new InvalidDataException("Expected the canonical 21 SQL cases in three lifetimes.");
            return result;
        }
        catch { result.Dispose(); throw; }
    }
    internal JsonElement Cases(string group) => document.RootElement.GetProperty(group);
    internal static string Name(JsonElement test) => test.GetProperty("name").GetString()!;
    internal static string Sql(JsonElement test) => test.GetProperty("sql").GetString()!;
    internal static void Assert(JsonElement test, PgliteQueryResult response)
    {
        string name = Name(test);
        var rows = response.ResultSets.SelectMany(set => set.Rows).ToArray();
        var commands = response.ResultSets.Select(set => set.Command).ToArray();
        var expectedRows = test.TryGetProperty("rows", out var values)
            ? values.EnumerateArray().Select(row => row.EnumerateArray().Select(field => field.ValueKind == JsonValueKind.Null ? null : field.GetString()).ToArray()).ToArray()
            : [];
        var expectedCommands = test.TryGetProperty("commands", out var tags)
            ? tags.EnumerateArray().Select(tag => tag.GetString()!).ToArray() : [];
        string[] expectedErrors = test.TryGetProperty("error", out var error) ? [error.GetString()!] : [];
        if (rows.Length != expectedRows.Length || rows.Where((row, index) => !row.SequenceEqual(expectedRows[index])).Any())
            throw new InvalidDataException(name + ": incorrect exact row values, field count or NULL.");
        if (!commands.SequenceEqual(expectedCommands)) throw new InvalidDataException(name + ": incorrect command completion.");
        if (!response.Errors.Select(item => item.SqlState).SequenceEqual(expectedErrors))
            throw new InvalidDataException(name + ": incorrect SQLSTATE.");
        if ((char)response.TransactionStatus != test.GetProperty("ready").GetString()![0])
            throw new InvalidDataException(name + ": incorrect ReadyForQuery state.");
    }
    public void Dispose() => document.Dispose();
}
