#!/usr/bin/env python3
"""Private ownership/index semantics. --managed freshly translates authored C.

Default: native legacy and shared owners with structural AVL assertions.
--sanitize adds ASan/UBSan. --managed adds raw/processed JIT; --aot also adds
NativeAOT. Run managed builds only after the parent releases the build slot.
All outputs and frozen inputs stay in performance-implementation/mapping.
No product build, compiler rebuild, generated-source edits or timing assertions.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
BLINK = HERE.parents[1]
ROOT = BLINK.parent
sys.path.insert(0, str(ROOT / 'Scripts'))
from production_support import locked_command
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--managed', action='store_true')
parser.add_argument('--aot', action='store_true')
parser.add_argument('--sanitize', action='store_true')
args = parser.parse_args()
if args.aot and not args.managed:
    parser.error('--aot requires --managed')
base = BLINK / 'artifacts/performance-implementation/mapping'
base.mkdir(parents=True, exist_ok=True)
work = Path(tempfile.mkdtemp(prefix='semantics-', dir=base))
inputs = work / 'inputs'
inputs.mkdir()
receipt = dict(passed=False, scope='host range ownership/index; mock file leases, not guest execution',
               managed_requested=args.managed, aot_requested=args.aot, results=[], owned_processes=[])


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def inventory(path):
    return {str(p.relative_to(path)): sha(p) for p in sorted(path.rglob('*')) if p.is_file()}


def save():
    (work / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')


def run(label, command, timeout=60):
    command = list(map(str, command))
    effective = locked_command(command)
    entry = dict(label=label, command=command, effective_command=effective, exit_code=None, group_gone=False)
    receipt['results'].append(entry)
    with (work / (label + '.stdout')).open('wb') as stdout, (work / (label + '.stderr')).open('wb') as stderr:
        process = subprocess.Popen(effective, cwd=work, env=dict(os.environ, LC_ALL='C'),
                                   stdout=stdout, stderr=stderr, start_new_session=True)
        receipt['owned_processes'] = [process.pid]
        save()
        try:
            entry['exit_code'] = process.wait(timeout=timeout)
        finally:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait()
            try:
                os.killpg(process.pid, 0)
            except ProcessLookupError:
                entry['group_gone'] = True
            receipt['owned_processes'] = []
            entry['stdout_sha256'] = sha(work / (label + '.stdout'))
            entry['stderr_sha256'] = sha(work / (label + '.stderr'))
            save()
    if entry['exit_code'] != 0 or not entry['group_gone']:
        raise RuntimeError(label + ' failed: ' + str(work))
    return (work / (label + '.stdout')).read_bytes()


def interrupted(number, frame):
    raise InterruptedError('received signal ' + str(number))


signal.signal(signal.SIGTERM, interrupted)
try:
    sources = [HERE / 'probe.c', HERE / 'Program.cs', Path(__file__),
               BLINK / 'src/Host/HostMemory.c', BLINK / 'src/Host/include/HostMemory.h']
    receipt['authored_inputs'] = {str(p.relative_to(ROOT)): sha(p) for p in sources}
    for p in sources:
        shutil.copy2(p, inputs / p.name)
    expected = b'mapping ranges/protections/remap/reuse/aliases/truncate/quota/cleanup=pass\n'
    for profile in ['legacy', 'shared']:
        binary = work / ('native-' + profile)
        command = ['cc', '-std=c17', '-O2', '-Wall', '-Wextra', '-Werror', '-pthread',
                   '-DMAPPING_INDEX_VALIDATE', '-I', inputs, inputs / 'probe.c', '-o', binary]
        if profile == 'shared':
            command += ['-DBLINK_MANAGED_GUEST_THREADS']
        if args.sanitize:
            command += ['-fsanitize=address,undefined', '-fno-omit-frame-pointer']
        run('native-' + profile + '-build', command)
        digest = sha(binary)
        if run('native-' + profile, [binary]) != expected or sha(binary) != digest:
            raise RuntimeError('native transcript or executable mismatch')
    if args.managed:
        # Frozen tools/runtime and private projects avoid shared build outputs.
        for name, source in [('cli', ROOT / 'DotCC/bin/Release/net10.0'),
                             ('post', ROOT / 'DotCC.PostProcess/bin/Release/net10.0')]:
            shutil.copytree(source, inputs / name)
        shutil.copytree(ROOT / 'DotCC.Lib/include', inputs / 'headers')
        shutil.copy2(ROOT / 'DotCC.Libc/bin/Release/net10.0/DotCC.Libc.dll', inputs / 'DotCC.Libc.dll')
        profile = inputs / 'profile'
        (profile / 'sys').mkdir(parents=True)
        for source, target in [('config/managed-host/abi.h', 'abi.h'),
                               ('config/managed-host/sys/mman.h', 'sys/mman.h'),
                               ('config/managed-threaded/pthread.h', 'pthread.h'),
                               ('src/Host/include/host-guest-threads.h', 'host-guest-threads.h')]:
            shutil.copy2(BLINK / source, profile / target)
        (profile / 'config.h').write_text('#define BLINK_MANAGED_GUEST_THREADS 1\n'
                                          '#define HAVE_THREADS 1\n#define NOLINEAR 1\n#define DISABLE_JIT 1\n')
    receipt['frozen_inputs'] = inventory(inputs)
    save()
    if args.managed:
        for mode in ['legacy', 'shared']:
            raw = work / (mode + '-raw')
            flags = ['-DBLINK_TEST_MANAGED', '-DNOLINEAR', '-DDISABLE_JIT']
            if mode == 'shared':
                flags += ['-DBLINK_MANAGED_GUEST_THREADS', '-DHAVE_THREADS']
            run(mode + '-translate', ['dotnet', inputs / 'cli/dotcc.dll', '-std=c17', *flags,
                '-I', profile, '-I', inputs, '-I', inputs / 'headers', inputs / 'probe.c',
                '--runtime=c', '--emit=managedlib', '--nest-types', '--instance-methods',
                '--class-name', 'MappingProbe', '--namespace', 'Managed.Emulation', '-o', raw], 180)
            raw_before = inventory(raw)
            processed = work / (mode + '-processed')
            shutil.copytree(raw, processed)
            for form, generated in [('raw', raw), ('processed', processed)]:
                label = mode + '-' + form
                consumer = work / (label + '-consumer')
                consumer.mkdir()
                shutil.copy2(inputs / 'Program.cs', consumer / 'Program.cs')
                xml = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
                props = ET.SubElement(xml, 'PropertyGroup')
                for key, value in dict(TargetFramework='net10.0', OutputType='Exe',
                    AllowUnsafeBlocks='true', ImplicitUsings='enable', AssemblyName='MappingProbe',
                    WarningsAsErrors='CS8500').items():
                    ET.SubElement(props, key).text = value
                items = ET.SubElement(xml, 'ItemGroup')
                ET.SubElement(items, 'Compile', Include=str(generated / '*.cs'))
                ref = ET.SubElement(items, 'Reference', Include='DotCC.Libc')
                ET.SubElement(ref, 'HintPath').text = str(inputs / 'DotCC.Libc.dll')
                ET.SubElement(items, 'TrimmerRootAssembly', Include='MappingProbe')
                project = consumer / 'MappingProbe.csproj'
                ET.ElementTree(xml).write(project, encoding='unicode')
                if form == 'processed':
                    run(label + '-restore', ['dotnet', 'restore', project])
                    run(label + '-process', ['dotnet', inputs / 'post/dotcc-postprocess.dll', project, '--in-place'], 180)
                    shutil.copy2(inputs / 'Program.cs', consumer / 'Program.cs')
                run(label + '-build', ['dotnet', 'build', project, '-c', 'Release', '-m:1'], 180)
                binary = consumer / 'bin/Release/net10.0/MappingProbe.dll'
                before = inventory(binary.parent)
                if run(label + '-jit', ['dotnet', binary]) != expected or inventory(binary.parent) != before:
                    raise RuntimeError(label + ' JIT transcript/binary mismatch')
                if args.aot:
                    publish = consumer / 'publish'
                    run(label + '-publish', ['dotnet', 'publish', project, '-c', 'Release', '-m:1',
                        '-r', 'linux-x64', '-p:PublishAot=true', '-o', publish], 300)
                    before = inventory(publish)
                    if run(label + '-aot', [publish / 'MappingProbe']) != expected or inventory(publish) != before:
                        raise RuntimeError(label + ' AOT transcript/binary mismatch')
            if inventory(raw) != raw_before:
                raise RuntimeError('raw generated sources changed')
    if inventory(inputs) != receipt['frozen_inputs']:
        raise RuntimeError('frozen input changed')
    if any(sha(ROOT / name) != digest for name, digest in receipt['authored_inputs'].items()):
        raise RuntimeError('authored source changed')
    receipt['passed'] = True
finally:
    save()
    print(json.dumps(dict(passed=receipt['passed'], receipt=str(work / 'receipt.json'))))
