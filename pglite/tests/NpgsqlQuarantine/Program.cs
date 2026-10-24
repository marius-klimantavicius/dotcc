using System.Data;
using Managed.PGlite;
using Npgsql;

// Run separately: this intentionally quarantines the sole engine owner until
// process exit. It uses only public configuration/SQL, without corrupting files.
string root = Path.Combine(Path.GetTempPath(), "npgsql-quarantine-" + Guid.NewGuid().ToString("N"));
var options = new PgliteOptions(Path.Combine(root, "data"),
    Path.Combine(AppContext.BaseDirectory, "bootstrap-prefix"), Path.Combine(root, "work"))
{ MaximumResponseBytes = 1024 * 1024 };
int checks = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    checks++;
}
async Task<T> Reject<T>(Func<Task> operation) where T : Exception
{
    try { await operation(); }
    catch (T error) { checks++; return error; }
    throw new InvalidOperationException("Expected " + typeof(T).Name);
}

await using var source = NpgsqlEmbeddedDataSource.Create(options);
var connection = await source.OpenConnectionAsync();
using var command = new NpgsqlCommand("SELECT 42", connection);
Check(Equals(42, await command.ExecuteScalarAsync()), "The real engine must work before the limit is exceeded.");
command.CommandText = "SELECT repeat('x',2097152)";
await Reject<NpgsqlException>(async () => { await command.ExecuteScalarAsync(); });
Check(connection.State == ConnectionState.Closed, "A failed engine must not remain a usable open connection.");
for (int attempt = 0; attempt < 2; attempt++)
{
    var error = await Reject<NpgsqlException>(() => connection.CloseAsync());
    Check(error.Message.Contains("quarantined", StringComparison.Ordinal), "Close must report the retained quarantine.");
}
var reopen = await Reject<InvalidOperationException>(() => connection.OpenAsync());
Check(reopen.Message.Contains("quarantined", StringComparison.Ordinal), "Reopen must report the retained quarantine.");
await using var otherSource = NpgsqlEmbeddedDataSource.Create(options, create: false);
var second = await Reject<InvalidOperationException>(async () => { await using var other = await otherSource.OpenConnectionAsync(); });
Check(second.Message.Contains("One embedded PGlite owner", StringComparison.Ordinal), "Another source must not acquire a quarantined owner.");
Check(File.Exists(Path.Combine(options.DataDirectory, "global/pg_control")), "Caller PGDATA must be retained after failed shutdown.");
Console.WriteLine($"PASS {checks} real-engine response-limit/quarantine assertions; files retained at {root}");
