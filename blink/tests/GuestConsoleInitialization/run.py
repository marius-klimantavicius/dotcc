#!/usr/bin/env python3
"""Private diagnostic of retained r8 Console/native initialization; no qualification claim."""
import argparse
import json
from pathlib import Path
import shutil
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REPO = ROOT.parent
sys.path.insert(0, str(HERE.parent / 'CoreClrRuntime'))
from evidence import Evidence, DOTNET, sha
sys.path.insert(0, str(REPO / 'Scripts'))
from production_support import locked_command


def once(source, before, after):
    if source.count(before) != 1:
        raise RuntimeError('Diagnostic boundary differs: ' + before[:100])
    return source.replace(before, after, 1)


def prepare(ev, captured):
    receipt = ev.pin(captured / 'receipt.json')
    original = json.loads(receipt.read_text())
    if original['delivery']['sha256'] != '3cfb7826eb9018e63b40d56a8055eaf1e02601572ac6e9c8e5c5df08185f235b':
        raise RuntimeError('Expected retained r8 consumer')
    for row in original['source_closure'].values():
        ev.pin(row['retained'], row['sha256'])
    private = ev.directory / 'private'
    shutil.copytree(captured / 'private', private, ignore=shutil.ignore_patterns('bin', 'obj', '__pycache__'))
    copied = {p.relative_to(private).as_posix(): sha(p) for p in private.rglob('*') if p.is_file()}
    for name, digest in copied.items():
        ev.pin(captured / 'private' / name, digest)
    ev.value['captured_source'] = dict(attempt=str(captured), receipt_sha256=sha(receipt), files=copied,
        delivery=original['delivery'], frozen_owner=True)
    owner = private / 'src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs'
    source = owner.read_text()
    source = once(source, 'ulong Argument5, ulong Argument6, ulong? ReturnValue);',
        'ulong Argument5, ulong Argument6, ulong? ReturnValue) { public string? DiagnosticPath { get; init; } public ulong OwnerBytes { get; init; } public long MemoryFileBytes { get; init; } public long Instructions { get; init; } public long VssPages { get; init; } public long RssPages { get; init; } public long MaxVssPages { get; init; } public long MaxRssPages { get; init; } public ulong RlimitAsCurrent { get; init; } }')
    source = once(source, 'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{',
        'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{\n'
        '    public static Action<ThreadedSyscallObservation>? DiagnosticTrace;\n'
        '    private ulong diagnosticNativeBase;')
    source = once(source, 'trace = syscallTrace;', 'trace = syscallTrace ?? DiagnosticTrace;')
    # Use the dispatcher's normal instruction cache, inside the same trap path.
    # Preserve errno around observation and leave guest fault handling to the
    # original ExecuteOne catch/finally boundary.
    source = once(source, 'ThreadedSyscallObservation? observation = trace == null ? null : Observe(worker);',
        'ThreadedSyscallObservation? observation = null;')
    source = once(source, 'try { program!.ExecuteInstruction(worker.Machine); returned = true; }',
        'try { observation = trace == null ? null : Observe(worker); program!.ExecuteInstruction(worker.Machine); returned = true; }')
    source = once(source, 'Blink.XedDecodedInst decoded = default;\n            var machine = worker.Machine;\n            if (program!.GetInstruction(machine, (long)machine->ip, &decoded) != 0 || ((decoded.op.rde >> 40) & 0x7ff) != 0x105) return null;',
        '''var machine = worker.Machine;
            program!.LoadInstruction(machine, (ulong)program.GetPc(machine));
            if (diagnosticNativeBase != 0 && machine->ip == diagnosticNativeBase + 0x1638e)
                return new(worker.GuestId, machine->ip, ulong.MaxValue, Register(machine, 0), 0, 0, 0, 0, 0, null)
                    { DiagnosticPath = "exact-native-signal-core-return" };
            if (((machine->xedd->op.rde >> 40) & 0x7ff) != 0x105) return null;
            // glibc tcgetattr uses a stack temporary for ioctl. The signal
            // pipe passes the exact library's static g_signalPipe directly.
            if (Register(machine, 0) == 293 && Register(machine, 6) == 0x80000)
            {
                ulong candidate = Register(machine, 7) - 0x1a9b8;
                byte* pointer = program.SpyAddress(machine, (long)(candidate + 0x16389));
                if (pointer != null && pointer[0] == 0xe8 && pointer[1] == 0x12 && pointer[2] == 0xdf && pointer[3] == 0xff && pointer[4] == 0xff)
                    diagnosticNativeBase = candidate;
            }''')
    source = once(source, 'var item = observation with { ReturnValue = returned ? Register(worker.Machine, 0) : null };',
        'var item = observation with { ReturnValue = returned ? Register(worker.Machine, 0) : null, OwnerBytes = program!.BlinkHostMemoryBytes(), MemoryFileBytes = io.MemoryFilesUsage().Bytes, Instructions = Interlocked.Read(ref issued), VssPages = Blink.Atomic.Load(ref system->vss), RssPages = Blink.Atomic.Load(ref system->rss), MaxVssPages = program.GetMaxVss(system), MaxRssPages = program.GetMaxRss(system), RlimitAsCurrent = BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(((Blink.rlimit_linux*)&system->rlim)[9].cur, 8)) };')
    source = once(source,
        'Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null);',
        'Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null)\n'
        '                { DiagnosticPath = DiagnosticPath(worker) };')
    helper = '''    private string? DiagnosticPath(Worker worker)
    {
        var machine = worker.Machine;
        ulong number = Register(machine, 0);
        ulong address = number switch
        {
            2 or 21 or 89 or 137 => Register(machine, 7),
            257 or 262 or 267 => Register(machine, 6),
            _ => 0
        };
        if (address == 0) return null;
        var bytes = new List<byte>();
        for (int index = 0; index < 1024; ++index)
        {
            byte* pointer = program!.SpyAddress(machine, (long)(address + (ulong)index));
            if (pointer == null || *pointer == 0) break;
            bytes.Add(*pointer);
        }
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
'''
    source = once(source, '    private ThreadedSyscallObservation? Observe(Worker worker)',
        helper + '    private ThreadedSyscallObservation? Observe(Worker worker)')
    owner.write_text(source)
    probe = private / 'tests/CoreClrRuntime/Probe/Program.cs'
    source = probe.read_text()
    source = once(source, 'BlinkMachine? machine = null;', '''var diagnostics = new List<Managed.Emulation.Execution.ThreadedSyscallObservation>();
var diagnosticGate = new object();
Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = observation =>
{
    lock (diagnosticGate)
    {
        if (diagnostics.Count < 32768) diagnostics.Add(observation);
        File.AppendAllText(output + ".syscalls.jsonl", JsonSerializer.Serialize(observation) + "\\n");
    }
};
string? nativeFixture = Environment.GetEnvironmentVariable("CONSOLE_INIT_NATIVE");
string nativeMode = Environment.GetEnvironmentVariable("CONSOLE_INIT_MODE") ?? "init";
BlinkMachine? machine = null;''')
    source = once(source, '    string[] arguments = scenario switch', '''    if (nativeFixture != null) machine.ImportImage([new ImageFile("/bin/console-init", File.ReadAllBytes(nativeFixture), true)]);
    string[] arguments = scenario switch''')
    source = once(source, '    run = await machine.StartAsync(new ExecutionOptions { Executable = scenario == "memfd" ? "/bin/guest-memfd" : "/dotnet/dotnet", Arguments = arguments,',
        '''    if (nativeFixture != null) arguments = [nativeMode];
    run = await machine.StartAsync(new ExecutionOptions { Executable = nativeFixture != null ? "/bin/console-init" : scenario == "memfd" ? "/bin/guest-memfd" : "/dotnet/dotnet", Arguments = arguments,''')
    marker = 'Console.WriteLine($"coreclr {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");'
    source = once(source, marker, '''Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = null;
File.WriteAllText(output + ".syscalls.json", JsonSerializer.Serialize(new
{
    DiagnosticOnly = true, FreshQualification = false, PrivateOwnerInstrumentation = true,
    ActualPublicImportPath = true, Scenario = scenario, NativeMode = nativeFixture == null ? null : nativeMode,
    Calls = diagnostics.ToArray()
}, new JsonSerializerOptions { WriteIndented = true }));
''' + marker)
    probe.write_text(source)
    ev.value['private_instrumentation'] = {str(p): sha(p) for p in (owner, probe)}
    for p in private.rglob('*'):
        if p.is_file(): ev.pin(p)
    return private


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--captured', type=Path, default=ROOT / 'artifacts/coreclr-runtime/guest/attempt-zqy711oa')
    parser.add_argument('--closure', type=Path, default=ROOT / 'artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json')
    parser.add_argument('--managed-smoke', action='store_true')
    parser.add_argument('--fixture', type=Path, help='Reuse an identical previously built native diagnostic')
    parser.add_argument('--seconds', type=int, default=600)
    args = parser.parse_args()
    if not 1 <= args.seconds <= 600:
        parser.error('--seconds must be from 1 through 600')
    ev = Evidence('console-initialization')
    try:
        ev.value.update(max_build_jobs=1, fresh_qualification=False, observer='normal LoadInstruction opcache; observation inside original fault boundary; errno preserved',
            prior_coordinator_stop='Prior coordinator reported automated content filter possible cyber risk; safeguards neither disabled nor circumvented')
        for path in (Path(__file__), HERE / 'console-init.c', REPO / 'Scripts/production_support.py', HERE.parent / 'CoreClrRuntime/evidence.py'):
            ev.capture(path, ev.directory / 'harness-inputs' / path.name)
        private = prepare(ev, args.captured.resolve())
        fixture = ev.directory / 'console-init'
        if args.fixture:
            source = ev.pin(args.fixture)
            prior = json.loads(ev.pin(source.parent / 'receipt.json').read_text())
            source_binding = prior.get('captured_inputs', {}).get(str(HERE / 'console-init.c'), {})
            if source_binding.get('sha256') != sha(HERE / 'console-init.c') or not any(
                    row['label'] == 'native-build' and row['exit_code'] == 0 for row in prior['commands']):
                raise RuntimeError('Reused native fixture lacks matching source and successful build evidence')
            ev.capture(source, fixture)
            fixture.chmod(0o755)
            ev.value['native_fixture_reused'] = str(source)
        else:
            ev.run('native-build', locked_command(['cc', '-O2', '-pthread', HERE / 'console-init.c', '-ldl', '-o', fixture]), timeout=60)
        ev.pin(fixture)
        project = private / 'tests/CoreClrRuntime/Probe/Probe.csproj'
        ev.run('private-build', locked_command([DOTNET, 'build', project, '-c', 'Release',
            '-p:BlinkProject=' + str(private / 'generated/TranslatedBlink/TranslatedBlink.csproj')], env=ev.environment), timeout=600)
        app = private / 'tests/CoreClrRuntime/Probe/bin/Release/net10.0'
        ev.tree(app)
        closure = ev.pin(args.closure)
        manifest = json.loads(closure.read_text())
        for row in manifest['Files']: ev.pin(Path(manifest['Images']) / row['File'], row['Sha256'])
        library = Path(manifest['Images']) / 'dotnet/shared/Microsoft.NETCore.App/10.0.12/libSystem.Native.so'
        for mode in ('preflight', 'init'):
            ev.run('host-' + mode, [fixture, mode, library], timeout=15, require_success=False)
            report = ev.directory / ('native-' + mode + '.json')
            ev.run('public-native-' + mode, [DOTNET, app / 'Probe.dll', 'InProcess', closure,
                'smoke', '50000000', '60', '256', 'wx-default', report], timeout=90, require_success=False,
                env={'CONSOLE_INIT_NATIVE': str(fixture), 'CONSOLE_INIT_MODE': mode})
            ev.pin(report); ev.pin(str(report) + '.syscalls.json')
            outcome = json.loads(report.read_text())
            ev.value.setdefault('native_outcomes', {})[mode] = dict(
                exited=outcome.get('Result', {}).get('Reason') == 0,
                exit_code=outcome.get('Result', {}).get('ExitCode'),
                stdout=outcome.get('Stdout'), stderr=outcome.get('Stderr'),
                resources_released=outcome.get('Result', {}).get('ResourcesReleased'),
                run_disposed=outcome.get('RunDisposed'), machine_disposed=outcome.get('MachineDisposed'))
        if args.managed_smoke:
            report = ev.directory / 'managed-smoke.json'
            ev.run('public-managed-smoke', [DOTNET, app / 'Probe.dll', 'InProcess', closure,
                'smoke', '700000000', str(args.seconds), '256', 'wx-default', report], timeout=args.seconds + 30, require_success=False)
            ev.pin(report); ev.pin(str(report) + '.syscalls.json')
        ev.value['passed'] = True  # diagnostic collection completed, not guest qualification
        ev.check()
    except BaseException as error:
        ev.value['error'] = type(error).__name__ + ': ' + str(error)
    return ev.finish()


if __name__ == '__main__': raise SystemExit(main())
