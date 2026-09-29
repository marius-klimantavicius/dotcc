#if LWIP_HAS_SOCKETS
using static Managed.Networking.Lwip;
namespace Managed.Networking;
public sealed class LwipNetconnException(string operation,int error):IOException($"lwIP netconn {operation} failed ({error})."){public int Error {get;}=error;}

public sealed record LwipNetconnData(byte[] Bytes,LwipEndpoint? Sender);

/// <summary>Owns direct netconn handles with one active reader/acceptor and one writer/connector per connection. Cancellation closes the entire affected connection. Teardown prepares deletion, joins callers, then frees the connection.</summary>
public sealed unsafe class LwipNetconns:IDisposable
{
    readonly LwipStack stack;readonly Lwip owner;readonly object gate=new();readonly HashSet<Connection> connections=new();bool stopping,disposed;
    private LwipNetconns(LwipStack stack){this.stack=stack;owner=stack.Core;}
    public static LwipNetconns Start(LwipStack stack)=>stack.StartService(s=>new LwipNetconns(s));
    // Scheduling is admission too: returned Tasks must settle before backing
    // can be released, even if their worker has not entered an original call.
    internal TaskScheduler Scheduler = TaskScheduler.Default;
    internal Task<T> Schedule<T>(Connection item,Func<T> action)
    {
        RejectCore();var completion=new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);IDisposable retain;
        lock(gate){ObjectDisposedException.ThrowIf(stopping||disposed||item.Closing,item);retain=owner.__DotCcRetain();item.Scheduled++;}
        void Complete(T result,Exception? error)
        {
            try{retain.Dispose();}catch(Exception release){error=error==null?release:new AggregateException(error,release);}
            lock(gate)
            {
                if(error is OperationCanceledException canceled)completion.TrySetCanceled(canceled.CancellationToken);
                else if(error!=null)completion.TrySetException(error);
                else completion.TrySetResult(result);
                item.Scheduled--;Monitor.PulseAll(gate);
            }
        }
        try
        {
            _=Task.Factory.StartNew(()=>
            {
                T result=default!;Exception? error=null;
                try{result=action();}catch(Exception failure){error=failure;}
                Complete(result,error);
            },CancellationToken.None,TaskCreationOptions.DenyChildAttach,Scheduler);
        }
        catch(Exception error){Complete(default!,error);}
        return completion.Task;
    }
    void RejectCore(){if(LwipSysArch.IsCoreContext(owner))throw new InvalidOperationException("Blocking netconn calls cannot run on the owning core.");}
    static void Check(int error,string operation){if(error!=0)throw new LwipNetconnException(operation,error);}
    public Connection CreateTcp(bool ipv6=false)=>Create(ipv6?netconn_type.NETCONN_TCP_IPV6:netconn_type.NETCONN_TCP,true);
    public Connection CreateUdp(bool ipv6=false)=>Create(ipv6?netconn_type.NETCONN_UDP_IPV6:netconn_type.NETCONN_UDP,false);
    Connection Create(netconn_type type,bool tcp)
    {
        RejectCore();lock(gate)
        {
            ObjectDisposedException.ThrowIf(stopping||disposed,this);using var retain=owner.__DotCcRetain();using var entry=owner.__DotCcEnter();owner.netconn_thread_init();
            try{var native=owner.netconn_new_with_proto_and_callback(type,0,null);if(native==null)throw new LwipNetconnException("allocate",-1);var item=new Connection(this,(nint)native,tcp);connections.Add(item);return item;}
            finally{owner.netconn_thread_cleanup();}
        }
    }
    internal T Call<T>(Connection item,Func<Lwip,nint,T> action,CancellationToken cancellation=default,int direction=0)
    {
        RejectCore();cancellation.ThrowIfCancellationRequested();IDisposable retain;
        lock(gate){ObjectDisposedException.ThrowIf(stopping||disposed||item.Closing,item);if(((direction&1)!=0&&item.Reading)||((direction&2)!=0&&item.Writing))throw new InvalidOperationException("lwIP permits one active reader/acceptor and one writer/connector per netconn.");retain=owner.__DotCcRetain();item.Active++;if((direction&1)!=0)item.Reading=true;if((direction&2)!=0)item.Writing=true;}
        try
        {
            using var registration=cancellation.Register(()=>{try{Prepare(item);}catch(Exception error){lock(gate)item.PrepareError=error;}});
            cancellation.ThrowIfCancellationRequested();using var entry=owner.__DotCcEnter();owner.netconn_thread_init();
            try{return action(owner,item.Pointer);}catch(LwipNetconnException) when(cancellation.IsCancellationRequested){throw new OperationCanceledException(cancellation);}
            finally{owner.netconn_thread_cleanup();}
        }
        finally{retain.Dispose();lock(gate){item.Active--;if((direction&1)!=0)item.Reading=false;if((direction&2)!=0)item.Writing=false;Monitor.PulseAll(gate);}}
    }
    internal Connection Accept(Connection listener,CancellationToken cancellation)=>Call(listener,(core,pointer)=>
    {
        netconn*accepted=null;Check(core.netconn_accept((netconn*)pointer,&accepted),"accept");if(accepted==null)throw new InvalidOperationException("Successful netconn accept returned null.");
        return AdoptAccepted(listener,accepted);
    },cancellation,1);
    internal Connection AdoptAccepted(Connection listener,netconn*accepted)
    {
        var child=new Connection(this,(nint)accepted,true);bool rejected;
        lock(gate){connections.Add(child);rejected=stopping||listener.Closing;}
        // An accepted struct is ours even when admission closes concurrently.
        // Retain the child in the registry if prepare/delete fails, so a later
        // service disposal can finish releasing its original backing.
        if(rejected){Delete(child);throw new ObjectDisposedException(nameof(LwipNetconns));}
        return child;
    }
    void Prepare(Connection item)
    {
        RejectCore();lock(gate){if(item.Prepared||item.Freed||item.Preparing)return;item.Closing=true;item.Preparing=true;item.PrepareError=null;}
        Exception? failure=null;
        try{using var retain=owner.__DotCcRetain();using var entry=owner.__DotCcEnter();owner.netconn_thread_init();try{Check(owner.netconn_prepare_delete((netconn*)item.Pointer),"prepare delete");}finally{owner.netconn_thread_cleanup();}}
        catch(Exception error){failure=error;}
        lock(gate){item.Preparing=false;item.Prepared=failure==null;item.PrepareError=failure;Monitor.PulseAll(gate);}if(failure!=null)throw failure;
    }
    internal void Delete(Connection item)
    {
        Prepare(item);var watch=System.Diagnostics.Stopwatch.StartNew();
        lock(gate)
        {
            while(item.Preparing||item.Active!=0||item.Scheduled!=0){var left=TimeSpan.FromSeconds(10)-watch.Elapsed;if(left<=TimeSpan.Zero)throw new TimeoutException("Netconn callers did not finish; backing retained.");Monitor.Wait(gate,left);}
            if(item.PrepareError!=null)throw new AggregateException("Netconn prepare-delete failed; retained for retry.",item.PrepareError);
            if(item.Freed)return;
            using var retain=owner.__DotCcRetain();using var entry=owner.__DotCcEnter();owner.netconn_thread_init();
            try{Check(owner.netconn_delete((netconn*)item.Pointer),"delete after callers finish");item.Freed=true;connections.Remove(item);}
            finally{owner.netconn_thread_cleanup();}
        }
    }
    public void Dispose()
    {
        RejectCore();Connection[] snapshot;lock(gate){if(disposed)return;stopping=true;snapshot=connections.ToArray();}
        List<Exception>? failures=null;foreach(var item in snapshot)try{Prepare(item);}catch(Exception error){(failures??=new()).Add(error);}
        if(failures!=null)throw new AggregateException("Netconn shutdown failed; core retained.",failures);
        foreach(var item in snapshot)Delete(item);
        lock(gate){if(connections.Count!=0)throw new InvalidOperationException("Accepted netconn backing remains registered; retained for disposal retry.");disposed=true;}
        stack.CompleteServiceDisposal(this);
    }
    public sealed class Connection:IDisposable
    {
        readonly LwipNetconns service;readonly bool tcp;internal readonly nint Pointer;internal int Active,Scheduled;internal bool Closing,Preparing,Prepared,Freed,Reading,Writing;internal Exception? PrepareError;
        internal Connection(LwipNetconns service,nint pointer,bool tcp){this.service=service;Pointer=pointer;this.tcp=tcp;}
        public void Bind(LwipEndpoint endpoint){ArgumentNullException.ThrowIfNull(endpoint);service.Call(this,(core,pointer)=>{ip_addr address;endpoint.WriteIp(&address);Check(core.netconn_bind((netconn*)pointer,&address,endpoint.Port),"bind");return 0;},direction:3);}
        public void Listen(byte backlog=8)=>service.Call(this,(core,pointer)=>{Check(core.netconn_listen_with_backlog((netconn*)pointer,backlog),"listen");return 0;},direction:3);
        public Task ConnectAsync(LwipEndpoint endpoint,CancellationToken cancellation=default){ArgumentNullException.ThrowIfNull(endpoint);return service.Schedule(this,()=>service.Call(this,(core,pointer)=>{ip_addr address;endpoint.WriteIp(&address);Check(core.netconn_connect((netconn*)pointer,&address,endpoint.Port),"connect");return 0;},cancellation,2));}
        public Task<Connection> AcceptAsync(CancellationToken cancellation=default)=>service.Schedule(this,()=>service.Accept(this,cancellation));
        public Task<int> SendAsync(ReadOnlyMemory<byte> bytes,CancellationToken cancellation=default)
        {
            if(!tcp)throw new InvalidOperationException("Use SendToAsync for UDP netconns.");byte[]copy=bytes.ToArray();return service.Schedule(this,()=>service.Call(this,(core,pointer)=>{fixed(byte*p=copy){ulong written=0;Check(core.netconn_write_partly((netconn*)pointer,p,(ulong)copy.Length,NETCONN_COPY,&written),"write copied bytes");return checked((int)written);}},cancellation,2));
        }
        public Task<int> SendToAsync(ReadOnlyMemory<byte> bytes,LwipEndpoint destination,CancellationToken cancellation=default)
        {
            if(tcp)throw new InvalidOperationException("Use SendAsync for TCP netconns.");ArgumentNullException.ThrowIfNull(destination);if(bytes.Length>65535)throw new ArgumentOutOfRangeException(nameof(bytes));byte[]copy=bytes.ToArray();return service.Schedule(this,()=>service.Call(this,(core,pointer)=>
            {
                netbuf*buffer=core.netbuf_new();if(buffer==null)throw new LwipNetconnException("allocate send netbuf",-1);
                try{void*data=core.netbuf_alloc(buffer,(ushort)copy.Length);if(data==null)throw new LwipNetconnException("allocate copied send bytes",-1);copy.CopyTo(new Span<byte>(data,copy.Length));ip_addr address;destination.WriteIp(&address);Check(core.netconn_sendto((netconn*)pointer,buffer,&address,destination.Port),"sendto");return copy.Length;}
                finally{core.netbuf_delete(buffer);}
            },cancellation,2));
        }
        public Task<LwipNetconnData?> ReceiveAsync(CancellationToken cancellation=default)=>service.Schedule(this,()=>service.Call(this,(core,pointer)=>
        {
            netbuf*buffer=null;int error=core.netconn_recv((netconn*)pointer,&buffer);if(tcp&&error==-15)return null;Check(error,"receive");if(buffer==null)throw new InvalidOperationException("Successful netconn receive returned null.");
            try{byte[]copy=new byte[buffer->p->tot_len];fixed(byte*p=copy)if(core.pbuf_copy_partial(buffer->p,p,(ushort)copy.Length,0)!=copy.Length)throw new InvalidOperationException("Incomplete netbuf copy.");return new LwipNetconnData(copy,tcp?null:LwipEndpoint.ReadIp(&buffer->addr,buffer->port));}
            finally{core.netbuf_delete(buffer);}
        },cancellation,1));
        public void Dispose()=>service.Delete(this);
    }
}

#endif
