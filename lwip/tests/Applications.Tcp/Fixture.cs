using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

// Draft-only lifetime owner. This is not a consumer-facing application service.
// SMTP/lwiperf callback roots deliberately survive until both cores stop.
internal sealed class Fixture:IDisposable
{
    internal readonly VirtualPair Pair=new();
    readonly List<GCHandle> roots=new();
    readonly List<nint> strings=new();
    readonly List<nint> allocations=new();
    readonly List<Action> cleanup=new();
    internal static void Require(bool value,string message)
    {if(!value)throw new InvalidOperationException(message);}
    internal nint Root(object state)
    {var root=GCHandle.Alloc(state);roots.Add(root);return GCHandle.ToIntPtr(root);}
    internal nint Text(string value)
    {nint result=Marshal.StringToCoTaskMemUTF8(value);strings.Add(result);return result;}
    internal unsafe nint Allocate(nuint size)
    {nint value=(nint)NativeMemory.AllocZeroed(size);if(value==0)throw new OutOfMemoryException();allocations.Add(value);return value;}
    internal void BeforeStop(Action action)=>cleanup.Add(action);
    internal void Wait(Func<bool> ready,Action? check=null,int milliseconds=10000)
    {
        var watch=Stopwatch.StartNew();
        while(!ready())
        {
            Pair.Check();check?.Invoke();Require(watch.ElapsedMilliseconds<milliseconds,"bounded original application progress");
            // A sustained original sender need not make the Ethernet queue idle.
            // Core barriers provide progress without requiring global quiescence.
            Pair.A.Invoke(_=>{});Pair.B.Invoke(_=>{});Thread.Yield();
        }
        Pair.Check();check?.Invoke();
    }
    public void Dispose()
    {
        List<Exception>? errors=null;
        for(int i=cleanup.Count-1;i>=0;i--)try{cleanup[i]();}catch(Exception error){(errors??=new()).Add(error);}
        // Do not release retained callback memory if owner shutdown fails.
        Pair.Dispose();
        foreach(var root in roots)root.Free();foreach(nint text in strings)Marshal.FreeCoTaskMem(text);
        unsafe{foreach(nint allocation in allocations)NativeMemory.Free((void*)allocation);}
        if(errors!=null)throw new AggregateException("TCP fixture cleanup failed",errors);
    }
    internal static unsafe void Address(Lwip owner,string text,ip_addr* result)
    {byte[] bytes=System.Text.Encoding.ASCII.GetBytes(text+"\0");fixed(byte* p=bytes)Require(owner.ipaddr_aton(p,result)==1,"valid fixture address");}
}
