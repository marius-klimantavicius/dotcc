#!/usr/bin/env python3
"""Compare a normal static Linux churn ELF with a privately built managed owner."""
import argparse
import glob
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
EXPECTED = b'guest-thread-churn: 96 cleared, capacity recovered\n'
SCENARIOS = ('churn', 'capacity', 'join', 'cancel-start', 'cancel-clone', 'blocked')
sys.dont_write_bytecode = True
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    selection = parser.add_mutually_exclusive_group(required=True)
    selection.add_argument('--native-only', action='store_true')
    selection.add_argument('--delivery-receipt', type=Path, help='Actual fresh shared producer receipt')
    selection.add_argument('--host-library', type=Path,
                           help='Existing DLL directory; host-owner evidence only, NEVER fresh C qualification')
    parser.add_argument('--delivery-sha256', help='Required exact receipt SHA256 with --delivery-receipt')
    parser.add_argument('--form', choices=('raw', 'processed'), default='processed')
    parser.add_argument('--aot', action='store_true', help='Also publish/run this isolated consumer with NativeAOT')
    args = parser.parse_args()
    if bool(args.delivery_receipt) != bool(args.delivery_sha256):
        parser.error('--delivery-receipt and --delivery-sha256 must be supplied together')
    if args.native_only and args.aot:
        parser.error('--aot requires managed execution')
    base = ROOT / 'artifacts/guest-thread-churn'; base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, completed=False, inputs={}, commands=[], executions=[],
                   managed_execution=not args.native_only, fresh_delivery=False,
                   scope='native-only' if args.native_only else 'host-owner-only' if args.host_library else 'fresh-delivery',
                   form=args.form)

    def save():
        temporary = attempt / 'receipt.tmp'
        temporary.write_text(json.dumps(receipt, indent=2) + '\n')
        temporary.replace(attempt / 'receipt.json')

    def pin(path, expected=None):
        path = Path(path).resolve(); digest = sha(path)
        if expected is not None and digest != expected:
            raise RuntimeError('Identity differs: ' + str(path))
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def run(label, command, timeout=60):
        row = dict(label=label, command=list(map(str, command)), timeout_seconds=timeout)
        receipt['commands'].append(row); save()
        stdout, stderr = attempt / (label + '.stdout'), attempt / (label + '.stderr')
        process = None; started = time.monotonic()
        try:
            with stdout.open('wb') as out, stderr.open('wb') as err:
                process = subprocess.Popen(row['command'], cwd=attempt, stdin=subprocess.DEVNULL,
                    stdout=out, stderr=err, start_new_session=True,
                    env=dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1'))
                row['exit_code'] = process.wait(timeout=timeout)
        finally:
            # Reap the whole private process group on timeout/interruption.
            signals = []
            if process is not None:
                for sig in (signal.SIGTERM, signal.SIGKILL):
                    process.poll()
                    try: os.killpg(process.pid, 0)
                    except ProcessLookupError: break
                    os.killpg(process.pid, sig); signals.append(sig.name)
                    try: process.wait(timeout=3)
                    except subprocess.TimeoutExpired: pass
                process.wait(timeout=5)
            row.update(cleanup_signals=signals, seconds=time.monotonic() - started,
                       stdout=str(stdout), stderr=str(stderr))
            row['outputs'] = {str(p): sha(p) for p in (stdout, stderr) if p.exists()}
            save()
        if row['exit_code'] != 0 or signals:
            raise RuntimeError(label + ' did not finish normally; see ' + str(stderr))
        print(label + ': passed', flush=True)
        return stdout.read_bytes(), stderr.read_bytes()

    def source_closure(project):
        selected, visited = {}, set()
        def visit(path):
            path = path.resolve()
            if path in visited: return
            visited.add(path)
            if not path.is_relative_to(ROOT): raise RuntimeError('External project: ' + str(path))
            selected[path] = sha(path)
            tree = ET.parse(path).getroot()
            if tree.findall('.//Import'): raise RuntimeError('Unreviewed MSBuild import')
            defaults = tree.find('.//EnableDefaultCompileItems')
            if defaults is None or defaults.text != 'false':
                for source in path.parent.rglob('*.cs'):
                    if not {'bin', 'obj'}.intersection(source.relative_to(path.parent).parts):
                        selected[source] = sha(source)
            for node in tree.findall('.//Compile'):
                include = node.get('Include')
                if not include or '$(' in include or ';' in include or node.get('Condition'):
                    raise RuntimeError('Unreviewed Compile selection')
                sources = [Path(p).resolve() for p in glob.glob(str(path.parent / include), recursive=True) if Path(p).is_file()]
                if not sources: raise RuntimeError('Empty Compile selection')
                for source in sources:
                    if not source.is_relative_to(ROOT): raise RuntimeError('External authored source')
                    selected[source] = sha(source)
            for node in tree.findall('.//ProjectReference'):
                include = node.get('Include')
                if not include or '$(' in include or ';' in include or node.get('Condition'):
                    raise RuntimeError('Unreviewed ProjectReference selection')
                visit(path.parent / include)
        visit(project)
        return selected

    try:
        if (platform.system(), platform.machine()) != ('Linux', 'x86_64'):
            raise RuntimeError('This fixture requires Linux x86-64')
        inputs = attempt / 'inputs'; inputs.mkdir()
        for name in ('run.py', 'Program.cs', 'fixture.S', 'fixture.ld'):
            original = pin(HERE / name)
            shutil.copyfile(original, inputs / name); pin(inputs / name, sha(original))
        for tool in ('as', 'ld', 'dotnet') if not args.native_only else ('as', 'ld'):
            pin(shutil.which(tool))
        run('assemble', ['as', '--64', inputs / 'fixture.S', '-o', attempt / 'fixture.o'])
        image = attempt / 'guest-thread-churn.elf'
        run('link', ['ld', '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096',
                     '-T', inputs / 'fixture.ld', attempt / 'fixture.o', '-o', image])
        pin(image)
        receipt['elf'] = dict(path=str(image), sha256=sha(image))
        for scenario, argument in (('churn', 's'), ('capacity', 'n'), ('join', 'j')):
            out, err = run('native-' + scenario, [image, argument], 20)
            if out != (b'' if scenario == 'join' else EXPECTED) or err:
                raise RuntimeError('Native witness output differs')
            receipt['executions'].append(dict(mode='native-linux', scenario=scenario, passed=True,
                managed_cap_enforced=False, argument=argument, stdout_hex=out.hex()))
        receipt['native_completed'] = True; save()
        if not args.native_only:
            private = attempt / 'private'; private.mkdir()
            project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
            props = ET.SubElement(project, 'PropertyGroup')
            for key, value in dict(TargetFramework='net10.0', OutputType='Exe', ImplicitUsings='enable',
                                  Nullable='enable', AllowUnsafeBlocks='true', AssemblyName='GuestThreadChurn',
                                  IsAotCompatible='true').items():
                ET.SubElement(props, key).text = value
            items = ET.SubElement(project, 'ItemGroup')
            ET.SubElement(items, 'TrimmerRootAssembly', Include='TranslatedBlink')
            owner = ROOT / 'src/Managed.Emulation.ThreadedExecution'
            if args.host_library:
                # Explicit diagnostic mode compiles today's owner, never the old owner DLL.
                library = args.host_library.resolve()
                copied_library = private / 'library'; copied_library.mkdir()
                for name in ('TranslatedBlink.dll', 'Managed.Emulation.Host.dll'):
                    original = pin(library / name)
                    shutil.copyfile(original, copied_library / name); pin(copied_library / name, sha(original))
                    ref = ET.SubElement(items, 'Reference', Include=Path(name).stem)
                    ET.SubElement(ref, 'HintPath').text = str(copied_library / name)
                original = pin(owner / 'ThreadedGuestExecution.cs')
                shutil.copyfile(original, private / original.name); pin(private / original.name, sha(original))
            else:
                sys.path.insert(0, str(ROOT / 'scripts'))
                from campaign_delivery import load_clean_delivery
                pin(ROOT / 'scripts/campaign_delivery.py')
                delivery_path = pin(args.delivery_receipt, args.delivery_sha256)
                pointer = attempt / 'delivery-pointer.json'
                pointer.write_text(json.dumps(dict(receipt=str(delivery_path))))
                _, delivery = load_clean_delivery(pointer)
                receipt['delivery'] = dict(path=str(delivery_path), sha256=sha(delivery_path))
                receipt['fresh_delivery'] = True
                library = Path(delivery['raw_snapshot' if args.form == 'raw' else 'stable_output'])
                inventory = delivery['raw_files' if args.form == 'raw' else 'final_files']
                copied = private / 'generated/TranslatedBlink'
                copied.mkdir(parents=True)
                for name, digest in inventory.items():
                    original = pin(library / name, digest)
                    target = copied / name; target.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(original, target); pin(target, digest)
                closure = source_closure(library / 'TranslatedBlink.csproj')
                for original, digest in closure.items():
                    pin(original, digest)
                    if not original.is_relative_to(library):
                        if delivery['authored_sources'].get(str(original.relative_to(ROOT))) != digest:
                            raise RuntimeError('Authored producer input differs: ' + str(original))
                        target = private / original.relative_to(ROOT)
                        target.parent.mkdir(parents=True, exist_ok=True)
                        shutil.copyfile(original, target); pin(target, digest)
                copied_owner = private / 'src/Managed.Emulation.ThreadedExecution'
                copied_owner.mkdir(parents=True)
                for original in owner.iterdir():
                    if original.suffix in ('.cs', '.csproj'):
                        pin(original)
                        target = copied_owner / original.name
                        shutil.copyfile(original, target); pin(target, sha(original))
                ET.SubElement(items, 'ProjectReference', Include=str(copied_owner / 'Managed.Emulation.ThreadedExecution.csproj'))
            shutil.copyfile(inputs / 'Program.cs', private / 'Program.cs'); pin(private / 'Program.cs')
            # Directory.Build.props cannot discover unrelated shared project settings.
            (attempt / 'Directory.Build.props').write_text('<Project><PropertyGroup><LangVersion>14</LangVersion></PropertyGroup></Project>\n')
            csproj = private / 'GuestThreadChurn.csproj'
            # Keep the consumer in a separate directory so its default glob cannot
            # also compile the privately copied owner/generated project sources.
            consumer = private / 'consumer'; consumer.mkdir()
            (private / 'Program.cs').rename(consumer / 'Program.cs')
            receipt['inputs'].pop(str(private / 'Program.cs'))
            pin(consumer / 'Program.cs')
            if args.host_library:
                (private / 'ThreadedGuestExecution.cs').rename(consumer / 'ThreadedGuestExecution.cs')
                receipt['inputs'].pop(str(private / 'ThreadedGuestExecution.cs'))
                pin(consumer / 'ThreadedGuestExecution.cs')
            csproj = consumer / csproj.name
            ET.ElementTree(project).write(csproj, encoding='unicode'); pin(csproj)
            run('build', ['dotnet', 'build', csproj, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'], 600)
            modes = [('jit', ['dotnet', consumer / 'bin/Release/net10.0/GuestThreadChurn.dll'])]
            if args.aot:
                run('publish-aot', ['dotnet', 'publish', csproj, '-c', 'Release', '-r', 'linux-x64',
                    '-p:PublishAot=true', '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', attempt / 'aot'], 1200)
                modes.append(('aot', [attempt / 'aot/GuestThreadChurn']))
            for mode, command in modes:
                for binary in Path(command[-1]).parent.glob('*.dll'): pin(binary)
                pin(command[-1])
                for scenario in SCENARIOS:
                    report = attempt / (mode + '-' + scenario + '.json')
                    run(mode + '-' + scenario, [*command, image, scenario, report], 40)
                    value = json.loads(pin(report).read_text())
                    if value.get('passed') is not True or value['image_sha256'] != sha(image):
                        raise RuntimeError('Managed execution report differs')
                    receipt['executions'].append(dict(mode=mode, scenario=scenario, report=value))
                save()
            if args.delivery_receipt: load_clean_delivery(pointer)
        if any(sha(path) != digest for path, digest in receipt['inputs'].items()):
            raise RuntimeError('Pinned input changed during qualification')
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True; save()
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
