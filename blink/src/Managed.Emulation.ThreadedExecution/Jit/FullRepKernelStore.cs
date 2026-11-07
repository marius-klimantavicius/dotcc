using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Owner-immutable native code, independent of the resettable TB store. The
// finite specialization index excludes guest PC/length and never replaces a
// published routine. Bytes append to packed regions; no per-kernel mapping.
internal readonly record struct FullRepKernelBinding(int Slot, ulong Cookie, nint Resume, nint FaultFinalize);

internal struct FullRepKernelLease : IDisposable
{
    internal FullRepKernelStore? Owner;
    internal int Ticket;
    internal ulong Serial;
    internal FullRepKernelBinding Binding;
    public void Dispose() { Owner?.Release(Ticket, Serial); this = default; }
}

internal sealed unsafe partial class FullRepKernelStore : IDisposable
{
    internal const int MaximumSpecializations = 152;
    private const int RegionBytes = 65536;
    private const long MaximumCodeBytes = MaximumSpecializations * FullBlockEmitter.RepKernelCapacity;
    private struct Entry { public FullRepInstruction Key; public FullRepKernelBinding Binding; }
    private struct Ticket { public ulong Serial; public int Slot; }
    private readonly FullCodeAuthority authority;
    private readonly RetainedTranslationStore quota;
    private readonly TranslationCodeRegions code;
    private readonly Entry[] entries = new Entry[MaximumSpecializations];
    private readonly Ticket[] tickets;
    private int count, active;
    private ulong serial;
    private bool disposed, disposing;
    internal bool AdmissionOpen => !disposed && !disposing && !code.Quarantined;
    internal int Count => count;
    internal long EmittedBytes { get; private set; }
    internal long ReservedBytes => disposed ? 0 : code.ReservedBytes;
    internal static long FixedChargeFor(int workers) => 256 +
        MaximumSpecializations * (long)sizeof(Entry) + (workers + 4L) * sizeof(Ticket) +
        TranslationCodeRegions.MetadataBytes(MaximumCodeBytes, RegionBytes);

    internal FullRepKernelStore(FullCodeAuthority authority, RetainedTranslationStore quota,
        int workers, bool allowUnverifiedWindows)
    {
        this.authority = authority; this.quota = quota;
        tickets = new Ticket[checked(workers + 4)];
        code = new(MaximumCodeBytes, RegionBytes, allowUnverifiedWindows, requireNativeFlags: false);
    }

    // Gate and a compiler scratch span belong to the caller. No native frame
    // may overlap a packed tail page becoming writable. Existing issued-code
    // leases can remain: they retain lifetime, not executable admission.
    internal bool TryBind(in FullRepInstruction instruction, Span<byte> staging,
        out FullRepKernelBinding binding, out TranslationStoreFailure failure)
    {
        RequireGate(); ObjectDisposedException.ThrowIf(disposed, this);
        RequireAdmission();
        binding = default; failure = TranslationStoreFailure.None;
        if (!FullRepLowering.IsCanonical(in instruction)) throw new ArgumentException("Invalid REP specialization");
        var key = instruction with { Ip = 0, Length = 2 };
        for (int i = 0; i < count; ++i)
            if (entries[i].Key == key) { binding = entries[i].Binding; return true; }
        if (count == entries.Length) throw new InvalidOperationException("REP specialization bound exceeded");
        // Only host kernel-arena W^X changes here. Gate excludes new issued
        // kernel enrollment; retain the runtime pulse and drain through RET.
        authority.AdvanceCodePublicationRuntime(); authority.WaitNativeFrames();
        // Kernel cookies are owner-authenticated slot identities. Slots never
        // retire/reuse; owner cookie is separately checked by generated code.
        ulong cookie = checked((ulong)count + 1);
        // Only the production REP controller invokes these owner kernels: both
        // Resume and fresh FaultFinalize bind their own R189 stack completion delta.
        int length = FullBlockEmitter.EmitOwnerPrivateCompletionRepKernel(key, quota.OwnerCookie, cookie, staging, out var layout);
        long extra = code.AdditionalReservationBytes(length);
        if (!quota.TryReserveExternalBytes(extra, out failure)) return false;
        long before = code.ReservedBytes;
        if (!code.TryStage(length, out var run, out nint address, out failure))
        {
            long retained = code.ReservedBytes - before;
            quota.ReleaseExternalBytes(extra - retained);
            return false;
        }
        staging[..length].CopyTo(new Span<byte>((void*)address, length));
        if (!code.TrySeal(run))
            throw new InvalidOperationException("REP packed code RX publication failed; charged mapping retained for cleanup");
        code.Commit(run, length);
        binding = new(count, cookie, address + layout.ResumeOffset, address + layout.FaultFinalizeOffset);
        entries[count++] = new() { Key = key, Binding = binding };
        EmittedBytes += length;
        return true;
    }

