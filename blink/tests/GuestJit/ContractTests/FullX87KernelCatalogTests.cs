using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    // Uses the existing R106 TranslationCodeRegions test partial/hooks. Product
    // builds erase these implementations. Never calls emitted native code.
    internal sealed unsafe partial class FullX87KernelCatalog
    {
        internal bool TestPadKernels;
        internal int TestSealFailureSlot = -1;
        private TranslationCodeRun testFirstRun;
        partial void BeforeStage(int slot, Span<byte> staging, ref int length)
        {
            if (!TestPadKernels) return;
            staging[length..].Fill(0x90);
            length = StagingBytes; // deterministic one-whole-region-per-entry release fixture
        }
        partial void BeforeSeal(int slot, TranslationCodeRun run)
        {
            if (slot == 0) testFirstRun = run;
            if (slot == TestSealFailureSlot) code!.TestFailSeal = true;
        }
        internal void TestQuarantinePublished()
        {
            RequireGate();
            code!.TestFailSeal = true;
            if (code.TrySeal(testFirstRun)) throw new InvalidOperationException("Expected real arena seal failure");
        }
        internal void TestFailNextRegionRelease(int region) => code!.TestFailRegionRelease = region;
        internal void TestFailNextMetadataRelease() => code!.TestFailMetadataRelease = true;
        internal static bool TestEmit(FullX87KernelKey key, Span<byte> staging, out int length) => Emit(key, staging, out length);
    }
}

unsafe partial class Program
{
    // AUTHORED_UNRUN. Parent selector must call this explicitly. These tests
    // emit/copy/seal/read product code and allocate real quota-backed mappings,
    // but never invoke an entry, native delegate, guest or managed semantic op.
    static void CheckFullX87KernelCatalog()
    {
        CheckX87CatalogKeys();
        CheckX87CatalogPublication();
        CheckX87CatalogQuotaFailure();
        CheckX87CatalogQuarantine();
        CheckX87CatalogPartialInitialization();
        CheckX87CatalogStoreBarrier();
        CheckX87CatalogPartialDisposal();
    }

    static void CheckX87CatalogKeys()
    {
        var keys = new HashSet<FullX87KernelKey>();
        foreach (var op in Enum.GetValues<FullExtended80Operation>()) keys.Add(FullX87KernelCatalog.KeyFor(op));
        foreach (var op in Enum.GetValues<FullExtended80Conversion>())
        {
            bool missing = op is FullExtended80Conversion.PackedBcdLoadMissing or FullExtended80Conversion.PackedBcdStoreMissing;
            bool refused = false;
            try { keys.Add(FullX87KernelCatalog.KeyFor(op)); } catch (ArgumentOutOfRangeException) { refused = true; }
            Check(refused == missing, "x87 catalog admits every authored conversion and refuses missing BCD");
        }
        foreach (var op in Enum.GetValues<FullExtended80Control>()) keys.Add(FullX87KernelCatalog.KeyFor(op));
        foreach (var op in Enum.GetValues<FullExtended80Transform>()) keys.Add(FullX87KernelCatalog.KeyFor(op));
        foreach (var format in Enum.GetValues<FullExtended80MemoryArithmeticFormat>())
        {
            foreach (var operation in Enum.GetValues<FullExtended80MemoryArithmeticOperation>())
                keys.Add(FullX87KernelCatalog.KeyFor(operation, format));
            keys.Add(FullX87KernelCatalog.KeyForMemoryCompare(format, false));
            keys.Add(FullX87KernelCatalog.KeyForMemoryCompare(format, true));
        }
        foreach (var operation in Enum.GetValues<FullExtended80BcdOperation>()) keys.Add(FullX87KernelCatalog.KeyFor(operation));
        foreach (var operation in Enum.GetValues<FullExtended80Reduction>()) keys.Add(FullX87KernelCatalog.KeyFor(operation));
        foreach (var operation in Enum.GetValues<FullExtended80Transcendental>()) keys.Add(FullX87KernelCatalog.KeyFor(operation));
        Check(keys.Count == 77 && Enum.GetValues<FullX87KernelKey>().Length == 77 &&
            Enum.GetValues<FullX87KernelKey>().All(keys.Contains), "x87 coherent family catalog is exactly77 distinct typed continuations");
        bool invalid = false;
        try { FullX87KernelCatalog.KeyFor((FullExtended80Operation)255); }
        catch (ArgumentOutOfRangeException) { invalid = true; }
        Check(invalid, "x87 unknown operation has no registry identity");
    }

