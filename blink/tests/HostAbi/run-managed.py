#!/usr/bin/env python3
"""Execute emitted host storage layouts; no host operations or guest CPU work."""
import difflib
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
BASE = ROOT / 'generated/host-abi'
BASE.mkdir(parents=True, exist_ok=True)
ATTEMPT = Path(tempfile.mkdtemp(prefix='attempt-', dir=BASE))
OUT = ROOT / 'artifacts/host-abi/managed' / ATTEMPT.name
OUT.mkdir(parents=True, exist_ok=True)
CLI = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
POSTPROCESS = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
ENV = dict(os.environ, LC_ALL='C')
receipt = dict(kind='executed-emitted-host-storage-not-host-runtime-or-guest-execution',
               host=platform.platform(), machine=platform.machine(),
               attempt=str(ATTEMPT.relative_to(ROOT)), results={}, passed=False)
TIMERS = '--timers' in sys.argv
RESOURCES = '--resources' in sys.argv
TIMES = '--times' in sys.argv
IOCTL = '--ioctl' in sys.argv
STATVFS = '--statvfs' in sys.argv
HEADER_MODE = TIMERS or RESOURCES or TIMES or IOCTL or STATVFS
FAMILY = 'statvfs' if STATVFS else 'ioctl' if IOCTL else 'resource' if RESOURCES else 'times' if TIMES else 'timer'
if TIMERS:
    receipt['kind'] = 'executed-emitted-timer-header-storage-not-host-runtime'
if RESOURCES:
    receipt['kind'] = 'executed-emitted-resource-header-storage-not-host-runtime'
if TIMES:
    receipt['kind'] = 'executed-emitted-process-times-header-storage-not-host-runtime'


if IOCTL:
    receipt['kind'] = 'executed-emitted-ioctl-header-storage-not-host-runtime'


if STATVFS:
    receipt['kind'] = 'executed-emitted-statvfs-header-storage-not-host-runtime'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save():
    (OUT / 'receipt.json').write_text(json.dumps(receipt, indent=2)+'\n')


def run(command, name, timeout=180):
    started = time.monotonic()
    with (OUT / (name + '.log')).open('wb') as output:
        result = subprocess.run(command, env=ENV, stdout=output,
                                stderr=subprocess.STDOUT, timeout=timeout)
    receipt['results'][name] = dict(command=list(map(str, command)), exitCode=result.returncode,
                                    seconds=time.monotonic()-started)
    save()
    if result.returncode:
        raise RuntimeError(f'{name} failed with exit {result.returncode}; see {OUT / (name + ".log")}')
    return (OUT / (name + '.log')).read_bytes()


def compare(command, name):
    actual = run(command, name, timeout=30)
    receipt['results'][name]['stdoutSha256'] = hashlib.sha256(actual).hexdigest()
    receipt['results'][name]['matchesNative'] = actual == expected
    if actual != expected:
        (OUT / (name + '.diff')).write_text(''.join(difflib.unified_diff(
            expected.decode().splitlines(True), actual.decode().splitlines(True),
            fromfile='native-profile', tofile=name)))
        raise RuntimeError(f'{name} differs from native profile')
    save()


