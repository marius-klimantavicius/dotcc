#!/usr/bin/env python3
"""Focused translation of authored waiter selection; no shared product builds."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()


def main():
    base = ROOT / 'generated/guest-memory-futex-queue'
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    print(attempt, flush=True)
    receipt = dict(passed=False, scope='isolated authored queue selection, not translated guest execution',
                   commands=[], inputs={})
    def save():
        (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
    def run(name, args, timeout=120):
        with (attempt / (name + '.log')).open('wb') as output:
            result = subprocess.run(list(map(str, args)), cwd=ROOT.parent,
                env=dict(os.environ, LC_ALL='C', MSBUILDDISABLENODEREUSE='1'),
                stdout=output, stderr=subprocess.STDOUT, timeout=timeout)
        receipt['commands'].append(dict(name=name, command=list(map(str, args)), exit_code=result.returncode))
        save()
        if result.returncode: raise RuntimeError(name + ' failed: ' + str(attempt / (name + '.log')))
        print(name + ': passed', flush=True)
        return (attempt / (name + '.log')).read_bytes()
    try:
        upstream = ROOT / 'ref' / json.loads((ROOT / 'config/source-manifest.json').read_text())['upstream']['directory']
        cli = ROOT.parent / 'DotCC/bin/Release/net10.0/dotcc.dll'
        include = attempt / 'include'; include.mkdir()
        (include / 'config.h').write_text('#define DISABLE_JIT 1\n#define NOLINEAR 1\n')
        inputs = [HERE / 'queue.c', ROOT / 'src/UpstreamGuestThreads/futex-wake.inc', upstream / 'blink/dll.c', upstream / 'blink/pte32.c',
                  Path(__file__), cli, cli.with_name('DotCC.Lib.dll'), include / 'config.h']
        receipt['inputs'] = {str(p): sha(p) for p in inputs}
        args = ['-std=c17', '-I', include, '-I', upstream, '-I', ROOT / 'src/UpstreamGuestThreads',
                HERE / 'queue.c', upstream / 'blink/dll.c', upstream / 'blink/pte32.c']
        run('native-build', ['cc', *args, '-pthread', '-o', attempt / 'queue-native'])
        expected = run('native', [attempt / 'queue-native'])
        library = attempt / 'library'
        run('translate', ['dotnet', cli, *args, '--runtime=c', '--emit=managedlib', '--nest-types',
                           '--class-name', 'FutexQueue', '--namespace', 'Focused', '-o', library])
        consumer = attempt / 'consumer'; consumer.mkdir()
        (consumer / 'Program.cs').write_text('return Focused.FutexQueue.main();\n')
        project = consumer / 'Queue.csproj'
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
            '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup>'
            '<ItemGroup><ProjectReference Include="../library/library.csproj" /></ItemGroup></Project>')
        run('managed-build', ['dotnet', 'build', project, '-c', 'Release', '--disable-build-servers', '-p:UseSharedCompilation=false'])
        actual = run('managed-jit', ['dotnet', consumer / 'bin/Release/net10.0/Queue.dll'])
        if actual != expected: raise RuntimeError('JIT output differs')
        run('aot-build', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true',
                         '--disable-build-servers', '-p:UseSharedCompilation=false', '-o', attempt / 'aot'], 300)
        if run('managed-aot', [attempt / 'aot/Queue']) != expected: raise RuntimeError('AOT output differs')
        if any(sha(p) != digest for p, digest in receipt['inputs'].items()): raise RuntimeError('Input changed')
        receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    finally:
        receipt['completed'] = True
        save()
    print('receipt=' + str(attempt / 'receipt.json'), flush=True)
    return 0 if receipt['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
