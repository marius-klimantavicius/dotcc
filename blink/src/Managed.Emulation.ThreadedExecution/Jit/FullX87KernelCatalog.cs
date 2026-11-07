namespace Managed.Emulation.Execution;

// Compile-time identities only. There is no opcode selector in WorkFrame384.
// Explicit values keep this finite registry independent of emitter enum layouts.
internal enum FullX87KernelKey : byte
{
    Add = 0, Subtract = 1, Multiply = 2, Divide = 3,
    LoadBinary32 = 4, LoadBinary64 = 5, LoadInt16 = 6, LoadInt32 = 7, LoadInt64 = 8,
    StoreBinary32 = 9, StoreBinary64 = 10, StoreInt16 = 11, StoreInt32 = 12, StoreInt64 = 13,
    TruncateInt16 = 14, TruncateInt32 = 15, TruncateInt64 = 16, LoadRaw80 = 17, StoreRaw80 = 18,
    Fcom = 19, Fcomp = 20, Fcompp = 21, Fucom = 22, Fucomp = 23, Fucompp = 24,
    Fcomi = 25, Fcomip = 26, Fucomi = 27, Fucomip = 28, RoundToIntegral = 29,
    Sqrt = 30, Extract = 31,
    MemoryAddBinary32 = 32,
    MemoryMultiplyBinary32 = 33,
    MemorySubtractBinary32 = 34,
    MemorySubtractReverseBinary32 = 35,
    MemoryDivideBinary32 = 36,
    MemoryDivideReverseBinary32 = 37,
    MemoryAddBinary64 = 38,
    MemoryMultiplyBinary64 = 39,
    MemorySubtractBinary64 = 40,
    MemorySubtractReverseBinary64 = 41,
    MemoryDivideBinary64 = 42,
    MemoryDivideReverseBinary64 = 43,
    MemoryAddInt32 = 44,
    MemoryMultiplyInt32 = 45,
    MemorySubtractInt32 = 46,
    MemorySubtractReverseInt32 = 47,
    MemoryDivideInt32 = 48,
    MemoryDivideReverseInt32 = 49,
    MemoryAddInt16 = 50,
    MemoryMultiplyInt16 = 51,
    MemorySubtractInt16 = 52,
    MemorySubtractReverseInt16 = 53,
    MemoryDivideInt16 = 54,
    MemoryDivideReverseInt16 = 55,
    MemoryFcomBinary32 = 56,
    MemoryFcompBinary32 = 57,
    MemoryFcomBinary64 = 58,
    MemoryFcompBinary64 = 59,
    MemoryFcomInt32 = 60,
    MemoryFcompInt32 = 61,
    MemoryFcomInt16 = 62,
    MemoryFcompInt16 = 63,
    LoadPackedBcd = 64,
    StorePackedBcd = 65,
    Scale = 66,
    Remainder = 67,
    RemainderNearest = 68,
    F2xm1 = 69, Fyl2x = 70, Fptan = 71, Fpatan = 72, Fyl2xp1 = 73,
    Fsincos = 74, Fsin = 75, Fcos = 76
}

// Retains the owner object, not an independent code lifetime or native ticket.
// Entry is deliberately validated on access; raw addresses already embedded in
// a TB still REQUIRE the parent's checked catalog/native admission protocol.
internal readonly struct FullX87KernelBinding
{
    private readonly FullX87KernelCatalog? owner;
    internal FullX87KernelKey Key { get; }
    internal int Slot => (int)Key;
    internal int CodeBytes { get; }
    internal nint Entry => owner != null ? owner.EntryFor(in this) :
        throw new InvalidOperationException("Unbound x87 kernel");
    internal FullX87KernelBinding(FullX87KernelCatalog owner, FullX87KernelKey key, int codeBytes)
    { this.owner = owner; Key = key; CodeBytes = codeBytes; }
    internal bool BelongsTo(FullX87KernelCatalog candidate) => ReferenceEquals(owner, candidate);
}

