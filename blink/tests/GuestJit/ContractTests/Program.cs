using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    const ulong Ip = 0x400000, Page = 0x100000;
    static int checks;
    static void Check(bool condition, string name)
    { if (!condition) throw new InvalidOperationException(name); ++checks; }

    static int Main(string[] args)
    {
        if (args is ["native-inventory", var input, var output]) return WriteNativeLoweringInventory(input, output);
        string? unavailable = NativeCodePlatform.UnavailableReason(allowUnverifiedWindows: true);
        if (unavailable != null) { Console.WriteLine("SKIP native contract: " + unavailable); return 0; }
        byte* machine = (byte*)NativeMemory.AllocZeroed((nuint)sizeof(Blink.Machine));
        byte* data = (byte*)NativeMemory.AlignedAlloc(4096, 4096);
        try
        {
            if (args is ["owned-shared-read-family"])
            {
                CheckFullNativeCompletionR189();
                CheckFullOwnedSharedReadR207(machine);
                CheckFullRetainedCostR205(machine);
                PrintQualificationCounts("R207 owned shared READ mapping family (source-authored, no speed claim)");
                Console.WriteLine($"PASS owned shared READ mapping family checks={checks}"); return 0;
            }
            if (args is ["retained-ticket-cost-family"])
            {
                CheckFullCodeRoleMappingsR204(machine);
                CheckFullRetainedCostR205(machine);
                PrintQualificationCounts("combined R204/R205 mapping and retained-ticket cost family");
                Console.WriteLine($"PASS combined mapping/retained-ticket cost family checks={checks}"); return 0;
            }
            if (args is ["code-role-mapping-authority-family"])
            {
                CheckFullCodeRoleMappingsR204(machine);
                PrintQualificationCounts("R204 code-role/data-cache authority family (no speed claim)");
                Console.WriteLine($"PASS code-role mapping authority family checks={checks}"); return 0;
            }
            if (args is ["authority-guard-family"])
            {
                var failures = new List<Exception>();
                try { CheckFullImmutableGuardR202(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullPreciseMappingsR203(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("combined immutable guard and precise mapping authority family");
                if (failures.Count != 0) throw new AggregateException("Authority/guard family failed", failures);
                Console.WriteLine($"PASS authority/guard family checks={checks}"); return 0;
            }
            if (args is ["precise-mapping-authority-family"])
            {
                CheckFullPreciseMappingsR203(machine);
                PrintQualificationCounts("R203 precise mapping/code authority family");
                Console.WriteLine($"PASS precise mapping authority family checks={checks}"); return 0;
            }
            if (args is ["immutable-guard-family"])
                return CheckFullImmutableGuardR202(machine);
            if (args is ["code-snapshot-epoch-family"])
            {
                CheckFullCodeSnapshotsR201(machine);
                PrintQualificationCounts("R201 code snapshot/content/eligibility epoch family");
                Console.WriteLine($"PASS code snapshot epoch family checks={checks}"); return 0;
            }
            if (args is ["retained-native-entry-family"])
                return CheckFullRetainedNativeEntryR195(machine);
            if (args is ["mapping-cache-epoch-family"])
            {
                CheckFullMappingCacheEpochR193(machine);
                PrintQualificationCounts("mapping cache lifetime/epoch family");
                Console.WriteLine($"PASS mapping cache epoch family checks={checks}"); return 0;
            }
            if (args is ["address-extension-family"])
            {
                CheckFullAddressExtensionsR192(machine);
                PrintQualificationCounts("R192 ordinary address and integer-extension family");
                Console.WriteLine($"PASS address-extension family checks={checks}"); return 0;
            }
            if (args is ["native-completion-family" or "owner-private-completion-family"])
            {
                CheckFullNativeCompletionR189();
                Console.WriteLine($"PASS native completion family checks={checks}"); return 0;
            }
            if (args is ["native-memory-cache-family"])
            {
                CheckFullNativeMemoryCacheR181(machine);
                Console.WriteLine($"PASS native memory-cache family checks={checks}"); return 0;
            }
            if (args is ["processor-environment-family"])
            {
                var failures=new List<Exception>();
                try { CheckFullOwnerScaffoldEncodingAndAllocation(); CheckFullOwnerScaffoldReuse(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeProcessorEnvironment(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("processor clock/entropy data assist and emitted computation family");
                if(failures.Count!=0)throw new AggregateException("Processor environment family batch failed",failures);
                Console.WriteLine($"PASS native processor environment family checks={checks}"); return 0;
            }
            if (args is ["portable-computation-family"])
            {
                var failures=new List<Exception>();
                try { CheckFullNativeNearControl(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeResidualScalars(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullX87KernelCatalog(); CheckFullX87KernelOwner(); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullCanonicalX87PortableFamily(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("near-control and portable all8 canonical/catalog/controller family");
                if(failures.Count!=0)throw new AggregateException("Portable computation family batch failed",failures);
                Console.WriteLine($"PASS native portable computation family checks={checks}"); return 0;
            }
            if (args is ["near-control-software-family"])
            {
                var failures=new List<Exception>();
                try { CheckFullNativeNearControl(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { Trans168SoftwareCodeSizes(); Trans168SoftwareFamilySuite(); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("near-control family and standalone portable transcendental correspondence");
                if(failures.Count!=0)throw new AggregateException("Near-control/software family batch failed",failures);
                Console.WriteLine($"PASS native near-control and standalone software family checks={checks}"); return 0;
            }
            if (args is ["native-remaining-family"])
            {
                var failures=new List<Exception>();
                try { CheckFullNativeProcessorQueries(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullCanonicalX87Controls(machine); CheckFullX87State(); CheckFullCanonicalX87Prefixes(machine); CheckR159ProjectionCells(); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullCanonicalX87Transcendentals(machine, integerOnly:true); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("processor queries, x87 state and integer transcendental subdomains");
                if(failures.Count!=0)throw new AggregateException("Remaining native family batch failed",failures);
                Console.WriteLine($"PASS native remaining family batch checks={checks}"); return 0;
            }
            if (args is ["ordinary-form-family"])
            {
                var failures=new List<Exception>();
                try { CheckFullNativeStrings(machine); CheckFullStringFormsR162(machine); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullCanonicalX87Controls(machine); CheckFullCanonicalX87Prefixes(machine); CheckR159ProjectionCells(); }
                catch(Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("ordinary string and x87 state form families");
                if(failures.Count!=0)throw new AggregateException("Ordinary form family batch failed",failures);
                Console.WriteLine($"PASS ordinary string and x87 state form families checks={checks}"); return 0;
            }
            if (args is ["canonical-encoding-family"])
            { CheckFullCanonicalEncodingR159(machine); PrintQualificationCounts("prefix/encoding family"); return 0; }
            if (args is ["canonical-x87-controls"])
            {
                CheckFullCanonicalX87Controls(machine); PrintQualificationCounts("control/state family");
                Console.WriteLine($"PASS native complete x87 control/state family checks={checks}"); return 0;
            }
            if (args is ["canonical-x87-family"]) return CheckFullCanonicalX87Family(machine);
            if (args is ["atomic-family", var registeredGuest])
            {
                CheckFullAtomicFamilyR156(machine); CheckFullAtomicRegisteredR156(registeredGuest);
                Console.WriteLine($"PASS coherent atomic family checks={checks}");
                return 0;
            }
            if (args is ["dynamic-control-cache-family"])
            {
                CheckFullJumpCacheIndex(machine);
                Console.WriteLine($"PASS dynamic control cache family checks={checks}");
                return 0;
            }
            if (args is ["native-family-batch", var atomicGuest])
            {
                var failures = new List<Exception>();
                try { CheckFullCanonicalX87Family(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullAtomicFamilyR156(machine); CheckFullAtomicRegisteredR156(atomicGuest); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullCanonicalEncodingR159(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullStringFormsR162(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeNearControl(machine); CheckFullNativeResidualScalars(machine); CheckFullNativeProcessorQueries(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeProcessorEnvironment(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeMemoryCacheR181(machine); CheckFullRetainedAdmissionR185(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullNativeCompletionR189(); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullBoundedCopiesR190(); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullAddressExtensionsR192(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullMappingCacheEpochR193(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullSharedMemoryGuardsR188(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                try { CheckFullJumpCacheIndex(machine); }
                catch (Exception error) { failures.Add(error); Console.Error.WriteLine(error); }
                PrintQualificationCounts("complete native family batch");
                if (failures.Count != 0) throw new AggregateException("Native family integration batch failed.", failures);
                Console.WriteLine($"PASS native x87 and atomic family batch checks={checks}");
                return 0;
            }
            if (args is ["enter-materialization"])
            {
                CheckFullEnterDemandMaterialization();
                Console.WriteLine($"PASS native ENTER mapping-only demand checks={checks}");
                return 0;
            }
            if (args is ["enter"])
            {
                CheckFullNativeEnter(machine); CheckFullEnterDemandMaterialization();
                Console.WriteLine($"PASS native ENTER checks={checks}");
                return 0;
            }
            if (args is ["extended80-transforms"])
            {
                CheckFullExtended80Transforms();
                Console.WriteLine($"PASS standalone binary80 transforms checks={checks}");
                return 0;
            }
            if (args is ["x87-owned-frame"])
            {
                CheckFullX87OwnedFrame();
                Console.WriteLine($"PASS exact canonical workframe and fatal-return ownership checks={checks}");
                return 0;
            }
            if (args is ["extended80-bcd"])
            {
                CheckFullExtended80Bcd();
                Console.WriteLine($"PASS standalone binary80 packed BCD checks={checks}");
                return 0;
            }
            if (args is ["x87-memory-materialization"])
            {
                CheckFullCanonicalX87ProbeMaterialization();
                Console.WriteLine($"PASS canonical x87 permission-only materialization checks={checks}");
                return 0;
            }
            if (args is ["canonical-x87-memory"])
            {
                CheckFullCanonicalX87Memory(machine);
                Console.WriteLine($"PASS native canonical x87 memory conversions checks={checks}");
                return 0;
            }
            if (args is ["canonical-x87-arithmetic"])
            {
                CheckFullCanonicalX87Arithmetic(machine);
                Console.WriteLine($"PASS native canonical x87 arithmetic checks={checks}");
                return 0;
            }
            if (args is ["x87-catalog"])
            {
                CheckFullX87KernelCatalog(); CheckFullX87KernelOwner();
                Console.WriteLine($"PASS immutable x87 kernel catalog and owner checks={checks}");
                return 0;
            }
            if (args is ["x87-work-storage"])
            {
                CheckFullX87WorkStorage();
                Console.WriteLine($"PASS charged canonical x87 native scratch checks={checks}");
                return 0;
            }
            if (args is ["canonical-x87-stack"])
            {
                CheckFullCanonicalX87Stack(machine);
                Console.WriteLine($"PASS native canonical x87 stack/control checks={checks}");
                return 0;
            }
            if (args is ["scaffold"])
            {
                CheckFullBlockEmitter(machine);
                Console.WriteLine($"PASS native scaffold/issue contracts checks={checks}");
                return 0;
            }
            if (args is ["canonical-mmx"])
            {
                CheckFullCanonicalMmx(machine);
                Console.WriteLine($"PASS canonical physical80 MMX checks={checks}");
                return 0;
            }
            if (args is ["x87-transport"])
            {
                CheckFullX87SignalTransport();
                Console.WriteLine($"PASS actual canonical x87 signal transport checks={checks}");
                return 0;
            }
            if (args is ["x87-rep-owner"])
            {
                CheckFullRepControllerR104();
                CheckFullRepOrdinaryState();
                Console.WriteLine($"PASS REP owner return integration checks={checks}");
                return 0;
            }
            if (args is ["x87-frame"])
            {
                CheckFullX87Frame(machine);
                Console.WriteLine($"PASS canonical x87 ABI7 frame checks={checks}");
                return 0;
            }
            if (args is ["x87-ownership"])
            {
                CheckFullX87Ownership();
                Console.WriteLine($"PASS inactive x87 ownership checks={checks}");
                return 0;
            }
            if (args is ["x87-state"])
            {
                CheckFullX87State();
                Console.WriteLine($"PASS canonical x87 state codecs checks={checks}");
                return 0;
            }
            if (args is ["extended80-controls"])
            {
                CheckFullExtended80Controls();
                Console.WriteLine($"PASS standalone binary80 controls checks={checks}");
                return 0;
            }
            if (args is ["extended80-conversions"])
            {
                CheckFullExtended80Conversions();
                Console.WriteLine($"PASS standalone binary80 conversions checks={checks}");
                return 0;
            }
            if (args is ["extended80"])
            {
                CheckFullExtended80Native();
                Console.WriteLine($"PASS standalone binary80 foundation checks={checks}");
                return 0;
            }
            if (args is ["masked-store"])
            {
                CheckFullNativeMaskedStore(machine); CheckFullMaskedStoreUnion(machine); CheckFullMaskedStoreControllerBoundaries();
                Console.WriteLine($"PASS native masked stores checks={checks}");
                return 0;
            }
            if (args is ["checksum"])
            {
                CheckFullNativeChecksum(machine);
                Console.WriteLine($"PASS native checksum checks={checks}");
                return 0;
            }
            if (args is ["flag-stack"])
            {
                CheckFullNativeFlagStack(machine);
                Console.WriteLine($"PASS native flag stack checks={checks}");
                return 0;
            }
            if (args is ["coverage-wave"])
            {
                CheckFullNativeFlagStack(machine); CheckFullNativeChecksum(machine);
                CheckFullNativeScalarMisc(machine); CheckFullNativeBitCounts(machine);
                CheckFullNativeUnlockedBits(machine); CheckFullNativeVectorExtended(machine);
                CheckFullNativeMmx(machine); CheckFullNativeFloatMmx(machine);
                CheckFullNativeFloatExtended(machine); CheckFullNativeSystemState(machine);
                CheckFullNativeSegmentFaults(machine); CheckFullNativeDenseMemory(); CheckFullNativeR118(machine);
                Console.WriteLine($"PASS targeted native coverage wave checks={checks}");
                return 0;
            }
            if (args is ["r118-response"])
            {
                CheckFullNativeR118(machine);
                Console.WriteLine($"PASS isolated R118 native response checks={checks}");
                return 0;
            }
            if (args is ["system-state"])
            {
                CheckFullNativeSystemState(machine);
                Console.WriteLine($"PASS isolated native state checks={checks}");
                return 0;
            }
            if (args is ["dense-memory"])
            {
                CheckFullNativeDenseMemory();
                Console.WriteLine($"PASS isolated dense native mapping checks={checks}");
                return 0;
            }
            if (args is ["mmx"])
            {
                CheckFullNativeMmx(machine);
                Console.WriteLine($"PASS isolated native MMX checks={checks}");
                return 0;
            }
            if (args is ["float-extended"])
            {
                CheckFullNativeFloatExtended(machine);
                Console.WriteLine($"PASS isolated native extended-float checks={checks}");
                return 0;
            }
            if (args is ["segment-faults"])
            {
                CheckFullNativeSegmentFaults(machine);
                Console.WriteLine($"PASS isolated native segment-fault checks={checks}");
                return 0;
            }
            if (args is ["coverage-new"])
            {
                CheckFullNativeScalarMisc(machine);
                CheckFullNativeUnlockedBits(machine);
                CheckFullNativeBitCounts(machine);
                CheckFullNativeVectorExtended(machine);
                CheckFullNativeFloatMmx(machine);
                Console.WriteLine($"PASS targeted new native coverage checks={checks}");
                return 0;
            }
            if (args is ["float-mmx"])
            {
                CheckFullNativeFloatMmx(machine);
                Console.WriteLine($"PASS isolated native MMX-FP-conversion checks={checks}");
                return 0;
            }
            if (args is ["bit-counts"])
            {
                CheckFullNativeBitCounts(machine);
                Console.WriteLine($"PASS isolated native bit-count checks={checks}");
                return 0;
            }
            if (args is ["vector-extended"])
            {
                CheckFullNativeVectorExtended(machine);
                Console.WriteLine($"PASS isolated native extended-vector checks={checks}");
                return 0;
            }
            if (args is ["unlocked-bits"])
            {
                CheckFullNativeUnlockedBits(machine);
                Console.WriteLine($"PASS isolated native unlocked-bit checks={checks}");
                return 0;
            }
            if (args is ["scalar-misc"])
            {
                CheckFullNativeScalarMisc(machine);
                Console.WriteLine($"PASS isolated native scalar-misc checks={checks}");
                return 0;
            }
            if (args is ["rep-kernel"])
            {
                CheckFullRepKernelStore();
                CheckFullRepKernels(machine);
                CheckRepSharedYieldCleanup(machine);
                Console.WriteLine($"PASS isolated REP kernel/storage checks={checks}; controller qualification excluded");
                return 0;
            }
            CheckRetainedTranslationStore();
            CheckFullRepKernelStore();
            CheckFullSemanticAuthority();
            CheckFullMemoryMissDiagnostics();
            CheckFullNativeDenseMemory();
            CheckFullLastExitLinks(machine);
            CheckFullStaticExitMetadata();
            CheckFullRetainedAuthority(machine);
            CheckFullNativeStrings(machine);
            Console.WriteLine("native-string contracts passed");
            CheckFullRepContracts(machine);
            CheckFullRepControllerR104();
            CheckFullRepOrdinaryState();
            CheckRepSharedYieldCleanup(machine);
            Console.WriteLine("native-REP contracts passed");
            Console.WriteLine("native-last-exit-link contracts passed");
            CheckFullNativeScalarMisc(machine);
            CheckFullNativeBitCounts(machine);
            CheckFullNativeSystemState(machine);
            CheckFullNativeSegmentFaults(machine);
            CheckFullNativeControlFlags(machine);
            Console.WriteLine("native-control-flag contracts passed");
            CheckFullNativeHints(machine);
            CheckFullNativeResidualScalars(machine);
            Console.WriteLine("native-hint contracts passed");
            CheckFullAddressExtensionsR192(machine);
            CheckFullNativeInteger(machine);
            Console.WriteLine("native-integer contracts passed");
            CheckFullNativeMemory(machine);
            CheckFullNativeStackMemory(machine);
            CheckFullStackMemoryAdmission();
            Console.WriteLine("native-stack-memory contracts passed");
            Console.WriteLine("native-memory contracts passed");
            CheckFullNativeMemoryInteger(machine);
            CheckFullNativeUnlockedBits(machine);
            Console.WriteLine("native-memory-integer contracts passed");
            CheckFullNativeController();
            CheckFullNativeCompletionR189();
            Console.WriteLine("native-controller contracts passed");
            CheckFullNativeVector(machine);
            CheckFullNativeMmx(machine);
            CheckFullNativeVectorExtended(machine);
            Console.WriteLine("native-vector contracts passed");
            CheckFullNativeVectorMove(machine);
            Console.WriteLine("native-vector-move contracts passed");
            CheckFullNativeFloat(machine);
            CheckFullNativeFloatExtended(machine);
            CheckFullNativeFloatMmx(machine);
            Console.WriteLine("native-float contracts passed");
            CheckFullBlockEmitter(machine);
            CheckFullCodeEpochs(machine);
            CheckHostMemoryFileMutation();
            CheckHostMemoryFileRead();
            CheckFullController();
            CheckQemuAuthorityScopes();
            CheckQemuProvenanceMigration();
            CheckQemuLinkPreparation();
            CheckQemuOwnerScaffoldLifetime();
            CheckQemuBlockPolicy();
            CheckQemuPublicationProgress();
            for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
            JitDataDescriptor* descriptors = stackalloc JitDataDescriptor[4];
            foreach (var form in new[] { (JitOperation.LoadMov, 32, 32), (JitOperation.LoadMov, 64, 64),
                (JitOperation.LoadMovzx, 32, 8), (JitOperation.LoadMovzx, 64, 8),
                (JitOperation.LoadMovzx, 32, 16), (JitOperation.LoadMovzx, 64, 16) })
            {
                var load = new JitInstruction(Ip, 4, form.Item1, form.Item2, new(15, 0), default,
                    SourceWidth: form.Item3, Address: new(3, 12, 3, -48));
                byte[] emitted = X64BlockEmitter.Emit([load], 1);
                Check(NativeCodeAllocation.TryCreate(emitted, out var allocation, out string reason), reason);
                using (allocation!)
                {
                    foreach (string kind in new[] { "hit", "fourth-hit", "epoch-hit", "epoch-zero", "tag", "read", "identity", "generation", "host",
                        "start", "end", "bad-end", "cross-page", "noncanonical", "negative", "count-zero", "count-five",
                        "null-array", "budget", "stop", "attention" })
                    {
                        Reset(machine);
                        for (int i = 0; i < 4; ++i) descriptors[i] = new()
                        { Page = Page + (ulong)i * 4096, Host = (nint)data, Identity = 1, Generation = 1,
                            ValidEnd = 4096, Permissions = 1 };
                        long issued = 0, completed = 0; int stop = kind == "stop" ? 1 : 0;
                        uint count = kind == "count-zero" ? 0u : kind == "count-five" ? 5u : 4u;
                        if (kind == "fourth-hit") (descriptors[0], descriptors[3]) = (descriptors[3], descriptors[0]);
                        if (kind == "tag") descriptors[0].Page = Page + 16384;
                        if (kind == "read") descriptors[0].Permissions = 0;
                        if (kind == "identity") descriptors[0].Identity = 0;
                        if (kind == "generation") descriptors[0].Generation = 2;
                        if (kind == "epoch-hit") descriptors[0].Generation = 0xf1234567;
                        if (kind == "host") descriptors[0].Host = 0;
                        if (kind == "start") descriptors[0].ValidStart = 33;
                        if (kind == "end") descriptors[0].ValidEnd = (uint)(31 + form.Item3 / 8);
                        if (kind == "bad-end") descriptors[0].ValidEnd = 4097;
                        // The byte load at offset 4095 is a valid one-page hit.
                        if (kind == "cross-page") Write64(machine, 24 + 3 * 8, Page + 4095 + 32);
                        if (kind == "noncanonical") Write64(machine, 24 + 3 * 8, (1UL << 47) + 32);
                        if (kind == "negative") Write64(machine, 24 + 3 * 8, ulong.MaxValue - 31);
                        if (kind == "attention") machine[11] = 1;
                        byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                        JitFrame frame = new() { DataGeneration = kind == "epoch-zero" ? 0u : kind == "epoch-hit" ? 0xf1234567u : 1u, Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame), Machine = machine,
                            Issued = &issued, Completed = &completed, Stop = &stop, Budget = kind == "budget" ? 0UL : 100UL,
                            DataDescriptors = kind == "null-array" ? null : descriptors, DataDescriptorCount = count };
                        var call = (delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry;
                        int returned = call(&frame);
                        bool hit = kind is "hit" or "fourth-hit" or "epoch-hit" || kind == "cross-page" && form.Item3 == 8;
                        JitExit expected = hit ? JitExit.Continue : kind == "budget" ? JitExit.Budget :
                            kind == "stop" ? JitExit.Stop : kind == "attention" ? JitExit.Attention : JitExit.MemoryMiss;
                        Check(returned == (int)expected && frame.Exit == expected, kind + " exit");
                        Check(issued == (hit || kind == "budget" ? 1 : 0) && completed == (hit ? 1 : 0) &&
                            frame.Executed == (hit ? 1UL : 0UL), kind + " counters");
                        if (hit)
                        {
                            int offset = kind == "cross-page" ? 4095 : 32;
                            ulong result = form.Item3 switch { 8 => data[offset],
                                16 => BinaryPrimitives.ReadUInt16LittleEndian(new(data + offset, 2)),
                                32 => BinaryPrimitives.ReadUInt32LittleEndian(new(data + offset, 4)),
                                _ => BinaryPrimitives.ReadUInt64LittleEndian(new(data + offset, 8)) };
                            BinaryPrimitives.WriteUInt64LittleEndian(before.AsSpan(24 + 15 * 8), result);
                            BinaryPrimitives.WriteUInt64LittleEndian(before, Ip + 4); before[8] = 0;
                        }
                        Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), kind + " complete machine state");
                        Check(frame.NextIp == (hit ? Ip + 4 : Ip), kind + " NextIp");
                    }
                }
            }
            // A prior committed scalar op remains visible on a later load miss;
            // the missed instruction reserves nothing and retains its own IP.
            Reset(machine);
            var first = new JitInstruction(Ip, 5, JitOperation.Mov, 32, new(0, 0), new(0, 123, true));
            var second = new JitInstruction(Ip + 5, 3, JitOperation.LoadMov, 64, new(1, 0), default,
                SourceWidth: 64, Address: new(3, -1, 0, 0));
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit([first, second], 2), out var pair, out string why), why);
            using (pair!)
            {
                byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                BinaryPrimitives.WriteUInt64LittleEndian(expected, Ip + 5); expected[8] = 0;
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24), 123);
                long issued = 0, completed = 0; int stop = 0;
                JitFrame frame = new() { DataGeneration = 1, Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop, Budget = 100 };
                ((delegate* unmanaged<JitFrame*, int>)(void*)pair!.Entry)(&frame);
                Check(frame.Exit == JitExit.MemoryMiss && issued == 1 && completed == 1 && frame.Executed == 1 &&
                    frame.NextIp == Ip + 5, "partial-block miss accounting");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected), "partial-block miss full state");
            }
            CheckNativeLimits(machine, data);
            CheckFullNativeDispatcher(machine);
            CheckSemanticReadLifetime();
            CheckAddressPlans(machine);
            CheckStoresAndStack(machine, data, descriptors);
            CheckMemorySequences(machine, data, descriptors);
            CheckCallReturnTraces(machine, data, descriptors);
            CheckNestedCallReturns(machine, data, descriptors);
            CheckMemoryFlags(machine, data, descriptors);
            CheckConditionalTraces(machine, data, descriptors);
            var loadPrefix = new JitInstruction(Ip, 4, JitOperation.LoadMov, 64, new(0, 0), default,
                SourceWidth: 64, Address: new(3, -1, 0, 0));
            var addPrefix = new JitInstruction(Ip + 4, 4, JitOperation.Add, 64, new(0, 0), new(0, 1, true));
            foreach (bool fullLoop in new[] { false, true })
            {
                var jump = new JitInstruction(Ip + 8, 2, JitOperation.ConditionalJump, 64, default, default,
                    Target: fullLoop ? Ip : Ip + 4);
                byte[] counts = JitMemoryCounts.Build([loadPrefix, addPrefix, jump]);
                for (int n = 0; n <= 32; ++n)
                    Check(counts[n] == (fullLoop ? (n + 2) / 3 : n == 0 ? 0 : 1), "exact native memory prefix count");
            }
        }
        finally { NativeMemory.Free(machine); NativeMemory.AlignedFree(data); }
        Console.WriteLine("PASS emitter contract checks=" + checks);
        return 0;
    }

    static void CheckStoresAndStack(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        var forms = new List<JitInstruction>();
        foreach (int width in new[] { 32, 64 })
        {
            forms.Add(new(Ip, 4, JitOperation.StoreMov, width, default, new(15, 0),
                SourceWidth: width, Address: new(3, 12, 3, -48)));
            forms.Add(new(Ip, 4, JitOperation.StoreMov, width, default, new(4, 0),
                SourceWidth: width, Address: new(4, -1, 0, 0)));
            // Address base aliases the source; it must be read before effects.
            forms.Add(new(Ip, 4, JitOperation.StoreMov, width, default, new(3, 0),
                SourceWidth: width, Address: new(3, 12, 3, -48)));
            forms.Add(new(Ip, 4, JitOperation.StoreMov, width, default, new(12, 0),
                SourceWidth: width, Address: new(3, 12, 3, -48)));
        }
        // All guest register numbers, including REX.B and both RSP special cases.
        for (int register = 0; register < 16; ++register)
        {
            forms.Add(new(Ip, 2, JitOperation.Push, 64, default, new(register, 0),
                SourceWidth: 64, Address: new(4, -1, 0, -8)));
            forms.Add(new(Ip, 2, JitOperation.Pop, 64, new(register, 0), default,
                SourceWidth: 64, Address: new(4, -1, 0, 0)));
        }
        // CALL/RET share the exact descriptor and pre-issuance guard matrix.
        // Self targets must still leave the native frame after one guest op.
        forms.Add(new(Ip, 5, JitOperation.Call, 64, default, default, Target: Ip,
            SourceWidth: 64, Address: new(4, -1, 0, -8)));
        forms.Add(new(Ip, 1, JitOperation.Ret, 64, default, default,
            SourceWidth: 64, Address: new(4, -1, 0, 0)));
        foreach (var instruction in forms)
        {
            bool write = instruction.Operation is JitOperation.StoreMov or JitOperation.Push or JitOperation.Call;
            string label = instruction.Operation + "/" + instruction.Width + "/" +
                (write ? instruction.Source.Register : instruction.Destination.Register);
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit([instruction], 1), out var allocation, out string reason), reason);
            using (allocation!)
            {
                foreach (string kind in new[] { "hit", "fourth-hit", "epoch-hit", "epoch-zero", "read-only", "write-only", "neither", "tag", "identity",
                    "generation", "host", "host-wrap", "start", "end", "bad-end", "page-end", "cross-page",
                    "noncanonical", "negative", "count-zero", "count-five", "null-array", "budget", "stop", "attention" })
                {
                    Reset(machine);
                    for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                    int width = instruction.SourceWidth / 8;
                    ulong address = kind switch
                    {
                        "page-end" => Page + 4096 - (uint)width,
                        "cross-page" => Page + 4097 - (uint)width,
                        "noncanonical" => 1UL << 47,
                        "negative" => ulong.MaxValue - 31,
                        _ => Page + 32
                    };
                    if (instruction.Address.Base == 4)
                        Write64(machine, 56, unchecked(address + (instruction.Operation is JitOperation.Push or JitOperation.Call ? 8UL : 0UL)));
                    else Write64(machine, 48, unchecked(address + 32));
                    if (instruction.Operation == JitOperation.Ret && address >= Page && address <= Page + 4088)
                        Write64(data, (int)(address - Page), Ip);
                    for (int i = 0; i < 4; ++i) descriptors[i] = new()
                    { Page = Page + (ulong)i * 4096, Host = (nint)data, Identity = 1, Generation = 1,
                        ValidEnd = 4096, Permissions = 3 };
                    uint count = kind == "count-zero" ? 0u : kind == "count-five" ? 5u : 4u;
                    if (kind == "fourth-hit") (descriptors[0], descriptors[3]) = (descriptors[3], descriptors[0]);
                    if (kind == "read-only") descriptors[0].Permissions = 1;
                    if (kind == "write-only") descriptors[0].Permissions = 2;
                    if (kind == "neither") descriptors[0].Permissions = 0;
                    if (kind == "tag") descriptors[0].Page = Page + 16384;
                    if (kind == "identity") descriptors[0].Identity = 0;
                    if (kind == "generation") descriptors[0].Generation = 2;
                        if (kind == "epoch-hit") descriptors[0].Generation = 0xf1234567;
                    if (kind == "host") descriptors[0].Host = 0;
                    if (kind == "host-wrap") descriptors[0].Host = unchecked((nint)(-16));
                    if (kind == "start") descriptors[0].ValidStart = 33;
                    if (kind == "end") descriptors[0].ValidEnd = (uint)(31 + width);
                    if (kind == "bad-end") descriptors[0].ValidEnd = 4097;
                    if (kind == "attention") machine[11] = 1;
                    byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                    long issued = 0, completed = 0; int stop = kind == "stop" ? 1 : 0;
                    JitFrame frame = new() { DataGeneration = kind == "epoch-zero" ? 0u : kind == "epoch-hit" ? 0xf1234567u : 1u, Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame), Machine = machine,
                        Issued = &issued, Completed = &completed, Stop = &stop, Budget = kind == "budget" ? 0UL : 100UL,
                        DataDescriptors = kind == "null-array" ? null : descriptors, DataDescriptorCount = count };
                    int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry)(&frame);
                    bool hit = kind is "hit" or "fourth-hit" or "epoch-hit" or "page-end" ||
                        kind == "read-only" && !write || kind == "write-only" && write;
                    JitExit expectedExit = hit ? JitExit.Continue : kind == "budget" ? JitExit.Budget :
                        kind == "stop" ? JitExit.Stop : kind == "attention" ? JitExit.Attention : JitExit.MemoryMiss;
                    string name = label + "/" + kind;
                    Check(returned == (int)expectedExit && frame.Exit == expectedExit, name + " exit");
                    Check(issued == (hit || kind == "budget" ? 1 : 0) && completed == (hit ? 1 : 0) &&
                        frame.Executed == (hit ? 1UL : 0UL), name + " counters");
                    if (hit)
                    {
                        int offset = (int)(address - Page);
                        ulong oldSp = Read64(expectedMachine, 56);
                        if (write)
                        {
                            ulong source = instruction.Operation == JitOperation.Call ? Ip + (uint)instruction.Length :
                                Read64(expectedMachine, 24 + instruction.Source.Register * 8);
                            if (width == 4) BinaryPrimitives.WriteUInt32LittleEndian(expectedData.AsSpan(offset), (uint)source);
                            else BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(offset), source);
                            if (instruction.Operation is JitOperation.Push or JitOperation.Call)
                                BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), unchecked(oldSp - 8));
                        }
                        else
                        {
                            ulong value = Read64(expectedData, offset);
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), unchecked(oldSp + 8));
                            if (instruction.Operation != JitOperation.Ret)
                                BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24 + instruction.Destination.Register * 8), value);
                        }
                        ulong next = instruction.Operation == JitOperation.Call ? instruction.Target :
                            instruction.Operation == JitOperation.Ret ? Read64(expectedData, (int)(address - Page)) :
                            Ip + (uint)instruction.Length;
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, next);
                        expectedMachine[8] = 0;
                    }
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), name + " complete machine state");
                    Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), name + " exact memory bytes");
                    Check(frame.NextIp == Read64(expectedMachine, 0), name + " NextIp");
                }
            }
            // A missing descriptor after a committed scalar instruction must
            // leave the store/stack instruction unissued and without effects.
            Reset(machine);
            Write64(machine, 56, Page + (instruction.Operation is JitOperation.Push or JitOperation.Call ? 40UL : 32UL));
            var first = new JitInstruction(Ip, 5, JitOperation.Mov, 32, new(0, 0), new(0, 123, true));
            var second = instruction with { Ip = Ip + 5 };
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit([first, second], 2), out var pair, out string why), why);
            using (pair!)
            {
                byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, Ip + 5); expectedMachine[8] = 0;
                BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24), 123);
                long issued = 0, completed = 0; int stop = 0;
                JitFrame frame = new() { DataGeneration = 1, Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame), Machine = machine,
                    Issued = &issued, Completed = &completed, Stop = &stop, Budget = 100 };
                ((delegate* unmanaged<JitFrame*, int>)(void*)pair!.Entry)(&frame);
                Check(frame.Exit == JitExit.MemoryMiss && issued == 1 && completed == 1 && frame.Executed == 1 &&
                    frame.NextIp == Ip + 5, label + " partial-block miss accounting");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), label + " partial-block full state");
                Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), label + " partial-block memory");
            }
        }
    }

    static ulong Read64(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset));

    static void CheckMemorySequences(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        foreach (bool stack in new[] { false, true })
        {
            JitInstruction[] sequence = stack ?
            [
                new(Ip, 2, JitOperation.Push, 64, default, new(15, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
                new(Ip + 2, 2, JitOperation.Pop, 64, new(4, 0), default, SourceWidth: 64, Address: new(4, -1, 0, 0)),
                new(Ip + 4, 4, JitOperation.StoreMov, 64, default, new(4, 0), SourceWidth: 64, Address: new(4, -1, 0, 0))
            ] :
            [
                new(Ip, 4, JitOperation.StoreMov, 32, default, new(15, 0), SourceWidth: 32, Address: new(3, -1, 0, 0)),
                new(Ip + 4, 4, JitOperation.LoadMov, 64, new(4, 0), default, SourceWidth: 64, Address: new(3, -1, 0, 0)),
                new(Ip + 8, 3, JitOperation.Mov, 64, new(0, 0), new(4, 0))
            ];
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(sequence, 3), out var allocation, out string reason), reason);
            using (allocation!)
            {
                foreach (int budget in new[] { 0, 1, 2, 3 })
                {
                    Reset(machine);
                    Write64(machine, 56, Page + 128);
                    Write64(machine, 144, stack ? Page + 160 : 0x8877665544332211);
                    for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                    byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                    if (budget >= 1)
                    {
                        if (stack)
                        {
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(120), Page + 160);
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), Page + 120);
                        }
                        else BinaryPrimitives.WriteUInt32LittleEndian(expectedData.AsSpan(64), 0x44332211);
                    }
                    if (budget >= 2)
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), stack ? Page + 160 : Read64(expectedData, 64));
                    if (budget >= 3)
                    {
                        if (stack) BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(160), Page + 160);
                        else BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24), Read64(expectedData, 64));
                    }
                    ulong next = budget == 3 ? sequence[2].Ip + (uint)sequence[2].Length : sequence[budget].Ip;
                    BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, next);
                    if (budget > 0) expectedMachine[8] = 0;
                    descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = 1,
                        ValidEnd = 4096, Permissions = 3 };
                    long issued = 0, completed = 0; int stop = 0;
                    JitFrame frame = new() { DataGeneration = 1, Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame), Machine = machine,
                        Issued = &issued, Completed = &completed, Stop = &stop, Budget = (ulong)budget,
                        DataDescriptors = descriptors, DataDescriptorCount = 1 };
                    int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry)(&frame);
                    JitExit exit = budget == 3 ? JitExit.Continue : JitExit.Budget;
                    string name = (stack ? "push/pop-rsp/store" : "store32/load64/copy-rsp") + "/budget-" + budget;
                    Check(returned == (int)exit && frame.Exit == exit && frame.NextIp == next, name + " exit");
                    Check(issued == (budget == 3 ? 3 : budget + 1) && completed == budget && frame.Executed == (ulong)budget,
                        name + " exact issue/completion");
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), name + " complete machine state");
                    Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), name + " entire data page");
                }
            }
        }
    }

    static void CheckMemoryFlags(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (var op in new[] { JitOperation.MemoryCmp, JitOperation.MemoryCmpReverse, JitOperation.MemoryTest })
        foreach (bool immediate in new[] { false, true })
        {
            if (immediate && op == JitOperation.MemoryCmpReverse) continue;
            var operand = new JitOperand(15, 0x91, immediate);
            var instruction = new JitInstruction(Ip, 4, op, width, operand, operand,
                SourceWidth: width, Address: new(3, 12, 3, -48));
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit([instruction], 1), out var allocation, out string reason), reason);
            using (allocation!)
            foreach (string kind in new[] { "hit", "epoch-hit", "epoch-zero", "generation", "read", "extent", "cross-page", "budget", "stop", "attention" })
            {
                Reset(machine);
                for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                uint generation = kind == "epoch-hit" ? 0xf1234567u : 1u;
                descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = generation,
                    ValidEnd = 4096, Permissions = 1 };
                if (kind == "epoch-zero") generation = 0;
                if (kind == "generation") descriptors[0].Generation = 2;
                if (kind == "read") descriptors[0].Permissions = 2;
                if (kind == "extent") descriptors[0].ValidEnd = (uint)(31 + width / 8);
                if (kind == "cross-page") Write64(machine, 48, Page + 4096 + 32);
                if (kind == "attention") machine[11] = 1;
                byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                byte[] memoryBefore = new ReadOnlySpan<byte>(data, 4096).ToArray();
                long issued = 0, completed = 0; int stop = kind == "stop" ? 1 : 0;
                JitFrame frame = new() { DataGeneration = generation, Machine = machine, Issued = &issued,
                    Completed = &completed, Stop = &stop, Budget = kind == "budget" ? 0UL : 100UL,
                    DataDescriptors = descriptors, DataDescriptorCount = 1 };
                int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry)(&frame);
                bool hit = kind is "hit" or "epoch-hit";
                var expected = hit ? JitExit.Continue : kind == "budget" ? JitExit.Budget :
                    kind == "stop" ? JitExit.Stop : kind == "attention" ? JitExit.Attention : JitExit.MemoryMiss;
                string label = op + "/" + width + "/" + immediate + "/" + kind;
                Check(returned == (int)expected && frame.Exit == expected, label + " exit");
                Check(issued == (hit || kind == "budget" ? 1 : 0) && completed == (hit ? 1 : 0) &&
                    frame.Executed == (hit ? 1UL : 0UL), label + " counts");
                if (hit)
                {
                    ulong memory = width switch { 8 => data[32], 16 => BinaryPrimitives.ReadUInt16LittleEndian(new(data + 32, 2)),
                        32 => BinaryPrimitives.ReadUInt32LittleEndian(new(data + 32, 4)), _ => BinaryPrimitives.ReadUInt64LittleEndian(new(data + 32, 8)) };
                    ulong register = immediate ? operand.Immediate : BinaryPrimitives.ReadUInt64LittleEndian(before.AsSpan(144));
                    ulong left = op == JitOperation.MemoryCmpReverse ? register : memory;
                    ulong right = op == JitOperation.MemoryCmpReverse ? memory : register;
                    ulong mask = width == 64 ? ulong.MaxValue : (1UL << width) - 1;
                    left &= mask; right &= mask;
                    bool test = op == JitOperation.MemoryTest;
                    ulong result = (test ? left & right : unchecked(left - right)) & mask;
                    ulong sign = 1UL << (width - 1);
                    uint status = ((result & sign) != 0 ? 0x80u : 0) | (result == 0 ? 0x40u : 0);
                    if (!test)
                    {
                        if (left < right) status |= 1;
                        if (((left ^ right ^ result) & 16) != 0) status |= 16;
                        if (((left ^ right) & (left ^ result) & sign) != 0) status |= 0x800;
                    }
                    uint old = BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12));
                    uint flags = (old & 0x00fff72e) | status | ((uint)(byte)result << 24);
                    BinaryPrimitives.WriteUInt32LittleEndian(before.AsSpan(12), flags);
                    BinaryPrimitives.WriteUInt64LittleEndian(before, Ip + 4); before[8] = 0;
                }
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before), label + " whole state");
                Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(memoryBefore), label + " no memory write");
                Check(frame.NextIp == (hit ? Ip + 4 : Ip), label + " IP");
            }
        }
    }

    static void CheckConditionalTraces(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        foreach (int backedge in new[] { 0, 2, 4 })
        {
            JitInstruction[] trace = [
                new(Ip, 4, JitOperation.LoadMov, 64, new(0, 0), default, SourceWidth: 64, Address: new(3, -1, 0, -32)),
                // Target is a later label in this trace. Taking it must leave
                // before the MOV rather than skip it inside the native frame.
                new(Ip + 4, 4, JitOperation.ConditionalJump, 64, default, default, Target: Ip + 12, Condition: 4),
                new(Ip + 8, 4, JitOperation.Mov, 64, new(2, 0), new(0, 42, true)),
                new(Ip + 12, 4, JitOperation.StoreMov, 64, default, new(2, 0), SourceWidth: 64, Address: new(3, -1, 0, -24)),
                new(Ip + 16, 4, JitOperation.ConditionalJump, 64, default, default, Target: Ip + (uint)backedge * 4, Condition: 5)
            ];
            byte[] counts = JitMemoryCounts.Build(trace);
            foreach (int cap in new[] { 1, 2, 3, 31, 32 })
            {
                Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(trace, cap), out var allocation, out string reason), reason);
                using (allocation!)
                foreach (bool earlyTaken in new[] { false, true })
                foreach (ulong budget in Enumerable.Range(0, 34).Select(x => (ulong)x))
                {
                    Reset(machine);
                    if (!earlyTaken) BinaryPrimitives.WriteUInt32LittleEndian(new(machine + 12, 4), 0xab000895);
                    for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                    descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = 1, ValidEnd = 4096, Permissions = 3 };
                    byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                    int executed = 0, memory = 0, index = 0; bool escaped = false;
                    while (executed < cap && (ulong)executed < budget)
                    {
                        if (index == 0)
                        {
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24), Read64(expectedData, 32));
                            ++memory; ++index;
                        }
                        else if (index == 1)
                        {
                            if (earlyTaken) { index = 3; escaped = true; }
                            else ++index;
                        }
                        else if (index == 2)
                        { BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(40), 42); ++index; }
                        else if (index == 3)
                        { BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(40), 42); ++memory; ++index; }
                        else index = backedge;
                        ++executed;
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, Ip + (uint)index * 4);
                        expectedMachine[8] = 0;
                        if (escaped) break;
                    }
                    JitExit expectedExit = escaped || executed == cap ? JitExit.Continue : JitExit.Budget;
                    long issued = 0, completed = 0; int stop = 0;
                    JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                        Budget = budget, DataGeneration = 1, DataDescriptors = descriptors, DataDescriptorCount = 1 };
                    int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry)(&frame);
                    string label = "trace/" + backedge + "/" + cap + "/" + earlyTaken + "/" + budget;
                    Check(returned == (int)expectedExit && frame.Exit == expectedExit, label + " exit");
                    Check(issued == executed + (expectedExit == JitExit.Budget ? 1 : 0) && completed == executed &&
                        frame.Executed == (ulong)executed, label + " exact counts");
                    Check(counts[executed] == memory, label + " exact memory prefix");
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), label + " whole machine");
                    Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), label + " whole RAM");
                    Check(frame.NextIp == Ip + (uint)index * 4, label + " NextIp");
                }
            }
            // A write guard miss after fall-through retains the prior load,
            // branch and MOV, without issuing or performing the store.
            Reset(machine); BinaryPrimitives.WriteUInt32LittleEndian(new(machine + 12, 4), 0xab000895);
            descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = 1, ValidEnd = 4096, Permissions = 1 };
            Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(trace), out var miss, out string why), why);
            using (miss!)
            {
                byte[] before = new ReadOnlySpan<byte>(data, 4096).ToArray();
                long issued = 0, completed = 0; int stop = 0;
                JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                    Budget = 100, DataGeneration = 1, DataDescriptors = descriptors, DataDescriptorCount = 1 };
                ((delegate* unmanaged<JitFrame*, int>)(void*)miss!.Entry)(&frame);
                Check(frame.Exit == JitExit.MemoryMiss && frame.Executed == 3 && issued == 3 && completed == 3 &&
                    frame.NextIp == Ip + 12 && Read64(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray(), 40) == 42,
                    "trace partial write miss state/count");
                Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(before), "trace partial write miss no store");
            }
        }
    }

    static void CheckCallReturnTraces(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        foreach (bool call in new[] { true, false })
        foreach (int length in new[] { 1, 2, 31, 32 })
        {
            var trace = new List<JitInstruction>();
            for (int i = 0; i < length - 1; ++i)
                trace.Add(i % 2 == 0 ?
                    new(Ip + (uint)i * 4, 4, JitOperation.LoadMov, 64, new(0, 0), default,
                        SourceWidth: 64, Address: new(3, -1, 0, 0)) :
                    new(Ip + (uint)i * 4, 4, JitOperation.StoreMov, 64, default, new(15, 0),
                        SourceWidth: 64, Address: new(3, -1, 0, 32)));
            var terminal = new JitInstruction(Ip + (uint)(length - 1) * 4, call ? 5 : 1,
                call ? JitOperation.Call : JitOperation.Ret, 64, default, default, Target: Ip,
                SourceWidth: 64, Address: new(4, -1, 0, call ? -8 : 0));
            trace.Add(terminal);
            byte[] counts = JitMemoryCounts.Build(trace.ToArray());
            for (int n = 0; n <= length; ++n)
                Check(counts[n] == n, "call/return exact terminal memory prefix");
            if (length < 32) Check(counts[length + 1] == 0, "call/return memory prefix terminates");
            foreach (int cap in new[] { 1, 31, 32 })
            {
                Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(trace, cap), out var allocation, out string reason), reason);
                using (allocation!)
                foreach (bool miss in new[] { false, true })
                foreach (int budget in Enumerable.Range(0, 34))
                {
                    Reset(machine);
                    for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                    Write64(machine, 56, Page + (call ? 264UL : 256UL));
                    Write64(data, 256, Ip); // RET to an existing label must dispatch externally.
                    descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = 1,
                        ValidEnd = miss ? 128u : 4096u, Permissions = 3 };
                    byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                    byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                    int executed = 0; long expectedIssued = 11;
                    JitExit expectedExit = JitExit.Continue;
                    while (executed < cap && executed < length)
                    {
                        bool last = executed == length - 1;
                        if (last && miss) { expectedExit = JitExit.MemoryMiss; break; }
                        if (expectedIssued++ >= 11 + budget) { expectedExit = JitExit.Budget; break; }
                        var instruction = trace[executed];
                        ulong next = instruction.Ip + (uint)instruction.Length;
                        if (last)
                        {
                            if (call) BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(256), next);
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), Page + (call ? 256UL : 264UL));
                            next = Ip;
                        }
                        else if (executed % 2 == 0)
                            BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24), Read64(expectedData, 64));
                        else BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan(96), Read64(expectedMachine, 144));
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, next); expectedMachine[8] = 0;
                        ++executed;
                        if (last) break;
                    }
                    long issued = 11, completed = 7; int stop = 0;
                    JitFrame frame = new() { Version = JitFrame.AbiVersion, Size = (uint)sizeof(JitFrame),
                        Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop, Budget = (ulong)(11 + budget),
                        DataGeneration = 1, DataDescriptors = descriptors, DataDescriptorCount = 1, NextIp = ulong.MaxValue };
                    int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocation!.Entry)(&frame);
                    string label = terminal.Operation + "/trace-" + length + "/cap-" + cap + "/miss-" + miss + "/budget-" + budget;
                    Check(returned == (int)expectedExit && frame.Exit == expectedExit, label + " exit");
                    Check(issued == expectedIssued && completed == 7 + executed && frame.Executed == (ulong)executed,
                        label + " exact issue/completion");
                    Check(counts[executed] == executed, label + " exact memory count");
                    Check(frame.NextIp == Read64(expectedMachine, 0), label + " dynamic NextIp");
                    Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), label + " full machine");
                    Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), label + " entire RAM page");
                }
            }
            if (length == 1)
            {
                var suffix = new JitInstruction(Ip + 5, 1, JitOperation.Nop, 32, default, default);
                foreach (var invalid in new[] { terminal with { Width = 32 }, terminal with { SourceWidth = 32 },
                    terminal with { Length = call ? 4 : 2 }, terminal with { Address = new(0, -1, 0, 0) } })
                {
                    bool rejected = false;
                    try { X64BlockEmitter.Emit([invalid]); } catch (ArgumentException) { rejected = true; }
                    Check(rejected, "invalid call/return IR rejected");
                }
                bool nonterminal = false;
                try { X64BlockEmitter.Emit([terminal, suffix]); } catch (ArgumentException) { nonterminal = true; }
                Check(nonterminal, "nonterminal call/return IR rejected");
            }
        }
    }

    static void CheckNestedCallReturns(byte* machine, byte* data, JitDataDescriptor* descriptors)
    {
        // Finite guest path with two nested calls. Each dispatcher re-entry is
        // explicit here; a native CALL or internal call/return loop would change
        // the entry count, stack bytes or the exact per-boundary reference state.
        var blocks = new Dictionary<ulong, JitInstruction[]>();
        const ulong outer = Ip + 0x400, inner = Ip + 0x800, done = Ip + 0xc00;
        JitInstruction[] Body(ulong start, JitInstruction terminal)
        {
            var body = Enumerable.Range(0, 30).Select(i => new JitInstruction(start + (uint)i * 10, 10,
                JitOperation.Mov, 64, new(i % 3, 0), new(0, (ulong)i + start, true))).ToList();
            body.Add(terminal); return body.ToArray();
        }
        blocks[Ip] = Body(Ip, new(Ip + 300, 5, JitOperation.Call, 64, default, default, Target: outer,
            SourceWidth: 64, Address: new(4, -1, 0, -8)));
        blocks[outer] = Body(outer, new(outer + 300, 5, JitOperation.Call, 64, default, default, Target: inner,
            SourceWidth: 64, Address: new(4, -1, 0, -8)));
        blocks[inner] = Body(inner, new(inner + 300, 1, JitOperation.Ret, 64, default, default,
            SourceWidth: 64, Address: new(4, -1, 0, 0)));
        blocks[outer + 305] = [
            new(outer + 305, 10, JitOperation.Mov, 64, new(8, 0), new(0, 0x123456789abcdef0, true)),
            new(outer + 315, 10, JitOperation.Mov, 64, new(9, 0), new(0, 0xfedcba9876543210, true)),
            new(outer + 325, 10, JitOperation.Mov, 64, new(15, 0), new(0, 0x8877665544332211, true)),
            new(outer + 335, 1, JitOperation.Ret, 64, default, default, SourceWidth: 64, Address: new(4, -1, 0, 0)) ];
        blocks[Ip + 305] = Body(Ip + 305, new(Ip + 605, 5, JitOperation.Jump, 64, default, default, Target: done));
        blocks[done] = [new(done, 10, JitOperation.Mov, 64, new(12, 0), new(0, 123, true)),
            new(done + 10, 10, JitOperation.Mov, 64, new(13, 0), new(0, 456, true))];
        var byIp = blocks.Values.SelectMany(x => x).ToDictionary(x => x.Ip);
        var allocations = new Dictionary<ulong, NativeCodeAllocation>();
        try
        {
            foreach (var block in blocks.Values)
            {
                Check(NativeCodeAllocation.TryCreate(X64BlockEmitter.Emit(block), out var allocation, out string reason), reason);
                allocations[block[0].Ip] = allocation!;
            }
            for (int budget = 1; budget <= 128; ++budget)
            {
                Reset(machine); Write64(machine, 56, Page + 512);
                for (int i = 0; i < 4096; ++i) data[i] = (byte)(i * 37 + 0x91);
                byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                byte[] expectedData = new ReadOnlySpan<byte>(data, 4096).ToArray();
                int expectedMemory = 0;
                for (int executed = 0; executed < budget; ++executed)
                {
                    var instruction = byIp[Read64(expectedMachine, 0)];
                    ulong next = instruction.Ip + (uint)instruction.Length;
                    if (instruction.Operation == JitOperation.Mov)
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(24 + instruction.Destination.Register * 8), instruction.Source.Immediate);
                    else if (instruction.Operation == JitOperation.Call)
                    {
                        ulong sp = Read64(expectedMachine, 56) - 8;
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedData.AsSpan((int)(sp - Page)), next);
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), sp);
                        next = instruction.Target; ++expectedMemory;
                    }
                    else if (instruction.Operation == JitOperation.Ret)
                    {
                        ulong sp = Read64(expectedMachine, 56);
                        next = Read64(expectedData, (int)(sp - Page));
                        BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine.AsSpan(56), sp + 8); ++expectedMemory;
                    }
                    else next = instruction.Target;
                    BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, next); expectedMachine[8] = 0;
                }
                descriptors[0] = new() { Page = Page, Host = (nint)data, Identity = 1, Generation = 1, ValidEnd = 4096, Permissions = 3 };
                long issued = 0, completed = 0; int stop = 0, nativeMemory = 0, entries = 0;
                JitFrame frame = new() { Machine = machine, Issued = &issued, Completed = &completed, Stop = &stop,
                    Budget = (ulong)budget, DataGeneration = 1, DataDescriptors = descriptors, DataDescriptorCount = 1 };
                do
                {
                    Check(++entries <= 8, "nested calls always return to a bounded dispatcher");
                    ulong start = *(ulong*)machine;
                    byte[] counts = JitMemoryCounts.Build(blocks[start]);
                    frame.NextIp = ulong.MaxValue;
                    int returned = ((delegate* unmanaged<JitFrame*, int>)(void*)allocations[start].Entry)(&frame);
                    Check(returned == (int)frame.Exit && frame.Exit is JitExit.Continue or JitExit.Budget, "nested dispatcher exit");
                    Check(frame.NextIp == *(ulong*)machine && frame.Executed <= 32, "nested dynamic NextIp and cap");
                    nativeMemory += counts[(int)frame.Executed];
                } while (frame.Exit == JitExit.Continue);
                string label = "nested call/return prefix-" + budget;
                Check(issued == budget + 1 && completed == budget, label + " exact counters");
                Check(nativeMemory == expectedMemory, label + " exact memory prefix");
                Check(new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine), label + " full machine");
                Check(new ReadOnlySpan<byte>(data, 4096).SequenceEqual(expectedData), label + " entire RAM page");
            }
        }
        finally { foreach (var allocation in allocations.Values) allocation.Dispose(); }
    }

    static void Reset(byte* machine)
    {
        new Span<byte>(machine, sizeof(Blink.Machine)).Clear();
        Write64(machine, 0, Ip); machine[8] = 7;
        BinaryPrimitives.WriteUInt32LittleEndian(new(machine + 12, 4), 0xab0008d5);
        for (int reg = 0; reg < 16; ++reg) Write64(machine, 24 + reg * 8, 0xfeedface12340000UL + (ulong)reg);
        Write64(machine, 24 + 3 * 8, Page + 64); Write64(machine, 24 + 12 * 8, 2);
    }
    static void Write64(byte* pointer, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(new(pointer + offset, 8), value);
}
