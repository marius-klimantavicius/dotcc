using Managed.PGlite;

// The application owns these real host directories. Bootstrap assets are copied
// by the project targets; no external PostgreSQL executable is started.
string root = Path.Combine(Path.GetTempPath(), "pglite-api-example-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var options = new PgliteOptions(
    Path.Combine(root, "data"),
    Path.Combine(AppContext.BaseDirectory, "bootstrap-prefix"),
    Path.Combine(root, "work"));
bool closedCleanly = false;

try
{
    await using var worker = PgliteWorker.Create(options);
    try
    {
        await worker.WaitReady();
        await worker.ExecAsync("CREATE TABLE example_entries (id integer PRIMARY KEY, name text NOT NULL)");

        int id = 7;
        string name = "Žąsis";
        // This callback is synchronous and stays on the worker's owner thread.
        // Interpolated values become parameters; they never become SQL text.
        await worker.TransactionAsync(tx =>
        {
            tx.Sql(PgliteTemplate.Sql($"INSERT INTO example_entries VALUES ({id}, {name})"));
        });

        var response = await worker.QueryAsync(
            "SELECT id, name FROM example_entries WHERE id = $1::integer",
            new object?[] { id });
        var row = response.EnsureSuccess().ResultSets.Single().TypedObjectRows.Single();
        Console.WriteLine($"id={(int)row["id"]!}, name={(string)row["name"]!}");
    }
    finally
    {
        // Drain accepted work and close on the dedicated owner thread before
        // deleting caller-owned files. Caller-side await may resume anywhere.
        await worker.CloseAsync();
        closedCleanly = true;
    }
}
finally
{
    if (closedCleanly)
        Directory.Delete(root, recursive: true);
    else
        Console.Error.WriteLine($"Cluster and diagnostics retained at {root}");
}
