using Managed.CPython;
using System.Text.Json;
using VM = Managed.Interpreters.CPython;

// Every Python import and unittest assertion executes inside the selected actual
// translated profile, through ordinary owning API/project references.
if (args.Length is < 3 or > 4)
    throw new ArgumentException("Expected explicit test-profile stdlib, inventory, report, and optional discover/qualify.");
string stdlib = Path.GetFullPath(args[0]);
string inventory = Path.GetFullPath(args[1]);
string report = Path.GetFullPath(args[2]);
string action = args.Length == 4 ? args[3] : "qualify";
if (action is not ("qualify" or "discover")) throw new ArgumentException("Unknown upstream action " + action);
if (!File.Exists(inventory)) throw new FileNotFoundException("Required upstream inventory", inventory);
Directory.CreateDirectory(Path.GetDirectoryName(report)!);
// Isolated preinitialization deliberately preserves the consumer's C locale.
// Select the native-matched qualification locale on the exact existing Libc
// program owner BEFORE Python startup. Keep its nullable binding unchanged.
var qualificationOwner = VM.Libc.CaptureRuntimeContext();
unsafe
{
    fixed (byte* locale = "C.UTF-8\0"u8)
        if (VM.Libc.setlocale(0, locale) == null)
            throw new InvalidOperationException("Qualification consumer locale C.UTF-8 is unavailable.");
}
if (!ReferenceEquals(qualificationOwner, VM.Libc.CaptureRuntimeContext()))
    throw new InvalidOperationException("Qualification program owner changed before startup.");
// Qualification uses the consumer-owned copied test stdlib/application/cache.
// Product defaults remain unchanged; select real config before any discovery.
using (var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, Path.Combine(AppContext.BaseDirectory, "upstream"))
{
    WriteBytecode = true,
    Utf8Mode = false
}))
{
    using (var bytecode = runtime.Evaluate("int(__import__('sys').dont_write_bytecode)", "qualification_startup.py"))
    using (var utf8 = runtime.Evaluate("__import__('sys').flags.utf8_mode", "qualification_startup.py"))
    {
        if (bytecode.AsInt64() != 0 || utf8.AsInt64() != 0)
            throw new InvalidOperationException("Upstream qualification startup options were not applied before discovery.");
    }
    // Execute the newly selected API through the owning boundary in this
    // consumer, before import discovery can rely on it for support filenames.
    using (var firstPid = runtime.Evaluate("__import__('os').getpid()", "getpid_bootstrap.py"))
    using (var repeatedPid = runtime.Evaluate("__import__('os').getpid()", "getpid_bootstrap.py"))
    {
        long actual = firstPid.AsInt64();
        if (actual <= 0 || actual != Environment.ProcessId || repeatedPid.AsInt64() != actual)
            throw new InvalidOperationException("Translated GETPID bootstrap identity mismatch.");
        Console.WriteLine($"GETPID bootstrap passed: {actual}");
    }
    using (var firstHostname = runtime.Evaluate("__import__('socket').gethostname()", "hostname_bootstrap.py"))
    using (var repeatedHostname = runtime.Evaluate("__import__('socket').gethostname()", "hostname_bootstrap.py"))
    {
        string actual = firstHostname.AsString();
        if (actual.Length == 0 || actual != System.Net.Dns.GetHostName() || repeatedHostname.AsString() != actual)
            throw new InvalidOperationException("Translated hostname bootstrap identity mismatch.");
        Console.WriteLine($"Hostname bootstrap passed: {actual}");
    }
using (var runner = runtime.Import("run_inventory"))
using (var run = runner.GetAttribute("run_embedded"))
using (var inventoryValue = runtime.FromString(inventory))
using (var reportValue = runtime.FromString(report))
using (var actionValue = runtime.FromString(action))
using (var code = run.Call(inventoryValue, reportValue, actionValue))
{
    // Preserve the unchanged runner's raw exit, including declared SKIP/XFAIL.
    // The ordinary campaign derives any authorized acceptance separately.
    if (code.AsInt64() != 0) return checked((int)code.AsInt64());
    using var document = JsonDocument.Parse(File.ReadAllText(report));
    var root = document.RootElement;
    if (root.GetProperty("implementation").GetString() != "translated" || root.GetProperty("status").GetString() != "passed")
        throw new InvalidOperationException("Invalid translated upstream report identity/status.");
    if (action == "qualify" && (!root.GetProperty("qualifies_translated_interpreter").GetBoolean() || root.GetProperty("executed").GetInt32() == 0))
        throw new InvalidOperationException("Empty/partial/native upstream report cannot qualify.");
    Console.WriteLine(root.GetProperty("counts").GetRawText());
}
}
return 0;
