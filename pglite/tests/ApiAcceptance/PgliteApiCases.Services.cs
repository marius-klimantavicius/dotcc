using Managed.PGlite.ApiServicesTests;

namespace Managed.PGlite.ProductTests;

internal static partial class PgliteApiCases
{
    static partial void Services(PgliteDatabase database)
    {
        CoreProtocolCases.Run(database);
        assertions += CoreProtocolCases.Assertions;
        assertions += ApiServicesCases.Run(database, Console.WriteLine);
    }

    static partial void WorkerAndStorage(PgliteOptions options)
    {
        int previous = CoreProtocolCases.Assertions;
        CoreProtocolCases.ClosePending(options);
        assertions += CoreProtocolCases.Assertions - previous;
        assertions += ApiServicesCases.RunLifetimes(options, Console.WriteLine);
        var configured = options with
        {
            DataDirectory = options.DataDirectory + "-configured",
            Configuration = new PgliteConfiguration
            {
                Username = "api_owner", Database = "postgres", Debug = 1, RelaxedDurability = true,
                Settings = new Dictionary<string, string> { ["application_name"] = "managed-api", ["TimeZone"] = "UTC" }
            }
        };
        using (var database = PgliteDatabase.Create(configured))
        {
            var row = database.Query("SELECT current_user, current_database(), current_setting('application_name'), current_setting('synchronous_commit'), current_setting('io_method')")
                .EnsureSuccess().ResultSets.Single().Rows.Single();
            Check(row.SequenceEqual(new[] { "api_owner", "postgres", "managed-api", "off", "sync" }) && database.Debug == 1,
                "configuration/user-database-settings-debug-durability");
            database.Exec("CREATE TABLE api_configuration_value AS SELECT 42 AS value");
            database.SyncToFs();
            Check(database.Ready, "configuration/explicit-durability-fence");
        }
        using (var reopened = PgliteDatabase.Open(configured))
            Check(reopened.Query("SELECT value FROM api_configuration_value").EnsureSuccess().ResultSets.Single().Rows.Single()[0] == "42",
                "configuration/configured-clean-reopen");
        foreach (string setting in new[] { "data_directory", "shared_preload_libraries", "client_encoding", "max_worker_processes", "io_method" })
        {
            bool rejected = false;
            try { using var invalid = PgliteDatabase.Open(configured with { Configuration = new() { Settings = new Dictionary<string, string> { [setting] = "unsupported" } } }); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected, "configuration/product-owned-setting/" + setting);
        }
    }
}
