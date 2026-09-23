using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Managed.Emulation.Host;

internal static class PageTableChecks
{
    internal static unsafe void Run()
    {
        const int pageCount = 4096;
        byte[] backing = GC.AllocateArray<byte>(pageCount * 4096, pinned: true);
        nint start = (nint)Unsafe.AsPointer(ref backing[0]);
        var table = new HostPageTable();
        var published = new ConcurrentDictionary<ulong, nint>();
        Parallel.For(0, pageCount, i =>
        {
            nint address = start + i * 4096;
            ulong entry = table.Track(address);
            if (!published.TryAdd(entry, address)) throw new InvalidOperationException("Duplicate page identity.");
            // Keep resolving previously published pages while other threads
            // append entries and grow the table.
            foreach (var pair in published.Take(8))
                if (table.Find(pair.Key | 0x8000000000000407) != pair.Value)
                    throw new InvalidOperationException("Page address changed during growth.");
        });
        if (published.Count != pageCount) throw new InvalidOperationException("Missing page identities.");
        foreach (var pair in published)
            if (table.Find(pair.Key) != pair.Value) throw new InvalidOperationException("Incorrect published page.");
        var independent = new HostPageTable();
        ulong first = independent.Track(start + 1);
        if (first != 0 || independent.Find(first) != start + 1 || table.Find(first) == start + 1)
            throw new InvalidOperationException("Page registries are not independent.");
        GC.KeepAlive(backing);
        Console.WriteLine("PASS 4096 concurrently registered pages, lookup during growth, independent registries");
    }
}
