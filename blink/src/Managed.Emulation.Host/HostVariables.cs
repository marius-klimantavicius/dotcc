using System.Runtime.InteropServices;
using System.Text;

namespace Managed.Emulation.Host;

/// <summary>Immutable private variables with explicitly owned C value storage.
/// Dispose only after every C borrower and cached pointer has been discarded.</summary>
public sealed unsafe class HostVariables : IDisposable
{
    public const int MaximumEntries = 128, MaximumUtf8Bytes = 32768, MaximumNameBytes = 1024;
    private static readonly UTF8Encoding Encoding = new(false, true);
    private readonly object sync = new();
    private readonly Dictionary<string, nuint> values;
    private bool disposed;
    private int allocatedBytes;
    public HostVariables(IReadOnlyDictionary<string, string>? source = null)
    {
        var prepared = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        int total = 0;
        if (source != null) foreach (var item in source)
        {
            if (prepared.Count == MaximumEntries) throw new ArgumentException("Too many private variables.", nameof(source));
            string name = item.Key, value = item.Value;
            if (string.IsNullOrEmpty(name) || name.Length > MaximumNameBytes || name.Contains('=') || name.Contains('\0') ||
                value == null || value.Length > MaximumUtf8Bytes || value.Contains('\0'))
                throw new ArgumentException("Invalid private variable name or value.", nameof(source));
            int nameBytes, valueBytes;
            try { nameBytes = Encoding.GetByteCount(name); valueBytes = Encoding.GetByteCount(value); }
            catch (EncoderFallbackException error) { throw new ArgumentException("Private variables require valid Unicode.", nameof(source), error); }
            if (nameBytes > MaximumNameBytes || nameBytes + valueBytes > MaximumUtf8Bytes - total)
                throw new ArgumentException("Private variable byte budget exceeded.", nameof(source));
            if (!prepared.TryAdd(name, Encoding.GetBytes(value))) throw new ArgumentException("Duplicate private variable.", nameof(source));
            total += nameBytes + valueBytes;
        }
        values = new Dictionary<string, nuint>(prepared.Count, StringComparer.Ordinal);
        try
        {
            foreach (var item in prepared)
            {
                byte* memory = (byte*)NativeMemory.Alloc((nuint)item.Value.Length + 1);
                if (memory == null) throw new OutOfMemoryException();
                try
                {
                    item.Value.AsSpan().CopyTo(new Span<byte>(memory, item.Value.Length));
                    memory[item.Value.Length] = 0;
                    values.Add(item.Key, (nuint)memory);
                }
                catch { NativeMemory.Free(memory); throw; }
                allocatedBytes += item.Value.Length + 1;
            }
        }
        catch { Dispose(); throw; }
    }
    public int AllocatedBytes { get { lock (sync) return allocatedBytes; } }
    public HostResult<nuint> Lookup(string name)
    {
        lock (sync)
        {
            if (disposed) return HostResult<nuint>.Failure(GuestError.BadDescriptor);
            // getenv's empty/equals names cannot match a validated key.
            return HostResult<nuint>.Success(values.TryGetValue(name, out var pointer) ? pointer : 0);
        }
    }
    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            disposed = true;
            foreach (var pointer in values.Values) NativeMemory.Free((void*)pointer);
            values.Clear(); allocatedBytes = 0;
        }
    }
}
