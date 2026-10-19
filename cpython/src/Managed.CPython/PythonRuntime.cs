namespace Managed.CPython;

public sealed partial class PythonRuntime : IDisposable
{
    private readonly CPythonExports exports;
    private readonly RuntimeOwnership ownership;
    private readonly PythonExecutionThread execution;
    private readonly PythonExecutionContext executionContext;
    private readonly List<long> callbackTokens = new();
    private bool disposed;
    // Owned by the external initializing caller, including its callback pump.
    private bool entered;

    internal PythonRuntime(CPythonExports exports, PythonOptions options)
    {
        this.exports = exports;
        ownership = RuntimeOwnership.Claim();
        PythonExecutionContext? captured = null;
        PythonExecutionThread? executor = null;
        bool initializationAttempted = false;
        try
        {
            captured = exports.CaptureExecutionContext();
            executionContext = captured;
            executor = new PythonExecutionThread(() =>
            {
                ownership.BindExecutionThread();
                return captured.Enter();
            });
            execution = executor;
            executor.Invoke(() =>
            {
                ownership.RequireExecutionThread();
                initializationAttempted = true;
                exports.Initialize(options).ThrowIfNotOk();
            });
        }
        catch
        {
            if (!initializationAttempted || exports.InitializationFailureIsClean)
            {
                // Do not release the claim before a started executor has exited
                // and its original runtime binding has been restored.
                try { executor?.Dispose(); captured?.Dispose(); ownership.ReleaseCleanInitializationFailure(); }
                catch { ownership.MarkFaulted(); throw; }
            }
            else ownership.MarkFaulted(); // Retain context/policy and process claim.
            throw;
        }
    }

    private void Authorize()
    {
        ownership.RequireOwner();
        if (entered) throw new InvalidOperationException("Reentrant owning API calls are not supported from a callback.");
    }

    private T Run<T>(Func<T> operation)
    {
        Authorize();
        entered = true;
        try
        {
            return execution.Invoke(() =>
            {
                ownership.RequireExecutionThread();
                try { exports.RestoreOwnerThread(); }
                catch { ownership.MarkFaulted(); throw; }
                try { return operation(); }
                finally
                {
                    try { exports.SaveOwnerThread(); }
                    catch { ownership.MarkFaulted(); throw; }
                }
            });
        }
        catch { if (execution.IsFaulted) ownership.MarkFaulted(); throw; }
        finally { entered = false; }
    }
    private void Run(Action operation) => Run(() => { operation(); return 0; });
    internal void RequireExecutionThread() => ownership.RequireExecutionThread();
    private void CheckError() { if (exports.ErrorOccurred()) throw exports.TakeException(); }

    private PythonValue Own(nint handle)
    {
        ownership.RequireExecutionThread();
        if (handle == 0)
        {
            CheckError();
            throw new InvalidOperationException("CPython returned NULL without a Python error.");
        }
        if (exports.ErrorOccurred())
        {
            PythonException failure = exports.TakeException();
            exports.DecRef(handle);
            throw failure;
        }
        try { var value = new PythonValue(this, handle); ownership.AddValue(); return value; }
        catch { exports.DecRef(handle); throw; }
    }

    public PythonExecutionResult Execute(string source, string filename = "<managed>", CancellationToken cancellationToken = default)
    {
        PythonText.RequireCString(source, nameof(source)); PythonText.RequireCString(filename, nameof(filename));
        return Run(() => { var result = exports.Execute(source, filename, cancellationToken); CheckError(); return result; });
    }
    public PythonValue Evaluate(string expression, string filename = "<managed>", CancellationToken cancellationToken = default)
    {
        PythonText.RequireCString(expression, nameof(expression)); PythonText.RequireCString(filename, nameof(filename));
        return Run(() => Own(exports.Evaluate(expression, filename, cancellationToken)));
    }
    public PythonValue Import(string module)
    {
        PythonText.RequireCString(module, nameof(module));
        return Run(() => Own(exports.Import(module)));
    }
    public PythonValue FromInt64(long value) => Run(() => Own(exports.FromInt64(value)));
    public PythonValue FromString(string value, PythonSurrogatePolicy surrogates = PythonSurrogatePolicy.Reject)
    {
        Authorize();
        uint[] points = PythonText.ToCodePoints(value, surrogates);
        return Run(() => Own(exports.FromCodePoints(points)));
    }

