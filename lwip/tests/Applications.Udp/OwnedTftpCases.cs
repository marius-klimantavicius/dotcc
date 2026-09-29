using System.Buffers.Binary;
using System.Net;
using System.Text;
using Managed.Networking;
using static UdpPeer;

internal static class OwnedTftpCases
{
    static byte[] Request(ushort operation,string name)
    {
        byte[] suffix=Encoding.ASCII.GetBytes(name+"\0octet\0"),bytes=new byte[suffix.Length+2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes,operation);suffix.CopyTo(bytes,2);return bytes;
    }
    static byte[] Data(ushort block,int length)
    {
        byte[] bytes=new byte[length+4];BinaryPrimitives.WriteUInt16BigEndian(bytes,3);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(2),block);for(int i=0;i<length;i++)bytes[i+4]=(byte)(i%251);return bytes;
    }
    static void Complete(VirtualPair pair,Task task)
    {Wait(pair,()=>task.IsCompleted);task.GetAwaiter().GetResult();}
    internal static void Run(VirtualPair pair)
    {
        TftpCases.ConstructionRollback(pair);
        byte[] payload=Enumerable.Range(0,1025).Select(i=>(byte)(i%251)).ToArray();
        using var server=LwipTftpService.StartServer(pair.B,2048,4096);
        using var client=LwipTftpService.StartClient(pair.A,2048);
        var endpoint=new LwipEndpoint(IPAddress.Parse("10.0.0.2"),69);
        server.SetFile("source.bin",payload);
        bool duplicate=false;try{LwipTftpService.StartClient(pair.A);}catch(InvalidOperationException){duplicate=true;}
        Require(duplicate,"TFTP singleton registration rollback");
        var get=client.DownloadAsync(endpoint,"source.bin");Complete(pair,get);Require(get.Result.SequenceEqual(payload),"owning TFTP GET copied payload");
        var put=client.UploadAsync(endpoint,"uploaded.bin",payload);Complete(pair,put);pair.Drain();Require(server.GetFile("uploaded.bin").SequenceEqual(payload),"owning TFTP corrected PUT and server storage");
        Require(client.CompletedTransfers==2&&server.CompletedTransfers==2&&client.ActiveTransfers==0&&server.ActiveTransfers==0,"TFTP final DATA/ACK closes count as success");

        var missing=client.DownloadAsync(endpoint,"absent.bin");Wait(pair,()=>missing.IsCompleted);
        try{missing.GetAwaiter().GetResult();throw new InvalidOperationException("Missing file unexpectedly succeeded");}catch(IOException){ }
        Require(client.FailedTransfers==1&&client.ActiveTransfers==0,"ordinary file error closes owned transfer");

        var pending=client.DownloadAsync(new LwipEndpoint(IPAddress.Parse("10.0.0.2"),31068),"pending.bin");
        pair.Drain();Require(!pending.IsCompleted&&client.ActiveTransfers==1,"unanswered original client request remains pending");
        client.CancelPending();Require(pending.IsCanceled&&client.ActiveTransfers==0&&client.CanceledTransfers==1,"explicit client cancel releases original handle and restarts");
        get=client.DownloadAsync(endpoint,"source.bin");Complete(pair,get);Require(get.Result.SequenceEqual(payload),"client continues after canceled request");

        // A second owned stack has no peer. Its pending original request and
        // callback roots must be canceled by whole-owner disposal, while the
        // existing independent B server remains useful.
        using(var victim=new LwipStack())
        {
            victim.Start();victim.AddInterface("10.9.0.1","",new byte[]{2,9,0,0,0,1},_=>{});
            var victimClient=LwipTftpService.StartClient(victim);
            var task=victimClient.DownloadAsync(new LwipEndpoint(IPAddress.Parse("10.9.0.2"),69),"pending.bin");
            Require(!task.IsCompleted,"victim original request admitted");victim.Dispose();Require(task.IsCanceled,"owner disposal cancels pending TFTP task");
        }
        GC.Collect();GC.WaitForPendingFinalizers();get=client.DownloadAsync(endpoint,"source.bin");Complete(pair,get);
        Require(get.Result.SequenceEqual(payload),"independent server B continues after other owner disposal");

        using var peer=new UdpPeer(pair.A,31070);
        server.SetFile("partial.bin","original"u8);
        peer.Send("10.0.0.2",69,Request(2,"partial.bin"));var ack=peer.Next(pair);
        Require(ack.Bytes.AsSpan().SequenceEqual(new byte[]{0,4,0,0}),"normal WRQ ACK zero");
        peer.Send("10.0.0.2",ack.Port,Data(1,512));ack=peer.Next(pair);
        Require(ack.Bytes.AsSpan().SequenceEqual(new byte[]{0,4,0,1})&&server.ActiveTransfers==1,"partial coherent write remains active");
        server.CancelPending();Require(server.ActiveTransfers==0&&server.GetFile("partial.bin").AsSpan().SequenceEqual("original"u8),"explicit server cancellation discards partial replacement");

        // Advance only the host clock and original timeout dispatcher. No timer
        // function or protocol state is substituted. The coherent reader simply
        // withholds ACKs; original server retries and eventually closes it.
        uint now=LwipSysArch.Now(pair.B.Core);LwipSysArch.SetClock(pair.B.Core,()=>Volatile.Read(ref now));
        long failed=server.FailedTransfers,completed=server.CompletedTransfers;
        server.SetFile("tail.bin","end"u8);
        peer.Send("10.0.0.2",69,Request(1,"tail.bin"));var first=peer.Next(pair);
        Require(BinaryPrimitives.ReadUInt16BigEndian(first.Bytes)==3,"server emits first original DATA");
        for(int i=0;i<80&&server.ActiveTransfers!=0;i++)
        {now=unchecked(now+1100);pair.B.Invoke(core=>core.sys_check_timeouts());pair.Drain();}
        Require(server.ActiveTransfers==0&&server.FailedTransfers==failed+1&&server.CompletedTransfers==completed,"server timeout is unsuccessful closure, not final ACK success");
        client.ThrowCallbackErrors();server.ThrowCallbackErrors();
        server.Dispose();
        using var small=LwipTftpService.StartServer(pair.B,8,8,maximumFiles:1);
        small.SetFile("empty.bin",Array.Empty<byte>());
        bool bounded=false;try{small.SetFile("second.bin",Array.Empty<byte>());}catch(IOException){bounded=true;}
        Require(bounded,"distinct nonempty filenames with zero-byte payloads obey the metadata budget");
        var rejected=client.UploadAsync(endpoint,"second.bin",ReadOnlyMemory<byte>.Empty);Wait(pair,()=>rejected.IsCompleted);
        try{rejected.GetAwaiter().GetResult();throw new InvalidOperationException("Full file table unexpectedly accepted a new name");}catch(IOException){ }
        Require(small.RemoveFile("empty.bin"),"remove file releases metadata budget");
        put=client.UploadAsync(endpoint,"second.bin",ReadOnlyMemory<byte>.Empty);Complete(pair,put);
        Require(small.GetFile("second.bin").Length==0,"ordinary empty write succeeds after metadata recovery");
        // A coherent nine-byte DATA block exceeds the configured storage bound.
        // Original error handling must override any last-block completion marker.
        rejected=client.UploadAsync(endpoint,"second.bin",new byte[9]);Wait(pair,()=>rejected.IsCompleted);
        try{rejected.GetAwaiter().GetResult();throw new InvalidOperationException("Storage callback failure became success");}catch(IOException){ }
        bool callbackFailure=false;try{small.ThrowCallbackErrors();}catch(AggregateException){callbackFailure=true;}
        Require(callbackFailure&&small.ActiveTransfers==0&&small.GetFile("second.bin").Length==0,"bounded storage callback error contained and old file preserved");
    }
}
