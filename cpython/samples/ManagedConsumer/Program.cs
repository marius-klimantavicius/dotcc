using Managed.CPython;

// No dependency on Python executables, native libpython, or host Python paths.
// The translated product must deliver its own stdlib beside the application.
string stdlib = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "cpython", "stdlib");
string application = Path.Combine(AppContext.BaseDirectory, "app");
string work = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(Path.GetTempPath(), "dotcc-cpython-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(work);
for (int cycle = 0; cycle < 2; cycle++)
{
    string input = Path.Combine(work, $"input-{cycle}.json");
    string output = Path.Combine(work, $"output-{cycle}.json");
    File.WriteAllBytes(input, "{\"items\":[2,3,5]}"u8.ToArray());
    int callbacks = 0;
    using (var runtime = PythonRuntime.Initialize(new PythonOptions(stdlib, application)))
    {
        runtime.RegisterInt64Callback("host", "double", item => { Interlocked.Increment(ref callbacks); return checked(item * 2); });
        using (var package = runtime.Import("campaign_app"))
        using (var run = package.GetAttribute("run_captured"))
        using (var inputValue = runtime.FromString(input))
        using (var outputValue = runtime.FromString(output))
        using (var result = run.Call(inputValue, outputValue))
            Require(result.AsInt64() == 20, "Python function result");
        Require(callbacks == 3, $"callback count {callbacks}");
        Require(File.ReadAllBytes(output).AsSpan().SequenceEqual("{\"sum\":20,\"values\":[4,6,10]}"u8), "exact JSON bytes");
        using (var package = runtime.Import("campaign_app"))
        using (var stdout = package.GetAttribute("last_stdout"))
        using (var stderr = package.GetAttribute("last_stderr"))
        {
            Require(stdout.AsString() == "workflow: 3 callbacks; sum=20\n", "captured workflow stdout");
            Require(stderr.AsString() == "", "captured workflow stderr");
        }

        // Separate Execute capture contract: both streams are host-owned and
        // restored on exceptional exits by the actual export adapter.
        PythonExecutionResult captured = runtime.Execute("print('capture: λ🐍')\nimport sys\nprint('diagnostic', file=sys.stderr)", "consumer_capture.py");
        Require(captured.StandardOutput == "capture: λ🐍\n", "captured stdout");
        Require(captured.StandardError == "diagnostic\n", "captured stderr");
        using (var traceback = runtime.Evaluate("__import__('campaign_app').caught_traceback()", "consumer_caught.py"))
            Require(traceback.AsString().Contains("ValueError: campaign failure λ🐍", StringComparison.Ordinal), "caught traceback");
        try
        {
            runtime.Execute("__import__('campaign_app').raise_for_host()", "consumer_exception.py");
            throw new InvalidOperationException("Python exception did not reach the host.");
        }
        catch (PythonException error)
        {
            Require(error.PythonType == "ValueError", "exception type");
            Require(error.Message == "campaign failure λ🐍", "exception message");
            Require(error.PythonTraceback.Contains("consumer_exception.py", StringComparison.Ordinal)
                && error.PythonTraceback.Contains("raise_for_host", StringComparison.Ordinal)
                && error.PythonTraceback.Contains("inner", StringComparison.Ordinal), "preserved traceback frames");
        }
        using (var recovered = runtime.Evaluate("40 + 2")) Require(recovered.AsInt64() == 42, "post-error reuse");
        foreach (string value in new[] { "ASCII", "λ🐍", "a\0b", "\ud800x\udfff" })
        {
            using var text = runtime.FromString(value, PythonSurrogatePolicy.Preserve);
            Require(text.AsString(PythonSurrogatePolicy.Preserve) == value, "Unicode/NUL/surrogate round trip");
        }
    }
    Console.WriteLine($"cycle {cycle + 1}: exact JSON, 3 callbacks, return/output/traceback, finalize");
}

static void Require(bool condition, string context)
{
    if (!condition) throw new InvalidOperationException("CPython consumer assertion: " + context);
}
