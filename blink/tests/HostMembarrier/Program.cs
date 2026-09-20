using System;
using Managed.Emulation;
using Managed.Emulation.Host;

var owner = new HostSingleThreadMemoryBarrier();
if (!owner.IsOwnerThread || owner.Registered || owner.FenceCount != 0 || owner.CapabilityFenceCount != 0)
    throw new InvalidOperationException("Unexpected initial memory barrier state.");
Blink.BindHostMembarrier(owner);
try
{
    if (Blink.MembarrierProbe() != 0 || !owner.Registered || owner.FenceCount != 2 || owner.CapabilityFenceCount != 1)
        throw new InvalidOperationException("Memory barrier probe/state mismatch.");
}
finally { Blink.UnbindHostMembarrier(); }
