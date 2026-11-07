using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class FullTranslationJit
    {
        internal void FullTestStartCanonicalFamily()
        {
            // Test-only fresh-owner startup; no public activation option. Every
            // literal binding exists before the first body is compiled.
            InitializeX87Kernels(true);
            lock (Authority.Gate)
            {
                if (Store.Publications != 0 || Store.ActiveNativeEpochs != 0 || !x87Kernels!.AdmissionOpen)
                    throw new InvalidOperationException("Canonical family fixture must start unpublished.");
                canonicalX87Enabled = true;
            }
        }
        internal void FullTestEndCanonicalFamily()
        {
            lock (Authority.Gate)
            {
                if (Store.ActiveNativeEpochs != 0) throw new InvalidOperationException("Canonical test native frame remains live.");
                x87KernelClosing = true;
                x87Kernels!.CloseAdmission();
                canonicalX87Enabled = false;
            }
        }
    }
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal sealed partial class FullControllerFixture
        {
            internal void StartCanonicalFamily()
            {
                Jit.FullTestStartCanonicalFamily();
                foreach (var worker in testWorkers)
                {
                    if (worker.X87NativeInFlight || worker.X87Retired || worker.FullNativeState == null)
                        throw new InvalidOperationException("Canonical family worker is not returned and owned.");
                    worker.FullNativeState.X87Pointer->ProfileActive = 1;
                }
            }
            internal void EndCanonicalFamily()
            {
                Jit.FullTestEndCanonicalFamily();
                foreach (var worker in testWorkers)
                {
                    if (worker.X87NativeInFlight) throw new InvalidOperationException("Canonical family return is uncertain.");
                    worker.FullNativeState!.X87Pointer->ProfileActive = 0;
                }
            }
            internal FullX87RegisterFile* CanonicalFile(int worker = 0) => testWorkers[worker].FullNativeState!.X87Pointer;
        }
    }
}

unsafe partial class Program
{
    // One family suite. Independent fixtures are grouped so ordinary assertion
    // failures are collected in one run instead of provoking per-op campaigns.
    static int CheckFullCanonicalX87Family(byte* machine)
    {
        var failures = new List<Exception>();
        (string Name, Action Run)[] groups = [
            ("ownership/catalog", () => { CheckFullX87KernelCatalog(); CheckFullX87KernelOwner(); CheckFullX87OwnedFrame(); CheckFullX87WorkStorage(); }),
            ("stack/register arithmetic", () => { CheckFullCanonicalX87Stack(machine); CheckFullCanonicalX87Arithmetic(machine); }),
            ("memory conversions/computation", () => { CheckFullCanonicalX87Memory(machine); CheckFullCanonicalX87MemoryArithmetic(machine); }),
            ("comparison/control/state", () => CheckFullCanonicalX87Controls(machine)),
            ("all eight transcendental operations", () => CheckFullCanonicalX87PortableFamily(machine)),
            ("real compiler/controller", () => { CheckCanonicalX87ActivationReadiness(); CheckCanonicalX87InvalidController(); CheckCanonicalX87NoFallback(); CheckCanonicalX87PC01Profile(); CheckCanonicalX87SelectedContexts(); CheckCanonicalX87HelperClosure(); CheckCanonicalFamilyController(); CheckCanonicalFamilyFxController(); }),
            ("MMX aliases/transport", () => { CheckFullCanonicalMmx(machine); CheckFullX87SignalTransport(); CheckFullX87Ownership(); }),
            ("permission-only materialization", CheckFullCanonicalX87ProbeMaterialization)
        ];
        foreach (var group in groups)
        {
            int before = checks;
            try { group.Run(); Console.WriteLine($"PASS x87 family {group.Name}; checks={checks - before}"); }
            catch (Exception error) { failures.Add(new InvalidOperationException(group.Name, error)); Console.Error.WriteLine($"FAIL x87 family {group.Name}: {error}"); }
        }
        PrintQualificationCounts("x87 family");
        if (failures.Count != 0) throw new AggregateException("Canonical x87 family batch failed.", failures);
        Console.WriteLine($"PASS coherent canonical x87 family checks={checks}");
        return 0;
    }

