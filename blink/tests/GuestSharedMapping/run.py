#!/usr/bin/env python3
"""Focused isolated checks; never translates/builds the shared Blink delivery."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import tempfile
import time

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    base = ROOT / 'artifacts/coreclr-coordination'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='memory-focused-', dir=base))
    receipt = dict(passed=False, scope='isolated authored host/bridge C# and C owner; Linux guest fixture oracle; no managed Blink execution',
                   inputs={}, commands=[])
    env = dict(os.environ, LC_ALL='C', DOTNET_CLI_TELEMETRY_OPTOUT='1')

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def pin(path):
        path = Path(path).resolve()
        digest = sha(path)
        if receipt['inputs'].setdefault(str(path), digest) != digest:
            raise RuntimeError('Input changed: ' + str(path))
        return path

    def run(name, args, timeout=120, expected=0):
        row = dict(name=name, command=list(map(str, args)), cwd=str(attempt), expected=expected)
        receipt['commands'].append(row)
        started = time.monotonic()
        with (attempt / (name + '.log')).open('wb') as log:
            completed = subprocess.run(row['command'], cwd=attempt, env=env,
                                       stdout=log, stderr=subprocess.STDOUT, timeout=timeout)
        row.update(exit=completed.returncode, seconds=time.monotonic() - started)
        save()
        if completed.returncode != expected:
            raise RuntimeError(name + ': see ' + str(attempt / (name + '.log')))
        print(name + ': passed', flush=True)

    try:
        for path in HERE.glob('*'):
            if path.is_file(): pin(path)
        host = attempt / 'host'
        shutil.copytree(ROOT / 'src/Managed.Emulation.Host', host, ignore=shutil.ignore_patterns('bin', 'obj'))
        for path in host.glob('*.cs'): pin(path)
        for name in ('HostMemory.c', 'HostFileMappingBridge.cs'):
            path = pin(ROOT / 'src/Host' / name)
            shutil.copyfile(path, attempt / name)
        for name in ('HostMemory.h',):
            shutil.copyfile(pin(ROOT / 'src/Host/include' / name), attempt / name)
        for name in ('Program.cs', 'owner.c', 'fixture.c'):
            shutil.copyfile(HERE / name, attempt / name)
        (attempt / 'probe.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
<ItemGroup><Compile Include="Program.cs"/><Compile Include="HostFileMappingBridge.cs"/>
<ProjectReference Include="host/Managed.Emulation.Host.csproj"/></ItemGroup></Project>''')
        # Isolated copied host library only. No compiler tools, generated sources,
        # shared obj/bin, or published delivery are touched.
        run('managed-build', ['dotnet', 'build', attempt / 'probe.csproj', '-c', 'Release', '--nologo'])
        run('managed-host', ['dotnet', attempt / 'bin/Release/net10.0/probe.dll'])
        for threaded in (False, True):
            label = 'owner-threaded' if threaded else 'owner-legacy'
            run(label + '-build', ['cc', '-std=c17', '-Wall', '-Wextra', '-Werror',
                *(['-DBLINK_MANAGED_GUEST_THREADS', '-pthread'] if threaded else []),
                '-I', attempt, attempt / 'owner.c', attempt / 'HostMemory.c', '-o', attempt / label])
            run(label, [attempt / label])
        run('guest-build', ['cc', '-std=c17', '-O2', '-Wall', '-Wextra', '-Werror', '-nostdlib', '-static',
                           '-fno-stack-protector', '-fno-pie', '-no-pie', '-Wl,-e,_start', '-Wl,-z,noexecstack',
                           attempt / 'fixture.c', '-o', attempt / 'guest-memfd'])
        run('guest-linux', [attempt / 'guest-memfd'])
        pin(attempt / 'guest-memfd')
        # Exercise pure authored staging transforms against pinned archive bytes;
        # do not write a shared staging tree or invoke the translator.
        manifest = json.loads(pin(ROOT / 'config/source-manifest.json').read_text())['upstream']
        archive = pin(ROOT / 'ref' / manifest['archive'])
        if sha(archive) != manifest['sha256']: raise RuntimeError('Archive pin differs')
        with tarfile.open(archive) as stream:
            original = {name: stream.extractfile(manifest['directory'] + '/blink/' + name).read()
                        for name in ('syscall.c', 'memorymalloc.c')}
        path = pin(ROOT / 'scripts/memory_bindings.py')
        spec = importlib.util.spec_from_file_location(path.stem, path)
        bindings = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(bindings)
        for relative in bindings.INPUTS: pin(ROOT / relative)
        # Model the exact early sharing guard emitted by the thread predecessor.
        # Full thread/futex staging needs its own preceding signal boundaries and
        # belongs to the coordinator's fresh integration run, not this test.
        guard = ('  if ((flags & MAP_TYPE_LINUX) == MAP_SHARED_LINUX ||\n'
                 '      (flags & MAP_TYPE_LINUX) == (MAP_SHARED_LINUX | MAP_PRIVATE_LINUX)) {\n'
                 '    return enotsup();\n  }\n'
                 '  if ((flags & MAP_TYPE_LINUX) != MAP_PRIVATE_LINUX) return einval();\n')
        anchor = '  if (flags & MAP_GROWSDOWN_LINUX) return enotsup();\n'
        if original['syscall.c'].decode().count(anchor) != 1: raise RuntimeError('Guard anchor differs')
        syscall = original['syscall.c'].decode().replace(anchor, guard + anchor)
        for label, body in [('base', original['syscall.c']), ('threaded', syscall.encode())]:
            adapted = bindings.adapt('blink/syscall.c', body).decode()
            if adapted.count('"memfd_create", SysMemfdCreate') != 1: raise RuntimeError('Missing memfd syscall')
            if 'if (!rc) InvalidateSystem(m->system, true, true);' not in adapted:
                raise RuntimeError('Missing truncate instruction-cache invalidation')
            if label == 'threaded' and 'if ((flags & MAP_TYPE_LINUX) != MAP_PRIVATE_LINUX &&' not in adapted:
                raise RuntimeError('Shared guard unchanged')
            (attempt / (label + '-syscall.c')).write_text(adapted)
        adapted = bindings.adapt('blink/memorymalloc.c', original['memorymalloc.c'])
        (attempt / 'memorymalloc.c').write_bytes(adapted)
        receipt['staging'] = dict(passed=True, memory_sha256=hashlib.sha256(adapted).hexdigest())
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Frozen input changed: ' + path)
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = type(error).__name__ + ': ' + str(error)
    finally:
        receipt['completed'] = True
        save()
    print('receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
