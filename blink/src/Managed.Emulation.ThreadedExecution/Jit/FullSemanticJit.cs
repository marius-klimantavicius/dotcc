using System.Diagnostics;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Conservative compiled tier for every selected semantic operation, including
// mutable/aliased code. It retains values, never guest code/data pointers. Each
// entry is byte-validated with execute access under a short allocation read pin
// and the anonymous-page retirement gate, without waiting on mmap_lock.
// Native leaves return a typed operation continuation; semantic work runs only
// after both the native frame and all code/cache leases have ended.
internal sealed unsafe class FullSemanticJit : IDisposable
{
    private enum Exit : uint { None, NeedSemanticHelper }
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct ContinuationFrame
    {
        internal const uint AbiVersion = 1;
        [FieldOffset(0)] internal uint Version;
        [FieldOffset(4)] internal uint Size;
        [FieldOffset(8)] internal ulong Token;
        [FieldOffset(16)] internal Exit Reason;
    }
    internal readonly struct Prepared
    {
        internal readonly Blink.XedDecodedInst Decoded;
        internal readonly delegate*<Blink, Blink.Machine*, ulong, long, ulong, void> Helper;
        internal readonly int Operation, Error;
        internal Prepared(Blink.XedDecodedInst decoded,
            delegate*<Blink, Blink.Machine*, ulong, long, ulong, void> helper)
        { Decoded = decoded; Helper = helper; Operation = (int)((decoded.op.rde >> 40) & 2047); Error = 0; }
        internal Prepared(int error) { Decoded = default; Helper = null; Operation = 0; Error = error; }
    }

