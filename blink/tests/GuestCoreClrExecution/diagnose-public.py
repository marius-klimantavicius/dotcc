#!/usr/bin/env python3
"""Private-only syscall trace of the actual public CoreCLR import/run path.

Copies an existing captured consumer closure, never shared generated output.
Only the private execution owner and Probe gain diagnostic observations; guest
instructions, host bindings, resource policies and images retain their bytes.
"""
import argparse
import hashlib
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
        raise RuntimeError('Private diagnostic boundary differs: ' + before[:100])
    return source.replace(before, after, 1)


def prepare(ev, captured):
    original_receipt = ev.pin(captured / 'receipt.json')
    original = json.loads(original_receipt.read_text())
    private = ev.directory / 'private'
    shutil.copytree(captured / 'private', private, ignore=shutil.ignore_patterns('bin', 'obj', '__pycache__'))
    copied = {p.relative_to(private).as_posix(): sha(p) for p in private.rglob('*') if p.is_file()}
    for name, digest in copied.items():
        if sha(captured / 'private' / name) != digest:
            raise RuntimeError('Captured input changed during copy: ' + name)
    ev.value['captured_source'] = dict(attempt=str(captured), receipt_sha256=sha(original_receipt), files=copied,
        delivery=original['delivery'], frozen_owner=True)
    owner = private / 'src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs'
    source = owner.read_text()
    source = once(source, 'ulong Argument5, ulong Argument6, ulong? ReturnValue);',
        'ulong Argument5, ulong Argument6, ulong? ReturnValue) { public string? DiagnosticPath { get; init; } }')
    source = once(source, 'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{',
        'public sealed unsafe partial class ThreadedGuestExecution : IHostGuestThreads\n{\n'
        '    public static Action<ThreadedSyscallObservation>? DiagnosticTrace;')
    source = once(source, 'trace = syscallTrace;', 'trace = syscallTrace ?? DiagnosticTrace;')
    source = once(source,
        'Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null);',
        'Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null)\n'
        '                { DiagnosticPath = DiagnosticPath(worker) };')
    marker = '    private ThreadedSyscallObservation? Observe(Worker worker)'
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
    source = once(source, marker, helper + marker)
    owner.write_text(source)
    probe = private / 'tests/CoreClrRuntime/Probe/Program.cs'
    source = probe.read_text()
    marker = 'BlinkMachine? machine = null;'
    source = once(source, marker, '''var diagnostics = new List<Managed.Emulation.Execution.ThreadedSyscallObservation>();
var diagnosticGate = new object();
Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = observation =>
{
    lock (diagnosticGate) { if (diagnostics.Count < 16384) diagnostics.Add(observation); }
};
''' + marker)
    marker = 'Console.WriteLine($"coreclr {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");'
    source = once(source, marker, '''Managed.Emulation.Execution.ThreadedGuestExecution.DiagnosticTrace = null;
File.WriteAllText(output + ".syscalls.json", System.Text.Json.JsonSerializer.Serialize(new
{
    DiagnosticOnly = true, FreshQualification = false, PrivateOwnerInstrumentation = true,
    ActualPublicImportPath = true, Scenario = scenario, Calls = diagnostics.ToArray()
}, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
''' + marker)
    probe.write_text(source)
    ev.value['private_instrumentation'] = {str(p): sha(p) for p in (owner, probe)}
    for p in private.rglob('*'):
        if p.is_file(): ev.pin(p)
    project = private / 'tests/CoreClrRuntime/Probe/Probe.csproj'
    ev.value['build_command'] = [str(DOTNET), 'build', str(project), '-c', 'Release', '--disable-build-servers',
        '-m:1', '-p:UseSharedCompilation=false', '-p:BlinkProject=' + str(private / 'generated/TranslatedBlink/TranslatedBlink.csproj')]
    ev.value['prepared'] = True
    ev.save()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--captured', type=Path, default=ROOT / 'artifacts/coreclr-runtime/guest/attempt-jx5tip6e')
    parser.add_argument('--closure', type=Path, default=ROOT / 'artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json')
    parser.add_argument('--scenario', choices=['smoke', 'workload', 'service'], default='smoke')
    parser.add_argument('--prepare-only', action='store_true')
    parser.add_argument('--resume', type=Path, help='Run the already prepared private attempt')
    args = parser.parse_args()
    ev = Evidence('public-diagnostic')
    if args.resume:
        # Retain the invocation receipt while all execution stays in its earlier
        # prepared, hash-checked private closure.
        prior = args.resume.resolve()
        previous = json.loads((prior / 'receipt.json').read_text())
        if not previous.get('prepared') or previous.get('executed'):
            raise RuntimeError('Expected a prepared, unexecuted diagnostic')
        ev.value['resumed_preparation'] = str(prior)
        ev.save()
        invocation_receipt = str(ev.directory / 'receipt.json')
        ev.directory = prior
        ev.value = previous
        ev.value.update(completed=False, resume_invocation=invocation_receipt)
    try:
        ev.capture(__file__, ev.directory / 'diagnose-public.py')
        ev.pin(REPO / 'Scripts/production_support.py')
        ev.value.update(scope=__doc__, max_build_jobs=1, fresh_qualification=False,
            prior_coordinator_stop='Prior coordinator reported automated content filter possible cyber risk; safeguards neither disabled nor circumvented')
        if not args.resume: prepare(ev, args.captured.resolve())
        if not args.prepare_only:
            ev.pin(DOTNET)
            ev.run('private-build', locked_command(ev.value['build_command'], env=ev.environment), timeout=600)
            app = ev.directory / 'private/tests/CoreClrRuntime/Probe/bin/Release/net10.0'
            ev.tree(app)
            closure = ev.pin(args.closure)
            manifest = json.loads(closure.read_text())
            for row in manifest['Files']: ev.pin(Path(manifest['Images']) / row['File'], row['Sha256'])
            report = ev.directory / ('public-' + args.scenario + '.json')
            ev.run('public-' + args.scenario, [DOTNET, app / 'Probe.dll', 'InProcess', closure,
                args.scenario, '300000000', '120', '256', 'wx-default', report], timeout=150, require_success=False)
            ev.pin(report); ev.pin(str(report) + '.syscalls.json')
            ev.value['executed'] = True
            ev.value['diagnostic_report'] = str(report)
            ev.value['passed'] = True
            ev.check()
    except BaseException as error:
        ev.value['error'] = type(error).__name__ + ': ' + str(error)
    result = ev.finish()
    return 0 if args.prepare_only and ev.value.get('prepared') and not ev.value.get('error') else result


if __name__ == '__main__': raise SystemExit(main())
