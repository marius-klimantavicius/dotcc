namespace Managed.CPython;

// Implement ONLY with calls to the generated Managed.Interpreters.CPython.
// This internal contract cannot be supplied by an embedding application's backend.
// Handles are opaque; actual layouts/constants must come from translated headers.
internal abstract class CPythonExports
{
    // Capture the SAME embedded Libc owner on the external caller, retain it
    // explicitly, then bind it on the executor. Never reference a second Libc.
    internal abstract PythonExecutionContext CaptureExecutionContext();
    // Initialize isolated PyConfig: explicit search paths, no ambient home/site,
    // no signal handlers; apply explicit bytecode/UTF8 options before discovery;
    // clear config on every path. On success SAVE owner tstate
    // and release GIL. On failure report whether the process can safely be reused.
    internal abstract PythonInitializationStatus Initialize(PythonOptions options);
    internal abstract bool InitializationFailureIsClean { get; }
    internal abstract void RestoreOwnerThread();
    internal abstract void SaveOwnerThread();
    internal abstract PythonExecutionResult Execute(string source, string filename, CancellationToken cancellationToken);
    internal abstract nint Evaluate(string expression, string filename, CancellationToken cancellationToken);
    internal abstract nint Import(string module);
    internal abstract nint GetAttribute(nint value, string name);
    internal abstract nint Call(nint callable, ReadOnlySpan<nint> arguments, CancellationToken cancellationToken);
    internal abstract nint FromInt64(long value);
    internal abstract nint FromCodePoints(ReadOnlySpan<uint> value);
    internal abstract long AsInt64(nint value);
    internal abstract uint[] AsCodePoints(nint value);
    internal abstract void IncRef(nint value);
    internal abstract void DecRef(nint value);
    internal abstract bool ErrorOccurred();
    // Consume/clear raised exception, retain exact formatted frames, copy strings,
    // release all temporary Python references; formatting failure remains visible.
    internal abstract PythonException TakeException();
    // METH_O, context capsule owns a monotonically unique CallbackRoots token.
    // Real thunk converts input, calls CallbackRoots.Invoke, creates a NEW result
    // or sets Python RuntimeError and returns NULL. Never unwind managed errors
    // through a C callback. No Marshal.GetFunctionPointerForDelegate/dynamic IL.
    internal abstract void RegisterInt64Callback(string module, string name, long token);
    // While owning GIL, call Python threading shutdown/join path; blocking joins
    // must release GIL using CPython, with NO managed monitor held. Allow worker
    // callbacks until joined. Propagate worker/shutdown errors before finalizing.
    internal abstract void JoinPythonWorkers();
    internal abstract int FinalizeRuntime();
}

internal abstract class PythonExecutionContext : IDisposable
{
    internal abstract IDisposable Enter();
    public abstract void Dispose();
}