    // Stable PC/mode directory and preallocated value slots. Directory hash
    // collisions only add bounded probes; they never evict a translation.
    // Every resident key owns one immutable native continuation slot. Rebinding
    // patches metadata under gate after native return, never executable bytes.
    internal sealed class PlanCache : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            internal ulong Pc, Used;
            internal ushort Mode;
            internal bool Protected;
            internal int Previous, Next;
            internal Prepared Plan;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Cell { internal ulong Pc; internal int SlotPlusOne; internal ushort Mode; }
        private Entry[] entries;
        private Cell[] directory;
        private readonly int capacity, mask;
        private int resident, protectedCount;
        private int probationFirst = -1, probationLast = -1, protectedFirst = -1, protectedLast = -1;
        internal int Capacity => capacity;
        internal int Resident => resident;
        internal int PeakResident { get; private set; }
        internal long Hits { get; private set; }
        internal long CapacityEvictions { get; private set; }
        internal long LookupProbes { get; private set; }
        internal long LookupCollisions { get; private set; }
        internal long MetadataBytes { get; }
        internal static int DirectoryLength(int count)
        { int n = 8; while (n < count * 2) n *= 2; return n; }
        internal static long Charge(int count) => checked((long)count * sizeof(Entry) + (long)DirectoryLength(count) * sizeof(Cell));
        internal PlanCache(int count)
        {
            if (count is < 4 or > 32768) throw new ArgumentOutOfRangeException(nameof(count));
            capacity = count; entries = new Entry[count]; directory = new Cell[DirectoryLength(count)];
            mask = directory.Length - 1; MetadataBytes = Charge(count);
        }
        internal static ulong Hash(ulong pc, ushort mode)
        {
            ulong h = pc ^ ((ulong)mode * 0x9e3779b97f4a7c15UL);
            h ^= h >> 30; h *= 0xbf58476d1ce4e5b9UL;
            h ^= h >> 27; h *= 0x94d049bb133111ebUL;
            return h ^ (h >> 31);
        }
        internal bool TryFind(ulong pc, ushort mode, out int slot)
        {
            int at = (int)(Hash(pc, mode) & (uint)mask);
            for (int probes = 0; probes < directory.Length; ++probes, at = (at + 1) & mask)
            {
                ++LookupProbes;
                ref var cell = ref directory[at];
                if (cell.SlotPlusOne == 0) { slot = -1; return false; }
                if (cell.Pc == pc && cell.Mode == mode) { slot = cell.SlotPlusOne - 1; return true; }
                ++LookupCollisions;
            }
            throw new InvalidOperationException("Full-JIT plan directory has no empty cell.");
        }
        internal Prepared Get(int slot) => entries[slot].Plan;
        private void Unlink(int slot)
        {
            ref var entry = ref entries[slot];
            if (entry.Previous >= 0) entries[entry.Previous].Next = entry.Next;
            else if (entry.Protected) protectedFirst = entry.Next; else probationFirst = entry.Next;
            if (entry.Next >= 0) entries[entry.Next].Previous = entry.Previous;
            else if (entry.Protected) protectedLast = entry.Previous; else probationLast = entry.Previous;
            entry.Previous = entry.Next = -1;
        }
        private void Append(int slot)
        {
            ref var entry = ref entries[slot];
            int last = entry.Protected ? protectedLast : probationLast;
            entry.Previous = last; entry.Next = -1;
            if (last >= 0) entries[last].Next = slot;
            else if (entry.Protected) protectedFirst = slot; else probationFirst = slot;
            if (entry.Protected) protectedLast = slot; else probationLast = slot;
        }
        internal void Touch(int slot, ulong clock)
        {
            Unlink(slot);
            if (!entries[slot].Protected) { entries[slot].Protected = true; ++protectedCount; }
            entries[slot].Used = clock; Append(slot); ++Hits;
            // Repeated keys occupy a protected segment. One-use scans enter
            // probation and cannot flush the whole hot working set.
            if (protectedCount > capacity * 3 / 4)
            {
                int demoted = protectedFirst; Unlink(demoted);
                entries[demoted].Protected = false; --protectedCount; Append(demoted);
            }
        }
        private void Remove(ulong pc, ushort mode, int slot)
        {
            int hole = (int)(Hash(pc, mode) & (uint)mask);
            for (int probes = 0; probes < directory.Length; ++probes, hole = (hole + 1) & mask)
            {
                ref var cell = ref directory[hole];
                if (cell.SlotPlusOne == 0) throw new InvalidOperationException("Full-JIT resident key missing.");
                if (cell.Pc == pc && cell.Mode == mode && cell.SlotPlusOne == slot + 1) break;
                if (probes == directory.Length - 1) throw new InvalidOperationException("Full-JIT directory removal bound.");
            }
            // Backward shift closes the hole, including wraparound clusters;
            // tombstones cannot accumulate under repeated slot reuse.
            for (int next = (hole + 1) & mask, probes = 0; probes < directory.Length; ++probes, next = (next + 1) & mask)
            {
                var cell = directory[next];
                if (cell.SlotPlusOne == 0) { directory[hole] = default; return; }
                int home = (int)(Hash(cell.Pc, cell.Mode) & (uint)mask);
                if (((next - home) & mask) >= ((next - hole) & mask))
                { directory[hole] = cell; hole = next; }
            }
            throw new InvalidOperationException("Full-JIT directory shift bound.");
        }
        private void Insert(ulong pc, ushort mode, int slot)
        {
            int at = (int)(Hash(pc, mode) & (uint)mask);
            for (int probes = 0; probes < directory.Length; ++probes, at = (at + 1) & mask)
                if (directory[at].SlotPlusOne == 0)
                { directory[at] = new() { Pc = pc, Mode = mode, SlotPlusOne = slot + 1 }; return; }
            throw new InvalidOperationException("Full-JIT directory insertion bound.");
        }
        internal int Bind(ulong pc, ushort mode, Prepared plan, ulong clock, int existingSlot)
        {
            int slot = existingSlot;
            if (slot < 0)
            {
                if (resident < capacity) { slot = resident++; PeakResident = Math.Max(PeakResident, resident); }
                else
                {
                    // Protected entries must age when execution changes phase.
                    // Otherwise a stale protected set can permanently confine
                    // a new fitting working set to the smaller probation list.
                    // Examine at most one LRU entry per capacity admission; hot
                    // entries refreshed within four cache-sized access windows
                    // keep their scan protection. Clock subtraction wraps.
                    if (protectedFirst >= 0 && unchecked(clock - entries[protectedFirst].Used) > (ulong)capacity * 4)
                    {
                        int aged = protectedFirst; Unlink(aged);
                        entries[aged].Protected = false; --protectedCount; Append(aged);
                    }
                    slot = probationFirst >= 0 ? probationFirst : protectedFirst;
                    if (slot < 0) throw new InvalidOperationException("Full-JIT replacement list is empty.");
                    Unlink(slot); if (entries[slot].Protected) --protectedCount;
                    Remove(entries[slot].Pc, entries[slot].Mode, slot); ++CapacityEvictions;
                }
                Insert(pc, mode, slot);
            }
            else if ((uint)slot >= (uint)resident || entries[slot].Pc != pc || entries[slot].Mode != mode)
                throw new InvalidOperationException("Full-JIT metadata rebind key mismatch.");
            else
            {
                Unlink(slot); if (entries[slot].Protected) --protectedCount;
            }
            entries[slot] = new() { Pc = pc, Mode = mode, Used = clock, Previous = -1, Next = -1, Plan = plan };
            Append(slot);
            return slot;
        }
        public void Dispose()
        { entries = Array.Empty<Entry>(); directory = Array.Empty<Cell>(); resident = protectedCount = 0; probationFirst = probationLast = protectedFirst = protectedLast = -1; }
    }

    private readonly object gate = new();
    private readonly PlanCache plans;
    private readonly nint[] retiredPages;
    private int readers, retiredCount, peakRetired;
    private long semanticBorrows, walkBorrows, deferredRecycles;
    private readonly long limit;
    private readonly NativeCodeAllocation continuations;
    private readonly long[] helperCalls = new long[2048], helperCompleted = new long[2048];
    private long bytes, peak, misses, validationMisses, compilations, entriesCalled, helpers, completed;
    private long compileAllocated;
    private double compileMs;
    private ulong clock;
    private bool disposed;

    internal FullSemanticJit(GuestJitOptions options, int workers, ulong memoryLimitBytes)
    {
        string? unavailable = NativeCodePlatform.UnavailableReason(options.AllowUnverifiedWindows, requireNativeFlags: false);
        if (unavailable != null) throw new PlatformNotSupportedException("Full-JIT: " + unavailable);
        // Querying policy alone cannot detect e.g. Linux seccomp denying
        // executable publication. Prove W^X allocation before guest entry.
        if (!NativeCodeAllocation.TryCreate([0xf3, 0x0f, 0x1e, 0xfa, 0xc3], out var probe, out string reason, requireNativeFlags: false))
            throw new PlatformNotSupportedException("Full-JIT native publication: " + reason);
        probe!.Dispose();
        limit = options.CacheLimitBytes;
        // Charge every retained slab/directory byte before allocating. Fixed
        // overhead includes array/object headers and bounded worker metadata.
        int pageCount = checked((int)(memoryLimitBytes / 4096));
        long fixedCharge = 4096 + 2 * 2048 * sizeof(long) + workers * 128L + (long)pageCount * sizeof(nint);
        int count = 32768;
        long codeCharge;
        while (true)
        {
            codeCharge = (count * 32L + Environment.SystemPageSize - 1) / Environment.SystemPageSize * Environment.SystemPageSize;
            if (fixedCharge + PlanCache.Charge(count) + codeCharge <= limit) break;
            count -= count > 128 ? 128 : 4;
            if (count < 4) throw new InvalidOperationException("Full-JIT cache cannot cover bounded owner/worker metadata and compiled slots.");
        }
        retiredPages = new nint[pageCount];
        plans = new PlanCache(count);
        // Immutable reusable continuation slots: a translation binds a copied
        // decoded instruction and selected typed operation to its slot. Reusing
        // a retired slot never changes executable bytes or publishes RWX pages.
        // Every entry still crosses a real native typed-continuation boundary;
        // none of that semantic helper work is counted as inline execution.
        byte[] code = new byte[count * 32];
        for (int slot = 0; slot < count; ++slot)
        {
            var leaf = code.AsSpan(slot * 32, 32);
            leaf.Fill(0xcc);
            leaf[0] = 0xf3; leaf[1] = 0x0f; leaf[2] = 0x1e; leaf[3] = 0xfa;
            leaf[4] = 0x48; leaf[5] = 0xb8;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(leaf[6..14], (ulong)slot + 1);
            leaf[14] = 0x48; leaf[15] = 0x89; leaf[16] = OperatingSystem.IsWindows() ? (byte)0x41 : (byte)0x47; leaf[17] = 8;
            leaf[18] = 0xc7; leaf[19] = OperatingSystem.IsWindows() ? (byte)0x41 : (byte)0x47; leaf[20] = 16;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(leaf[21..25], (uint)Exit.NeedSemanticHelper);
            leaf[25] = 0xc3;
        }
        bytes = peak = fixedCharge + plans.MetadataBytes + codeCharge;
        if (!NativeCodeAllocation.TryCreate(code, out var allocation, out string publicationReason, requireNativeFlags: false))
            throw new PlatformNotSupportedException("Full-JIT native publication: " + publicationReason);
        continuations = allocation!;
    }

    // Caller has reserved exactly this instruction. Fetch faults therefore
    // retain the interpreter's issued-but-not-completed accounting. No guest
    // exception is delivered while either lock is held.
    internal Prepared Prepare(Blink program, Blink.Machine* machine)
    {
        Blink.XedDecodedInst decoded = default;
        int error = 0;
        ulong pc = (ulong)program.GetPc(machine);
        ushort mode = (ushort)(machine->mode.omode | machine->mode.genmode << 8);
        if (program.BlinkHostMemoryInstructionReadBegin() != 0)
            throw new InvalidOperationException("Full-JIT code read lease failed.");
        try
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                // Bypass stale TLB/codehost shortcuts. The read pin protects
                // physical allocations, and this gate excludes clearing and
                // recycling anonymous data/page-table pages through the pool.
                program.ResetTlb(machine);
                ++clock;
                int slot;
                if (plans.TryFind(pc, mode, out slot))
                {
                    var saved = plans.Get(slot);
                    var savedCode = saved.Decoded;
                    if (Matches(program, machine, pc, &savedCode))
                    { plans.Touch(slot, clock); return Enter(saved, slot); }
                    ++validationMisses;
                }
                ++misses;
                long allocated = GC.GetAllocatedBytesForCurrentThread();
                long start = Stopwatch.GetTimestamp();
                // Translation-time fetch/decode only. LoadInstruction2 returns
                // faults as data; HaltMachine runs after releasing both leases.
                program.ResetInstructionCache(machine);
                error = program.LoadInstruction2(machine, pc);
                if (error == 0)
                {
                    decoded = *machine->xedd;
                    var helper = program.BlinkFullJitSelect(decoded.op.rde, decoded.op.disp, decoded.op.uimm0);
                    if (helper == null || decoded.length is < 1 or > 15)
                        throw new InvalidOperationException("Selected full-JIT semantic binding is incomplete.");
                    // Bind this translation to an immutable authenticated
                    // native continuation slot and an operation-specific helper.
                    var replacement = new Prepared(decoded, helper);
                    slot = plans.Bind(pc, mode, replacement, clock, slot);
                    ++compilations;
                    compileAllocated += GC.GetAllocatedBytesForCurrentThread() - allocated;
                    compileMs += Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    return Enter(replacement, slot);
                }
            }
        }
        finally
        {
            if (program.BlinkHostMemoryInstructionReadEnd() != 0)
                throw new InvalidOperationException("Full-JIT code read lease did not release.");
        }
        return new Prepared(error); // Deliver only after the caller restores fetch-protection state.
    }

    private Prepared Enter(Prepared plan, int slot)
    {
        ++entriesCalled;
        ContinuationFrame frame = new() { Version = ContinuationFrame.AbiVersion, Size = (uint)sizeof(ContinuationFrame) };
        ((delegate* unmanaged<ContinuationFrame*, void>)(continuations.Entry + slot * 32))(&frame);
        if (frame.Version != ContinuationFrame.AbiVersion || frame.Size != sizeof(ContinuationFrame) ||
            frame.Token != (ulong)slot + 1 || frame.Reason != Exit.NeedSemanticHelper)
            throw new InvalidOperationException("Full-JIT continuation authentication failed.");
        return plan;
    }

    internal void BeginRead(Blink program, bool semantic)
    {
        // This is passive reclamation protection, not a writer-excluding lease.
        // Writers detach and queue without waiting for a semantic helper.
        if (program.BlinkHostMemoryInstructionReadBegin() != 0)
            throw new InvalidOperationException("Full-JIT data read protection failed.");
        try
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                readers = checked(readers + 1);
                if (semantic) ++semanticBorrows; else ++walkBorrows;
            }
        }
        catch { program.BlinkHostMemoryInstructionReadEnd(); throw; }
    }

    internal void EndRead(Blink program)
    {
        try
        {
            lock (gate)
            {
                if (readers <= 0) throw new InvalidOperationException("Full-JIT data read balance.");
                if (--readers == 0)
                    while (retiredCount != 0)
                    {
                        nint item = retiredPages[--retiredCount]; retiredPages[retiredCount] = 0;
                        // The authored raw primitive has no System dereference;
                        // its allocator belongs to this bound program instance.
                        program.BlinkGuestRecycleAnonymousPage(null, (byte*)(item & ~(nint)1), (int)(item & 1));
                    }
            }
        }
        finally
        {
            // No semantic gate -> physical-owner/IO lock edge.
            if (program.BlinkHostMemoryInstructionReadEnd() != 0)
                throw new InvalidOperationException("Full-JIT data read protection did not release.");
        }
    }

    internal void RecycleAnonymousPage(Blink program, Blink.System* system, byte* page, bool clear)
    {
        lock (gate)
        {
            if (page == null || ((nuint)page & 4095) != 0)
                throw new InvalidOperationException("Full-JIT anonymous retirement address invariant.");
            if (readers == 0) program.BlinkGuestRecycleAnonymousPage(system, page, clear ? 1 : 0);
            else
            {
                if (retiredCount == retiredPages.Length)
                    throw new InvalidOperationException("Full-JIT anonymous retirement resource invariant.");
                retiredPages[retiredCount++] = (nint)page | (clear ? (nint)1 : 0);
                peakRetired = Math.Max(peakRetired, retiredCount); ++deferredRecycles;
            }
        }
    }

    private static bool Matches(Blink program, Blink.Machine* machine, ulong pc, Blink.XedDecodedInst* decoded)
    {
        var nofault = machine->nofault;
        var segvcode = machine->segvcode;
        machine->nofault = true;
        try
        {
            int first = Math.Min(decoded->length, 4096 - (int)(pc & 4095));
            byte* source = program.LookupAddress2(machine, (long)pc, (ulong)Blink.PAGE_XD, 0);
            if (source == null || !new ReadOnlySpan<byte>(source, first).SequenceEqual(new ReadOnlySpan<byte>(decoded->bytes, first)))
                return false;
            int second = decoded->length - first;
            if (second == 0) return true;
            source = program.LookupAddress2(machine, unchecked((long)(pc + (uint)first)), (ulong)Blink.PAGE_XD, 0);
            return source != null && new ReadOnlySpan<byte>(source, second).SequenceEqual(new ReadOnlySpan<byte>(decoded->bytes + first, second));
        }
        finally { machine->nofault = nofault; machine->segvcode = segvcode; }
    }

    internal void BeginHelper(int operation) { Interlocked.Increment(ref helpers); Interlocked.Increment(ref helperCalls[operation]); }
    internal void CompleteHelper(int operation) { Interlocked.Increment(ref completed); Interlocked.Increment(ref helperCompleted[operation]); }

    internal GuestJitStatistics Snapshot(GuestJitStatistics inline)
    {
        lock (gate)
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);
            for (int i = 0; i < helperCalls.Length; ++i)
                if (Volatile.Read(ref helperCalls[i]) != 0)
                { counts[$"{i:x3}/calls"] = Volatile.Read(ref helperCalls[i]); counts[$"{i:x3}/completed"] = Volatile.Read(ref helperCompleted[i]); }
            return inline with { Mode = "Full", Capability = "Available", InlineNativeInstructions = inline.GuestInstructions,
                SemanticDataBorrows = semanticBorrows, PageTableReadBorrows = walkBorrows,
                DeferredAnonymousRecycles = deferredRecycles, PeakDeferredAnonymousPages = peakRetired,
                ActiveDataBorrows = readers, DeferredAnonymousPages = retiredCount,
                SemanticHelperInstructions = Volatile.Read(ref completed), SemanticHelperCalls = Volatile.Read(ref helpers),
                InterpreterAttempts = inline.InterpreterAttempts, InterpreterInstructions = 0,
                TranslationMisses = misses, CodeValidationMisses = validationMisses,
                SemanticPlanBindings = compilations, SemanticCacheHits = plans.Hits,
                SemanticCapacityEvictions = plans.CapacityEvictions, SemanticCacheCapacity = plans.Capacity,
                SemanticCacheEntries = plans.Resident, SemanticPeakCacheEntries = plans.PeakResident,
                SemanticDirectoryProbes = plans.LookupProbes, SemanticDirectoryCollisions = plans.LookupCollisions,
                CompiledBlocks = inline.CompiledBlocks + compilations, NativeEntries = inline.NativeEntries + entriesCalled,
                NativeDispatches = inline.NativeDispatches + entriesCalled,
                CacheBytes = inline.CacheBytes + bytes, PeakCacheBytes = inline.PeakCacheBytes + peak,
                CompileAllocatedBytes = inline.CompileAllocatedBytes + compileAllocated,
                CompileMilliseconds = inline.CompileMilliseconds + compileMs,
                Evictions = inline.Evictions + plans.CapacityEvictions, SemanticHelpers = counts };
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (readers != 0 || retiredCount != 0) throw new InvalidOperationException("Disposing active Full-JIT data borrows.");
            disposed = true;
            plans.Dispose();
            continuations.Dispose();
            bytes = 0;
        }
    }
}