    static IEnumerable<(byte Opcode, int Group, int Width)> CanonicalFamilyMemoryRows()
    {
        foreach (byte opcode in new byte[] { 0xd8, 0xdc, 0xda, 0xde })
        for (int group = 0; group < 8; ++group)
            yield return (opcode, group, opcode == 0xdc ? 64 : opcode == 0xde ? 16 : 32);
        yield return (0xdf, 4, 80); yield return (0xdf, 6, 80);
    }
    static void CheckCanonicalFamilyController()
    {
        foreach (var row in CanonicalFamilyMemoryRows())
        {
            using var fixture = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            ulong data = pc + 8191;
            fixture.Map(pc, 12288); fixture.Fill(pc, 12288);
            byte[] bytes = [row.Opcode, (byte)(row.Group * 8 + 3), 0xeb, 0xfe];
            WriteControllerCode(fixture, pc, bytes);
            fixture.DataScope(0, () =>
            {
                for (int b = 0; b < 10; ++b)
                {
                    byte* at = fixture.Program.LookupAddress2(fixture.Machine(), (long)(data + (uint)b), 0, 0);
                    at[0] = row.Opcode == 0xdf ? (b == 0 ? (byte)0x12 : (byte)0) :
                        row.Opcode == 0xd8 ? (b == 3 ? (byte)0x40 : (byte)0) :
                        row.Opcode == 0xdc ? (b == 7 ? (byte)0x40 : (byte)0) : (b == 0 ? (byte)2 : (byte)0);
                }
            });
            fixture.StartCanonicalFamily();
            try
            {
                var machine = fixture.Machine(); var file = fixture.CanonicalFile();
                machine->fpu.cw = 0x37f; machine->fpu.sw = 0; machine->fpu.tw = 0xfffc;
                *FullX87RegisterFile.Physical(file, 0) = new(0x8000000000000000, 0x3fff);
                Write64((byte*)machine, 48, data);
                fixture.Counts(100);
                Action<nint> cap = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
                fixture.NativePrepared = cap; fixture.NativeReturned = _ => fixture.NativePrepared = cap;
                Check(fixture.Step(pc), "family actual compiler entry");
                for (int retry = 0; fixture.Completed < 1 && retry < 16; ++retry) Check(fixture.Continue(), "family bounded same-issue continuation");
                Check(fixture.Issued == 1 && fixture.Completed == 1 && machine->ip == pc + 2 && machine->oplen == 0 &&
                    fixture.Statistics.SemanticHelperInstructions == 0 && fixture.Statistics.InterpreterAttempts == 0,
                    "all34 memory computation rows execute actual compiled native plans without semantic fallback or reissue");
                if (row.Opcode == 0xdf)
                {
                    if (row.Group == 4)
                    {
                        var value = FullX87RegisterFile.Physical(file, 7);
                        Check(((machine->fpu.sw >> 11) & 7) == 7 && value->Significand == 0xc000000000000000 && value->SignExponent == 0x4002,
                            "actual FBLD pushes exact decimal12 into canonical80");
                    }
                    else
                    {
                        for (int b = 0; b < 10; ++b)
                        {
                            byte* page = (byte*)fixture.Physical((data + (uint)b) & ~4095UL);
                            Check(page[(data + (uint)b) & 4095] == (b == 0 ? 1 : 0), "actual FBSTP stores exact crosspage packed1");
                        }
                        Check(((machine->fpu.sw >> 11) & 7) == 1 && (machine->fpu.tw & 3) == 3, "actual FBSTP pops after checked write");
                    }
                }
                else if (row.Group is 2 or 3)
                    Check((machine->fpu.sw & 0x4700) == 0x100 && ((machine->fpu.sw >> 11) & 7) == (row.Group == 3 ? 1 : 0),
                        "actual memory compare produces less relation and correct pop");
                else
                {
                    var expected = row.Group switch
                    {
                        0 => new FullExtended80(0xc000000000000000, 0x4000),
                        1 or 7 => new FullExtended80(0x8000000000000000, 0x4000),
                        4 => new FullExtended80(0x8000000000000000, 0xbfff),
                        5 => new FullExtended80(0x8000000000000000, 0x3fff),
                        _ => new FullExtended80(0x8000000000000000, 0x3ffe)
                    };
                    var value = FullX87RegisterFile.Physical(file, 0);
                    Check(value->Significand == expected.Significand && value->SignExponent == expected.SignExponent,
                        "actual memory arithmetic and reverse forms produce exact literal result");
                }
            }
            finally { fixture.NativePrepared = null; fixture.NativeReturned = null; fixture.EndCanonicalFamily(); }
        }
        CheckCanonicalFamilyFaults();
    }

    static void CheckCanonicalFamilyFaults()
    {
        foreach (string kind in new[] { "read-missing", "suppressed-readonly", "prior-mf" })
        {
            using var fixture = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base, data = pc + 8192;
            fixture.Map(pc); fixture.Fill(pc);
            bool store = kind == "suppressed-readonly";
            WriteControllerCode(fixture, pc, store ? [0xdf, 0x33, 0xeb, 0xfe] : [0xd8, 0x03, 0xeb, 0xfe]);
            if (store)
            {
                fixture.Map(data); fixture.Fill(data);
                fixture.DataScope(0, () => Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system, (long)data, 4096, 1, false) == 0,
                    "family readonly suppressed destination setup"));
            }
            fixture.StartCanonicalFamily();
            try
            {
                var machine = fixture.Machine(); var file = fixture.CanonicalFile();
                machine->fpu.cw = 0x37e; machine->fpu.sw = kind == "prior-mf" ? 0x8081u : 0x200u; machine->fpu.tw = 0xffff;
                Write64((byte*)machine, 48, data);
                byte[] raw = new ReadOnlySpan<byte>(file, 192).ToArray();
                uint sw = machine->fpu.sw; fixture.Counts(100);
                var fault = fixture.StepSynchronousFault(pc);
                Check(fault.Signal == (kind == "prior-mf" ? 8 : 11) && fixture.Completed == 0 && fixture.Issued == 1 &&
                    machine->ip == pc && machine->fpu.sw == sw && new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw) &&
                    fixture.Statistics.SemanticHelperInstructions == 0,
                    "actual canonical controller transports PF versus priorMF without FP effects or managed semantics");
            }
            finally { fixture.EndCanonicalFamily(); }
        }
    }
}

