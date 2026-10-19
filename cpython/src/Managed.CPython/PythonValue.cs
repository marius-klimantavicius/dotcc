namespace Managed.CPython;

/// <summary>An owning new CPython reference, bound to its runtime and owner thread.</summary>
public sealed class PythonValue : IDisposable
{
    private readonly PythonRuntime runtime;
    private nint handle;
    internal PythonValue(PythonRuntime runtime, nint handle) { this.runtime = runtime; this.handle = handle; }
    internal nint RequireHandle(PythonRuntime owner)
    {
        owner.RequireExecutionThread();
        if (!ReferenceEquals(runtime, owner)) throw new ArgumentException("Python values cannot cross runtime generations.");
        ObjectDisposedException.ThrowIf(handle == 0, this);
        return handle;
    }
    internal void ClearHandle() => handle = 0;
    internal bool IsReleased => handle == 0;
    public PythonValue GetAttribute(string name) => runtime.Attribute(this, name);
    public PythonValue Call(params ReadOnlySpan<PythonValue> arguments) => runtime.Call(this, arguments);
    public PythonValue Call(CancellationToken cancellationToken, params ReadOnlySpan<PythonValue> arguments) => runtime.Call(this, arguments, cancellationToken);
    public PythonValue Clone() => runtime.Clone(this);
    public long AsInt64() => runtime.AsInt64(this);
    public string AsString(PythonSurrogatePolicy surrogates = PythonSurrogatePolicy.Reject) => runtime.AsString(this, surrogates);
    public void Dispose() => runtime.Release(this);
}
