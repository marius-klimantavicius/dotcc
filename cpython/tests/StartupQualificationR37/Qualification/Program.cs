using System.Text.Json;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

// This owning harness requires an ordinary copy of the selected test stdlib.
// It never writes into upstream ref/source, changes selection or edits flags.
if (args.Length != 3 || args[2] is not ("0" or "1")) return 20;
string stdlib = Path.GetFullPath(args[0]);
string report = Path.GetFullPath(args[1]);
bool utf8 = args[2] == "1";
string application = Path.Combine(AppContext.BaseDirectory, "upstream");
// Match the native embedded consumer's locale before isolated preinitialization.
// The existing program context and nullable binding span the runtime lifetime.
var qualificationOwner = VM.Libc.CaptureRuntimeContext();
unsafe
{
    fixed (byte* locale = "C.UTF-8\0"u8)
        if (VM.Libc.setlocale(0, locale) == null)
            throw new InvalidOperationException("Qualification consumer locale C.UTF-8 is unavailable.");
}
if (!ReferenceEquals(qualificationOwner, VM.Libc.CaptureRuntimeContext()))
    throw new InvalidOperationException("Qualification program owner changed before startup.");
using var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, application)
{
    WriteBytecode = true,
    Utf8Mode = utf8
});
runtime.Execute($"import sys\nassert sys.flags.isolated == 1 and sys.flags.ignore_environment == 1\nassert not sys.dont_write_bytecode\nassert sys.flags.utf8_mode == {(utf8 ? 1 : 0)}\n", "qualification_startup_r37.py");
using var runner = runtime.Import("run_inventory");
using var run = runner.GetAttribute("run_embedded");
using var inventory = runtime.FromString(Path.Combine(application, "inventory.frozen.json"));
using var reportValue = runtime.FromString(report);
using var action = runtime.FromString("qualify");
using var result = run.Call(inventory, reportValue, action);
using var document = JsonDocument.Parse(File.ReadAllText(report));
var root = document.RootElement;
Console.WriteLine(root.GetProperty("counts").GetRawText());
// Required SKIP/XFAIL remain failures under the unchanged production runner.
return result.AsInt64() == 0 && root.GetProperty("implementation").GetString() == "translated"
    && root.GetProperty("qualifies_translated_interpreter").GetBoolean() ? 0 : 1;