    public void RegisterInt64Callback(string module, string name, Func<long, long> callback)
    {
        PythonText.RequireCString(module, nameof(module)); PythonText.RequireCString(name, nameof(name));
        Run(() =>
        {
            // Retain even after partial Python publication. Active accounting in
            // CallbackRoots includes the entire executor/owner callback roundtrip.
            long token = CallbackRoots.Add(callback, execution);
            try { callbackTokens.Add(token); }
            catch { CallbackRoots.RemoveAfterDrain(token); throw; }
            exports.RegisterInt64Callback(module, name, token); CheckError();
        });
    }
    internal PythonValue Attribute(PythonValue value, string name)
    {
        PythonText.RequireCString(name, nameof(name));
        return Run(() => Own(exports.GetAttribute(value.RequireHandle(this), name)));
    }
    internal PythonValue Call(PythonValue callable, ReadOnlySpan<PythonValue> arguments, CancellationToken cancellationToken = default)
    {
        Authorize();
        // A span cannot survive submission. The request owns this copy until the
        // interpreter and every synchronous callback have completed.
        PythonValue[] copied = arguments.ToArray();
        return Run(() =>
        {
            var handles = new nint[copied.Length];
            for (int i = 0; i < copied.Length; i++) handles[i] = copied[i].RequireHandle(this);
            return Own(exports.Call(callable.RequireHandle(this), handles, cancellationToken));
        });
    }
    internal long AsInt64(PythonValue value) => Run(() =>
    { long result = exports.AsInt64(value.RequireHandle(this)); CheckError(); return result; });
    internal string AsString(PythonValue value, PythonSurrogatePolicy policy) => Run(() =>
    { uint[] points = exports.AsCodePoints(value.RequireHandle(this)); CheckError(); return PythonText.FromCodePoints(points, policy); });
    internal PythonValue Clone(PythonValue value) => Run(() =>
    { nint handle = value.RequireHandle(this); exports.IncRef(handle); return Own(handle); });
    internal void Release(PythonValue value)
    {
        ownership.RequireCallerThread();
        if (entered) throw new InvalidOperationException("Reentrant owning API calls are not supported from a callback.");
        if (value.IsReleased) return;
        Run(() =>
        {
            nint handle = value.RequireHandle(this);
            value.ClearHandle(); // Destructor reentry cannot see a live handle.
            try { exports.DecRef(handle); }
            finally { ownership.ReleaseValue(); }
            CheckError();
        });
    }

    // Explicit external-owner disposal only. No runtime/value finalizer calls Python.
    public void Dispose()
    {
        ownership.RequireCallerThread();
        if (disposed) return;
        Authorize();
        entered = true;
        bool finalized = false;
        try
        {
            int result = execution.Invoke(() =>
            {
                ownership.RequireExecutionThread();
                // Reject live references before changing any interpreter state,
                // allowing their release and another Dispose attempt.
                ownership.BeginStop();
                try { exports.RestoreOwnerThread(); }
                catch { ownership.MarkFaulted(); throw; }
                try { exports.JoinPythonWorkers(); CheckError(); }
                catch
                {
                    ownership.MarkFaulted();
                    exports.SaveOwnerThread();
                    throw;
                }
                int status;
                try { status = exports.FinalizeRuntime(); }
                catch { ownership.MarkFaulted(); throw; }
                finalized = true;
                disposed = true;
                foreach (long token in callbackTokens) CallbackRoots.RemoveAfterDrain(token);
                callbackTokens.Clear();
                ownership.MarkFinalized();
                return status;
            });
            if (result != 0) throw new InvalidOperationException($"Py_FinalizeEx returned {result}.");
        }
        catch { if (execution.IsFaulted) ownership.MarkFaulted(); throw; }
        finally
        {
            entered = false;
            if (finalized)
            {
                // A completed finalization is the only ordinary shutdown path
                // permitted to stop the executor and retire its stack policy.
                try { execution.Dispose(); }
                finally { executionContext.Dispose(); }
            }
        }
    }
}
