using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Managed.Transport.Hosting;

internal static partial class DatagramAsync
{
    static partial void ObserveSend(ref ValueTask<int> operation)
    {
        if (Interlocked.Exchange(ref Program.SendFault, 0) == 0) return;
        // Retain and await the genuine socket operation before replacing its
        // completion. The production completion still owns and frees the send.
        operation = new ValueTask<int>(FailAfterCompletion(operation));
    }
    private static async Task<int> FailAfterCompletion(ValueTask<int> operation)
    {
        await operation.ConfigureAwait(false);
        Interlocked.Increment(ref Program.InjectedSendFaults);
        throw new SocketException((int)SocketError.NetworkUnreachable);
    }
}

public sealed unsafe partial class MsQuicHost
{
    static partial void ObserveBeforeDatagramReceive()
    {
        if (Interlocked.Exchange(ref Program.ReceiveFault, 0) == 0) return;
        Interlocked.Increment(ref Program.InjectedReceiveFaults);
        // Rent has succeeded; no OS operation owns the memory yet. The genuine
        // receive catch must return this lease and start the next receive.
        throw new SocketException((int)SocketError.NetworkDown);
    }
}
