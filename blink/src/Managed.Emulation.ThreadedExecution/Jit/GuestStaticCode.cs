using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// All pages are enrolled exactly once, before the first guest instruction.
// Data-pointer escape or mapping mutation permanently revokes the address.
// The cache gate is the read lease; Revoke drains it before a writer proceeds.
internal sealed unsafe class GuestStaticCode
{
    private readonly ulong start;
    private readonly nint[] pages;
    private readonly bool[] revoked, committed;
    internal long Charge { get; }
    internal long RevokedPages { get; private set; }
    internal int PageCount { get; }

    private GuestStaticCode(ulong start, nint[] pages, bool[] committed)
    {
        this.start = start; this.pages = pages; this.committed = committed; revoked = new bool[pages.Length];
        Charge = 256 + pages.LongLength * 32;
        PageCount = pages.Count(p => p != 0);
    }

    internal static GuestStaticCode? Enroll(Blink program, Blink.Machine* machine, long available)
    {
        // Producer capability is a real translated export. An older unaudited
        // producer cannot silently enable this execution contract.
        if (program.BlinkGuestCodeRevocationProtocol() != 1 || program.BlinkGuestMappingRevocationProtocol() != 1) return null;
        long length = machine->blink_host_system->codesize;
        ulong address = (ulong)machine->blink_host_system->codestart;
        if (length <= 0 || address >= 0x800000000000UL || (ulong)length > 0x800000000000UL - address) return null;
        ulong first = address & ~4095UL;
        ulong count = ((address & 4095) + (ulong)length + 4095) / 4096;
        // Cache accounting includes all enrollment metadata, independently of
        // whether a page qualifies. No unbounded owner or worker side table.
        if (count == 0 || count > 65536 || count * 32 + 256 > (ulong)Math.Max(0, available)) return null;
        var pointers = new nint[(int)count];
        var committed = new bool[(int)count];
        for (int i = 0; i < pointers.Length; ++i)
        {
            ulong page = first + (uint)i * 4096UL;
            if (!JitPageIdentity.TryRead(program, machine, page, out ulong entry)) continue;
            ulong required = (ulong)(Blink.PAGE_V | Blink.PAGE_U | Blink.PAGE_FILE | Blink.PAGE_HOST);
            if ((entry & required) != required || (entry & (ulong)(Blink.PAGE_RW | Blink.PAGE_XD)) != 0) continue;
            byte* host = program.FindHostPage(entry);
            if (host == null || program.BlinkHostMemoryFilePageShared(host) != 0 ||
                program.BlinkHostMemoryFilePageAvailable(host) == 0 || program.BlinkHostMemoryContains(host, 4096) == 0) continue;
            pointers[i] = (nint)host;
            committed[i] = (entry & (ulong)Blink.PAGE_RSRV) == 0;
        }
        // Capturing an immutable private snapshot must not clear reservation
        // bits or alter guest RSS/admission accounting before guest fetch.
        return new(first, pointers, committed);
    }

    internal bool Validate(Blink program, Blink.Machine* machine, ulong ip)
    {
        if (ip < start) return false;
        ulong index = (ip - start) >> 12;
        if (index >= (ulong)pages.Length || pages[(int)index] == 0 || revoked[(int)index]) return false;
        if (committed[(int)index]) return true;
        if (!JitPageIdentity.TryRead(program, machine, ip, out ulong entry) || (entry & (ulong)Blink.PAGE_RSRV) != 0) return false;
        ulong required = (ulong)(Blink.PAGE_V | Blink.PAGE_U | Blink.PAGE_FILE | Blink.PAGE_HOST);
        if ((entry & required) != required || (entry & (ulong)(Blink.PAGE_RW | Blink.PAGE_XD)) != 0 ||
            (nint)program.FindHostPage(entry) != pages[(int)index]) return false;
        committed[(int)index] = true;
        return true;
    }

    internal bool Contains(ulong ip)
    {
        if (ip < start) return false;
        ulong index = (ip - start) >> 12;
        return index < (ulong)pages.Length && pages[(int)index] != 0 && !revoked[(int)index] && committed[(int)index];
    }

    internal bool Copy(ulong ip, Span<byte> destination)
    {
        if (destination.Length == 0 || destination.Length > 4096 - (int)(ip & 4095) || !Contains(ip)) return false;
        new ReadOnlySpan<byte>((byte*)pages[(int)((ip - start) >> 12)] + (int)(ip & 4095), destination.Length).CopyTo(destination);
        return true;
    }

    internal bool MayOverlap(ulong address, ulong length)
    {
        ulong end = start + (ulong)pages.Length * 4096;
        return length != 0 && address < end && (address >= start || length > start - address);
    }

    internal void Revoke(ulong address, ulong length)
    {
        if (!MayOverlap(address, length)) return;
        ulong offset = address > start ? address - start : 0;
        ulong first = offset >> 12;
        ulong end = length > ulong.MaxValue - address ? ulong.MaxValue : address + length;
        ulong last = end <= start ? 0 : Math.Min((ulong)pages.Length, ((end - start - 1) >> 12) + 1);
        for (ulong i = first; i < last; ++i)
            if (pages[(int)i] != 0 && !revoked[(int)i]) { revoked[(int)i] = true; ++RevokedPages; }
    }
}