// Owner-immutable, eagerly sealed catalog. Never reset, evict, append after
// initialization, or participate in the TB store's bulk flush. All entries are
// R11 WorkFrame384/v1 -> JMP [Work+88] kernels, never host-ABI call targets.
internal sealed unsafe partial class FullX87KernelCatalog : IDisposable
{
    internal const int MaximumKernels = 77;
    internal const int StagingBytes = 65536;
    internal const int RegionBytes = 65536;
    // Measured c107 all8 maximum43646 (RET/R11 and both ABIs) fits one64KiB
    // region. Staging may occupy a WHOLE region; the former half-region packing
    // bound is invalid. Any next-fit append allocates at most one new region,
    // hence77 entries require at most77 regions, irrespective of alignment.
    // Reservations remain incremental and quota-charged BEFORE each mapping.
    internal const long MaximumCodeBytes = MaximumKernels * (long)RegionBytes;
    private const long OwnerAllowance = 1024, ArrayHeaderAllowance = 32;
    private struct Entry { public nint Address; public int CodeBytes; }
    internal static long FixedCharge => OwnerAllowance + ArrayHeaderAllowance +
        MaximumKernels * (long)sizeof(Entry) +
        TranslationCodeRegions.MetadataBytes(MaximumCodeBytes, RegionBytes);

    private readonly FullCodeAuthority authority;
    private readonly RetainedTranslationStore quota;
    private readonly bool allowUnverifiedWindows;
    private Entry[]? entries;
    private TranslationCodeRegions? code;
    private bool attempted, ready, closing, barrierAdvanced, fixedCharged, disposed;
    internal int Count => ready ? MaximumKernels : 0;
    internal long EmittedBytes { get; private set; }
    internal long ReservedBytes => code?.ReservedBytes ?? 0;
    internal long ChargedBytes => (fixedCharged ? FixedCharge : 0) + ReservedBytes;
    internal bool AdmissionOpen
    {
        get { RequireGate(); return ready && !closing && !disposed && code != null && !code.Quarantined; }
    }

    // Only while holding this owner's real TB lease + store native epoch.
    // Initialization release-publishes ready; afterward code is immutable until
    // CloseAdmission publishes closing, advances runtime, and drains native.
    // The caller checks AFTER SC enrollment. Unmap additionally requires the
    // original Store.NativeLifetimeClosed, including this caller's root lease.
    internal bool RetainedNativeAdmissionOpen => Volatile.Read(ref ready) &&
        !Volatile.Read(ref closing) && !Volatile.Read(ref disposed) &&
        code != null && !code.Quarantined;

    // No registry array, native resource or quota acquired in the constructor. The caller MUST
    // register this object in cleanup before Initialize, which may throw after
    // acquiring a charged mapping. Never discard a failed/partly disposed owner.
    internal FullX87KernelCatalog(FullCodeAuthority authority, RetainedTranslationStore quota,
        bool allowUnverifiedWindows)
    {
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(quota);
        this.authority = authority; this.quota = quota;
        this.allowUnverifiedWindows = allowUnverifiedWindows;
    }

