using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    private long lastExitLinkAttempts, lastExitLinksCompleted; // Authority.Gate only
    // A worker owns one nonowning predecessor value between controller calls.
    // No header, payload, code pointer or lease survives the native epoch.
    internal readonly record struct LastExitLink(TranslationToken Source, ulong StoreGeneration,
        ulong OwnerCookie, ulong Context, ulong SourcePc, ulong SitePc, ulong TargetPc,
        ulong SourceCodeEpoch, ulong CodeAuthorityEpoch, ulong WorkerInstructions, uint SiteIndex, int Edge);

    internal LastExitLink CaptureLastExitLink(Blink program, in NativeEpochLease epoch, in FullBlockFrame frame,
        ulong workerInstructions)
    {
        // ABI5 Continue is emitted only by GuardCap, static Link, and dynamic
        // jump-cache dispatch. Excluding cap/barrier returns, then requiring a
        // native STATIC terminal site, proves its Commit reached static Link.
        // ResumeIndex alone is insufficient: a capped internal branch can
        // stop at that same index with the same next PC before dispatch.
        if (epoch.Owner != Store || frame.Exit != FullBlockExit.Continue || frame.NativeLimit != 0 ||
            (frame.MemoryFlags & FullMemoryFlags.BarrierConsumed) != 0 || frame.Executed == 0 ||
            frame.FirstPreissued != 0 || frame.HelperIndex != FullBlockFrame.NoHelperIndex ||
            frame.Header == null || frame.Machine == null) return default;
        var machine = (Blink.Machine*)frame.Machine;
        if (machine->oplen != 0 || machine->ip != frame.NextIp || (int)machine->attention != 0 ||
            frame.Context != Context(program, machine) ||
            frame.ObservedGlobalEpoch != Authority.Epoch ||
            frame.ObservedCodeAuthorityEpoch != Authority.CodeEpoch ||
            frame.ObservedStoreGeneration != Store.Generation) return default;
        var token = new TranslationToken(checked((int)frame.Header->Slot), frame.Header->BodyGeneration);
        // The caller's global epoch protects even a tail-reached header. The
        // store must independently recognize this exact resident token/address.
        nint authenticated = Store.GetHeader(in epoch, token);
        if (authenticated == 0 || authenticated != (nint)frame.Header) return default;
        var header = ((TranslationBlockHeader*)authenticated)[0];
        if (!TrustedLinkHeader(in header, frame.OwnerCookie, frame.Context, frame.ObservedCodeAuthorityEpoch) ||
            header.BodyGeneration != frame.BodyGeneration || header.PayloadBytes == 0 ||
            header.PayloadBytes % sizeof(Plan) != 0 || header.Payload == 0) return default;
        if (!TryReadStaticExit(in header, out var exit) || frame.ResumeIndex != (uint)exit.Count - 1)
            return default;
        bool match0 = exit.Edge0 && frame.NextIp == exit.Fallthrough;
        bool match1 = exit.Edge1 && frame.NextIp == exit.Taken;
        // Equal destinations do not identify the selected edge; an internal
        // target never leaves native Dispatch. Both were classified once at
        // translation time, not reconstructed from guest bytes at this return.
        if (match0 == match1 || (match1 && exit.TakenInternal)) return default;
        return new(token, frame.ObservedStoreGeneration, header.OwnerCookie, header.Context,
            header.GuestPc, exit.SitePc, frame.NextIp, header.CodeEpoch,
            frame.ObservedCodeAuthorityEpoch, workerInstructions, frame.ResumeIndex, match1 ? 1 : 0);
    }

    private static bool TrustedLinkHeader(in TranslationBlockHeader header, ulong owner, ulong context, ulong codeStamp)
        => header.OwnerCookie == owner && header.Context == context && (context & FlatLongContext) != 0 &&
            header.Live == 1 && header.Reserved136 == 1 && header.Entry != 0 && header.CodeEpoch != 0 &&
            header.CodeEpoch == header.ValidatedCodeEpoch && header.ValidatedGlobalEpoch == codeStamp;

    internal bool CompleteLastExitLink(in LastExitLink candidate, in TranslationLease target,
        uint index, bool multiple, uint nativeLimit, ulong context, ulong workerInstructions)
    {
        if (!Monitor.IsEntered(Authority.Gate)) throw new InvalidOperationException("Last-exit linking requires authority gate.");
        if (candidate.Source.Generation == 0) return false;
        ++lastExitLinkAttempts;
        if ((uint)candidate.Edge > 1 || target.Owner != Store ||
            index != 0 || !multiple || nativeLimit != 0 || candidate.Context != context ||
            candidate.OwnerCookie != Store.OwnerCookie || candidate.StoreGeneration != Store.Generation ||
            candidate.CodeAuthorityEpoch != Authority.CodeEpoch || candidate.WorkerInstructions != workerInstructions)
            return false;
        var destination = Store.GetHeaderCopy(in target);
        if (destination.GuestPc != candidate.TargetPc ||
            !TrustedLinkHeader(in destination, candidate.OwnerCookie, context, candidate.CodeAuthorityEpoch)) return false;
        if (!Store.TryAcquireToken(candidate.Source, out var source, requireLive: true)) return false;
        using (source)
        {
            var header = Store.GetHeaderCopy(in source);
            if (header.GuestPc != candidate.SourcePc || header.CodeEpoch != candidate.SourceCodeEpoch ||
                !TrustedLinkHeader(in header, candidate.OwnerCookie, context, candidate.CodeAuthorityEpoch)) return false;
            if (!TryReadStaticExit(in header, out var exit) || candidate.SiteIndex != (uint)exit.Count - 1 ||
                exit.SitePc != candidate.SitePc ||
                (candidate.Edge == 0 ? !exit.Edge0 || exit.Fallthrough != candidate.TargetPc :
                    !exit.Edge1 || exit.TakenInternal || exit.Taken != candidate.TargetPc))
                return false;
            // This is the only patch. Store owns reverse adjacency and clears
            // incoming cells before retirement/reuse. Native guards remain the
            // authority at every site and at every linked transfer.
            if (!Store.TryLink(source.Token, candidate.Edge, target.Token)) return false;
            ++lastExitLinksCompleted;
            return true;
        }
    }
}
