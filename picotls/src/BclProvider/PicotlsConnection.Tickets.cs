using static Managed.Security.PicoTls;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Managed.Security;

/// <summary>A client resumption credential containing PSK material. Keep it
/// confidential. Dispose clears the owned bytes; Export returns a caller-owned copy.</summary>
public sealed class SavedSessionTicket : IDisposable
{
    private readonly object gate = new();
    private byte[]? bytes;
    public DateTimeOffset ExpiresAt { get; }
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    // This profile never offers early data, regardless of the ticket extension.
    public bool EarlyDataEnabled => false;
    internal SavedSessionTicket(byte[] bytes, uint lifetime)
    { this.bytes = bytes; ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime); }
    public byte[] Export()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(bytes == null, this);
            return (byte[])bytes!.Clone();
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (bytes != null) { CryptographicOperations.ZeroMemory(bytes); bytes = null; }
        }
        GC.SuppressFinalize(this);
    }
    ~SavedSessionTicket() { Dispose(); }
}

public sealed unsafe partial class PicotlsConnection
{
    private nint savedTicketHandle;
    private sealed class SavedTicketState : IDisposable
    {
        public readonly List<SavedSessionTicket> Pending = [];
        public void Dispose() { foreach (var ticket in Pending) ticket.Dispose(); Pending.Clear(); }
    }
    internal static readonly delegate*<st_ptls_save_ticket_t*, st_ptls_t*, st_ptls_iovec_t, st_ptls_save_ticket_properties_t*, int>
        SaveTicketPointer = &SaveTicket;
    private void InitializeSavedTickets()
    {
        savedTicketHandle = GCHandle.ToIntPtr(GCHandle.Alloc(new SavedTicketState()));
        *PicoTls.ptls_get_data_ptr(native) = (void*)savedTicketHandle;
    }
    private void ReleaseSavedTickets()
    {
        if (savedTicketHandle == 0) return;
        var handle = GCHandle.FromIntPtr(savedTicketHandle); savedTicketHandle = 0;
        try { ((SavedTicketState)handle.Target!).Dispose(); }
        finally { handle.Free(); }
    }
    /// <summary>Transfers ownership of newly received tickets to the caller.
    /// Feed post-handshake bytes to Process to receive NewSessionTicket messages.</summary>
    public IReadOnlyList<SavedSessionTicket> TakeSessionTickets()
    {
        lock (gate)
        {
            RequireLive();
            if (savedTicketHandle == 0) return Array.Empty<SavedSessionTicket>();
            var state = (SavedTicketState)GCHandle.FromIntPtr(savedTicketHandle).Target!;
            var result = state.Pending.ToArray(); state.Pending.Clear(); return result;
        }
    }
    private static int SaveTicket(st_ptls_save_ticket_t* self, st_ptls_t* tls, st_ptls_iovec_t input, st_ptls_save_ticket_properties_t* properties)
    {
        SavedSessionTicket? saved = null;
        try
        {
            CallbackScope.RequireActive();
            if (CallbackScope.HasFailure) return 0x203;
            if (properties == null || tls == null || input.len > 65536 || input.@base == null) return 47;
            if (properties->lifetime is 0 or > 604800) return 0; // Discard out-of-profile lifetimes.
            nint opaque = (nint)(*PicoTls.ptls_get_data_ptr(tls));
            if (opaque == 0) throw new InvalidOperationException("A client ticket callback requires connection-owned state.");
            var state = (SavedTicketState)GCHandle.FromIntPtr(opaque).Target!;
            saved = new(new ReadOnlySpan<byte>(input.@base, (int)input.len).ToArray(), properties->lifetime);
            state.Pending.Add(saved); saved = null;
            if (state.Pending.Count > 4) { state.Pending[0].Dispose(); state.Pending.RemoveAt(0); }
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return error is OutOfMemoryException ? 0x201 : 0x203; }
        finally { saved?.Dispose(); }
    }
}
