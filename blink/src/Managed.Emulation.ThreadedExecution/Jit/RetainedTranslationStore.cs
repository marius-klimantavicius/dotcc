using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// One owner-wide translation gate, an exact retained index, and nonowning
// worker jump hints. Code, descriptors, indices and payload append to bounded
// arenas. Nothing is individually reused; only a quiescent flush rewinds them.
// No guest fetch, helper, native execution or IO callback occurs under gate.
internal unsafe sealed partial class RetainedTranslationStore : IDisposable
{
    private const byte Draft = 1, Resident = 2, Retired = 3, Dead = 4;
    private const int OwnerAllowance = 16384, DataChunkBytes = 65536;
    private const ulong GuestPageMask = 4095;
    private struct PageLink { public Body* Owner; public PageLink* Next; public ulong Page; }
    private struct Edge { public Body* Owner; public Body* Target; public Edge* Previous; public Edge* Next; public int Number; }
    private struct Body
    {
        public TranslationBlockHeader Header;
        public TranslationCodeRun Code;
        public Body* KeyNext;
        public Body* RetiredNext;
        public Edge* Incoming;
        public Edge Edge0, Edge1;
        public PageLink Virtual0, Virtual1, Physical0, Physical1;
        public ulong FetchPc;
        public ulong LastInvalidation;
        public int UnknownPrevious, UnknownNext;
        public int CodeCapacity, PayloadCapacity, UsefulCode, Leases;
        public byte State, AuthorityDirty, HasProvenance;
    }
    private struct DataChunk { public DataChunk* Next; public long Bytes, Used; }
    private struct Ticket { public ulong Serial, Generation; public int Slot; public byte Kind; }
    private struct Worker { public ulong Serial; }
    private readonly object gate = new();
    private readonly TranslationStoreOptions options;
    private readonly TranslationCodeRegions code;
    private readonly int managedTicketCount, nativeTicketCount;
    private readonly long fixedMappingBytes, codeMetadataBytes;
    private void* fixedMapping;
    // Stable owner allocation, NEVER placed in resettable body/data arenas.
    private ulong* generationWord;
    private bool generationExhausted;
    internal static long GenerationWordBytes => Environment.SystemPageSize;
    internal ulong* GenerationPointer { get { lock (gate) { ThrowDisposed(); return generationWord; } } }
    internal ulong Generation { get { lock (gate) { ThrowDisposed(); return Volatile.Read(ref *generationWord); } } }
    private Ticket* tickets;
    private Worker* workers;
    private TranslationToken* hints;
    private DataChunk* chunks;
    private DataChunk* currentChunk;
    private Body** records;
    private Body** directory;
    private PageLink** virtualPages;
    private PageLink** physicalPages;
    private int recordCount, recordCapacity, bucketCount, indexedCount, unknownHead = -1;
    private Body* retiredHead;
    private int residents, retiredCount, draftCount, activeManaged, activeNative, workerCount;
    private ulong sequence;
    private bool disposed, disposing, flushPending, publicationPaused;
    private static long nextOwnerCookie, nextBodyGeneration;
    private long dataReserved, dataUsed, descriptorUsed, indexUsed, payloadUsed, publishedPayload;
    // Owner-immutable generated routines share this byte quota, but survive
    // body flushes. Their owner releases this charge only after actual unmap.
    private long externalReserved;
    private long usefulCode, peakUsefulCode, peakPayload, peakDescriptor, peakIndex, peakArenaReserved, peakCharged;
    private long publications, lookups, hintHits, probes, contentRetirements;
    private long workerLookups, directKeyLookups, tokenLookups;
    private long revocations, revalidations, reclaimed, denials, totalFlushes, flushRetirements, flushRequests, flushDenials;
    private long setupAllocated, compilationAllocated, publicationAllocated, retirementAllocated, cumulativeEmission;
    private long physicalIndexProbes, virtualIndexProbes;

    internal ulong OwnerCookie { get; }
    // Byte-derived upper bound only; no array with this many slots is reserved.
    public int BodyCapacity => checked((int)Math.Min(int.MaxValue, options.BudgetBytes / Align64(sizeof(Body))));
    public long ChargedBytes { get { lock (gate) return disposed ? externalReserved : CurrentCharge; } }
    internal bool NativeLifetimeClosed { get { lock (gate) return disposed; } }
    public int ActiveNativeEpochs => Volatile.Read(ref activeNative);
    public long Publications { get { lock (gate) return publications; } }
    public bool FlushPending { get { lock (gate) return flushPending; } }
    private long FixedCharge => OwnerAllowance + fixedMappingBytes + codeMetadataBytes + GenerationWordBytes;
    private long CurrentCharge => FixedCharge + dataReserved + code.ReservedBytes + externalReserved;

    internal long ExternalReservedBytes { get { lock (gate) return externalReserved; } }
    internal bool TryReserveExternalBytes(long bytes, out TranslationStoreFailure failure)
    {
        lock (gate)
        {
            ThrowDisposed(); failure = TranslationStoreFailure.None;
            if (bytes < 0 || bytes > options.BudgetBytes - FixedCharge)
            { failure = TranslationStoreFailure.InvalidSize; return false; }
            if (generationExhausted)
            { failure = TranslationStoreFailure.AuthorityChanged; return false; }
            if (draftCount != 0) { failure = TranslationStoreFailure.NotPublished; return false; }
            if (activeNative != 0) { failure = TranslationStoreFailure.NativeEpochActive; return false; }
            if (CurrentCharge > options.BudgetBytes - bytes)
            {
                flushPending = true;
                if (!FlushCore(out failure)) return false;
                if (CurrentCharge > options.BudgetBytes - bytes && !TrimEmptyReservations())
                { failure = TranslationStoreFailure.NativeAllocation; return false; }
            }
            if (CurrentCharge > options.BudgetBytes - bytes)
            { failure = TranslationStoreFailure.Capacity; return false; }
            externalReserved = checked(externalReserved + bytes);
            UpdatePeaks(); return true;
        }
    }
    internal void ReleaseExternalBytes(long bytes)
    {
        lock (gate)
        {
            // Actual owner-code disposal follows successful store disposal.
            // This accounting-only release remains valid after that point.
            if (bytes < 0 || bytes > externalReserved) throw new ArgumentOutOfRangeException(nameof(bytes));
            externalReserved -= bytes;
        }
    }

