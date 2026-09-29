namespace Managed.Networking;
// Standalone host-contract witness only. Product and integration suites use the real generated owner.
public sealed class Lwip
{
    private int leases;
    public System.IDisposable __DotCcEnter() => new Binding();
    private sealed class Binding : System.IDisposable { public void Dispose() { } }
    public void Dispose() { if(Leases!=0)throw new System.InvalidOperationException("Live leases"); }
    public int Leases => System.Threading.Volatile.Read(ref leases);
    public System.IDisposable __DotCcRetain() { System.Threading.Interlocked.Increment(ref leases); return new Lease(this); }
    private sealed class Lease(Lwip owner) : System.IDisposable
    {
        private Lwip? held = owner;
        public void Dispose() { var value = System.Threading.Interlocked.Exchange(ref held, null); if(value != null) System.Threading.Interlocked.Decrement(ref value.leases); }
    }
}
