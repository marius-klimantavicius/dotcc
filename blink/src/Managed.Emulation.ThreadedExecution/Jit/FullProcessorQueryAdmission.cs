using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Mapping authority only. This class never reads descriptors or computes LSL.
// The caller must ALREADY hold the allocation owner's actual lifetime/immutability
// lease. Pointer equality and length checks alone are not ownership proof.
internal static unsafe class FullProcessorQueryAdmission
{
    internal const uint DescriptorMappingTag = 0x51523136;
    internal const uint RealSize = 0x1000000;

    // Bind for ONE synchronous native invocation, including native links. Keep
    // system/real/base/limit/descriptor bytes stable through RET. Rebind on resume.
    // A default Linux empty GDT needs no binding. Nonempty without binding is
    // explicitly MISSING; never manufacture a lease from an arbitrary host pointer.
    internal static bool BindRetainedDescriptorMapping(FullBlockFrame* frame, Blink.System* system,
        byte* retainedReal, nuint retainedLength)
    {
        if (frame == null) return false;
        frame->Reserved = 0;
        var m = (Blink.Machine*)frame->Machine;
        if (!FullBlockEmitter.ProcessorQueryLayoutSupported || m == null || system == null ||
            m->blink_host_system != system ||
            m->mode.omode != Blink.XED_MODE_LONG || m->mode.genmode != Blink.XED_GEN_MODE_PROTECTED ||
            (m->flags & 0x50100) != 0 || retainedReal == null || system->real != retainedReal ||
            retainedLength < RealSize) return false;
        if ((int)m->metal != 0) return false;
        ulong start = (ulong)retainedReal;
        // Host canonical allocation endpoints, no overflow. Actual ownership and
        // readability are supplied by the retained mapping owner, not this test.
        if (start > ulong.MaxValue - (RealSize - 1)) return false;
        ulong end = start + RealSize - 1;
        if (!Canonical(start) || !Canonical(end) || (start >> 47) != (end >> 47)) return false;
        frame->Reserved = DescriptorMappingTag; return true;
    }

    internal static void UnbindDescriptorMapping(FullBlockFrame* frame)
    { if (frame != null) frame->Reserved = 0; }

    private static bool Canonical(ulong address) => unchecked((long)address) >> 47 is 0 or -1;
}
