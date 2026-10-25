#!/usr/bin/env python3
"""Private host/bridge/owner checks and the ordinary Linux regular-file guest.

No translation/shared build; receipt distinguishes these checks from translated
guest qualification, which requires the parent's next strict producer.
"""
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import time

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    base = HERE / '.artifacts'
    base.mkdir(exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='focused-', dir=base))
    receipt = dict(passed=False, completed=False, inputs={}, commands=[],
        scope='Private authored InstanceIo/bridge and ordinary Linux guest; no translated guest qualification',
        native_blink_oracle_resolved=False,
        prior_coordinator_termination='Automated content filter citing possible cyber risk remains recorded; no safeguard workaround')
    env = dict(os.environ, LC_ALL='C', DOTNET_PROCESSOR_COUNT='2', MSBUILDDISABLENODEREUSE='1',
        DOTNET_CLI_USE_MSBUILD_SERVER='0', DOTNET_CLI_TELEMETRY_OPTOUT='1')
    print('attempt=' + str(attempt), flush=True)

    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')

    def capture(source, target):
        source = Path(source).resolve()
        digest = sha(source)
        if receipt['inputs'].setdefault(str(source), digest) != digest:
            raise RuntimeError('Input changed: ' + str(source))
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        if sha(source) != digest or sha(target) != digest:
            raise RuntimeError('Capture changed: ' + str(source))
        return target

    def run(name, args, timeout=120):
        row = dict(name=name, command=list(map(str, args)), timeout=timeout)
        receipt['commands'].append(row)
        save()
        with (attempt / (name + '.log')).open('wb') as log:
            process = subprocess.Popen(row['command'], cwd=attempt, env=env, stdin=subprocess.DEVNULL,
                stdout=log, stderr=subprocess.STDOUT, start_new_session=True)
            try:
                row['exit'] = process.wait(timeout=timeout)
            finally:
                sent = []
                for sig in (signal.SIGTERM, signal.SIGKILL):
                    try:
                        os.killpg(process.pid, 0)
                    except ProcessLookupError:
                        break
                    os.killpg(process.pid, sig)
                    sent.append(sig.name)
                    end = time.monotonic() + 2
                    while time.monotonic() < end:
                        process.poll()
                        try: os.killpg(process.pid, 0)
                        except ProcessLookupError: break
                        time.sleep(.025)
                process.wait(timeout=3)
                try: os.killpg(process.pid, 0); gone = False
                except ProcessLookupError: gone = True
                row['cleanup'] = dict(signals=sent, group_gone=gone)
                save()
        if row.get('exit') != 0 or sent or not gone:
            raise RuntimeError(name + ': ' + str(attempt / (name + '.log')))
        print(name + ': passed', flush=True)

    try:
        for name in ('run.py', 'Program.cs', 'fixture.c', 'owner.c'):
            capture(HERE / name, attempt / name)
        for source in sorted((ROOT / 'src/Managed.Emulation.Host').glob('*')):
            if source.is_file(): capture(source, attempt / 'host' / source.name)
        capture(ROOT / 'src/Host/HostFileMappingBridge.cs', attempt / 'HostFileMappingBridge.cs')
        for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'nuget.config'):
            if (ROOT.parent / name).is_file(): capture(ROOT.parent / name, attempt / name)
        (attempt / 'probe.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
<ItemGroup><Compile Include="Program.cs"/><Compile Include="HostFileMappingBridge.cs"/>
<ProjectReference Include="host/Managed.Emulation.Host.csproj"/></ItemGroup></Project>''')
        run('managed-build', ['dotnet', 'build', attempt / 'probe.csproj', '-c', 'Release', '--nologo',
            '--disable-build-servers', '-m:2', '-p:UseSharedCompilation=false', '-p:BuildInParallel=false'])
        run('managed-host', ['dotnet', attempt / 'bin/Release/net10.0/probe.dll'])
        capture(ROOT / 'src/Host/HostMemory.c', attempt / 'HostMemory.c')
        capture(ROOT / 'src/Host/include/HostMemory.h', attempt / 'HostMemory.h')
        for threaded in (False, True):
            label = 'owner-threaded' if threaded else 'owner-legacy'
            run(label + '-build', ['cc', '-std=c17', '-Wall', '-Wextra', '-Werror',
                *(['-DBLINK_MANAGED_GUEST_THREADS', '-pthread'] if threaded else []),
                '-I', attempt, attempt / 'owner.c', attempt / 'HostMemory.c', '-o', attempt / label])
            run(label, [attempt / label])
        linker = capture(ROOT / 'tests/GuestThreads/fixture.ld', attempt / 'fixture.ld')
        run('guest-build', ['cc', '-std=c17', '-O2', '-Wall', '-Wextra', '-Werror', '-nostdlib', '-static',
            '-fno-stack-protector', '-fno-pie', '-no-pie', '-Wl,-e,_start', '-Wl,-z,noexecstack',
            '-Wl,-T,' + str(linker), attempt / 'fixture.c', '-o', attempt / 'guest-regular-shared'])
        run('guest-linux', [attempt / 'guest-regular-shared'])
        receipt['guest_elf_sha256'] = sha(attempt / 'guest-regular-shared')
        receipt['expected_stdout'] = 'guest-regular-shared: coherence/identity/private/EOF/permissions: PASS\n'
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
