using System.Collections.Concurrent;
using Managed.Networking;

internal sealed class VirtualPair : IDisposable
{
    internal readonly LwipStack A=new(),B=new();
    private readonly BlockingCollection<(bool ToB,byte[] Bytes)> frames=new(1024);
    private readonly Thread transport;
    private readonly nint interfaceA,interfaceB;
    private Exception? error;
    private readonly Action<byte[]>? capture;
    private volatile bool stopping;
    private long queued,processed;
    internal volatile Func<bool,byte[],byte[]?>? Filter;
    internal volatile Action<bool,byte[]>? Observe;
    internal VirtualPair(Action<byte[]>? capture=null)
    {
        this.capture=capture;
        A.Start();B.Start();
        interfaceA=A.AddInterface("10.0.0.1","fd00::1",new byte[]{2,0,0,0,0,1},bytes=>Send(true,bytes));
        interfaceB=B.AddInterface("10.0.0.2","fd00::2",new byte[]{2,0,0,0,0,2},bytes=>Send(false,bytes));
        transport=new Thread(Pump){IsBackground=true,Name="lwIP test Ethernet"};transport.Start();
    }
    private void Send(bool toB,byte[] bytes)
    {
        capture?.Invoke(bytes);Observe?.Invoke(toB,bytes);var filter=Filter;byte[]? selected=filter==null?bytes:filter(toB,bytes);
        if(selected==null||stopping)return;
        if(filter!=null)capture?.Invoke(selected);
        Interlocked.Increment(ref queued);
        if(!frames.TryAdd((toB,selected))){Interlocked.Decrement(ref queued);throw new InvalidOperationException("Bounded Ethernet queue exhausted");}
    }
    private void Pump()
    {
        try
        {
            while(!stopping||frames.Count!=0)
            {
                if(frames.TryTake(out var frame,1)){if(frame.ToB)B.Receive(interfaceB,frame.Bytes);else A.Receive(interfaceA,frame.Bytes);Interlocked.Increment(ref processed);}
                A.Poll();B.Poll();
            }
        }
        catch(Exception failure){error=failure;}
    }
    internal void Drain()
    {
        var limit=System.Diagnostics.Stopwatch.StartNew();
        while(limit.Elapsed<TimeSpan.FromSeconds(10))
        {
            Check();long submitted=Volatile.Read(ref queued);
            if(frames.Count!=0||Volatile.Read(ref processed)<submitted){Thread.Yield();continue;}
            // Every observed ingress has been posted before these core barriers.
            A.Invoke(_=>{});B.Invoke(_=>{});
            if(frames.Count==0&&Volatile.Read(ref queued)==submitted&&Volatile.Read(ref processed)>=submitted)return;
            Thread.Yield();
        }
        throw new TimeoutException("Ethernet ingress/core queues did not drain");
    }
    internal void Check()
    {if(error!=null)throw new AggregateException("Ethernet transport failed",error);LwipFrames.ThrowErrors(A.Core);LwipFrames.ThrowErrors(B.Core);}
    public void Dispose()
    {
        stopping=true;if(!transport.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("Ethernet transport did not stop");
        try{Check();}
        finally{try{A.Dispose();}finally{try{B.Dispose();}finally{frames.Dispose();}}}
    }
}