unsafe partial class Program
{
    static void CheckCanonicalFamilyFxController()
    {
        foreach (bool load in new[] { false, true })
        foreach (string route in new[] { "crosspage", "missing-tail", "readonly-tail", "lazy-tail", "watched-code" })
        {
            using var fixture = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            bool watched = route == "watched-code";
            ulong dataPage = watched ? pc : pc + 8192;
            ulong ea = dataPage + (watched ? 512u : 4096u - 416u);
            fixture.Map(pc); fixture.Fill(pc);
            if (!watched) { fixture.Map(dataPage); fixture.Fill(dataPage); }
            if (!watched && route != "missing-tail") fixture.Map(dataPage + 4096);
            if (!watched && route is "crosspage" or "readonly-tail") fixture.Fill(dataPage + 4096);
            var image = new byte[512]; Array.Fill(image, (byte)0xa5);
            if (load)
            {
                Array.Clear(image); image[0] = 0x7f; image[1] = 3;
                image[24] = 0x80; image[25] = 0x1f;
            }
            fixture.DataScope(0, () =>
            {
                // The defined416 data lies entirely on the first page here.
                // Do not accidentally materialize the tail in the lazy case.
                for (int at = 0; at < (watched || route is "crosspage" or "readonly-tail" ? 512 : 416); ++at)
                {
                    byte* pointer = fixture.Program.LookupAddress2(fixture.Machine(), (long)(ea + (uint)at), 0, 0);
                    pointer[0] = image[at];
                }
                if (route == "readonly-tail")
                    Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
                        (long)(dataPage + 4096), 4096, 1, false) == 0, "family FX readonly tail setup");
            });
            WriteControllerCode(fixture, pc, [0x48, 0x0f, 0xae, (byte)(load ? 0x0b : 0x03), 0xeb, 0xfe]);
            fixture.StartCanonicalFamily();
            try
            {
                var m = fixture.Machine(); var file = fixture.CanonicalFile();
                m->fpu.cw = 0x37f; m->fpu.sw = 0; m->fpu.tw = 0xffff;
                *(uint*)((byte*)m + 688) = 0x1f80; Write64((byte*)m, 48, ea);
                byte[] before = new ReadOnlySpan<byte>(file, FullX87RegisterFile.ByteSize).ToArray();
                fixture.Counts(100);
                Action<nint> cap = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
                FullBlockFrame lastFx=default;FullStackMemoryDescriptor lastDescriptor=default;
                fixture.NativePrepared = cap; fixture.NativeReturned = pointer => {
                    lastFx=*(FullBlockFrame*)pointer;
                    if(lastFx.StackMemory!=null)lastDescriptor=*lastFx.StackMemory;
                    fixture.NativePrepared=cap;
                };
                bool faultExpected = route == "missing-tail" || route == "readonly-tail" && !load;
                if (faultExpected)
                {
                    var fault = fixture.StepSynchronousFault(pc);
                    Check(fault.Signal == 11 && fixture.Issued == 1 && fixture.Completed == 0 &&
                        m->fpu.cw == 0x37f && m->fpu.sw == 0 && m->fpu.tw == 0xffff &&
                        new ReadOnlySpan<byte>(file, FullX87RegisterFile.ByteSize).SequenceEqual(before),
                        "actual FX512 permission fault precedes all state/data effects");
                }
                else
                {
                    Check(fixture.Step(pc), "family FX actual controller entry");
                    for (int retry = 0; fixture.Completed < 1 && retry < 24; ++retry)
                        Check(fixture.Continue(), "family FX512 proof/actual416 same-issue resume");
                    Check(fixture.Issued == 1 && fixture.Completed == 1 && m->ip == pc + 4 &&
                        fixture.Statistics.SemanticHelperInstructions == 0 && fixture.Statistics.InterpreterAttempts == 0,
                        $"FX route={route} load={load} issued={fixture.Issued} completed={fixture.Completed} ip={m->ip:x} helper={fixture.Statistics.SemanticHelperInstructions} interpreter={fixture.Statistics.InterpreterAttempts} exit={lastFx.Exit} resume={lastFx.ResumeIndex} memory={lastFx.MemoryAddress:x}/{lastFx.MemoryAccess}/{lastFx.MemoryWidth}/{lastFx.MemoryFlags} state={lastDescriptor.State} tag={lastDescriptor.Version:x}");
                    if (!load && route is not "lazy-tail")
                        for (int at = 416; at < 512; ++at)
                        {
                            byte* page = (byte*)fixture.Physical((ea + (uint)at) & ~4095UL);
                            Check(page[(ea + (uint)at) & 4095] == image[at], "actual FXsave leaves software tail unchanged");
                        }
                }
            }
            finally { fixture.NativePrepared = null; fixture.NativeReturned = null; fixture.EndCanonicalFamily(); }
        }
    }
}
