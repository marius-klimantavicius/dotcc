using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Fresh metadata walk under the native gate. Never consult a worker TLB or
// fault/commit a guest page. A retained initial mapping pins this table path:
// mutations revoke it before removal, and empty-table collection cannot free it.
internal static unsafe class JitPageIdentity
{
    internal static bool TryRead(Blink program, Blink.Machine* machine, ulong address, out ulong entry)
    {
        var system = machine->blink_host_system;
        entry = system->cr3;
        for (int level = 39; level >= 12; level -= 9)
        {
            byte* table = program.GetPageAddress(system, entry, level == 39);
            if (table == null || program.BlinkHostMemoryContains(table, 4096) == 0) return false;
            entry = program.LoadPte(table + (int)((address >> level) & 511) * 8);
            if ((entry & (ulong)Blink.PAGE_V) == 0 || (entry & (ulong)Blink.PAGE_PS) != 0 ||
                (level != 12 && (entry & (ulong)Blink.PAGE_RSRV) != 0)) return false;
        }
        return true;
    }
}
