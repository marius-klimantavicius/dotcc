using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    private static bool NativeCanonicalX87FxData(in FullTranslationJit.Plan plan,
        FullBlockFrame* frame, Blink.Machine* machine, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null) return false;
        var decoded = plan.Decoded;
        if (!FullTranslationJit.TryClassifyNative(plan.Ip,
            new(decoded.bytes, decoded.length), out instruction) ||
            !FullNativeMemory.IsCanonicalX87FxData(in instruction, out var access)) return false;
        if (frame->Machine != machine || machine->ip != instruction.Ip || frame->MemoryWidth != 3328 ||
            frame->MemoryAccess != access || frame->MemoryResume != 1 ||
            frame->MemorySite != frame->ResumeIndex ||
            frame->MemoryAddress != FullNativeMemory.CanonicalX87MemoryAddress(in instruction, machine))
            throw new InvalidOperationException("FX data request differs from its authenticated typed plan/state.");
        return true;
    }

    private static FullStackMemoryDescriptor CreateCanonicalX87FxProof(FullBlockFrame* frame) => new()
    {
        Version = FullCanonicalX87MemoryLowering.MemoryProbeTag,
        Count = 1, Site = frame->ResumeIndex, State = FullStackMemoryState.Request,
        Request0 = new FullStackMemoryRequest
        { Address = frame->MemoryAddress, Access = frame->MemoryAccess, Width = 4096 }
    };

    private static FullBlockFrame CreateCanonicalX87FxProofFrame(FullBlockFrame* frame,
        FullStackMemoryDescriptor* descriptor)
    {
        var proofFrame = *frame;
        proofFrame.StackMemory = descriptor;
        proofFrame.MemoryWidth = 4096;
        proofFrame.MemoryFlags = FullMemoryFlags.None;
        proofFrame.MemoryTicket = 0;
        proofFrame.MemoryEpoch = 0;
        return proofFrame;
    }

    private static void CopyCanonicalX87ProofFault(in FullBlockFrame proof, FullBlockFrame* target)
    {
        target->FaultTrap = proof.FaultTrap;
        target->FaultCode = proof.FaultCode;
        target->FaultAddress = proof.FaultAddress;
    }

    private FullMemoryAssistResult CheckCanonicalX87FxPermissions(Worker worker,
        in JitInstruction instruction, FullBlockFrame* frame, out FullStackMemoryDescriptor proof)
    {
        proof = CreateCanonicalX87FxProof(frame);
        // The local frame/descriptor never publishes pointers or modifies the
        // actual 416-byte request, its retained reservation or its ticket.
        fixed (FullStackMemoryDescriptor* p = &proof)
        {
            var local = CreateCanonicalX87FxProofFrame(frame, p);
            if (!FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(&local, in instruction, worker.Machine))
                throw new InvalidOperationException("Invalid typed FX full-image permission proof.");
            var result = FullNativeMemory.TryFillCanonicalX87MemoryProbe(program!, worker.Machine,
                &local, fullJit!.Authority);
            frame->MemoryAssists = local.MemoryAssists;
            if (result == FullMemoryAssistResult.GuestFault) CopyCanonicalX87ProofFault(in local, frame);
            return result;
        }
    }

    private void PublishCanonicalX87FxProof(FullBlockFrame* frame, in FullStackMemoryDescriptor proof)
    {
        if (!Monitor.IsEntered(fullJit!.Authority.Gate) || frame->StackMemory == null ||
            proof.State != FullStackMemoryState.Ready || proof.Epoch != frame->ObservedGlobalEpoch ||
            proof.Epoch != fullJit.Authority.Epoch)
            throw new InvalidOperationException("FX permission proof publication lost its final admission epoch.");
        *frame->StackMemory = proof;
        // Preserve the actual data summary, resume bit and any actual416 ticket.
    }

    private FullMemoryAssistResult TryFillNativeMemoryForPlan(Worker worker,
        in FullTranslationJit.Plan plan, FullBlockFrame* frame)
    {
        if (NativeAtomic(in plan, out var atomic))
            return FullNativeMemory.TryFillScalarAtomic(program!, worker.Machine, frame, fullJit!.Authority, in atomic);
        if (NativeNearCall(in plan, out var nearCall))
        {
            AuthenticateNativeMemoryDescriptor(in plan, frame, worker.Machine);
            return FullNativeMemory.TryFillNearCall(program!, worker.Machine, frame, fullJit!.Authority, in nearCall);
        }
        if (FullNativeMemory.IsCanonicalX87MemoryProbe(frame) ||
            !NativeCanonicalX87FxData(in plan, frame, worker.Machine, out var instruction))
            return FullNativeMemory.TryFill(program!, worker.Machine, frame, fullJit!.Authority);
        var result = CheckCanonicalX87FxPermissions(worker, in instruction, frame, out var proof);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = FullNativeMemory.TryFill(program!, worker.Machine, frame, fullJit!.Authority);
        if (result == FullMemoryAssistResult.Resolved) PublishCanonicalX87FxProof(frame, in proof);
        return result;
    }

    private FullMemoryAssistResult InspectNativeTicketWithFxProof(Worker worker,
        in FullTranslationJit.Plan plan, FullBlockFrame* frame, out byte* first, out byte* second,
        out uint firstLength, out nint page0, out nint page1, out FullStackMemoryDescriptor proof)
    {
        proof = default;
        first = second = null; firstLength = 0; page0 = page1 = 0;
        if (NativeCanonicalX87FxData(in plan, frame, worker.Machine, out var instruction))
        {
            var result = CheckCanonicalX87FxPermissions(worker, in instruction, frame, out proof);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        return InspectNativeTicket(worker, in plan, frame, out first, out second,
            out firstLength, out page0, out page1);
    }

    private FullMemoryAssistResult MaterializeNativeMemoryForPlan(Worker worker,
        in FullTranslationJit.Plan plan, FullBlockFrame* frame)
    {
        if (FullNativeMemory.IsCanonicalX87MemoryProbe(frame) ||
            !NativeCanonicalX87FxData(in plan, frame, worker.Machine, out _))
            return MaterializeNativeMemory(worker, frame);
        var proof = CreateCanonicalX87FxProof(frame);
        var local = CreateCanonicalX87FxProofFrame(frame, &proof);
        var result = MaterializeNativeMemory(worker, &local);
        if (result == FullMemoryAssistResult.GuestFault) CopyCanonicalX87ProofFault(in local, frame);
        return result;
    }
}
