using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    private readonly GuestJitOptions jitOptions;
    private GuestBlockJit? guestJit;
    private volatile bool guestJitActive;
    private long reapedJitBlockEntries, reapedJitNegativeSkips, reapedJitColdEntries, reapedJitHintCollisions;
    internal int JitWorkerCapacity => maximumWorkers;
    internal Blink JitProgram => program!;
    // Admission observes executed control-flow boundaries, even when compilation
    // is throttled. This state belongs to the worker, never to the owner.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ObserveJitBlockEntry(Worker worker, ulong previousIp)
    {
        var decoded = worker.Machine->xedd;
        int length = decoded == null ? 0 : decoded->length;
        bool boundary = length is < 1 or > 15 ||
            worker.Machine->ip != unchecked(previousIp + (uint)length);
        if (!boundary)
        {
            // The interpreter has already decoded the opcode map into rde.
            // Reuse it instead of rescanning prefixes after every instruction.
            uint opcode = (uint)((decoded->op.rde >> 40) & 2047);
            boundary = opcode - 0x70 <= 15 || opcode - 0x180 <= 15 ||
                opcode is >= 0xe0 and <= 0xe3 or 0xe8 or 0xe9 or 0xeb or
                0xc2 or 0xc3 or 0xca or 0xcb or 0xcf or 0xcc or 0xcd or
                0x105 or 0x107 or 0x134 or 0x135;
        }
        // Long straight-line regions also have bounded candidate entries.
        worker.JitBlockEntry = boundary || ++worker.JitStraightLine >= 32;
        if (worker.JitBlockEntry)
        {
            worker.JitStraightLine = 0;
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private bool ShouldProbeGuestJit(Worker worker)
    {
        if (!worker.JitBlockEntry) return false;
        ++worker.JitObservedEntries;
        // This negative hint belongs only to this IP/worker. Observations at
        // another block are never suppressed by an unsuitable memory loop.
        ulong ip = worker.Machine->ip;
        if (worker.JitNegativeIp == ip && worker.JitNegativeUntil > worker.JitObservedEntries)
        { ++worker.JitNegativeSkips; worker.JitNegativeInstructions = 256; return false; }
        var hints = worker.JitNegatives!;
        int slot = (int)((ip ^ (ip >> 12) ^ (ip >> 24)) & (ulong)(hints.Length / 2 - 1)) * 2;
        if (hints[slot + 1].Ip == ip) ++slot;
        else if (hints[slot].Ip != ip)
        {
            if (hints[slot + 1].HotCount == 0 || hints[slot + 1].Until < hints[slot].Until) ++slot;
            if (hints[slot].HotCount != 0) ++worker.JitHintCollisions;
            hints[slot] = new() { Ip = ip };
        }
        ref var hint = ref hints[slot];
        if (hint.Until > worker.JitObservedEntries)
        {
            ++worker.JitNegativeSkips;
            worker.JitNegativeIp = ip; worker.JitNegativeUntil = hint.Until; worker.JitNegativeInstructions = 256; return false;
        }
        if (hint.HotCount < jitOptions.HotBlockThreshold && ++hint.HotCount < jitOptions.HotBlockThreshold)
        { ++worker.JitColdEntries; return false; }
        return true;
    }

    private static void RememberJitNegative(Worker worker, ulong ip, int delay, int maximumDelay = 16384)
    {
        var hints = worker.JitNegatives!;
        int slot = (int)((ip ^ (ip >> 12) ^ (ip >> 24)) & (ulong)(hints.Length / 2 - 1)) * 2;
        if (hints[slot + 1].Ip == ip) ++slot;
        else if (hints[slot].Ip != ip && hints[slot + 1].Until < hints[slot].Until) ++slot;
        ref var hint = ref hints[slot];
        delay = hint.Ip == ip ? Math.Max(delay, Math.Min(maximumDelay, hint.Delay * 2)) : delay;
        hint = new() { Ip = ip, Until = worker.JitObservedEntries + delay, Delay = delay, HotCount = hint.Ip == ip ? hint.HotCount : 0 };
        worker.JitNegativeIp = ip; worker.JitNegativeUntil = hint.Until;
    }

    public GuestJitStatistics JitStatistics
    {
        get
        {
            var snapshot = guestJit?.Snapshot() ?? new GuestJitStatistics { InterpreterInstructions = Volatile.Read(ref completed) };
            // Acquire the lifecycle gate only after Snapshot released the native
            // gate. Reaping transfers worker-local diagnostic totals atomically
            // with removal; no per-boundary shared diagnostic atomic is needed.
            lock (gate)
            {
                long entries = reapedJitBlockEntries, negatives = reapedJitNegativeSkips, cold = reapedJitColdEntries, collisions = reapedJitHintCollisions;
                foreach (var worker in workers)
                { entries += Volatile.Read(ref worker.JitObservedEntries); negatives += Volatile.Read(ref worker.JitNegativeSkips); cold += Volatile.Read(ref worker.JitColdEntries); collisions += Volatile.Read(ref worker.JitHintCollisions); }
                var fallbacks = new Dictionary<string, long>(snapshot.Fallbacks, StringComparer.Ordinal);
                if (cold != 0) fallbacks["ColdBlock"] = cold;
                return snapshot with { BlockEntries = entries, NegativeCacheHits = snapshot.NegativeCacheHits + negatives,
                    HotnessCollisions = collisions, Fallbacks = fallbacks };
            }
        }
    }

    private bool TryExecuteGuestBlock(Worker worker)
    {
        // Keep the original rejected-issuance path authoritative when the
        // budget is already exhausted. Even a checked code probe may commit
        // a lazy guest page, so it must not precede this known budget stop.
        if (guestJit == null || (ulong)Volatile.Read(ref issued) >= instructionBudget) return false;
        // No native entry inside SignalActor: its execute/restored/attention
        // ordering remains the original single-instruction interpreter path.
        worker.NativePoll ??= worker.Stop.AcquireNativePoll();
        JitFrame frame = new()
        {
            Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame),
            Machine = worker.Machine, Stop = worker.NativePoll.Pointer,
            Budget = instructionBudget, NextIp = worker.Machine->ip
        };
        ulong entryIp = worker.Machine->ip;
        bool entered;
        int retryVisits;
        var entryHints = worker.JitEntries!;
        int hintSlot = (int)((entryIp ^ (entryIp >> 12) ^ (entryIp >> 24)) & 7) * 2;
        if (entryHints[hintSlot + 1].Ip == entryIp) ++hintSlot;
        else if (entryHints[hintSlot].Ip != entryIp && (worker.JitObservedEntries & 1) != 0) ++hintSlot;
        fixed (long* issuedPointer = &issued, completedPointer = &completed)
        {
            frame.Issued = issuedPointer; frame.Completed = completedPointer;
            entered = guestJit.TryExecute(this, worker.Machine, &frame, ref entryHints[hintSlot], out retryVisits);
        }
        worker.JitBlockEntry = false;
        if (!entered)
        {
            guestJitActive = guestJit.IsActive;
            if (retryVisits != 0)
            {
                RememberJitNegative(worker, worker.Machine->ip, retryVisits);
            }
            return false;
        }
        // A bounded native exit is a real continuation entry. The cache may
        // reuse the loop head without sending a suffix through interpretation.
        worker.JitBlockEntry = true;
        worker.JitStraightLine = 0;
        worker.Instructions += frame.Executed;
        if (retryVisits != 0) RememberJitNegative(worker, entryIp, retryVisits, maximumDelay: 256);
        switch (frame.Exit)
        {
            case JitExit.Budget: stop.RequestBudgetStop(); break;
            case JitExit.MemoryMiss:
                // Guards precede issuance and effects. Return to the ordinary
                // checkpoint, then interpret this IP exactly once. No native
                // retry or preissued ExecuteOne continuation is needed.
                worker.JitBlockEntry = false;
                if (frame.Executed < 8)
                {
                    RememberJitNegative(worker, entryIp, 64, maximumDelay: 256);
                }
                break;
            case JitExit.Continue:
            case JitExit.Stop:
            case JitExit.Attention: break;
            default: throw new InvalidOperationException("Unknown native guest block exit.");
        }
        // Every native exit goes back through the ordinary stop/attention
        // checkpoint. Only a later cache miss/unsupported instruction interprets.
        return true;
    }

    internal bool TryCopyJitCode(Blink.Machine* machine, ulong ip, Span<byte> bytes, out string reason)
    {
        reason = "CodePermission";
        // Called only under the owner native gate. Enrollment plus permanent
        // pre-writer revocation provides content/identity/lifetime protection.
        if (guestJit?.TryCopyStaticCode(machine, ip, bytes) == true) return true;
        if (guestJit?.HasWorkers == true) { reason = "NonStaticCode"; return false; }
        if (bytes.Length == 0 || bytes.Length > 4096 - (int)(ip & 4095)) return false;
        // Private file mappings are copied into owner allocations by
        // HostMemory.c. Mutable shared file/memfd pages retain live leases and
        // can be changed through InstanceIo, so none is eligible. This older
        // byte-comparison path is available only before child publication.
        // Multiworker entries require the enrolled static-code lease above.
        var segvcode = machine->segvcode;
        var nofault = machine->nofault;
        machine->nofault = true;
        try
        {
            if ((int)program!.IsValidMemory(machine, (long)ip, bytes.Length, 4) == 0) return false;
            ulong entry = program.FindPageTableEntry(machine, ip & ~4095UL);
            if ((entry & (ulong)Blink.PAGE_HOST) != 0 &&
                program.BlinkHostMemoryFilePageShared(program.FindHostPage(entry)) != 0)
            { reason = "SharedExecutableBacking"; return false; }
            fixed (byte* destination = bytes)
                return program.CopyFromUser(machine, destination, (long)ip, (ulong)bytes.Length) == 0;
        }
        finally { machine->segvcode = segvcode; machine->nofault = nofault; }
    }

    internal bool TryDecodeJitInstruction(Blink.Machine* machine, ulong ip,
        out JitInstruction instruction, Span<byte> bytes, out int length, out string reason)
    {
        instruction = default; length = 0; reason = "Decode";
        var segvcode = machine->segvcode;
        var nofault = machine->nofault;
        try
        {
            Blink.XedDecodedInst decoded = default;
            int available = Math.Min(15, 4096 - (int)(ip & 4095));
            Span<byte> protectedBytes = stackalloc byte[15];
            if (!TryCopyJitCode(machine, ip, protectedBytes[..available], out reason)) return false;
            // GetInstruction is a debugger helper and uses SpyAddress: a data
            // pointer escape would correctly revoke enrolled code. Decode the
            // protected instruction bytes with the same Blink decoder instead.
            fixed (byte* source = protectedBytes)
                if (program!.DecodeInstruction(&decoded, source, (ulong)available, machine->mode.omode) != 0 ||
                    decoded.length is < 1 or > 15) return false;
            length = decoded.length;
            if (length > available) { reason = "PageBoundary"; return false; }
            new ReadOnlySpan<byte>(decoded.bytes, length).CopyTo(bytes);
            return GuestBlockJit.TryClassify(ip, bytes[..length], out instruction, out reason);
        }
        finally { machine->segvcode = segvcode; machine->nofault = nofault; }
    }

    internal bool IsJitMachineMode(Blink.Machine* machine) =>
        machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
        (int)machine->metal == 0 && (machine->flags & ((1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF))) == 0 &&
        program!.GetPc(machine) == (long)machine->ip && machine->stashaddr == 0 &&
        machine->sysdepth == 0 && machine->sigdepth == 0 && machine->pagelocks.i == 0;

    private GuestCpuState CaptureCpuState(Blink.Machine* machine)
    {
        byte* address = (byte*)machine;
        return new(machine->ip, machine->oplen, machine->flags, program!.ExportFlags(machine->flags),
            new ReadOnlySpan<byte>(address + 24, 128).ToArray(),
            new ReadOnlySpan<byte>(address + 160, 256).ToArray(),
            new ReadOnlySpan<byte>(address + 584, 104).ToArray(), machine->mxcsr,
            new ReadOnlySpan<byte>(address + 456, 128).ToArray());
    }
}
