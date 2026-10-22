namespace Managed.PGlite;

/// <summary>Owned synchronous byte transport, independent of SQL semantics.
/// C buffer pointers are borrowed only while Read/Write copies their bytes.</summary>
internal sealed unsafe class PgliteTransport(int maximumResponseBytes) : IDisposable
{
    private readonly MemoryStream response = new();
    private readonly List<byte[]> chunks = new();
    private byte[] request = [];
    private int offset;
    internal Exception? Error { get; private set; }
    internal bool InputConsumed => offset == request.Length;
    internal int InputOffset => offset;
    internal IReadOnlyList<byte[]> Chunks => chunks;
    internal ReadOnlySpan<byte> Response => response.GetBuffer().AsSpan(0, checked((int)response.Length));

    internal void Begin(byte[] frame)
    {
        request = frame; offset = 0; response.SetLength(0); chunks.Clear(); Error = null;
    }
    internal void ReleaseRequest() { request = []; offset = 0; }

    internal long Read(void* buffer, ulong count)
    {
        if (Error != null) return -1;
        try
        {
            if (count == 0) return 0;
            if (buffer == null) throw new IOException("Null backend read buffer.");
            int length = (int)Math.Min(count, (ulong)(request.Length - offset));
            request.AsSpan(offset, length).CopyTo(new Span<byte>(buffer, length));
            offset += length;
            return length;
        }
        catch (Exception error) { Error ??= error; return -1; }
    }

    internal long Write(void* buffer, ulong count)
    {
        if (Error != null) return -1;
        try
        {
            if (count == 0) return 0;
            if (buffer == null || count > int.MaxValue || count > (ulong)(maximumResponseBytes - response.Length))
                throw new IOException("Invalid or oversized backend response buffer.");
            byte[] copy = new ReadOnlySpan<byte>(buffer, (int)count).ToArray();
            response.Write(copy);
            chunks.Add(copy);
            return (long)count;
        }
        catch (Exception error) { Error ??= error; return -1; }
    }

    public void Dispose() { response.Dispose(); request = []; }
}
