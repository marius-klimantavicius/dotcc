using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class FullTranslationJit
    {
        internal (bool Enabled, bool CatalogAllocated, long Charge) ActivationSnapshot()
        {
            lock (Authority.Gate) return (canonicalX87Enabled, x87Kernels != null, Store.ChargedBytes);
        }
    }

    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal sealed partial class FullControllerFixture
        {
            internal void ResetFaultObservation()
            { testWorkers[0].FirstTrap = null; testWorkers[0].Signal = 0; }
            internal void TryLegacyX87Helper(byte[] bytes)
            {
                Blink.XedDecodedInst decoded = default;
                fixed (byte* p = bytes)
                    Require(Program.DecodeInstruction(&decoded, p, (ulong)bytes.Length, Blink.XED_MODE_LONG) == 0,
                        "R179 helper defense exact authoritative decode");
                var helper = Program.BlinkFullJitSelect(decoded.op.rde, decoded.op.disp, decoded.op.uimm0);
                Owner.ExecuteFullHelper(testWorkers[0], new(Base, decoded, helper));
            }
        }
    }
}

unsafe partial class Program
{
    static void CheckCanonicalX87ActivationReadiness()
    {
        FullCanonicalX87ActivationBlocker obligations = FullExtended80TranscendentalSoftwareProfile.Qualified ?
            FullCanonicalX87ActivationBlocker.None : FullCanonicalX87ActivationBlocker.NumericalAcceptanceDomainUnqualified;
        foreach (bool owner in new[] { false, true })
        foreach (bool linux in new[] { false, true })
        {
            var expected = obligations |
                (owner ? FullCanonicalX87ActivationBlocker.None : FullCanonicalX87ActivationBlocker.StateOwnerNotBuilt) |
                (linux ? FullCanonicalX87ActivationBlocker.None : FullCanonicalX87ActivationBlocker.PlatformUnverified);
            Check(FullCanonicalX87Activation.Evaluate(owner, linux) == expected,
                "R179 platform/build inputs cannot substitute for numerical/context qualification");
        }
        Check(FullExtended80TranscendentalProtocol.GuestActivationReady == (FullCanonicalX87Activation.Blockers == FullCanonicalX87ActivationBlocker.None),
            "production activation requires the exact accepted numerical profile plus owner/platform gates");
        Check(FullCanonicalX87Activation.PrecisionControlPolicy.Contains("PC01=64SoftwareProfile") &&
            FullCanonicalX87Activation.Environment16Policy.Contains("OmittedFopZero"),
            "R179r1 PC01 software PC64 and protected16 software FOP0 remain explicit");

        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        var before = fixture.Jit.ActivationSnapshot();
        byte[] raw = new ReadOnlySpan<byte>(fixture.CanonicalFile(), 192).ToArray();
        if (FullCanonicalX87Activation.Blockers != FullCanonicalX87ActivationBlocker.None)
        {
        for (int attempt = 0; attempt < 2; ++attempt)
        {
            var blockers = fixture.Jit.PrepareCanonicalX87Production(true);
            Check(blockers == FullCanonicalX87Activation.Blockers && fixture.Jit.ActivationSnapshot() == before &&
                !before.Enabled && !before.CatalogAllocated,
                "R179 actual owner startup denial performs no eager catalog emission/publication/charge");
            bool denied = false;
            try
            {
                var file = fixture.CanonicalFile();
                FullX87StateStorage.ActivateFresh(file, (nint)fixture.Machine(), file->OwnerGeneration, file->OwnerToken);
            }
            catch (NotSupportedException error)
            {
                denied = error.Message.Contains("NumericalAcceptanceDomainUnqualified") &&
                    error.Message.Contains(FullExtended80TranscendentalSoftwareProfile.Id);
            }
            Check(denied && new ReadOnlySpan<byte>(fixture.CanonicalFile(), 192).SequenceEqual(raw),
                "R179 exact production state gate denies before writing ProfileActive or importing shadow values");
        }
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc); fixture.Fill(pc);
        WriteControllerCode(fixture, pc, [0x90, 0xeb, 0xfe]);
        fixture.Counts(100);
        fixture.NativePrepared = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
        Check(fixture.Step(pc) && fixture.Completed == 1 && fixture.Statistics.SemanticHelperInstructions == 0 &&
            !fixture.Jit.ActivationSnapshot().CatalogAllocated && fixture.CanonicalFile()->ProfileActive == 0,
            "R179 no-x87 workload keeps catalog absent and executes native code with default inactive state");
        }
        else
        {
            Check(fixture.Jit.PrepareCanonicalX87Production(true) == FullCanonicalX87ActivationBlocker.None,
                "qualified production startup admits the exact portable numerical owner");
            var after = fixture.Jit.ActivationSnapshot();
            Check(after.Enabled && after.CatalogAllocated && after.Charge > before.Charge,
                "production activation owns and charges all immutable catalog bindings before publication");
            var file = fixture.CanonicalFile();
            FullX87StateStorage.ActivateFresh(file, (nint)fixture.Machine(), file->OwnerGeneration, file->OwnerToken);
            Check(file->ProfileActive == 1, "fresh production canonical state activates without importing binary64 shadow values");
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(pc); fixture.Fill(pc);
            fixture.Machine()->fpu.tw = -1; fixture.Machine()->fpu.sw = 0; fixture.Machine()->fpu.cw = 0x37f;
            WriteControllerCode(fixture, pc, [0xd9, 0xe8, 0xeb, 0xfe]); fixture.Counts(100);
            fixture.NativePrepared = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
            Check(fixture.Step(pc) && fixture.Completed == 1 && fixture.Statistics.SemanticHelperInstructions == 0 &&
                fixture.Statistics.InterpreterAttempts == 0 && FullX87RegisterFile.Physical(file,7)->Significand == 0x8000000000000000UL &&
                FullX87RegisterFile.Physical(file,7)->SignExponent == 0x3fff,
                "real production admission compiles and executes canonical FLD1 without test-only startup");
        }
        bool late = false;
        try { fixture.Jit.PrepareCanonicalX87Production(true); }
        catch (InvalidOperationException) { late = true; }
        Check(late, "R179 no late state adoption/catalog startup after body publication");
        RecordQualificationController(fixture.Statistics);
    }

    // Independent selector masks retained from the c117 source audit. The 201
    // remaining selectors partition into 170 selected-UD and 31 unadopted
    // reserved comparisons. Ordinary and the 54 adopted aliases remain intact.
    static IEnumerable<byte[]> CanonicalX87R179ReservedForms()
    {
        for (int op = 0xd8; op <= 0xdf; ++op)
        for (int m = 0; m <= 255; ++m)
        {
            bool ordinary = m >= 0xc0 ? CanonicalX87R174OrdinaryRegister((byte)op, (byte)m) :
                CanonicalX87R174OrdinaryMemory((byte)op, m >> 3 & 7);
            if (!ordinary && !(m >= 0xc0 && CanonicalX87R174Alias((byte)op, (byte)m)))
                yield return CanonicalX87R174Tuple((byte)op, (byte)m);
        }
    }

    static void CheckCanonicalX87InvalidController()
    {
        var reserved = CanonicalX87R179ReservedForms().ToArray();
        Check(reserved.Length == 201, "R179 exact reserved selector complement, not blanket failed-classification UD");
        foreach (byte[] bytes in reserved)
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0xf3], [0x48, 0x67] })
        {
            byte[] original = [..prefix, ..bytes];
            Check(FullTranslationJit.TryClassifyNative(Ip, original, out var i) &&
                FullBlockEmitter.ValidateCanonicalX87Invalid(i) && i.Length == original.Length &&
                i.Options == FullCanonicalX87InvalidLowering.ReservedUd,
                "R179 exact reserved selector/prefix composition has authenticated native UD IR");
            Check(!FullTranslationJit.TryClassifyNative(Ip, original[..^1], out _),
                "R179 truncated reserved bytes cannot gain a UD plan");
        }
        foreach (byte[] locked in new byte[][] { [0xf0, 0xd8, 0xc1], [0xf0, 0xd9, 0x03], [0xf0, 0x9b] })
            Check(FullCanonicalX87InvalidLowering.TryClassify(Ip, locked, out var lockedIr) &&
                lockedIr.Options == FullCanonicalX87InvalidLowering.LockUd && FullBlockEmitter.ValidateCanonicalX87Invalid(lockedIr),
                "R179 LOCK x87 is explicitly UD; demand decode may deliver it before native classification");
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity], common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false, true })
        foreach (bool shared in new[] { false, true })
        {
            FullBlockEmitter.EmitScaffold(common, out var layout, windows);
            var scaffold = layout.Resolve((nint)0x10000000);
            foreach (ulong address in new[] { Ip, 0x7fff12345678UL })
            {
                Check(FullTranslationJit.TryClassifyNative(address, [0xd9, 0xe2], out var i), "R179 UD exact source form");
                FullBlockInstruction[] body = [new(i)];
                Check(shared ? FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows) :
                    FullBlockEmitter.TryEmit(body, staging, out _, windows),
                    "R179 UD both ABI/scaffold byte emission only; Windows execution remains unverified");
                foreach (var malformed in new[] { i with { SourceWidth = 8 }, i with { Width = 64 },
                    i with { Extra = new(0, 0xc1, IsImmediate: true) }, i with { Destination = new(1, 0) } })
                {
                    bool denied = false;
                    try { FullBlockEmitter.ValidateCanonicalX87Invalid(malformed); }
                    catch (ArgumentException) { denied = true; }
                    Check(denied, "R179 malformed/ordinary-selector UD IR cannot borrow native fault admission");
                }
            }
        }
        Check(!FullCanonicalX87InvalidLowering.TryClassify(Ip, [0xd8, 0xc1], out _) &&
            !FullCanonicalX87InvalidLowering.TryClassify(Ip, [0xd9, 0xfe], out _),
            "R179 ordinary arithmetic and transcendental computation cannot become reserved UD");

        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc); fixture.Fill(pc); fixture.StartCanonicalFamily();
        try
        {
            int ordinal = 0;
            foreach (byte[] bytes in reserved)
            {
                var m = fixture.Machine(); var file = fixture.CanonicalFile();
                m->fpu.cw = 0x37e; m->fpu.sw = 0x8081; m->fpu.tw = 0xaaaa;
                m->fpu.op = 0x345; m->fpu.ip = 0x12345678; m->fpu.dp = 0x87654321;
                for (int r = 0; r < 8; ++r)
                    *FullX87RegisterFile.Physical(file, r) = new(0x8000000000000001UL + (uint)r, (ushort)(0x3fff + r));
                // All memory selector bases point into noncanonical/unmapped
                // data. UD must not touch data or first deliver the pending MF.
                for (int r = 0; r < 16; ++r) Write64((byte*)m, 24 + r * 8, 0x0000800000000000UL);
                byte[] raw = new ReadOnlySpan<byte>(file, 192).ToArray();
                byte[] metadata = new ReadOnlySpan<byte>(&m->fpu, sizeof(Blink.MachineFpu)).ToArray();
                ulong at = pc + (uint)(ordinal++ * 16);
                WriteControllerCode(fixture, at, [..bytes, 0xeb, 0xfe]);
                fixture.Counts(100);
                fixture.ResetFaultObservation();
                var fault = fixture.StepSynchronousFault(at);
                Check(fault.Signal == 4 && fault.Detail != null && fault.Detail.Contains("signal=4 code=2") && m->trapno == 6 &&
                    m->ip == at && m->faultaddr == (long)at && fixture.Issued == 1 && fixture.Completed == 0 &&
                    fixture.Jit.Store.ActiveNativeEpochs == 0 &&
                    new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw) &&
                    new ReadOnlySpan<byte>(&m->fpu, sizeof(Blink.MachineFpu)).SequenceEqual(metadata) &&
                    fixture.Statistics.SemanticHelperInstructions == 0 && fixture.Statistics.InterpreterAttempts == 0,
                    $"R179 reserved native UD: ordinal={ordinal} bytes={Convert.ToHexString(bytes)} signal={fault.Signal} detail={fault.Detail} trap={m->trapno}/{Blink.kMachineUndefinedInstruction} ip={m->ip:x}/{at:x} address={m->faultaddr:x} issue={fixture.Issued} completed={fixture.Completed} epochs={fixture.Jit.Store.ActiveNativeEpochs} rawEqual={new ReadOnlySpan<byte>(file,192).SequenceEqual(raw)} metaEqual={new ReadOnlySpan<byte>(&m->fpu,sizeof(Blink.MachineFpu)).SequenceEqual(metadata)} helpers={fixture.Statistics.SemanticHelperInstructions} interpreter={fixture.Statistics.InterpreterAttempts}");
            }
            // Resume ordinary native work after the fixture intercepts SIGILL.
            // FullX87SignalTransportTests separately exercise actual Linux
            // handler-frame save/reset/sigreturn for every TOP/raw80 category.
            var machine = fixture.Machine(); machine->fpu.cw = 0x37f; machine->fpu.sw = 0; machine->fpu.tw = 0xffff;
            WriteControllerCode(fixture, pc + 4080, [0xd9, 0xe8, 0xeb, 0xfe]);
            fixture.Counts(100);
            fixture.NativePrepared = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
            Check(fixture.Step(pc + 4080) && fixture.Completed == 1 &&
                FullX87RegisterFile.Physical(fixture.CanonicalFile(), 7)->Significand == 0x8000000000000000 &&
                fixture.CanonicalFile()->ProfileActive == 1,
                "R179 returned fault does not retire canonical ownership or prevent native recovery");
            RecordQualificationController(fixture.Statistics);
        }
        finally { fixture.NativePrepared = null; fixture.EndCanonicalFamily(); }
    }

    static void CheckCanonicalX87NoFallback()
    {
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc); fixture.Fill(pc);
        WriteControllerCode(fixture, pc, [0xd8, 0xc1, 0xeb, 0xfe]);
        fixture.StartCanonicalFamily();
        try
        {
            foreach (byte[] bytes in new byte[][] { [0xd8, 0xc1], [0xd9, 0xfe], [0x9b], [0x48, 0x0f, 0xae, 0x03] })
            {
                byte[] raw = new ReadOnlySpan<byte>(fixture.CanonicalFile(), 192).ToArray();
                bool denied = false;
                try { fixture.TryLegacyX87Helper(bytes); }
                catch (InvalidOperationException error) { denied = error.Message.Contains("managed computation helper"); }
                Check(denied && fixture.Statistics.SemanticHelperInstructions == 0 &&
                    new ReadOnlySpan<byte>(fixture.CanonicalFile(), 192).SequenceEqual(raw),
                    "R179 cached/forged x87 and FX helper consumption fails before calling managed semantics");
            }

        }
        finally { fixture.EndCanonicalFamily(); }
    }
}