try:
    source = ATTEMPT / 'source'
    source.mkdir()
    if HEADER_MODE:
        (source / 'sys').mkdir()
        shutil.copy2(ROOT / ('tests/HostAbi/' + FAMILY + '-probe.c'), source / 'probe.c')
        shutil.copy2(ROOT / 'config/managed-host/sys/time.h', source / 'sys/time.h')
        shutil.copy2(ROOT / 'config/managed-host/timer-constants.h', source / 'timer-constants.h')
        if RESOURCES:
            shutil.copy2(ROOT / 'config/managed-host/sys/resource.h', source / 'sys/resource.h')
            shutil.copy2(ROOT / 'config/managed-host/resource-constants.h', source / 'resource-constants.h')
        if TIMES:
            shutil.copy2(ROOT / 'config/managed-host/sys/times.h', source / 'sys/times.h')
        if IOCTL:
            shutil.copy2(ROOT / 'config/managed-host/sys/ioctl.h', source / 'sys/ioctl.h')
            shutil.copy2(ROOT / 'config/managed-host/ioctl-constants.h', source / 'ioctl-constants.h')
        if STATVFS:
            shutil.copy2(ROOT / 'config/managed-host/sys/statvfs.h', source / 'sys/statvfs.h')
    else:
        for original in [ROOT / 'tests/HostAbi/probe.c', ROOT / 'config/managed-host/abi.h',
                         ROOT / 'config/managed-host/retained-thread-types.h']:
            shutil.copy2(original, source / original.name)
    receipt['inputs'] = {str(p.relative_to(source)):sha(p) for p in source.rglob('*') if p.is_file()}
    compiler_files = [CLI, CLI.with_name('DotCC.Lib.dll')]
    receipt['compiler'] = {p.name:sha(p) for p in compiler_files}
    receipt['postprocessorSha256'] = sha(POSTPROCESS)
    save()
    native = ATTEMPT / 'native-profile'
    run(['cc', '-std=c17', '-DBLINK_HOST_STORAGE_ONLY', '-I' if HEADER_MODE else '-iquote', str(source),
         str(source / 'probe.c'), '-o', str(native)], 'native-build')
    expected = run([str(native)], 'native-profile', timeout=30)
    # Original two-column native-system/profile evidence remains separately
    # reproducible through run.py. Check common cases against this execution.
    native_system = ATTEMPT / 'native-system'
    run(['cc', '-std=c17', '-iquote', str(source), str(source / 'probe.c'),
         '-o', str(native_system)], 'native-system-build')
    system_rows = run([str(native_system)], 'native-system', timeout=30).decode().splitlines()
    profile_values = dict(line.rsplit(' ', 1) for line in expected.decode().splitlines())
    for line in system_rows:
        if HEADER_MODE:
            name, system = line.rsplit(' ', 1)
            profile = profile_values[name]
        else:
            name, system, profile = line.rsplit(' ', 2)
        if system != profile or profile_values[name] != profile:
            raise RuntimeError(f'native system/profile comparison differs: {name}')
    receipt['caseCount'] = len(profile_values)
    receipt['nativeSystemCases'] = len(system_rows)
    receipt['nativeBinarySha256'] = sha(native)
    raw = ATTEMPT / 'raw' / 'HostAbiStorage'
    run(['dotnet', str(CLI), '-std=c17', '-DBLINK_HOST_STORAGE_ONLY', '-I', str(source),
         str(source / 'probe.c'), '--runtime=c', '-o', str(raw)], 'translate')
    if receipt['compiler'] != {p.name:sha(p) for p in compiler_files}:
        raise RuntimeError('compiler changed during emission; retry with a stable compiler')
    receipt['rawGenerated'] = {str(p.relative_to(raw)):sha(p) for p in raw.rglob('*.cs')}
    optimized = ATTEMPT / 'optimized' / 'HostAbiStorage'
    shutil.copytree(raw, optimized)
    project = raw / 'HostAbiStorage.csproj'
    run(['dotnet', 'build', str(project), '-c', 'Release'], 'raw-jit-build')
    compare(['dotnet', str(raw / 'bin/Release/net10.0/HostAbiStorage.dll')], 'raw-jit')
    raw_publish = ATTEMPT / 'raw-aot'
    run(['dotnet', 'publish', str(project), '-c', 'Release', '-r', 'linux-x64',
         '-p:PublishAot=true', '-o', str(raw_publish)], 'raw-aot-build', timeout=300)
    compare([str(raw_publish / 'HostAbiStorage')], 'raw-aot')
    optimized_project = optimized / 'HostAbiStorage.csproj'
    run(['dotnet', 'restore', str(optimized_project)], 'optimized-restore')
    run(['dotnet', str(POSTPROCESS), str(optimized_project), '--in-place'], 'postprocess')
    receipt['optimizedGenerated'] = {str(p.relative_to(optimized)):sha(p)
                                     for p in optimized.rglob('*.cs') if 'obj' not in p.parts}
    run(['dotnet', 'build', str(optimized_project), '-c', 'Release'], 'optimized-jit-build')
    compare(['dotnet', str(optimized / 'bin/Release/net10.0/HostAbiStorage.dll')], 'optimized-jit')
    optimized_publish = ATTEMPT / 'optimized-aot'
    run(['dotnet', 'publish', str(optimized_project), '-c', 'Release', '-r', 'linux-x64',
         '-p:PublishAot=true', '-o', str(optimized_publish)], 'optimized-aot-build', timeout=300)
    compare([str(optimized_publish / 'HostAbiStorage')], 'optimized-aot')
    receipt['passed'] = True
    save()
    (ROOT / ('artifacts/host-abi/' + FAMILY + '-managed-latest.json' if HEADER_MODE else 'artifacts/host-abi/managed-latest.json')).write_text(json.dumps(
        {'receipt':str((OUT / 'receipt.json').relative_to(ROOT))}, indent=2)+'\n')
    print(f'{receipt["caseCount"]} authored ABI outputs match native under raw/optimized JIT/NativeAOT. Receipt: {OUT / "receipt.json"}')
except Exception as error:
    receipt['error'] = str(error)
    save()
    raise
