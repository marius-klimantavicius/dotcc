#!/usr/bin/env python3
"""Observe a frozen public CoreCLR workload without changing resource policies.

Only attempt-local execution/IO owners and the host Probe gain observations.
The captured translated code and all imported guest images remain byte-identical.
Guest failures are retained and never counted as a qualification pass.
"""
import argparse
import json
from pathlib import Path
import shutil
import sys

sys.dont_write_bytecode = True
REPO = next(parent for parent in Path(__file__).resolve().parents
            if (parent / 'blink/tests/CoreClrRuntime/evidence.py').is_file())
BLINK = REPO / 'blink'
sys.path.insert(0, str(BLINK / 'tests/CoreClrRuntime'))
from evidence import Evidence, DOTNET, sha
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command


def once(source, before, after):
    if source.count(before) != 1:
        raise RuntimeError('Private observation boundary differs: ' + before[:100])
    return source.replace(before, after, 1)


def prepare(ev, captured, diagnostic_virtual_mib=None):
    original_receipt = ev.pin(captured / 'receipt.json')
    original = json.loads(original_receipt.read_text())
    private = ev.directory / 'private'
    shutil.copytree(captured / 'private', private,
                    ignore=shutil.ignore_patterns('bin', 'obj', '__pycache__'))
    copied = {p.relative_to(private).as_posix(): sha(p)
              for p in private.rglob('*') if p.is_file()}
    for name, digest in copied.items():
        if sha(captured / 'private' / name) != digest:
            raise RuntimeError('Captured input changed during copy: ' + name)
    ev.value['captured_source'] = dict(attempt=str(captured), receipt_sha256=sha(original_receipt),
        files=copied, delivery=original['delivery'], frozen_owner=True)

    io = private / 'src/Managed.Emulation.Host/InstanceIo.MemoryDiagnostic.cs'
    io.write_text('''namespace Managed.Emulation.Host;

public sealed record DiagnosticMemoryBudget(long OwnerBytes, long CombinedLimit,
    long CombinedBytes, ulong BudgetToken, int RegularFiles, int RegularPages,
    int RegularLeases);

public sealed partial class InstanceIo
{
    public DiagnosticMemoryBudget DiagnosticMemorySnapshot()
    {
        lock (sync)
        {
            int pages = 0, leases = 0;
            foreach (var file in regularSharedFiles.Values)
            { pages += file.Pages.Count; leases += file.Leases; }
            return new(memoryOwnerBytes, memoryCombinedLimit,
                memoryOwnerBytes + memoryFileBytes, memoryBudgetToken,
                regularSharedFiles.Count, pages, leases);
        }
    }
}
''')
    owner = private / 'src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs'
    source = owner.read_text()
    source = once(source, 'ulong Argument5, ulong Argument6, ulong? ReturnValue);', '''ulong Argument5, ulong Argument6, ulong? ReturnValue)
{
    public string? DiagnosticPath { get; init; }
    public string? DiagnosticText { get; init; }
    public object? EntryMemory { get; init; }
    public object? ReturnMemory { get; init; }
    public ulong Instructions { get; init; }
    public ulong[]? GuestStack { get; init; }
}''')
    source = once(source, 'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{',
        'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{\n'
        '    public static Action<ThreadedSyscallObservation>? DiagnosticTrace;')
    source = once(source, 'trace = syscallTrace;', 'trace = syscallTrace ?? DiagnosticTrace;')
    source = once(source, 'trace = syscallTrace ?? DiagnosticTrace;',
        'trace = syscallTrace ?? DiagnosticTrace;\n        if (trace != null) InstallDiagnosticOpcode();')
    source = once(source,
        'ThreadedSyscallObservation? observation = trace == null ? null : Observe(worker);',
        'ThreadedSyscallObservation? observation = null; // Opcode 261 owns private observations.')
    source = once(source, 'if (IsQuiescent) Cleanup(program.Dispose);',
        'if (IsQuiescent) { RemoveDiagnosticOpcode(); Cleanup(program.Dispose); }')
    source = once(source,
        'Blink.XedDecodedInst decoded = default;', '')
    source = once(source,
        'if (program!.GetInstruction(machine, (long)machine->ip, &decoded) != 0 || ((decoded.op.rde >> 40) & 0x7ff) != 0x105) return null;',
        'Require(((machine->xedd->op.rde >> 40) & 0x7ff) == 0x105, "Diagnostic syscall opcode");')
    source = once(source,
        'Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null);',
        '''Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null)
            {
                DiagnosticPath = DiagnosticPath(worker),
                DiagnosticText = DiagnosticText(worker),
                EntryMemory = DiagnosticMemory(), Instructions = InstructionsCompleted
            };''')
    source = once(source,
        'var item = observation with { ReturnValue = returned ? Register(worker.Machine, 0) : null };',
        '''int diagnosticErrno = Blink.Libc.errno;
                ThreadedSyscallObservation item;
                try
                {
                    ulong? value = returned ? Register(worker.Machine, 0) : null;
                    item = observation with { ReturnValue = value, ReturnMemory = DiagnosticMemory(),
                        GuestStack = value is >= 0xfffffffffffff001UL || observation.Number is 1 or 20 or 234
                            ? DiagnosticStack(worker) : null };
                }
                finally { Blink.Libc.errno = diagnosticErrno; }''')
    marker = '    private ThreadedSyscallObservation? Observe(Worker worker)'
    helper = '''    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Blink, ThreadedGuestExecution>
        diagnosticOwners = new();
    private delegate*<Blink, Blink.Machine*, ulong, long, ulong, void> diagnosticOriginalOpcode;
    private void InstallDiagnosticOpcode()
    {
        var table = program!.Globals.kNexgen32e__unit_B1A290C531C47EA642D807D5161A1EC96CE8B5FE6A7B75385D90345E041A45E2;
        diagnosticOriginalOpcode = table[261];
        Require(diagnosticOriginalOpcode == Blink.BlinkCoreFunctionPointers.OpSyscall,
            "Captured syscall pointer identity");
        diagnosticOwners.Add(program, this);
        table[261] = &DiagnosticSyscall;
    }
    private void RemoveDiagnosticOpcode()
    {
        if (diagnosticOriginalOpcode == null) return;
        program!.Globals.kNexgen32e__unit_B1A290C531C47EA642D807D5161A1EC96CE8B5FE6A7B75385D90345E041A45E2[261]
            = diagnosticOriginalOpcode;
        diagnosticOwners.Remove(program);
    }
    private static void DiagnosticSyscall(Blink instance, Blink.Machine* machine, ulong rde, long disp, ulong uimm0)
    {
        if (!diagnosticOwners.TryGetValue(instance, out var owner))
            throw new InvalidOperationException("Diagnostic opcode has no instance owner.");
        var worker = owner.RequireCurrent();
        Require(worker.Machine == machine && ReferenceEquals(owner.program, instance), "Diagnostic syscall owner");
        var observation = owner.Observe(worker)! with { InstructionPointer = machine->ip - machine->oplen };
        bool returned = false;
        try { instance.OpSyscall(machine, rde, disp, uimm0); returned = true; }
        finally
        {
            int saved = Blink.Libc.errno;
            ThreadedSyscallObservation item;
            try
            {
                ulong? value = returned ? owner.Register(machine, 0) : null;
                item = observation with { ReturnValue = value, ReturnMemory = owner.DiagnosticMemory(),
                    GuestStack = value is >= 0xfffffffffffff001UL || observation.Number is 1 or 20 or 234
                        ? owner.DiagnosticStack(worker) : null };
            }
            finally { Blink.Libc.errno = saved; }
            lock (owner.gate) owner.trace!(item);
        }
    }
    private object DiagnosticMemory() => new
    {
        OwnerBytes = program!.BlinkHostMemoryBytes(),
        OwnerMappings = program.BlinkHostMemoryMappings(),
        OwnerLimit = program.BlinkHostMemoryLimit(),
        GuestVssPages = system->vss, GuestRssPages = system->rss,
        GuestMaxVssPages = program.GetMaxVss(system),
        GuestMaxRssPages = program.GetMaxRss(system),
        GuestRlimitAsBytes = BinaryPrimitives.ReadUInt64LittleEndian(
            new ReadOnlySpan<byte>(((Blink.rlimit_linux*)&system->rlim)[9].cur, 8)),
        MemoryFiles = io.MemoryFilesUsage(), Combined = io.DiagnosticMemorySnapshot()
    };
    private string? DiagnosticString(Worker worker, ulong address, int count, bool stopAtZero)
    {
        if (address == 0) return null;
        var bytes = new List<byte>();
        for (int index = 0; index < count; ++index)
        {
            byte* pointer = program!.SpyAddress(worker.Machine, (long)(address + (ulong)index));
            if (pointer == null || (stopAtZero && *pointer == 0)) break;
            bytes.Add(*pointer);
        }
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
    private string? DiagnosticPath(Worker worker)
    {
        var machine = worker.Machine;
        ulong address = Register(machine, 0) switch
        {
            2 or 21 or 83 or 87 or 89 or 137 or 319 => Register(machine, 7),
            257 or 258 or 262 or 263 or 267 => Register(machine, 6),
            _ => 0
        };
        return DiagnosticString(worker, address, 1024, true);
    }
    private string? DiagnosticText(Worker worker)
    {
        var machine = worker.Machine;
        return Register(machine, 0) == 1 && Register(machine, 7) <= 2
            ? DiagnosticString(worker, Register(machine, 6), (int)Math.Min(4096UL, Register(machine, 2)), false)
            : null;
    }
    private ulong[] DiagnosticStack(Worker worker)
    {
        ulong address = Register(worker.Machine, 4);
        var result = new List<ulong>();
        for (int index = 0; index < 32; ++index)
        {
            ulong value = 0;
            for (int part = 0; part < 8; ++part)
            {
                byte* pointer = program!.SpyAddress(worker.Machine, (long)(address + (ulong)(index * 8 + part)));
                if (pointer == null) return result.ToArray();
                value |= (ulong)*pointer << (part * 8);
            }
            result.Add(value);
        }
        return result.ToArray();
    }
'''
    source = once(source, marker, helper + marker)
    owner.write_text(source)

    probe = private / 'tests/CoreClrRuntime/Probe/Program.cs'
    source = probe.read_text()
    source = once(source, 'BlinkMachine? machine = null;', '''using var diagnosticWriter = new StreamWriter(output + ".syscalls.jsonl");
long diagnosticSequence = 0;
Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = observation =>
{
    diagnosticWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
        { Sequence = ++diagnosticSequence, Call = observation }));
    if (diagnosticSequence % 128 == 0 || observation.ReturnValue is >= 0xfffffffffffff001UL)
        diagnosticWriter.Flush();
};
BlinkMachine? machine = null;''')
    source = once(source,
        'Console.WriteLine($"coreclr {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");',
        '''Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = null;
diagnosticWriter.Flush();
Console.WriteLine($"coreclr {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");''')
    probe.write_text(source)
    observed = [owner, io, probe]
    if diagnostic_virtual_mib is not None:
        resources = private / 'src/Host/HostResourcesBridge.cs'
        source = resources.read_text()
        source = once(source, 'if (resource is 2 or 9)',
            'if (resource == 9) { limit = ' + str(diagnostic_virtual_mib) + 'UL << 20; return 0; }\n'
            '        if (resource is 2 or 9)')
        resources.write_text(source)
        observed.append(resources)
        guest_resources = private / 'src/GuestResources/GuestResourcesBridge.cs'
        source = guest_resources.read_text()
        source = once(source,
            'return BlinkHostInitializeResourceLimits(system, (ulong)capacity.Value);',
            '''int diagnosticResult = BlinkHostInitializeResourceLimits(system, (ulong)capacity.Value);
        if (diagnosticResult == 0)
        {
            ulong diagnosticAs = ''' + str(diagnostic_virtual_mib) + '''UL << 20;
            global::System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(
                new global::System.Span<byte>(((rlimit_linux*)&system->rlim)[9].cur, 8), diagnosticAs);
            global::System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(
                new global::System.Span<byte>(((rlimit_linux*)&system->rlim)[9].max, 8), diagnosticAs);
        }
        return diagnosticResult;''')
        guest_resources.write_text(source)
        observed.append(guest_resources)
        ev.value['diagnostic_virtual_override'] = dict(mib=diagnostic_virtual_mib,
            boundary='private HostResourcesBridge.ResourceLimit selector RLIMIT_AS (9) and GuestResourcesBridge initial rlimAS cur/max only',
            resident_backing_mib=256, memoryfile_mib=64, qualification=False)
    ev.value['private_instrumentation'] = {str(p): sha(p) for p in observed}
    ev.value['translated_code_unchanged'] = all(
        sha(private / name) == digest for name, digest in copied.items()
        if name.startswith('generated/'))
    ev.value['observer'] = dict(kind='per-instance-opcode-261',
        expected_original='Managed.Emulation.BlinkCore.BlinkCoreFunctionPointers.OpSyscall',
        dispatch_table='kNexgen32e__unit_B1A290C531C47EA642D807D5161A1EC96CE8B5FE6A7B75385D90345E041A45E2',
        generated_bytes_edited=False, direct_original_call=True, errno_preserved=True,
        owner_lookup='ConditionalWeakTable keyed by the exact BlinkCore instance; worker identity checked',
        restored_after_quiescence=True)
    for p in private.rglob('*'):
        if p.is_file(): ev.pin(p)
    project = private / 'tests/CoreClrRuntime/Probe/Probe.csproj'
    ev.value['build_command'] = [str(DOTNET), 'build', str(project), '-c', 'Release',
        '-p:BlinkProject=' + str(private / 'generated/TranslatedBlink/TranslatedBlink.csproj')]
    ev.value['prepared'] = True
    ev.save()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--captured', type=Path,
        default=BLINK / 'artifacts/coreclr-runtime/guest/attempt-wfoso6ej')
    parser.add_argument('--closure', type=Path,
        default=BLINK / 'artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json')
    parser.add_argument('--seconds', type=int, default=600)
    parser.add_argument('--instruction-limit', type=int, default=2_000_000_000)
    parser.add_argument('--prepare-only', action='store_true')
    parser.add_argument('--diagnostic-virtual-mib', type=int, choices=[4096],
        help='Private control only, after baseline proof; leaves backing and memoryfile limits unchanged')
    parser.add_argument('--baseline-evidence', type=Path,
        help='Completed baseline analysis to pin before an AS-only private control')
    parser.add_argument('--resume', type=Path, help='Retry a prepared, unexecuted private attempt')
    args = parser.parse_args()
    if not 1 <= args.seconds <= 600 or not 1 <= args.instruction_limit <= 2_000_000_000:
        parser.error('Diagnostic bounds cannot exceed 600 seconds / 2 billion instructions')
    if args.diagnostic_virtual_mib is not None and not args.baseline_evidence:
        parser.error('An AS-only control requires completed --baseline-evidence')
    ev = Evidence('memory-diagnostic')
    if args.resume:
        prior = args.resume.resolve()
        previous = json.loads((prior / 'receipt.json').read_text())
        if not previous.get('prepared') or previous.get('executed'):
            raise RuntimeError('Expected a prepared, unexecuted diagnostic')
        ev.value['resumed_preparation'] = str(prior)
        invocation_receipt = str(ev.save())
        ev.directory, ev.value = prior, previous
        ev.value.update(completed=False, resume_invocation=invocation_receipt)
        ev.value.pop('error', None)
    try:
        if not args.resume:
            ev.capture(__file__, ev.directory / 'diagnose.py')
            ev.capture(BLINK / 'tests/CoreClrRuntime/evidence.py', ev.directory / 'evidence.py')
            ev.capture(REPO / 'Scripts/production_support.py', ev.directory / 'production_support.py')
        ev.value.update(scope=__doc__, max_build_jobs=1, fresh_qualification=False,
            bounds=dict(memory_mib=256, memoryfile_mib=64, instruction_limit=args.instruction_limit,
                        seconds=args.seconds),
            prior_coordinator_stop='Prior coordinator reported automated content filter possible cyber risk; safeguards neither disabled nor circumvented')
        if args.baseline_evidence:
            baseline = ev.pin(args.baseline_evidence)
            analysis = json.loads(baseline.read_text())
            if not analysis.get('guest_report', {}).get('Result', {}).get('ResourcesReleased'):
                raise RuntimeError('Baseline must retain a completed guest report and released resources')
            for path, digest in analysis['inputs'].items(): ev.pin(path, digest)
            ev.value['baseline_evidence'] = str(baseline)
        if not args.resume: prepare(ev, args.captured.resolve(), args.diagnostic_virtual_mib)
        if not args.prepare_only:
            ev.pin(DOTNET)
            ev.run('private-build', locked_command(ev.value['build_command'], env=ev.environment), timeout=600)
            app = ev.directory / 'private/tests/CoreClrRuntime/Probe/bin/Release/net10.0'
            ev.tree(app)
            closure = ev.pin(args.closure)
            manifest = json.loads(closure.read_text())
            for row in manifest['Files']:
                ev.pin(Path(manifest['Images']) / row['File'], row['Sha256'])
            report = ev.directory / 'public-workload.json'
            ev.run('public-workload', [DOTNET, app / 'Probe.dll', 'InProcess', closure,
                'workload', str(args.instruction_limit), str(args.seconds), '256', 'wx-default', report],
                timeout=args.seconds + 30, require_success=False)
            ev.pin(report)
            ev.pin(str(report) + '.syscalls.jsonl')
            ev.value.update(executed=True, diagnostic_report=str(report),
                guest_passed=json.loads(report.read_text())['Passed'],
                diagnostic_capture_completed=True)
            ev.value['passed'] = ev.value['guest_passed']
            ev.check()
    except BaseException as error:
        ev.value['error'] = type(error).__name__ + ': ' + str(error)
    return ev.finish()


if __name__ == '__main__':
    raise SystemExit(main())
