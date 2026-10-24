using Managed.PGlite;
using Npgsql;
using NpgsqlTypes;

// The application owns these directories. For a durable application, keep a
// stable path and pass create:false when opening an already initialized cluster.
string root = Path.Combine(Path.GetTempPath(), "pglite-npgsql-example-" + Guid.NewGuid().ToString("N"));
var options = new PgliteOptions(Path.Combine(root, "data"),
    Path.Combine(AppContext.BaseDirectory, "bootstrap-prefix"), Path.Combine(root, "work"));
bool closedCleanly = false;
try
{
    await using (var source = NpgsqlEmbeddedDataSource.Create(options))
    {
        await using (var connection = await source.OpenConnectionAsync())
        {
            await using (var command = new NpgsqlCommand(
                "CREATE TABLE entries (id integer PRIMARY KEY, name text NOT NULL)", connection))
                await command.ExecuteNonQueryAsync();

            await using (var transaction = await connection.BeginTransactionAsync())
            {
                await using var insert = new NpgsqlCommand("INSERT INTO entries VALUES ($1, $2)", connection, transaction);
                insert.Parameters.AddWithValue(NpgsqlDbType.Integer, 7);
                insert.Parameters.AddWithValue(NpgsqlDbType.Text, "Žąsis");
                await insert.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }

            await using (var transaction = await connection.BeginTransactionAsync())
            {
                await using var delete = new NpgsqlCommand("DELETE FROM entries", connection, transaction);
                await delete.ExecuteNonQueryAsync();
                await transaction.RollbackAsync();
            }
        }

        // Closing releases the sole session. Reopening resets session state and
        // retains committed data in caller-owned PGDATA.
        await using var reopened = await source.OpenConnectionAsync();
        await using var select = new NpgsqlCommand("SELECT id, name FROM entries WHERE id = $1", reopened);
        select.Parameters.AddWithValue(NpgsqlDbType.Integer, 7);
        await select.PrepareAsync();
        await using var reader = await select.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) throw new InvalidOperationException("Committed row was not retained.");
        Console.WriteLine($"id={reader.GetInt32(0)}, name={reader.GetString(1)}");
    }
    closedCleanly = true;
}
finally
{
    if (closedCleanly) Directory.Delete(root, recursive: true);
    else Console.Error.WriteLine($"Cluster and diagnostics retained at {root}");
}
