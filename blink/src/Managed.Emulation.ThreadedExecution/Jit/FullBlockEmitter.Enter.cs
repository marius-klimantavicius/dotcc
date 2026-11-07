using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineEnter(JitInstruction i) => i.Operation == JitOperation.Enter;

    internal static bool ValidateEnter(JitInstruction i)
    {
        if (!CanInlineEnter(i)) return false;
        if (i.Length is < 4 or > 15 || i.Width is not (16 or 64) || i.SourceWidth != i.Width ||
            i.Destination != default || i.Source != default || i.Target != 0 || i.Condition != 0 ||
            i.Options != 0 || i.Extra != new JitOperand(0, i.Extra.Immediate, IsImmediate: true) ||
            i.Extra.Immediate > ushort.MaxValue || i.Address != new JitAddress(4, -1, 0, 0))
            throw new ArgumentException("Native ENTER canonical metadata");
        if (!EnterLayout.Supported) throw new PlatformNotSupportedException("Native ENTER ABI6 layout");
        return true;
    }

    private static class EnterLayout { internal static readonly bool Supported = HasExpectedEnterLayout(); }

    internal static unsafe bool HasExpectedEnterLayout()
    {
        Blink.Machine m = default; byte* b = (byte*)&m;
        return HasExpectedLayout() && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(FullBlockFrame) == 416 && sizeof(TranslationBlockHeader) == 160 &&
            sizeof(FullStackMemoryDescriptor) == 128 && sizeof(FullStackMemoryRequest) == 48 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)).ToInt32() == 384 &&
            Marshal.OffsetOf<FullStackMemoryDescriptor>(nameof(FullStackMemoryDescriptor.Request0)).ToInt32() == 32 &&
            Marshal.OffsetOf<FullStackMemoryDescriptor>(nameof(FullStackMemoryDescriptor.Request1)).ToInt32() == 80 &&
            // HasExpectedLayout includes X64BlockEmitter's selected pure-MOV
            // placement proof for ALL sixteen GPRs, including RSP56/RBP64.
            // Those buffers belong to anonymous aggregates, not Machine.sp/bp.
            (byte*)&m.flags - b == 12;
    }

    private ref partial struct Writer
    {
        private bool TryEnterOperation(in JitInstruction i)
        {
            if (!CanInlineEnter(i)) return false;
            ValidateEnter(i);
            NativeEnterPrepare(i);
            // All mapping, endpoint and ticket checks have completed. Old BP
            // is captured before either guest register changes. Only this
            // emitted store writes guest bytes; final-RSP probe never does.
            NativeStackMemoryAdopt(32);
            IntegerLoad(new(5, 0), i.Width, 8); NativeMemoryStore(i.Width);
            IntegerFrameLoad(184, 8); Store(new(5, 0), i.Width);
            IntegerFrameLoad(192, 0); Bytes(0x49, 0x89, 0x43, 56);
            NativeMemoryCount(); return true; // core owns exactly one Commit
        }

        private void NativeEnterCanonical(JitInstruction i, bool final)
        {
            int displacement = -i.Width / 8 - (final ? (int)i.Extra.Immediate : 0);
            ScalarMiscCheckCanonical(i with { Address = new(4, -1, 0, displacement, Segment: 2) }, i.Width);
        }

        private void NativeEnterSummary(JitInstruction i)
        {
            IntegerFrameLoad(184, 0); IntegerFrameStore(216, 0);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x82); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)FullMemoryAccess.Write);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32((uint)i.Width);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
        }

        private void NativeEnterPrepare(JitInstruction i)
        {
            Load(new(4, 0), 64, 0); IntegerConstant(5, 0, (uint)(i.Width / 8), 64);
            IntegerFrameStore(184, 0); // frameTemp; full stack-address64
            IntegerConstant(5, 0, (uint)i.Extra.Immediate, 64); IntegerFrameStore(192, 0);
            NativeStackMemoryDescriptor();
            // A Count0 denial/cancel ALWAYS stages afresh even if Ready remains.
            Bytes(0x41, 0x81, 0x39); U32(FullEnterLowering.MemoryTag);
            int wrongTag = NearCondition(0x85);
            Bytes(0x41, 0x83, 0x79, 4, 2); int wrongCount = NearCondition(0x85);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.Ready);
            int ready = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ticket = NearCondition(0x84);
            Patch(wrongTag, position); Patch(wrongCount, position);
            NativeEnterSummary(i);
            NativeStackMemoryDword(0, FullEnterLowering.MemoryTag); NativeStackMemoryDword(4, 2);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x41, 8);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request); NativeStackMemoryZeroPointer(24);
            NativeStackMemoryStage(32, 184, FullMemoryAccess.Write, i.Width);
            NativeStackMemoryStage(80, 192, FullMemoryAccess.Write, i.Width);
            NativeStackMemoryDword(68, 0); NativeStackMemoryZeroPointer(72);
            NativeStackMemoryDword(116, 0); NativeStackMemoryZeroPointer(120);
            // Complete descriptor BEFORE either miss. Fault order is actual
            // push then final allocation endpoint; there is no span over alloc.
            NativeEnterCanonical(i, false); NativeStackMemoryProbe(32, FullMemoryAccess.Write, i.Width);
            NativeEnterCanonical(i, true);
            // The ordinary writable TLB permission also conveys actual-store
            // authority. A probe must NOT require that authority or revoke a
            // watched final page. Request the mapping-only PTE/RW check even
            // on a warm cache; no new permission bit or frame field is needed.
            IntegerFrameLoad(192, 0); IntegerFrameStore(216, 0);
            JumpExit(FullBlockExit.NeedMemory);

            Patch(ready, position); Patch(ticket, position);
            NativeEnterSummary(i);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x3b, 0x41, 8);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            NativeStackMemoryValidate(32, 184, FullMemoryAccess.Write, i.Width);
            NativeStackMemoryValidate(80, 192, FullMemoryAccess.Write, i.Width);
            Bytes(0x49, 0x8b, 0x41, 16, 0x49, 0x3b, 0x82); U32(136);
            int staleObserved = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(128); IntegerRR(0x85, 2, 2);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x02); int current = NearCondition(0x84);
            Patch(staleObserved, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request); NativeStackMemoryZeroPointer(24);
            JumpExit(FullBlockExit.NeedMemory);
            Patch(current, position);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.TicketReady);
            int ordinary = NearCondition(0x85);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            int authorized = NativeNearJump();
            Patch(ordinary, position);
            Bytes(0x49, 0x83, 0x79, 24, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Patch(authorized, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Consumed); NativeStackMemoryZeroPointer(24);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
        }
    }
}

