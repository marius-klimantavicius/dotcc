using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// A descriptor is valid only while the caller holds BOTH the guest mapping
// mutex and native gate. Mapping writers hold that mutex before mutation and
// advance this cache's epoch under the native gate. Epochs permit reuse under
// a live lock; they never substitute for writer exclusion or pinning.
internal sealed unsafe class GuestDataLeaseCache
{
    internal const long Charge = 8192;
    private struct Entry
    {
        internal ulong Page;
        internal uint Epoch, Used;
        internal JitDataDescriptor Descriptor;
    }
    private readonly Entry[] entries = new Entry[64];
    private uint epoch = 1, clock;
    internal uint Epoch => epoch;
    internal void Invalidate()
    {
        epoch = unchecked(epoch + 1);
        if (epoch != 0) return;
        // No invocation can retain a descriptor while the native gate is held.
        // Clear before recycling the numeric epoch, including every old tag.
        Array.Clear(entries); epoch = 1;
    }

    private bool Resolve(Blink program, Blink.Machine* machine, ulong page, out JitDataDescriptor descriptor)
    {
        int first = (int)(((page >> 12) ^ (page >> 24)) & 15) * 4, victim = first;
        for (int i = first; i < first + 4; ++i)
        {
            ref var entry = ref entries[i];
            if (entry.Epoch == epoch && entry.Page == page)
            { entry.Used = ++clock; descriptor = entry.Descriptor; return true; }
            if (entry.Epoch != epoch || entries[victim].Epoch == epoch && entry.Used < entries[victim].Used) victim = i;
        }
        descriptor = default;
        if (!JitPageIdentity.TryRead(program, machine, page, out ulong pte) ||
            (pte & (ulong)(Blink.PAGE_U | Blink.PAGE_XD | Blink.PAGE_RSRV)) != (ulong)(Blink.PAGE_U | Blink.PAGE_XD)) return false;
        byte* host = (pte & (ulong)Blink.PAGE_HOST) != 0 ? program.FindHostPage(pte) : program.GetPageAddress(machine->blink_host_system, pte, false);
        if (host == null || program.BlinkHostMemoryContains(host, 4096) == 0) return false;
        if ((pte & (ulong)Blink.PAGE_HOST) != 0 &&
            (program.BlinkHostMemoryFilePageShared(host) != 0 || program.BlinkHostMemoryFilePageAvailable(host) == 0)) return false;
        descriptor = new() { Page = page, Host = (nint)host, Identity = (ulong)host,
            Generation = epoch, ValidStart = 0, ValidEnd = 4096,
            Permissions = 1u | ((pte & (ulong)Blink.PAGE_RW) != 0 ? 2u : 0u) };
        entries[victim] = new() { Page = page, Epoch = epoch, Used = ++clock, Descriptor = descriptor };
        return true;
    }

    internal int Fill(Blink program, Blink.Machine* machine, JitAddressPlan plan, Span<JitDataDescriptor> descriptors)
    {
        // Empty/register-only plans must not touch machine or mapping state:
        // those entries may execute while the caller holds only the gate.
        if (!plan.HasMemory || descriptors.IsEmpty) return 0;
        int maximum = Math.Min(descriptors.Length, JitDataDescriptor.MaximumCount);
        ReadOnlySpan<ulong> registers = new((byte*)machine + 24, 16);
        Span<ulong> scratch = stackalloc ulong[plan.EvaluationCount];
        int evaluated = 0;
        int count = 0;
        for (int m = 0; m < plan.Memory.Length; ++m)
        {
            if (!plan.TryEvaluateAddress(m, registers, scratch, ref evaluated, out ulong address)) continue;
            int width = plan.Memory[m].Width;
            if (address >= 0x800000000000UL || width is < 1 or > 8 || (address & 4095) + (uint)width > 4096) continue;
            ulong page = address & ~4095UL;
            bool duplicate = false;
            for (int i = 0; i < count; ++i) if (descriptors[i].Page == page) { duplicate = true; break; }
            if (duplicate || !Resolve(program, machine, page, out var descriptor)) continue;
            descriptors[count++] = descriptor;
            if (count == maximum) break;
        }
        return count;
    }
}
