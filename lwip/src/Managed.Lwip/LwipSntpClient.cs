using System;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Managed.Networking.Lwip;

namespace Managed.Networking;

/// <summary>NTP wire-era seconds and unsigned 2^-32 second fraction, in host byte order. Era resolution and wall-clock policy belong to the consumer.</summary>
public readonly record struct LwipNtpTime(uint Seconds,uint Fraction);

/// <summary>Owns the original SNTP client and a bounded queue of copied time notifications. Never changes the operating-system clock.</summary>
public sealed unsafe class LwipSntpClient:IDisposable
{
    readonly LwipStack stack;
    readonly Channel<LwipNtpTime> notifications;
    bool registered,started,stopped;
    volatile bool disposed;
    long observed;
    LwipSntpClient(LwipStack stack,int capacity)
    {
        this.stack=stack;
        notifications=Channel.CreateBounded<LwipNtpTime>(new BoundedChannelOptions(capacity)
        {SingleWriter=true,SingleReader=false,AllowSynchronousContinuations=false,FullMode=BoundedChannelFullMode.DropOldest});
    }
    /// <summary>Start one numeric-address client per stack. At most notificationCapacity samples are retained; the oldest is dropped when full.</summary>
    public static LwipSntpClient Start(LwipStack stack,IPAddress server,int notificationCapacity=32)
    {
        ArgumentNullException.ThrowIfNull(stack);ArgumentNullException.ThrowIfNull(server);
        if(notificationCapacity<1||notificationCapacity>65536)throw new ArgumentOutOfRangeException(nameof(notificationCapacity));
        var endpoint=new LwipEndpoint(server,123);
        return stack.StartInitializedService(s=>new LwipSntpClient(s,notificationCapacity),service=>service.Initialize(endpoint));
    }
    void Initialize(LwipEndpoint server)=>stack.RunServiceBoundary(()=>stack.Invoke(core=>
    {
        if(core.sntp_enabled()!=0)throw new InvalidOperationException("A borrowed SNTP client is already active in this stack.");
        LwipUdpApplications.RegisterSntp(core,this,Notify);registered=true;
        ip_addr address;server.WriteIp(&address);
        core.sntp_setoperatingmode(0);
        for(byte index=0;index<SNTP_MAX_SERVERS;index++)core.sntp_setserver(index,index==0?&address:null);
        core.sntp_init();started=core.sntp_enabled()!=0;
        if(!started)throw new InvalidOperationException("Original SNTP client could not allocate its UDP PCB.");
    }));
    void Notify(uint seconds,uint fraction)
    {
        // This callback executes on the original core. Only copy a value; user
        // continuations run asynchronously, so no facade reentry is introduced.
        if(notifications.Writer.TryWrite(new(seconds,fraction)))Interlocked.Increment(ref observed);
    }
    public long ObservedNotifications=>Interlocked.Read(ref observed);
    public bool TryReadTime(out LwipNtpTime value)=>notifications.Reader.TryRead(out value);
    /// <summary>Wait for a copied sample. Canceling a wait does not stop the original client; disposal completes pending readers.</summary>
    public ValueTask<LwipNtpTime> ReadTimeAsync(CancellationToken cancellation=default)=>notifications.Reader.ReadAsync(cancellation);
    public void Dispose()
    {
        if(disposed)return;
        try{stack.RunServiceBoundary(()=>
        {
            if(disposed)return;
            if(!stopped)
            {
                stack.Invoke(core=>{if(started)core.sntp_stop();started=false;});
                stopped=true;
            }
            stack.ServiceBarrier();
            stack.Invoke(core=>{if(registered){LwipUdpApplications.UnregisterSntp(core,this);registered=false;}});
            notifications.Writer.TryComplete(new ObjectDisposedException(nameof(LwipSntpClient)));
            disposed=true;
        });}catch(ObjectDisposedException)when(disposed){}
    }
}
