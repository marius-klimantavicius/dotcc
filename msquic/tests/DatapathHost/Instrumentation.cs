using System;
using Managed.Transport;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost
{
    internal bool DatagramInterfacesMatch(nint socket,uint expected)
    {
        var record=Resource<DatagramSocket>((void*)socket);
        lock(record.gate)
        {
            Span<byte> value=stackalloc byte[4];
            foreach(var physical in record.physical)
            {
                var transport=physical.Socket;
                if(transport.AddressFamily==System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    if(transport.GetRawSocketOption(41,76,value)!=4||System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(value)!=expected)return false;
                }
                if(transport.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork||transport.DualMode)
                {
                    if(transport.GetRawSocketOption(0,50,value)!=4||System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(value)!=expected)return false;
                }
            }
            return true;
        }
    }
    // Test-only synchronization: prove ordering through the queue predicate rather
    // than relying on a scheduling delay before releasing the blocked worker.
    internal bool HasQueuedDatagram(CXPLAT_EVENTQ* queue, nint socket)
    {
        var target=Resource<DatagramSocket>((void*)socket).NotificationSqe;
        var record=Resource<PlatformQueue>((void*)queue->Handle);
        lock(record.Gate)
            for(var entry=record.Head;entry!=null;entry=entry.Next)
                if(entry.Address==target)return true;
        return false;
    }
    internal bool CurrentBatchHasDatagram(CXPLAT_EVENTQ* queue, nint socket)
    {
        var target=Resource<DatagramSocket>((void*)socket).NotificationSqe;
        var record=Resource<PlatformQueue>((void*)queue->Handle);
        lock(record.Gate)
            for(int i=0;i<record.BatchCount;i++)
                if(record.Batch[i]!.Address==target)return true;
        return false;
    }
}

internal static partial class DatagramAsync
{
    private static int holdNext;
    internal static readonly System.Threading.ManualResetEventSlim Sent = new();
    internal static readonly System.Threading.Tasks.TaskCompletionSource Release = new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);
    internal static void HoldNext() => System.Threading.Interlocked.Exchange(ref holdNext,1);
    static partial void ObserveSend(ref System.Threading.Tasks.ValueTask<int> operation)
    {
        if(System.Threading.Interlocked.Exchange(ref holdNext,0)==1)
            operation=new System.Threading.Tasks.ValueTask<int>(Hold(operation));
    }
    private static async System.Threading.Tasks.Task<int> Hold(System.Threading.Tasks.ValueTask<int> operation)
    {
        int count=await operation.ConfigureAwait(false);
        Sent.Set();
        await Release.Task.ConfigureAwait(false);
        return count;
    }
}
