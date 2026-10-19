using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

// Diagnostic only. Each invocation is a separate disposable process. The whole
// owning lifecycle and its callbacks stay on one thread, including the 64 MiB
// comparison. This does not implement dispatch or a production stack guard.
internal static unsafe class Program
{
    private static object? guardObservation;

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private static int Main(string[] args)
    {
        if (args.Length != 6 || args[0] is not ("exact-case" or "guard") || args[1] is not ("default" or "64m"))
            throw new ArgumentException("Expected exact-case|guard, default|64m, stdlib, application, input-manifest, report.");
        int status = 3;
        Exception? failure = null;
        void Execute()
        {
            try { status = Run(args); }
            catch (Exception error) { failure = error; }
        }
        if (args[1] == "default") Execute();
        else
        {
            var thread = new Thread(Execute, 64 * 1024 * 1024);
            thread.Start();
            thread.Join();
        }
        if (failure != null) { Console.Error.WriteLine(failure); return 2; }
        return status;
    }

    private static int Run(string[] args)
    {
        using var inputs = JsonDocument.Parse(File.ReadAllText(args[4]));
        void Bind(string key, Type type)
        {
            var expected = inputs.RootElement.GetProperty(key);
            string path = type.Assembly.Location;
            Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).Equals(
                expected.GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase), "Wrong " + key + " PE bytes.");
            Require(type.Module.ModuleVersionId == Guid.Parse(expected.GetProperty("mvid").GetString()!), "Wrong " + key + " MVID.");
        }
        Bind("library", typeof(VM));
        Bind("facade", typeof(PythonRuntime));
        int owner = Environment.CurrentManagedThreadId, callbacks = 0;
        Console.WriteLine(JsonSerializer.Serialize(new { diagnostic = args[0], stack = args[1], owner,
            libraryMvid = typeof(VM).Module.ModuleVersionId, facadeMvid = typeof(PythonRuntime).Module.ModuleVersionId,
            framework = RuntimeInformation.FrameworkDescription }));
        using (var runtime = PythonRuntime.Initialize(new PythonOptions(Path.GetFullPath(args[2]), Path.GetFullPath(args[3]))))
        {
            runtime.RegisterInt64Callback("r27_guard", "probe", _ =>
            {
                Require(Environment.CurrentManagedThreadId == owner, "Callback changed owning thread.");
                callbacks++;
                return GuardCounters();
            });
            if (args[0] == "guard")
            {
                using var result = runtime.Evaluate("__import__('r27_guard').probe(0)");
                Require(result.AsInt64() == 1, "Guard counter probe failed.");
            }
            else
            {
                var result = runtime.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "exact_case.py")), "r27_exact_case.py");
                Console.Write(result.StandardOutput);
                Console.Error.Write(result.StandardError);
            }
            // A real callback and ordinary evaluation after guard/error unwinding.
            using var answer = runtime.Evaluate("6 * 7");
            Require(answer.AsInt64() == 42 && Environment.CurrentManagedThreadId == owner, "Recovery/owner mismatch.");
        }
        Require(VM.Py_IsInitialized() == 0, "Interpreter did not finalize.");
        File.WriteAllText(args[5], JsonSerializer.Serialize(new { status = "passed", diagnostic = args[0], stack = args[1],
            owner, callbacks, guardObservation, finalized = true, productionGuardFix = false }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private static int GuardCounters()
    {
        // Enter/leave the REAL compiled API without recursive machine frames.
        // No writes to a threshold, recursion budget or headroom field occur.
        var state = VM.PyThreadState_GetUnchecked();
        Require(state != null && VM.PyErr_Occurred() == null, "Expected attached clean Python thread.");
        int initial = state->c_recursion_remaining, headroom = state->recursion_headroom;
        Require(initial > 0 && headroom == 0, "Unexpected initial guard state.");
        int entered = 0, failedRemaining = 0;
        try
        {
            fixed (byte* where = " in A27 guard counter probe\0"u8)
            {
                while (entered <= initial + 1)
                {
                    if (VM.Py_EnterRecursiveCall(where) != 0) break;
                    entered++;
                }
                Require(entered == initial + 1, "C enter guard did not use the pin's postdecrement boundary.");
                failedRemaining = state->c_recursion_remaining;
                Require(VM.PyErr_ExceptionMatches((VM._object*)VM.Globals.PyExc_RecursionError) != 0,
                    "Guard did not set RecursionError.");
                Require(state->recursion_headroom == headroom, "Guard leaked recursion headroom.");
                VM.PyErr_Clear();
                Require(VM.Py_EnterRecursiveCall(where) != 0 && state->c_recursion_remaining == failedRemaining,
                    "Failed reentry consumed a successful-entry slot.");
                Require(VM.PyErr_ExceptionMatches((VM._object*)VM.Globals.PyExc_RecursionError) != 0,
                    "Repeated overflow did not retain RecursionError semantics.");
                VM.PyErr_Clear();
            }
        }
        finally
        {
            for (int i = 0; i < entered; i++) VM.Py_LeaveRecursiveCall();
        }
        Require(state->c_recursion_remaining == initial && state->recursion_headroom == headroom,
            "Successful enter/leave pairs did not restore the budget/headroom.");
        fixed (byte* where = " after A27 guard unwind\0"u8)
            Require(VM.Py_EnterRecursiveCall(where) == 0, "Shallow enter after unwind failed.");
        VM.Py_LeaveRecursiveCall();
        Require(state->c_recursion_remaining == initial && VM.PyErr_Occurred() == null, "Recovery left changed state.");
        guardObservation = new { initial, entered, failedRemaining, headroom, restored = state->c_recursion_remaining };
        return 1;
    }
}
