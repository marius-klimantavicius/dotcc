using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    partial void MappingRevocationBudget(ref int remaining);
    // Mapping mutations are rare. Bound work by metadata actually visited,
    // skip absent subtrees, and fall back globally on any incomplete proof.
    // This is a no-fault raw metadata walk; it never commits a guest page or
    // consults a cached native data pointer. Caller owns Gate and exclusion.
    private bool RevokeMappingPhysicalSources(Blink program, Blink.Machine* machine, ulong start, ulong length)
    {
        BeginRead(program, semantic: false);
        try
        {
            int remaining = 65536;
            MappingRevocationBudget(ref remaining); // erased in production
            ulong root = machine->blink_host_system->cr3;
            if ((root & (ulong)Blink.PAGE_V) == 0) return false;
            return RevokeMappingTable(program, machine->blink_host_system, root, 39, start, start + length, ref remaining);
        }
        finally { EndRead(program); }
    }
    private bool RevokeMappingTable(Blink program, Blink.System* system, ulong entry, int level,
        ulong start, ulong end, ref int remaining)
    {
        byte* table = program.GetPageAddress(system, entry, level == 39);
        if (table == null || program.BlinkHostMemoryContains(table, 4096) == 0) return false;
        ulong span = 1UL << level;
        while (start < end)
        {
            if (--remaining < 0) return false;
            ulong next = Math.Min(end, (start | (span - 1)) + 1);
            ulong child = program.LoadPte(table + (int)((start >> level) & 511) * 8);
            if ((child & (ulong)Blink.PAGE_V) != 0)
            {
                if ((child & (ulong)Blink.PAGE_PS) != 0) return false;
                if (level > 12)
                {
                    if ((child & (ulong)Blink.PAGE_RSRV) != 0 ||
                        !RevokeMappingTable(program, system, child, level - 9, start, next, ref remaining)) return false;
                }
                else if ((child & (ulong)Blink.PAGE_HOST) != 0)
                {
                    byte* page = program.FindHostPage(child);
                    if (page == null || ((nuint)page & 4095) != 0 || program.BlinkHostMemoryContains(page, 4096) == 0)
                        return false;
                    // Includes reserved file backing. Never infer its physical
                    // ownership from executable bits, Code cells, or the FD.
                    Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page, 4096);
                }
                else if ((child & (ulong)Blink.PAGE_RSRV) == 0) return false;
            }
            start = next;
        }
        return true;
    }
}
