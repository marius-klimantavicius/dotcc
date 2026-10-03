#!/usr/bin/env python3
"""Required partial runtime contracts: calls, pointers, native memory and owner storage."""
import argparse
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append({'label': label, 'command': command, 'exitCode': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        assert (result.returncode == 0) == success, (label, result.stdout[-3000:], result.stderr[-3000:])
        return result

    for instance in (False, True):
        label = 'instance' if instance else 'static'
        directory = out / label
        base = [sys.executable, ROOT / 'dotnet.py', ROOT / 'backend/tests/partial_externals.cpp',
                '--compiler', args.compiler.resolve(), '--emit', 'managedlib',
                '--namespace', 'External.Test', '--class-name', 'Core', '--split', 'source',
                '--unbound-externals', 'partial', '--output-dir', directory]
        if instance:
            base.append('--instance-methods')
        run(label + '-translate-missing', base)
        missing = run(label + '-missing-implementation', ['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release', '--nologo'], success=False)
        assert 'CS8795' in missing.stdout
        mod = '' if instance else 'static '
        provider = out / (label + '-runtime.cs')
        provider.write_text('namespace External.Test; public ' + ('sealed' if instance else 'static') + ''' unsafe partial class Core {
private MODreadonly int[] host_storage = global::System.GC.AllocateArray<int>(1, pinned: true);
private MODpartial int host_add(int arg0) => arg0 + 10;
private MODpartial void* host_allocate(ulong arg0) {
    void* value = global::System.Runtime.InteropServices.NativeMemory.Alloc((nuint)arg0);
    if(value == null) throw new global::System.OutOfMemoryException();
    return value;
}
private MODpartial void host_release(void* arg0) => global::System.Runtime.InteropServices.NativeMemory.Free(arg0);
private MODpartial int* __edg_unbound_address_host_value() => (int*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref host_storage[0]);
}
'''.replace('MOD', mod))
        run(label + '-translate-bound', [*base, '--runtime-source', provider])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        assert manifest['unboundExternals']['requiresImplementation']
        assert not manifest['unboundExternals']['reviewOnly']
        assert len(manifest['externalRuntimeSources']) == 1
        code = '\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        assert 'partial int host_add' in code and 'partial void host_release' in code
        assert 'Unbound external function in review-only' not in code
        host = out / (label + '-host')
        host.mkdir()
        (host / 'Host.csproj').write_text(f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="{directory}/Generated.csproj" /></ItemGroup></Project>')
        body = ('using var a = new External.Test.Core(); using var b = new External.Test.Core(); '
                'return a.check_external_runtime()==27 && a.check_external_runtime()==28 && b.check_external_runtime()==27 ? 0 : 1;') if instance else (
                'return External.Test.Core.check_external_runtime()==27 && External.Test.Core.check_external_runtime()==28 ? 0 : 1;')
        (host / 'Program.cs').write_text('unsafe class Program { static int Main() { ' + body + ' } }')
        run(label + '-build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(label + '-jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        if args.nativeaot:
            run(label + '-publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:PublishAot=true', '-o', host / 'aot'])
            run(label + '-aot', [host / 'aot/Host'])
    (out / 'results.json').write_text(json.dumps({'status': 'passed', 'nativeAot': args.nativeaot,
        'checks': ['missing implementations fail compilation', 'direct and indirect partial calls',
                   'BCL native allocation/free', 'stable external storage addresses', 'independent instance storage']}, indent=2) + '\n')
    print('PASS: required partial extern implementations, static/instance JIT' + (' and NativeAOT' if args.nativeaot else ''))


if __name__ == '__main__':
    main()
