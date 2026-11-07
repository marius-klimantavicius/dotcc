using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    partial void FullAtomicCopyReadStarted(nint machine);
    partial void FullAtomicCopyReadEnded(nint machine);
    partial void FullAtomicCopyWriteStarting(nint machine, nint address, int count);

    // Stack value retained across two-phase retries; never an escaped guest
    // pointer or managed allocation. Binds the generated EA and semantic inputs
    // to the same approved issue. Single-worker execution cannot mutate them
    // while suspended; malformed hooks/recursion fail before ticket use.
    internal static long AtomicScopeCharge => checked((sizeof(AtomicRequestSnapshot) + 127L) & ~63L);

    private readonly record struct AtomicRequestSnapshot(bool Valid, ulong Address, ulong Base, ulong Index,
        ulong Source, ulong Ax, ulong Dx, ulong Bx, ulong Cx, uint Flags, ulong Fs, ulong Gs);

    private static bool NativeAtomic(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null) return false;
        var d = plan.Decoded;
        return FullTranslationJit.TryClassifyNative(plan.Ip, new(d.bytes, d.length), out instruction) &&
            FullBlockEmitter.CanInlineAtomic(instruction);
    }

    private static AtomicRequestSnapshot CaptureAtomicRequest(in FullTranslationJit.Plan plan, FullBlockFrame* frame, Blink.Machine* machine)
    {
        if (!NativeAtomic(in plan, out var i)) return default;
        if (!FullNativeMemory.AuthenticateAtomicRequest(frame, in i, machine))
            throw new InvalidOperationException("Atomic request differs from its exact decoded plan/state.");
        ulong* r = (ulong*)((byte*)machine + 24);
        return new(true, frame->MemoryAddress, i.Address.Base < 0 ? 0 : r[i.Address.Base],
            i.Address.Index < 0 ? 0 : r[i.Address.Index], i.Source.IsImmediate ? i.Source.Immediate : r[i.Source.Register],
            r[0], r[2], r[3], r[1], machine->flags, machine->fs.@base, machine->gs.@base);
    }

    private static void AuthenticateAtomicSnapshot(in FullTranslationJit.Plan plan, FullBlockFrame* frame,
        Blink.Machine* machine, in AtomicRequestSnapshot snapshot)
    {
        if (!snapshot.Valid) return;
        if (!NativeAtomic(in plan, out _) || CaptureAtomicRequest(in plan, frame, machine) != snapshot)
            throw new InvalidOperationException("Atomic input snapshot changed while its approved issue was pending.");
    }

    private bool RequireAtomicAdmission(in FullTranslationJit.Plan plan, FullBlockFrame* frame,
        Blink.Machine* machine, in AtomicRequestSnapshot snapshot, bool ticket, bool enrolled)
    {
        if (!snapshot.Valid) return true;
        AuthenticateAtomicSnapshot(in plan, frame, machine, in snapshot);
        var h = frame->Header;
        if (h == null ||
            frame->MemoryEpoch != frame->ObservedGlobalEpoch || frame->MemorySite != frame->ResumeIndex ||
            frame->OwnerCookie != h->OwnerCookie || frame->BodyGeneration != h->BodyGeneration || frame->Context != h->Context ||
            h->Live != 1 || frame->MemorySite >= h->PayloadBytes / sizeof(FullTranslationJit.Plan) ||
            frame->ObservedCodeAuthorityEpoch != h->ValidatedGlobalEpoch)
        {
            if (!ticket && fullJit!.Authority.ScalarAtomicAdmissionClosing(frame->ObservedGlobalEpoch)) return false;
            throw new InvalidOperationException($"Atomic entry lacks exact admission: ticketOwner={ticket} flags={frame->MemoryFlags} ticket={frame->MemoryTicket} memoryEpoch={frame->MemoryEpoch} observed={frame->ObservedGlobalEpoch} site={frame->MemorySite}/{frame->ResumeIndex} headerNull={h==null} owner={frame->OwnerCookie}/{(h==null?0:h->OwnerCookie)} generation={frame->BodyGeneration}/{(h==null?0:h->BodyGeneration)} context={frame->Context:x}/{(h==null?0:h->Context):x} live={(h==null?0:h->Live)} payload={(h==null?0:h->PayloadBytes)} codeAuthority={frame->ObservedCodeAuthorityEpoch}/{(h==null?0:h->ValidatedGlobalEpoch)}");
        }
        var admitted = ((FullTranslationJit.Plan*)h->Payload)[frame->MemorySite];
        if (admitted.Helper != null || !SameNativeInstruction(in plan, in admitted))
            throw new InvalidOperationException("Atomic admission site differs from its retained plan.");
        if (!ticket && fullJit!.Authority.ScalarAtomicAdmissionClosing(frame->ObservedGlobalEpoch)) return false;
        bool ownedTicket = ticket && enrolled && frame->MemoryFlags == FullMemoryFlags.BarrierTicket && frame->MemoryTicket != 0;
        bool fastMapping = !ticket && NativeAtomic(in plan, out var instruction) &&
            FullNativeMemory.AuthenticateScalarAtomicMapping(frame, in instruction, machine, fullJit!.Authority, enrolled);
        if (!ownedTicket && !fastMapping)
        {
            if (!ticket && fullJit!.Authority.ScalarAtomicAdmissionClosing(frame->ObservedGlobalEpoch)) return false;
            throw new InvalidOperationException("Atomic entry lacks owned ticket or authenticated enrolled scalar mapping.");
        }
        return true;
    }
}
