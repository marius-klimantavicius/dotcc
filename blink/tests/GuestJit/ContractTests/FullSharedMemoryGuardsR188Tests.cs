using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using Shared188Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    readonly record struct SharedMemoryRow188(string Name, byte[] Bytes, FullMemoryAccess Rights, bool Atomic = false);
    static SharedMemoryRow188[] SharedMemoryRows188() => [
        new("read", [0x48,0x8b,0x03], FullMemoryAccess.Read),
        new("write", [0x48,0x89,0x0b], FullMemoryAccess.Write),
        new("read-write", [0x48,0x01,0x0b], FullMemoryAccess.Read | FullMemoryAccess.Write),
        new("atomic-fast", [0xf0,0x48,0x01,0x0b], FullMemoryAccess.Read | FullMemoryAccess.Write, true)
    ];

    // ONE parent-wired family entry. SOURCE_AUTHORED_UNRUN: synthetic encodings
    // for both ABIs, native invocation only on the actual host ABI. No timing.
    static void CheckFullSharedMemoryGuardsR188(byte* machine)
    {
        Check(!fullOwnerScaffold.IsValid && FullBlockFrame.ByteSize == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007, "R188 unchanged frame/ABI and isolated fixture");
        SharedMemoryEncoding188();
        foreach (bool owner in new[] { false, true })
        {
            using var slab = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = owner ? addresses : default;
            try
            {
                SharedMemoryNative188(machine);
                SharedMemoryTicketsAndPolls188(machine);
                // R9 carries independently staged two-access state across TLB
                // lookups. Exercise these existing alias/cross-page witnesses in
                // both modes, in the same family, without changing their oracle.
                CheckFullStackMemoryComputed(machine);
                CheckFullStackMemoryTlb(machine);
                ScalarAtomicTable(machine);
                ScalarAtomicInterference(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
        Cache181RefillTable();
        Cache181AuthorityDenials();
        SharedMemoryController188();
    }

    static JitInstruction SharedMemoryInstruction188(SharedMemoryRow188 row, ulong ip = Ip)
    {
        if (row.Atomic) return AtomicClassify(row.Bytes) with { Ip = ip };
        Check(FullMemoryLowering.TryClassify(ip, row.Bytes, out var i, out _), "R188 strict classified memory row");
        return i;
    }

    static nint SharedMemoryTarget188(SharedMemoryRow188 row, FullSharedScaffold s) => row.Atomic ? s.MemoryAtomicFast :
        row.Rights == FullMemoryAccess.Read ? s.MemoryRead : row.Rights == FullMemoryAccess.Write ? s.MemoryWrite : s.MemoryReadWrite;

    static void SharedMemoryEncoding188()
    {
        byte[] slab = new byte[FullBlockEmitter.ScaffoldCapacity + 16];
        byte[] storage = new byte[FullBlockEmitter.CodeCapacity + 16], inline = new byte[FullBlockEmitter.CodeCapacity];
        byte[] fold = [0x48,0xc1,0xe8,17,0x83,0xe0,0xfe,0x31,0xc1];
        foreach (bool windows in new[] { false, true })
        {
            slab.AsSpan().Fill(0xa5);
            int commonLength = FullBlockEmitter.EmitScaffold(slab.AsSpan(8, FullBlockEmitter.ScaffoldCapacity), out var layout, windows);
            Check(FullBlockEmitter.ScaffoldCapacity == 4096 && commonLength <= 4096 &&
                TranslationCodeRegions.RoundPage(commonLength) == 4096 && slab[7] == 0xa5 && slab[^8] == 0xa5,
                "R188 four guards and scaffold stay within one charged RX page on both ABIs");
            var common = slab.AsSpan(8, commonLength);
            int[] offsets = [layout.MemoryReadOffset, layout.MemoryWriteOffset, layout.MemoryReadWriteOffset,
                layout.MemoryAtomicFastOffset, layout.ExitsOffset];
            Check(CountFullPattern(common, fold) == 4, "R188 exactly four folded TLB guards per owner");
            for (int n = 0; n < 4; ++n)
            {
                var guard = common.Slice(offsets[n], offsets[n + 1] - offsets[n]);
                byte rights = n == 0 ? (byte)1 : n == 1 ? (byte)2 : (byte)3;
                Check(guard[..4].SequenceEqual<byte>([0xf3,0x0f,0x1e,0xfa]) &&
                    guard[^4..].SequenceEqual<byte>([0x41,0xff,0x62,72]) &&
                    CountFullPattern(guard, [0x8b,0x4a,32,0x83,0xe1,rights,0x83,0xf9,rights]) == 1 &&
                    CountFullPattern(guard, fold) == 1,
                    "R188 fixed CET guard, exact rights/fold and private-frame JMP return");
                Check(CountFullPattern(guard, [0xf7,0x42,36]) == (n == 0 ? 0 : 1) &&
                    CountFullPattern(guard, [0x83,0x7a,32,3]) == (n == 3 ? 1 : 0) &&
                    CountFullPattern(guard, [0x83,0x7a,36,0]) == (n == 3 ? 1 : 0),
                    "R188 ordinary write barriers and exact R183 RW/zero-flags atomic checks stay separate");
            }
            // All four offset validators reject a default/negative/out-of-code
            // entry; no native allocation or execution uses these fake bases.
            foreach (int bad in new[] { -1, 0, commonLength - 3, int.MaxValue })
            foreach (int n in new[] { 0, 1, 2, 3 })
            {
                var invalid = n switch {
                    0 => layout with { MemoryReadOffset = bad },
                    1 => layout with { MemoryWriteOffset = bad },
                    2 => layout with { MemoryReadWriteOffset = bad },
                    _ => layout with { MemoryAtomicFastOffset = bad }
                };
                bool rejected = false;
                try { invalid.Resolve((nint)0x100000000); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "R188 malformed memory entry cannot resolve");
            }
            foreach (long address in new long[] { 0x100000000, 0x700000000000 })
            {
                var shared = layout.Resolve((nint)address);
                foreach (var row in SharedMemoryRows188())
                foreach (int count in new[] { 1, 8, 16 })
                {
                    var first = SharedMemoryInstruction188(row);
                    var sites = Enumerable.Range(0, count).Select(n => new FullBlockInstruction(first with {
                        Ip = Ip + (uint)(n * first.Length) })).ToArray();
                    storage.AsSpan().Fill(0xa5);
                    int length = FullBlockEmitter.Emit(sites, storage.AsSpan(8, FullBlockEmitter.CodeCapacity), in shared, windows);
                    int oldLength = FullBlockEmitter.Emit(sites, inline, windows);
                    var body = storage.AsSpan(8, length);
                    int expected = count * (row.Atomic ? 1 : 2);
                    Check(length < oldLength && oldLength - length >= expected * 128 &&
                        CountFullPattern(body, fold) == 0 && storage[7] == 0xa5 && storage[^8] == 0xa5,
                        "R188 dense owner bodies remove repeated guards within code budget and span canaries");
                    int landings = 0;
                    for (int at = 0; at <= body.Length - 47; ++at)
                    {
                        if (!body.Slice(at, 6).SequenceEqual<byte>([0x48,0x89,0xc1,0x48,0x8d,0x05])) continue;
                        int landing = at + 10 + BinaryPrimitives.ReadInt32LittleEndian(body.Slice(at + 6, 4));
                        Check(landing == at + 29 &&
                            body.Slice(at + 10, 9).SequenceEqual<byte>([0x49,0x89,0x42,72,0x48,0x89,0xc8,0x48,0xba]) &&
                            BinaryPrimitives.ReadUInt64LittleEndian(body.Slice(at + 19, 8)) == (ulong)SharedMemoryTarget188(row, shared) &&
                            body.Slice(at + 27, 8).SequenceEqual<byte>([0xff,0xe2,0xf3,0x0f,0x1e,0xfa,0x48,0xb9]) &&
                            BinaryPrimitives.ReadUInt64LittleEndian(body.Slice(at + 35, 8)) == Ip &&
                            body.Slice(at + 43, 4).SequenceEqual<byte>([0x49,0x89,0x4a,72]),
                            "R188 47-byte EA-preserving absolute transfer, CET landing and compile-time root restoration");
                        ++landings;
                    }
                    Check(landings == expected, "R188 first/second-page and atomic transfers use only their fixed guard entry");
                    foreach (int bytes in new[] { 0, 1, 7, 29, 47, length - 1 })
                    {
                        storage.AsSpan().Fill(0xa5);
                        Check(!FullBlockEmitter.TryEmit(sites, storage.AsSpan(8, bytes), in shared, out int actual, windows) &&
                            actual == 0 && storage[7] == 0xa5 && storage[8 + bytes] == 0xa5,
                            "R188 short opcode/continuation/fixup spans fail unpublished with intact canaries");
                    }
                    for (int n = 0; n < 4; ++n) FullBlockEmitter.Emit(sites, storage, in shared, windows);
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    for (int n = 0; n < 16; ++n) FullBlockEmitter.Emit(sites, storage, in shared, windows);
                    Check(GC.GetAllocatedBytesForCurrentThread() == before, "R188 warmed owner memory emission allocates zero managed bytes");
                }
                var load = SharedMemoryInstruction188(SharedMemoryRows188()[0]);
                var large = Enumerable.Range(0, 512).Select(n => new FullBlockInstruction(load with { Ip = Ip + (uint)(n * load.Length) })).ToArray();
                storage.AsSpan().Fill(0xa5);
                Check(!FullBlockEmitter.TryEmit(large, storage.AsSpan(8), in shared, out int oversized, windows) && oversized == 0 &&
                    storage.AsSpan(8 + FullBlockEmitter.CodeCapacity).IndexOfAnyExcept((byte)0xa5) < 0,
                    "R188 dense 512-site overflow still respects the backend code envelope");
                int countFit = 512;
                do { countFit /= 2; } while (!FullBlockEmitter.TryEmit(large.AsSpan(0, countFit), storage, in shared, out _, windows));
                Check(countFit > 0 && countFit < 512, "R188 ordinary compiler half-prefix retry remains usable");
            }
            for (int n = 0; n < 4; ++n) FullBlockEmitter.EmitScaffold(slab, out _, windows);
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            for (int n = 0; n < 16; ++n) FullBlockEmitter.EmitScaffold(slab, out _, windows);
            Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "R188 warmed scaffold emission allocates zero managed bytes");
            foreach (int bytes in new[] { 0, 1, 7, 2048, commonLength - 1 })
            {
                slab.AsSpan().Fill(0xa5); bool rejected = false;
                try { FullBlockEmitter.EmitScaffold(slab.AsSpan(8, bytes), out _, windows); }
                catch (ArgumentException) { rejected = true; }
                Check(rejected && slab[7] == 0xa5 && slab[8 + bytes] == 0xa5, "R188 short scaffold fails without crossing its span");
            }
        }
    }

    static void SharedMemoryNative188(byte* machine)
    {
        byte* pages = (byte*)NativeMemory.AlignedAlloc(12288, 4096);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach (var row in SharedMemoryRows188())
            foreach (int offset in new[] { 64, 4095 })
            foreach (string variant in new[] { "hit", "mask0", "mask6", "mask8191", "table", "tag", "epoch", "context",
                "rights", "start", "end", "barrier", "host", "second-tag", "second-rights", "atomic-rights", "atomic-flags", "identity", "host-alignment" })
            {
                if (variant.StartsWith("second-") && (offset != 4095 || row.Atomic)) continue;
                if ((variant is "atomic-rights" or "atomic-flags" or "identity" or "host-alignment") && !row.Atomic) continue;
                new Span<byte>(pages, 12288).Fill(0xa5);
                new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
                const ulong page = 0x1f000; // fold boundary; second EA differs from staged EA
                SetFullMemoryEntry(tlb, page, pages, row.Rights);
                SetFullMemoryEntry(tlb, page + 4096, pages + 8192, row.Rights);
                int slot = FullNativeMemory.TlbIndex(page, 63), next = FullNativeMemory.TlbIndex(page + 4096, 63);
                if (variant == "tag") tlb[slot].Page += 8192;
                if (variant == "epoch") --tlb[slot].AuthorityEpoch;
                if (variant == "context") ++tlb[slot].Context;
                if (variant == "rights") tlb[slot].Permissions = row.Rights == FullMemoryAccess.Read ? FullMemoryAccess.Write : FullMemoryAccess.Read;
                if (variant == "start") tlb[slot].ValidStart = 1;
                if (variant == "end") tlb[slot].ValidEnd = 4095;
                if (variant == "barrier") tlb[slot].Flags = FullMemoryFlags.WriteBarrier;
                if (variant == "host") tlb[slot].Host = null;
                if (variant == "second-tag") tlb[next].Page += 8192;
                if (variant == "second-rights") tlb[next].Permissions = (FullMemoryAccess)0;
                if (variant == "atomic-rights") tlb[slot].Permissions |= FullMemoryAccess.Atomic;
                if (variant == "atomic-flags") tlb[slot].Flags = FullMemoryFlags.BarrierConsumed;
                if (variant == "identity") ++tlb[slot].Identity;
                if (variant == "host-alignment") { ++tlb[slot].Host; ++tlb[slot].Identity; }
                int first = Math.Min(8, 4096 - offset);
                const ulong old = 0x1122334455667788;
                AtomicWrite(pages + offset, pages + 8192, first, 0, 64, old);
                ResetFull(machine); Write64(machine, 48, page + (uint)offset); Write64(machine, 32, 5);
                var i = SharedMemoryInstruction188(row, Ip + 1);
                // A later header guard and Link read the restored BODY root,
                // which differs from this site's own IP and continuation address.
                using var code = CreateFullBlock([new(new(Ip,1,JitOperation.Nop,64,default,default)), new(i),
                    new(new(Ip + 1 + (uint)i.Length,1,JitOperation.Nop,64,default,default))]);
                var h = FullHeader(Ip, code.Entry); ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
                var f = FullFrame(machine, &h, &epoch, &issued, &completed, &stop);
                f.MemoryTlb = variant == "table" ? null : tlb;
                f.MemoryTlbMask = variant == "mask0" ? 0u : variant == "mask6" ? 6u : variant == "mask8191" ? 8191u : 63u;
                byte[] regs = new ReadOnlySpan<byte>(machine + 24, 128).ToArray(); uint flags = ((Blink.Machine*)machine)->flags;
                bool success = (variant == "hit" || variant == "barrier" && row.Rights == FullMemoryAccess.Read) && !(row.Atomic && offset == 4095);
                Check(RunFull(code, &f) == (success ? FullBlockExit.Continue : FullBlockExit.NeedMemory) &&
                    issued == (success ? 3 : 2) && completed == (success ? 3 : 1) && f.Executed == (success ? 3UL : 1UL),
                    "R188 exact native success/miss and issue/commit table: " + row.Name + "/" + variant);
                ulong value = AtomicRead(pages + offset, pages + 8192, first, 0, 64);
                if (success)
                {
                    Check(value == (row.Rights == FullMemoryAccess.Read ? old : row.Rights == FullMemoryAccess.Write ? 5UL : old + 5) &&
                        (row.Rights != FullMemoryAccess.Read || ((ulong*)(machine + 24))[0] == old) &&
                        f.NextIp == Ip + (uint)i.Length + 2 && f.MemoryResume == 0,
                        "R188 exact discontiguous cross-page bytes, result and root restoration through next poll/Link/RET");
                    if (row.Rights == FullMemoryAccess.Read) BinaryPrimitives.WriteUInt64LittleEndian(regs, old);
                    Check(new ReadOnlySpan<byte>(machine + 24, 128).SequenceEqual(regs),
                        "R188 successful guard preserves every guest register except the selected load destination");
                    uint expectedFlags = row.Rights == (FullMemoryAccess.Read | FullMemoryAccess.Write) ?
                        AtomicFamilyModel(FullAtomicOperation.Add, 64, old, 5, 0, flags).Flags : flags;
                    Check(((Blink.Machine*)machine)->flags == expectedFlags,
                        "R188 read/write flag preservation and RW/atomic selected arithmetic flags");
                }
                else Check(value == old && ((Blink.Machine*)machine)->flags == flags &&
                    new ReadOnlySpan<byte>(machine + 24, 128).SequenceEqual(regs) &&
                    f.NextIp == Ip + 1 && f.MemoryResume == 1 && f.MemoryAddress == page + (uint)offset,
                    "R188 failed guard leaks no native continuation and has no data/register/flag effect");
                Check(pages[4096] == 0xa5 && pages[8191] == 0xa5, "R188 independently mapped page canaries survive");
            }
        }
        finally { NativeMemory.AlignedFree(pages); }
    }

    static void SharedMemoryTicketsAndPolls188(byte* machine)
    {
        byte* data = (byte*)NativeMemory.AllocZeroed(16);
        try
        {
            foreach (var row in SharedMemoryRows188())
            {
                var i = SharedMemoryInstruction188(row); using var code = CreateFullBlock([new(i)]);
                foreach (string gate in new[] { "stop", "attention", "budget", "cap", "runtime", "code", "abi" })
                {
                    ResetFull(machine); Write64(machine, 48, 0x100000); Write64(machine, 32, 5); ((ulong*)data)[0] = 9;
                    ulong epoch = 11, codeEpoch = 11; long issued = 0, completed = 0; int stop = gate == "stop" ? 1 : 0;
                    var h = FullHeader(Ip, code.Entry);
                    var f = FullFrame(machine, &h, &epoch, &issued, &completed, &stop, gate == "budget" ? 0UL : 100UL, codeEpoch: &codeEpoch);
                    if (gate == "attention") ((Blink.Machine*)machine)->attention = true;
                    if (gate == "cap") { f.NativeLimit = 1; f.Executed = 1; }
                    if (gate == "runtime") ++epoch;
                    if (gate == "code") ++codeEpoch;
                    if (gate == "abi") f.Version = 0;
                    var expected = gate == "stop" ? FullBlockExit.Stop : gate == "attention" ? FullBlockExit.Attention :
                        gate == "budget" ? FullBlockExit.Budget : gate == "cap" ? FullBlockExit.Continue : FullBlockExit.Invalidated;
                    Check(RunFull(code, &f) == expected && issued == (gate == "budget" ? 1 : 0) && completed == 0 &&
                        f.MemoryResume == 0 && ((ulong*)data)[0] == 9, "R188 poll/epoch/ABI rejection precedes guard scratch use");
                }
                foreach (bool changedEpoch in new[] { false, true })
                {
                    ResetFull(machine); Write64(machine, 48, 0x100fff); Write64(machine, 32, 5); ((ulong*)data)[0] = 9;
                    ulong epoch = 11, codeEpoch = 11; long issued = 0, completed = 0; int stop = 0;
                    var h = FullHeader(Ip, code.Entry);
                    var f = FullFrame(machine, &h, &epoch, &issued, &completed, &stop, budget: 1, codeEpoch: &codeEpoch);
                    Check(RunFull(code, &f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0,
                        "R188 ordinary/atomic miss retains the exact approved issue");
                    FullNativeMemory.PublishBarrierTicket(&f, data, data + 1, 1, epoch, 188);
                    FullNativeMemory.ResumeSameIssue(&f); f.Budget = 0;
                    if (changedEpoch) { ++epoch; f.ObservedGlobalEpoch = epoch; }
                    Check(RunFull(code, &f) == (changedEpoch ? FullBlockExit.NeedMemory : FullBlockExit.Continue) &&
                        issued == 1 && completed == (changedEpoch ? 0 : 1) &&
                        (changedEpoch || f.MemoryTicket == 0 && f.MemoryFlags == FullMemoryFlags.BarrierConsumed && f.MemoryResume == 0),
                        "R188 exact one-shot crossing ticket and current epoch checks preserve exhausted-budget same-issue resume");
                    Check(((ulong*)data)[0] == (changedEpoch || row.Rights == FullMemoryAccess.Read ? 9UL : row.Rights == FullMemoryAccess.Write ? 5UL : 14UL),
                        "R188 ticket effects occur only after current epoch authentication");
                }
            }
        }
        finally { NativeMemory.Free(data); }
    }

    static void SharedMemoryController188()
    {
        foreach (var row in SharedMemoryRows188())
        {
            using var f = new Shared188Fixture(); ulong pc = Shared188Fixture.Base, data = pc + 8192;
            AtomicFamilySetup(f, pc, data); WriteControllerCode(f, pc, row.Bytes);
            var setup = f.Statistics; int returns = 0;
            Check(setup.SharedScaffoldReservedBytes == 4096, "R188 actual owner still reserves one RX page");
            f.NativeReturned = p => {
                var frame = (FullBlockFrame*)p;
                if (frame->NativeMemoryComputations == 0) return;
                Check(frame->NextIp == f.Machine()->ip && f.Jit.Store.ActiveNativeEpochs > 0 &&
                    !f.Jit.Store.TryFlush(out _), "R188 real owner epoch holds body and scaffold through RET before managed consumers");
                if (row.Atomic) lock (f.Jit.Authority.Gate)
                    Check(f.Jit.Authority.HasScalarAtomicWriteAuthority(f.Physical(data), frame->ObservedGlobalEpoch),
                        "R188 real scalar fast admission still owns physical write authority through RET");
                ++returns;
            };
            long assists = 0;
            for (int n = 0; n < 2; ++n)
            {
                AtomicFamilySeed(f, data + 64); Write64((byte*)f.Machine(), 32, 5);
                Check(f.Step(pc, signalActor: true), "R188 ordinary controller owns classification, refill and native invocation");
                if (n == 0) assists = f.Statistics.NativeMemoryAssists;
                else Check(f.Statistics.NativeMemoryAssists == assists, "R188 warm real mapping requires no additional refill");
            }
            Check(returns == 2 && f.Completed == 2 && f.Issued == 2 &&
                f.Statistics.SharedScaffoldEmittedBytes == setup.SharedScaffoldEmittedBytes,
                "R188 real warm entries reuse the single shared guard slab");
            f.NativeReturned = null;
            Check(f.Jit.Store.TryFlush(out _) && f.Statistics.SharedScaffoldReservedBytes == 4096,
                "R188 quiescent body flush retains shared guard lifetime and reservation");
            AtomicFamilySeed(f, data + 64);
            Check(f.Step(pc, signalActor: true) && f.Completed == 3 &&
                f.Statistics.SharedScaffoldEmittedBytes == setup.SharedScaffoldEmittedBytes,
                "R188 recompiled body after flush reuses the same guard slab");
            AtomicFamilyAssertClean(f);
        }
    }
}
