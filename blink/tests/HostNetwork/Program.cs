using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("C socket assertion failed"); }
async Task<IPEndPoint> Run()
{
    var ready=new TaskCompletionSource<IPEndPoint>(TaskCreationOptions.RunContinuationsAsynchronously);
    var complete=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
    var worker=new Thread(()=> {
        try {
            Blink.BindHostIo(owner);
            Check(Blink.Reject()==-1 && Blink.SocketErrors()==0);
            int fd=Blink.Prepare(8080);
            Check(fd>=0 && Blink.Port(fd)==8080);
            var endpoint=owner.Publish(fd);Check(endpoint.Succeeded);
            ready.SetResult(endpoint.Value);
            Check(Blink.Serve(fd)==0);
            Blink.UnbindHostIo();
            complete.SetResult();
        } catch(Exception error) { ready.TrySetException(error);complete.TrySetException(error); }
    });
    worker.IsBackground=true;
    worker.Start();
    try {
        var endpoint=await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using var client=new TcpClient();await client.ConnectAsync(endpoint);
        GC.Collect(2,GCCollectionMode.Forced,true,true);
        var stream=client.GetStream();
        await stream.WriteAsync("he"u8.ToArray());
        await stream.WriteAsync("llo!"u8.ToArray());
        byte[] bytes=new byte[131072];
        await stream.ReadExactlyAsync(bytes).AsTask().WaitAsync(TimeSpan.FromSeconds(10));
        for(int i=0;i<bytes.Length;i++)Check(bytes[i]==(byte)(i*13+7));
        Check(await stream.ReadAsync(new byte[1])==0);
        await complete.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Check(worker.Join(TimeSpan.FromSeconds(10)));
        return endpoint;
    } finally { await owner.DisposeAsync(); }
}
var endpoints=await Task.WhenAll(Run(),Run());
Check(endpoints[0].Port!=endpoints[1].Port);
var canceledOwner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
var waiting=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var canceled=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var blockedWorker=new Thread(()=> {
    try {
        Blink.BindHostIo(canceledOwner);
        int listener=Blink.Prepare(8080);Check(listener>=0);
        waiting.SetResult();
        Check(Blink.WaitAccept(listener)<0);
        Blink.UnbindHostIo();
        canceled.SetResult();
    } catch(Exception error) { waiting.TrySetException(error);canceled.TrySetException(error); }
}) { IsBackground=true };
blockedWorker.Start();
await waiting.Task.WaitAsync(TimeSpan.FromSeconds(10));
await canceledOwner.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
await canceled.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check(blockedWorker.Join(TimeSpan.FromSeconds(10)));
Console.WriteLine("translated C TCP: IPv4 bytes, fragmented request, exact 128 KiB response, EOF: PASS");
