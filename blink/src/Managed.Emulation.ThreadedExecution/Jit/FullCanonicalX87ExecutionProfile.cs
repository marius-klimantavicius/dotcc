using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Finite selected native12 Linux long/protected profile, not architectural
// debug-register/#DB or alignment-check emulation. Dispatch preserves TF/RF;
// ImportFlags alone clears RF. Linux LoadProgram sets CR0=80000013 (AM=0),
// MOV CR is selected UD and user LMSW cannot change AM. Never infer eligibility
// from AC alone or grant AM=1/AC=1 memory semantics without qualification.
internal static unsafe class FullCanonicalX87ExecutionProfile
{
    internal const uint AlignmentMask = 1u << 18;
    internal const ulong ContextAm = 1UL << 57, ContextMetal = 1UL << 58, ContextStash = 1UL << 59;
    internal static readonly int MachineSystemOffset = Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.blink_host_system)).ToInt32();
    internal static readonly int SystemCr0Offset = Marshal.OffsetOf<Blink.System>(nameof(Blink.System.cr0)).ToInt32();
    internal static readonly int MachineMetalOffset = Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.metal)).ToInt32();
    internal static readonly int MachineStashOffset = Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.stashaddr)).ToInt32();
    internal static bool LayoutSupported => MachineSystemOffset >= 0 && MachineSystemOffset <= sizeof(Blink.Machine)-sizeof(nint) &&
        SystemCr0Offset >= 0 && SystemCr0Offset <= sizeof(Blink.System)-sizeof(ulong) &&
        MachineMetalOffset >= 0 && MachineMetalOffset < sizeof(Blink.Machine) &&
        MachineStashOffset >= 0 && MachineStashOffset <= sizeof(Blink.Machine)-sizeof(ulong);
    internal static bool AllowsFlags(Blink.Machine* m, bool canonicalOwner)
    {
        uint flags = m->flags;
        if (!canonicalOwner)
            return (flags & ((1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF) | AlignmentMask)) == 0;
        return (flags & AlignmentMask) == 0 || m->blink_host_system != null &&
            (m->blink_host_system->cr0 & AlignmentMask) == 0;
    }
    internal static ulong ContextBits(Blink.Machine* m) =>
        ((m->flags & AlignmentMask) != 0 && m->blink_host_system != null && (m->blink_host_system->cr0 & AlignmentMask) != 0 ? ContextAm : 0) |
        ((int)m->metal != 0 ? ContextMetal : 0) | (m->stashaddr != 0 ? ContextStash : 0);
}
