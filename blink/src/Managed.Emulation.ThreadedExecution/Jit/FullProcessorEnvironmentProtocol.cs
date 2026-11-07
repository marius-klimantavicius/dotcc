using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal enum FullEnvironmentDataState : uint { Empty, Request, Ready, Consumed }

// A tagged union over the existing worker-owned128 bytes at Frame.StackMemory.
// Never pass ENV1 to a memory descriptor resolver, TLB publisher or ticket API.
[StructLayout(LayoutKind.Explicit, Size = 128)]
internal unsafe struct FullEnvironmentDataDescriptor
{
    internal const uint TagValue = 0x31564e45, CurrentVersion = 1;
    [FieldOffset(0)] internal uint Tag;
    [FieldOffset(4)] internal uint Version;
    [FieldOffset(8)] internal uint Site;
    [FieldOffset(12)] internal FullEnvironmentDataState State;
    [FieldOffset(16)] internal TranslationBlockHeader* Header;
    [FieldOffset(24)] internal ulong BodyGeneration;
    [FieldOffset(32)] internal ulong OwnerCookie;
    [FieldOffset(40)] internal ulong Context;
    [FieldOffset(48)] internal ulong GlobalEpoch;
    [FieldOffset(56)] internal ulong CodeEpoch;
    [FieldOffset(64)] internal ulong StoreGeneration;
    [FieldOffset(72)] internal ulong Ip;
    [FieldOffset(80)] internal FullProcessorEnvironmentOperation Operation;
    [FieldOffset(84)] internal uint Width;
    [FieldOffset(88)] internal ulong Data0;
    [FieldOffset(96)] internal ulong Data1;
    [FieldOffset(104)] internal ulong* RngState;
    [FieldOffset(112)] internal uint* RngCount;
    [FieldOffset(120)] internal ulong LeaseCookie;
}

internal static unsafe partial class FullProcessorEnvironmentProtocol
{
    static partial void DataReady(FullBlockFrame* frame);

    internal static bool IsEnvironment(FullBlockFrame* frame) => frame->StackMemory != null &&
        ((FullEnvironmentDataDescriptor*)frame->StackMemory)->Tag == FullEnvironmentDataDescriptor.TagValue;

    internal static bool SameIdentity(in FullEnvironmentDataDescriptor d, FullBlockFrame* f,
        in JitInstruction i) => f->Version == FullBlockFrame.AbiVersion && f->Size == FullBlockFrame.ByteSize &&
        f->StackMemory != null && f->Header != null && d.Tag == FullEnvironmentDataDescriptor.TagValue && d.Version == 1 &&
        d.Site == f->ResumeIndex && d.Header == f->Header && d.BodyGeneration == f->BodyGeneration &&
        d.OwnerCookie == f->OwnerCookie && d.Context == f->Context && d.GlobalEpoch == f->ObservedGlobalEpoch &&
        d.CodeEpoch == f->ObservedCodeAuthorityEpoch && d.StoreGeneration == f->ObservedStoreGeneration &&
        d.Ip == i.Ip && d.Ip == ((Blink.Machine*)f->Machine)->ip && d.Operation == (FullProcessorEnvironmentOperation)i.Options &&
        d.Width == i.Width && FullBlockEmitter.CanInlineProcessorEnvironment(i) &&
        f->HelperIndex == FullBlockFrame.NoHelperIndex && f->MemoryResume == 0 && f->MemoryTicket == 0 &&
        f->MemoryFlags == FullMemoryFlags.None;

    // The parent must first copy the exact typed Plan from the retained current
    // header/site under its real store epoch. A tuple alone is not ownership.
    internal static FullEnvironmentDataDescriptor Capture(FullBlockFrame* f, in JitInstruction i)
    {
        if (!IsEnvironment(f)) throw new InvalidOperationException("ENV1 owned descriptor missing");
        var d = *(FullEnvironmentDataDescriptor*)f->StackMemory;
        if (f->Exit != FullBlockExit.NeedEnvironment || !SameIdentity(in d, f, in i) ||
            d.State != FullEnvironmentDataState.Request || f->FirstPreissued != 0 ||
            d.Data0 != 0 || d.Data1 != 0 || d.RngState != null || d.RngCount != null || d.LeaseCookie != 0)
            throw new InvalidOperationException("ENV1 typed request authentication failed");
        return d;
    }

