using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Runtime.ExceptionServices;

namespace Managed.Networking;

/// <summary>Owns one translated stack and its host worker lifetimes.</summary>
public sealed unsafe partial class LwipStack : IDisposable
{
    private readonly Lwip owner = new();
    private readonly object gate = new();
    private readonly List<nint> interfaces = new();
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
        if(mac.Length!=6)throw new ArgumentException("Ethernet addresses contain six bytes.",nameof(mac));
        byte[] address4=Encoding.UTF8.GetBytes(ipv4+"\0"),address6=Encoding.UTF8.GetBytes(ipv6+"\0"),hardware=mac.ToArray(),subnet=Encoding.UTF8.GetBytes(netmask+"\0"),route=Encoding.UTF8.GetBytes(gateway+"\0");
        nint result=0;
        Invoke(core=>
        {
            ObjectDisposedException.ThrowIf(stopping,this);
            fixed(byte* a=address4,b=address6,m=hardware,mask=subnet,gw=route)
                result=(nint)core.lwip_host_interface_add(a,mask,gw,b,m);
            if(result==0)throw new InvalidOperationException("Interface creation failed.");
            LwipFrames.Attach(core,result,output);interfaces.Add(result);core.lwip_host_interface_up((void*)result);
        });
        return result;
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
                Invoke(core=>
                {
                    while(interfaces.Count!=0)
                    {
                        int index=interfaces.Count-1;var address=interfaces[index];
                        LwipFrames.Detach(core,address);core.lwip_host_interface_remove((void*)address);interfaces.RemoveAt(index);
                    }
                });
                using(owner.__DotCcEnter())
                    if(owner.lwip_host_stop_worker()!=0)throw new InvalidOperationException("Cannot queue TCP/IP shutdown marker; owner retained.");
                // A successful queue admits the marker exactly once. A later join or
                // cleanup failure must never dispatch another call to the stopped core.
                stopMarkerQueued=true;
            }
            if(!workersJoined){LwipSysArch.JoinWorkers(owner);workersJoined=true;}
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