    static void CheckX87CatalogPublication()
    {
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        byte[] staging = new byte[FullX87KernelCatalog.StagingBytes];
        var snapshots = new byte[FullX87KernelCatalog.MaximumKernels][];
        var bindings = new FullX87KernelBinding[FullX87KernelCatalog.MaximumKernels];
        lock (authority.Gate)
        {
            Check(catalog.ChargedBytes == 0 && store.ExternalReservedBytes == 0 && !catalog.AdmissionOpen,
                "x87 constructor owns no quota/native mapping before registration and Initialize");
            Check(catalog.Initialize(staging, out var failure), "x87 eager catalog publication: " + failure);
            Check(catalog.Count == FullX87KernelCatalog.MaximumKernels && catalog.AdmissionOpen && catalog.ReservedBytes % 65536 == 0 &&
                catalog.ReservedBytes <= FullX87KernelCatalog.MaximumCodeBytes &&
                catalog.ChargedBytes == FullX87KernelCatalog.FixedCharge + catalog.ReservedBytes &&
                store.ExternalReservedBytes == catalog.ChargedBytes &&
                store.GetStatistics().ExternalReservedBytes == catalog.ChargedBytes &&
                store.ChargedBytes <= RetainedOptions().BudgetBytes,
                "x87 bounded packed regions and full fixed/code charge share original TB quota");
            for (int slot = 0; slot < FullX87KernelCatalog.MaximumKernels; ++slot)
            {
                var key = (FullX87KernelKey)slot;
                bindings[slot] = catalog.Find(key);
                var binding = bindings[slot];
                Check(catalog.IsValid(in binding) && binding.Key == key && binding.Slot == slot && binding.Entry != 0,
                    "x87 owner binding has stable finite identity");
                Check(FullX87KernelCatalog.TestEmit(key, staging, out int length) && length == binding.CodeBytes,
                    "x87 published length is the actual continuation length");
                var bytes = new ReadOnlySpan<byte>((void*)binding.Entry, length);
                Check(bytes.SequenceEqual(staging.AsSpan(0, length)), "x87 real RX publication copies exact continuation bytes");
                snapshots[slot] = bytes.ToArray();
                for (int previous = 0; previous < slot; ++previous)
                    Check(binding.Entry >= bindings[previous].Entry + bindings[previous].CodeBytes ||
                        bindings[previous].Entry >= binding.Entry + length, "x87 packed kernel runs never overlap");
            }
            long emitted = catalog.EmittedBytes, reserved = catalog.ReservedBytes;
            Check(emitted == bindings.Sum(b => (long)b.CodeBytes) && emitted <= reserved && reserved < (long)FullX87KernelCatalog.MaximumKernels * 65536,
                "x87 catalog uses packed regions rather than one mapping per kernel");
            staging.AsSpan().Fill(0xcc);
            var token = RetainedPublish(store, 0x400000);
            Check(store.TryAcquireToken(token, out var root), "x87 real TB lease flush prerequisite");
            Check(store.TryBeginNativeEpoch(out var epoch), "x87 original store epoch prerequisite");
            Check(catalog.IsValid(in bindings[0]), "x87 owner binding does not require a released-TB REP protocol");
            epoch.Dispose(); root.Dispose();
            Check(store.TryFlush(out _), "x87 catalog survives original TB whole flush");
            for (int slot = 0; slot < FullX87KernelCatalog.MaximumKernels; ++slot)
            {
                var again = catalog.Find((FullX87KernelKey)slot);
                Check(again.Entry == bindings[slot].Entry &&
                    new ReadOnlySpan<byte>((void*)again.Entry, again.CodeBytes).SequenceEqual(snapshots[slot]),
                    "x87 code remains immutable across staging reuse and TB flush");
            }
            bool twice = false, invalid = false, underGate = false;
            try { catalog.Initialize(staging, out _); } catch (InvalidOperationException) { twice = true; }
            try { catalog.Find((FullX87KernelKey)255); } catch (ArgumentOutOfRangeException) { invalid = true; }
            try { catalog.Dispose(); } catch (InvalidOperationException) { underGate = true; }
            Check(twice && invalid && underGate && catalog.AdmissionOpen && catalog.EmittedBytes == emitted &&
                catalog.ReservedBytes == reserved, "x87 no append/reinitialize/invalid-key and no drain while holding Gate");
            Check(!catalog.IsValid(default), "x87 default binding is never callable");
            using var otherStore = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
            // Constructor-only foreign owner needs no native cleanup/charge.
            var other = new FullX87KernelCatalog(authority, otherStore, true);
            Check(!other.IsValid(in bindings[0]), "x87 binding cannot migrate to another owner");
            for (int window = 0; window < 8; ++window)
            {
                long allocated = CountX87CatalogLookups(catalog, bindings, out bool all);
                Check(all && catalog.EmittedBytes == emitted && catalog.Count == FullX87KernelCatalog.MaximumKernels, "x87 warmed binding reuse keeps immutable finite registry");
                if (window >= 5) Check(allocated == 0, "x87 steady compile-time binding lookup allocates zero managed bytes");
            }
        }
        bool unlocked = false;
        try { _ = bindings[0].Entry; } catch (InvalidOperationException) { unlocked = true; }
        Check(unlocked, "x87 callable entry access requires authority gate");
        store.Dispose(); // explicit required parent teardown order
        catalog.Dispose();
        lock (authority.Gate)
        {
            CheckX87Closed(catalog, in bindings[0]);
            Check(catalog.ChargedBytes == 0 && store.ExternalReservedBytes == 0 && store.ChargedBytes == 0,
                "x87 owner disposal releases all fixed and real code charge");
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static long CountX87CatalogLookups(FullX87KernelCatalog catalog, FullX87KernelBinding[] expected, out bool all)
    {
        long before = GC.GetAllocatedBytesForCurrentThread(); all = true;
        for (int i = 0; i < 10000; ++i)
        {
            int slot = i % FullX87KernelCatalog.MaximumKernels;
            var binding = catalog.Find((FullX87KernelKey)slot);
            all &= catalog.IsValid(in binding) && binding.Entry == expected[slot].Entry;
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    static void CheckX87CatalogQuotaFailure()
    {
        foreach (bool fixedFits in new[] { false, true })
        {
            using var io = new SemanticTestIo();
            using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
            using var store = new RetainedTranslationStore(RetainedOptions() with { BudgetBytes = 1 << 20, PayloadBytes = 0 });
            using var catalog = new FullX87KernelCatalog(authority, store, true);
            byte[] staging = new byte[FullX87KernelCatalog.StagingBytes];
            // Saturate the original quota with a separately owned accounting charge.
            long expected = fixedFits ? FullX87KernelCatalog.FixedCharge : 0;
            long pressure = (1 << 20) - store.GetStatistics().FixedChargedBytes - expected;
            Check(store.TryReserveExternalBytes(pressure, out _), "x87 original quota pressure prerequisite");
            lock (authority.Gate)
            {
                Check(!catalog.Initialize(staging, out var failure) && failure != TranslationStoreFailure.None &&
                    catalog.Count == 0 && catalog.ChargedBytes == expected && catalog.ReservedBytes == 0 && !catalog.AdmissionOpen &&
                    store.ExternalReservedBytes == pressure + expected,
                    "x87 neither fixed metadata nor first code mapping can bypass existing byte quota");
                CheckX87Closed(catalog, default);
            }
            store.ReleaseExternalBytes(pressure);
            store.Dispose();
        }
    }

    static void CheckX87CatalogQuarantine()
    {
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _), "x87 quarantine older-binding prerequisite");
            var older = catalog.Find(FullX87KernelKey.Add);
            Check(catalog.IsValid(in older), "x87 older binding callable before quarantine");
            long charge = catalog.ChargedBytes;
            catalog.TestQuarantinePublished(); // real FailClosed/PROT_NONE, no native invocation
            CheckX87Closed(catalog, in older);
            Check(catalog.ChargedBytes == charge && store.ExternalReservedBytes == charge,
                "x87 seal quarantine keeps every retained byte charged");
        }
        store.Dispose();
    }

    static void CheckX87CatalogPartialInitialization()
    {
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        catalog.TestSealFailureSlot = 1;
        lock (authority.Gate)
        {
            bool failed = false;
            try { catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _); }
            catch (InvalidOperationException) { failed = true; }
            Check(failed && catalog.Count == 0 && catalog.EmittedBytes > 0 && catalog.ReservedBytes > 0 &&
                catalog.ChargedBytes == FullX87KernelCatalog.FixedCharge + catalog.ReservedBytes &&
                store.ExternalReservedBytes == catalog.ChargedBytes,
                "x87 registered failed initializer retains exact code/metadata ownership without partial publication");
            CheckX87Closed(catalog, default);
        }
        store.Dispose(); catalog.Dispose();
        Check(catalog.ChargedBytes == 0 && store.ChargedBytes == 0, "x87 failed initializer cleanup releases actual retained mappings");
    }

