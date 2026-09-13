using static Managed.Transport.MsQuic;
using Managed.Transport.Api;
using Managed.Transport;

internal static class ConnectionParameterInputs
{
    internal static void Run()
    {
        QuicConnection.ValidateCloseReasonPhrase(new byte[0]);
        QuicConnection.ValidateCloseReasonPhrase(Enumerable.Repeat((byte)'x', 511).ToArray());
        try { QuicConnection.ValidateCloseReasonPhrase(new byte[] { 1, 0 }); throw new InvalidOperationException("Embedded NUL accepted."); }
        catch (ArgumentException) { }
        try { QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_CONN_QUIC_VERSION, (QuicParameterPriority)2); throw new InvalidOperationException("Unknown priority accepted."); }
        catch (ArgumentOutOfRangeException) { }
        byte[] caller = [1, 2, 3, 4];
        byte[] snapshot = QuicConnection.SnapshotResumptionTicket(new() { ResumptionTicket = caller });
        caller.AsSpan().Clear();
        if (!snapshot.AsSpan().SequenceEqual(new byte[] { 1, 2, 3, 4 }))
            throw new InvalidOperationException("Connection ticket snapshot borrowed caller storage.");
        if (QuicConnection.SnapshotResumptionTicket(null).Length != 0)
            throw new InvalidOperationException("Default connection options supplied a ticket.");
        try
        {
            QuicConnection.SnapshotResumptionTicket(new() { EnableEarlyData = true });
            throw new InvalidOperationException("Early data was accepted.");
        }
        catch (NotSupportedException) { }
        try
        {
            QuicConnection.SnapshotResumptionTicket(new() { ResumptionTicket = new byte[65536] });
            throw new InvalidOperationException("Oversized ticket was accepted.");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