    internal RetainedTranslationStore(TranslationStoreOptions options)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        ArgumentNullException.ThrowIfNull(options);
        if (options.BudgetBytes < 1 << 20 || options.BudgetBytes > 16L << 30 ||
            options.MaximumWorkers < 1 || options.HintsPerWorker < 1 ||
            (options.HintsPerWorker & (options.HintsPerWorker - 1)) != 0 ||
            options.RegionBytes < 65536 || options.RegionBytes % 65536 != 0 ||
            options.RegionBytes % Environment.SystemPageSize != 0 ||
            options.BodyCapacity < 0 || options.PayloadBytes < 0 || options.PayloadBytes > options.BudgetBytes)
            throw new ArgumentOutOfRangeException(nameof(options));
        string? unavailable = NativeCodePlatform.UnavailableReason(options.AllowUnverifiedWindows, options.RequireNativeFlags);
        if (unavailable != null) throw new PlatformNotSupportedException(unavailable);
        this.options = options;
        long cookie = Interlocked.Increment(ref nextOwnerCookie);
        if (cookie <= 0) throw new InvalidOperationException("Owner cookie space exhausted");
        OwnerCookie = (ulong)cookie;
        nativeTicketCount = options.MaximumWorkers;
        managedTicketCount = checked(options.MaximumWorkers * 4 + 16);
        fixedMappingBytes = TranslationCodeRegions.RoundPage(
            Align64((long)(managedTicketCount + nativeTicketCount) * sizeof(Ticket)) +
            Align64((long)options.MaximumWorkers * sizeof(Worker)) +
            Align64((long)options.MaximumWorkers * options.HintsPerWorker * sizeof(TranslationToken)));
        long codeCapacity = (options.BudgetBytes - OwnerAllowance - fixedMappingBytes - GenerationWordBytes) /
            options.RegionBytes * options.RegionBytes;
        codeMetadataBytes = TranslationCodeRegions.MetadataBytes(codeCapacity, options.RegionBytes);
        if (codeCapacity < options.RegionBytes || FixedCharge + options.RegionBytes + DataChunkBytes > options.BudgetBytes)
            throw new ArgumentOutOfRangeException(nameof(options), "Budget cannot hold fixed resources and initial code/data arenas");
        fixedMapping = TranslationCodeRegions.AllocateData(fixedMappingBytes);
        if (fixedMapping == null) throw new OutOfMemoryException();
        try
        {
            generationWord = (ulong*)TranslationCodeRegions.AllocateData(GenerationWordBytes);
            if (generationWord == null) throw new OutOfMemoryException();
            *generationWord = 1;
            NativeMemory.Clear(fixedMapping, (nuint)fixedMappingBytes);
            byte* cursor = (byte*)fixedMapping;
            tickets = (Ticket*)cursor; cursor += Align64((long)(managedTicketCount + nativeTicketCount) * sizeof(Ticket));
            workers = (Worker*)cursor; cursor += Align64((long)options.MaximumWorkers * sizeof(Worker));
            hints = (TranslationToken*)cursor;
            code = new TranslationCodeRegions(codeCapacity, options.RegionBytes,
                options.AllowUnverifiedWindows, options.RequireNativeFlags);
            lock (gate) { }
            UpdatePeaks();
            setupAllocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        }
        catch
        {
            if (generationWord != null) { TranslationCodeRegions.ReleaseData(generationWord, GenerationWordBytes); generationWord = null; }
            TranslationCodeRegions.ReleaseData(fixedMapping, fixedMappingBytes); fixedMapping = null;
            throw;
        }
    }

    public bool TryReserve(TranslationKey key, int codeCapacity, int payloadCapacity, uint guestLength,
        out TranslationDraft draft, out TranslationStoreFailure failure)
    {
        lock (gate)
        {
            ThrowDisposed(); draft = default; failure = TranslationStoreFailure.None;
            if (generationExhausted) { ++denials; failure = TranslationStoreFailure.AuthorityChanged; return false; }
            if (codeCapacity < 0 || codeCapacity > options.RegionBytes || payloadCapacity < 0 ||
                payloadCapacity > options.BudgetBytes - FixedCharge || guestLength == 0 ||
                key.Pc > ulong.MaxValue - (guestLength - 1UL) ||
                options.PayloadBytes != 0 && payloadCapacity > options.PayloadBytes)
            { failure = TranslationStoreFailure.InvalidSize; return false; }
            if (Find(key) != null) { failure = TranslationStoreFailure.AlreadyPresent; return false; }
            // Writable pointers escape this lock. This writer admission lasts
            // until Publish/Abort consumes the unique draft, including code=0.
            if (draftCount != 0) { ++denials; failure = TranslationStoreFailure.NotPublished; return false; }
            if (activeNative != 0) { ++denials; failure = TranslationStoreFailure.NativeEpochActive; return false; }
            for (int attempt = 0; attempt < 2; ++attempt)
            {
                if (code.Quarantined || flushPending)
                { if (!FlushCore(out failure)) return false; }
                int nextRecordCapacity = recordCount == recordCapacity ? Math.Max(64, checked(recordCapacity * 2)) : recordCapacity;
                int nextBucketCount = bucketCount == 0 ? 64 : indexedCount >= bucketCount ? checked(bucketCount * 2) : bucketCount;
                long recordIndexBytes = nextRecordCapacity != recordCapacity ? Align64((long)nextRecordCapacity * sizeof(nint)) : 0;
                long hashBytes = nextBucketCount != bucketCount ? Align64((long)nextBucketCount * sizeof(nint)) : 0;
                long payloadOccupied = Align16(payloadCapacity);
                long packetBytes = checked(recordIndexBytes + 3 * hashBytes + Align64(sizeof(Body)) + payloadOccupied);
                long additionalData = AdditionalDataReservation(packetBytes);
                long additionalCode = code.AdditionalReservationBytes(codeCapacity);
                bool payloadFits = options.PayloadBytes == 0 || payloadUsed <= options.PayloadBytes - payloadOccupied;
                if (attempt == 1 && recordCount == 0 &&
                    CurrentCharge > options.BudgetBytes - additionalData - additionalCode)
                {
                    // A prior workload may have grown mostly code or mostly
                    // data. Rebalance only empty reservations after the bulk
                    // reset, so the new mix can still use the shared quota.
                    if (!TrimEmptyReservations()) { failure = TranslationStoreFailure.NativeAllocation; return false; }
                    additionalData = AdditionalDataReservation(packetBytes);
                    additionalCode = code.AdditionalReservationBytes(codeCapacity);
                }
                if (!payloadFits || CurrentCharge > options.BudgetBytes - additionalData - additionalCode)
                {
                    flushPending = true;
                    if (attempt == 1) { ++denials; failure = TranslationStoreFailure.Capacity; return false; }
                    if (!FlushCore(out failure)) return false;
                    continue;
                }
                byte* packet = AllocatePacket(packetBytes, out failure);
                if (packet == null) return false;
                if (recordIndexBytes != 0)
                {
                    var expanded = (Body**)packet; packet += recordIndexBytes;
                    if (recordCount != 0) NativeMemory.Copy(records, expanded, checked((nuint)((long)recordCount * sizeof(nint))));
                    records = expanded; recordCapacity = nextRecordCapacity; indexUsed += recordIndexBytes;
                }
                if (hashBytes != 0)
                {
                    directory = (Body**)packet; packet += hashBytes;
                    virtualPages = (PageLink**)packet; packet += hashBytes;
                    physicalPages = (PageLink**)packet; packet += hashBytes;
                    bucketCount = nextBucketCount; indexUsed += hashBytes * 3;
                    RebuildIndices();
                }
                if (!code.TryStage(codeCapacity, out var run, out nint codePointer, out failure))
                {
                    if (code.Quarantined) QuarantineBodies();
                    UpdatePeaks(); return false;
                }
                long nextGeneration = Interlocked.Increment(ref nextBodyGeneration);
                if (nextGeneration <= 0)
                {
                    if (!code.TrySeal(run)) QuarantineBodies();
                    throw new InvalidOperationException("Translation generation space exhausted");
                }
                Body* body = (Body*)packet; packet += Align64(sizeof(Body));
                int slot = recordCount++;
                records[slot] = body;
                *body = default;
                body->State = Draft; body->Code = run; body->CodeCapacity = codeCapacity; body->PayloadCapacity = payloadCapacity;
                body->FetchPc = key.Pc; body->UnknownPrevious = body->UnknownNext = -1;
                body->Edge0.Owner = body->Edge1.Owner = body;
                body->Edge1.Number = 1;
                body->Header = new TranslationBlockHeader { OwnerCookie = OwnerCookie,
                    BodyGeneration = (ulong)nextGeneration, GuestPc = key.Pc, Context = key.Context,
                    Slot = (uint)slot, Payload = payloadCapacity == 0 ? 0 : (nint)packet,
                    GuestLength = guestLength, CodeEpoch = 1 };
                if (payloadCapacity != 0) NativeMemory.Clear(packet, (nuint)payloadCapacity);
                descriptorUsed += Align64(sizeof(Body)); payloadUsed += payloadOccupied;
                InsertKey(body); InsertVirtual(body); AddUnknown(body); ++draftCount;
                draft = new TranslationDraft { Owner = this, Token = new(slot, (ulong)nextGeneration), Header = (nint)(&body->Header),
                    CodePointer = codePointer, PayloadPointer = body->Header.Payload, CodeCapacity = codeCapacity, PayloadCapacity = payloadCapacity };
                UpdatePeaks(); return true;
            }
            ++denials; failure = TranslationStoreFailure.Capacity; return false;
        }
    }

    public bool Publish(ref TranslationDraft draft, int codeBytes, int payloadBytes,
        ulong codeEpoch, ulong globalEpoch, out TranslationStoreFailure failure)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        lock (gate)
        {
            ThrowDisposed(); failure = TranslationStoreFailure.None;
            if (!IsDraft(in draft)) { failure = TranslationStoreFailure.StaleDraft; return false; }
            Body* body = records[draft.Token.Slot];
            if (codeBytes < 0 || codeBytes > body->CodeCapacity || payloadBytes < 0 || payloadBytes > body->PayloadCapacity ||
                (body->CodeCapacity > 0 && codeBytes == 0) || codeEpoch == 0)
            { failure = TranslationStoreFailure.InvalidSize; return false; }
            if (body->AuthorityDirty != 0) { failure = TranslationStoreFailure.AuthorityChanged; return false; }
            if (!code.TrySeal(body->Code))
            {
                RetireCore(body, false); draft.Clear(); QuarantineBodies(); CollectCore();
                failure = TranslationStoreFailure.NativeAllocation; return false;
            }
            code.Commit(body->Code, codeBytes);
            body->Header.PayloadBytes = (uint)payloadBytes;
            body->Header.CodeEpoch = body->Header.ValidatedCodeEpoch = codeEpoch;
            body->Header.ValidatedGlobalEpoch = globalEpoch;
            body->UsefulCode = codeBytes;
            Volatile.Write(ref body->Header.Entry, draft.CodePointer);
            body->State = Resident; --draftCount; ++residents; ++publications;
            usefulCode += codeBytes; cumulativeEmission += codeBytes; publishedPayload += payloadBytes;
            Volatile.Write(ref body->Header.Live, 1);
            draft.Clear(); UpdatePeaks();
            publicationAllocated += GC.GetAllocatedBytesForCurrentThread() - allocationStart;
            return true;
        }
    }

    public void Abort(ref TranslationDraft draft)
    {
        lock (gate)
        {
            ThrowDisposed();
            if (IsDraft(in draft))
            {
                Body* body = records[draft.Token.Slot];
                bool sealedRx = code.TrySeal(body->Code);
                RetireCore(body, false);
                if (!sealedRx) QuarantineBodies();
            }
            draft.Clear(); CollectCore();
        }
    }
    internal Span<byte> GetDraftCode(scoped in TranslationDraft draft)
    { lock (gate) { ThrowDisposed(); if (!IsDraft(in draft) || code.Quarantined) throw new InvalidOperationException("Inactive writable translation draft"); return new((void*)draft.CodePointer, draft.CodeCapacity); } }
    internal Span<byte> GetDraftPayload(scoped in TranslationDraft draft)
    { lock (gate) { ThrowDisposed(); if (!IsDraft(in draft)) throw new InvalidOperationException("Stale translation draft"); return new((void*)draft.PayloadPointer, draft.PayloadCapacity); } }

    public bool TryAcquireKey(TranslationKey key, out TranslationLease lease, bool requireLive = false)
        => TryAcquireKey(key, out lease, out _, requireLive);
    public bool TryAcquireKey(TranslationKey key, out TranslationLease lease,
        out TranslationStoreFailure failure, bool requireLive = false)
    { lock (gate) { ThrowDisposed(); ++lookups; ++directKeyLookups; return AcquireCore(Find(key), out lease, out failure, requireLive); } }
    public bool TryAcquireKey(ref TranslationWorkerCache worker, TranslationKey key,
        out TranslationLease lease, bool requireLive = false)
        => TryAcquireKey(ref worker, key, out lease, out _, requireLive);
    public bool TryAcquireKey(ref TranslationWorkerCache worker, TranslationKey key,
        out TranslationLease lease, out TranslationStoreFailure failure, bool requireLive = false)
    {
        lock (gate)
        {
            ThrowDisposed(); ValidateWorker(in worker); ++lookups; ++workerLookups;
            int hintIndex = worker.Worker * options.HintsPerWorker + (int)(Hash(key) & (uint)(options.HintsPerWorker - 1));
            TranslationToken hint = hints[hintIndex];
            Body* body = TokenBody(hint, Resident);
            if (body != null && body->Header.GuestPc == key.Pc && body->Header.Context == key.Context) ++hintHits;
            else body = Find(key);
            if (!AcquireCore(body, out lease, out failure, requireLive)) return false;
            hints[hintIndex] = lease.Token; return true;
        }
    }
    public bool TryAcquireToken(TranslationToken token, out TranslationLease lease, bool requireLive = false)
        => TryAcquireToken(token, out lease, out _, requireLive);
    public bool TryAcquireToken(TranslationToken token, out TranslationLease lease,
        out TranslationStoreFailure failure, bool requireLive = false)
    { lock (gate) { ThrowDisposed(); ++lookups; ++tokenLookups; return AcquireCore(TokenBody(token, Resident), out lease, out failure, requireLive); } }
    private bool AcquireCore(Body* body, out TranslationLease lease, out TranslationStoreFailure failure, bool requireLive)
    {
        lease = default; failure = TranslationStoreFailure.None;
        if (body == null) { failure = TranslationStoreFailure.NotFound; return false; }
        if (body->State != Resident) { failure = TranslationStoreFailure.NotPublished; return false; }
        if (requireLive && Volatile.Read(ref body->Header.Live) == 0) { failure = TranslationStoreFailure.AuthorityChanged; return false; }
        int slot = (int)body->Header.Slot;
        int ticket = AllocateTicket(0, managedTicketCount, slot, 1);
        if (ticket < 0) { failure = TranslationStoreFailure.LeaseCapacity; return false; }
        ++body->Leases; ++activeManaged;
        lease = new TranslationLease { Owner = this, Ticket = ticket, Serial = tickets[ticket].Serial,
            Token = new(slot, body->Header.BodyGeneration) };
        return true;
    }
    public nint GetHeader(in TranslationLease lease)
    { lock (gate) { ValidateLease(in lease); return (nint)(&records[lease.Token.Slot]->Header); } }
    public TranslationBlockHeader GetHeaderCopy(in TranslationLease lease)
    { lock (gate) { ValidateLease(in lease); return records[lease.Token.Slot]->Header; } }
    // Copy helper plans before ending this explicit lifetime lease.
    public ReadOnlySpan<byte> GetPayloadCopySpan(scoped in TranslationLease lease)
    { lock (gate) { ValidateLease(in lease); ref var h = ref records[lease.Token.Slot]->Header; return new((void*)h.Payload, checked((int)h.PayloadBytes)); } }

    // The owner serializes one missing translation's publication phase. This
    // store-level gate also blocks cap1 and direct epoch admission while the
    // earlier generation drains; no replacement reader can starve the writer.
    internal void PauseNativePublication()
    {
        lock (gate)
        {
            ThrowDisposed();
            if (publicationPaused) throw new InvalidOperationException("Native publication pause already owned.");
            publicationPaused = true;
        }
    }
    internal void ResumeNativePublication()
    {
        lock (gate)
        {
            ThrowDisposed();
            if (!publicationPaused) throw new InvalidOperationException("Native publication pause is not owned.");
            publicationPaused = false;
        }
    }
    public bool TryBeginNativeEpoch(out NativeEpochLease epoch)
    {
        lock (gate) return BeginNativeEpochCore(out epoch);
    }
    // Used only by a controller holding an external finite service lease.
    // It must drop that lease before retrying or releasing a store capability.
    internal bool TryBeginNativeEpochNonblocking(out NativeEpochLease epoch)
    {
        epoch = default;
        if (!Monitor.TryEnter(gate)) return false;
        try { return BeginNativeEpochCore(out epoch); }
        finally { Monitor.Exit(gate); }
    }
    private bool BeginNativeEpochCore(out NativeEpochLease epoch)
    {
        ThrowDisposed(); epoch = default;
        if (generationExhausted || publicationPaused || draftCount != 0 || flushPending || code.Quarantined) return false;
        int ticket = AllocateTicket(managedTicketCount, nativeTicketCount, -1, 2);
        if (ticket < 0) return false;
        Interlocked.Increment(ref activeNative);
        epoch = new NativeEpochLease { Owner = this, Ticket = ticket, Serial = tickets[ticket].Serial }; return true;
    }
    public nint GetHeader(in NativeEpochLease epoch, TranslationToken token, bool requireLive = true)
    {
        lock (gate)
        {
            ValidateEpoch(in epoch); Body* body = TokenBody(token, Resident);
            return body != null && (!requireLive || Volatile.Read(ref body->Header.Live) != 0) ? (nint)(&body->Header) : 0;
        }
    }
    internal void ReleaseManagedLease(int ticket, ulong serial)
    { lock (gate) { if (disposed || !ValidTicket(ticket, serial, 1)) return; --records[tickets[ticket].Slot]->Leases; --activeManaged; tickets[ticket].Kind = 0; CollectCore(); } }
    internal void EndNativeEpoch(int ticket, ulong serial)
    { lock (gate) { if (disposed || !ValidTicket(ticket, serial, 2)) return; tickets[ticket].Kind = 0; if (Interlocked.Decrement(ref activeNative) == 0) CollectCore(); } }
    public bool TryCreateWorkerCache(out TranslationWorkerCache worker)
    {
        lock (gate)
        {
            ThrowDisposed(); worker = default;
            for (int i = 0; i < options.MaximumWorkers; ++i)
            {
                if (workers[i].Serial != 0) continue;
                ulong serial = NextSequence(); workers[i].Serial = serial; ++workerCount;
                NativeMemory.Clear(hints + i * options.HintsPerWorker, checked((nuint)((long)options.HintsPerWorker * sizeof(TranslationToken))));
                worker = new TranslationWorkerCache { Owner = this, Worker = i, Serial = serial }; return true;
            }
            return false;
        }
    }
    internal void ReleaseWorker(int worker, ulong serial)
    { lock (gate) { if (!disposed && (uint)worker < (uint)options.MaximumWorkers && workers[worker].Serial == serial && serial != 0) { workers[worker].Serial = 0; --workerCount; } } }

    // Checked fetch provenance is registered by the compiler/byte validator,
    // not inferred from the semantic key (which can omit a segment base).
    // Physical addresses are page bases; MaxValue means no second page.
    // Updating a resident's provenance does NOT authorize it: revalidation
    // still authenticates CodeEpoch and every native Live/context/code guard.
    public bool SetProvenance(in TranslationDraft draft, ulong actualFetchPc,
        ulong physicalPage0, ulong physicalPage1 = ulong.MaxValue)
    {
        lock (gate)
        {
            ThrowDisposed(); if (!IsDraft(in draft)) return false;
            return SetProvenanceCore(records[draft.Token.Slot], actualFetchPc, physicalPage0, physicalPage1);
        }
    }
    public bool SetProvenance(TranslationToken token, ulong actualFetchPc,
        ulong physicalPage0, ulong physicalPage1 = ulong.MaxValue)
    {
        lock (gate)
        {
            ThrowDisposed(); Body* body = TokenBody(token, Resident);
            return body != null && SetProvenanceCore(body, actualFetchPc, physicalPage0, physicalPage1);
        }
    }
    private bool SetProvenanceCore(Body* body, ulong pc, ulong page0, ulong page1)
    {
        uint bytes = body->Header.GuestLength;
        if (pc > ulong.MaxValue - (bytes - 1UL) || (page0 & GuestPageMask) != 0 ||
            ((pc & GuestPageMask) + bytes > 8192) ||
            ((pc & GuestPageMask) + bytes > 4096 ? page1 == ulong.MaxValue || (page1 & GuestPageMask) != 0 : page1 != ulong.MaxValue)) return false;
        RemovePageLinks(body);
        if (body->HasProvenance == 0) RemoveUnknown(body);
        body->FetchPc = pc; body->HasProvenance = 1;
        InsertVirtual(body);
        AddPageLink(&body->Physical0, body, page0, physicalPages);
        if (page1 != ulong.MaxValue) AddPageLink(&body->Physical1, body, page1, physicalPages);
        return true;
    }
    public int MarkAuthorityChanged(ulong start, ulong length, bool includeDrafts = true)
    { lock (gate) { ThrowDisposed(); return InvalidateRange(start, length, false, includeDrafts); } }
    public int MarkPhysicalAuthorityChanged(ulong start, ulong length, bool includeDrafts = true)
    { lock (gate) { ThrowDisposed(); return InvalidateRange(start, length, true, includeDrafts); } }
    private int InvalidateRange(ulong start, ulong length, bool physical, bool includeDrafts)
    {
        if (length == 0) return 0;
        ulong stamp = NextSequence(), last = SaturatingEnd(start, length);
        int changed = 0;
        // Only unknown-provenance records need conservative physical matching.
        // Generic virtual records spanning >2 pages also take this fallback.
        for (int i = unknownHead; i >= 0;)
        {
            Body* b = records[i]; i = b->UnknownNext;
            if ((physical || Overlaps(b->FetchPc, b->Header.GuestLength, start, length)) &&
                (includeDrafts || b->State != Draft))
            { b->LastInvalidation = stamp; RevokeCore(b); ++changed; }
        }
        if (bucketCount == 0) return changed;
        PageLink** table = physical ? physicalPages : virtualPages;
        ulong firstPage = start & ~GuestPageMask, lastPage = last & ~GuestPageMask;
        ulong pageCount = (lastPage - firstPage) / 4096 + 1;
        // Large range work is bounded by index size, never its address span.
        bool scanBuckets = pageCount > (ulong)bucketCount;
        ulong iterations = scanBuckets ? (ulong)bucketCount : pageCount;
        for (ulong i = 0; i < iterations; ++i)
        {
            ulong page = firstPage + (scanBuckets ? 0 : i * 4096);
            int bucket = scanBuckets ? (int)i : PageBucket(page);
            for (PageLink* link = table[bucket]; link != null;)
            {
                PageLink* next = link->Next; Body* b = link->Owner;
                if (physical) ++physicalIndexProbes; else ++virtualIndexProbes;
                if (b != null && link->Page >= firstPage && link->Page <= lastPage && b->LastInvalidation != stamp &&
                    (includeDrafts || b->State != Draft) &&
                    (physical ? PhysicalOverlaps(b, start, length) : Overlaps(b->FetchPc, b->Header.GuestLength, start, length)))
                { b->LastInvalidation = stamp; RevokeCore(b); ++changed; }
                link = next;
            }
        }
        return changed;
    }
    private static bool PhysicalOverlaps(Body* b, ulong start, ulong length)
    {
        uint first = Math.Min(b->Header.GuestLength, 4096U - (uint)(b->FetchPc & GuestPageMask));
        if (Overlaps(b->Physical0.Page + (b->FetchPc & GuestPageMask), first, start, length)) return true;
        uint second = b->Header.GuestLength - first;
        return second != 0 && Overlaps(b->Physical1.Page, second, start, length);
    }
    public bool TryRevalidate(TranslationToken token, ulong expectedCodeEpoch,
        ulong validatedCodeEpoch, ulong validatedGlobalEpoch)
    {
        lock (gate)
        {
            ThrowDisposed(); Body* body = TokenBody(token, Resident);
            if (body == null || code.Quarantined || expectedCodeEpoch == 0 || expectedCodeEpoch != validatedCodeEpoch ||
                body->Header.CodeEpoch != expectedCodeEpoch) return false;
            body->Header.ValidatedCodeEpoch = validatedCodeEpoch; body->Header.ValidatedGlobalEpoch = validatedGlobalEpoch;
            body->AuthorityDirty = 0; Volatile.Write(ref body->Header.Live, 1); ++revalidations; return true;
        }
    }
    public bool RetireContentChanged(TranslationToken token)
    {
        lock (gate)
        {
            ThrowDisposed(); Body* body = TokenBody(token, Resident); if (body == null) return false;
            RetireCore(body, true); CollectCore(); return true;
        }
    }
    public bool TryLink(TranslationToken source, int edge, TranslationToken target)
    {
        lock (gate)
        {
            ThrowDisposed(); Body* s = TokenBody(source, Resident); Body* t = TokenBody(target, Resident);
            if ((uint)edge > 1 || s == null || t == null) return false;
            ref var sh = ref s->Header; ref var th = ref t->Header;
            if (sh.Live == 0 || th.Live == 0 || sh.Context != th.Context || sh.OwnerCookie != OwnerCookie ||
                th.OwnerCookie != OwnerCookie || th.Entry == 0 || sh.ValidatedCodeEpoch != sh.CodeEpoch ||
                th.ValidatedCodeEpoch != th.CodeEpoch || code.Quarantined) return false;
            Edge* link = edge == 0 ? &s->Edge0 : &s->Edge1; UnlinkCore(link);
            link->Target = t; link->Previous = null; link->Next = t->Incoming;
            if (link->Next != null) link->Next->Previous = link;
            t->Incoming = link;
            if (edge == 0)
            { sh.Link0TargetGeneration = target.Generation; sh.Link0Pc = th.GuestPc; Volatile.Write(ref sh.Link0TargetHeader, (nint)(&t->Header)); }
            else
            { sh.Link1TargetGeneration = target.Generation; sh.Link1Pc = th.GuestPc; Volatile.Write(ref sh.Link1TargetHeader, (nint)(&t->Header)); }
            return true;
        }
    }
    public void Unlink(TranslationToken source, int edge)
    {
        lock (gate)
        {
            ThrowDisposed(); if ((uint)edge > 1) throw new ArgumentOutOfRangeException(nameof(edge));
            Body* b = TokenBody(source, Resident); if (b != null) UnlinkCore(edge == 0 ? &b->Edge0 : &b->Edge1);
        }
    }
    public int CollectRetired()
    { lock (gate) { ThrowDisposed(); return CollectCore(); } }

    // Explicit owner-exclusive flush admission. Busy leaves every retained
    // body/index/edge intact and exposes a pending request for the retry loop.
    public bool TryFlush(out TranslationStoreFailure failure)
    { lock (gate) { ThrowDisposed(); flushPending = true; return FlushCore(out failure); } }
    private bool FlushCore(out TranslationStoreFailure failure)
    {
        ++flushRequests; failure = TranslationStoreFailure.None;
        if (draftCount != 0) failure = TranslationStoreFailure.NotPublished;
        else if (activeNative != 0) failure = TranslationStoreFailure.NativeEpochActive;
        else if (activeManaged != 0) failure = TranslationStoreFailure.LeasedCapacity;
        if (failure != TranslationStoreFailure.None) { ++flushDenials; ++denials; return false; }
        if (!AdvanceGeneration())
        { ++flushDenials; ++denials; failure = TranslationStoreFailure.AuthorityChanged; return false; }
        // There are no pointers held by native frames, helper leases or drafts.
        // Clear authority before changing protections, including reset failure.
        for (int i = 0; i < recordCount; ++i)
        {
            Body* b = records[i];
            if (b->State == Resident) { Volatile.Write(ref b->Header.Live, 0); UnlinkAll(b); }
        }
        if (!code.TryReset())
        { ++flushDenials; failure = TranslationStoreFailure.NativeAllocation; QuarantineBodies(); return false; }
        flushRetirements += residents;
        NativeMemory.Clear(hints, checked((nuint)((long)options.MaximumWorkers * options.HintsPerWorker * sizeof(TranslationToken))));
        for (DataChunk* c = chunks; c != null; c = c->Next) c->Used = Align64(sizeof(DataChunk));
        currentChunk = chunks;
        records = directory = null; virtualPages = physicalPages = null;
        recordCount = recordCapacity = bucketCount = indexedCount = 0; unknownHead = -1; retiredHead = null;
        residents = retiredCount = 0;
        descriptorUsed = indexUsed = payloadUsed = publishedPayload = usefulCode = 0;
        dataUsed = 0;
        for (DataChunk* c = chunks; c != null; c = c->Next) dataUsed += c->Used;
        ++totalFlushes; flushPending = false; return true;
    }
    public void RecordCompilationAllocation(long allocatedBytes)
    { if (allocatedBytes < 0) throw new ArgumentOutOfRangeException(nameof(allocatedBytes)); lock (gate) { ThrowDisposed(); compilationAllocated = checked(compilationAllocated + allocatedBytes); } }
    public TranslationStoreStatistics GetStatistics()
    {
        lock (gate)
        {
            long charge = disposed ? externalReserved : CurrentCharge;
            return new(charge, disposed ? externalReserved : charge - OwnerAllowance, BodyCapacity,
                residents, retiredCount, activeManaged, activeNative, publications, lookups, hintHits, probes,
                flushRetirements, contentRetirements, revocations, revalidations, reclaimed, denials,
                setupAllocated, 0, retirementAllocated, compilationAllocated, publicationAllocated,
                code.RegionAllocations, code.ReusedPages, code.ProtectionFailures, 0)
            {
                UsefulPublishedCodeBytes = usefulCode, PeakUsefulPublishedCodeBytes = peakUsefulCode,
                AlignedOccupiedCodeBytes = disposed ? 0 : code.OccupiedBytes, PeakAlignedOccupiedCodeBytes = code.PeakOccupiedBytes,
                PayloadUsedBytes = payloadUsed, PeakPayloadUsedBytes = peakPayload, PublishedPayloadBytes = publishedPayload,
                DescriptorUsedBytes = descriptorUsed, PeakDescriptorUsedBytes = peakDescriptor,
                IndexUsedBytes = indexUsed, PeakIndexUsedBytes = peakIndex,
                FixedChargedBytes = disposed ? 0 : FixedCharge, PeakFixedChargedBytes = FixedCharge,
                ArenaReservedBytes = disposed ? 0 : dataReserved + code.ReservedBytes, PeakArenaReservedBytes = peakArenaReserved,
                ExternalReservedBytes = externalReserved,
                ArenaUsedBytes = disposed ? 0 : dataUsed + code.OccupiedBytes, DataArenaUsedBytes = dataUsed,
                DataArenaReservedBytes = dataReserved, DataArenaSlackBytes = dataReserved - dataUsed,
                DataArenaOverheadBytes = dataUsed - descriptorUsed - indexUsed - payloadUsed,
                CodeReservedBytes = disposed ? 0 : code.ReservedBytes,
                RegionSlackBytes = disposed ? 0 : code.ReservedBytes - code.TouchedPageBytes,
                PageSlackBytes = disposed ? 0 : code.TouchedPageBytes - code.OccupiedBytes,
                CumulativeEmittedCodeBytes = cumulativeEmission, PeakChargedBytes = peakCharged,
                ProtectionCalls = code.ProtectionCalls, TotalFlushes = totalFlushes, FlushRetirements = flushRetirements,
                FlushRequests = flushRequests, FlushDenials = flushDenials, FlushPending = flushPending,
                ActiveDrafts = draftCount, Quarantined = code.Quarantined,
                PhysicalIndexProbes = physicalIndexProbes, VirtualIndexProbes = virtualIndexProbes,
                WorkerLookups = workerLookups, DirectKeyLookups = directKeyLookups, TokenLookups = tokenLookups,
                WorkerHintMisses = workerLookups - hintHits
            };
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (publicationPaused || workerCount != 0 || activeManaged != 0 || activeNative != 0 || draftCount != 0)
                throw new InvalidOperationException("Join/unregister all workers and release all epochs, leases and drafts before disposal");
            if (!disposing)
            {
                if (!AdvanceGeneration()) throw new InvalidOperationException("Store generation exhausted; refusing owner unmap.");
                disposing = true;
                for (int i = 0; i < recordCount; ++i)
                    if (records[i]->State == Resident) { Volatile.Write(ref records[i]->Header.Live, 0); UnlinkAll(records[i]); }
                // A release failure remains retryable without reading a table
                // from an arena chunk already released by the previous attempt.
                records = directory = null; virtualPages = physicalPages = null;
                recordCount = residents = retiredCount = 0; retiredHead = null;
            }
            code.Dispose();
            while (chunks != null)
            {
                DataChunk* next = chunks->Next; long bytes = chunks->Bytes;
                TranslationCodeRegions.ReleaseData(chunks, bytes); chunks = next; dataReserved -= bytes;
            }
            TranslationCodeRegions.ReleaseData(fixedMapping, fixedMappingBytes); fixedMapping = null;
            // Every worker, managed lease, draft and native epoch is already
            // absent, and all arenas are gone. Free this stable word LAST.
            TranslationCodeRegions.ReleaseData(generationWord, GenerationWordBytes); generationWord = null;
            records = directory = null; virtualPages = physicalPages = null; tickets = null; workers = null; hints = null;
            residents = retiredCount = recordCount = 0; currentChunk = null;
            dataUsed = descriptorUsed = indexUsed = payloadUsed = publishedPayload = usefulCode = 0;
            disposed = true;
        }
    }

    private void RevokeCore(Body* b)
    {
        Volatile.Write(ref b->Header.Live, 0); b->AuthorityDirty = 1; ++revocations; UnlinkAll(b);
        if (b->Header.CodeEpoch == ulong.MaxValue)
        { if (b->State == Resident) RetireCore(b, true); return; }
        Volatile.Write(ref b->Header.CodeEpoch, b->Header.CodeEpoch + 1);
    }
    private void QuarantineBodies()
    {
        flushPending = true;
        for (int i = 0; i < recordCount; ++i)
        {
            Body* b = records[i];
            if (b->State is Resident or Draft) { Volatile.Write(ref b->Header.Live, 0); b->AuthorityDirty = 1; UnlinkAll(b); }
        }
    }
    private void RetireCore(Body* b, bool contentChanged)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        Volatile.Write(ref b->Header.Live, 0); UnlinkAll(b); RemoveKey(b); RemovePageLinks(b);
        if (b->HasProvenance == 0) RemoveUnknown(b);
        if (b->State == Resident) { --residents; usefulCode -= b->UsefulCode; publishedPayload -= b->Header.PayloadBytes; }
        else if (b->State == Draft) --draftCount;
        else throw new InvalidOperationException("Body already retired");
        b->State = Retired; b->RetiredNext = retiredHead; retiredHead = b; ++retiredCount;
        if (contentChanged) ++contentRetirements;
        retirementAllocated += GC.GetAllocatedBytesForCurrentThread() - allocationStart;
    }
    private int CollectCore()
    {
        if (activeNative != 0) return 0;
        int result = 0; Body* previous = null; Body* current = retiredHead;
        while (current != null)
        {
            Body* next = current->RetiredNext;
            if (current->Leases != 0) { previous = current; current = next; continue; }
            if (previous == null) retiredHead = next; else previous->RetiredNext = next;
            current->State = Dead; --retiredCount; ++reclaimed; ++result; current = next;
        }
        // Lifetime retirement does not reuse bytes. The final capability
        // release also services pending exclusive work, including resident-hit
        // retry paths which otherwise need not reserve another translation.
        if (flushPending && activeManaged == 0 && draftCount == 0) FlushCore(out _);
        return result;
    }
    private static void UnlinkAll(Body* b)
    { while (b->Incoming != null) UnlinkCore(b->Incoming); UnlinkCore(&b->Edge0); UnlinkCore(&b->Edge1); }
    private static void UnlinkCore(Edge* e)
    {
        if (e->Number == 0) Volatile.Write(ref e->Owner->Header.Link0TargetHeader, 0);
        else Volatile.Write(ref e->Owner->Header.Link1TargetHeader, 0);
        if (e->Target != null)
        {
            if (e->Previous != null) e->Previous->Next = e->Next; else e->Target->Incoming = e->Next;
            if (e->Next != null) e->Next->Previous = e->Previous;
        }
        e->Target = null; e->Previous = e->Next = null;
    }
    private Body* Find(TranslationKey key)
    {
        if (bucketCount == 0) return null;
        int at = (int)(Hash(key) & (uint)(bucketCount - 1)); ++probes;
        for (Body* b = directory[at]; b != null; b = b->KeyNext)
        { ++probes; if (b->Header.GuestPc == key.Pc && b->Header.Context == key.Context) return b; }
        return null;
    }
    private void InsertKey(Body* b)
    {
        int at = (int)(Hash(new(b->Header.GuestPc, b->Header.Context)) & (uint)(bucketCount - 1));
        b->KeyNext = directory[at]; directory[at] = b; ++indexedCount;
    }
    private void RemoveKey(Body* b)
    {
        int at = (int)(Hash(new(b->Header.GuestPc, b->Header.Context)) & (uint)(bucketCount - 1));
        Body** link = directory + at;
        while (*link != null && *link != b) link = &(*link)->KeyNext;
        if (*link != b) throw new InvalidOperationException("Missing retained directory entry");
        *link = b->KeyNext; b->KeyNext = null; --indexedCount;
    }
    private void RebuildIndices()
    {
        NativeMemory.Clear(directory, checked((nuint)((long)bucketCount * sizeof(nint))));
        NativeMemory.Clear(virtualPages, checked((nuint)((long)bucketCount * sizeof(nint))));
        NativeMemory.Clear(physicalPages, checked((nuint)((long)bucketCount * sizeof(nint))));
        indexedCount = 0;
        for (int i = 0; i < recordCount; ++i)
        {
            Body* b = records[i]; if (b->State is not (Resident or Draft)) continue;
            ulong p0 = b->Physical0.Page, p1 = b->Physical1.Page;
            b->Virtual0 = b->Virtual1 = b->Physical0 = b->Physical1 = default;
            InsertKey(b); InsertVirtual(b);
            if (b->HasProvenance != 0)
            {
                AddPageLink(&b->Physical0, b, p0, physicalPages);
                if ((b->FetchPc & GuestPageMask) + b->Header.GuestLength > 4096) AddPageLink(&b->Physical1, b, p1, physicalPages);
            }
        }
    }
    private void InsertVirtual(Body* b)
    {
        ulong first = b->FetchPc & ~GuestPageMask, last = (b->FetchPc + b->Header.GuestLength - 1) & ~GuestPageMask;
        if (last - first > 4096) return; // Unknown fallback covers long generic contracts.
        AddPageLink(&b->Virtual0, b, first, virtualPages);
        if (last != first) AddPageLink(&b->Virtual1, b, last, virtualPages);
    }
    private int PageBucket(ulong page) => (int)(Hash(new(page >> 12, 0)) & (uint)(bucketCount - 1));
    private void AddPageLink(PageLink* link, Body* b, ulong page, PageLink** table)
    { int at = PageBucket(page); *link = new PageLink { Owner = b, Page = page, Next = table[at] }; table[at] = link; }
    private void RemovePageLink(PageLink* link, PageLink** table)
    {
        if (link->Owner == null) return;
        PageLink** at = table + PageBucket(link->Page);
        while (*at != null && *at != link) at = &(*at)->Next;
        if (*at == link) *at = link->Next;
        *link = default;
    }
    private void RemovePageLinks(Body* b)
    { RemovePageLink(&b->Virtual0, virtualPages); RemovePageLink(&b->Virtual1, virtualPages); RemovePageLink(&b->Physical0, physicalPages); RemovePageLink(&b->Physical1, physicalPages); }
    private void AddUnknown(Body* b)
    {
        int slot = (int)b->Header.Slot; b->UnknownPrevious = -1; b->UnknownNext = unknownHead;
        if (unknownHead >= 0) records[unknownHead]->UnknownPrevious = slot;
        unknownHead = slot;
    }
    private void RemoveUnknown(Body* b)
    {
        if (b->UnknownPrevious >= 0) records[b->UnknownPrevious]->UnknownNext = b->UnknownNext; else unknownHead = b->UnknownNext;
        if (b->UnknownNext >= 0) records[b->UnknownNext]->UnknownPrevious = b->UnknownPrevious;
        b->UnknownPrevious = b->UnknownNext = -1;
    }
    internal static ulong Hash(TranslationKey key)
    {
        unchecked
        {
            ulong h = key.Pc ^ System.Numerics.BitOperations.RotateLeft(key.Context, 23);
            h ^= h >> 30; h *= 0xbf58476d1ce4e5b9UL; h ^= h >> 27; h *= 0x94d049bb133111ebUL; return h ^ (h >> 31);
        }
    }
    // Caller owns gate and has proven global quiescence before reset/unmap.
    // Exhaustion never wraps or allows another admission/publication; old
    // pointers remain mapped and quarantined until owner termination.
    private bool AdvanceGeneration()
    {
        if (activeNative != 0 || activeManaged != 0 || draftCount != 0)
            throw new InvalidOperationException("Store generation change requires global quiescence.");
        ulong previous = Volatile.Read(ref *generationWord);
        if (generationExhausted || previous == ulong.MaxValue)
        { generationExhausted = true; QuarantineBodies(); return false; }
        Volatile.Write(ref *generationWord, previous + 1);
        return true;
    }

    private bool TrimEmptyReservations()
    {
        if (recordCount != 0 || activeNative != 0 || activeManaged != 0 || draftCount != 0)
            throw new InvalidOperationException("Arena rebalance requires an empty exclusive store");
        if (!AdvanceGeneration()) return false;
        while (chunks != null)
        {
            DataChunk* next = chunks->Next; long bytes = chunks->Bytes, used = chunks->Used;
            TranslationCodeRegions.ReleaseData(chunks, bytes);
            chunks = next; currentChunk = chunks; dataReserved -= bytes; dataUsed -= used;
        }
        return code.TryTrimEmpty();
    }
    private long AdditionalDataReservation(long bytes)
    {
        for (DataChunk* c = currentChunk; c != null; c = c->Next)
            if (Align64(c->Used) <= c->Bytes - bytes) return 0;
        return Math.Max(DataChunkBytes, TranslationCodeRegions.RoundPage(checked(bytes + Align64(sizeof(DataChunk)))));
    }
    private byte* AllocatePacket(long bytes, out TranslationStoreFailure failure)
    {
        failure = TranslationStoreFailure.None; DataChunk* previous = null;
        for (DataChunk* c = currentChunk; c != null; c = c->Next)
        {
            long start = Align64(c->Used);
            if (start <= c->Bytes - bytes)
            {
                dataUsed += start + bytes - c->Used; c->Used = start + bytes; currentChunk = c;
                return (byte*)c + start;
            }
            previous = c;
        }
        long chunkBytes = Math.Max(DataChunkBytes, TranslationCodeRegions.RoundPage(checked(bytes + Align64(sizeof(DataChunk)))));
        // Caller has precharged this growth together with a possible code region.
        DataChunk* added = (DataChunk*)TranslationCodeRegions.AllocateData(chunkBytes);
        if (added == null) { failure = TranslationStoreFailure.NativeAllocation; return null; }
        *added = new DataChunk { Bytes = chunkBytes, Used = Align64(sizeof(DataChunk)) + bytes };
        if (previous != null) previous->Next = added; else if (chunks == null) chunks = added;
        else throw new InvalidOperationException("Missing append arena tail");
        currentChunk = added; dataReserved += chunkBytes; dataUsed += added->Used;
        return (byte*)added + Align64(sizeof(DataChunk));
    }
    private void UpdatePeaks()
    {
        peakUsefulCode = Math.Max(peakUsefulCode, usefulCode); peakPayload = Math.Max(peakPayload, payloadUsed);
        peakDescriptor = Math.Max(peakDescriptor, descriptorUsed); peakIndex = Math.Max(peakIndex, indexUsed);
        peakArenaReserved = Math.Max(peakArenaReserved, dataReserved + code.ReservedBytes);
        peakCharged = Math.Max(peakCharged, CurrentCharge);
    }
    private Body* TokenBody(TranslationToken token, byte state)
    {
        if ((uint)token.Slot >= (uint)recordCount || token.Generation == 0) return null;
        Body* b = records[token.Slot];
        return b->State == state && b->Header.BodyGeneration == token.Generation && b->Header.OwnerCookie == OwnerCookie ? b : null;
    }
    private bool IsDraft(in TranslationDraft draft)
    {
        if (!ReferenceEquals(draft.Owner, this)) return false;
        Body* b = TokenBody(draft.Token, Draft);
        return b != null && draft.Header == (nint)(&b->Header) && draft.CodePointer == code.Address(b->Code) &&
            draft.PayloadPointer == b->Header.Payload && draft.CodeCapacity == b->CodeCapacity && draft.PayloadCapacity == b->PayloadCapacity;
    }
    private int AllocateTicket(int start, int count, int slot, byte kind)
    {
        for (int i = start; i < start + count; ++i)
        {
            if (tickets[i].Kind != 0) continue;
            ulong serial = NextSequence();
            tickets[i] = new Ticket { Serial = serial, Slot = slot, Kind = kind,
                Generation = slot >= 0 ? records[slot]->Header.BodyGeneration : 0 }; return i;
        }
        return -1;
    }
    private ulong NextSequence()
    { if (sequence == ulong.MaxValue) throw new InvalidOperationException("Translation capability space exhausted"); return ++sequence; }
    private bool ValidTicket(int ticket, ulong serial, byte kind) =>
        (uint)ticket < (uint)(managedTicketCount + nativeTicketCount) && serial != 0 && tickets[ticket].Serial == serial && tickets[ticket].Kind == kind;
    private void ValidateLease(in TranslationLease lease)
    {
        ThrowDisposed();
        if (!ReferenceEquals(lease.Owner, this) || !ValidTicket(lease.Ticket, lease.Serial, 1) ||
            tickets[lease.Ticket].Slot != lease.Token.Slot || tickets[lease.Ticket].Generation != lease.Token.Generation)
            throw new InvalidOperationException("Inactive translation lease");
    }
    private void ValidateEpoch(in NativeEpochLease epoch)
    { ThrowDisposed(); if (!ReferenceEquals(epoch.Owner, this) || !ValidTicket(epoch.Ticket, epoch.Serial, 2)) throw new InvalidOperationException("Inactive native epoch"); }
    private void ValidateWorker(in TranslationWorkerCache worker)
    { if (!ReferenceEquals(worker.Owner, this) || (uint)worker.Worker >= (uint)options.MaximumWorkers || worker.Serial == 0 || workers[worker.Worker].Serial != worker.Serial) throw new InvalidOperationException("Inactive worker cache"); }
    private void ThrowDisposed() => ObjectDisposedException.ThrowIf(disposed || disposing, this);
    private static long Align16(long value) => checked((value + 15) & ~15L);
    private static long Align64(long value) => checked((value + 63) & ~63L);
    private static ulong SaturatingEnd(ulong start, ulong length) => start > ulong.MaxValue - (length - 1) ? ulong.MaxValue : start + length - 1;
    private static bool Overlaps(ulong pc, uint bytes, ulong start, ulong length)
        => pc <= SaturatingEnd(start, length) && start <= pc + (bytes - 1UL);
}
