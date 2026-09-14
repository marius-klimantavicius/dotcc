using System.Runtime.InteropServices;
using VM = Managed.Interpreters.Pinta;

namespace Managed.Interpreters;

public sealed class PintaException(uint status) : Exception($"Pinta returned status {status}.")
{
    public uint Status { get; } = status;
}

/// <summary>A module belongs to its creating engine and expires when that engine is disposed.</summary>
public sealed class PintaModule
{
    internal PintaModule(PintaEngine owner, nint address) => (Owner, Address) = (owner, address);
    internal PintaEngine Owner { get; }
    internal nint Address { get; }
}

/// <summary>Owns stable unmanaged arena storage and copies all results before another VM call.</summary>
public sealed unsafe partial class PintaEngine : IDisposable
{
    private readonly object gate = new();
    private readonly Host host;
    private GCHandle context;
    private void* allocation;
    private void* arena;
    private readonly uint arenaLength;
    private const int GuardBytes = 32;
    private VM.PintaApi* api;
    private bool busy;
    private bool disposed;
    private bool executionStarted;

    public PintaEngine(IReadOnlyDictionary<string, byte[]> modules,
        uint arenaBytes = 4 * 1024 * 1024, uint heapBytes = 1024 * 1024,
        uint stackBytes = 64 * 1024, uint initialOutputBytes = 2816)
    {
        ArgumentNullException.ThrowIfNull(modules);
        // Frame offsets count references; the u32 byte allocation is the tighter x64 bound.
        if (IntPtr.Size != 8) throw new PlatformNotSupportedException("Pinta is qualified for 64-bit hosts only.");
        if (heapBytes < 1024 || stackBytes < 128 ||
            stackBytes > uint.MaxValue - (uint)sizeof(VM.PintaStackFrame) ||
            (ulong)stackBytes / (uint)sizeof(VM.PintaReference) > 0x3fffffffUL ||
            (ulong)heapBytes + stackBytes >= arenaBytes)
            throw new ArgumentOutOfRangeException(nameof(arenaBytes), "Invalid arena, heap or stack budget.");
        host = new Host(modules);
        host.Owner = new WeakReference<PintaEngine>(this);
        arenaLength = arenaBytes;
        try
        {
            allocation = NativeMemory.AllocZeroed((nuint)arenaBytes + 2 * GuardBytes);
            if (allocation == null) throw new OutOfMemoryException();
            arena = (byte*)allocation + GuardBytes;
            new Span<byte>(allocation, GuardBytes).Fill(0xa5);
            new Span<byte>((byte*)arena + arenaBytes, GuardBytes).Fill(0xa5);
            context = GCHandle.Alloc(host);
            VM.PintaApiEnvironment environment = new()
            {
                memory = arena, memory_length = arenaBytes,
                heap_length = heapBytes, stack_length = stackBytes,
                initial_output_capacity = initialOutputBytes,
                platform_encoding = (VM.PintaApiEncoding)2,
                environment_context = (void*)GCHandle.ToIntPtr(context),
                file_open = &Open, file_size = &Size, file_read = &Read, file_close = &Close
            };
            api = VM.pinta_api_create(&environment);
            if (api == null) throw new OutOfMemoryException("The arena cannot initialize Pinta with these budgets.");
        }
        catch { Release(); throw; }
    }

    ~PintaEngine() => Release();

    public PintaModule LoadModule(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        lock (gate)
        {
            Enter();
            try
            {
                fixed (char* text = name)
                {
                    var value = Text(text, name.Length);
                    void* module = api->load_module(api, &value);
                    if (module == null) throw new InvalidOperationException($"Pinta could not load module '{name}'.");
                    Check((uint)VM.pinta_api_set_builtin_objects((VM.PintaCore*)api->core, (VM.PintaModuleDomain*)module));
                    return new PintaModule(this, (nint)module);
                }
            }
            finally { Leave(); }
        }
    }

    public void SetInteger(PintaModule module, string name, int value) => Set(module, name, value, null, 0);
    public void SetString(PintaModule module, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Set(module, name, 0, value, 1);
    }
    public void SetNull(PintaModule module, string name) => Set(module, name, 0, null, 2);

