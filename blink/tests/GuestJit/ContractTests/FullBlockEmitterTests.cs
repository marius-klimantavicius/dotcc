using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // Parent integration calls this from Main. These tests deliberately use
    // independently owned native allocations instead of the store; store lease,
    // reclamation, W^X and authority-barrier tests belong to their owners.
    // One owner fixture, never one common allocation per TB. Cleared before
    // disposal, after every body allocation in the selected contract group died.
    static FullSharedScaffold fullOwnerScaffold;

    static void CheckFullBlockEmitter(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedLayout(), "Full frame/header/register ABI layout");
        Check(sizeof(FullBlockFrame) == FullBlockFrame.ByteSize && sizeof(JitFrame) == 96 &&
            (uint)FullBlockExit.Invalidated == (uint)JitExit.MemoryMiss &&
            FullBlockFrame.AbiVersion != JitFrame.AbiVersion, "Full ABI distinguishes Hybrid exit value 4");
        Check(!fullOwnerScaffold.IsValid, "Full scaffold fixture starts cleared");
        CheckFullNativeContracts(machine); // every historical standalone body group
        using (var common = CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold = addresses;
            try { CheckFullNativeContracts(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckFullEmitterEncodingAndAllocation();
        CheckFullOversizeAndRetry();
        CheckFullSharedScaffoldingSize(); // historical standalone byte contracts
        CheckFullOwnerScaffoldEncodingAndAllocation();
        CheckFullOwnerScaffoldReuse(machine);
    }

    static void CheckFullNativeContracts(byte* machine)
    {
        CheckFullMixedHelperResume(machine);
        CheckFullHelperOnlyModes(machine);
        CheckFullSharedHelperResume(machine);
        CheckFullBoundaryPrecedence(machine);
        CheckFullAuthorityGuards(machine);
        CheckFullInstructionBoundaryGuards(machine);
        CheckFullTailChains(machine);
        CheckFullLinkAuthentication(machine);
        CheckFullConditionalLinks(machine);
        CheckFullScalarEquivalence(machine);
        CheckFullWrongBodyEntry(machine);
        CheckFullShapeEquivalence(machine);
        CheckFullLargeResumeAndGuards(machine);
        CheckFullLargeInternalLoop(machine);
    }

    static NativeCodeAllocation CreateFullScaffold(out FullSharedScaffold addresses)
    {
        byte[] staging = new byte[FullBlockEmitter.ScaffoldCapacity];
        int length = FullBlockEmitter.EmitScaffold(staging, out var layout);
        Check(length == layout.CodeLength && length <= FullBlockEmitter.ScaffoldCapacity,
            "one owner slab fits the advertised staging bound");
        Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0, length), out var allocation, out string reason), reason);
        addresses = layout.Resolve(allocation!.Entry);
        return allocation!;
    }

    static NativeCodeAllocation CreateFullBlock(ReadOnlySpan<FullBlockInstruction> instructions)
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity];
        int length = fullOwnerScaffold.IsValid ? FullBlockEmitter.Emit(instructions, staging, in fullOwnerScaffold) :
            FullBlockEmitter.Emit(instructions, staging);
        Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0, length), out var allocation, out string reason), reason);
        return allocation!;
    }

    static TranslationBlockHeader FullHeader(ulong pc, nint entry, ulong generation = 17) => new()
    {
        OwnerCookie = 73, BodyGeneration = generation, GuestPc = pc, Entry = entry,
        Context = 0x1234, Live = 1, CodeEpoch = 9, ValidatedCodeEpoch = 9, ValidatedGlobalEpoch = 11
    };

    static FullBlockFrame FullFrame(byte* machine, TranslationBlockHeader* header,
        ulong* global, long* issued, long* completed, int* stop, ulong budget = 100, uint limit = 0, ulong* codeEpoch = null) => new()
    {
        Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame), Machine = machine,
        Issued = issued, Completed = completed, Stop = stop, Budget = budget, NativeLimit = limit,
        Header = header, OwnerCookie = header->OwnerCookie, BodyGeneration = header->BodyGeneration,
        Context = header->Context, GlobalEpochPointer = global, ObservedGlobalEpoch = *global,
        CodeAuthorityEpochPointer = codeEpoch == null ? global : codeEpoch,
        ObservedCodeAuthorityEpoch = header->ValidatedGlobalEpoch,
        HelperIndex = FullBlockFrame.NoHelperIndex
    };

    static void ResetFull(byte* machine, ulong pc = Ip)
    {
        Reset(machine); *(ulong*)machine = pc;
        ((Blink.Machine*)machine)->mode.omode = Blink.XED_MODE_LONG;
        ((Blink.Machine*)machine)->mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
    }

    static FullBlockExit RunFull(NativeCodeAllocation code, FullBlockFrame* frame)
    {
        frame->NextIp = ulong.MaxValue; // output cannot leak the transient root, even on prologue failure
        ulong beforeExecuted = frame->Executed;
        int result = ((delegate* unmanaged<FullBlockFrame*, int>)code.Entry)(frame);
        RecordQualificationNative(frame, beforeExecuted);
        Check(frame->NextIp == *(ulong*)frame->Machine, "every Full leaf exit materializes NextIp from actual machine IP");
        Check(result == (int)frame->Exit, "Full native return agrees with typed frame exit");
        return frame->Exit;
    }

    static void CheckFullMixedHelperResume(byte* machine)
    {
        FullBlockInstruction[] body = [
            new(new(Ip, 5, JitOperation.Mov, 64, new(0, 0), new(0, 10, true))),
            FullBlockInstruction.Helper(Ip + 5, 3),
            new(new(Ip + 8, 4, JitOperation.Add, 64, new(0, 0), new(0, 5, true))),
            new(new(Ip + 12, 1, JitOperation.Nop, 64, default, default)) ];
        using var code = CreateFullBlock(body);
        foreach (uint preissued in new uint[] { 0, 1 })
        for (ulong budget = preissued; budget <= 5; ++budget)
        foreach (string poll in new[] { "none", "stop", "attention" })
        {
            ResetFull(machine); if (poll == "attention") machine[11] = 1;
            ulong global = 11; long issued = preissued, completed = 0; int stop = poll == "stop" ? 1 : 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop, budget);
            frame.FirstPreissued = preissued;
            byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            FullBlockExit exit = RunFull(code, &frame);
            FullBlockExit expected = poll == "stop" ? FullBlockExit.Stop : poll == "attention" ? FullBlockExit.Attention :
                budget < 2 ? FullBlockExit.Budget : FullBlockExit.NeedHelper;
            Check(exit == expected, "mixed inline/helper precise first exit");
            if (poll == "attention" && preissued == 1)
            {
                Check(issued == 1 && completed == 1 && frame.Executed == 1 && frame.FirstPreissued == 0 &&
                    frame.NextIp == Ip + 5 && *(ulong*)(machine + 24) == 10 && machine[8] == 0,
                    "approved first instruction executes before attention without losing its last budget issue");
                continue;
            }
            if (poll != "none")
            {
                Check(issued == preissued && completed == 0 && frame.Executed == 0 && frame.FirstPreissued == preissued,
                    "mixed cold/hot stop or attention precedes issue and preserves cold issue");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), "mixed poll preserves complete machine");
                continue;
            }
            if (budget == 0)
            {
                Check(issued == 1 && completed == 0 && frame.Executed == 0 &&
                    frame.NextIp == Ip && frame.HelperIndex == FullBlockFrame.NoHelperIndex,
                    "mixed exhausted first issue is rejected without execution");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), "mixed rejected first issue preserves machine");
                continue;
            }
            Check(issued == 2 && completed == 1 && frame.Executed == 1 && *(ulong*)(machine + 24) == 10 &&
                frame.FirstPreissued == 0, "mixed native prefix completes once and consumes cold issue once");
            if (budget == 1)
            {
                Check(frame.NextIp == Ip + 5 && machine[8] == 0 && frame.HelperIndex == FullBlockFrame.NoHelperIndex,
                    "budget before helper reserves rejected attempt but does not stage operation");
                continue;
            }
            Check(frame.HelperIndex == 1 && frame.ResumeIndex == 2 && frame.NextIp == Ip + 8 && machine[8] == 3,
                "typed helper exit identifies payload instruction and actual resume label");
            // Simulate a successful operation-specific helper AFTER returning
            // fully from native. Production additionally drops/reacquires epochs.
            *(ulong*)(machine + 24) += 100; machine[8] = 0; ++completed;
            FullBlockFrame resumed = FullFrame(machine, &header, &global, &issued, &completed, &stop, 100, 30);
            resumed.ResumeIndex = frame.ResumeIndex;
            Check(RunFull(code, &resumed) == FullBlockExit.Continue && issued == 4 && completed == 4 &&
                resumed.Executed == 2 && resumed.NextIp == Ip + 13 && *(ulong*)(machine + 24) == 115 && machine[8] == 0,
                "same emitted mixed TB resumes past helper without replaying prefix or helper");
            Check(resumed.Header == &header && resumed.BodyGeneration == 17, "helper resume retains exact body token");
        }
        // SignalActor-style cap returns before reaching the next helper.
        ResetFull(machine);
        ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
        TranslationBlockHeader h = FullHeader(Ip, code.Entry);
        FullBlockFrame one = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped, limit: 1);
        Check(RunFull(code, &one) == FullBlockExit.Continue && issue == 1 && complete == 1 &&
            one.Executed == 1 && one.NextIp == Ip + 5, "mixed cap one stops before helper issue");

        ResetFull(machine); issue = complete = 0;
        FullBlockFrame prefix = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped, limit: 3);
        Check(RunFull(code, &prefix) == FullBlockExit.NeedHelper && prefix.Executed == 1, "logical cap prefix reaches helper");
        *(ulong*)(machine + 24) += 100; machine[8] = 0; ++complete;
        FullBlockFrame remaining = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped, limit: 1);
        remaining.ResumeIndex = prefix.ResumeIndex;
        Check(RunFull(code, &remaining) == FullBlockExit.Continue && issue == 3 && complete == 3 && remaining.Executed == 1 &&
            remaining.NextIp == Ip + 12, "parent-provided remaining logical cap survives helper reacquisition");
    }

    static void CheckFullHelperOnlyModes(byte* machine)
    {
        using var helper = CreateFullBlock([FullBlockInstruction.Helper(Ip, 2), FullBlockInstruction.Helper(Ip + 2, 3)]);
        foreach (byte mode in new byte[] { 0, 1, 2 })
        foreach (uint flag in new uint[] { 0, 1u << Blink.FLAGS_TF, 1u << Blink.FLAGS_RF })
        foreach (uint preissued in new uint[] { 0, 1 })
        foreach (string poll in new[] { "none", "stop", "attention", "budget" })
        {
            ResetFull(machine);
            ((Blink.Machine*)machine)->mode.omode = mode;
            ((Blink.Machine*)machine)->mode.genmode = 0;
            *(uint*)(machine + 12) |= flag; if (poll == "attention") machine[11] = 1;
            ulong global = 11; long issued = preissued, completed = 0; int stop = poll == "stop" ? 1 : 0;
            TranslationBlockHeader header = FullHeader(Ip, helper.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop,
                budget: poll == "budget" ? preissued : 100);
            frame.FirstPreissued = preissued;
            byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            FullBlockExit exit = RunFull(helper, &frame);
            bool stages = poll == "none" || (poll == "budget" || poll == "attention") && preissued == 1;
            FullBlockExit expected = poll == "stop" ? FullBlockExit.Stop : poll == "attention" && preissued == 0 ? FullBlockExit.Attention :
                stages ? FullBlockExit.NeedHelper : FullBlockExit.Budget;
            Check(exit == expected && completed == 0 && frame.Executed == 0, "helper-only modes/TF/RF and polls");
            Check(issued == (poll is "stop" or "attention" ? preissued : 1), "first helper issue including preissued and rejected budget");
            if (stages)
            {
                Check(frame.HelperIndex == 0 && frame.ResumeIndex == 1 && frame.FirstPreissued == 0 &&
                    frame.NextIp == Ip + 2 && machine[8] == 2, "first helper stages without native completion");
                BinaryPrimitives.WriteUInt64LittleEndian(before, Ip + 2); before[8] = 2;
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), "helper staging changes only IP/oplen");
            }
            else Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), "helper poll/budget preserves complete machine");
        }
        // An end-of-body resume is a managed dispatch, never a native label.
        ResetFull(machine, Ip + 5);
        ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
        TranslationBlockHeader h = FullHeader(Ip, helper.Entry);
        ResetFull(machine, Ip + 2);
        FullBlockFrame last = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped);
        last.ResumeIndex = 1;
        Check(RunFull(helper, &last) == FullBlockExit.NeedHelper && last.HelperIndex == 1 && last.ResumeIndex == 2 &&
            last.NextIp == Ip + 5 && issue == 1 && complete == 0, "last helper returns informational end index without completion");
        issue = 0;
        FullBlockFrame past = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped);
        past.ResumeIndex = 2;
        Check(RunFull(helper, &past) == FullBlockExit.Invalidated && issue == 0 && complete == 0,
            "end-of-helper-body resume cannot enter arbitrary code");
    }

    static void CheckFullSharedHelperResume(byte* machine)
    {
        FullBlockInstruction[] body = new FullBlockInstruction[8];
        for (int i = 0; i < body.Length; ++i)
            body[i] = (i & 1) == 0 ? FullBlockInstruction.Helper(Ip + (uint)i, 1) :
                new(new(Ip + (uint)i, 1, JitOperation.Nop, 64, default, default));
        using var code = CreateFullBlock(body);
        foreach (uint preissued in new uint[] { 0, 1 })
        for (ulong budget = preissued; budget <= 9; ++budget)
        {
            ResetFull(machine);
            byte initialOperationLength = machine[8];
            ulong global = 11; long issued = preissued, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop, budget);
            frame.FirstPreissued = preissued;
            FullBlockExit exit;
            int helpers = 0;
            while ((exit = RunFull(code, &frame)) == FullBlockExit.NeedHelper)
            {
                Check(frame.HelperIndex == (uint)(helpers * 2) && frame.ResumeIndex == frame.HelperIndex + 1 &&
                    frame.FirstPreissued == 0 && machine[8] == 1 && completed == helpers * 2 &&
                    frame.Executed == (ulong)helpers, "shared helper issue returns exact plan without native completion");
                // Managed typed-helper success, after the native leaf has RETed.
                machine[8] = 0; ++completed; ++helpers;
            }
            ulong expected = Math.Min(budget, (ulong)body.Length);
            Check(exit == (budget < (ulong)body.Length ? FullBlockExit.Budget : FullBlockExit.Continue) &&
                issued == (long)(budget < (ulong)body.Length ? budget + 1 : (ulong)body.Length) &&
                completed == (long)expected && frame.Executed == expected / 2 && frame.NextIp == Ip + expected &&
                helpers == (int)((expected + 1) / 2) && machine[8] == (expected == 0 ? initialOperationLength : 0),
                $"repeated helper resume: preissued={preissued} budget={budget} exit={exit} issued={issued} completed={completed} native={frame.Executed} ip={frame.NextIp:x} helpers={helpers} oplen={machine[8]}");
        }

        ResetFull(machine);
        ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
        TranslationBlockHeader h = FullHeader(Ip, code.Entry);
        FullBlockFrame capped = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped, limit: 3);
        capped.Executed = 2;
        Check(RunFull(code, &capped) == FullBlockExit.NeedHelper && capped.Executed == 2,
            "helper staging preserves already accumulated native cap usage");
        machine[8] = 0; ++complete;
        Check(RunFull(code, &capped) == FullBlockExit.Continue && capped.NativeLimit == 3 && capped.Executed == 3 &&
            issue == 2 && complete == 2 && capped.NextIp == Ip + 2 && capped.HelperIndex == FullBlockFrame.NoHelperIndex,
            "same-frame helper resume does not reset total cap or issue the next helper after reaching it");

        // A preapproved helper must run before attention, but its allowance is
        // gone when the managed helper returns and the next label is entered.
        foreach (string changed in new[] { "none", "stop", "attention", "live", "owner", "generation", "context", "root",
            "code", "validated-code", "global", "validated-global", "null-global", "null-header", "pc", "mode", "TF", "RF", "cap", "budget" })
        {
            ResetFull(machine); machine[11] = 1;
            ulong global = 11; long issued = 1, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop);
            frame.FirstPreissued = 1;
            Check(RunFull(code, &frame) == FullBlockExit.NeedHelper && issued == 1 && completed == 0 &&
                frame.Executed == 0 && frame.FirstPreissued == 0, "preapproved helper stages once before pending attention");
            machine[8] = machine[11] = 0; ++completed;
            switch (changed)
            {
                case "stop": stop = 1; break;
                case "attention": machine[11] = 1; break;
                case "live": header.Live = 0; break;
                case "owner": ++header.OwnerCookie; break;
                case "generation": ++header.BodyGeneration; break;
                case "context": ++header.Context; break;
                case "root": ++header.GuestPc; break;
                case "code": ++header.CodeEpoch; break;
                case "validated-code": ++header.ValidatedCodeEpoch; break;
                case "global": ++global; break;
                case "validated-global": ++header.ValidatedGlobalEpoch; break;
                case "null-global": frame.GlobalEpochPointer = null; break;
                case "null-header": frame.Header = null; break;
                case "pc": ++*(ulong*)machine; break;
                case "mode": machine[9] = 0; break;
                case "TF": *(uint*)(machine + 12) |= 1u << Blink.FLAGS_TF; break;
                case "RF": *(uint*)(machine + 12) |= 1u << Blink.FLAGS_RF; break;
                case "cap": frame.NativeLimit = 32; frame.Executed = frame.NativeLimit; break;
                case "budget": frame.Budget = 1; break;
            }
            byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            FullBlockExit expected = changed switch {
                "none" => FullBlockExit.NeedHelper, "stop" => FullBlockExit.Stop,
                "attention" => FullBlockExit.Attention, "cap" => FullBlockExit.Continue,
                "budget" => FullBlockExit.Budget, _ => FullBlockExit.Invalidated };
            Check(RunFull(code, &frame) == expected && frame.FirstPreissued == 0 &&
                issued == (changed == "none" ? 3 : changed == "budget" ? 2 : 1) &&
                completed == (changed == "none" ? 2 : 1) &&
                frame.Executed == (changed == "none" ? 1UL : changed == "cap" ? frame.NativeLimit : 0UL),
                "helper resume re-enters complete shared guards without replaying its approved issue: " + changed);
            if (changed != "none")
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before),
                    "helper resume rejection preserves architectural state: " + changed);
        }
    }

    static void CheckFullBoundaryPrecedence(byte* machine)
    {
        using var code = CreateFullBlock([
            new(new(Ip, 1, JitOperation.Nop, 64, default, default)),
            FullBlockInstruction.Helper(Ip + 1, 1) ]);
        foreach (uint resume in new uint[] { 0, 1 })
        foreach (uint preissued in new uint[] { 0, 1 })
        foreach (string first in new[] { "stop", "attention", "authority", "pc", "cap", "inline", "issue" })
        {
            ResetFull(machine, Ip + resume);
            ulong global = 11; long issued = preissued, completed = 0; int stop = first == "stop" ? 1 : 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop, budget: preissued, limit: 32);
            frame.ResumeIndex = resume; frame.FirstPreissued = preissued;
            // Every later stage also fails. The first applicable stage wins;
            // preapproval skips attention and issue budget, never other guards.
            if (first is "stop" or "attention") machine[11] = 1;
            if (first is "stop" or "attention" or "authority") header.Live = 0;
            if (first is "stop" or "attention" or "authority" or "pc") ++*(ulong*)machine;
            if (first is not ("inline" or "issue")) frame.Executed = frame.NativeLimit;
            if (first != "issue") machine[9] = 0;
            FullBlockExit expected = first switch {
                "stop" => FullBlockExit.Stop, "attention" when preissued == 0 => FullBlockExit.Attention,
                "attention" or "authority" or "pc" => FullBlockExit.Invalidated,
                "cap" => FullBlockExit.Continue, "inline" when resume == 0 => FullBlockExit.Invalidated,
                _ when preissued == 0 => FullBlockExit.Budget,
                _ => FullBlockExit.NeedHelper };
            // Approved inline NOP reaches the helper's attention-free ordinary
            // issue and exhausts budget. The first instruction still committed.
            if (first == "issue" && preissued == 1 && resume == 0) expected = FullBlockExit.Budget;
            byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            Check(RunFull(code, &frame) == expected, "shared boundary preserves poll/authority/PC/cap/eligibility/issue order: " + first);
            bool ranInline = first == "issue" && preissued == 1 && resume == 0;
            bool stagedHelper = expected == FullBlockExit.NeedHelper;
            bool rejectedIssue = expected == FullBlockExit.Budget;
            Check(issued == preissued + (rejectedIssue ? 1 : 0) && completed == (ranInline ? 1 : 0) &&
                frame.FirstPreissued == (ranInline || stagedHelper ? 0 : preissued),
                "shared boundary failures preserve preissue until the reached Issue consumes it");
            if (!ranInline && !stagedHelper)
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before),
                    "earlier boundary guard rejects before architectural effects");
        }
    }

    static void CheckFullAuthorityGuards(byte* machine)
    {
        FullBlockInstruction[] body = [
            new(new(Ip, 1, JitOperation.Nop, 64, default, default)),
            FullBlockInstruction.Helper(Ip + 1, 2),
            new(new(Ip + 3, 1, JitOperation.Inc, 64, new(0, 0), default)) ];
        using var code = CreateFullBlock(body);
        foreach (uint resume in new uint[] { 0, 1, 2 })
        foreach (string failure in new[] { "live", "owner", "generation", "context", "root", "code", "validated-code", "zero-code",
            "global", "validated-global", "null-global", "null-header", "pc", "resume", "abi", "old-abi", "size", "preissued" })
        {
            ResetFull(machine, body[(int)resume].Instruction.Ip);
            ulong global = 11; long issued = 0, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop);
            frame.ResumeIndex = resume;
            switch (failure)
            {
                case "live": header.Live = 0; break;
                case "owner": ++header.OwnerCookie; break;
                case "generation": ++header.BodyGeneration; break;
                case "context": ++header.Context; break;
                case "root": ++header.GuestPc; break;
                case "code": ++header.CodeEpoch; break;
                case "validated-code": ++header.ValidatedCodeEpoch; break;
                case "zero-code": header.CodeEpoch = header.ValidatedCodeEpoch = 0; break;
                case "global": ++global; break;
                case "validated-global": ++header.ValidatedGlobalEpoch; break;
                case "null-global": frame.GlobalEpochPointer = null; break;
                case "null-header": frame.Header = null; break;
                case "pc": ++*(ulong*)machine; break;
                case "resume": frame.ResumeIndex = FullBlockEmitter.MaximumInstructions; break;
                case "abi": frame.Version = JitFrame.AbiVersion; break;
                case "old-abi": frame.Version = 0x46420001; break;
                case "size": frame.Size = 96; break;
                case "preissued": frame.FirstPreissued = 2; break;
            }
            byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            Check(RunFull(code, &frame) == FullBlockExit.Invalidated && issued == 0 && completed == 0 && frame.Executed == 0,
                "inline/helper/resume authority rejects before issue: " + failure);
            Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), "authority rejection preserves entire machine");
        }
        foreach (string change in new[] { "mode", "generation-mode", "TF", "RF" })
        {
            ResetFull(machine);
            ulong global = 11; long issued = 0, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, &completed, &stop);
            if (change == "mode") machine[9] = 0;
            else if (change == "generation-mode") ((Blink.Machine*)machine)->mode.genmode = 0;
            else *(uint*)(machine + 12) |= 1u << (change == "TF" ? Blink.FLAGS_TF : Blink.FLAGS_RF);
            Check(RunFull(code, &frame) == FullBlockExit.Invalidated && issued == 0 && completed == 0,
                "inline dynamic mode/TF/RF eligibility guard: " + change);
        }
    }

    static void CheckFullInstructionBoundaryGuards(byte* machine)
    {
        using var code = CreateFullBlock([
            new(new(Ip, 1, JitOperation.Inc, 64, new(0, 0), default)),
            new(new(Ip + 1, 1, JitOperation.Inc, 64, new(0, 0), default)),
            FullBlockInstruction.Helper(Ip + 2, 1) ]);
        foreach (string changed in new[] { "live", "owner", "generation", "context", "root", "code", "validated-code", "global", "validated-global" })
        {
            ResetFull(machine); *(ulong*)(machine + 24) = 0;
            ulong global = 11; long issued = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            // Deterministic invalidation immediately after the first operation:
            // its atomic completion store changes one native authority field.
            // This avoids a timing-dependent racing writer in a contract test.
            long* completion = changed switch {
                "live" => (long*)&header.Live, "owner" => (long*)&header.OwnerCookie,
                "generation" => (long*)&header.BodyGeneration, "context" => (long*)&header.Context,
                "root" => (long*)&header.GuestPc, "code" => (long*)&header.CodeEpoch,
                "validated-code" => (long*)&header.ValidatedCodeEpoch,
                "validated-global" => (long*)&header.ValidatedGlobalEpoch, _ => (long*)&global };
            FullBlockFrame frame = FullFrame(machine, &header, &global, &issued, completion, &stop);
            Check(RunFull(code, &frame) == FullBlockExit.Invalidated && issued == 1 && frame.Executed == 1 &&
                frame.NextIp == Ip + 1 && *(ulong*)(machine + 24) == 1,
                "each native instruction rechecks authority after prior commit: " + changed);
        }
    }

    static void CheckFullTailChains(byte* machine)
    {
        const ulong secondPc = Ip + 16;
        using var first = CreateFullBlock([
            new(new(Ip, 1, JitOperation.Inc, 64, new(0, 0), default)),
            new(new(Ip + 1, 5, JitOperation.Jump, 64, default, default, Target: secondPc)) ]);
        using var second = CreateFullBlock([
            new(new(secondPc, 1, JitOperation.Inc, 64, new(0, 0), default)),
            new(new(secondPc + 1, 5, JitOperation.Jump, 64, default, default, Target: Ip)) ]);
        foreach (uint requested in new uint[] { 0, 1, 2, 3, 7, 31, 32, 33, 100, 512, 513, uint.MaxValue })
        foreach (uint preissued in new uint[] { 0, 1 })
        for (int sample = (int)preissued; sample < 39; ++sample)
        {
            ulong budget = sample < 35 ? (ulong)sample : sample switch { 35 => 511UL, 36 => 512UL, 37 => 513UL, _ => 1025UL };
            ResetFull(machine); *(ulong*)(machine + 24) = 0;
            ulong global = 11; long issued = preissued, completed = 0; int stop = 0;
            TranslationBlockHeader a = FullHeader(Ip, first.Entry), b = FullHeader(secondPc, second.Entry, 18);
            a.Link1TargetHeader = (nint)(&b); a.Link1TargetGeneration = b.BodyGeneration; a.Link1Pc = secondPc;
            b.Link1TargetHeader = (nint)(&a); b.Link1TargetGeneration = a.BodyGeneration; b.Link1Pc = Ip;
            FullBlockFrame frame = FullFrame(machine, &a, &global, &issued, &completed, &stop, budget, requested);
            frame.FirstPreissued = preissued;
            FullBlockExit exit = RunFull(first, &frame);
            ulong cap = requested == 0 ? ulong.MaxValue : requested;
            ulong expected = Math.Min(budget, cap);
            ulong tailJumps = expected == cap ? (expected - 1) / 2 : expected / 2;
            ulong nextPc = (expected % 4) switch { 0 => Ip, 1 => Ip + 1, 2 => secondPc, _ => secondPc + 1 };
            Check(exit == (budget < cap ? FullBlockExit.Budget : FullBlockExit.Continue) &&
                issued == (long)(budget < cap ? budget + 1 : cap) && completed == (long)expected && frame.Executed == expected,
                "linked mixed native TBs preserve total cap and exact rejected-issue accounting");
            Check(frame.NativeLimit == requested && frame.FirstPreissued == 0 && frame.TailJumps == tailJumps && frame.NextIp == nextPc &&
                *(ulong*)(machine + 24) == (expected + 1) / 2,
                "linked bodies preserve explicit/unbounded policy and aggregate execution beyond 32 and 512");
        }
        // Also exercise a final known-label backedge with the same cap semantics.
        using var loop = CreateFullBlock([
            new(new(Ip, 1, JitOperation.Inc, 64, new(0, 0), default)),
            new(new(Ip + 1, 2, JitOperation.Jump, 64, default, default, Target: Ip)) ]);
        ResetFull(machine); *(ulong*)(machine + 24) = 0;
        ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
        TranslationBlockHeader h = FullHeader(Ip, loop.Entry);
        FullBlockFrame internalLoop = FullFrame(machine, &h, &epoch, &issue, &complete, &stopped);
        Check(RunFull(loop, &internalLoop) == FullBlockExit.Budget && issue == 101 && complete == 100 &&
            internalLoop.NativeLimit == 0 && internalLoop.Executed == 100 && internalLoop.TailJumps == 0 &&
            *(ulong*)(machine + 24) == 50,
            "unbounded internal backedge runs past 32 until the precise rejected budget issue");
    }

    static void CheckFullLinkAuthentication(byte* machine)
    {
        const ulong secondPc = Ip + 5;
        using var first = CreateFullBlock([new(new(Ip, 5, JitOperation.Mov, 64, new(0, 0), new(0, 4, true)))]);
        using var second = CreateFullBlock([FullBlockInstruction.Helper(secondPc, 2)]);
        foreach (string mismatch in new[] { "none", "null", "owner", "live", "generation", "context", "pc", "code", "validated-code",
            "zero-code", "global", "entry", "link-generation", "link-pc" })
        {
            ResetFull(machine);
            ulong global = 11; long issued = 0, completed = 0; int stop = 0;
            TranslationBlockHeader a = FullHeader(Ip, first.Entry), b = FullHeader(secondPc, second.Entry, 18);
            a.Link0TargetHeader = (nint)(&b); a.Link0TargetGeneration = b.BodyGeneration; a.Link0Pc = secondPc;
            switch (mismatch)
            {
                case "null": a.Link0TargetHeader = 0; break;
                case "owner": ++b.OwnerCookie; break;
                case "live": b.Live = 0; break;
                case "generation": ++b.BodyGeneration; break;
                case "context": ++b.Context; break;
                case "pc": ++b.GuestPc; break;
                case "code": ++b.CodeEpoch; break;
                case "validated-code": ++b.ValidatedCodeEpoch; break;
                case "zero-code": b.CodeEpoch = b.ValidatedCodeEpoch = 0; break;
                case "global": ++b.ValidatedGlobalEpoch; break;
                case "entry": b.Entry = 0; break;
                case "link-generation": ++a.Link0TargetGeneration; break;
                case "link-pc": ++a.Link0Pc; break;
            }
            FullBlockFrame frame = FullFrame(machine, &a, &global, &issued, &completed, &stop);
            bool linked = mismatch == "none";
            Check(RunFull(first, &frame) == (linked ? FullBlockExit.NeedHelper : FullBlockExit.Continue) &&
                completed == 1 && frame.Executed == 1 && issued == (linked ? 2 : 1) && frame.TailJumps == (linked ? 1UL : 0UL),
                "fallthrough data link authenticates target before tail jump: " + mismatch);
            Check(frame.Header == (linked ? &b : &a) && frame.BodyGeneration == (linked ? b.BodyGeneration : a.BodyGeneration),
                "tail transition adopts only authenticated header/generation");
        }
        foreach (string changed in new[] { "live", "owner", "generation", "context", "root", "code", "global" })
        {
            ResetFull(machine);
            ulong global = 11; long issued = 0; int stop = 0;
            TranslationBlockHeader a = FullHeader(Ip, first.Entry), b = FullHeader(secondPc, second.Entry, 18);
            a.Link0TargetHeader = (nint)(&b); a.Link0TargetGeneration = b.BodyGeneration; a.Link0Pc = secondPc;
            long* completion = changed switch {
                "live" => (long*)&a.Live, "owner" => (long*)&a.OwnerCookie,
                "generation" => (long*)&a.BodyGeneration, "context" => (long*)&a.Context,
                "root" => (long*)&a.GuestPc, "code" => (long*)&a.CodeEpoch, _ => (long*)&global };
            FullBlockFrame frame = FullFrame(machine, &a, &global, &issued, completion, &stop);
            Check(RunFull(first, &frame) == FullBlockExit.Continue && issued == 1 && frame.Executed == 1 && frame.TailJumps == 0,
                "link checks source authority again after final commit: " + changed);
        }
    }

    static void CheckFullConditionalLinks(byte* machine)
    {
        using var branch = CreateFullBlock([new(new(Ip, 2, JitOperation.ConditionalJump, 64,
            default, default, Target: Ip + 16, Condition: 4))]);
        using var fallthrough = CreateFullBlock([FullBlockInstruction.Helper(Ip + 2, 1)]);
        using var taken = CreateFullBlock([FullBlockInstruction.Helper(Ip + 16, 1)]);
        foreach (bool condition in new[] { false, true })
        foreach (string mismatch in new[] { "none", "other-edge", "live", "owner", "generation", "link-generation", "link-pc" })
        {
            ResetFull(machine);
            if (condition) *(uint*)(machine + 12) |= 1u << 6;
            else *(uint*)(machine + 12) &= ~(1u << 6);
            ulong global = 11; long issued = 0, completed = 0; int stop = 0;
            TranslationBlockHeader source = FullHeader(Ip, branch.Entry);
            TranslationBlockHeader a = FullHeader(Ip + 2, fallthrough.Entry, 18), b = FullHeader(Ip + 16, taken.Entry, 19);
            source.Link0TargetHeader = (nint)(&a); source.Link0TargetGeneration = a.BodyGeneration; source.Link0Pc = a.GuestPc;
            source.Link1TargetHeader = (nint)(&b); source.Link1TargetGeneration = b.BodyGeneration; source.Link1Pc = b.GuestPc;
            TranslationBlockHeader* selected = condition ? &b : &a;
            switch (mismatch)
            {
                case "other-edge": (condition ? &a : &b)->Live = 0; break;
                case "live": selected->Live = 0; break;
                case "owner": ++selected->OwnerCookie; break;
                case "generation": ++selected->BodyGeneration; break;
                case "link-generation":
                    if (condition) ++source.Link1TargetGeneration; else ++source.Link0TargetGeneration;
                    break;
                case "link-pc":
                    if (condition) ++source.Link1Pc; else ++source.Link0Pc;
                    break;
            }
            FullBlockFrame frame = FullFrame(machine, &source, &global, &issued, &completed, &stop);
            bool linked = mismatch is "none" or "other-edge";
            Check(RunFull(branch, &frame) == (linked ? FullBlockExit.NeedHelper : FullBlockExit.Continue) &&
                frame.Header == (linked ? selected : &source) && frame.NextIp == (condition ? Ip + 16 : Ip + 2) + (linked ? 1UL : 0UL) &&
                issued == (linked ? 2 : 1) && completed == 1 && frame.Executed == 1 && frame.TailJumps == (linked ? 1UL : 0UL),
                "conditional branch authenticates only selected edge zero/one and commits exactly once: " + mismatch);
        }
    }

    static void CheckFullScalarEquivalence(byte* machine)
    {
        foreach (JitOperation operation in new[] { JitOperation.Mov, JitOperation.Add, JitOperation.Sub, JitOperation.Cmp,
            JitOperation.And, JitOperation.Or, JitOperation.Xor, JitOperation.Test, JitOperation.Inc, JitOperation.Dec })
        foreach (int width in new[] { 8, 16, 32, 64 })
        {
            JitInstruction[] scalar = [
                new(Ip, 3, operation, width, new(0, 0, ByteOffset: width == 8 ? 1 : 0), new(15, 0)),
                new(Ip + 3, 3, JitOperation.Mov, 32, new(14, 0), new(0, 19, true)),
                new(Ip + 6, 2, JitOperation.ConditionalJump, 64, default, default, Target: Ip + 64, Condition: 4) ];
            FullBlockInstruction[] steps = [new(scalar[0]), new(scalar[1]), new(scalar[2])];
            using var full = CreateFullBlock(steps);
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit((ReadOnlySpan<JitInstruction>)scalar), out var hybrid, out string reason), reason);
            using (hybrid!)
            for (ulong budget = 0; budget <= 4; ++budget)
            {
                ResetFull(machine);
                long expectedIssued = 0, expectedCompleted = 0; int stop = 0;
                JitFrame expectedFrame = new() { Machine = machine, Issued = &expectedIssued, Completed = &expectedCompleted, Stop = &stop, Budget = budget };
                ((delegate* unmanaged<JitFrame*, int>)hybrid!.Entry)(&expectedFrame);
                byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                ResetFull(machine);
                ulong global = 11; long issued = 0, completed = 0;
                TranslationBlockHeader header = FullHeader(Ip, full.Entry);
                FullBlockFrame actual = FullFrame(machine, &header, &global, &issued, &completed, &stop, budget);
                RunFull(full, &actual);
                Check((uint)actual.Exit == (uint)expectedFrame.Exit && issued == expectedIssued && completed == expectedCompleted &&
                    actual.Executed == expectedFrame.Executed && actual.NextIp == expectedFrame.NextIp &&
                    new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected),
                    "Full preserves Hybrid scalar materialization/flags/branch semantics at each budget boundary");
            }
        }
    }

    static void CheckFullWrongBodyEntry(byte* machine)
    {
        const ulong targetPc = Ip + 1;
        using var first = CreateFullBlock([new(new(Ip, 1, JitOperation.Nop, 64, default, default))]);
        using var target = CreateFullBlock([FullBlockInstruction.Helper(targetPc, 1)]);
        // The frame and header agree, but the executable entry belongs to a
        // different root. Sharing guard bytes must not authenticate that body.
        ResetFull(machine, targetPc);
        ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
        TranslationBlockHeader source = FullHeader(Ip, first.Entry);
        FullBlockFrame direct = FullFrame(machine, &source, &epoch, &issue, &complete, &stopped);
        Check(RunFull(target, &direct) == FullBlockExit.Invalidated && issue == 0 && complete == 0 &&
            direct.NextIp == targetPc && direct.TailJumps == 0,
            "wrong executable body cannot borrow another header's authenticated root");

        ResetFull(machine); issue = complete = 0;
        TranslationBlockHeader destination = FullHeader(targetPc, first.Entry, 18); // corrupted nonnull Entry
        source.Link0TargetHeader = (nint)(&destination);
        source.Link0TargetGeneration = destination.BodyGeneration; source.Link0Pc = targetPc;
        FullBlockFrame linked = FullFrame(machine, &source, &epoch, &issue, &complete, &stopped);
        Check(RunFull(first, &linked) == FullBlockExit.Invalidated && issue == 1 && complete == 1 &&
            linked.Executed == 1 && linked.TailJumps == 1 && linked.Header == &destination &&
            linked.NextIp == targetPc && linked.FirstPreissued == 0,
            "corrupt target Entry passes nonnull link check but its own root guard rejects before another issue");
    }

    static void CheckFullShapeEquivalence(byte* machine)
    {
        // Native 1..512-site NOP/helper/mixed execution in BOTH forms.
        // A typed helper here has NOP semantics, committed only after full RET.
        foreach (int count in new[] { 1, 8, 32, 128, 256, 512 })
        foreach (int shape in new[] { 0, 1, 2 })
        {
            FullBlockInstruction[] body = FullShape(count, shape);
            using var code = CreateFullBlock(body);
            ResetFull(machine);
            ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issue, &complete, &stopped, budget: (ulong)count);
            byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            BinaryPrimitives.WriteUInt64LittleEndian(expected, Ip + (uint)count);
            expected[8] = 0;
            int helpers = 0;
            FullBlockExit exit;
            while ((exit = RunFull(code, &frame)) == FullBlockExit.NeedHelper)
            {
                Check(frame.HelperIndex < count && body[(int)frame.HelperIndex].RequiresHelper &&
                    frame.ResumeIndex == frame.HelperIndex + 1 && machine[8] == 1,
                    "shape helper is materialized without completion until the leaf returns");
                ++helpers; ++complete; machine[8] = 0;
                if (frame.ResumeIndex == count) break;
                Check(frame.NativeLimit == 0, "normal helper resume keeps the unbounded invocation policy");
            }
            Check((exit == FullBlockExit.Continue || exit == FullBlockExit.NeedHelper && frame.ResumeIndex == count) &&
                issue == count && complete == count && frame.Executed + (ulong)helpers == (ulong)count &&
                frame.NextIp == Ip + (uint)count && *(ulong*)machine == frame.NextIp && machine[8] == 0 &&
                new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected),
                "1..512 native/helper/mixed forms preserve exact issued/completed/IP accounting");
        }
    }

    static FullBlockInstruction[] FullShape(int count, int shape)
    {
        FullBlockInstruction[] body = new FullBlockInstruction[count];
        for (int i = 0; i < count; ++i)
            body[i] = shape == 1 || shape == 2 && (i & 1) != 0 ? FullBlockInstruction.Helper(Ip + (uint)i, 1) :
                new(new(Ip + (uint)i, 1, JitOperation.Nop, 64, default, default));
        return body;
    }

    static void CheckFullLargeResumeAndGuards(byte* machine)
    {
        foreach (bool helper in new[] { false, true })
        {
            using var code = CreateFullBlock(FullShape(512, helper ? 1 : 0));
            foreach (uint index in new uint[] { 127, 128, 255, 256, 511 })
            foreach (uint preissued in new uint[] { 0, 1 })
            foreach (string boundary in new[] { "none", "stop", "attention", "budget", "cap", "epoch" })
            {
                ResetFull(machine, Ip + index);
                if (boundary == "attention") machine[11] = 1;
                ulong epoch = 11; long issued = preissued, completed = 0; int stop = boundary == "stop" ? 1 : 0;
                TranslationBlockHeader header = FullHeader(Ip, code.Entry);
                FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop,
                    budget: boundary == "budget" ? preissued : 1024, limit: 1);
                frame.ResumeIndex = index; frame.FirstPreissued = preissued;
                if (boundary == "cap") frame.Executed = 1;
                if (boundary == "epoch") ++epoch;
                byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                FullBlockExit expected = boundary switch {
                    "stop" => FullBlockExit.Stop,
                    "attention" when preissued == 0 => FullBlockExit.Attention,
                    "budget" when preissued == 0 => FullBlockExit.Budget,
                    "epoch" => FullBlockExit.Invalidated,
                    "cap" => FullBlockExit.Continue,
                    _ => helper ? FullBlockExit.NeedHelper : FullBlockExit.Continue };
                bool executes = boundary == "none" || (boundary is "attention" or "budget") && preissued == 1;
                Check(RunFull(code, &frame) == expected && issued == (executes || expected == FullBlockExit.Budget ? 1 : preissued) &&
                    completed == (executes && !helper ? 1 : 0) &&
                    frame.Executed == (boundary == "cap" || executes && !helper ? 1UL : 0UL) &&
                    frame.FirstPreissued == (executes ? 0 : preissued),
                    "127/128/255/256/511 resume preserves exact guard, preissue, budget and explicit-cap semantics");
                if (executes)
                {
                    Check(frame.NextIp == Ip + index + 1 &&
                        (!helper || frame.HelperIndex == index && frame.ResumeIndex == index + 1 && machine[8] == 1),
                        "high resume dispatch reaches its exact instruction and full-width helper indices");
                    if (helper)
                    {
                        ++completed; machine[8] = 0; stop = 0; machine[11] = 0;
                        frame.NativeLimit = 0; frame.Budget = 1; // approved issue was consumed exactly once
                        Check(RunFull(code, &frame) == (index == 511 ? FullBlockExit.Invalidated : FullBlockExit.Budget) &&
                            issued == (index == 511 ? 1 : 2) && completed == 1 && frame.Executed == 0 &&
                            frame.FirstPreissued == 0 && frame.NextIp == Ip + index + 1,
                            "helper resume rechecks rejected issue once; informational resume512 never enters native code");
                    }
                }
                else Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before),
                    "high-index rejection preserves all architectural state");
            }
            if (helper) continue;
            // Completion changes the poll input immediately before the next
            // high-index boundary, deterministically, without a racing thread.
            foreach (uint index in new uint[] { 127, 128, 255, 256, 511 })
            foreach (bool attention in new[] { false, true })
            {
                ResetFull(machine, Ip + index - 1);
                ulong epoch = 11; long issued = 0, stopWord = 0;
                TranslationBlockHeader header = FullHeader(Ip, code.Entry);
                long* completion = attention ? (long*)(machine + 11) : &stopWord;
                FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, completion, (int*)&stopWord, budget: 1024);
                frame.ResumeIndex = index - 1;
                Check(RunFull(code, &frame) == (attention ? FullBlockExit.Attention : FullBlockExit.Stop) &&
                    issued == 1 && frame.Executed == 1 && frame.NextIp == Ip + index && frame.NativeLimit == 0,
                    "unbounded execution polls stop/attention again at every high instruction boundary");
            }
        }
    }

    static void CheckFullLargeInternalLoop(byte* machine)
    {
        FullBlockInstruction[] body = FullShape(512, 0);
        body[^1] = new(new(Ip + 511, 5, JitOperation.Jump, 64, default, default, Target: Ip));
        using var code = CreateFullBlock(body);
        foreach (uint preissued in new uint[] { 0, 1 })
        {
            ResetFull(machine);
            ulong epoch = 11; long issued = preissued, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop, budget: 1025);
            frame.FirstPreissued = preissued;
            Check(RunFull(code, &frame) == FullBlockExit.Budget && issued == 1026 && completed == 1025 &&
                frame.Executed == 1025 && frame.NativeLimit == 0 && frame.TailJumps == 0 &&
                frame.NextIp == Ip + 1 && frame.ResumeIndex == 1 && frame.FirstPreissued == 0,
                "512-site body and long backward fixup execute two full loops before precise budget rejection");
        }
        foreach (uint invalid in new uint[] { 512, 65536, uint.MaxValue })
        {
            ResetFull(machine);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop);
            frame.ResumeIndex = invalid;
            Check(RunFull(code, &frame) == FullBlockExit.Invalidated && issued == 0 && completed == 0 && frame.NextIp == Ip,
                "full-width unknown resume cannot alias a known low-byte instruction label");
        }
    }

    static FullBlockInstruction[] FullScalarShape(int count)
    {
        FullBlockInstruction[] body = new FullBlockInstruction[count];
        for (int i = 0; i < body.Length; ++i)
            body[i] = i == body.Length - 1
                ? new(new(Ip + (uint)(i * 15), 15, JitOperation.ConditionalJump, 64, default, default, Target: Ip, Condition: 15))
                : new(new(Ip + (uint)(i * 15), 15, JitOperation.Add, 64, new(15, 0), new(0, ulong.MaxValue, true)));
        return body;
    }

    static void CheckFullOwnerScaffoldReuse(byte* machine)
    {
        using var owner = CreateFullScaffold(out var addresses);
        Check(!fullOwnerScaffold.IsValid, "owner reuse fixture starts with no borrowed slab");
        fullOwnerScaffold = addresses;
        try
        {
            // Simulated quiescent store-body retirement. The owner slab is
            // unchanged and is not counted as a new per-instruction TB/stub.
            for (int generation = 17; generation < 21; ++generation)
            {
                using var body = CreateFullBlock([
                    new(new(Ip, 1, JitOperation.Nop, 64, default, default)),
                    new(new(Ip + 1, 1, JitOperation.Nop, 64, default, default)) ]);
                Check(body.Entry != owner.Entry, "shared code and each body use unrelated native allocations");
                ResetFull(machine);
                ulong epoch = 11; long issue = 0, complete = 0; int stopped = 0;
                TranslationBlockHeader header = FullHeader(Ip, body.Entry, (ulong)generation);
                FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issue, &complete, &stopped);
                Check(RunFull(body, &frame) == FullBlockExit.Continue && issue == 2 && complete == 2 &&
                    frame.Executed == 2 && frame.TailJumps == 0 && fullOwnerScaffold == addresses,
                    "one common slab survives retired bodies without adding issue/completion/tail work");
            }
        }
        finally { fullOwnerScaffold = default; }
    }

    static void CheckFullOwnerScaffoldEncodingAndAllocation()
    {
        byte[] common = new byte[FullBlockEmitter.ScaffoldCapacity];
        byte[] otherAbi = new byte[FullBlockEmitter.ScaffoldCapacity];
        byte[] body = new byte[FullBlockEmitter.CodeCapacity], standalone = new byte[FullBlockEmitter.CodeCapacity];
        int winLength = FullBlockEmitter.EmitScaffold(common, out var windows, windowsAbi: true);
        int sysLength = FullBlockEmitter.EmitScaffold(otherAbi, out var sysv, windowsAbi: false);
        Check(winLength == sysLength && windows.WindowsAbi && !sysv.WindowsAbi,
            "both shared scaffold ABI layouts have the same size");
        int differences = 0;
        for (int i = 0; i < winLength; ++i) if (common[i] != otherAbi[i]) ++differences;
        Check(differences == 1 && CountFullPattern(common.AsSpan(0, winLength), [0x4c, 0x89, 0xd1, 0xff, 0xe0]) == 1 &&
            CountFullPattern(otherAbi.AsSpan(0, sysLength), [0x4c, 0x89, 0xd7, 0xff, 0xe0]) == 1,
            "shared ABI difference is exactly the tail argument restore; no Windows nonvolatile RDI");
        Check(CountFullPattern(common.AsSpan(0, winLength), [0xf0, 0x48, 0x0f, 0xc1, 0x10]) == 1 &&
            CountFullPattern(common.AsSpan(0, winLength), [0x49, 0x8b, 0x42, 32, 0x83, 0x38, 0]) == 1 &&
            CountFullPattern(common.AsSpan(0, winLength), [0x49, 0x8b, 0x4a, 72, 0x48, 0x39, 0x48, 16]) == 2 &&
            CountFullPattern(common.AsSpan(0, winLength), [0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 72]) == 11,
            "one owner issue/poll, two body-independent root guards, and ten typed materialized exits plus the ABI-failure exit");
        foreach (int offset in new[] { windows.EntryOffset, windows.InlineGuardOffset,
            windows.HelperGuardOffset, windows.BoundaryOffset, windows.LinkOffset,
            windows.MemoryReadOffset, windows.MemoryWriteOffset, windows.MemoryReadWriteOffset, windows.MemoryAtomicFastOffset })
            Check(common.AsSpan(offset, 4).SequenceEqual<byte>([0xf3, 0x0f, 0x1e, 0xfa]),
                "every externally reached common entry has ENDBR64");
        for (int exit = 0; exit <= (int)FullBlockExit.NeedEnvironment; ++exit)
        {
            int offset = windows.ExitsOffset + exit * FullSharedScaffoldLayout.ExitStride;
            Check(common.AsSpan(offset, 4).SequenceEqual<byte>([0xf3, 0x0f, 0x1e, 0xfa]) &&
                BinaryPrimitives.ReadUInt32LittleEndian(common.AsSpan(offset + 12 + FullBlockEmitter.FloatRestoreHostEnvironmentBytes, 4)) == (uint)exit &&
                common[offset + FullSharedScaffoldLayout.ExitStride - 1] == 0xc3,
                "resolved typed exit stride reaches ENDBR64, exact exit value and full RET");
        }
        foreach (byte[] bytes in new[] { common, otherAbi })
        {
            Check(CountFullPattern(bytes.AsSpan(0, winLength), [0xf3, 0x0f, 0x1e, 0xfa]) == (int)FullBlockExit.NeedEnvironment + 1 + 9 + 2,
                "nine common external entries, two RDX continuations and all typed exits have ENDBR64");
            int eligibility = 0;
            for (int at = 0; at <= winLength - 7; ++at)
                if (bytes.AsSpan(at, 3).SequenceEqual<byte>([0x48, 0x8d, 0x15]))
                {
                    int target = checked(at + 7 + BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(at + 3, 4)));
                    Check(target >= 0 && target <= winLength - 4 &&
                        bytes.AsSpan(target, 4).SequenceEqual<byte>([0xf3, 0x0f, 0x1e, 0xfa]),
                        "both ABI local RDX continuation fixups stay inside the common slab at ENDBR64");
                    ++eligibility;
                }
            Check(eligibility == 2, "one inline and one helper eligibility continuation in the common slab");
        }
        // Far synthetic RX bases are ENCODING checks only; neither is invoked.
        // Common/body allocation addresses may differ by much more than rel32.
        foreach (bool windowsAbi in new[] { false, true })
        foreach (long rx in new long[] { 0x0000000100000000, 0x0000700000000000 })
        {
            var layout = windowsAbi ? windows : sysv;
            FullSharedScaffold addresses = layout.Resolve((nint)rx);
            foreach (int count in new[] { 1, 8, 32, 128, 256, 512 })
            foreach (int shape in new[] { 0, 1, 2 })
            {
                FullBlockInstruction[] sites = FullShape(count, shape);
                int length = FullBlockEmitter.Emit(sites, body, in addresses, windowsAbi);
                int oldLength = FullBlockEmitter.Emit(sites, standalone, windowsAbi);
                ReadOnlySpan<byte> emitted = body.AsSpan(0, length);
                Check(length <= 192 + count * 129 && (count > 32 || length < oldLength),
                    "compact 1..512 NOP/helper/mixed bodies stay bounded; small bodies remove fixed scaffold bytes");
                Check(emitted[6] == (windowsAbi ? 0xca : 0xfa) &&
                    CountFullPattern(emitted, [0x49, 0x89, 0x42, 72]) == 1 &&
                    CountFullPattern(emitted, [0xf0, 0x48, 0x0f, 0xc1, 0x10]) == 0 &&
                    CountFullPattern(emitted, [0x49, 0x8b, 0x42, 96, 0x48, 0x85, 0xc0]) == 0 &&
                    CountFullPattern(emitted, [0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 72]) == 0,
                    "body seeds private root once and carries no common issue/header/typed-RET copies");
                int transfers = 0, continuations = 0;
                for (int at = 0; at < length; ++at)
                {
                    if (at <= length - 12 && emitted[at] == 0x48 && emitted[at + 1] == 0xb8 &&
                        emitted[at + 10] == 0xff && emitted[at + 11] == 0xe0)
                    {
                        nint target = unchecked((nint)BinaryPrimitives.ReadUInt64LittleEndian(emitted.Slice(at + 2, 8)));
                        bool known = target == addresses.Entry || target == addresses.InlineGuard ||
                            target == addresses.HelperGuard || target == addresses.Link;
                        for (int exit = 0; exit <= (int)FullBlockExit.NeedEnvironment; ++exit)
                            known |= target == addresses.Exit((FullBlockExit)exit);
                        Check(known, "each cross-allocation JMP contains an exact full-width owner address");
                        ++transfers;
                    }
                    if (at <= length - 7 && emitted.Slice(at, 3).SequenceEqual<byte>([0x4c, 0x8d, 0x0d]))
                    {
                        int target = checked(at + 7 + BinaryPrimitives.ReadInt32LittleEndian(emitted.Slice(at + 3, 4)));
                        Check(target == at + 19 && target <= length - 4 &&
                            emitted.Slice(target, 4).SequenceEqual<byte>([0xf3, 0x0f, 0x1e, 0xfa]),
                            "dispatcher/operation RIP fixup skips exactly one absolute JMP to ENDBR64");
                        ++continuations;
                    }
                }
                Check(continuations == count + 1 && transfers >= count + 4 && transfers <= count + 6,
                    "one shared-prologue continuation plus one per instruction, all transfers use JMP");
                for (int i = 0; i < 4; ++i) FullBlockEmitter.Emit(sites, body, in addresses);
                long allocated = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 20; ++i) FullBlockEmitter.Emit(sites, body, in addresses);
                Check(GC.GetAllocatedBytesForCurrentThread() == allocated,
                    "warmed compact emission allocates zero managed bytes for every size/shape/ABI");
            }
        }
        FullBlockInstruction[] maximum = FullScalarShape(256);
        var far = sysv.Resolve((nint)0x0000700000000000);
        int fitting = maximum.Length;
        while (!FullBlockEmitter.TryEmit(maximum.AsSpan(0, fitting), body, in far, out _))
        {
            Check(fitting > 1, "scalar native body can shrink within the backend envelope");
            fitting /= 2;
        }
        Check(fitting > 0, "baseline flag capture uses bounded emission and shrink retry");
        for (int i = 0; i < 4; ++i) FullBlockEmitter.EmitScaffold(common, out _);
        long commonAllocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 20; ++i) FullBlockEmitter.EmitScaffold(common, out _);
        Check(GC.GetAllocatedBytesForCurrentThread() == commonAllocated,
            "warmed one-time scaffold emission also has no managed allocation");
        bool rejected = false;
        try { FullBlockEmitter.Emit(FullShape(1, 0), body, default(FullSharedScaffold)); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "default scaffold is rejected before emitting a body");
        rejected = false;
        try { FullBlockEmitter.Emit(FullShape(1, 0), body, in far, windowsAbi: true); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "explicit host ABI cannot disagree with shared slab ABI");
        rejected = false;
        try { default(FullSharedScaffoldLayout).Resolve((nint)1); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "default relative layout cannot resolve executable addresses");
    }

    static void CheckFullOversizeAndRetry()
    {
        Check(FullBlockEmitter.MaximumInstructions == 512 && FullBlockEmitter.CodeCapacity == 65536 &&
            FullBlockEmitter.ScratchBytes == 4259 && FullBlockEmitter.ScratchBytes <= 4352 &&
            FullBlockFrame.AbiVersion == 0x46420007, "512-site ABI has a bounded practical code envelope and small stack scratch");
        FullBlockInstruction[] large = FullScalarShape(512), small = FullShape(1, 0);
        byte[] storage = new byte[FullBlockEmitter.CodeCapacity + 32];
        byte[] common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windowsAbi in new[] { false, true })
        {
            FullBlockEmitter.EmitScaffold(common, out var layout, windowsAbi);
            var scaffold = layout.Resolve((nint)0x0000700000000000);
            foreach (bool shared in new[] { false, true })
            {
                storage.AsSpan().Fill(0xa5);
                int length;
                bool success = shared ? FullBlockEmitter.TryEmit(large, storage, in scaffold, out length, windowsAbi) :
                    FullBlockEmitter.TryEmit(large, storage, out length, windowsAbi);
                Check(!success && length == 0, "oversized 512-site scalar body returns a nonthrowing unpublished outcome");
                Check(storage.AsSpan(FullBlockEmitter.CodeCapacity).IndexOfAnyExcept((byte)0xa5) < 0,
                    "code envelope applies even when the caller supplies a larger staging span");
                int count = large.Length;
                do
                {
                    count /= 2;
                    success = shared ? FullBlockEmitter.TryEmit(large.AsSpan(0, count), storage, in scaffold, out length, windowsAbi) :
                        FullBlockEmitter.TryEmit(large.AsSpan(0, count), storage, out length, windowsAbi);
                } while (!success && count > 1);
                Check(success && count < 512 && length > 0 && length <= FullBlockEmitter.CodeCapacity,
                    "compiler can halve oversized body count and retry the same caller-owned staging");
                int fittingLength = length;
                int exact = shared ? FullBlockEmitter.Emit(large.AsSpan(0, count), storage, in scaffold, windowsAbi) :
                    FullBlockEmitter.Emit(large.AsSpan(0, count), storage, windowsAbi);
                Check(exact == fittingLength, "TryEmit and Emit publish the identical successful length");
                foreach (int bytes in new[] { 0, 1, 4, 7, 32, 128, fittingLength - 1 })
                {
                    Span<byte> truncated = storage.AsSpan(8, bytes);
                    storage[7] = storage[8 + bytes] = 0x5a;
                    success = shared ? FullBlockEmitter.TryEmit(large.AsSpan(0, count), truncated, in scaffold, out length, windowsAbi) :
                        FullBlockEmitter.TryEmit(large.AsSpan(0, count), truncated, out length, windowsAbi);
                    Check(!success && length == 0 && storage[7] == 0x5a && storage[8 + bytes] == 0x5a,
                        "overflow at opcode/immediate/forward-fixup boundaries cannot write outside the caller span");
                }
                for (int i = 0; i < 4; ++i)
                {
                    if (shared) FullBlockEmitter.TryEmit(large, storage, in scaffold, out _, windowsAbi);
                    else FullBlockEmitter.TryEmit(large, storage, out _, windowsAbi);
                }
                long allocated = GC.GetAllocatedBytesForCurrentThread();
                bool outcomes = true;
                for (int i = 0; i < 20; ++i)
                {
                    if (shared)
                    {
                        outcomes &= !FullBlockEmitter.TryEmit(large, storage, in scaffold, out int failed, windowsAbi) && failed == 0;
                        outcomes &= FullBlockEmitter.TryEmit(small, storage, in scaffold, out int emitted, windowsAbi) && emitted > 0;
                    }
                    else
                    {
                        outcomes &= !FullBlockEmitter.TryEmit(large, storage, out int failed, windowsAbi) && failed == 0;
                        outcomes &= FullBlockEmitter.TryEmit(small, storage, out int emitted, windowsAbi) && emitted > 0;
                    }
                }
                Check(GC.GetAllocatedBytesForCurrentThread() == allocated && outcomes,
                    "warmed success and oversize TryEmit paths allocate zero managed bytes on both ABIs/forms");
                bool rejected = false;
                try
                {
                    if (shared) FullBlockEmitter.Emit(large, storage, in scaffold, windowsAbi);
                    else FullBlockEmitter.Emit(large, storage, windowsAbi);
                }
                catch (ArgumentException) { rejected = true; }
                Check(rejected, "throwing Emit wrapper preserves its failed-publication contract");
            }
        }
        bool invalid = false;
        try { FullBlockEmitter.TryEmit(FullShape(513, 0), storage, out _); }
        catch (ArgumentOutOfRangeException) { invalid = true; }
        Check(invalid, "513 sites are invalid IR, distinct from a legal oversized 512-site body");
    }

    static void CheckFullEmitterEncodingAndAllocation()
    {
        FullBlockInstruction[] steps = [new(new(Ip, 1, JitOperation.Nop, 64, default, default)), FullBlockInstruction.Helper(Ip + 1, 2)];
        byte[] windows = new byte[FullBlockEmitter.CodeCapacity], sysv = new byte[FullBlockEmitter.CodeCapacity];
        int winLength = FullBlockEmitter.Emit(steps, windows, windowsAbi: true);
        int sysLength = FullBlockEmitter.Emit(steps, sysv, windowsAbi: false);
        Check(winLength == sysLength && windows[6] == 0xca && sysv[6] == 0xfa, "Full entry uses RCX on Windows and RDI on SysV");
        ReadOnlySpan<byte> winTail = [0x4c, 0x89, 0xd1, 0xff, 0xe0], sysTail = [0x4c, 0x89, 0xd7, 0xff, 0xe0];
        Check(CountFullPattern(windows.AsSpan(0, winLength), winTail) == 1 && CountFullPattern(windows.AsSpan(0, winLength), sysTail) == 0 &&
            CountFullPattern(sysv.AsSpan(0, sysLength), sysTail) == 1,
            "both Full link edges share a tail restore using exact argument register and never Windows RDI");
        int differences = 0;
        for (int i = 0; i < winLength; ++i) if (windows[i] != sysv[i]) ++differences;
        Check(differences == 2, "ABI byte differences are exactly entry and shared tail argument restore");
        // Warm before measuring; compilation retains no code array or per-site object.
        for (int i = 0; i < 4; ++i) FullBlockEmitter.Emit(steps, windows);
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; ++i) FullBlockEmitter.Emit(steps, windows);
        Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "warmed Full emission has zero managed allocation");
        FullBlockInstruction[] maximum = FullScalarShape(256);
        Check(FullBlockEmitter.Emit(maximum, windows) <= FullBlockEmitter.CodeCapacity,
            "256 guarded scalar instructions including final backedge fit the practical envelope");
        bool rejected = false;
        try { FullBlockEmitter.Emit([new(new(Ip, 4, JitOperation.LoadMov, 64, new(0, 0), default, SourceWidth: 64))], windows); }
        catch (ArgumentException) { rejected = true; }
        Check(rejected, "Full memory operation requires explicit typed helper plan");
    }

    static void CheckFullSharedScaffoldingSize()
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity];
        int[] counts = [1, 8, 32, 128, 256, 512];
        int[] nopLengths = new int[counts.Length], helperLengths = new int[counts.Length];
        foreach (int shape in new[] { 0, 1, 2 }) // native NOPs, typed helpers, alternating
        for (int sample = 0; sample < counts.Length; ++sample)
        {
            int count = counts[sample];
            FullBlockInstruction[] sites = new FullBlockInstruction[count];
            for (int i = 0; i < count; ++i)
                sites[i] = shape == 1 || shape == 2 && (i & 1) != 0 ? FullBlockInstruction.Helper(Ip + (uint)i, 1) :
                    new(new(Ip + (uint)i, 1, JitOperation.Nop, 64, default, default));
            int length = FullBlockEmitter.Emit(sites, staging);
            ReadOnlySpan<byte> emitted = staging.AsSpan(0, length);
            if (shape == 0) nopLengths[sample] = length;
            if (shape == 1) helperLengths[sample] = length;
            // imm32 resume comparisons add two bytes/site; zero-cap bypass adds
            // eight bytes to every inline commit and each common cap guard.
            // A consumed memory ticket adds a 17-byte postcommit guard; its
            // fixed common copies and two new typed exits also need headroom.
            int bound = 1536 + count * 129;
            Check(length <= bound && length <= FullBlockEmitter.CodeCapacity,
                "shared scaffolding bounds 1..512-site native/helper/mixed code size");
            Check(CountFullPattern(emitted, [0x49, 0x8b, 0x42, 32, 0x83, 0x38, 0]) == 1 &&
                CountFullPattern(emitted, [0xf0, 0x48, 0x0f, 0xc1, 0x10]) == 1 &&
                CountFullPattern(emitted, [0x41, 0xc7, 0x82, 152, 0, 0, 0, 0, 0, 0, 0]) == 1 &&
                CountFullPattern(emitted, [0x4d, 0x39, 0x03]) == 1 &&
                CountFullPattern(emitted, [0x41, 0xf7, 0x43, 12]) == 1,
                "poll, rejected-issue lock-xadd, preissue consumption, PC and inline context guards are shared once");
            Check(CountFullPattern(emitted, [0x49, 0x8b, 0x42, 96, 0x48, 0x85, 0xc0]) == 2,
                "exactly two fixed header guards preserve Invalidated boundary versus Continue source-link failures");
            Check(CountFullPattern(emitted, [0xf3, 0x0f, 0x1e, 0xfa]) == count + 3,
                "native entry, each operation continuation and both shared indirect continuations have ENDBR64");
            int continuations = 0, eligibilityContinuations = 0;
            for (int at = 0; at <= length - 7; ++at)
            {
                bool operation = emitted.Slice(at, 3).SequenceEqual<byte>([0x4c, 0x8d, 0x0d]);
                bool eligibility = emitted.Slice(at, 3).SequenceEqual<byte>([0x48, 0x8d, 0x15]);
                if (!operation && !eligibility) continue;
                int target = checked(at + 7 + BinaryPrimitives.ReadInt32LittleEndian(emitted.Slice(at + 3, 4)));
                Check(target >= 0 && target <= length - 4 && emitted.Slice(target, 4).SequenceEqual<byte>([0xf3, 0x0f, 0x1e, 0xfa]),
                    "every RIP-relative volatile continuation resolves inside its own body to ENDBR64");
                if (operation)
                {
                    Check(target == at + 12 && emitted[at + 7] == 0xe9,
                        "each operation continuation skips exactly one direct JMP into shared scaffolding");
                    ++continuations;
                }
                else ++eligibilityContinuations;
            }
            Check(continuations == count && eligibilityContinuations == 2,
                "sites use direct JMP guards and volatile R9/RDX continuations without a native CALL");
            for (int i = 0; i < 4; ++i) FullBlockEmitter.Emit(sites, staging);
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 20; ++i) FullBlockEmitter.Emit(sites, staging);
            Check(GC.GetAllocatedBytesForCurrentThread() == allocated,
                "warmed 1..512-site native/helper/mixed emission has zero managed allocation");
        }
        for (int i = 1; i < counts.Length; ++i)
            Check(nopLengths[i] - nopLengths[i - 1] == (counts[i] - counts[i - 1]) * 117 &&
                helperLengths[i] - helperLengths[i - 1] == (counts[i] - counts[i - 1]) * 93,
                "adding sites grows operation/resume/cap materialization; shared guard/issue code stays constant");
    }

    static int CountFullPattern(ReadOnlySpan<byte> source, ReadOnlySpan<byte> pattern)
    {
        int count = 0;
        for (int i = 0; i <= source.Length - pattern.Length; ++i)
            if (source.Slice(i, pattern.Length).SequenceEqual(pattern)) ++count;
        return count;
    }
}
