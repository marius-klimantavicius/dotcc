using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Managed.Libuv.Native;

public enum NativeLibraryKind
{
    CRuntime, Threads, DynamicLoader, Realtime,
    WindowsKernel, WindowsSockets, WindowsNt,
    AppleCoreFoundation, AppleCoreServices
}

/// <summary>
/// Lazy process-lifetime native module. Addresses stay valid for callbacks/resources.
/// This class does not infer signatures or replace upstream optional-feature policy.
/// </summary>
public sealed class NativeModule
{
    private readonly Lazy<nint> handle;
    private readonly ConcurrentDictionary<string, nint> symbols = new(StringComparer.Ordinal);
    private readonly string[] names;

    internal NativeModule(NativeLibraryKind kind, string[] names, Action<NativeLibraryKind, nint> validate)
    {
        Kind = kind;
        this.names = names;
        handle = new(() => Load(validate), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NativeLibraryKind Kind { get; }
    public bool IsLoaded => handle.IsValueCreated && handle.Value != 0;

    /// <summary>Absence may trigger upstream fallback; invalid provider identity still throws.</summary>
    public bool TryLoad() => handle.Value != 0;

    /// <summary>Absent libraries and exports remain absent so the caller can preserve upstream fallback.</summary>
    public bool TryGetExport(string symbol, out nint address)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        if (symbol.Contains('\0')) throw new ArgumentException("Export names cannot contain NUL.", nameof(symbol));
        address = symbols.GetOrAdd(symbol, name => TryLoad() && NativeLibrary.TryGetExport(handle.Value, name, out var value) ? value : 0);
        return address != 0;
    }

    public nint GetRequiredExport(string symbol)
    {
        if (!TryLoad())
            throw new DllNotFoundException($"Required native library {Kind} is unavailable ({string.Join(", ", names)}).");
        if (TryGetExport(symbol, out var address)) return address;
        throw new EntryPointNotFoundException($"Required native export {symbol} is missing from {Kind} ({string.Join(", ", names)}).");
    }

    private nint Load(Action<NativeLibraryKind, nint> validate)
    {
        foreach (var name in names)
        {
            if (!NativeLibrary.TryLoad(name, out var loaded)) continue;
            try { validate(Kind, loaded); }
            catch { NativeLibrary.Free(loaded); throw; }
            // Deliberately retained for the process. No finalizer can race native users.
            return loaded;
        }
        return 0;
    }
}
