using static Managed.Security.PicoTls;
using System.Runtime.InteropServices;

namespace Managed.Security;

public sealed unsafe partial class PicotlsConnection
{
    private nint _savedTicketHandle;
    
    private void InitializeSavedTickets()
    {
        _savedTicketHandle = GCHandle.ToIntPtr(GCHandle.Alloc(new SavedSessionTicket.SavedTicketState()));
        *ptls_get_data_ptr(_native) = (void*)_savedTicketHandle;
    }

    private void ReleaseSavedTickets()
    {
        if (_savedTicketHandle == 0)
            return;

        var handle = GCHandle.FromIntPtr(_savedTicketHandle);
        _savedTicketHandle = 0;

        try
        {
            ((SavedSessionTicket.SavedTicketState)handle.Target!).Dispose();
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>Transfers ownership of newly received tickets to the caller.
    /// Feed post-handshake bytes to Process to receive NewSessionTicket messages.</summary>
    public IReadOnlyList<SavedSessionTicket> TakeSessionTickets()
    {
        lock (_gate)
        {
            RequireLive();
            if (_savedTicketHandle == 0)
                return Array.Empty<SavedSessionTicket>();

            var state = (SavedSessionTicket.SavedTicketState)GCHandle.FromIntPtr(_savedTicketHandle).Target!;
            var result = state.Pending.ToArray();
            state.Pending.Clear();
            return result;
        }
    }
}