    // Called after original Plan/token reacquisition, ordinary prechecks and
    // actual native enrollment, BEFORE any service. Original captured identity
    // must have been validated by the parent; never refresh a sampled record.
    internal static void PrepareReady(FullBlockFrame* f, in JitInstruction i,
        FullProcessorEnvironmentLease lease, bool actuallyEnrolled)
    {
        if (!actuallyEnrolled || !lease.IsHeld || lease.GateHeld || !IsEnvironment(f))
            throw new InvalidOperationException("ENV1 requires real enrollment and service lease");
        var d = (FullEnvironmentDataDescriptor*)f->StackMemory;
        if (!SameIdentity(in *d, f, in i) || d->State != FullEnvironmentDataState.Request ||
            f->FirstPreissued != 1 || f->Executed != 0 || f->NativeLimit != 1 ||
            d->LeaseCookie != 0 || lease.Sampled)
            throw new InvalidOperationException("ENV1 pre-sample identity or state invalid");
        if ((i.Options is (uint)FullProcessorEnvironmentOperation.Rdtsc or (uint)FullProcessorEnvironmentOperation.Rdtscp) &&
            (int)((Blink.Machine*)f->Machine)->traprdtsc != 0)
            throw new InvalidOperationException("ENV1 trapped clock reached sampler");
        bool rng = i.Options == (uint)FullProcessorEnvironmentOperation.Rdrand;
        if (rng != lease.HasRng) throw new InvalidOperationException("ENV1 lease kind mismatch");
        f->Temporary0 = rng ? (ulong)lease.State : 0;
        f->Temporary1 = rng ? (ulong)lease.Count : 0;
        f->Temporary3 = lease.Cookie;
        d->RngState = rng ? lease.State : null; d->RngCount = rng ? lease.Count : null;
        d->LeaseCookie = lease.Cookie;
        // Mark Sampled BEFORE entering even an erroring/missing service.
        // No handler/WriteRegister/SetFlag/Vigna call occurs in this layer.
        lease.MarkSampled();
        switch ((FullProcessorEnvironmentOperation)i.Options)
        {
            case FullProcessorEnvironmentOperation.Rdtsc:
            case FullProcessorEnvironmentOperation.Rdtscp:
                Blink.Libc.timespec ts = default;
                _ = Blink.blink_host_clock_gettime(1, &ts);
                d->Data0 = unchecked((ulong)ts.tv_sec); d->Data1 = unchecked((ulong)ts.tv_nsec);
                break;
            case FullProcessorEnvironmentOperation.Rdseed:
                ulong entropy = 0;
                _ = Blink.blink_host_getrandom(&entropy, 8, 0);
                d->Data0 = entropy; d->Data1 = 0;
                break;
            case FullProcessorEnvironmentOperation.Rdrand:
                // Read only service eligibility while owning the SAME global
                // mutex. Count increment, state install and Vigna stay emitted.
                ulong seed = *lease.State;
                if ((*lease.Count & 15) == 0) _ = Blink.blink_host_getrandom(&seed, 8, 0);
                d->Data0 = seed; d->Data1 = 0;
                break;
            default: throw new InvalidOperationException("ENV1 exact typed operation missing");
        }
        d->State = FullEnvironmentDataState.Ready;
        DataReady(f); // test-only after-sample interleaving; erased in production
    }

    // Mandatory immediately after RET and BEFORE disposing the real RNG lease.
    // Every failure after sample is INTERNAL TERMINAL, never a guest fault,
    // retry, semantic helper or re-sampling path.
    internal static void RequireConsumed(FullBlockFrame* f, in JitInstruction i,
        FullProcessorEnvironmentLease lease)
    {
        var d = (FullEnvironmentDataDescriptor*)f->StackMemory;
        if (!lease.IsHeld || !lease.Sampled || d == null || d->Tag != FullEnvironmentDataDescriptor.TagValue ||
            d->Version != 1 || d->State != FullEnvironmentDataState.Consumed || d->LeaseCookie != lease.Cookie ||
            d->Operation != (FullProcessorEnvironmentOperation)i.Options || d->Ip != i.Ip || d->Width != i.Width ||
            d->Header != f->Header || d->BodyGeneration != f->BodyGeneration || d->OwnerCookie != f->OwnerCookie ||
            d->Context != f->Context || d->GlobalEpoch != f->ObservedGlobalEpoch ||
            d->CodeEpoch != f->ObservedCodeAuthorityEpoch || d->StoreGeneration != f->ObservedStoreGeneration ||
            d->RngState != lease.State || d->RngCount != lease.Count || f->Temporary0 != (ulong)lease.State ||
            f->Temporary1 != (ulong)lease.Count || f->Temporary3 != lease.Cookie ||
            f->HelperIndex != FullBlockFrame.NoHelperIndex || f->NativeLimit != 1 ||
            f->Exit != FullBlockExit.Continue || f->Executed != 1 || f->FirstPreissued != 0 ||
            f->ResumeIndex != d->Site + 1 || ((Blink.Machine*)f->Machine)->ip != unchecked(i.Ip + (uint)i.Length))
            throw new InvalidOperationException("ENV1 sampled operation did not commit exactly once; terminal");
    }
    // Mandatory acknowledgement AFTER RET while the real lease/enrollment/store
    // epoch remain held. This NEVER clears Request/Ready or malformed sampled
    // evidence. A terminal controller error must quarantine its record instead.
    internal static void ResetConsumed(FullBlockFrame* f, in JitInstruction i,
        FullProcessorEnvironmentLease lease)
    {
        RequireConsumed(f, in i, lease);
        *f->StackMemory = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
        f->Temporary0 = f->Temporary1 = f->Temporary3 = 0;
    }

}
