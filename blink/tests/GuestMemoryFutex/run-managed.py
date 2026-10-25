#!/usr/bin/env python3
"""Run fixtures against an explicitly fresh producer in a private build copy."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
from campaign_delivery import load_clean_delivery
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
HELPER = HERE.parent / 'GuestThreads/run-managed.py'
spec = importlib.util.spec_from_file_location('memory_futex_closure', HELPER)
helper = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helper)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--clean-delivery', type=Path, required=True,
                        help='Pointer accepted by campaign_delivery.load_clean_delivery; stale DLLs are insufficient')
    parser.add_argument('--native-receipt', type=Path, required=True)
    parser.add_argument('--form', choices=['raw', 'processed'], default='processed')
    parser.add_argument('--aot', action='store_true', help='Also publish/run the private consumer with NativeAOT')
    args = parser.parse_args()
    base = ROOT / 'generated/guest-memory-futex-managed'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, inputs={}, commands=[], executions=[], form=args.form)
    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
    def pin(path, expected=None):
        path = Path(path).resolve(); digest = sha(path)
        if expected is not None and digest != expected: raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), digest) != digest: raise RuntimeError('Input changed: ' + str(path))
        return path
    def run(label, command, timeout=300):
        row = dict(label=label, command=list(map(str, command)), exit_code=None,
                   timeout_seconds=timeout)
        receipt['commands'].append(row); save()
        log = attempt / (label + '.log')
        process = None
        try:
            with log.open('wb') as output:
                process = subprocess.Popen(row['command'], cwd=attempt, stdout=output, stderr=subprocess.STDOUT,
                    stdin=subprocess.DEVNULL, start_new_session=True,
                    env=dict(os.environ, MSBUILDDISABLENODEREUSE='1'))
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            if process is not None: row['cleanup'] = helper.cleanup(process)
            if log.exists(): row['log'] = dict(path=str(log), sha256=sha(log))
            save()
        if row['exit_code'] or row['cleanup']['signals'] or not row['cleanup']['group_gone']:
            raise RuntimeError(label + ' did not complete normally')
        print(label + ': passed', flush=True)
    try:
        for path in (Path(__file__), HERE / 'Program.cs', ROOT / 'scripts/campaign_delivery.py'):
            pin(path)
        producer_path, delivery = load_clean_delivery(pin(args.clean_delivery))
        pin(producer_path)
        receipt['delivery'] = dict(path=str(producer_path), sha256=sha(producer_path), form=args.form)
        profile = Path(delivery['profile'])
        assembly = json.loads(pin(delivery['assembly']['path'], delivery['assembly']['sha256']).read_text())
        # Check the actual C producer inputs contain today's authored corrections.
        # This rejects a structurally valid old delivery with stale syscall code.
        for unit, snippets in [('syscall.c', ['futex-wake.inc', 'futex-wait.inc']),
                                ('memorymalloc.c', ['memory-advice.inc'])]:
            row = assembly['objects']['blink/' + unit]
            # The assembly source identity is validated by load_clean_delivery.
            from core_inputs import profile_sources
            profile_inputs = json.loads((profile / 'inputs.json').read_text())
            entries = profile_sources(profile, ROOT, profile_inputs)
            entry = next(e for e in entries if e['path'] == 'blink/' + unit)
            source = pin(ROOT / entry['staged_path'], row['source_sha256']).read_text()
            for name in snippets:
                snippet = pin(ROOT / 'src/UpstreamGuestThreads' / name).read_text()
                if snippet not in source: raise RuntimeError('Fresh ' + name + ' producer required')
        native_path = pin(args.native_receipt)
        native = json.loads(native_path.read_text())
        if not native.get('passed') or not native.get('completed') or native.get('managed_execution') is not False:
            raise RuntimeError('Passing native run.py receipt required')
        for path, digest in native['inputs'].items(): pin(path, digest)
        pin(HELPER)
        library = Path(delivery['raw_snapshot' if args.form == 'raw' else 'stable_output'])
        closure = helper.source_closure(library / 'TranslatedBlink.csproj')
        private = attempt / 'private'
        copied = private / 'generated/TranslatedBlink'
        shutil.copytree(library, copied, ignore=shutil.ignore_patterns('bin', 'obj'))
        for original, digest in closure.items():
            pin(original, digest)
            if not original.is_relative_to(library):
                target = private / original.relative_to(ROOT)
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(original, target)
        owner = ROOT / 'src/Managed.Emulation.ThreadedExecution'
        copied_owner = private / 'src/Managed.Emulation.ThreadedExecution'
        shutil.copytree(owner, copied_owner, ignore=shutil.ignore_patterns('bin', 'obj'))
        for original in owner.rglob('*'):
            if original.is_file() and not {'bin', 'obj'}.intersection(original.relative_to(owner).parts): pin(original)
        for path in [private, *private.rglob('*')]:
            path.chmod(0o755 if path.is_dir() else 0o644)
        consumer = private / 'tests/GuestMemoryFutex'; consumer.mkdir(parents=True)
        shutil.copyfile(HERE / 'Program.cs', consumer / 'Program.cs')
        project = consumer / 'GuestMemoryFutex.csproj'
        root = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
        props = ET.SubElement(root, 'PropertyGroup')
        for key, value in dict(TargetFramework='net10.0', OutputType='Exe', ImplicitUsings='enable',
                              Nullable='enable', AllowUnsafeBlocks='true').items():
            ET.SubElement(props, key).text = value
        items = ET.SubElement(root, 'ItemGroup')
        ET.SubElement(items, 'ProjectReference', Include='../../src/Managed.Emulation.ThreadedExecution/Managed.Emulation.ThreadedExecution.csproj')
        ET.SubElement(items, 'TrimmerRootAssembly', Include='TranslatedBlink')
        ET.ElementTree(root).write(project, encoding='unicode')
        # All compilation outputs are private. Never rebuild the shared delivery.
        run('build', ['dotnet', 'build', project, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'])
        modes = [('jit', ['dotnet', consumer / 'bin/Release/net10.0/GuestMemoryFutex.dll'])]
        if args.aot:
            run('publish-aot', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                '-p:PublishAot=true', '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', attempt / 'aot'], 1200)
            modes.append(('aot', [attempt / 'aot/GuestMemoryFutex']))
        for mode, command in modes:
            for name in ('memory-profile', 'shared-profile', 'futex-profile'):
                image = pin(native_path.parent / name)
                report = attempt / (mode + '-' + name + '.json')
                run(mode + '-' + name, [*command, image, report], 40)
                value = json.loads(pin(report).read_text())
                if not value.get('passed') or value['image_sha256'] != sha(image): raise RuntimeError('Guest report differs')
                receipt['executions'].append(dict(mode=mode, fixture=name, report=value))
        if any(sha(path) != digest for path, digest in receipt['inputs'].items()): raise RuntimeError('Frozen input changed')
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True; save()
    print('receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
