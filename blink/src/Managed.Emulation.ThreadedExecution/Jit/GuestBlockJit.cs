using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Owner-scoped cache: four-way entries and two-way admission history are
// globally bounded. The gate spans each native call and allocation retirement.
internal sealed unsafe class GuestBlockJit : IDisposable, Managed.Emulation.Host.IHostGuestCodeRevocation
{
    private const int MaximumInstructions = 32, MaximumBytes = MaximumInstructions * 15, Ways = 4;
    private const long OwnerCharge = 4096, EntryCharge = 384;
    private static long nextOwnerIdentity;
    private readonly long ownerIdentity = Interlocked.Increment(ref nextOwnerIdentity);
    private ulong entryGeneration;
    private long entryHintHits, staleEntryHints, structuralNegativeHits, hotPositiveEvictions;
    private long compileAllocatedBytes, emittedCodeBytes, budgetEvictions, negativeBudgetEvictions, negativePromotions;
    private long nativeDispatches, continuations;
    private readonly object gate = new();
    private readonly ThreadedGuestExecution owner;
    private readonly Entry?[] entries;
    private readonly Entry?[] negatives;
    private readonly int hotBlockThreshold;
    private readonly long limit;
    internal int NegativeHintSlots { get; }
    private readonly Dictionary<string, long> fallbacks = new(StringComparer.Ordinal);
    private GCHandle ownerRoot;
    private string capability;
    private bool disposed, workersCreated;
    private volatile bool active;
    private GuestStaticCode? staticCode;
    private readonly GuestDataLeaseCache dataLeases = new();
    private long firstChildInstructions = -1, nativeAtFirstChild, retiredStaticPages, retiredRevokedPages;
    private long bytes, peakBytes, compiledBlocks, nativeEntries, nativeInstructions, nativeMemoryInstructions, memoryAtFirstChild, hits, validations;
    private double compileMilliseconds;
    private ulong visits;
    private int compileCredits = 8;
    private long compileAttempts, entryCollisions, evictions, negativeHits, compileBudgetDenials, shortNativeExits;
    private readonly long[] fragmentLengths = new long[MaximumInstructions + 1];

    private sealed class Entry
    {
        internal required ulong Ip;
        internal required byte[] Bytes;
        internal required long Charge;
        internal NativeCodeAllocation? Allocation;
        internal string? Fallback;
        internal ulong RetryAt, Used;
        internal int NegativeDelay = 64, ShortExitStreak;
        internal ulong Generation;
        internal bool StableNegative;
        internal uint UsefulCredit;
        internal long Calls, Instructions, Misses;
        internal JitAddressPlan AddressPlan = JitAddressPlan.Empty;
        internal byte[] MemoryPrefixes = Array.Empty<byte>();
    }

    internal GuestBlockJit(ThreadedGuestExecution owner, GuestJitOptions options)
    {
        this.owner = owner; limit = options.CacheLimitBytes;
        int count = 1;
        while (count < 1024 && count * 2 <= limit / 512) count *= 2;
        entries = new Entry?[count];
        negatives = new Entry?[count];
        hotBlockThreshold = options.HotBlockThreshold;
        int hintLimit = (int)Math.Max(2, Math.Min(1024, limit / (4L * owner.JitWorkerCapacity * sizeof(JitNegativeHint))));
        int hintSlots = 2;
        while (hintSlots * 2 <= hintLimit) hintSlots *= 2;
        NegativeHintSlots = hintSlots;
        // Includes table storage, cache object, locks, owner handle, poll cell,
        // bounded statistics keys and their dictionary overhead. EntryCharge
        // includes Entry/allocation wrappers, array headers and alignment.
        // Precharge all possible worker policy fields, NativePoll objects,
        // cells and stop-list references. Reaping can only reduce actual use.
        bytes = peakBytes = OwnerCharge + 512 + GuestDataLeaseCache.Charge + (count + negatives.Length) * sizeof(nint) + owner.JitWorkerCapacity * (1152L + hintSlots * sizeof(JitNegativeHint));
        capability = NativeCodePlatform.UnavailableReason(options.AllowUnverifiedWindows) ?? "Available";
        if (bytes > limit)
        {
            // A small cache with a very large worker ceiling cannot promise
            // all poll/hint storage. Keep guest execution on the interpreter.
            NegativeHintSlots = 0;
            bytes = peakBytes = OwnerCharge + 512 + GuestDataLeaseCache.Charge + (count + negatives.Length) * sizeof(nint) + owner.JitWorkerCapacity * 128L;
            if (capability == "Available") capability = "Cache limit cannot cover worker JIT metadata";
        }
        active = capability == "Available";
        ownerRoot = GCHandle.Alloc(owner);
    }

    internal bool CanEnter(bool tracing)
    {
        lock (gate)
        {
            if (disposed) throw new ObjectDisposedException(nameof(GuestBlockJit));
            string? reason = workersCreated && staticCode == null ? "Multiworker" : tracing ? "TraceEnabled" :
                capability != "Available" ? "Capability" : null;
            if (reason == null) return true;
            active = false; Fallback(reason); return false;
        }
    }

    internal void DisableForWorkers()
    {
        lock (gate)
        {
            if (workersCreated) return;
            workersCreated = true;
            firstChildInstructions = (long)owner.InstructionsCompleted;
            nativeAtFirstChild = nativeInstructions;
            memoryAtFirstChild = nativeMemoryInstructions;
            if (staticCode == null) { active = false; Fallback("Multiworker"); }
            else Fallback("StaticCodeOnly");
        }
    }

    internal void EnrollStaticCode(Blink program, Blink.Machine* machine)
    {
        lock (gate)
        {
            if (workersCreated || staticCode != null) throw new InvalidOperationException("Static enrollment is initial-only");
            if (!active) return;
            staticCode = GuestStaticCode.Enroll(program, machine, limit - bytes);
            if (staticCode != null) { bytes += staticCode.Charge; peakBytes = Math.Max(peakBytes, bytes); }
        }
    }

