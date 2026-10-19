using Managed.CPython;
using System.Text.Json;

if (args.Length != 3) throw new ArgumentException("Expected stdlib, report, and normal/missing-stdlib/missing-json/missing-app.");
string library = Path.GetFullPath(args[0]);
string report = Path.GetFullPath(args[1]);
string scenario = args[2];
string application = Path.Combine(AppContext.BaseDirectory, "app");
try
{
    using var runtime = PythonRuntime.Initialize(new PythonOptions(library, application));
    if (scenario == "missing-stdlib") throw new InvalidOperationException("Missing startup assets were accepted.");
    if (scenario == "missing-json")
    {
        using var unexpected = runtime.Import("json");
        throw new InvalidOperationException("Missing delivered json silently fell back to ambient Python.");
    }
    if (scenario == "missing-app")
    {
        using var unexpected = runtime.Import("delivery_contract");
        throw new InvalidOperationException("Missing application silently fell back to ambient imports.");
    }
    if (scenario != "normal") throw new ArgumentException("Unknown delivery scenario " + scenario);
    using var contract = runtime.Import("delivery_contract");
    using var run = contract.GetAttribute("run");
    using var libraryValue = runtime.FromString(library);
    using var applicationValue = runtime.FromString(application);
    using var result = run.Call(libraryValue, applicationValue);
    File.WriteAllText(report, result.AsString());
}
catch (PythonInitializationException error) when (scenario == "missing-stdlib")
{
    Require(error.Status.Kind == PythonStatusKind.Error, "missing startup must return real PyStatus Error");
    Require(!string.IsNullOrWhiteSpace(error.Status.Message), "missing startup diagnostic");
    WriteNegative(error.GetType().Name, error.Message, error.Status.Function);
}
catch (DirectoryNotFoundException error) when (scenario == "missing-stdlib")
{
    // A wholly absent directory is rejected by the owning API before entry.
    WriteNegative(error.GetType().Name, error.Message, null);
}
catch (PythonException error) when (scenario is "missing-json" or "missing-app")
{
    Require(error.PythonType == "ModuleNotFoundError", "missing module error type");
    string module = scenario == "missing-json" ? "json" : "delivery_contract";
    Require(error.Message.Contains(module, StringComparison.Ordinal), "missing module diagnostic names asset");
    WriteNegative(error.PythonType, error.Message, null);
}

void WriteNegative(string type, string message, string? function)
{
    using var stream = File.Create(report);
    using var writer = new Utf8JsonWriter(stream);
    writer.WriteStartObject();
    writer.WriteString("scenario", scenario);
    writer.WriteString("status", "expected-error");
    writer.WriteString("type", type);
    writer.WriteString("message", message);
    writer.WriteString("function", function);
    writer.WriteEndObject();
    writer.Flush();
    Console.WriteLine($"PASS {scenario}: {type}: {message}");
}

static void Require(bool value, string message)
{
    if (!value) throw new InvalidOperationException("CPython delivery assertion: " + message);
}