    private void Set(PintaModule module, string name, int integer, string? value, int kind)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        lock (gate)
        {
            Enter();
            try
            {
                Validate(module);
                fixed (char* key = name)
                fixed (char* data = value)
                {
                    var keyString = Text(key, name.Length);
                    var valueString = Text(data, value?.Length ?? 0);
                    Check(kind switch
                    {
                        0 => api->set_integer(api, (void*)module.Address, &keyString, integer),
                        1 => api->set_string(api, (void*)module.Address, &keyString, &valueString),
                        _ => api->set_null(api, (void*)module.Address, &keyString)
                    });
                }
            }
            finally { Leave(); }
        }
    }

    public void Execute(PintaModule module)
    {
        lock (gate)
        {
            Enter();
            try
            {
                Validate(module);
                if (executionStarted)
                    throw new InvalidOperationException("This upstream engine executes once. Create another engine to execute again.");
                executionStarted = true;
                Check(api->execute(api, (void*)module.Address));
            }
            finally { Leave(); }
        }
    }

    public string? GetString(PintaModule module, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        lock (gate)
        {
            Enter();
            try
            {
                Validate(module);
                fixed (char* key = name)
                {
                    var keyString = Text(key, name.Length);
                    uint length = 0;
                    void* data = null;
                    Check(api->unsafe_get_string(api, (void*)module.Address, &keyString,
                        (VM.PintaApiEncoding)2, &length, &data));
                    return data == null ? null : new string((char*)data, 0, checked((int)length));
                }
            }
            finally { Leave(); }
        }
    }

    public byte[] CopyOutput()
    {
        lock (gate)
        {
            Enter();
            try
            {
                uint length = 0;
                void* data = null;
                Check(api->unsafe_get_output_buffer(api, &length, &data));
                return new ReadOnlySpan<byte>(data, checked((int)length)).ToArray();
            }
            finally { Leave(); }
        }
    }

    public string GetOutputString()
    {
        lock (gate)
        {
            Enter();
            try
            {
                uint length = 0;
                void* data = null;
                Check(api->unsafe_get_output_string(api, (VM.PintaApiEncoding)2, &length, &data));
                return data == null ? "" : new string((char*)data, 0, checked((int)length));
            }
            finally { Leave(); }
        }
    }

    /// <summary>Runs Pinta's own collector and optionally compacts its heap.</summary>
    public void Collect(bool compact = true)
    {
        lock (gate)
        {
            Enter();
            try { VM.pinta_core_gc((VM.PintaCore*)api->core, (byte)(compact ? 1 : 0)); }
            finally { Leave(); }
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (busy) throw new InvalidOperationException("Cannot dispose an engine inside a callback.");
            disposed = true;
            Release();
        }
        GC.SuppressFinalize(this);
    }

    private void Enter()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (busy) throw new InvalidOperationException("Reentrant operations on an engine are unsupported.");
        busy = true;
    }
    private void Leave()
    {
        busy = false;
        try
        {
            for (int index = 0; index < GuardBytes; index++)
                if (((byte*)allocation)[index] != 0xa5 || ((byte*)arena)[(nuint)arenaLength + (uint)index] != 0xa5)
                    throw new InvalidOperationException("Pinta arena boundary guard changed.");
        }
        finally { GC.KeepAlive(this); }
    }
    private void Validate(PintaModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (!ReferenceEquals(module.Owner, this)) throw new ArgumentException("The module belongs to another engine.", nameof(module));
    }
    private static VM.PintaApiString Text(char* data, int length) => new()
    { string_data = data, string_length = checked((uint)length), string_encoding = (VM.PintaApiEncoding)2 };
    private static void Check(uint status) { if (status != 0) throw new PintaException(status); }
    private void Release()
    {
        api = null;
        if (context.IsAllocated) context.Free();
        NativeMemory.Free(allocation);
        allocation = null;
        arena = null;
        host?.Handles.Clear();
        host?.Modules.Clear();
        Array.Clear(functions);
    }

    private sealed class Cursor(byte[] bytes)
    {
        internal byte[] Bytes { get; } = bytes;
        internal int Position;
    }
    private sealed class Host
    {
        internal readonly Dictionary<string, byte[]> Modules;
        internal readonly Dictionary<nint, Cursor> Handles = new();
        internal nint NextHandle;
        internal WeakReference<PintaEngine>? Owner;
        internal Host(IReadOnlyDictionary<string, byte[]> modules)
        {
            Modules = new(StringComparer.Ordinal);
            foreach (var pair in modules) Modules.Add(pair.Key, (byte[])pair.Value.Clone());
        }
    }
    private static Host State(void* token) => (Host)GCHandle.FromIntPtr((nint)token).Target!;
    private static void* Open(void* token, void* name, uint length)
    {
        try
        {
            var state = State(token);
            var key = new string((char*)name, 0, checked((int)length));
            if (!state.Modules.TryGetValue(key, out var bytes)) return null;
            nint handle = checked(++state.NextHandle);
            state.Handles.Add(handle, new Cursor(bytes));
            return (void*)handle;
        }
        catch { return null; }
    }
    private static uint Size(void* token, void* handle)
    {
        try { return checked((uint)State(token).Handles[(nint)handle].Bytes.Length); }
        catch { return 0; }
    }
    private static uint Read(void* token, void* handle, void* buffer, uint length)
    {
        try
        {
            var cursor = State(token).Handles[(nint)handle];
            int count = (int)Math.Min(length, (uint)(cursor.Bytes.Length - cursor.Position));
            cursor.Bytes.AsSpan(cursor.Position, count).CopyTo(new Span<byte>(buffer, count));
            cursor.Position += count;
            return (uint)count;
        }
        catch { return 0; }
    }
    private static void Close(void* token, void* handle)
    {
        try { State(token).Handles.Remove((nint)handle); }
        catch { /* Exceptions must never unwind through a translated C callback. */ }
    }
}