    static void CheckX87CatalogStoreBarrier()
    {
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        FullX87KernelBinding binding;
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _), "x87 barrier publication prerequisite");
            binding = catalog.Find(FullX87KernelKey.Divide);
        }
        var token = RetainedPublish(store, 0x500000);
        Check(store.TryAcquireToken(token, out var root), "x87 barrier retained original root lease");
        Check(store.TryBeginNativeEpoch(out var epoch), "x87 barrier retained original native store epoch");
        long charge = catalog.ChargedBytes, beforeEpoch = (long)authority.Epoch;
        bool failed = false;
        try { catalog.Dispose(); } catch (InvalidOperationException) { failed = true; }
        lock (authority.Gate)
        {
            Check(failed && !store.NativeLifetimeClosed && (long)authority.Epoch > beforeEpoch && catalog.ChargedBytes == charge &&
                store.ExternalReservedBytes == charge, "x87 close advances runtime epoch but original TB capabilities forbid unmap");
            CheckX87Closed(catalog, in binding);
        }
        epoch.Dispose(); root.Dispose();
        store.Dispose(); catalog.Dispose();
        Check(catalog.ChargedBytes == 0 && store.ChargedBytes == 0, "x87 unmap follows successful TB-store barrier");
    }

    static void CheckX87CatalogPartialDisposal()
    {
        foreach (bool metadataFailure in new[] { false, true })
        foreach (bool storeDisposedFirst in new[] { false, true })
        {
            using var io = new SemanticTestIo();
            using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
            using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0, BudgetBytes = FullX87KernelCatalog.MaximumCodeBytes + (4L << 20) });
            using var catalog = new FullX87KernelCatalog(authority, store, true);
            catalog.TestPadKernels = true;
            FullX87KernelBinding binding;
            lock (authority.Gate)
            {
                Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _), "x87 multi-region release prerequisite");
                binding = catalog.Find(FullX87KernelKey.Add);
                Check(catalog.ReservedBytes == FullX87KernelCatalog.MaximumCodeBytes,
                    "x87 padding fixture reaches finite maximum reservation exactly");
                if (metadataFailure) catalog.TestFailNextMetadataRelease();
                else catalog.TestFailNextRegionRelease(1);
            }
            if (storeDisposedFirst) store.Dispose();
            else
            {
                bool barrierRejected = false;
                try { catalog.Dispose(); } catch (InvalidOperationException) { barrierRejected = true; }
                Check(barrierRejected && !store.NativeLifetimeClosed &&
                    catalog.ReservedBytes == FullX87KernelCatalog.MaximumCodeBytes &&
                    store.ExternalReservedBytes == catalog.ChargedBytes,
                    "x87 catalog never disposes borrowed live store and retains every mapping without its barrier");
                store.Dispose();
            }
            bool failed = false;
            try { catalog.Dispose(); } catch (InvalidOperationException) { failed = true; }
            long remaining = metadataFailure ? 0 : FullX87KernelCatalog.MaximumCodeBytes - 65536;
            long charged = remaining + FullX87KernelCatalog.FixedCharge;
            lock (authority.Gate)
            {
                Check(failed && catalog.ReservedBytes == remaining && catalog.ChargedBytes == charged &&
                    store.ExternalReservedBytes == charged && store.ChargedBytes == charged &&
                    store.GetStatistics().ChargedBytes == charged && store.GetStatistics().NativeReservedBytes == charged &&
                    store.GetStatistics().ExternalReservedBytes == charged,
                    "x87 partial unmap refunds only actual released regions, retaining full metadata until retry");
                CheckX87Closed(catalog, in binding);
            }
            catalog.Dispose(); catalog.Dispose();
            Check(catalog.ReservedBytes == 0 && catalog.ChargedBytes == 0 && store.ExternalReservedBytes == 0 &&
                store.ChargedBytes == 0 && store.GetStatistics().NativeReservedBytes == 0,
                "x87 cleanup retry releases every code/metadata charge without double refund");
        }
    }

    static void CheckX87Closed(FullX87KernelCatalog catalog, in FullX87KernelBinding older)
    {
        bool find = false, entry = false;
        try { catalog.Find(FullX87KernelKey.Add); } catch (InvalidOperationException) { find = true; }
        try { _ = older.Entry; } catch (InvalidOperationException) { entry = true; }
        Check(!catalog.AdmissionOpen && !catalog.IsValid(in older) && find && entry,
            "x87 closed/quarantined/partial-dispose owner refuses Find, validation and older callable binding");
    }
}
