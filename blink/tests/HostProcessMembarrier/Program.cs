using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

using var owner = new HostProcessMemoryBarrier();
using var ready = new CountdownEvent(2);
using var firstDone = new ManualResetEventSlim();
Exception?[] failures = new Exception?[2];
owner.AttachCurrentThread();
Blink.BindHostMembarrier(owner);
try
{
    if (Blink.ProcessBarrierInitialize() != 0 || !owner.Registered || owner.CapabilityFenceCount != 1)
        throw new InvalidOperationException("Initial process barrier state differs.");
    Thread Worker(int index) => new(() =>
    {
        bool attached = false, bound = false;
        try
        {
            owner.AttachCurrentThread(); attached = true;
            Blink.BindHostMembarrier(owner); bound = true;
            ready.Signal();
            if (!ready.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Worker attach deadline.");
            if (index == 2 && !firstDone.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("First worker deadline.");
            if (Blink.ProcessBarrierStep(index) != 0) throw new InvalidOperationException("Translated process fence failed.");
        }
        catch (Exception error) { failures[index - 1] = error; }
        finally
        {
            if (index == 1) firstDone.Set();
            if (bound) Blink.UnbindHostMembarrier();
            if (attached) owner.DetachCurrentThread();
        }
    }) { IsBackground = true };
    var first = Worker(1); var second = Worker(2);
    first.Start(); second.Start();
    if (!first.Join(TimeSpan.FromSeconds(15)) || !second.Join(TimeSpan.FromSeconds(15)))
        throw new TimeoutException("Workers did not join; discard process.");
    foreach (var failure in failures) if (failure != null) throw failure;
    if (owner.Attachments != 1 || owner.FenceCount != 2 || owner.CapabilityFenceCount != 1 ||
        Blink.ProcessBarrierFinish() != 0)
        throw new InvalidOperationException("Shared registration/accounting differs.");
}
finally
{
    Blink.UnbindHostMembarrier(); owner.DetachCurrentThread();
}
