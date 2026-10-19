using System.Runtime.InteropServices;
using System.Text.Json;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

// Early RAW JIT discovery, before final D10/matrix acceptance. Every Python
// assertion uses the actual owning API and the unchanged inventory runner.
internal static unsafe class Program
{
    private static void Require(bool value, string label)
    {
        if (!value) throw new InvalidOperationException(label);
    }

    private static void Emit(object value) => Console.WriteLine(JsonSerializer.Serialize(value));

    private static void Execute(PythonRuntime runtime, string path)
    {
        var result = runtime.Execute(File.ReadAllText(path), Path.GetFileName(path));
        Console.Write(result.StandardOutput);
        Console.Error.Write(result.StandardError);
    }

    private static int Main(string[] args)
    {
        try { return Run(args); }
        catch (PythonException error)
        {
            Console.Write(error.StandardOutput);
            Console.Error.Write(error.StandardError);
            Console.Error.WriteLine(error.PythonType + ": " + error.Message);
            Console.Error.WriteLine(error.PythonTraceback);
            return 2;
        }
    }

    private static int Run(string[] args)
    {
        if (args.Length != 4 || args[0] is not ("startup" or "suite"))
            throw new ArgumentException("Expected startup|suite, selected stdlib, frozen inventory, report.");
        string stdlib = Path.GetFullPath(args[1]);
        string inventory = Path.GetFullPath(args[2]);
        string report = Path.GetFullPath(args[3]);
        string application = Path.Combine(AppContext.BaseDirectory, "upstream");
        string controls = Path.Combine(AppContext.BaseDirectory, "controls");
        Directory.CreateDirectory(Path.GetDirectoryName(report)!);
        Require(File.Exists(inventory), "Frozen inventory is missing.");
        Require(VM.Py_IsInitialized() == 0, "Interpreter already initialized.");

        string version = Marshal.PtrToStringUTF8((nint)VM.Py_GetVersion())!;
        Require(version.StartsWith("3.13.15 ", StringComparison.Ordinal), "Unexpected CPython version.");
        var layout = new Dictionary<string, long>
        {
            ["pointer"] = sizeof(void*), ["long"] = sizeof(long), ["wchar_size"] = sizeof(char),
            ["config_size"] = sizeof(VM.PyConfig), ["preconfig_size"] = sizeof(VM.PyPreConfig),
            ["list_size"] = sizeof(VM.PyWideStringList),
            ["stdlib_dir"] = (long)Marshal.OffsetOf<VM.PyConfig>("stdlib_dir"),
            ["module_search_paths"] = (long)Marshal.OffsetOf<VM.PyConfig>("module_search_paths"),
        };
        Require(layout["pointer"] == 8 && layout["long"] == 8 && layout["wchar_size"] == 2,
            "Expected Linux x64 LP64/unsigned wchar16 selected ABI.");
        Emit(new { scope = "RAW JIT discovery; final D10 acceptance remains open", version,
            entryMvid = typeof(Program).Module.ModuleVersionId,
            selectedLibraryMvid = typeof(VM).Module.ModuleVersionId, layout,
            runtimeProfile = new { framework = RuntimeInformation.FrameworkDescription,
                version = Environment.Version.ToString(), rid = RuntimeInformation.RuntimeIdentifier,
                architecture = RuntimeInformation.ProcessArchitecture.ToString() } });

        if (args[0] == "startup")
        {
            Require(Environment.GetEnvironmentVariable("DOTCC_B29_ORIGINAL_FOUR") == "1",
                "Startup control must execute the four unchanged frozen finder IDs.");
            int callbacks = 0;
            for (int cycle = 0; cycle < 2; cycle++)
            {
                using (var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, application)))
                {
                    Require(VM.Py_IsInitialized() != 0, "Owning startup did not initialize CPython.");
                    runtime.RegisterInt64Callback("host", "double", value => { callbacks++; return checked(value * 2); });
                    Execute(runtime, Path.Combine(controls, "frozen_startup_checks_r29.py"));
                    Execute(runtime, Path.Combine(controls, "getcwd_startup_r30.py"));
                    using var answer = runtime.Evaluate("__import__('host').double(21)");
                    Require(answer.AsInt64() == 42, "Actual owning callback returned wrong value.");
                }
                Require(VM.Py_IsInitialized() == 0, "Actual owning finalization left CPython initialized.");
                Emit(new { owningStartupCycle = cycle, finalized = true });
            }
            Require(callbacks == 2, "Expected two actual owning callbacks.");
            File.WriteAllText(report, JsonSerializer.Serialize(new { status = "passed", cycles = 2, callbacks,
                originalFrozenFinderExecutions = 8, layout, version }));
            return 0;
        }

        using (var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, application)))
        {
            using (var pid = runtime.Evaluate("__import__('os').getpid()"))
                Require(pid.AsInt64() == Environment.ProcessId, "GETPID bootstrap mismatch.");
            using (var hostname = runtime.Evaluate("__import__('socket').gethostname()"))
                Require(hostname.AsString() == System.Net.Dns.GetHostName(), "Hostname bootstrap mismatch.");
            using var runner = runtime.Import("run_inventory");
            using var run = runner.GetAttribute("run_embedded");
            using var inventoryValue = runtime.FromString(inventory);
            using var reportValue = runtime.FromString(report);
            using var actionValue = runtime.FromString("qualify");
            using var code = run.Call(inventoryValue, reportValue, actionValue);
            using var document = JsonDocument.Parse(File.ReadAllText(report));
            Emit(document.RootElement.GetProperty("counts"));
            return code.AsInt64() == 0 ? 0 : 1;
        }
    }
}