// Mapping-only adapter in the same NEW owned source file. No decode, Op*, data
// load/store, flags, IP, RSP or BP computation. Parent wires explicit tag and
// authenticated-plan dispatch; unknown tags continue to fail closed.
internal static unsafe partial class FullNativeMemory
{
    internal static bool IsEnterRequest(FullBlockFrame* f) => f != null && f->StackMemory != null &&
        f->StackMemory->Version == FullEnterLowering.MemoryTag;

    internal static bool AuthenticateEnter(FullBlockFrame* f, in JitInstruction i, Blink.Machine* m)
    {
        if (f == null || m == null || f->Machine != m || !IsEnterRequest(f) || !FullBlockEmitter.CanInlineEnter(i)) return false;
        try { FullBlockEmitter.ValidateEnter(i); } catch (ArgumentException) { return false; }
        var d = f->StackMemory;
        ulong push = unchecked(((ulong*)((byte*)m + 56))[0] - (uint)(i.Width / 8));
        return d->Count == 2 && d->Site == f->ResumeIndex && m->ip == i.Ip &&
            d->State is >= FullStackMemoryState.Request and <= FullStackMemoryState.TicketReady &&
            d->Request0.Address == push && d->Request0.Access == FullMemoryAccess.Write && d->Request0.Width == (uint)i.Width &&
            d->Request1.Address == unchecked(push - i.Extra.Immediate) &&
            d->Request1.Access == FullMemoryAccess.Write && d->Request1.Width == (uint)i.Width;
    }

