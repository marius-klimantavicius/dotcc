using System.Runtime.ExceptionServices;
using System.Text;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

if (args.Length != 5) throw new ArgumentException("mode stdlib app inventory report");
try
{
    if (args[0] == "host")
    {
        // Direct real translated Host exports, on one owner/executor thread.
        // This is a separate process from the production owning API control.
        ExceptionDispatchInfo? failure = null;
        var executor = new Thread(() =>
        {
            try { HostControl.Run(args[1], args[2]); }
            catch (Exception error) { failure = ExceptionDispatchInfo.Capture(error); }
        }, 64 * 1024 * 1024);
        using (ExecutionContext.SuppressFlow()) executor.Start();
        executor.Join();
        failure?.Throw();
        return 0;
    }
    // The SAME static-program owner spans every Python generation.
    using var owner = new VM.Libc.RuntimeContext();
    using var binding = owner.Enter();
    if (args[0] == "startup")
    {
        for (int cycle = 0; cycle < 6; cycle++)
        {
            var options = cycle is 0 or 5 ? new PythonOptions(args[1], args[2]) :
                new PythonOptions(args[1], args[2])
                {
                    WriteBytecode = (cycle - 1) / 2 != 0,
                    Utf8Mode = (cycle - 1) % 2 != 0
                };
            using var runtime = PythonRuntime.Initialize(options);
            runtime.RegisterInt64Callback("host", "double", value => checked(value * 2));
            var output = runtime.Execute(StartupScript(cycle, options.WriteBytecode, options.Utf8Mode) +
                "\nimport host\nassert host.double(21) == 42\n", "owning_startup_r40.py");
            Console.Write(output.StandardOutput);
            using var result = runtime.Evaluate("40 + 2");
            if (result.AsInt64() != 42) throw new InvalidOperationException("Evaluation mismatch");
        }
        Console.WriteLine("owning configured/default startup generations=6 callbacks=6 cache=verified");
        return 0;
    }
    if (args[0] is not ("markers" or "full")) throw new ArgumentException("Unknown mode");
    using (var runtime = PythonRuntime.Initialize(new PythonOptions(args[1], args[2])
    {
        WriteBytecode = true,
        Utf8Mode = false
    }))
    {
        // Actual real flags BEFORE test/support/runner imports. These values are
        // temporary and are disposed before discovery and before finalization.
        using (var bytecode = runtime.Evaluate("int(__import__('sys').dont_write_bytecode)"))
        using (var utf8 = runtime.Evaluate("__import__('sys').flags.utf8_mode"))
        {
            if (bytecode.AsInt64() != 0 || utf8.AsInt64() != 0)
                throw new InvalidOperationException("Qualification startup configuration mismatch");
        }
        using var runner = runtime.Import(args[0] == "full" ? "run_inventory" : "focused_markers");
        using var run = runner.GetAttribute("run_embedded");
        using var inventory = runtime.FromString(args[3]);
        using var report = runtime.FromString(args[4]);
        using var result = run.Call(inventory, report);
        Console.WriteLine(File.ReadAllText(args[4]).Length + " report bytes");
        // The unchanged runner keeps required SKIP/XFAIL as failed gates.
        return checked((int)result.AsInt64());
    }
}
catch (PythonException error)
{
    Console.Write(error.StandardOutput);
    Console.Error.Write(error.StandardError);
    Console.Error.WriteLine(error.PythonType + ": " + error.Message);
    Console.Error.WriteLine(error.PythonTraceback);
    return 2;
}

