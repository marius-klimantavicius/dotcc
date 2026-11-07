using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    private static void CopyMemoryRequest(in FullBlockFrame source, ref FullBlockFrame target, uint index)
    {
        target.MemoryAddress = source.MemoryAddress;
        target.MemoryAccess = source.MemoryAccess;
        target.MemoryWidth = source.MemoryWidth;
        target.MemorySite = index;
        target.MemoryResume = 1;
        target.FirstPreissued = 0;
    }
    private static bool SameNativeInstruction(in FullTranslationJit.Plan left, in FullTranslationJit.Plan right)
    {
        var a = left.Decoded; var b = right.Decoded;
        return left.Ip == right.Ip && a.length == b.length &&
            new ReadOnlySpan<byte>(a.bytes, a.length).SequenceEqual(new(b.bytes, b.length));
    }
    partial void FullControllerMemoryMaterializing(FullBlockFrame* frame, bool finalCheckOnly);
    partial void FullControllerMemoryMaterialized(FullBlockFrame* frame, bool finalCheckOnly);
    private FullMemoryAssistResult MaterializeNativeMemory(Worker worker, FullBlockFrame* request)
    {
        // Ordinary destinations keep the existing conservative writable resolver.
        // ENTER final-only probes use an explicit no-writer lookup: they check
        // U/RW and backing without a writable pointer escape or code revocation.
        // Both routes only establish mappings; the native operation is rechecked.
        using var data = new FullDataScope(this, worker);
        var m = worker.Machine;
        var previousNoFault = m->nofault; var previousCode = m->segvcode;
        m->nofault = false; // PAGE_RSRV commitment is explicitly authorized here.
        try
        {
            program!.ResetTlb(m);
            bool finalCheckOnly = FullNativeMemory.IsCanonicalX87MemoryProbe(request) ||
                FullNativeMemory.IsEnterRequest(request) &&
                request->StackMemory->Count == 2 &&
                request->StackMemory->Request0.Address != request->StackMemory->Request1.Address &&
                request->MemoryAddress == request->StackMemory->Request1.Address &&
                request->MemoryWidth == request->StackMemory->Request1.Width &&
                request->MemoryAccess == FullMemoryAccess.Write;
            FullControllerMemoryMaterializing(request, finalCheckOnly);
            if (LookupNativeMaterialization(m, request->MemoryAddress, finalCheckOnly, request->MemoryAccess) == null)
                return FullNativeMemory.Fault(request, request->MemoryAddress, m->segvcode == 0 ? 1u : unchecked((uint)m->segvcode));
            ulong last = unchecked(request->MemoryAddress + request->MemoryWidth / 8 - 1);
            if ((last >> 12) != (request->MemoryAddress >> 12) &&
                LookupNativeMaterialization(m, last, finalCheckOnly, request->MemoryAccess) == null)
                return FullNativeMemory.Fault(request, last & ~4095UL, m->segvcode == 0 ? 1u : unchecked((uint)m->segvcode));
            FullControllerMemoryMaterialized(request, finalCheckOnly);
            return FullMemoryAssistResult.Resolved;
        }
        finally { m->nofault = previousNoFault; m->segvcode = previousCode; }
    }
    private byte* LookupNativeMaterialization(Blink.Machine* machine, ulong address, bool finalCheckOnly, FullMemoryAccess access)
    {
        if (!finalCheckOnly) return program!.LookupAddress2(machine, unchecked((long)address), 0, 0);
        ulong permission = (ulong)Blink.PAGE_U;
        if ((access & FullMemoryAccess.Write) != 0) permission |= (ulong)Blink.PAGE_RW;
        return program!.BlinkGuestLookupAddressCopy(machine, unchecked((long)address), permission, permission);
    }
    private static bool NativeMaskedStore(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null || plan.Operation != 0x1f7) return false;
        var decoded = plan.Decoded;
        return FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out instruction) &&
            FullBlockEmitter.CanInlineMaskedStore(instruction);
    }
    private static bool NativeEnter(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null || plan.Operation != 0x0c8) return false;
        var decoded = plan.Decoded;
        return FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out instruction) &&
            FullBlockEmitter.CanInlineEnter(instruction);
    }
    private static bool NativeCanonicalX87Memory(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null) return false;
        var decoded = plan.Decoded;
        return FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out instruction) &&
            (FullBlockEmitter.CanInlineCanonicalX87Memory(instruction) ||
             FullBlockEmitter.CanInlineCanonicalX87MemoryArithmetic(instruction) ||
             FullBlockEmitter.CanInlineCanonicalX87Control(instruction));
    }
    private static bool NativeNearCall(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        instruction = default;
        if (plan.Helper != null) return false;
        var decoded = plan.Decoded;
        return FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out instruction) &&
            instruction.Operation == JitOperation.IndirectCall && instruction.Source.IsMemory &&
            FullBlockEmitter.CanInlineNearControl(instruction);
    }

    private static void AuthenticateNativeMemoryDescriptor(in FullTranslationJit.Plan plan, FullBlockFrame* frame, Blink.Machine* machine)
    {
        if (NativeNearCall(in plan, out var nearCall))
        {
            if (!FullNativeMemory.AuthenticateNearCall(frame, in nearCall, machine))
                throw new InvalidOperationException("Near CALL pair differs from its exact plan/state.");
        }
        else if (NativeAtomic(in plan, out var atomic))
        {
            if (!FullNativeMemory.AuthenticateAtomicRequest(frame, in atomic, machine))
                throw new InvalidOperationException("Atomic memory request differs from its exact plan/state.");
        }
        else if (FullNativeMemory.IsCanonicalX87MemoryProbe(frame))
        {
            if (!NativeCanonicalX87Memory(in plan, out var store) ||
                !FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(frame, in store, machine))
                throw new InvalidOperationException("x87 permission proof differs from its authenticated store plan/state.");
        }
        else if (NativeMaskedStore(in plan, out var instruction))
        {
            if (!FullNativeMemory.AuthenticateMaskedStore(frame, in instruction, machine))
                throw new InvalidOperationException("Sparse native memory request differs from its authenticated plan/state.");
        }
        else if (NativeEnter(in plan, out instruction))
        {
            if (!FullNativeMemory.AuthenticateEnter(frame, in instruction, machine))
                throw new InvalidOperationException("ENTER native memory request differs from its authenticated plan/state.");
        }
        else if (frame->StackMemory != null && frame->StackMemory->Version != FullStackMemoryDescriptor.CurrentVersion)
            throw new InvalidOperationException("Non-sparse native operation supplied an unknown memory descriptor tag.");
    }

    private static bool NativeStringPair(in FullTranslationJit.Plan plan, out bool compare, out int width, out int sourceSegment)
    {
        compare = false; width = 0; sourceSegment = -1;
        if (plan.Helper != null || plan.Operation is not (0x0a4 or 0x0a5 or 0x0a6 or 0x0a7)) return false;
        var decoded = plan.Decoded;
        if (!FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out var instruction) ||
            !FullBlockEmitter.CanInlineString(instruction) || !FullStringLowering.IsPair(instruction)) return false;
        compare = instruction.Operation == JitOperation.StringCompare; width = instruction.Width;
        sourceSegment = instruction.Address.Segment;
        return true;
    }
    private static void AuthenticateStringPairWidth(FullBlockFrame* f, int width)
    {
        if (f->StackMemory == null || f->StackMemory->Count != 2 ||
            f->StackMemory->Request0.Width != (uint)width || f->StackMemory->Request1.Width != (uint)width)
            throw new InvalidOperationException("Full-JIT string descriptor differs from its authenticated native plan.");
    }
    private FullMemoryAssistResult InspectNativeTicket(Worker worker, in FullTranslationJit.Plan plan, FullBlockFrame* f,
        out byte* first, out byte* second, out uint firstLength, out nint page0, out nint page1)
    {
        first = second = null; page0 = page1 = 0;
        if (NativeAtomic(in plan, out _))
        {
            AuthenticateNativeMemoryDescriptor(in plan, f, worker.Machine);
            return FullNativeMemory.InspectAtomicTicket(program!, worker.Machine, f, fullJit!.Authority,
                out first, out second, out firstLength, out page0, out page1);
        }
        if (NativeMaskedStore(in plan, out _))
        {
            AuthenticateNativeMemoryDescriptor(in plan, f, worker.Machine);
            firstLength = 0;
            var maskedResult = FullNativeMemory.InspectMaskedStoreTicket(program!, worker.Machine, f,
                fullJit!.Authority, out page0, out page1);
            if (maskedResult == FullMemoryAssistResult.Resolved)
            {
                var sparse = (FullMaskedStoreDescriptor*)f->StackMemory;
                first = sparse->HostPage0; second = sparse->HostPage1;
            }
            return maskedResult;
        }
        if (NativeEnter(in plan, out _))
        {
            AuthenticateNativeMemoryDescriptor(in plan, f, worker.Machine); firstLength = 0;
            var enterResult = FullNativeMemory.InspectEnterTicket(program!, worker.Machine, f,
                fullJit!.Authority, out page0, out page1);
            if (enterResult == FullMemoryAssistResult.Resolved)
            {
                var destination = f->StackMemory->Request0;
                first = destination.Host0; second = destination.Host1; firstLength = destination.Length0;
            }
            return enterResult;
        }
        if (NativeStringPair(in plan, out bool compare, out int width, out int sourceSegment))
        {
            AuthenticateStringPairWidth(f, width); firstLength = 0;
            var stringResult = FullNativeMemory.InspectStringPairTicket(program!, worker.Machine, f,
                fullJit!.Authority, compare, out page0, out page1, sourceSegment);
            if (stringResult == FullMemoryAssistResult.Resolved)
            {
                var destination = f->StackMemory->Request1;
                first = destination.Host0; second = destination.Host1; firstLength = destination.Length0;
            }
            return stringResult;
        }
        if (NativeNearCall(in plan, out var nearCall))
        {
            AuthenticateNativeMemoryDescriptor(in plan, f, worker.Machine); firstLength = 0;
            var nearResult = FullNativeMemory.InspectNearCallTicket(program!, worker.Machine, f,
                fullJit!.Authority, in nearCall, out page0, out page1);
            if (nearResult == FullMemoryAssistResult.Resolved)
            {
                var destination = f->StackMemory->Request1;
                first = destination.Host0; second = destination.Host1; firstLength = destination.Length0;
            }
            return nearResult;
        }
        if (FullNativeMemory.IsStackRequest(f))
        {
            firstLength = 0;
            var stackResult = FullNativeMemory.InspectStackTicket(program!, worker.Machine, f,
                fullJit!.Authority, out page0, out page1);
            if (stackResult == FullMemoryAssistResult.Resolved)
            {
                var destination = f->StackMemory->Count == 2 ? f->StackMemory->Request1 : f->StackMemory->Request0;
                first = destination.Host0; second = destination.Host1; firstLength = destination.Length0;
            }
            return stackResult;
        }
        uint bytes = f->MemoryWidth / 8;
        firstLength = Math.Min(bytes, 4096u - (uint)(f->MemoryAddress & 4095));
        var result = InspectNativePage(worker, f, f->MemoryAddress, out byte* a);
        if (result != FullMemoryAssistResult.Resolved) return result;
        page0 = (nint)a;
        first = a + (int)(f->MemoryAddress & 4095);
        if (firstLength != bytes)
        {
            result = InspectNativePage(worker, f, unchecked((f->MemoryAddress & ~4095UL) + 4096), out byte* b);
            if (result != FullMemoryAssistResult.Resolved) return result;
            page1 = (nint)b; second = b;
        }
        return FullMemoryAssistResult.Resolved;
    }
    private FullMemoryAssistResult InspectNativePage(Worker worker, FullBlockFrame* f, ulong address, out byte* host)
    {
        host = null;
        if (!JitPageIdentity.TryRead(program!, worker.Machine, address, out ulong pte) ||
            (pte & (ulong)Blink.PAGE_V) == 0) return FullNativeMemory.Fault(f, address, 1);
        if ((pte & (ulong)Blink.PAGE_U) == 0 ||
            (f->MemoryAccess & FullMemoryAccess.Write) != 0 && (pte & (ulong)Blink.PAGE_RW) == 0)
            return FullNativeMemory.Fault(f, address, 2);
        if ((pte & (ulong)Blink.PAGE_RSRV) != 0) return FullMemoryAssistResult.DemandPage;
        host = (pte & (ulong)Blink.PAGE_HOST) != 0 ? program!.FindHostPage(pte) :
            program!.GetPageAddress(worker.Machine->blink_host_system, pte, false);
        if (host == null || program.BlinkHostMemoryContains(host, 4096) == 0)
            return FullMemoryAssistResult.UnsupportedBacking;
        if ((pte & (ulong)Blink.PAGE_HOST) != 0 && program.BlinkHostMemoryFilePageAvailable(host) == 0)
            return FullNativeMemory.Fault(f, address, unchecked((uint)-2));
        return FullMemoryAssistResult.Resolved;
    }
    // Issued REP has its own immutable IR/request authority. In particular it
    // must never enter the ordinary Plan/stack grammar or reacquire its opcode.
    private FullMemoryAssistResult InspectRepTicket(Worker worker, in FullIssuedRep issuedRep, FullRepState* state,
        in FullRepRequest request, FullBlockFrame* frame, out byte* first, out byte* second,
        out uint firstLength, out nint page0, out nint page1)
    {
        ValidateRepMapping(in issuedRep, state, in request, frame);
        first = second = null; page0 = page1 = 0; firstLength = 0;
        var instruction = issuedRep.Instruction;
        if (instruction.IsPair)
        {
            var result = FullNativeMemory.InspectStringPairTicket(program!, worker.Machine, frame,
                fullJit!.Authority, instruction.Compares, out page0, out page1, instruction.SourceSegment);
            if (result == FullMemoryAssistResult.Resolved)
            {
                var destination = frame->StackMemory->Request1;
                first = destination.Host0; second = destination.Host1; firstLength = destination.Length0;
            }
            return result;
        }
        uint bytes = frame->MemoryWidth / 8;
        firstLength = Math.Min(bytes, 4096u - (uint)(frame->MemoryAddress & 4095));
        var scalarResult = InspectNativePage(worker, frame, frame->MemoryAddress, out byte* a);
        if (scalarResult != FullMemoryAssistResult.Resolved) return scalarResult;
        page0 = (nint)a; first = a + (int)(frame->MemoryAddress & 4095);
        if (firstLength != bytes)
        {
            scalarResult = InspectNativePage(worker, frame, unchecked((frame->MemoryAddress & ~4095UL) + 4096), out byte* b);
            if (scalarResult != FullMemoryAssistResult.Resolved) return scalarResult;
            page1 = (nint)b; second = b;
        }
        return FullMemoryAssistResult.Resolved;
    }
    // Called after successful endpoint inspection, before ticket promotion.
    // The native-produced summary alone cannot classify compound operations:
    // CALL, PUSH/POP pairs, MOVS, sparse stores and ENTER may name a read first.
    // Permission-only x87/ENTER probes can never acquire a data ticket.
    private static bool NativeTicketWritesContent(in FullTranslationJit.Plan plan, FullBlockFrame* f, Blink.Machine* m)
    {
        AuthenticateNativeMemoryDescriptor(in plan, f, m);
        if (FullNativeMemory.IsCanonicalX87MemoryProbe(f))
            throw new InvalidOperationException("Permission-only proof cannot acquire a data ticket.");
        if (NativeAtomic(in plan, out _) || NativeNearCall(in plan, out _) ||
            NativeMaskedStore(in plan, out _) || NativeEnter(in plan, out _)) return true;
        if (NativeStringPair(in plan, out bool compare, out int width, out int segment))
            return FullNativeMemory.StringPairTicketWritesContent(f, compare, width, segment);
        if (NativeCanonicalX87FxData(in plan, f, m, out _))
            return f->MemoryAccess != FullMemoryAccess.Read;
        if (FullNativeMemory.IsStackRequest(f))
        {
            var decoded = plan.Decoded;
            if (!FullTranslationJit.TryClassifyNative(plan.Ip, new(decoded.bytes, decoded.length), out var instruction))
                throw new InvalidOperationException("Stack ticket lacks an immutable native plan.");
            return FullNativeMemory.StackTicketWritesContent(f, in instruction, m);
        }
        if (f->Machine != m || m->ip != plan.Ip || f->MemoryResume != 1 || f->MemorySite != f->ResumeIndex ||
            !FullNativeMemory.ValidMemoryWidth(f->MemoryWidth) || (uint)f->MemoryAccess is < 1 or > 3 ||
            f->StackMemory != null && (f->StackMemory->Version != FullStackMemoryDescriptor.CurrentVersion ||
                f->StackMemory->Count != 0))
            throw new InvalidOperationException("Ordinary ticket lacks an authenticated single data request.");
        // Same issued-plan/native request authority as R201. All Ready pointer
        // and ticket checks remain in the emitted operation after enrollment.
        return f->MemoryAccess != FullMemoryAccess.Read;
    }

    private void RevokeNativeTicketPages(nint page0, nint page1, bool writesContent, bool post)
        => fullJit!.TicketSourceEdge(page0, page1, writesContent, post);
}
