using System.Runtime.CompilerServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Value-only pending issue, never ownership of Identity.Header. The origin
    // must be reacquired; the old pointer is only authenticated capture evidence.
    private struct FullIssuedEnvironment
    {
        internal FullTranslationJit.Plan Plan;
        internal JitInstruction Instruction;
        internal FullEnvironmentDataDescriptor Identity;
        internal TranslationToken Origin;
    }

    // Parent adds workers * this charge to FullTranslationJit.overhead. No new
    // worker/native allocation: ENV1 reuses the exact existing128-byte union.
    // Include outer pending/capture values, dedicated/expected/outcome frames,
    // validation Plan/IR/header/capability temporaries and bounded service lease.
    internal static long ProcessorEnvironmentScopeCharge { get; } = checked((
        2L * Unsafe.SizeOf<FullIssuedEnvironment>() + 8L * sizeof(FullBlockFrame) +
        3L * sizeof(FullEnvironmentDataDescriptor) + 2L * Unsafe.SizeOf<FullTranslationJit.Plan>() +
        2L * sizeof(JitInstruction) + sizeof(TranslationBlockHeader) + sizeof(long) + // private native completion counter
        2L * Unsafe.SizeOf<TranslationLease>() + Unsafe.SizeOf<NativeEpochLease>() + 1024L + 63L) & ~63L);

    private static bool NativeProcessorEnvironment(in FullTranslationJit.Plan plan, out JitInstruction instruction)
    {
        var decoded = plan.Decoded;
        instruction = default;
        return plan.Helper == null && decoded.length is >= 2 and <= 15 &&
            FullProcessorEnvironmentLowering.TryClassify(plan.Ip, new(decoded.bytes, decoded.length), out instruction, out _) &&
            FullBlockEmitter.CanInlineProcessorEnvironment(instruction);
    }

    // Dispatch the tag before any memory/ticket diagnostics or resolver sees it.
    // In particular an ENV1 Invalidated/NeedMemory/NeedHelper cannot be retried
    // through ordinary Issue, or have its ownership evidence silently cleared.
    private static void RequireProcessorEnvironmentDispatch(Worker worker, in FullBlockFrame frame)
    {
        if (frame.StackMemory != worker.FullNativeState!.StackMemory)
            throw new InvalidOperationException("ENV1 native return lost its exact worker union.");
        bool environment = ((FullEnvironmentDataDescriptor*)frame.StackMemory)->Tag == FullEnvironmentDataDescriptor.TagValue;
        if (environment != (frame.Exit == FullBlockExit.NeedEnvironment))
            throw new InvalidOperationException("ENV1 native union/exit dispatch mismatch; internal terminal.");
    }

    private FullIssuedEnvironment CaptureIssuedEnvironment(Worker worker, in NativeEpochLease epoch,
        FullBlockFrame* frame, bool signalActor)
    {
        if (epoch.Owner != fullJit!.Store || epoch.Serial == 0 || frame->Header == null ||
            frame->Machine != worker.Machine || frame->StackMemory != worker.FullNativeState!.StackMemory ||
            unchecked((ulong)frame->OwnerHandle) != worker.FullCache.Serial || frame->HostMxcsrDirty != 0)
            throw new InvalidOperationException("ENV1 initial handoff has no exact native/store/worker ownership.");
        // A linked tail may name a body different from the root TranslationLease.
        // Only this original store epoch protects the returned table here.
        var header = *frame->Header;
        if (header.OwnerCookie != fullJit.Store.OwnerCookie || header.OwnerCookie != frame->OwnerCookie ||
            header.BodyGeneration != frame->BodyGeneration || header.Context != frame->Context ||
            header.Context != FullTranslationJit.Context(program!, worker.Machine, signalActor ? 1u : 0u) ||
            header.Entry == 0 || header.Payload == 0 || header.PayloadBytes == 0 ||
            header.PayloadBytes % sizeof(FullTranslationJit.Plan) != 0 ||
            frame->ResumeIndex >= header.PayloadBytes / sizeof(FullTranslationJit.Plan))
            throw new InvalidOperationException("ENV1 initial immutable header/table authentication failed.");
        var plan = ((FullTranslationJit.Plan*)header.Payload)[frame->ResumeIndex];
        if (!NativeProcessorEnvironment(in plan, out var instruction) || plan.Ip != worker.Machine->ip ||
            frame->NextIp != plan.Ip)
            throw new InvalidOperationException("ENV1 initial handoff has no exact typed native Plan.");
        var request = FullProcessorEnvironmentProtocol.Capture(frame, in instruction);
        return new() { Plan = plan, Instruction = instruction, Identity = request,
            Origin = new(checked((int)header.Slot), header.BodyGeneration) };
    }

    private void ValidateIssuedEnvironment(Worker worker, in FullIssuedEnvironment pending, bool signalActor)
    {
        ref readonly var id = ref pending.Identity;
        if (!NativeProcessorEnvironment(in pending.Plan, out var instruction) || instruction != pending.Instruction ||
            pending.Origin.Slot < 0 || pending.Origin.Generation == 0 || pending.Origin.Generation != id.BodyGeneration ||
            id.Header == null || id.GlobalEpoch == 0 || id.CodeEpoch == 0 || id.StoreGeneration == 0 ||
            id.OwnerCookie != fullJit!.Store.OwnerCookie || id.Context !=
                FullTranslationJit.Context(program!, worker.Machine, signalActor ? 1u : 0u) ||
            id.Tag != FullEnvironmentDataDescriptor.TagValue || id.Version != 1 ||
            id.State != FullEnvironmentDataState.Request || id.Ip != instruction.Ip ||
            id.Operation != (FullProcessorEnvironmentOperation)instruction.Options || id.Width != instruction.Width ||
            worker.Machine->ip != instruction.Ip || worker.Machine->oplen != 0 ||
            id.Data0 != 0 || id.Data1 != 0 || id.RngState != null || id.RngCount != null || id.LeaseCookie != 0)
            throw new InvalidOperationException("ENV1 pending approved issue changed before sampling; internal terminal.");
    }

    private static FullBlockFrame EnvironmentOutcome(Worker worker, FullBlockExit exit) => new()
    {
        Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize,
        Machine = worker.Machine, Exit = exit, FirstPreissued = 1, NextIp = worker.Machine->ip,
        HelperIndex = FullBlockFrame.NoHelperIndex
    };

    private bool EnvironmentPaused(Worker worker, in JitInstruction instruction, out FullBlockFrame outcome)
    {
        // NeedEnvironment already passed Poll + Issue. Like FirstPreissued1,
        // this pending operation defers attention until its one native commit;
        // returning here would lose the approved issue across signal handling.
        outcome = default;
        if (worker.Stop.Reason != HostExecutionStopReason.None ||
            worker.NativePoll != null && Volatile.Read(ref *worker.NativePoll.Pointer) != 0)
            outcome = EnvironmentOutcome(worker, FullBlockExit.Stop);
        else if ((instruction.Options is (uint)FullProcessorEnvironmentOperation.Rdtsc or (uint)FullProcessorEnvironmentOperation.Rdtscp) &&
            (int)worker.Machine->traprdtsc != 0)
        {
            outcome = EnvironmentOutcome(worker, FullBlockExit.GuestFault);
            outcome.FaultTrap = 14; outcome.FaultCode = unchecked((uint)(int)(sbyte)worker.Machine->segvcode);
            outcome.FaultAddress = 0;
        }
        return outcome.Version != 0;
    }

    // PRE-SERVICE only: actual owner stamps, never an old unleased header
    // dereference or a caller-supplied "revoked" bit, witness ordinary change.
    private bool EnvironmentPreSampleAuthorityChanged(in FullIssuedEnvironment pending) =>
        fullJit!.Authority.Epoch != pending.Identity.GlobalEpoch ||
        fullJit.Authority.CodeEpoch != pending.Identity.CodeEpoch ||
        fullJit.Store.Generation != pending.Identity.StoreGeneration;

    private FullBlockFrame RetryInvalidatedEnvironment(Worker worker, in FullIssuedEnvironment pending,
        in FullEnvironmentDataDescriptor expectedRequest, bool signalActor)
    {
        ValidateIssuedEnvironment(worker, in pending, signalActor);
        // Every call is before TryAcquire(service). Without actual revocation,
        // a missing/changed origin is an internal identity failure, not SMC.
        if (!EnvironmentPreSampleAuthorityChanged(in pending))
            throw new InvalidOperationException("ENV1 changed origin without an actual authority/store change.");
        var allocation = worker.FullNativeState!.StackMemory;
        var owned = (FullEnvironmentDataDescriptor*)allocation;
        var actual = *owned; var expected = expectedRequest;
        if (expected.Tag != FullEnvironmentDataDescriptor.TagValue || expected.Version != 1 ||
            expected.State != FullEnvironmentDataState.Request || expected.Header == null || expected.Site != pending.Identity.Site ||
            expected.OwnerCookie != pending.Identity.OwnerCookie || expected.BodyGeneration != pending.Identity.BodyGeneration ||
            expected.Context != pending.Identity.Context || expected.Ip != pending.Instruction.Ip ||
            expected.Operation != (FullProcessorEnvironmentOperation)pending.Instruction.Options ||
            expected.Width != pending.Instruction.Width || expected.Data0 != 0 || expected.Data1 != 0 ||
            expected.RngState != null || expected.RngCount != null || expected.LeaseCookie != 0 ||
            !new ReadOnlySpan<byte>(&actual, sizeof(FullEnvironmentDataDescriptor)).SequenceEqual(
                new(&expected, sizeof(FullEnvironmentDataDescriptor))))
            throw new InvalidOperationException("ENV1 invalidation cannot erase foreign, malformed or sampled ownership evidence.");
        // The copied Request is now authenticated AND proven unsampled. No
        // sampled Ready/Consumed descriptor can reach this acknowledgement.
        *allocation = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
        worker.FullResume = default; worker.FullResumeIndex = 0;
        // Outer Core already preserves FirstPreissued for Version0 retries.
        // It refetches original PC and consumes this allowance without Issue++.
        return new() { FirstPreissued = 1 };
    }

    private static void RequireExactEnvironmentFrame(in FullBlockFrame frame, in FullBlockFrame expected)
    {
        // Prepare hooks may observe; they cannot mint another frame/capability.
        var actualCopy = frame; var expectedCopy = expected;
        if (!new ReadOnlySpan<byte>(&actualCopy, sizeof(FullBlockFrame)).SequenceEqual(new(&expectedCopy, sizeof(FullBlockFrame))))
            throw new InvalidOperationException("ENV1 dedicated invocation frame was replaced before sampling.");
    }

    // Error-only managed evidence survives capability cleanup. No sampled
    // descriptor is reset, refreshed, retried, guest-faulted or replayed.
    private sealed class FullEnvironmentTerminalException : InvalidOperationException
    {
        internal readonly FullBlockFrame Frame;
        internal readonly FullEnvironmentDataDescriptor Descriptor;
        internal readonly FullIssuedEnvironment Pending;
        internal FullEnvironmentTerminalException(in FullBlockFrame frame, in FullEnvironmentDataDescriptor descriptor,
            in FullIssuedEnvironment pending, Exception error)
            : base("ENV1 sampled operation failed internally; admission quarantined; no replay.", error)
        {
            Frame = frame;
            Frame.Completed = null; // terminal evidence must not retain a private stack counter
            Descriptor = descriptor; Pending = pending;
        }
    }

    private FullBlockFrame ExecuteIssuedEnvironment(Worker worker, bool signalActor,
        in FullIssuedEnvironment pending, out int error)
    {
        error = 0;
        var instruction = pending.Instruction;
        var expectedRequest = pending.Identity;
        while (true)
        {
            if (EnvironmentPaused(worker, in instruction, out var paused)) return paused;
            if (fullJit!.Authority.SemanticQuarantined)
                throw new InvalidOperationException("ENV1 semantic admission is quarantined.");
            TranslationLease lease = default;
            NativeEpochLease epoch = default;
            FullProcessorEnvironmentLease? service = null;
            FullBlockFrame frame = default;
            long nativeCompleted = 0; // fresh per actual invocation, never frame.Executed
            TranslationBlockHeader* ownedHeader = null;
            nint entry = 0;
            bool enrolled = false;
            try
            {
                // ALL fetching, retained-token lookup, byte validation and
                // ordinary prepare callbacks precede the actual RNG mutex.
                bool previousFetch = worker.FullFetchActive;
                fullJit.BeginRead(program!, semantic: false);
                try
                {
                    worker.FullFetchActive = true;
                    lock (fullJit.Authority.Gate)
                    {
                        fullJit.Authority.DrainNativeForFiniteGateRead();
                        ValidateIssuedEnvironment(worker, in pending, signalActor);
                        if (fullJit.Store.GetStatistics().Quarantined)
                            throw new InvalidOperationException("ENV1 code arena is quarantined; native admission closed.");
                        if (EnvironmentPaused(worker, in instruction, out paused)) return paused;
                        if (EnvironmentPreSampleAuthorityChanged(in pending))
                            return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                        // Retain the exact origin while checked Acquire runs.
                        // Ordinary pre-sample revocation refetches with the SAME
                        // approved issue; it never samples a replacement here.
                        if (!fullJit.Store.TryAcquireToken(pending.Origin, out var anchor, out var originFailure))
                        {
                            if (originFailure == TranslationStoreFailure.LeaseCapacity) continue;
                            return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                        }
                        using (anchor)
                        {
                            if (!fullJit.Acquire(program!, worker.Machine, ref worker.FullCache,
                                pending.Origin, pending.Identity.Site, out lease, out uint index, out bool multiple,
                                out error, requestedCount: signalActor ? 1u : 0u))
                            {
                                if (error != 0) return default; // normal fetch fault after finally
                                if (EnvironmentPreSampleAuthorityChanged(in pending))
                                    return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                                continue;
                            }
                            if (lease.Token != pending.Origin || index != pending.Identity.Site)
                                return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                            var plan = FullTranslationJit.Plans(fullJit.Store, in lease)[(int)index];
                            if (!SameNativeInstruction(in plan, in pending.Plan) ||
                                !NativeProcessorEnvironment(in plan, out var liveInstruction) || liveInstruction != instruction)
                                return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                            // This is one exact approved site, regardless of the
                            // ordinary multi-site eligibility result. Actual native
                            // enrollment remains mandatory, including cap1 bodies.
                            fullJit.AuthorizeLinks(in lease, multiple, index, nativeLimit: 1);
                            var header = fullJit.Store.GetHeaderCopy(in lease);
                            if (header.OwnerCookie != pending.Identity.OwnerCookie ||
                                header.BodyGeneration != pending.Identity.BodyGeneration || header.Context != pending.Identity.Context ||
                                header.Slot != pending.Origin.Slot || header.Entry == 0)
                                throw new InvalidOperationException("ENV1 reacquired foreign immutable header identity.");
                            if (header.Live != 1 || header.CodeEpoch == 0 || header.CodeEpoch != header.ValidatedCodeEpoch ||
                                header.ValidatedGlobalEpoch == 0 || header.ValidatedGlobalEpoch != fullJit.Authority.CodeEpoch)
                            {
                                if (EnvironmentPreSampleAuthorityChanged(in pending))
                                    return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                                continue;
                            }
                            worker.NativePoll ??= worker.Stop.AcquireNativePoll();
                            frame = new()
                            {
                                Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize,
                                Machine = worker.Machine, Stop = worker.NativePoll.Pointer, Budget = instructionBudget,
                                NativeLimit = 1, FirstPreissued = 1, NextIp = instruction.Ip,
                                Header = (TranslationBlockHeader*)fullJit.Store.GetHeader(in lease),
                                OwnerCookie = header.OwnerCookie, BodyGeneration = header.BodyGeneration, Context = header.Context,
                                OwnerHandle = unchecked((nint)worker.FullCache.Serial),
                                GlobalEpochPointer = fullJit.Authority.EpochPointer, ObservedGlobalEpoch = fullJit.Authority.Epoch,
                                CodeAuthorityEpochPointer = fullJit.Authority.CodeEpochPointer,
                                ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch,
                                StoreGenerationPointer = fullJit.Store.GenerationPointer,
                                ObservedStoreGeneration = fullJit.Store.Generation,
                                ResumeIndex = index, HelperIndex = FullBlockFrame.NoHelperIndex,
                                StackMemory = worker.FullNativeState!.StackMemory, X87 = GetX87NativePointer(worker),
                                FsBase = worker.Machine->fs.@base, GsBase = worker.Machine->gs.@base
                            };
                            ownedHeader = frame.Header; entry = header.Entry;
                            var request = pending.Identity;
                            // This refresh is unsampled, under an exact live
                            // Plan/token lease and authenticated current stamps.
                            request.Header = frame.Header; request.GlobalEpoch = frame.ObservedGlobalEpoch;
                            request.CodeEpoch = frame.ObservedCodeAuthorityEpoch; request.StoreGeneration = frame.ObservedStoreGeneration;
                            var requestPointer = (FullEnvironmentDataDescriptor*)frame.StackMemory;
                            *requestPointer = request;
                            expectedRequest = request;
                            var expected = frame;
                            FullControllerNativePrepared(&frame);
                            RequireExactEnvironmentFrame(in frame, in expected);
                            ValidateOwnedX87KernelFrame(worker, in frame);
                            fullJit.RequireX87KernelAdmission(in frame);
                        }
                    }
                }
                finally { worker.FullFetchActive = previousFetch; fullJit.EndRead(program!); }

                if (EnvironmentPaused(worker, in instruction, out paused)) return paused;
                if (EnvironmentPreSampleAuthorityChanged(in pending))
                    return RetryInvalidatedEnvironment(worker, in pending, in expectedRequest, signalActor);
                // No fetch/native/store epoch or Gate enrollment here. The
                // retained TranslationLease alone prevents origin recycling.
                if (!FullProcessorEnvironmentLease.TryAcquire(program!, fullJit.Authority, worker.FullCache.Worker,
                    instruction.Options == (uint)FullProcessorEnvironmentOperation.Rdrand, out service))
                    continue;
                // Under service/RNG ownership, only bounded scalar checks and
                // explicitly NONBLOCKING admission. Never Acquire/GetHeader/
                // Plans/Generation/BeginRead/Drain here: those can take locks.
                if (!Monitor.TryEnter(fullJit.Authority.Gate)) continue;
                try
                {
                    if (worker.Machine->ip != instruction.Ip || worker.Machine->oplen != 0 ||
                        FullTranslationJit.Context(program!, worker.Machine, signalActor ? 1u : 0u) != pending.Identity.Context)
                        throw new InvalidOperationException("ENV1 current instruction/context changed before sample.");
                    if (EnvironmentPaused(worker, in instruction, out paused)) return paused;
                    if (frame.ObservedGlobalEpoch == 0 || fullJit.Authority.Epoch != frame.ObservedGlobalEpoch ||
                        frame.ObservedCodeAuthorityEpoch == 0 || fullJit.Authority.CodeEpoch != frame.ObservedCodeAuthorityEpoch)
                        continue;
                    fullJit.RequireX87KernelAdmission(in frame);
                    // Parent-owned additive Store seam uses Monitor.TryEnter on
                    // its private gate, identical epoch ticket/check logic.
                    if (!fullJit.Store.TryBeginNativeEpochNonblocking(out epoch)) continue;
                    // The saved pointer is read only AFTER both exact retained
                    // lease and actual store epoch protect this immutable body.
                    var header = ownedHeader;
                    if (header->OwnerCookie != frame.OwnerCookie || header->BodyGeneration != frame.BodyGeneration ||
                        header->Context != frame.Context || header->Slot != pending.Origin.Slot || header->Entry != entry ||
                        header->Live != 1 || header->CodeEpoch == 0 || header->CodeEpoch != header->ValidatedCodeEpoch ||
                        header->ValidatedGlobalEpoch != frame.ObservedCodeAuthorityEpoch ||
                        Volatile.Read(ref *frame.StoreGenerationPointer) != frame.ObservedStoreGeneration)
                        continue;
                    var request = (FullEnvironmentDataDescriptor*)frame.StackMemory;
                    if (!FullProcessorEnvironmentProtocol.SameIdentity(in *request, &frame, in instruction))
                        throw new InvalidOperationException("ENV1 refreshed request identity changed before enrollment.");
                    if (request->State != FullEnvironmentDataState.Request || request->Data0 != 0 || request->Data1 != 0 ||
                        request->RngState != null || request->RngCount != null || request->LeaseCookie != 0)
                        throw new InvalidOperationException("ENV1 pre-sample request already owns sampled evidence.");
                    ValidateOwnedX87KernelFrame(worker, in frame);
                    enrolled = fullJit.Authority.TryEnterProcessorEnvironmentNative(worker.FullCache.Worker, frame.ObservedGlobalEpoch);
                    if (!enrolled) continue; // finally releases ALL ownership before retry
                }
                finally { Monitor.Exit(fullJit.Authority.Gate); }

                fixed (long* issuedPointer = &issued)
                {
                    frame.Issued = issuedPointer; frame.Completed = &nativeCompleted;
                    // All stop/context/trap/epoch/eligibility checks are complete.
                    // From PrepareReady through RET, no Poll/Issue/restamp or
                    // reacquisition, even when writers advance epochs or cancel.
                    FullProcessorEnvironmentProtocol.PrepareReady(&frame, in instruction, service!, enrolled);
                    EnterX87Native(worker);
                    ((delegate* unmanaged<FullBlockFrame*, void>)entry)(&frame);
                    // Only this straight-line path proves RET. Keep the x87 guard first;
                    // publish even if it rejects, before any other managed consumer.
                    try { X87NativeReturned(worker); }
                    finally
                    {
                        if (nativeCompleted != 0) Interlocked.Add(ref completed, nativeCompleted);
                    }
                    if (frame.Header != ownedHeader || frame.StackMemory != worker.FullNativeState!.StackMemory ||
                        frame.Machine != worker.Machine || unchecked((ulong)frame.OwnerHandle) != worker.FullCache.Serial ||
                        frame.HostMxcsrDirty != 0)
                        throw new InvalidOperationException("ENV1 native return lost its exact invocation ownership.");
                    ValidateOwnedX87KernelFrame(worker, in frame);
                    ValidateX87KernelReturn(worker, in frame);
                    FullProcessorEnvironmentProtocol.RequireConsumed(&frame, in instruction, service!);
                    FullControllerNativeReturned(&frame);
                    if (frame.Header != ownedHeader || frame.StackMemory != worker.FullNativeState!.StackMemory ||
                        frame.Machine != worker.Machine)
                        throw new InvalidOperationException("ENV1 return callback changed invocation ownership.");
                    FullProcessorEnvironmentProtocol.ResetConsumed(&frame, in instruction, service!);
                }
                fullJit.NativeCompleted(in frame);
                worker.Instructions += frame.Executed;
                worker.FullResume = pending.Origin; worker.FullResumeIndex = frame.ResumeIndex;
                // Return only a copied ordinary outcome; no expired stack/global
                // or code pointer can escape to signal/Actor/fault handling.
                return new()
                {
                    Version = frame.Version, Size = frame.Size, Machine = worker.Machine, Exit = frame.Exit,
                    Executed = frame.Executed, FirstPreissued = frame.FirstPreissued,
                    NextIp = frame.NextIp, ResumeIndex = frame.ResumeIndex, HelperIndex = frame.HelperIndex
                };
            }
            catch (Exception failure) when (service?.Sampled == true)
            {
                fullJit.Authority.QuarantineSemanticAdmission();
                // Always snapshot the genuine worker allocation, never a possibly
                // corrupt returned frame.StackMemory pointer. Preserve it in situ.
                var owned = (FullEnvironmentDataDescriptor*)worker.FullNativeState!.StackMemory;
                var evidence = *owned;
                throw new FullEnvironmentTerminalException(in frame, in evidence, in pending, failure);
            }
            finally
            {
                // Normal path acknowledged while ALL capabilities were held.
                // Failure keeps evidence. Release actual RNG/service first so no
                // store-gate cleanup can wait while holding the global mutex.
                try
                {
                    try { service?.Dispose(); }
                    finally
                    {
                        try { if (enrolled) fullJit.Authority.ExitNative(); }
                        finally
                        {
                            try { epoch.Dispose(); }
                            finally { lease.Dispose(); }
                        }
                    }
                }
                catch (Exception cleanupFailure) when (service?.Sampled == true)
                {
                    fullJit.Authority.QuarantineSemanticAdmission();
                    var owned = (FullEnvironmentDataDescriptor*)worker.FullNativeState!.StackMemory;
                    var evidence = *owned;
                    throw new FullEnvironmentTerminalException(in frame, in evidence, in pending, cleanupFailure);
                }
            }
            // Every continue unwinds the service, enrollment, epoch and token.
        }
    }
}
