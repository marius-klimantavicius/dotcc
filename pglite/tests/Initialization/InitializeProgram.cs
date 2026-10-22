using System.Text.Json;
using Managed.PGlite.Initialization;

if (args.Length != 3) throw new ArgumentException("Expected: staged-bootstrap-prefix fresh-pgdata private-artifacts-directory");
string prefix = Path.GetFullPath(args[0]), data = Path.GetFullPath(args[1]), artifacts = Path.GetFullPath(args[2]);
Directory.CreateDirectory(artifacts);
string input = Path.Combine(artifacts, "initdb.stdin"), output = Path.Combine(artifacts, "initdb.stdout"), error = Path.Combine(artifacts, "initdb.stderr");
File.WriteAllBytes(input, []);
var backend = new InitializationBackend(prefix);
var observations = new List<object>();
var operations = new List<BootstrapOperation>();
int sequence = 0;
using var frontend = new InitializationFrontend(Path.Combine(prefix, "bin/postgres"), artifacts, request =>
{
    int index = ++sequence;
    operations.Add(request.Operation);
    int? status = null;
    string? failure = null;
    try { return (status = backend.Invoke(request)).Value; }
    catch (Exception exception) { failure = exception.ToString(); throw; }
    finally
    {
        foreach (var (path, suffix) in new[] { (request.InputPath, ".stdin"), (request.OutputPath, ".stdout"), (request.ErrorPath, ".stderr") })
            if (File.Exists(path)) File.Copy(path, Path.Combine(artifacts, $"backend-{index:000}{suffix}"));
        observations.Add(new { index, request.Operation, request.Arguments, status, failure });
        File.WriteAllText(Path.Combine(artifacts, "backend-invocations.json"), JsonSerializer.Serialize(observations, new JsonSerializerOptions { WriteIndented = true }));
    }
});
int result = frontend.Run([Path.Combine(prefix, "bin/initdb"), "-D", data, "-L", Path.Combine(prefix, "share/postgresql"), "--locale=C", "--encoding=UTF8", "--username=postgres", "--auth=trust", "--no-instructions"], input, output, error);
Console.WriteLine($"Original translated initdb exit {result}; backend commands {sequence}; output: {output}; errors: {error}");
if (frontend.ProcessError != null) throw new InvalidOperationException("Translated initialization process transport failed.", frontend.ProcessError);
if (result != 0) return result;
if (!File.Exists(Path.Combine(data, "global/pg_control")) || !File.Exists(Path.Combine(data, "PG_VERSION")))
    throw new InvalidOperationException("Original initdb did not create a PostgreSQL cluster.");
if (!operations.Contains(BootstrapOperation.Bootstrap) || !operations.Contains(BootstrapOperation.SingleUser))
    throw new InvalidOperationException("Original backend bootstrap/single-user commands did not run.");
return 0;