    // Startup only, BEFORE any owner TB publication/native entry. Borrow pooled
    // staging; do not retain it. Gate excludes compiler/admission/disposal; no
    // entry escapes while packed pages may transition back to RW. Failed startup
    // is terminal and retryable only as cleanup, never as partial publication.
    internal bool Initialize(Span<byte> staging, out TranslationStoreFailure failure)
    {
        RequireGate();
        failure = TranslationStoreFailure.None;
        if (attempted || closing || disposed) throw new InvalidOperationException("x87 catalog initialization is one-shot");
        if (staging.Length < StagingBytes) throw new ArgumentException($"x87 staging requires at least {StagingBytes} bytes", nameof(staging));
        attempted = true;
        try
        {
            if (!quota.TryReserveExternalBytes(FixedCharge, out failure)) return false;
            fixedCharged = true;
            // Reserve the entire bounded fixed charge BEFORE array allocation.
            // There is no native mapping yet: an OOM can refund this reservation
            // immediately without losing ownership of a native resource.
            try { entries = new Entry[MaximumKernels]; }
            catch { quota.ReleaseExternalBytes(FixedCharge); fixedCharged = false; throw; }
            // TranslationCodeRegions acquires its metadata last in construction;
            // no throwing managed setup follows that acquisition in its ctor.
            code = new(MaximumCodeBytes, RegionBytes, allowUnverifiedWindows, requireNativeFlags: false);
            for (int slot = 0; slot < MaximumKernels; ++slot)
            {
                if (!Emit((FullX87KernelKey)slot, staging[..StagingBytes], out int length) ||
                    length <= 0 || length > StagingBytes)
                    throw new InvalidOperationException("x87 continuation emission failed");
                BeforeStage(slot, staging[..StagingBytes], ref length);
                if (length <= 0 || length > StagingBytes) throw new InvalidOperationException("Invalid x87 kernel length");
                long extra = code.AdditionalReservationBytes(length);
                if (!quota.TryReserveExternalBytes(extra, out failure)) return false;
                long before = code.ReservedBytes;
                try
                {
                    if (!code.TryStage(length, out var run, out nint address, out failure)) return false;
                    staging[..length].CopyTo(new Span<byte>((void*)address, length));
                    BeforeSeal(slot, run);
                    if (!code.TrySeal(run))
                        throw new InvalidOperationException("x87 RX publication failed; charged owner retained for cleanup");
                    code.Commit(run, length);
                    entries![slot] = new() { Address = address, CodeBytes = length };
                    EmittedBytes += length;
                }
                finally
                {
                    // Includes exceptions after a successful mmap but failed
                    // staging/protection. Never refund a retained mapping.
                    long unused = extra - (code.ReservedBytes - before);
                    if (unused != 0) quota.ReleaseExternalBytes(unused);
                }
            }
            Volatile.Write(ref ready, true); // the ONLY entry-publication point
            return true;
        }
        finally { if (!ready) closing = true; }
    }

    internal FullX87KernelBinding Find(FullX87KernelKey key)
    {
        RequireGate(); RequireAdmission();
        int slot = CheckedSlot(key);
        return new(this, key, entries![slot].CodeBytes);
    }
    internal bool IsValid(in FullX87KernelBinding binding)
    {
        RequireGate();
        return AdmissionOpen && binding.BelongsTo(this) && (uint)binding.Slot < MaximumKernels &&
            entries![binding.Slot].Address != 0 && entries![binding.Slot].CodeBytes == binding.CodeBytes;
    }
    internal nint EntryFor(in FullX87KernelBinding binding)
    {
        RequireGate();
        if (!IsValid(in binding)) throw new InvalidOperationException("Unavailable or foreign x87 kernel binding");
        return entries![binding.Slot].Address;
    }

    // The tuple is meaningful only under this owner Gate. Used when binding
    // compiler IR and by the family qualification tests; copying it creates no
    // native lifetime. Enrollment/root lease still spans kernel through RET.
    internal bool Authenticates(in FullX87KernelCall call)
    {
        RequireGate();
        return AdmissionOpen && call.OwnerCookie == quota.OwnerCookie &&
            (uint)call.Key < MaximumKernels && call.Entry != 0 &&
            entries![call.Key].Address == call.Entry &&
            entries[call.Key].CodeBytes == call.CodeBytes && call.CodeBytes > 0;
    }

