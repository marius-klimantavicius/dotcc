using System.Text.Json;
using Npgsql;

namespace NpgsqlAcceptance;

internal sealed partial class Suite
{
    internal async Task SamePinOracle()
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "oracle-cases.json")));
        foreach (var item in document.RootElement.EnumerateArray())
        {
            string name = item.GetProperty("name").GetString()!;
            using var cmd = Command(item.GetProperty("sql").GetString()!);
            if (item.TryGetProperty("error", out var expectedError))
            {
                var error = await check.ThrowsAsync<PostgresException>(async () => { await cmd.ExecuteNonQueryAsync(); }, "native oracle error " + name);
                check.Equal(expectedError.GetString(), error.SqlState, "native oracle SQLSTATE " + name);
                continue;
            }
            var expected = item.GetProperty("rows").EnumerateArray().ToArray();
            await using var reader = await cmd.ExecuteReaderAsync();
            int row = 0;
            do
            {
                while (await reader.ReadAsync())
                {
                    check.True(row < expected.Length, "native oracle no extra row " + name);
                    var columns = expected[row++].EnumerateArray().ToArray();
                    check.Equal(columns.Length, reader.FieldCount, "native oracle columns " + name);
                    for (int col = 0; col < columns.Length; col++)
                    {
                        if (columns[col].ValueKind == JsonValueKind.Null)
                            check.True(reader.IsDBNull(col), "native oracle NULL " + name);
                        else
                            check.Equal(columns[col].GetString(), reader.GetString(col), "native oracle text " + name);
                    }
                }
            } while (await reader.NextResultAsync());
            check.Equal(expected.Length, row, "native oracle row count " + name);
        }
    }
}
