using System;
using System.Collections.Generic;
using System.Threading;
namespace Managed.Networking;
// A stopped service may retain callback roots until the original core can no
// longer invoke them. Hooks remain registered through failed disposal/retry.
internal interface ILwipAfterCoreStop { void ReleaseAfterCoreStop(); }
public sealed unsafe partial class LwipStack
{
    private readonly List<IDisposable> services=new();
    private readonly List<ILwipAfterCoreStop> afterCoreStop=new();
    private bool stopping;
    private bool stopMarkerQueued;
    private bool workersJoined;
    private bool applicationStorageReleased;
    private bool hostStopped;
    private int disposalThread;
    private int serviceConstructionDepth;
    /// <summary>Start and own a service atomically with stack lifetime. Dispose must close local operations and join workers without waiting for remote network progress. A factory that throws must release its partial resources. Neither factory nor cleanup may synchronously reenter this facade from a core callback.</summary>
    public T StartService<T>(Func<LwipStack,T> factory) where T:class,IDisposable
    {
        RejectCallbackReentry();
        ArgumentNullException.ThrowIfNull(factory);
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed||stopping,this);
            if(!started)throw new InvalidOperationException("Stack is not ready.");
            // A successful native factory must not become untracked because
            // its subsequent managed registration needs another allocation.
            services.EnsureCapacity(services.Count+1);
            afterCoreStop.EnsureCapacity(afterCoreStop.Count+1);
            T service;
            serviceConstructionDepth++;
            try{service=factory(this)??throw new InvalidOperationException("Service factory returned null.");}
            finally{serviceConstructionDepth--;}
            services.Add(service);
            if(service is ILwipAfterCoreStop retained)afterCoreStop.Add(retained);
            return service;
        }
    }
    /// <summary>After a failed Dispose and before the core stop marker, synchronously release previously borrowed resources. Does not reopen admission for new work. Callback may not reenter this facade.</summary>
    public void CleanupForDisposal(Action<Lwip> cleanup)
    {
        RejectCallbackReentry();ArgumentNullException.ThrowIfNull(cleanup);
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);
            if(!stopping||stopMarkerQueued||disposalThread!=0)throw new InvalidOperationException("Cleanup requires failed disposal with the core still available.");
            disposalThread=Environment.CurrentManagedThreadId;
            try{Invoke(cleanup);}finally{disposalThread=0;}
        }
    }
    // Register an empty managed wrapper before it acquires native state. A
    // failed rollback remains reachable through the owner's disposal registry.
    internal T StartInitializedService<T>(Func<LwipStack,T> allocate,Action<T> initialize) where T:class,IDisposable
    {
        RejectCallbackReentry();lock(gate)
        {
            T service=StartService(allocate);
            serviceConstructionDepth++;
            try{initialize(service);return service;}
            catch(Exception initialization)
            {
                try{service.Dispose();}
                catch(Exception cleanup){throw new AggregateException("Service initialization and rollback failed; registered backing retained for stack disposal retry.",initialization,cleanup);}
                RemoveServiceReference(service);
                // Retained callback hooks survive ordinary registration removal
                // until the core joins, just as after normal service stop.
                throw;
            }
            finally{serviceConstructionDepth--;}
        }
    }
    // Multi-phase link boundaries hold the existing facade gate on the caller
    // thread. Invoke enters the core separately; FIFO waits never run on core.
    internal void RunServiceBoundary(Action action)
    {
        RejectCallbackReentry();lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);CheckServiceAdmission();
            if(!started)throw new InvalidOperationException("Stack is not ready.");
            action();
        }
    }
    internal void ServiceBarrier()=>RunServiceBoundary(DrainCoreQueue);
    internal void RequireOwnedInterface(nint value)
    {
        // Caller holds the facade service boundary, so removal cannot race.
        if(value!=0&&!interfaces.Contains(value))throw new ArgumentException("The transport interface must belong to this stack.",nameof(value));
    }
    private void CheckServiceAdmission()
    {
        if(stopMarkerQueued||(stopping&&disposalThread!=Environment.CurrentManagedThreadId))
            throw new ObjectDisposedException(nameof(LwipStack),"Stack is stopping; only its registered service cleanup may enter.");
    }
    // Called only after a built-in service has completed native cleanup and
    // stopped its callers. Post-core-stop callback retention is independent.
    internal void CompleteServiceDisposal(IDisposable service)
    {
        RejectCallbackReentry();lock(gate)RemoveServiceReference(service);
    }
    private void RemoveServiceReference(IDisposable service)
    {
        for(int i=services.Count-1;i>=0;i--)
            if(ReferenceEquals(services[i],service)){services.RemoveAt(i);return;}
    }
    private void StopOwnedServices()
    {
        while(services.Count!=0)
        {
            var service=services[^1];
            // Successful built-ins may unregister themselves. A failed stop
            // keeps its exact registration and the core available for retry.
            service.Dispose();RemoveServiceReference(service);
        }
    }
    private void ReleaseStoppedServiceContexts()
    {
        while(afterCoreStop.Count!=0)
        {
            int last=afterCoreStop.Count-1;
            afterCoreStop[last].ReleaseAfterCoreStop();
            afterCoreStop.RemoveAt(last);
        }
    }
}
