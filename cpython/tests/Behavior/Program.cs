using Managed.CPython;

if (args.Length != 3) throw new ArgumentException("Expected explicit stdlib, private work directory, and suite (behavior/threads/lifecycle/all).");
string library = Path.GetFullPath(args[0]);
string work = Path.GetFullPath(args[1]);
string suite = args[2];
if (suite is not ("behavior" or "threads" or "lifecycle" or "all")) throw new ArgumentException("Unknown suite " + suite);
for (int cycle = 0; cycle < 2; cycle++)
{
    string cycleWork = Path.Combine(work, $"cycle-{cycle}");
    Directory.CreateDirectory(cycleWork);
    int doubles = 0, failures = 0;
    using var runtime = PythonRuntime.Initialize(new PythonOptions(library, Path.Combine(AppContext.BaseDirectory, "fixtures")));
    runtime.RegisterInt64Callback("host", "double", value => { Interlocked.Increment(ref doubles); return checked(value * 2); });
    runtime.RegisterInt64Callback("host", "fail", _ => { Interlocked.Increment(ref failures); throw new InvalidOperationException("managed failure"); });
    using (var runner = runtime.Import("run_required"))
    using (var run = runner.GetAttribute("run"))
    using (var suiteValue = runtime.FromString(suite))
    using (var workValue = runtime.FromString(cycleWork))
    using (var reportValue = runtime.FromString(Path.Combine(cycleWork, "cases.json")))
    using (var result = run.Call(suiteValue, workValue, reportValue))
        Require(result.AsInt64() == 0, "required Python fixture failures; inspect cases.json");
    Require(doubles == (suite switch { "threads" => 2, "lifecycle" => 1, "all" => 3, _ => 0 }), "managed callback count");
    Require(failures == (suite is "lifecycle" or "all" ? 1 : 0), "failing callback count");

    if (suite is "lifecycle" or "all")
    {
        runtime.Execute("import sys\nassert sys.gettrace() is None\n_trace_events = []\ndef user_trace(frame, event, arg):\n    _trace_events.append(event)\n    return user_trace\nsys.settrace(user_trace)\n", "ordinary_trace.py");
        runtime.Execute("assert sys.gettrace() is user_trace\nassert _trace_events\nsys.settrace(None)\n", "ordinary_trace_next_entry.py");
        foreach (string source in new[] { "while True:\n    pass\n", "__import__('host').wait_ms(30000)\n" })
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));
            var duration = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                runtime.Execute(source, "managed_cancellation.py", cancellation.Token);
                throw new InvalidOperationException("Cooperative interruption did not raise a Python exception.");
            }
            catch (PythonException error)
            {
                Require(error.PythonType == "KeyboardInterrupt" && error.Message.Contains("cancelled", StringComparison.Ordinal), "actual cancellation error");
                Require(error.PythonTraceback.Contains("managed_cancellation.py", StringComparison.Ordinal), "interrupted Python frame retained");
            }
            Require(duration.Elapsed < TimeSpan.FromSeconds(3), "cooperative cancellation deadline; external timeout is never a pass");
            using var reusable = runtime.Evaluate("40 + 2");
            Require(reusable.AsInt64() == 42, "same interpreter reusable after cancellation");
        }
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        try
        {
            runtime.Execute("raise AssertionError('pre-cancelled code ran')", cancellationToken: cancelled.Token);
            throw new InvalidOperationException("Pre-cancelled operation unexpectedly ran.");
        }
        catch (PythonException error) { Require(error.PythonType == "KeyboardInterrupt", "pre-cancelled execution rejected by CPython boundary"); }
        try
        {
            runtime.Execute("print('before failure')\nraise ValueError('a\\x00b')", "stream_failure.py");
            throw new InvalidOperationException("Missing Python stream/error failure.");
        }
        catch (PythonException error)
        {
            Require(error.StandardOutput == "before failure\n" && error.StandardError == "", "stream capture retained on exceptional exit");
            Require(error.Message == "a\0b", "length-bearing exception message retains NUL");
            Require(error.PythonType == "ValueError" && error.PythonTraceback.Contains("stream_failure.py", StringComparison.Ordinal), "stream/error traceback");
        }
    }

    using (var owned = runtime.FromInt64(42))
    {
        RequireThrows<InvalidOperationException>(() => runtime.Dispose(), "outstanding reference must block finalize");
        using var clone = owned.Clone();
        owned.Dispose();
        Require(clone.AsInt64() == 42, "clone owns an independent reference");
        RequireThrows<ObjectDisposedException>(() => owned.AsInt64(), "disposed reference access");
    }
    Exception? foreignError = null;
    var foreign = new Thread(() => { try { using var ignored = runtime.Evaluate("42"); } catch (Exception error) { foreignError = error; } });
    foreign.Start();
    Require(foreign.Join(TimeSpan.FromSeconds(5)), "managed owner-thread rejection deadline");
    Require(foreignError is InvalidOperationException, "managed foreign-thread rejection");
    // Worker callback entry stays rooted while owner shutdown joins with the GIL
    // released by CPython. No host lock may span this join.
    int beforeShutdown = Volatile.Read(ref doubles);
    runtime.Execute("import threading, time, host\ndef pending_callback():\n    time.sleep(.03)\n    host.double(21)\ndef pending_host_wait():\n    host.wait_ms(200)\n_pending = threading.Thread(target=pending_callback)\n_pending_wait = threading.Thread(target=pending_host_wait, daemon=True)\n_pending.start()\n_pending_wait.start()\n", "shutdown_worker.py");
    var shutdownDuration = System.Diagnostics.Stopwatch.StartNew();
    runtime.Dispose();
    Require(shutdownDuration.Elapsed < TimeSpan.FromSeconds(3), "worker/active host wait drain deadline");
    Require(Volatile.Read(ref doubles) == beforeShutdown + 1, "worker callback drained before roots released");
    RequireThrows<ObjectDisposedException>(() => runtime.Evaluate("42"), "dead-runtime entry");
    Console.WriteLine($"PASS translated {suite} cycle {cycle + 1}; explicit values released, workers joined, runtime finalized");
}

static void Require(bool condition, string context)
{ if (!condition) throw new InvalidOperationException("CPython behavior assertion: " + context); }

static void RequireThrows<T>(Action action, string context) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidOperationException("CPython behavior assertion: " + context);
}
