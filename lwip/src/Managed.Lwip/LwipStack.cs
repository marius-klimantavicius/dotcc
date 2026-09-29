using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Runtime.ExceptionServices;
using System.Net;
using System.Net.Sockets;

namespace Managed.Networking;

/// <summary>Owns one translated stack and its host worker lifetimes.</summary>
public sealed unsafe partial class LwipStack : IDisposable
{
    private readonly Lwip owner = new();
    private readonly object gate = new();
    private readonly List<nint> interfaces = new();
    private readonly List<nint> retiredInterfaces = new();
    [ThreadStatic] private static LwipStack? executingCallback;
    private void RejectCallbackReentry()
    {if(ReferenceEquals(executingCallback,this)||LwipSysArch.IsCoreContext(owner))throw new InvalidOperationException("A core callback cannot synchronously reenter its owning facade.");}
    private bool started;
    private bool disposed;
    private ManualResetEventSlim? readiness;
    private GCHandle readinessHandle;
    public Lwip Core => !disposed ? owner : throw new ObjectDisposedException(nameof(LwipStack));
    public void Start()
    {
        RejectCallbackReentry();
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed||stopping, this);
            if (readiness != null) throw new InvalidOperationException("Stack initialization already attempted.");
            readiness = new ManualResetEventSlim();
            readinessHandle = GCHandle.Alloc(readiness);
            using (owner.__DotCcEnter()) owner.lwip_host_initialize(&Ready, (void*)GCHandle.ToIntPtr(readinessHandle));
            if (!readiness.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("TCP/IP worker did not report readiness; dispose stack to join pending initialization.");
            started = true;
        }
    }
    private static void Ready(Lwip owner, void* argument) => ((ManualResetEventSlim)GCHandle.FromIntPtr((nint)argument).Target!).Set();
    private sealed class Call(LwipStack initiator,Action<Lwip> action)
    {
        internal readonly Action<Lwip> Action = action;
        internal readonly LwipStack Initiator=initiator;
        internal Exception? Error;
    }
    private static void InvokeCallback(Lwip owner, void* argument)
    {
        var call = (Call)GCHandle.FromIntPtr((nint)argument).Target!;
        var previous=executingCallback;executingCallback=call.Initiator;
        try { call.Action(owner); } catch(Exception error) { call.Error = error; }
        finally{executingCallback=previous;}
    }
    /// <summary>Execute synchronously on the translated TCP/IP core. Callback must not reenter this facade.</summary>
    public void Invoke(Action<Lwip> action)
    {
        RejectCallbackReentry();
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);CheckServiceAdmission();
            if(!started)throw new InvalidOperationException("Stack is not ready.");
            var call=new Call(this,action);var handle=GCHandle.Alloc(call);
            try
            {
                using(owner.__DotCcEnter())
                    if(owner.lwip_host_call(&InvokeCallback,(void*)GCHandle.ToIntPtr(handle))!=0)throw new InvalidOperationException("Cannot dispatch core callback.");
                if(call.Error!=null)ExceptionDispatchInfo.Capture(call.Error).Throw();
            }
            finally{handle.Free();}
        }
    }
    public nint AddInterface(string ipv4,string ipv6,ReadOnlySpan<byte> mac,Action<byte[]> output,string netmask="255.255.255.0",string gateway="0.0.0.0")
    {
        RejectCallbackReentry();ArgumentNullException.ThrowIfNull(output);
        static void CheckAddress(string value,string name,bool v6,bool optional=false)
        {
            ArgumentNullException.ThrowIfNull(value,name);
            if(optional&&value.Length==0)return;
            if(value.Contains('\0')||value.Contains('%')||!IPAddress.TryParse(value,out var parsed)||
               parsed.AddressFamily!=(v6?AddressFamily.InterNetworkV6:AddressFamily.InterNetwork)||
               (v6&&parsed.ScopeId!=0))throw new ArgumentException("A complete IP address of the specified family is required; interface addresses do not contain zone suffixes.",name);
        }
        CheckAddress(ipv4,nameof(ipv4),false);CheckAddress(ipv6,nameof(ipv6),true,true);
        CheckAddress(netmask,nameof(netmask),false);CheckAddress(gateway,nameof(gateway),false);
        if(mac.Length!=6)throw new ArgumentException("Ethernet addresses contain six bytes.",nameof(mac));
        byte[] address4=Encoding.UTF8.GetBytes(ipv4+"\0"),address6=Encoding.UTF8.GetBytes(ipv6+"\0"),hardware=mac.ToArray(),subnet=Encoding.UTF8.GetBytes(netmask+"\0"),route=Encoding.UTF8.GetBytes(gateway+"\0");
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed||stopping,this);
            interfaces.EnsureCapacity(interfaces.Count+1);retiredInterfaces.EnsureCapacity(retiredInterfaces.Count+1);
            nint result=0,previousDefault=0;
            try
            {
                Invoke(core=>
                {
                    previousDefault=(nint)core.Globals.netif_default;
                    fixed(byte* a=address4,b=address6,m=hardware,mask=subnet,gw=route)
                        result=(nint)core.lwip_host_interface_add(a,mask,gw,b,m);
                    if(result==0)throw new InvalidOperationException("Interface creation failed.");
                    // Capacity was reserved before native allocation. Every
                    // allocated interface is registered before attach/up may fail.
                    interfaces.Add(result);LwipFrames.Attach(core,result,output);core.lwip_host_interface_up((void*)result);
                });
                return result;
            }
            catch(Exception original)
            {
                if(result!=0)
                {
                    try
                    {
                        DrainCoreQueue();
                        Invoke(core=>
                        {
                            if(!retiredInterfaces.Contains(result))retiredInterfaces.Add(result);
                            LwipFrames.Detach(core,result);core.lwip_host_interface_stop((void*)result);interfaces.Remove(result);
                            if(interfaces.Contains(previousDefault))core.netif_set_default((Lwip.netif*)previousDefault);
                        });
                        DrainCoreQueue();
                        Invoke(core=>{core.lwip_host_interface_free((void*)result);retiredInterfaces.Remove(result);});
                    }
                    catch(Exception cleanup){throw new AggregateException("Interface initialization failed; registered backing is retained for disposal retry.",original,cleanup);}
                }
                throw;
            }
        }
    }
    /// <summary>Select the fallback route among this owner's interfaces.</summary>
    public void SetDefaultInterface(nint interfaceAddress) => Invoke(core=>
    {
        if(!interfaces.Contains(interfaceAddress))throw new ArgumentException("Interface does not belong to this stack.",nameof(interfaceAddress));
        core.netif_set_default((Lwip.netif*)interfaceAddress);
    });
    /// <summary>Change physical link availability while preserving configured addresses.</summary>
    public void SetLinkState(nint interfaceAddress,bool up) => Invoke(core=>
    {
        if(!interfaces.Contains(interfaceAddress))throw new ArgumentException("Interface does not belong to this stack.",nameof(interfaceAddress));
        if(up)core.netif_set_link_up((Lwip.netif*)interfaceAddress);else core.netif_set_link_down((Lwip.netif*)interfaceAddress);
    });
    public void Receive(nint interfaceAddress,ReadOnlySpan<byte> frame)
    {
        RejectCallbackReentry();
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);CheckServiceAdmission();
            if(!interfaces.Contains(interfaceAddress))throw new ArgumentException("Interface does not belong to this stack.");
            var previous=executingCallback;executingCallback=this;
            try
            {
                using(owner.__DotCcEnter())
                fixed(byte* bytes=frame)
                    if(owner.lwip_host_frame_input((void*)interfaceAddress,bytes,(uint)frame.Length)!=0)throw new InvalidOperationException("Frame input rejected or queue exhausted.");
            }
            finally{executingCallback=previous;}
        }
    }
    public void Poll()
    {
        RejectCallbackReentry();lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);CheckServiceAdmission();
            var previous=executingCallback;executingCallback=this;
            try{using(owner.__DotCcEnter())owner.lwip_host_poll();}finally{executingCallback=previous;}
        }
    }
    // Unlike tcpip_callback_wait under core locking, this queues a real FIFO
    // marker. It must execute outside a core callback and outside the core lock.
    private void DrainCoreQueue()
    {
        using(owner.__DotCcEnter())
            if(owner.lwip_host_barrier()!=0)throw new InvalidOperationException("Cannot queue core lifetime barrier; interfaces retained.");
    }
    public void Dispose()
    {
        RejectCallbackReentry();
        lock (gate)
        {
            if (disposed) return;
            if(disposalThread==Environment.CurrentManagedThreadId)throw new InvalidOperationException("Cannot recursively dispose a stack from its service cleanup.");
            if(serviceConstructionDepth!=0)throw new InvalidOperationException("Cannot dispose a stack from its service factory.");
            stopping=true;disposalThread=Environment.CurrentManagedThreadId;
            try
            {
            if(readiness!=null&&!started)
            {
                if(!readiness.Wait(TimeSpan.FromSeconds(10)))throw new TimeoutException("Initialization has not completed; owner retained for disposal retry.");
                started=true;
            }
            StopOwnedServices();
            if(started&&!stopMarkerQueued)
            {
                LwipApplications.CheckQuiescent(owner);
                DrainCoreQueue();
                Invoke(core=>
                {
                    while(interfaces.Count!=0)
                    {
                        int index=interfaces.Count-1;var address=interfaces[index];
                        // Register backing before any irreversible removal, so
                        // allocation failure cannot lose its retirement record.
                        if(!retiredInterfaces.Contains(address))retiredInterfaces.Add(address);
                        LwipFrames.Detach(core,address);core.lwip_host_interface_stop((void*)address);interfaces.RemoveAt(index);
                    }
                });
                DrainCoreQueue();
                Invoke(core=>
                {
                    while(retiredInterfaces.Count!=0)
                    {
                        int last=retiredInterfaces.Count-1;
                        core.lwip_host_interface_free((void*)retiredInterfaces[last]);
                        retiredInterfaces.RemoveAt(last);
                    }
                });
                using(owner.__DotCcEnter())
                    if(owner.lwip_host_stop_worker()!=0)throw new InvalidOperationException("Cannot queue TCP/IP shutdown marker; owner retained.");
                // A successful queue admits the marker exactly once. A later join or
                // cleanup failure must never dispatch another call to the stopped core.
                stopMarkerQueued=true;
            }
            if(!workersJoined){LwipSysArch.JoinWorkers(owner);workersJoined=true;}
            ReleaseStoppedServiceContexts();
            if(!applicationStorageReleased){LwipApplications.Release(owner);applicationStorageReleased=true;}
            if(!hostStopped){LwipSysArch.Stop(owner);hostStopped=true;}
            owner.Dispose();
            if (readinessHandle.IsAllocated) readinessHandle.Free();
            readiness?.Dispose();
            disposed = true;
            }
            finally{disposalThread=0;}
        }
    }
}
