using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Managed.Transport.Hosting;

// Test-only lifecycle facade, compile-linked to the actual resource registry.
public sealed unsafe partial class MsQuicHost : IDisposable
{
    public MsQuicHost() => InitializeResources();
    public void Dispose() => ReleaseContext();
    private sealed class Owner : IDisposable
    {
        public int Disposals;
        public void Dispose() => Interlocked.Increment(ref Disposals);
    }
    private sealed class OtherOwner : IDisposable { public void Dispose() { } }
    private static void Check(bool condition) { if (!condition) throw new Exception("Resource ownership assertion failed."); }

    private static int Main(string[] args)
    {
        if (args.Length != 0)
        {
            using var first = new MsQuicHost();
            using var second = new MsQuicHost();
            void* token = first.AddResource(new Owner());
            switch (args[0])
            {
                case "stale": first.ReleaseResource<Owner>(token); first.Resource<Owner>(token); break;
                case "foreign": second.Resource<Owner>(token); break;
                case "wrong-type": first.Resource<OtherOwner>(token); break;
                default: return 2;
            }
            return 3;
        }
        using (var host = new MsQuicHost())
        {
            Check(ReferenceEquals(FromContext(host.ContextPointer), host));
            var owner = new Owner();
            void* token = host.AddResource(owner);
            Check(ReferenceEquals(owner, host.Resource<Owner>(token)) && host.OutstandingResources == 1);
            bool blocked = false;
            try { host.Dispose(); } catch (InvalidOperationException) { blocked = true; }
            Check(blocked && ReferenceEquals(FromContext(host.ContextPointer), host));
            host.ReleaseResource<Owner>(token);
            Check(owner.Disposals == 1 && host.OutstandingResources == 0);
        }
        // The shared token sequence must not alias another host, even under load.
        using (var host = new MsQuicHost())
        {
            var seen = new System.Collections.Concurrent.ConcurrentDictionary<nint, byte>();
            Parallel.For(0, 2048, _ =>
            {
                var owner = new Owner();
                nint token = (nint)host.AddResource(owner);
                Check(seen.TryAdd(token, 0));
                Check(ReferenceEquals(owner, host.Resource<Owner>((void*)token)));
                host.ReleaseResource<Owner>((void*)token);
                Check(owner.Disposals == 1);
            });
            Check(seen.Count == 2048 && host.OutstandingResources == 0);
        }
        // Either registration succeeds and close refuses, or close wins and the
        // caller still owns the unregistered object. No object may become orphaned.
        for (int iteration = 0; iteration < 128; iteration++)
        {
            var host = new MsQuicHost();
            var owner = new Owner();
            nint registered = 0;
            using var start = new ManualResetEventSlim();
            Task add = Task.Run(() =>
            {
                start.Wait();
                try { registered = (nint)host.AddResource(owner); }
                catch (ObjectDisposedException) { owner.Dispose(); }
            });
            Task close = Task.Run(() =>
            {
                start.Wait();
                try { host.Dispose(); } catch (InvalidOperationException) { }
            });
            start.Set(); Task.WaitAll(add, close);
            if (registered != 0) host.ReleaseResource<Owner>((void*)registered);
            host.Dispose();
            Check(owner.Disposals == 1 && host.OutstandingResources == 0);
        }
        Check(contexts.IsEmpty);
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported ? "jit: registry passed" : "nativeaot: registry passed");
        return 0;
    }
}