    void Managed.Emulation.Host.IHostGuestCodeRevocation.Revoke(long address, long size)
    {
        var code = staticCode;
        if (code == null || size <= 0 || !code.MayOverlap((ulong)address, (ulong)size)) return;
        // No mmap/registry/page lock is acquired here. Mutators can hold their
        // mmap lock: a native call only reads precommitted enrolled backing.
        // Return after draining and permanent revocation, before mutation or
        // pointer escape. Never carry this gate into a syscall or helper.
        lock (gate) code.Revoke((ulong)address, (ulong)size);
    }

    void Managed.Emulation.Host.IHostGuestCodeRevocation.RevokeMapping(long address, long size)
    {
        if (size <= 0) return;
        lock (gate)
        {
            dataLeases.Invalidate();
            staticCode?.Revoke((ulong)address, (ulong)size);
        }
    }

    internal bool TryCopyStaticCode(Blink.Machine* machine, ulong ip, Span<byte> destination) =>
        staticCode?.Validate(owner.JitProgram, machine, ip) == true && staticCode.Copy(ip, destination);
    internal bool HasWorkers => workersCreated;

    internal bool IsActive => active;

    internal GuestJitStatistics Snapshot()
    {
        lock (gate) return new()
        {
            Capability = capability, CompiledBlocks = compiledBlocks, NativeEntries = nativeEntries,
            NativeDispatches = nativeDispatches, Continuations = continuations,
            GuestInstructions = nativeInstructions, NativeMemoryInstructions = nativeMemoryInstructions, NativeMemoryInstructionsAtFirstChild = memoryAtFirstChild, CacheHits = hits, CacheBytes = bytes,
            PeakCacheBytes = peakBytes, ValidationFailures = validations,
            FirstChildInstructions = firstChildInstructions, NativeInstructionsAtFirstChild = nativeAtFirstChild,
            StaticCodePages = staticCode?.PageCount ?? retiredStaticPages, RevokedStaticCodePages = staticCode?.RevokedPages ?? retiredRevokedPages,
            InterpreterInstructions = (long)owner.InstructionsCompleted - nativeInstructions,
            CompileMilliseconds = compileMilliseconds,
            BlockEntries = checked((long)visits), CompileAttempts = compileAttempts,
            EntryCollisions = entryCollisions, Evictions = evictions,
            NegativeCacheHits = negativeHits, CompileBudgetDenials = compileBudgetDenials, ShortNativeExits = shortNativeExits,
            FragmentLengths = (long[])fragmentLengths.Clone(),
            EntryHintHits = entryHintHits, StaleEntryHints = staleEntryHints,
            StructuralNegativeHits = structuralNegativeHits, HotPositiveEvictions = hotPositiveEvictions,
            CompileAllocatedBytes = compileAllocatedBytes, EmittedCodeBytes = emittedCodeBytes, BudgetEvictions = budgetEvictions,
            NegativeBudgetEvictions = negativeBudgetEvictions, NegativePromotions = negativePromotions,
            Fallbacks = new Dictionary<string, long>(fallbacks, StringComparer.Ordinal)
        };
    }

    private Entry? ValidateHint(in JitEntryHint hint, ulong ip)
    {
        if (hint.Owner != ownerIdentity || hint.Ip != ip || (uint)hint.Slot >= (uint)entries.Length) return null;
        var entry = entries[hint.Slot];
        return entry?.Allocation != null && entry.Ip == ip && entry.Generation == hint.Generation &&
            hint.HasData == (entry.AddressPlan.HasMemory) ? entry : null;
    }

    internal bool TryExecute(ThreadedGuestExecution owner, Blink.Machine* machine, JitFrame* frame,
        ref JitEntryHint hint, out int retryVisits)
    {
        // A data hint only selects lock order. Its identity is authenticated
        // again under the gate before any allocation/descriptor can be used.
        if (!owner.IsJitMachineMode(machine)) { retryVisits = 0; lock (gate) Fallback("MachineState"); return false; }
        if (!(hint.Owner == ownerIdentity && hint.Ip == machine->ip && hint.HasData))
        {
            lock (gate)
            {
                var known = ValidateHint(hint, machine->ip);
                if (known != null && !known.AddressPlan.HasMemory &&
                    staticCode?.Validate(owner.JitProgram, machine, machine->ip) == true)
                    return TryExecuteLocked(owner, machine, frame, ref hint, mappingHeld: false, out retryVisits);
                ulong ip = machine->ip;
                int first = Bucket(ip, entries.Length);
                for (int i = first; i < first + Ways; ++i)
                    if (entries[i] is { Allocation: not null } entry && entry.Ip == ip &&
                        !entry.AddressPlan.HasMemory && staticCode?.Validate(owner.JitProgram, machine, ip) == true)
                        return TryExecuteLocked(owner, machine, frame, ref hint, mappingHeld: false, out retryVisits);
                first = Bucket(ip, negatives.Length);
                for (int i = first; i < first + Ways; ++i)
                    if (negatives[i] is { } entry && entry.Ip == ip &&
                        (visits + 1 < entry.RetryAt || entry.StableNegative && staticCode?.Contains(ip) == true))
                        return TryExecuteLocked(owner, machine, frame, ref hint, mappingHeld: false, out retryVisits);
            }
        }
        int* mappingMutex = &machine->blink_host_system->mmap_lock;
        if (Blink.Libc.pthread_mutex_trylock(mappingMutex) != 0)
        { retryVisits = 4; lock (gate) Fallback("MappingLeaseBusy"); return false; }
        try { lock (gate) return TryExecuteLocked(owner, machine, frame, ref hint, mappingHeld: true, out retryVisits); }
        finally
        {
            if (Blink.Libc.pthread_mutex_unlock(mappingMutex) != 0)
                throw new InvalidOperationException("Native mapping lease did not release");
        }
    }