static string StartupScript(int cycle, bool write, bool utf8) => $$"""
    import sys, os, importlib.util
    assert sys.flags.isolated == 1 and sys.flags.ignore_environment == 1
    assert sys.flags.utf8_mode == {{(utf8 ? 1 : 0)}}
    assert sys.dont_write_bytecode is {{(write ? "False" : "True")}}
    assert 'poison-r40' not in sys.path
    module = __import__('startup_fixture_{{cycle}}')
    assert module.VALUE == 37
    cache = importlib.util.cache_from_source(module.__file__)
    assert os.path.exists(cache) is {{(write ? "True" : "False")}}, cache
    print('startup cycle={{cycle}} write={{(write ? 1 : 0)}} utf8={{(utf8 ? 1 : 0)}} cache=verified')
    """;

static unsafe class HostControl
{
    static byte[] CString(string value) => Encoding.UTF8.GetBytes(value + "\0");
    static string Text(byte* value) => value == null ? "<null>" :
        System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)value)!;
    static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }
    public static void Run(string stdlib, string app)
    {
        using var owner = new VM.Libc.RuntimeContext();
        using var binding = owner.Enter();
        using var policy = owner.UseMinimumPthreadStackSize(64 * 1024 * 1024);
        fixed (byte* library = CString(stdlib))
        fixed (byte* application = CString(app))
        {
            void* context = VM.dcp_create();
            Require(context != null, "Host create");
            foreach (var (write, utf8) in new[] { (-1, 1), (2, 1), (0, -1), (0, 2) })
            {
                Require(VM.dcp_initialize_configured(context, library, application, write, utf8) == 1,
                    "Host invalid boolean status");
                Require(VM.dcp_status_kind(context) == 1 && VM.dcp_status_message(context) != null &&
                    VM.dcp_failure_clean(context) != 0 && VM.Py_IsInitialized() == 0,
                    "Host invalid boolean must be clean before preinitialization");
            }
            VM.dcp_destroy(context);
            for (int cycle = 0; cycle < 6; cycle++)
            {
                int write = cycle is 0 or 5 ? 0 : (cycle - 1) / 2;
                int utf8 = cycle is 0 or 5 ? 1 : (cycle - 1) % 2;
                context = VM.dcp_create();
                Require(context != null, "Host create generation");
                int status = cycle is 0 or 5 ? VM.dcp_initialize(context, library, application) :
                    VM.dcp_initialize_configured(context, library, application, write, utf8);
                Require(status == 0, "Host initialization: " + Text(VM.dcp_status_message(context)));
                VM.dcp_restore(context);
                // Keep the script identical in substance to B37's native C oracle.
                string script = $$"""
                    import sys, os, importlib.util
                    assert sys.flags.isolated == 1 and sys.flags.ignore_environment == 1
                    assert sys.flags.utf8_mode == {{utf8}}
                    assert sys.dont_write_bytecode is {{(write != 0 ? "False" : "True")}}
                    assert 'poison-r40' not in sys.path
                    module = __import__('startup_fixture_{{cycle}}')
                    assert module.VALUE == 37
                    cache = importlib.util.cache_from_source(module.__file__)
                    assert os.path.exists(cache) is {{(write != 0 ? "True" : "False")}}, cache
                    print('startup cycle={{cycle}} write={{write}} utf8={{utf8}} cache=verified')
                    """;
                int result;
                fixed (byte* code = CString(script))
                fixed (byte* filename = CString("host_startup_r40.py"))
                    result = VM.dcp_execute(context, code, filename);
                if (result != 0)
                {
                    VM.dcp_take_error(context);
                    throw new InvalidOperationException(Text(VM.dcp_error_message(context)) + "\n" + Text(VM.dcp_error_traceback(context)));
                }
                ulong length = 0;
                byte* output = VM.dcp_output(context, 0, &length);
                Console.Write(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(output, checked((int)length))));
                VM.dcp_join_workers(context);
                Require(VM.dcp_finalize(context) == 0 && VM.dcp_failure_clean(context) != 0 &&
                    VM.Py_IsInitialized() == 0, "Host finalization");
                VM.dcp_destroy(context);
            }
        }
        Console.WriteLine("invalidBooleanControls=4 clean=true generations=6 defaultsPreserved=true");
    }
}
