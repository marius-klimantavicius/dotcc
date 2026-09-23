namespace Managed.Emulation.Host;

/// <summary>Per-execution non-linear Blink page identities. The caller owns page
/// storage; this table publishes stable addresses without freeing a reader's array.</summary>
public sealed class HostPageTable
{
    private readonly object gate = new();
    private nint[] pages = new nint[64];
    private int count;

    public ulong Track(nint page)
    {
        if (page == 0) throw new ArgumentException("A host page address is required.", nameof(page));
        lock (gate)
        {
            var current = pages;
            if (count == current.Length)
            {
                var larger = new nint[checked(current.Length * 2)];
                current.CopyTo(larger, 0);
                Volatile.Write(ref pages, current = larger);
            }
            // A slot is assigned once. Return its identity only after both the
            // array and address are visible to workers publishing guest PTEs.
            Volatile.Write(ref current[count], page);
            return (ulong)count++ << 12;
        }
    }

    public nint Find(ulong entry)
    {
        const ulong pageAddressMask = 0x0000fffffffff000;
        ulong index = (entry & pageAddressMask) >> 12;
        var current = Volatile.Read(ref pages);
        if (index >= (ulong)current.Length) throw new InvalidOperationException("Unregistered host page identity.");
        nint page = Volatile.Read(ref current[(int)index]);
        if (page == 0) throw new InvalidOperationException("Unpublished host page identity.");
        return page;
    }
}
