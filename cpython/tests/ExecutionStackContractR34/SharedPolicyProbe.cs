using System.Runtime.InteropServices;
using static DotCC.Libc.Libc;

// Actual current shared Libc + ordinary threads, also compiled/executed in AOT.
internal static unsafe class SharedPolicyProbe
{
    const int Minimum = 64 * 1024 * 1024;
    sealed class Owner : IProgramInstance { public RuntimeContext __DotCcRuntime { get; } = new(); public ulong Physical; public bool Seen; }
    [DllImport("libc", EntryPoint="pthread_self")] static extern nuint HostSelf();
    [DllImport("libc", EntryPoint="pthread_getattr_np")] static extern int HostAttributes(nuint thread, void* attributes);
    [DllImport("libc", EntryPoint="pthread_attr_getstack")] static extern int HostStack(void* attributes, void** address, nuint* size);
    [DllImport("libc", EntryPoint="pthread_attr_destroy")] static extern int HostDestroy(void* attributes);
    static void Require(bool value) { if (!value) throw new InvalidOperationException("Shared policy contract failed"); }
    static void* Worker(Owner owner, void* argument)
    {
        owner.Seen = ReferenceEquals(CaptureRuntimeContext(), owner.__DotCcRuntime);
        byte* attributes = stackalloc byte[128];
        Require(HostAttributes(HostSelf(), attributes) == 0);
        try { void* address; nuint size; Require(HostStack(attributes, &address, &size) == 0); Require(address != null); owner.Physical = size; }
        finally { Require(HostDestroy(attributes) == 0); }
        return argument;
    }
    static void* StaticWorker(void* argument) => Worker((Owner)GCHandle.FromIntPtr((nint)argument).Target!, argument);
    static int Main()
    {
        var owner = new Owner();
        using var context = owner.__DotCcRuntime;
        using var binding = context.Enter();
        using var policy = context.UseMinimumPthreadStackSize(Minimum);
        int attr;
        Require(pthread_attr_init(&attr) == 0);
        var handle = GCHandle.Alloc(owner);
        try
        {
            foreach (int logical in new[] {0, DOTCC_PTHREAD_DEFAULT_STACK_SIZE, PTHREAD_STACK_MIN, 65*1024*1024})
            {
                if (logical != 0) Require(pthread_attr_setstacksize(&attr,(ulong)logical) == 0);
                ulong observed; Require(pthread_attr_getstacksize(&attr,&observed) == 0);
                Require(observed == (ulong)(logical == 0 ? DOTCC_PTHREAD_DEFAULT_STACK_SIZE : logical));
                for (int route=0; route<2; ++route)
                {
                    long thread;
                    Require((route == 0 ? pthread_create(&thread,logical == 0 ? null : &attr,&StaticWorker,(void*)GCHandle.ToIntPtr(handle))
                        : pthread_create(owner,&thread,logical == 0 ? null : &attr,&Worker,null)) == 0);
                    Require(pthread_join(thread,null) == 0);
                    Require(owner.Seen && owner.Physical >= (ulong)Math.Max(logical, Minimum));
                    Console.WriteLine($"shared route={route} logical={logical} physical={owner.Physical}");
                }
            }
        }
        finally { handle.Free(); Require(pthread_attr_destroy(&attr) == 0); }
        Console.WriteLine("PASS current-Libc physical stack bounds; owner+2=192MiB, owner+8=576MiB requested virtual plus runtime overhead; no Python execution claim");
        return 0;
    }
}
