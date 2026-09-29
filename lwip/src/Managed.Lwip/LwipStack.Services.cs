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
    private void CheckServiceAdmission()
    {
        if(stopMarkerQueued||(stopping&&disposalThread!=Environment.CurrentManagedThreadId))
            throw new ObjectDisposedException(nameof(LwipStack),"Stack is stopping; only its registered service cleanup may enter.");
    }
    private void StopOwnedServices()
    {
        for(int i=services.Count-1;i>=0;i--)
        {
            // Remove only after a successful stop; a failed disposal retains its
            // service registration and the live core for an explicit retry.
            services[i].Dispose();services.RemoveAt(i);
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
