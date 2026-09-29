using System.Net;
using static Managed.Networking.Lwip;
namespace Managed.Networking;
#if LWIP_HAS_SOCKETS
public sealed class LwipSocketException(string operation,int error):IOException($"lwIP {operation} failed (errno {error})."){public int Error {get;}=error;}
public sealed record LwipDatagram(byte[] Bytes,LwipEndpoint Sender);

/// <summary>Owns socket children. At most one read/accept and one write/connect call may be active per handle, concurrently with close. Cancellation closes the entire affected socket, including its other pending operation. Calls execute without holding the stack facade lock.</summary>
public sealed unsafe class LwipSockets:IDisposable
{
    readonly Lwip owner;readonly object gate=new();readonly HashSet<LwipSocket> sockets=new();bool stopping,disposed;int active;
    private LwipSockets(Lwip owner)=>this.owner=owner;
    public static LwipSockets Start(LwipStack stack){ArgumentNullException.ThrowIfNull(stack);return stack.StartService(s=>new LwipSockets(s.Core));}
    public LwipSocket CreateTcp(bool ipv6=false)=>Create(ipv6,SOCK_STREAM);
    public LwipSocket CreateUdp(bool ipv6=false)=>Create(ipv6,SOCK_DGRAM);
    // Scheduling is admission too: returned Tasks must settle before backing
    // can be released, even if their worker has not entered an original call.
    internal TaskScheduler Scheduler = TaskScheduler.Default;
    internal Task<T> Schedule<T>(LwipSocket item,Func<T> action)
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
    void RejectCore(){if(LwipSysArch.IsCoreContext(owner))throw new InvalidOperationException("Blocking socket operations cannot run on the owning core.");}
    static void Check(long result,string operation){if(result<0)throw new LwipSocketException(operation,Libc.errno);}
    LwipSocket Create(bool ipv6,int kind)
    {
        RejectCore();lock(gate)
        {
            ObjectDisposedException.ThrowIf(stopping||disposed,this);
            using var retain=owner.__DotCcRetain();using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
            try{int fd=owner.lwip_socket(ipv6?AF_INET6:AF_INET,kind,0);Check(fd,"socket");var socket=new LwipSocket(this,fd);sockets.Add(socket);return socket;}
            finally{owner.lwip_socket_thread_cleanup();}
        }
    }
    internal T Call<T>(LwipSocket socket,Func<Lwip,int,T> action,CancellationToken cancellation=default,int direction=0)
    {
        RejectCore();cancellation.ThrowIfCancellationRequested();IDisposable retain;nint pin;
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(stopping||disposed||socket.Closing,socket);
            // Original fd_used is u8_t: each admitted operation needs an external
            // pin plus the original syscall reference. Leave headroom for close.
            if(socket.Active>=100)throw new InvalidOperationException("Socket operation concurrency limit reached.");
            if(((direction&1)!=0&&socket.Reading)||((direction&2)!=0&&socket.Writing))throw new InvalidOperationException("lwIP permits one active reader/acceptor and one writer/connector per socket.");
            retain=owner.__DotCcRetain();
            try{using var entry=owner.__DotCcEnter();pin=(nint)owner.lwip_owned_socket_pin(socket.Descriptor);if(pin==0)throw new LwipSocketException("pin admitted descriptor",Libc.errno);}
            catch{retain.Dispose();throw;}
            active++;socket.Active++;if((direction&1)!=0)socket.Reading=true;if((direction&2)!=0)socket.Writing=true;
        }
        try
        {
            using var registration=cancellation.Register(()=>{try{BeginClose(socket);}catch(Exception error){lock(gate)socket.CloseError=error;}});
            cancellation.ThrowIfCancellationRequested();using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
            try{return action(owner,socket.Descriptor);}
            catch(LwipSocketException) when(cancellation.IsCancellationRequested){throw new OperationCanceledException(cancellation);}
            finally{owner.lwip_socket_thread_cleanup();}
        }
        finally
        {
            try{using var entry=owner.__DotCcEnter();owner.lwip_owned_socket_unpin((void*)pin);}
            finally{retain.Dispose();lock(gate){active--;socket.Active--;if((direction&1)!=0)socket.Reading=false;if((direction&2)!=0)socket.Writing=false;Monitor.PulseAll(gate);}}
        }
    }
    internal LwipSocket Accept(LwipSocket listener,CancellationToken cancellation)=>Call(listener,(core,fd)=>
    {
        int accepted=core.lwip_accept(fd,null,null);Check(accepted,"accept");
        return AdoptAccepted(listener,accepted);
    },cancellation,1);
    internal LwipSocket AdoptAccepted(LwipSocket listener,int descriptor)
    {
        var child=new LwipSocket(this,descriptor);bool rejected;
        lock(gate){sockets.Add(child);rejected=stopping||listener.Closing;}
        if(rejected){Close(child);throw new ObjectDisposedException(nameof(LwipSockets));}
        return child;
    }
    void BeginClose(LwipSocket socket)
    {
        RejectCore();lock(gate){if(socket.Closed||socket.CloseRunning)return;socket.Closing=true;socket.CloseRunning=true;socket.CloseError=null;}
        Exception? failure=null;
        try
        {
            using var retain=owner.__DotCcRetain();using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();
            try{Check(owner.lwip_close(socket.Descriptor),"close");}finally{owner.lwip_socket_thread_cleanup();}
        }
        catch(Exception error){failure=error;}
        lock(gate){socket.CloseRunning=false;socket.Closed=failure==null;socket.CloseError=failure;Monitor.PulseAll(gate);}
        if(failure!=null)throw failure;
    }
    internal void Close(LwipSocket socket)
    {
        BeginClose(socket);var watch=System.Diagnostics.Stopwatch.StartNew();
        lock(gate)
        {
            while(socket.CloseRunning||socket.Active!=0||socket.Scheduled!=0){TimeSpan left=TimeSpan.FromSeconds(10)-watch.Elapsed;if(left<=TimeSpan.Zero)throw new TimeoutException("Socket callers did not finish after close; owner retained.");Monitor.Wait(gate,left);}
            if(socket.CloseError!=null)throw new AggregateException("Socket close failed; owner retained for retry.",socket.CloseError);
            sockets.Remove(socket);
        }
    }
    public void Dispose()
    {
        RejectCore();LwipSocket[] snapshot;lock(gate){if(disposed)return;stopping=true;snapshot=sockets.ToArray();}
        List<Exception>? failures=null;foreach(var socket in snapshot)try{BeginClose(socket);}catch(Exception error){(failures??=new()).Add(error);}
        if(failures!=null)throw new AggregateException("Socket service close failed; core retained for retry.",failures);
        foreach(var socket in snapshot)Close(socket);
        lock(gate){if(active!=0||sockets.Count!=0)throw new InvalidOperationException("Socket operation/adoption accounting did not quiesce; retained for retry.");disposed=true;}
    }
    public sealed class LwipSocket:IDisposable
    {
        readonly LwipSockets service;internal readonly int Descriptor;internal int Active,Scheduled;internal bool Closing,Closed,CloseRunning,Reading,Writing;internal Exception? CloseError;
        internal LwipSocket(LwipSockets service,int descriptor){this.service=service;Descriptor=descriptor;}
        public void Bind(LwipEndpoint address){ArgumentNullException.ThrowIfNull(address);service.Call(this,(core,fd)=>{sockaddr_storage native;uint size=address.Write(&native);Check(core.lwip_bind(fd,(sockaddr*)&native,size),"bind");return 0;},direction:3);}
        public void Listen(int backlog=8){if(backlog<0||backlog>255)throw new ArgumentOutOfRangeException(nameof(backlog));service.Call(this,(core,fd)=>{Check(core.lwip_listen(fd,backlog),"listen");return 0;},direction:3);}
        public Task ConnectAsync(LwipEndpoint address,CancellationToken cancellation=default){ArgumentNullException.ThrowIfNull(address);return service.Schedule(this,()=>service.Call(this,(core,fd)=>{sockaddr_storage native;uint size=address.Write(&native);Check(core.lwip_connect(fd,(sockaddr*)&native,size),"connect");return 0;},cancellation,2));}
        public Task<LwipSocket> AcceptAsync(CancellationToken cancellation=default)=>service.Schedule(this,()=>service.Accept(this,cancellation));
        public Task<int> SendAsync(ReadOnlyMemory<byte> bytes,CancellationToken cancellation=default)
        {
            byte[]copy=bytes.ToArray();return service.Schedule(this,()=>service.Call(this,(core,fd)=>{fixed(byte*p=copy){long sent=core.lwip_send(fd,p,(ulong)copy.Length,0);Check(sent,"send");return checked((int)sent);}},cancellation,2));
        }
        public Task<byte[]> ReceiveAsync(int maximum=65536,CancellationToken cancellation=default)
        {
            if(maximum<1||maximum>16*1024*1024)throw new ArgumentOutOfRangeException(nameof(maximum));return service.Schedule(this,()=>service.Call(this,(core,fd)=>{byte[]bytes=new byte[maximum];fixed(byte*p=bytes){long got=core.lwip_recv(fd,p,(ulong)bytes.Length,0);Check(got,"receive");Array.Resize(ref bytes,checked((int)got));return bytes;}},cancellation,1));
        }
        public Task<int> SendToAsync(ReadOnlyMemory<byte> bytes,LwipEndpoint destination,CancellationToken cancellation=default)
        {
            ArgumentNullException.ThrowIfNull(destination);byte[]copy=bytes.ToArray();return service.Schedule(this,()=>service.Call(this,(core,fd)=>{sockaddr_storage native;uint size=destination.Write(&native);fixed(byte*p=copy){long sent=core.lwip_sendto(fd,p,(ulong)copy.Length,0,(sockaddr*)&native,size);Check(sent,"sendto");return checked((int)sent);}},cancellation,2));
        }
        public Task<LwipDatagram> ReceiveFromAsync(int maximum=65535,CancellationToken cancellation=default)
        {
            if(maximum<1||maximum>65535)throw new ArgumentOutOfRangeException(nameof(maximum));return service.Schedule(this,()=>service.Call(this,(core,fd)=>{byte[]bytes=new byte[maximum];sockaddr_storage native;uint size=(uint)sizeof(sockaddr_storage);fixed(byte*p=bytes){long got=core.lwip_recvfrom(fd,p,(ulong)bytes.Length,0,(sockaddr*)&native,&size);Check(got,"recvfrom");Array.Resize(ref bytes,checked((int)got));return new LwipDatagram(bytes,LwipEndpoint.Read(&native,size));}},cancellation,1));
        }
        public LwipEndpoint LocalAddress=>service.Call(this,(core,fd)=>{sockaddr_storage native;uint size=(uint)sizeof(sockaddr_storage);Check(core.lwip_getsockname(fd,(sockaddr*)&native,&size),"getsockname");return LwipEndpoint.Read(&native,size);});
        public void ShutdownSend()=>service.Call(this,(core,fd)=>{Check(core.lwip_shutdown(fd,SHUT_WR),"shutdown send");return 0;},direction:2);
        public void Dispose()=>service.Close(this);
    }
}

#endif
