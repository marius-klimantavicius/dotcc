#!/usr/bin/env python3
"""Retain SDK-10.0.112 IL apps, runtime-10.0.12 and actual ELF dependencies; run native oracles."""
import argparse
import http.client
import json
import os
from pathlib import Path
import re
import shutil
import socket
import sys
import time
from evidence import (Evidence, HERE, DOTNET, SDK, VERSION, HOST_COMMIT, ENVIRONMENT, EXPECTED, manifest, sha)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--dotnet-root', type=Path, default=DOTNET.parent)
    args = parser.parse_args()
    ev = Evidence('native'); receipt = ev.value
    receipt['blink_execution'] = False
    receipt['native_runtime_execution'] = True
    receipt['runtime'] = dict(sdk=SDK, version=VERSION, host_commit=HOST_COMMIT)
    try:
        ev.authored()
        dotnet = ev.pin(args.dotnet_root / 'dotnet')
        info, _ = ev.run('installed-info', [dotnet, '--info'], cwd=HERE)
        if not all(text.encode() in info for text in (SDK, VERSION, HOST_COMMIT)):
            raise RuntimeError('Installed SDK/runtime identity differs')
        for framework in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App'):
            if not (args.dotnet_root / 'shared' / framework / VERSION).is_dir(): raise RuntimeError('Missing pinned framework')
        fixture_sources = ev.directory / 'fixtures'
        shutil.copytree(HERE, fixture_sources, ignore=shutil.ignore_patterns('bin', 'obj', '__pycache__'))
        images = ev.directory / 'images'; images.mkdir()
        for fixture, assembly in (('console', 'CoreClrConsole'), ('service', 'CoreClrService')):
            project = fixture_sources / fixture.title() / (fixture.title() + '.csproj')
            ev.run(fixture + '-publish-il', [dotnet, 'publish', project, '-c', 'Release', '--no-self-contained',
                '--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false', '-p:PublishAot=false',
                '-p:PublishReadyToRun=false', '-p:UseAppHost=false', '-p:RollForward=Disable',
                '-p:PathMap=' + str(fixture_sources) + '=/_/CoreClrRuntime',
                '-o', images / 'apps' / fixture], timeout=180, cwd=fixture_sources)
            runtime = json.loads((images / 'apps' / fixture / (assembly + '.runtimeconfig.json')).read_text())['runtimeOptions']
            frameworks = runtime.get('frameworks', [runtime.get('framework')])
            if runtime.get('rollForward') != 'Disable' or any(f['version'] != VERSION for f in frameworks):
                raise RuntimeError('App framework pin differs: ' + fixture)
        retained = images / 'dotnet'
        def copy(source, target):
            source = Path(source)
            ev.pin(source)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
            target.chmod(source.stat().st_mode & 0o777)
        copy(dotnet, retained / 'dotnet')
        runtime_inputs = [args.dotnet_root / 'host/fxr' / VERSION,
            *[args.dotnet_root / 'shared' / f / VERSION for f in ('Microsoft.NETCore.App', 'Microsoft.AspNetCore.App')]]
        for folder in runtime_inputs:
            for source in sorted(folder.rglob('*')):
                if source.is_file(): copy(source, retained / source.relative_to(args.dotnet_root))
        native_objects = [retained / 'dotnet', *retained.rglob('*.so')]
        ldd = ev.pin(shutil.which('ldd'))
        deps, _ = ev.run('runtime-ldd', [ldd, *native_objects], timeout=60)
        if b'not found' in deps: raise RuntimeError('Installed native runtime dependency missing')
        resolved = sorted(set(re.findall(r'(?:=>\s+|^\s*)(/\S+)\s+\(', deps.decode(), flags=re.M)))
        # ldd also reports runtime-local DT_NEEDED entries. Those bytes are
        # already under /dotnet; never import an attempt's host path into the
        # guest namespace as an accidental duplicate.
        dependencies = [p for p in resolved if not Path(p).is_relative_to(retained)]
        receipt['runtime_local_dependencies'] = {'/dotnet/' + Path(p).relative_to(retained).as_posix(): sha(p)
            for p in resolved if Path(p).is_relative_to(retained)}
        if not {'libc.so.6', 'libstdc++.so.6', 'libgcc_s.so.1', 'ld-linux-x86-64.so.2'} <= {Path(p).name for p in dependencies}:
            raise RuntimeError('Incomplete libc/loader/runtime closure')
        for name in dependencies: copy(name, images / name.lstrip('/'))
        readelf = ev.pin(shutil.which('readelf'))
        header, _ = ev.run('dotnet-elf', [readelf, '-h', '-lW', retained / 'dotnet'])
        interp = re.search(r'Requesting program interpreter: ([^\]]+)\]', header.decode()).group(1)
        if not (images / interp.lstrip('/')).is_file(): copy(interp, images / interp.lstrip('/'))
        (images / 'work').mkdir(); (images / 'work/input.txt').write_bytes(b'guest-input\n')
        (images / 'tmp').mkdir(); (images / 'tmp/.keep').write_bytes(b'')
        files = [dict(GuestPath='/' + name, File=name, Sha256=digest,
            Executable=name == 'dotnet/dotnet' or name == interp.lstrip('/')) for name, digest in manifest(images).items()]
        closure = ev.directory / 'closure.json'
        closure.write_text(json.dumps(dict(Images=str(images), Files=files, Environment=ENVIRONMENT), indent=2) + '\n')
        ev.pin(closure)
        receipt.update(closure=str(closure), image_files=manifest(images), libraries={p: sha(p) for p in dependencies},
            environment=ENVIRONMENT, interpreter=interp, native_cases=[])
        for row in files: ev.pin(images / row['File'], row['Sha256'])
        native_env = dict(ENVIRONMENT, DOTNET_ROOT=str(retained),
            LD_LIBRARY_PATH=':'.join(str(images / p.lstrip('/')) for p in ENVIRONMENT['LD_LIBRARY_PATH'].split(':')),
            TMPDIR=str(ev.directory / 'native-tmp'))
        Path(native_env['TMPDIR']).mkdir()
        native_work = ev.directory / 'native-work'; native_work.mkdir()
        (native_work / 'input.txt').write_bytes(b'guest-input\n')
        # The kernel loads the PT_INTERP named by dotnet. Its host image is
        # pinned above and byte-identical to the retained guest interpreter.
        # Invoking ld-linux directly changes /proc/self/exe and dotnet rejects
        # its resulting basename (retained failed attempt documents this).
        prefix = [retained / 'dotnet']
        receipt['native_loader'] = dict(path=interp, sha256=sha(interp), retained_sha256=sha(images / interp.lstrip('/')))
        for scenario in ('smoke', 'workload'):
            arguments = [images / 'apps/console/CoreClrConsole.dll', scenario]
            if scenario == 'workload': arguments.append(native_work)
            stdout, stderr = ev.run('native-' + scenario, [*prefix, *arguments], cwd=native_work, env=native_env)
            if stdout != EXPECTED[scenario] or stderr: raise RuntimeError('Native output differs: ' + scenario)
            if scenario == 'workload' and (native_work / 'output.txt').read_bytes() != b'guest-output\nappend\n':
                raise RuntimeError('Native file round trip differs')
            receipt['native_cases'].append(dict(scenario=scenario, passed=True))
        with socket.socket() as reserve:
            reserve.bind(('127.0.0.1', 0)); port = reserve.getsockname()[1]
        handle = ev.start('native-service', [*prefix, images / 'apps/service/CoreClrService.dll', port],
            timeout=60, cwd=native_work, env=native_env)
        responses = []
        try:
            end = time.monotonic() + 30
            while b'SERVICE READY\n' not in Path(handle[1]['stdout']).read_bytes():
                if handle[0].poll() is not None: raise RuntimeError('Native service exited before readiness')
                if time.monotonic() > end: raise TimeoutError('Native service readiness')
                time.sleep(.025)
            for method, path, body, expected in [('GET', '/health', '', 'CORECLR SERVICE OK\n'),
                    ('POST', '/echo', 'payload', 'CORECLR:payload\n'), ('POST', '/stop', '', 'STOP OK\n')]:
                connection = http.client.HTTPConnection('127.0.0.1', port, timeout=10)
                try:
                    connection.request(method, path, body=body, headers={'Connection': 'close'})
                    response = connection.getresponse(); actual = response.read().decode()
                    responses.append(dict(path=path, status=response.status, body=actual, headers=response.getheaders()))
                    if response.status != 200 or actual != expected: raise RuntimeError('Native HTTP mismatch')
                finally: connection.close()
        finally:
            stdout, stderr = ev.finish_command(handle)
        if stdout != EXPECTED['service'] or stderr: raise RuntimeError('Native service output differs')
        receipt['native_cases'].append(dict(scenario='service', passed=True, responses=responses))
        if manifest(images) != receipt['image_files']: raise RuntimeError('Retained image changed')
        receipt['passed'] = True
    except BaseException as error: receipt['error'] = repr(error)
    return ev.finish()


if __name__ == '__main__': raise SystemExit(main())