    private static int Bucket(ulong ip, int length) =>
        (int)((ip ^ (ip >> 12) ^ (ip >> 24)) & (ulong)(length / Ways - 1)) * Ways;

    private ulong RetentionScore(Entry entry) =>
        (ulong)entry.UsefulCredit * 4096 / (ulong)Math.Max(4096, entry.Charge) /
            (1 + (visits - entry.Used) / 64);

    // Native gate is held. Data-capable entries additionally require mmap_lock;
    // the gate-only caller above can neither compile nor execute a data entry.
    private bool TryExecuteLocked(ThreadedGuestExecution owner, Blink.Machine* machine, JitFrame* frame, ref JitEntryHint hint, bool mappingHeld, out int retryVisits)
    {
        retryVisits = 0;
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!owner.IsJitMachineMode(machine)) { Fallback("MachineState"); return false; }
            if (workersCreated && staticCode?.Validate(owner.JitProgram, machine, machine->ip) != true)
            { Fallback("NonStaticCode"); retryVisits = 64; return false; }
            ulong ip = machine->ip;
            ++visits;
            if ((visits & 63) == 0 && compileCredits < 8) ++compileCredits;
            int first = Bucket(ip, entries.Length), slot = first;
            Entry? found = ValidateHint(hint, ip);
            bool foundNegative = false, reconsideredNegative = false;
            if (found != null) { slot = hint.Slot; ++entryHintHits; }
            else
            {
                if (hint.Owner != 0 && hint.Ip == ip) ++staleEntryHints;
                for (int i = first; i < first + Ways; ++i)
                {
                    var candidate = entries[i];
                    if (candidate?.Ip == ip) { found = candidate; slot = i; break; }
                    var victim = entries[slot];
                    if (candidate == null || victim != null && RetentionScore(candidate) < RetentionScore(victim)) slot = i;
                }
                if (found == null)
                {
                    int negativeFirst = Bucket(ip, negatives.Length);
                    for (int i = negativeFirst; i < negativeFirst + Ways; ++i)
                        if (negatives[i]?.Ip == ip) { found = negatives[i]; foundNegative = true; break; }
                }
            }
            bool leasedCode = staticCode?.Contains(ip) == true;
            Span<byte> current = stackalloc byte[found?.Allocation != null && leasedCode ? 0 : MaximumBytes];
            Entry? entry = found;
            if (entry != null && entry.Ip == ip)
            {
                // Negative decisions merely choose interpretation, so retaining
                // one briefly across a code mutation cannot execute stale code.
                // Periodic revalidation lets newly supported code be admitted
                // without repeating a checked copy at every memory instruction.
                entry.Used = visits;
                if (entry.Allocation == null && (visits < entry.RetryAt ||
                    entry.StableNegative && staticCode?.Contains(ip) == true))
                { if (entry.StableNegative && staticCode?.Contains(ip) == true) ++structuralNegativeHits;
                    retryVisits = entry.NegativeDelay; ++negativeHits; Fallback(entry.Fallback!); return false; }
                if (entry.Allocation != null && leasedCode)
                {
                    // Initial private backing cannot change or retire until
                    // this gate releases. Permanent revocation prevents a
                    // different identity from reusing this positive entry.
                    ++hits;
                }
                else
                {
                    if (!owner.TryCopyJitCode(machine, ip, current[..entry.Bytes.Length], out string reason))
                    {
                        ++validations; if (foundNegative) RetireNegative(ip); else Retire(slot); Fallback(reason); return false;
                    }
                    if (current[..entry.Bytes.Length].SequenceEqual(entry.Bytes))
                    {
                        if (entry.Allocation == null)
                        {
                            // Mutable/revoked negatives may depend on bytes
                            // beyond their first-instruction fingerprint. An
                            // expired decision is freshly decoded, even if
                            // that prefix stayed equal. Immutable structural
                            // identities already returned above.
                            RetireNegative(ip); reconsideredNegative = true; entry = null;
                        }
                        else ++hits;
                    }
                    else { ++validations; if (foundNegative) { RetireNegative(ip); reconsideredNegative = true; } else Retire(slot); entry = null; }
                }
            }
            else entry = null;
            if (entry == null)
            {
                // Compiler budget is separate from worker-local visit counts.
                // Do not copy/commit code merely to reject a throttled attempt.
                if (hotBlockThreshold != 1 && compileCredits == 0)
                { ++compileBudgetDenials; retryVisits = 4; Fallback("CompileBudget"); return false; }
                if (!owner.TryCopyJitCode(machine, ip, current[..Math.Min(15, 4096 - (int)(ip & 4095))], out string reason))
                { retryVisits = 64; Fallback(reason); return false; }
                if (hotBlockThreshold != 1) --compileCredits;
                entry = Compile(owner, machine, ip, slot);
                if (entry?.Allocation == null)
                {

                    retryVisits = entry?.NegativeDelay ?? 64;
                    Fallback(entry?.Fallback ?? (active ? "CacheFull" : "Capability")); return false;
                }
                if (reconsideredNegative) ++negativePromotions;
                if (!owner.TryCopyJitCode(machine, ip, current[..entry.Bytes.Length], out reason) ||
                    !current[..entry.Bytes.Length].SequenceEqual(entry.Bytes))
                {
                    ++validations; Retire(slot); Fallback("CodeChanged"); return false;
                }
            }
            hint = new() { Owner = ownerIdentity, Ip = ip, Slot = slot, Generation = entry.Generation,
                HasData = entry.AddressPlan.HasMemory };
            frame->OwnerHandle = GCHandle.ToIntPtr(ownerRoot);
            ++nativeDispatches;
            // Each leaf retains all architectural writes and per-instruction
            // effects. Continuation reuses only the owner/mapping read leases;
            // it never follows an unauthenticated guest number as host code.
            JitDataDescriptor* descriptors = stackalloc JitDataDescriptor[4];
            ulong totalExecuted = 0;
            int calls = 0;
            while (true)
            {
                ++nativeEntries; ++calls;
                frame->Executed = 0;
                frame->NativeLimit = (uint)(MaximumInstructions - totalExecuted);
                frame->DataDescriptors = descriptors;
                frame->DataGeneration = dataLeases.Epoch;
                frame->DataDescriptorCount = (uint)dataLeases.Fill(owner.JitProgram, machine, entry.AddressPlan,
                    new Span<JitDataDescriptor>(descriptors, 4));
                try { ((delegate* unmanaged<JitFrame*, int>)entry.Allocation!.Entry)(frame); }
                finally { frame->DataDescriptors = null; frame->DataDescriptorCount = 0; frame->DataGeneration = 0; }
                ulong part = frame->Executed;
                if (part > (ulong)MaximumInstructions - totalExecuted)
                    throw new InvalidOperationException("Native continuation exceeded its shared dynamic cap");
                totalExecuted += part;
                ++entry.Calls; entry.Instructions += checked((long)part);
                if (frame->Exit == JitExit.MemoryMiss) ++entry.Misses;
                entry.UsefulCredit = Math.Min(65535u, entry.UsefulCredit + (uint)Math.Max(0, (long)part - 3));
                nativeInstructions += checked((long)part);
                // Path-aware accounting: each leaf's actual committed prefix
                // is charged separately, never one prefix across linked paths.
                if (entry.MemoryPrefixes.Length != 0) nativeMemoryInstructions += entry.MemoryPrefixes[(int)part];
                if (frame->Exit == JitExit.MemoryMiss) Fallback("MemoryGuardMiss");
                if ((frame->Exit is JitExit.Continue or JitExit.MemoryMiss) && part < 4)
                {
                    ++shortNativeExits;
                    entry.ShortExitStreak = Math.Min(4, entry.ShortExitStreak + 1);
                    // The caller's cooldown belongs to its original IP. A
                    // chained target's short feedback only suppresses linking.
                    if (calls == 1 && hotBlockThreshold != 1 && entry.ShortExitStreak == 4)
                    { retryVisits = 64; Fallback("ShortNativeExit"); }
                }
                else entry.ShortExitStreak = 0;
                if (frame->Exit != JitExit.Continue || retryVisits != 0 || part == 0 ||
                    calls == 8 || totalExecuted > MaximumInstructions - 4 ||
                    !AllowsContinuation(entry.AddressPlan, part, frame->NextIp) || frame->NextIp != machine->ip ||
                    !owner.IsJitMachineMode(machine)) break;
                ulong next = frame->NextIp;
                if (next == ip || staticCode?.Validate(owner.JitProgram, machine, next) != true) break;
                int nextFirst = Bucket(next, entries.Length);
                Entry? successor = null;
                for (int i = nextFirst; i < nextFirst + Ways; ++i)
                    if (entries[i] is { Allocation: not null } candidate && candidate.Ip == next &&
                        (mappingHeld || !candidate.AddressPlan.HasMemory) &&
                        (hotBlockThreshold == 1 || candidate.ShortExitStreak < 4))
                    { successor = candidate; break; }
                if (successor == null) break;
                entry = successor; ip = next; entry.Used = visits;
                ++hits; ++continuations;
            }
            frame->Executed = totalExecuted;
            frame->NativeLimit = 0;
            return true;
        }
    }

    // An exact completed-instruction index names the materialized IR exit
    // map. Final internal backedges repeat only their linear suffix; early
    // taken branches already exit that same prefix. Dynamic RET targets remain
    // materialized guest values and still require the target cache/code lease.
    private static bool AllowsContinuation(JitAddressPlan plan, ulong executed, ulong nextIp)
    {
        var steps = plan.Instructions;
        if (executed == 0 || steps.IsEmpty) return false;
        int index = checked((int)executed - 1);
        if (index >= steps.Length)
        {
            int header = steps[^1].InternalTarget;
            if (header < 0 || header >= steps.Length) return false;
            index = header + (index - steps.Length) % (steps.Length - header);
        }
        ref readonly var instruction = ref steps[index];
        if (instruction.Operation == JitOperation.Ret)
            return instruction.NextIpValue != 0 && instruction.ExitState(JitPlanBoundary.CompletedTaken).StateIsMaterialized;
        var boundary = instruction.Operation == JitOperation.ConditionalJump && nextIp == instruction.Target ?
            JitPlanBoundary.CompletedTaken : JitPlanBoundary.CompletedFallThrough;
        var state = instruction.ExitState(boundary);
        return state.StateIsMaterialized && state.ExecutedDelta == 1 && state.Ip == nextIp;
    }

    private Entry? Compile(ThreadedGuestExecution owner, Blink.Machine* machine, ulong ip, int slot)
    {
        ++compileAttempts;
        long started = Stopwatch.GetTimestamp();
        long allocationStarted = GC.GetAllocatedBytesForCurrentThread();
        try
        {
            Span<JitInstruction> instructions = stackalloc JitInstruction[MaximumInstructions];
            int instructionCount = 0;
            Span<byte> code = stackalloc byte[MaximumBytes];
            Span<byte> decoded = stackalloc byte[15];
            int size = 0;
            string reason = "UnsupportedInstruction";
            for (int i = 0; i < MaximumInstructions; ++i)
            {
                ulong next = ip + (uint)size;
                if ((next & ~4095UL) != (ip & ~4095UL)) break;
                if (!owner.TryDecodeJitInstruction(machine, next, out var instruction, decoded,
                    out _, out reason)) break;
                decoded[..instruction.Length].CopyTo(code[size..]);
                size += instruction.Length; instructions[instructionCount++] = instruction;
                // Forward conditional branches can keep their fall-through
                // in the same bounded trace. Taken early branches side-exit;
                // only the final backedge may dispatch internally.
                if (instruction.Operation is JitOperation.Jump or JitOperation.Call or JitOperation.Ret ||
                    instruction.Operation == JitOperation.ConditionalJump && instruction.Target <= instruction.Ip) break;
            }
            ++fragmentLengths[instructionCount];
            // Default admission avoids short fragments surrounding interpreted
            // memory instructions. Internal backedges amortize the transition
            // across up to 32 dynamic instructions; longer straight blocks
            // can also repay it. Threshold one is the diagnostic override.
            bool internalBackedge = false;
            if (instructionCount != 0 && instructions[instructionCount - 1].Operation is JitOperation.Jump or JitOperation.ConditionalJump)
                for (int i = 0; i < instructionCount; ++i)
                    if (instructions[i].Ip == instructions[instructionCount - 1].Target) { internalBackedge = true; break; }
            if (instructionCount == 0 || (hotBlockThreshold != 1 && instructionCount < 4 && !internalBackedge))
            {
                if (instructionCount != 0) reason = "UnprofitableBlock";
                int length = Math.Min(15, 4096 - (int)(ip & 4095));
                if (!owner.TryCopyJitCode(machine, ip, code[..length], out string copyReason))
                { Fallback(copyReason); return null; }
                return Publish(slot, ip, code[..length], null, reason);
            }
            // Stop at a complete loop iteration when its backedge targets the
            // first instruction. This avoids an unprofitable suffix between calls.
            int dynamicLimit = internalBackedge && instructions[instructionCount - 1].Target == ip ?
                MaximumInstructions / instructionCount * instructionCount : MaximumInstructions;
            byte[] emitted = X64BlockEmitter.Emit(instructions[..instructionCount], dynamicLimit);
            emittedCodeBytes += emitted.Length;
            // Preflight the same rounded page charge before allocating RX
            // storage. Allocation's actual charge is checked again on publish.
            long expectedAllocation = ((emitted.LongLength + Environment.SystemPageSize - 1) /
                Environment.SystemPageSize) * Environment.SystemPageSize;
            var plan = JitAddressPlan.Build(instructions[..instructionCount]);
            if (!MakeRoom(EntryCharge + size + expectedAllocation + plan.Charge, slot)) return null;
            if (!NativeCodeAllocation.TryCreate(emitted, out var allocation, out string allocationReason))
            {
                capability = allocationReason; active = false;
                return Publish(slot, ip, code[..size], null, "Capability");
            }
            Entry? entry = Publish(slot, ip, code[..size], allocation, null, plan,
                !plan.HasMemory ? null : JitMemoryCounts.Build(instructions[..instructionCount]));
            if (entry != null) ++compiledBlocks;
            return entry;
        }
        catch (PlatformNotSupportedException error)
        {
            // Layout/host emitter capability failures occur in managed code,
            // before any native invocation. Preserve ordinary interpretation.
            capability = "Native emitter unavailable: " + error.Message;
            active = false;
            return null;
        }
        finally { compileMilliseconds += Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            compileAllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocationStarted; }
    }

    // Bounded global reclamation prevents cold entries in unrelated sets from
    // denying an admitted block while the selected set has little to reclaim.
    // The gate drains all allocations; worker hints contain no native pointer.
    private bool MakeRoom(long charge, int reservedSlot)
    {
        if (charge <= limit - bytes + (entries[reservedSlot]?.Charge ?? 0)) return true;
        long reclaimable = entries[reservedSlot]?.Charge ?? 0;
        for (int i = 0; i < entries.Length; ++i)
            if (i != reservedSlot) reclaimable += entries[i]?.Charge ?? 0;
        foreach (var negative in negatives) reclaimable += negative?.Charge ?? 0;
        if (charge > limit - bytes + reclaimable) return false;
        while (charge > limit - bytes + (entries[reservedSlot]?.Charge ?? 0))
        {
            // Cold structural decisions cannot permanently starve executable
            // work. They retain their own table, but not an unreclaimable byte
            // reservation. Reclaim oldest negatives before useful positives.
            int negativeVictim = -1;
            for (int i = 0; i < negatives.Length; ++i)
                if (negatives[i] is { } candidate && (negativeVictim < 0 || candidate.Used < negatives[negativeVictim]!.Used))
                    negativeVictim = i;
            if (negativeVictim >= 0)
            {
                bytes -= negatives[negativeVictim]!.Charge; negatives[negativeVictim] = null;
                ++evictions; ++budgetEvictions; ++negativeBudgetEvictions; continue;
            }
            int victim = -1;
            ulong score = ulong.MaxValue;
            for (int i = 0; i < entries.Length; ++i)
                if (i != reservedSlot && entries[i] is { } entry)
                {
                    ulong candidate = RetentionScore(entry);
                    if (victim < 0 || candidate < score) { victim = i; score = candidate; }
                }
            if (victim < 0) return false;
            if (entries[victim]!.UsefulCredit > 64) ++hotPositiveEvictions;
            Retire(victim); ++evictions; ++budgetEvictions;
        }
        return true;
    }

    private Entry? Publish(int slot, ulong ip, ReadOnlySpan<byte> code, NativeCodeAllocation? allocation, string? fallback, JitAddressPlan? plan = null, byte[]? memoryPrefixes = null)
    {
        long charge = EntryCharge + code.Length + (allocation?.AllocatedBytes ?? 0) + (plan?.Instructions.Length > 0 ? plan.Charge : 0);
        bool negative = allocation == null;
        var table = negative ? negatives : entries;
        if (negative)
        {
            int first = Bucket(ip, negatives.Length); slot = first;
            for (int i = first; i < first + Ways; ++i)
            {
                if (negatives[i]?.Ip == ip) { slot = i; break; }
                if (negatives[i] == null || negatives[slot] != null && negatives[i]!.Used < negatives[slot]!.Used) slot = i;
            }
        }
        var victim = table[slot];
        if (charge > limit - bytes + (victim?.Charge ?? 0)) { allocation?.Dispose(); return null; }
        Entry entry;
        try
        {
            entry = new() { Ip = ip, Bytes = code.ToArray(), Charge = charge,
                Allocation = allocation, Fallback = fallback, Used = visits, RetryAt = visits + 64,
                Generation = ++entryGeneration,
                StableNegative = negative && (fallback is "UnsupportedInstruction" or "UnprofitableBlock" or "Prefix" or "MemoryInstruction") && staticCode?.Contains(ip) == true,
                AddressPlan = plan ?? JitAddressPlan.Empty,
                MemoryPrefixes = memoryPrefixes ?? Array.Empty<byte>() };
        }
        catch { allocation?.Dispose(); throw; }
        if (victim != null)
        {
            ++entryCollisions; ++evictions;
            if (!negative && victim.UsefulCredit > 64) ++hotPositiveEvictions;
            table[slot] = null; bytes -= victim.Charge; victim.Allocation?.Dispose();
        }
        table[slot] = entry;
        bytes += charge; peakBytes = Math.Max(peakBytes, bytes);
        return entry;
    }

    private void RetireNegative(ulong ip)
    {
        int first = Bucket(ip, negatives.Length);
        for (int i = first; i < first + Ways; ++i)
            if (negatives[i]?.Ip == ip) { bytes -= negatives[i]!.Charge; negatives[i] = null; return; }
    }

    private void Retire(int slot)
    {
        Entry? entry = entries[slot];
        if (entry == null) return;
        // The cache gate covers native entry through return, so retirement can
        // never free storage underneath a native caller. No other entry owns
        // this allocation, and clearing precedes its sole Dispose.
        entries[slot] = null; bytes -= entry.Charge; entry.Allocation?.Dispose();
    }

    private void Fallback(string reason) => fallbacks[reason] = fallbacks.GetValueOrDefault(reason) + 1;

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            for (int i = 0; i < entries.Length; ++i) Retire(i);
            Array.Clear(negatives);
            retiredStaticPages = staticCode?.PageCount ?? 0; retiredRevokedPages = staticCode?.RevokedPages ?? 0;
            ownerRoot.Free(); staticCode = null; bytes = 0; active = false; disposed = true;
        }
    }

    // This allowlist accepts exact x86 encodings only after Blink's decoder
    // supplies their length. Length agreement prevents a prefix/encoding from
    // being interpreted by a second, more permissive decoder. No guest bytes
    // are ever copied into executable storage.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction, out string reason)
    {
        instruction = default; reason = "UnsupportedInstruction";
        if (bytes.IsEmpty) return false;
        // Blink's 0F 1E hint handler models ENDBR64 as a no-op. Recognize
        // exactly this encoding, without admitting other REP/prefix families.
        if (bytes.Length == 4 && bytes[0] == 0xf3 && bytes[1] == 0x0f && bytes[2] == 0x1e && bytes[3] == 0xfa)
        {
            instruction = new(ip, 4, JitOperation.Nop, 32, default, default);
            return true;
        }
        int position = 0;
        bool operand16 = bytes[position] == 0x66;
        if (operand16 && ++position == bytes.Length) return false;
        byte rex = 0;
        if (bytes[position] is >= 0x40 and <= 0x4f)
        { rex = bytes[position++]; if (position == bytes.Length) return false; }
        byte opcode = bytes[position++];
        if (opcode is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 ||
            opcode is >= 0x40 and <= 0x4f)
        { reason = "Prefix"; return false; }
        int width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        JitOperation operation;
        JitOperand destination = default, source = default;
        ulong target = 0; int condition = 0;
        int sourceWidth = 0; JitAddress address = default;
        if (opcode is >= 0xb0 and <= 0xbf)
        {
            operation = JitOperation.Mov;
            if (opcode < 0xb8) width = 8;
            destination = Operand((opcode & 7) | ((rex & 1) << 3), width, rex != 0);
            if (!Immediate(bytes, ref position, width / 8, false, out source)) return false;
        }
        else if (opcode is 0xe8 or 0xc3)
        {
            // Only unprefixed long-mode CALL rel32 and RET near64. Blink's
            // stack helpers use eight bytes, leave flags untouched and set IP.
            if (operand16 || rex != 0) { reason = "Prefix"; return false; }
            operation = opcode == 0xe8 ? JitOperation.Call : JitOperation.Ret;
            width = sourceWidth = 64;
            address = new(4, -1, 0, opcode == 0xe8 ? -8 : 0);
            if (opcode == 0xe8)
            {
                if (!Immediate(bytes, ref position, 4, true, out var displacement)) return false;
                target = unchecked(ip + (uint)bytes.Length + displacement.Immediate);
            }
        }
        else if (opcode is >= 0x50 and <= 0x5f)
        {
            if (operand16) { reason = "Prefix"; return false; }
            width = sourceWidth = 64;
            var register = new JitOperand((opcode & 7) | ((rex & 1) << 3), 0);
            operation = opcode < 0x58 ? JitOperation.Push : JitOperation.Pop;
            if (operation == JitOperation.Push) source = register;
            else destination = register;
            address = new(4, -1, 0, operation == JitOperation.Push ? -8 : 0);
        }
        else if (opcode == 0x8b && width is 32 or 64 && position < bytes.Length && (bytes[position] & 0xc0) != 0xc0)
        {
            byte modrm = bytes[position++];
            destination = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0);
            if (!Address(bytes, ref position, modrm, rex, out address)) return false;
            operation = JitOperation.LoadMov; sourceWidth = width;
        }
        else if (opcode == 0x89 && width is 32 or 64 && position < bytes.Length && (bytes[position] & 0xc0) != 0xc0)
        {
            byte modrm = bytes[position++];
            source = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0);
            if (!Address(bytes, ref position, modrm, rex, out address)) return false;
            operation = JitOperation.StoreMov; sourceWidth = width;
        }
        else if (position < bytes.Length && (bytes[position] & 0xc0) != 0xc0 &&
            (opcode is >= 0x38 and <= 0x3b or 0x84 or 0x85 ||
             opcode is 0x80 or 0x81 or 0x83 && ((bytes[position] >> 3) & 7) == 7 ||
             opcode is 0xf6 or 0xf7 && ((bytes[position] >> 3) & 7) == 0 && (rex & 4) == 0))
        {
            byte modrm = bytes[position++];
            if ((opcode & 1) == 0 && opcode != 0x83) width = 8;
            sourceWidth = width;
            if (!Address(bytes, ref position, modrm, rex, out address)) return false;
            bool reverse = opcode is 0x3a or 0x3b;
            operation = opcode is 0x84 or 0x85 or 0xf6 or 0xf7 ? JitOperation.MemoryTest :
                reverse ? JitOperation.MemoryCmpReverse : JitOperation.MemoryCmp;
            if (opcode is 0x80 or 0x81 or 0x83 or 0xf6 or 0xf7)
            {
                int count = opcode == 0x83 ? 1 : width == 64 ? 4 : width / 8;
                if (!Immediate(bytes, ref position, count, opcode == 0x83 || width == 64, out source)) return false;
            }
            else
            {
                var register = Operand(((modrm >> 3) & 7) | ((rex & 4) << 1), width, rex != 0);
                if (reverse) destination = register; else source = register;
            }
        }
        else if (opcode is 0x88 or 0x89 or 0x8a or 0x8b or 0xc6 or 0xc7)
        {
            operation = JitOperation.Mov;
            if ((opcode & 1) == 0) width = 8;
            if (!Registers(bytes, ref position, width, rex, out var rm, out var reg, out int group, out reason)) return false;
            if (opcode is 0xc6 or 0xc7)
            {
                if (group != 0 || (rex & 4) != 0) return false;
                destination = rm;
                if (!Immediate(bytes, ref position, width == 64 ? 4 : width / 8, width == 64, out source)) return false;
            }
            else if ((opcode & 2) == 0) { destination = rm; source = reg; }
            else { destination = reg; source = rm; }
        }
        else if (opcode == 0x8d)
        {
            if (width is not (32 or 64)) return false;
            if (position >= bytes.Length) return false;
            byte modrm = bytes[position++];
            destination = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0);
            if (!Address(bytes, ref position, modrm, rex, out address)) return false;
            operation = JitOperation.Lea;
        }
        else if (opcode == 0x63)
        {
            // Only the ordinary long-mode MOVSXD form; ARPL and non-REX.W
            // variants retain their existing interpreter semantics.
            if (width != 64) return false;
            if (!Registers(bytes, ref position, 32, rex, out source, out destination, out _, out reason)) return false;
            operation = JitOperation.Movsx; sourceWidth = 32;
        }
        else if (opcode == 0x0f && position < bytes.Length && bytes[position] is 0xb6 or 0xb7 or 0xbe or 0xbf)
        {
            byte extension = bytes[position++];
            sourceWidth = (extension & 1) == 0 ? 8 : 16;
            if (position < bytes.Length && (bytes[position] & 0xc0) != 0xc0 && (extension & 8) == 0 && width is 32 or 64)
            {
                byte modrm = bytes[position++];
                destination = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0);
                if (!Address(bytes, ref position, modrm, rex, out address)) return false;
                operation = JitOperation.LoadMovzx;
            }
            else
            {
                if (!Registers(bytes, ref position, sourceWidth, rex, out source, out _, out int group, out reason)) return false;
                // ModRM.reg has the destination width, so encodings 4..7 select
                // SP/BP/SI/DI rather than AH/CH/DH/BH even without REX.
                destination = new(group | ((rex & 4) << 1), 0);
                operation = (extension & 8) == 0 ? JitOperation.Movzx : JitOperation.Movsx;
            }
        }
        else if (opcode == 0x0f && position < bytes.Length && bytes[position] == 0x1f)
        {
            ++position;
            if (position >= bytes.Length) return false;
            byte modrm = bytes[position++];
            if ((modrm & 0x38) != 0 || (rex & 4) != 0) return false;
            // Blink uses these nominal NOPs for BOFRAM/BINBASE side effects.
            // They must execute the interpreter hook even if no data is read.
            if (modrm is 0x45 or 0x07 or 0x47 or 0x87)
            { reason = "InstrumentationNop"; return false; }
            if ((modrm & 0xc0) != 0xc0 && !Address(bytes, ref position, modrm, rex, out _)) return false;
            operation = JitOperation.Nop;
        }
        else if ((opcode <= 0x3d) && (opcode & 7) <= 5 && Alu(opcode >> 3, out operation))
        {
            int form = opcode & 7;
            if ((form & 1) == 0) width = 8;
            if (form < 4)
            {
                if (!Registers(bytes, ref position, width, rex, out var rm, out var reg, out _, out reason)) return false;
                if (form < 2) { destination = rm; source = reg; }
                else { destination = reg; source = rm; }
            }
            else
            {
                destination = new(0, 0);
                if (!Immediate(bytes, ref position, width == 64 ? 4 : width / 8, width == 64, out source)) return false;
            }
        }
        else if (opcode is 0x80 or 0x81 or 0x83)
        {
            if (opcode == 0x80) width = 8;
            if (!Registers(bytes, ref position, width, rex, out destination, out _, out int group, out reason) ||
                !Alu(group, out operation)) return false;
            int count = opcode == 0x83 ? 1 : width == 64 ? 4 : width / 8;
            if (!Immediate(bytes, ref position, count, opcode == 0x83 || width == 64, out source)) return false;
        }
        else if (opcode is 0x84 or 0x85 or 0xa8 or 0xa9 or 0xf6 or 0xf7)
        {
            operation = JitOperation.Test;
            if ((opcode & 1) == 0) width = 8;
            if (opcode is 0xa8 or 0xa9) destination = new(0, 0);
            else
            {
                if (!Registers(bytes, ref position, width, rex, out destination, out source, out int group, out reason)) return false;
                if (opcode is 0xf6 or 0xf7 && (group != 0 || (rex & 4) != 0)) return false;
            }
            if (opcode is not (0x84 or 0x85) &&
                !Immediate(bytes, ref position, width == 64 ? 4 : width / 8, width == 64, out source)) return false;
        }
        else if (opcode is 0xfe or 0xff)
        {
            if (opcode == 0xfe) width = 8;
            if (!Registers(bytes, ref position, width, rex, out destination, out _, out int group, out reason) ||
                group > 1 || (rex & 4) != 0) return false;
            operation = group == 0 ? JitOperation.Inc : JitOperation.Dec;
        }
        else if (opcode is 0xe9 or 0xeb || opcode is >= 0x70 and <= 0x7f || opcode == 0x0f)
        {
            if (operand16 || rex != 0) { reason = "Prefix"; return false; }
            if (opcode == 0x0f)
            {
                if (position >= bytes.Length || bytes[position] is < 0x80 or > 0x8f) return false;
                condition = bytes[position++] & 15;
                operation = JitOperation.ConditionalJump;
            }
            else
            {
                operation = opcode is 0xe9 or 0xeb ? JitOperation.Jump : JitOperation.ConditionalJump;
                condition = opcode & 15;
            }
            int count = opcode is 0xe9 or 0x0f ? 4 : 1;
            if (!Immediate(bytes, ref position, count, true, out var displacement)) return false;
            target = unchecked(ip + (uint)bytes.Length + displacement.Immediate);
            width = 64;
        }
        else if (opcode == 0x90 && (rex & 1) == 0) { operation = JitOperation.Nop; }
        else
        {
            // Common implicit/explicit memory operations have a useful reason
            // even though no addressing mode is parsed or issued natively.
            if (opcode is >= 0x50 and <= 0x5f or 0x68 or 0x6a or 0x8d or 0xc2 or 0xc3 or 0xc8 or 0xc9 or
                >= 0xa0 and <= 0xa7 or >= 0xaa and <= 0xaf) reason = "MemoryInstruction";
            return false;
        }
        if (position != bytes.Length) { reason = "EncodingLength"; return false; }
        instruction = new(ip, bytes.Length, operation, width, destination, source, target, condition, sourceWidth, address);
        return true;
    }

    private static bool Alu(int group, out JitOperation operation)
    {
        operation = group switch
        {
            0 => JitOperation.Add, 1 => JitOperation.Or, 4 => JitOperation.And,
            5 => JitOperation.Sub, 6 => JitOperation.Xor, 7 => JitOperation.Cmp, _ => JitOperation.Nop
        };
        return group is 0 or 1 or 4 or 5 or 6 or 7;
    }

    private static JitOperand Operand(int register, int width, bool rex) =>
        width == 8 && !rex && register is >= 4 and < 8
            ? new(register - 4, 0, ByteOffset: 1) : new(register, 0);

    private static bool Registers(ReadOnlySpan<byte> bytes, ref int position, int width, byte rex,
        out JitOperand rm, out JitOperand reg, out int group, out string reason)
    {
        rm = reg = default; group = 0; reason = "UnsupportedInstruction";
        if (position >= bytes.Length) return false;
        byte modrm = bytes[position++]; group = (modrm >> 3) & 7;
        if ((modrm & 0xc0) != 0xc0) { reason = "MemoryInstruction"; return false; }
        rm = Operand((modrm & 7) | ((rex & 1) << 3), width, rex != 0);
        reg = Operand(group | ((rex & 4) << 1), width, rex != 0);
        return true;
    }

    private static bool Immediate(ReadOnlySpan<byte> bytes, ref int position, int count, bool signed, out JitOperand value)
    {
        value = default;
        if (count > bytes.Length - position) return false;
        ReadOnlySpan<byte> source = bytes.Slice(position, count);
        ulong number = count switch
        {
            1 => signed ? unchecked((ulong)(sbyte)source[0]) : source[0],
            2 => signed ? unchecked((ulong)BinaryPrimitives.ReadInt16LittleEndian(source)) : BinaryPrimitives.ReadUInt16LittleEndian(source),
            4 => signed ? unchecked((ulong)BinaryPrimitives.ReadInt32LittleEndian(source)) : BinaryPrimitives.ReadUInt32LittleEndian(source),
            8 => BinaryPrimitives.ReadUInt64LittleEndian(source), _ => throw new InvalidOperationException()
        };
        position += count; value = new(0, number, true); return true;
    }

    private static bool Address(ReadOnlySpan<byte> bytes, ref int position, byte modrm, byte rex, out JitAddress address)
    {
        address = default;
        int mod = modrm >> 6, rm = modrm & 7;
        if (mod == 3) return false;
        int @base = -1, index = -1, scale = 0;
        bool ripRelative = false;
        int displacementBytes = mod == 1 ? 1 : mod == 2 ? 4 : 0;
        if (rm == 4)
        {
            if (position >= bytes.Length) return false;
            byte sib = bytes[position++];
            int sibBase = sib & 7, sibIndex = (sib >> 3) & 7;
            scale = sib >> 6;
            if (mod == 0 && sibBase == 5) displacementBytes = 4;
            else @base = sibBase | ((rex & 1) << 3);
            if (sibIndex != 4 || (rex & 2) != 0) index = sibIndex | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5)
        { displacementBytes = 4; ripRelative = true; }
        else @base = rm | ((rex & 1) << 3);
        long displacement = 0;
        if (displacementBytes != 0)
        {
            if (!Immediate(bytes, ref position, displacementBytes, true, out var value)) return false;
            displacement = unchecked((long)value.Immediate);
        }
        address = new(@base, index, scale, displacement, ripRelative);
        return true;
    }
}