    // Gate-held entry checks AdmissionOpen; retained entry instead checks the
    // immutable catalog after its real store epoch + SC authority enrollment.
    // Both include cached TBs with literal entries and reject this closure.
    // Closing alone is not a quiescence proof. An epoch advance makes old native
    // TBs exit after bounded computational kernels; they retain their original
    // store epoch/root lease and native enrollment through the continuation.
    internal void CloseAdmission()
    {
        RequireGate();
        if (disposed) return;
        Volatile.Write(ref closing, true);
        if (!barrierAdvanced) { authority.Advance(); barrierAdvanced = true; }
        // On generation exhaustion, retain all charge/pointers and stay closed.
        // This deliberately does not claim to repair generic R101-04 recovery.
    }

    public void Dispose()
    {
        // ExitNative/finalizers must not be blocked on this caller's Gate.
        if (Monitor.IsEntered(authority.Gate))
            throw new InvalidOperationException("Dispose x87 catalog outside authority gate");
        lock (authority.Gate)
        {
            if (disposed) return;
            CloseAdmission();
        }
        authority.WaitNativeFrames(); // outside Gate; no callbacks inside kernels
        lock (authority.Gate)
        {
            if (disposed) return;
            // Borrowed quota is NEVER disposed here. This narrow parent-owned
            // getter locks the store gate and returns its final disposed flag,
            // set only after every original body lifetime has been released.
            // Failed or partial Store.Dispose must leave the proof false.
            if (!quota.NativeLifetimeClosed)
                throw new InvalidOperationException("Dispose original TB store before x87 catalog unmap");
            long before = ReservedBytes;
            try { code?.Dispose(); }
            finally
            {
                long released = before - ReservedBytes;
                if (released != 0) quota.ReleaseExternalBytes(released);
            }
            // Metadata remains charged when the arena's final release throws.
            if (fixedCharged) { quota.ReleaseExternalBytes(FixedCharge); fixedCharged = false; }
            code = null;
            if (entries != null) Array.Clear(entries);
            entries = null;
            disposed = true;
        }
    }