    internal FullRepKernelLease Acquire(in FullRepKernelBinding binding)
    {
        RequireGate(); ObjectDisposedException.ThrowIf(disposed, this);
        RequireAdmission();
        if ((uint)binding.Slot >= (uint)count || entries[binding.Slot].Binding != binding)
            throw new InvalidOperationException("Foreign REP kernel binding");
        if (serial == ulong.MaxValue) throw new InvalidOperationException("REP lease generation exhausted");
        for (int i = 0; i < tickets.Length; ++i)
            if (tickets[i].Serial == 0)
            {
                ulong next = ++serial; tickets[i] = new() { Serial = next, Slot = binding.Slot }; ++active;
                return new() { Owner = this, Ticket = i, Serial = next, Binding = binding };
            }
        throw new InvalidOperationException("REP kernel lease capacity exhausted");
    }
    internal bool IsValid(in FullRepKernelLease lease)
    {
        RequireGate();
        return AdmissionOpen && ReferenceEquals(lease.Owner, this) && (uint)lease.Ticket < (uint)tickets.Length &&
            lease.Serial != 0 && tickets[lease.Ticket].Serial == lease.Serial &&
            tickets[lease.Ticket].Slot == lease.Binding.Slot &&
            (uint)lease.Binding.Slot < (uint)count && entries[lease.Binding.Slot].Binding == lease.Binding;
    }
    internal FullRepKernelBinding Find(in FullRepInstruction instruction)
    {
        RequireGate(); ObjectDisposedException.ThrowIf(disposed, this);
        RequireAdmission();
        var key = instruction with { Ip = 0, Length = 2 };
        for (int i = 0; i < count; ++i) if (entries[i].Key == key) return entries[i].Binding;
        throw new InvalidOperationException("REP site has no published owner kernel");
    }
    internal void Release(int ticket, ulong expected)
    {
        lock (authority.Gate)
            if ((uint)ticket < (uint)tickets.Length && expected != 0 && tickets[ticket].Serial == expected)
            { tickets[ticket] = default; --active; }
    }
    internal void AssertQuiescent()
    {
        RequireGate(); if (active != 0) throw new InvalidOperationException("Issued REP capabilities remain active");
    }
    public void Dispose()
    {
        lock (authority.Gate)
        {
            if (disposed) return;
            AssertQuiescent();
            disposing = true; // irreversible admission closure, even on partial unmap
            long bytes = code.ReservedBytes;
            try { code.Dispose(); }
            finally
            {
                // Release exactly the reservations actually unmapped, also
                // when a later region/metadata release throws. A retry sees
                // only the still-mapped balance and cannot strand old charge.
                long released = bytes - code.ReservedBytes;
                if (released != 0) quota.ReleaseExternalBytes(released);
            }
            disposed = true;
        }
    }
    private void RequireGate()
    { if (!Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("REP kernel manifest requires authority gate"); }
    private void RequireAdmission()
    { if (!AdmissionOpen) throw new InvalidOperationException("REP kernel code is quarantined or disposing; admission is closed"); }
}