    private static void ValidateEnterRequest(FullBlockFrame* f)
    {
        if (!IsEnterRequest(f) || f->StackMemory->Count != 2 || f->StackMemory->State != FullStackMemoryState.Request ||
            f->StackMemory->Site != f->ResumeIndex || f->MemoryResume != 1 || f->MemorySite != f->ResumeIndex ||
            f->StackMemory->Request0.Access != FullMemoryAccess.Write || f->StackMemory->Request0.Width is not (16 or 64) ||
            f->StackMemory->Request1.Access != FullMemoryAccess.Write || f->StackMemory->Request1.Width != f->StackMemory->Request0.Width)
            throw new InvalidOperationException("Invalid ENTER mapping request");
    }

    private static FullMemoryAssistResult ResolveEnterRequest(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority a, ref FullStackMemoryRequest r, ulong epoch, bool permission, ref bool barrier)
    {
        ulong last = unchecked(r.Address + r.Width / 8 - 1);
        f->MemoryAddress = r.Address; f->MemoryAccess = r.Access; f->MemoryWidth = r.Width;
        if (last < r.Address || !Canonical(r.Address) || !Canonical(last))
        {
            f->FaultTrap = 12; f->FaultCode = 128; f->FaultAddress = 0; f->MemoryResume = 0;
            return FullMemoryAssistResult.GuestFault;
        }
        return ResolveStackRequest(p, m, f, a, ref r, epoch, permission, ref barrier);
    }

    internal static FullMemoryAssistResult TryFillEnter(Blink p, Blink.Machine* m, FullBlockFrame* f, FullCodeAuthority a)
    {
        if (!Monitor.IsEntered(a.Gate)) throw new InvalidOperationException("ENTER mapping needs authority gate");
        ValidateEnterRequest(f); ++f->MemoryAssists;
        var value = *f->StackMemory; ulong epoch = a.Epoch; bool barrier = false;
        var result = ResolveEnterRequest(p, m, f, a, ref value.Request0, epoch, true, ref barrier);
        if (result != FullMemoryAssistResult.Resolved) return result;
        // Permission/backing check ONLY; never announce physical write intent
        // or revoke aliases of an endpoint which will not actually be stored.
        result = ResolveEnterRequest(p, m, f, a, ref value.Request1, epoch, false, ref barrier);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (a.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (barrier) return FullMemoryAssistResult.WriteBarrier;
        PublishStackTlb(f, in value.Request0, epoch);
        value.Epoch = epoch; value.Ticket = 0; value.State = FullStackMemoryState.Ready;
        *f->StackMemory = value; f->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    internal static FullMemoryAssistResult InspectEnterTicket(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority a, out nint page0, out nint page1)
    {
        ValidateEnterRequest(f); page0 = page1 = 0;
        var value = *f->StackMemory; ulong epoch = a.Epoch; bool ignored = false;
        var result = ResolveEnterRequest(p, m, f, a, ref value.Request0, epoch, false, ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        result = ResolveEnterRequest(p, m, f, a, ref value.Request1, epoch, false, ref ignored);
        if (result != FullMemoryAssistResult.Resolved) return result;
        if (a.Epoch != epoch || f->ObservedGlobalEpoch != epoch) return FullMemoryAssistResult.Retry;
        page0 = (nint)(value.Request0.Host0 - (int)(value.Request0.Address & 4095));
        page1 = (nint)value.Request0.Host1; *f->StackMemory = value;
        return FullMemoryAssistResult.Resolved;
    }

    internal static void PublishEnterTicket(FullBlockFrame* f, ulong epoch, ulong ticket)
    {
        ValidateEnterRequest(f);
        if (ticket == 0 || epoch != f->ObservedGlobalEpoch) throw new InvalidOperationException("Invalid ENTER ticket");
        f->StackMemory->Epoch = epoch; f->StackMemory->Ticket = ticket; f->StackMemory->State = FullStackMemoryState.TicketReady;
        f->MemoryEpoch = epoch; f->MemoryFlags = FullMemoryFlags.BarrierTicket;
    }
}