    private static int CheckedSlot(FullX87KernelKey key) => (uint)key < MaximumKernels ? (int)key :
        throw new ArgumentOutOfRangeException(nameof(key));
    private void RequireGate()
    { if (!Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("x87 catalog requires authority gate"); }
    private void RequireAdmission()
    { if (!AdmissionOpen) throw new InvalidOperationException("x87 catalog unavailable, quarantined or closing"); }

    // Test-only implementations erase from product builds. They use the actual
    // arena protection/unmap failure hooks; no alternate allocator is introduced.
    partial void BeforeStage(int slot, Span<byte> staging, ref int length);
    partial void BeforeSeal(int slot, TranslationCodeRun run);

    internal static FullX87KernelKey KeyFor(FullExtended80Operation operation) => operation switch
    {
        FullExtended80Operation.Add => FullX87KernelKey.Add,
        FullExtended80Operation.Subtract => FullX87KernelKey.Subtract,
        FullExtended80Operation.Multiply => FullX87KernelKey.Multiply,
        FullExtended80Operation.Divide => FullX87KernelKey.Divide,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
    internal static FullX87KernelKey KeyFor(FullExtended80Conversion operation) => operation switch
    {
        FullExtended80Conversion.LoadBinary32 => FullX87KernelKey.LoadBinary32,
        FullExtended80Conversion.LoadBinary64 => FullX87KernelKey.LoadBinary64,
        FullExtended80Conversion.LoadInt16 => FullX87KernelKey.LoadInt16,
        FullExtended80Conversion.LoadInt32 => FullX87KernelKey.LoadInt32,
        FullExtended80Conversion.LoadInt64 => FullX87KernelKey.LoadInt64,
        FullExtended80Conversion.StoreBinary32 => FullX87KernelKey.StoreBinary32,
        FullExtended80Conversion.StoreBinary64 => FullX87KernelKey.StoreBinary64,
        FullExtended80Conversion.StoreInt16 => FullX87KernelKey.StoreInt16,
        FullExtended80Conversion.StoreInt32 => FullX87KernelKey.StoreInt32,
        FullExtended80Conversion.StoreInt64 => FullX87KernelKey.StoreInt64,
        FullExtended80Conversion.TruncateInt16 => FullX87KernelKey.TruncateInt16,
        FullExtended80Conversion.TruncateInt32 => FullX87KernelKey.TruncateInt32,
        FullExtended80Conversion.TruncateInt64 => FullX87KernelKey.TruncateInt64,
        FullExtended80Conversion.LoadRaw80 => FullX87KernelKey.LoadRaw80,
        FullExtended80Conversion.StoreRaw80 => FullX87KernelKey.StoreRaw80,
        _ => throw new ArgumentOutOfRangeException(nameof(operation)) // BCD missing
    };
    internal static FullX87KernelKey KeyFor(FullExtended80Control operation) => operation switch
    {
        FullExtended80Control.Fcom => FullX87KernelKey.Fcom,
        FullExtended80Control.Fcomp => FullX87KernelKey.Fcomp,
        FullExtended80Control.Fcompp => FullX87KernelKey.Fcompp,
        FullExtended80Control.Fucom => FullX87KernelKey.Fucom,
        FullExtended80Control.Fucomp => FullX87KernelKey.Fucomp,
        FullExtended80Control.Fucompp => FullX87KernelKey.Fucompp,
        FullExtended80Control.Fcomi => FullX87KernelKey.Fcomi,
        FullExtended80Control.Fcomip => FullX87KernelKey.Fcomip,
        FullExtended80Control.Fucomi => FullX87KernelKey.Fucomi,
        FullExtended80Control.Fucomip => FullX87KernelKey.Fucomip,
        FullExtended80Control.RoundToIntegral => FullX87KernelKey.RoundToIntegral,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
    internal static FullX87KernelKey KeyFor(FullExtended80Transform operation) => operation switch
    {
        FullExtended80Transform.Sqrt => FullX87KernelKey.Sqrt,
        FullExtended80Transform.Extract => FullX87KernelKey.Extract,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    internal static FullX87KernelKey KeyFor(FullExtended80MemoryArithmeticOperation operation,
        FullExtended80MemoryArithmeticFormat format)
    {
        if ((uint)operation >= 6 || (uint)format >= 4)
            throw new ArgumentOutOfRangeException(nameof(operation));
        return (FullX87KernelKey)(32 + (int)format * 6 + (int)operation);
    }
    internal static FullX87KernelKey KeyForMemoryCompare(FullExtended80MemoryArithmeticFormat format, bool pop)
    {
        if ((uint)format >= 4) throw new ArgumentOutOfRangeException(nameof(format));
        return (FullX87KernelKey)(56 + (int)format * 2 + (pop ? 1 : 0));
    }
    internal static FullX87KernelKey KeyFor(FullExtended80BcdOperation operation) => operation switch
    {
        FullExtended80BcdOperation.LoadPackedBcd => FullX87KernelKey.LoadPackedBcd,
        FullExtended80BcdOperation.StorePackedBcd => FullX87KernelKey.StorePackedBcd,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
    internal static FullX87KernelKey KeyFor(FullExtended80Reduction operation) => operation switch
    {
        FullExtended80Reduction.Scale => FullX87KernelKey.Scale,
        FullExtended80Reduction.Remainder => FullX87KernelKey.Remainder,
        FullExtended80Reduction.RemainderNearest => FullX87KernelKey.RemainderNearest,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    internal static FullX87KernelKey KeyFor(FullExtended80Transcendental operation)
    {
        if ((uint)operation >= 8) throw new ArgumentOutOfRangeException(nameof(operation));
        return (FullX87KernelKey)(69 + (int)operation);
    }

    // This switch runs once per key at owner startup, never per guest instruction
    // or native kernel invocation. All selected emitters use true80 continuation.
    private static bool Emit(FullX87KernelKey key, Span<byte> staging, out int length) => key switch
    {
        FullX87KernelKey.Add => FullExtended80Emitter.TryEmitContinuation(FullExtended80Operation.Add, staging, out length),
        FullX87KernelKey.Subtract => FullExtended80Emitter.TryEmitContinuation(FullExtended80Operation.Subtract, staging, out length),
        FullX87KernelKey.Multiply => FullExtended80Emitter.TryEmitContinuation(FullExtended80Operation.Multiply, staging, out length),
        FullX87KernelKey.Divide => FullExtended80Emitter.TryEmitContinuation(FullExtended80Operation.Divide, staging, out length),
        FullX87KernelKey.LoadBinary32 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadBinary32, staging, out length),
        FullX87KernelKey.LoadBinary64 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadBinary64, staging, out length),
        FullX87KernelKey.LoadInt16 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadInt16, staging, out length),
        FullX87KernelKey.LoadInt32 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadInt32, staging, out length),
        FullX87KernelKey.LoadInt64 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadInt64, staging, out length),
        FullX87KernelKey.StoreBinary32 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreBinary32, staging, out length),
        FullX87KernelKey.StoreBinary64 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreBinary64, staging, out length),
        FullX87KernelKey.StoreInt16 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreInt16, staging, out length),
        FullX87KernelKey.StoreInt32 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreInt32, staging, out length),
        FullX87KernelKey.StoreInt64 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreInt64, staging, out length),
        FullX87KernelKey.TruncateInt16 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.TruncateInt16, staging, out length),
        FullX87KernelKey.TruncateInt32 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.TruncateInt32, staging, out length),
        FullX87KernelKey.TruncateInt64 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.TruncateInt64, staging, out length),
        FullX87KernelKey.LoadRaw80 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.LoadRaw80, staging, out length),
        FullX87KernelKey.StoreRaw80 => FullExtended80ConversionEmitter.TryEmitContinuation(FullExtended80Conversion.StoreRaw80, staging, out length),
        FullX87KernelKey.Fcom => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fcom, staging, out length),
        FullX87KernelKey.Fcomp => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fcomp, staging, out length),
        FullX87KernelKey.Fcompp => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fcompp, staging, out length),
        FullX87KernelKey.Fucom => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fucom, staging, out length),
        FullX87KernelKey.Fucomp => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fucomp, staging, out length),
        FullX87KernelKey.Fucompp => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fucompp, staging, out length),
        FullX87KernelKey.Fcomi => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fcomi, staging, out length),
        FullX87KernelKey.Fcomip => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fcomip, staging, out length),
        FullX87KernelKey.Fucomi => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fucomi, staging, out length),
        FullX87KernelKey.Fucomip => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.Fucomip, staging, out length),
        FullX87KernelKey.RoundToIntegral => FullExtended80ControlEmitter.TryEmitContinuation(FullExtended80Control.RoundToIntegral, staging, out length),
        FullX87KernelKey.Sqrt => FullExtended80TransformEmitter.TryEmitContinuation(FullExtended80Transform.Sqrt, staging, out length),
        FullX87KernelKey.Extract => FullExtended80TransformEmitter.TryEmitContinuation(FullExtended80Transform.Extract, staging, out length),
        _ when (uint)key is >= 32 and <= 65 =>
            FullCanonicalX87MemoryArithmeticLowering.TryEmitKernel((byte)key, staging, out length),
        FullX87KernelKey.Scale => FullExtended80ReductionEmitter.TryEmitContinuation(FullExtended80Reduction.Scale, staging, out length),
        FullX87KernelKey.Remainder => FullExtended80ReductionEmitter.TryEmitContinuation(FullExtended80Reduction.Remainder, staging, out length),
        FullX87KernelKey.RemainderNearest => FullExtended80ReductionEmitter.TryEmitContinuation(FullExtended80Reduction.RemainderNearest, staging, out length),
        _ when (uint)key is >= 69 and <= 76 => FullExtended80TranscendentalSoftwareEmitter.TryEmitContinuation(
            (FullExtended80Transcendental)((int)key - 69), staging, out length),
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
}
