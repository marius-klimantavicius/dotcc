using Managed.Transport.Hosting;

internal static unsafe class Program
{
    private const uint Tag = 0x4564674d;
    private static int checks;
    private static void Check(bool value, string reason)
    {
        ++checks;
        if (!value) throw new InvalidOperationException(reason);
    }

    private static void Main()
    {
        using var owner = new MsQuicHost(1);
        using var other = new MsQuicHost(1);
        var table = owner.CreateTable();
        foreach (ulong length in new ulong[] { 0, 1, 15, 16, 17, 65536 })
        {
            byte* memory = (byte*)table.CxPlatAlloc(table.Context, length, Tag);
            Check(memory != null && ((nuint)memory & 15) == 0, "Allocation has stable 16-byte alignment");
            try
            {
                Check(owner.OutstandingPlatformAllocations == 1 && other.OutstandingPlatformAllocations == 0,
                    "Allocation ledger belongs to the owner");
                Check(new ReadOnlySpan<byte>(memory, (int)length).IndexOfAnyExcept((byte)0) < 0,
                    "Requested allocation range is zeroed");
                if (length > 0) memory[length - 1] = 0x73;
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                Check(length == 0 || memory[length - 1] == 0x73, "Native allocation survives compacting GC");
            }
            finally { table.CxPlatFree(table.Context, memory, Tag); }
            Check(owner.OutstandingPlatformAllocations == 0, "Free drains allocation ledger");
        }
        Check(table.CxPlatAlloc(table.Context, ulong.MaxValue, Tag) == null, "Oversized allocation is rejected");
        byte* uninitialized = (byte*)table.CxPlatAllocUninitialized(table.Context, 31, Tag);
        Check(uninitialized != null && ((nuint)uninitialized & 15) == 0, "Uninitialized allocation alignment");
        new Span<byte>(uninitialized, 31).Fill(0x91);
        table.CxPlatFree(table.Context, uninitialized, Tag);
        table.CxPlatFree(table.Context, null, Tag);
        Check(owner.OutstandingPlatformAllocations == 0 && owner.OutstandingResources == 0,
            "All host ownership drained");
        Console.WriteLine($"PASS EDG platform memory: {checks} checks");
    }
}
