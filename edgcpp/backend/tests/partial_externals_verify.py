#!/usr/bin/env python3
"""Required partial runtime contracts: calls, pointers, native memory and owner storage."""
import argparse
import hashlib
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

    provider = out / 'shared-runtime.cs'
    provider.write_text('''namespace @Managed;
public unsafe partial class @Runtime {
private readonly int[] host_storage = global::System.GC.AllocateArray<int>(1, pinned: true);
private int* previous_address;
internal partial int host_add(int arg0) => arg0 + 10;
internal partial void* host_allocate(ulong arg0) {
    void* value = global::System.Runtime.InteropServices.NativeMemory.Alloc((nuint)arg0);
    if(value == null) throw new global::System.OutOfMemoryException();
    return value;
}
internal partial void host_release(void* arg0) {
    global::System.Runtime.InteropServices.NativeMemory.Free(arg0);
    global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
}
internal partial int* __edg_unbound_address_host_value() {
    int* address = (int*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref host_storage[0]);
    if(previous_address != null && previous_address != address)
        throw new global::System.InvalidOperationException("external storage moved");
    previous_address = address;
    return address;
}
// An implementation superset can be supplied to projects which never use this API.
internal partial int host_unused(int value) => Owner is null ? value * 7 : value * 8;
}
''')
    shared_digest = hashlib.sha256(provider.read_bytes()).hexdigest()
    checks = []
    projects = []
    collision = run('reserved-owner-rejected', [sys.executable, ROOT / 'dotnet.py',
        ROOT / 'backend/tests/partial_externals.cpp', '--compiler', args.compiler.resolve(),
        '--emit', 'managedlib', '--namespace', 'Managed', '--class-name', 'Runtime',
        '--unbound-externals', 'partial', '--output-dir', out / 'reserved-owner'], success=False)
    assert 'Managed.Runtime is reserved' in collision.stdout + collision.stderr
    checks.append('Managed.Runtime owner collision fails with an explicit reserved-name diagnostic')

    def build_host(label, directory, owner_type, exported_name, first, second, instance, aot=True):
        host = out / (label + '-host')
        host.mkdir()
        (host / 'Host.csproj').write_text(f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="{directory}/Generated.csproj" /></ItemGroup></Project>')
        body = (f'using var a = new {owner_type}(); using var b = new {owner_type}(); '
                f'return a.{exported_name}()=={first} && a.{exported_name}()=={second} && b.{exported_name}()=={first} ? 0 : 1;') if instance else (
                f'return {owner_type}.{exported_name}()=={first} && {owner_type}.{exported_name}()=={second} ? 0 : 1;')
        (host / 'Program.cs').write_text('unsafe class Program { static int Main() { ' + body + ' } }')
        run(label + '-build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(label + '-jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])
        checks.append(label + ': strict JIT direct/indirect extern calls and stable independent runtime storage')
        if args.nativeaot and aot:
            run(label + '-publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '--nologo', '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            run(label + '-aot', [host / 'aot/Host'])
            checks.append(label + ': NativeAOT execution')

    for instance in (False, True):
        label = 'instance' if instance else 'static'
        namespace = 'Different.InstanceBeta' if instance else 'External.StaticAlpha'
        # A colliding owner class name must not rename the reusable Runtime API.
        class_name = 'Beta' if instance else 'host_add'
        directory = out / label
        base = [sys.executable, ROOT / 'dotnet.py', ROOT / 'backend/tests/partial_externals.cpp',
                '--compiler', args.compiler.resolve(), '--emit', 'managedlib',
                '--namespace', namespace, '--class-name', class_name, '--split', 'source' if instance else 'none',
                '--unbound-externals', 'partial', '--output-dir', directory]
        if instance:
            base.append('--instance-methods')
        run(label + '-translate-missing', base)
        missing = run(label + '-missing-implementation', ['dotnet', 'build', directory / 'Generated.csproj', '-c', 'Release', '--nologo'], success=False)
        assert 'CS8795' in missing.stdout
        checks.append(label + ': missing implementations fail with CS8795')
        run(label + '-translate-bound', [*base, '--runtime-source', provider])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        assert manifest['unboundExternals']['requiresImplementation']
        assert not manifest['unboundExternals']['reviewOnly']
        assert len(manifest['externalRuntimeSources']) == 1
        assert manifest['externalRuntimeSources'][0]['sha256'] == shared_digest
        assert hashlib.sha256(provider.read_bytes()).hexdigest() == shared_digest
        code = '\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
        assert 'Unbound external function in review-only' not in code
        contract = (directory / 'Managed.Runtime.generated.cs').read_text().replace('@', '')
        assert 'partial int host_add' in contract and 'partial void host_release' in contract
        supplements = '\n'.join(p.read_text().replace('@', '') for p in directory.glob('Managed.Runtime.Supplement*.cs'))
        assert 'partial int host_unused' in supplements
        assert 'partial class Runtime' in contract and 'namespace Managed' in contract
        assert 'static partial' not in contract
        build_host(label, directory, namespace + '.' + class_name, 'check_external_runtime', 27, 28, instance)
        projects.append({'label': label, 'namespace': namespace, 'class': class_name,
                         'instance': instance, 'providerSha256': shared_digest})

    for instance in (False, True):
        label = 'typed-instance' if instance else 'typed-static'
        namespace = 'Contract.Flattened' if instance else 'Contract.Nested'
        class_name = 'LibraryTwo' if instance else 'LibraryOne'
        owner_type = namespace + '.' + class_name
        payload_type = namespace + '.' + class_name + '_Payload' if instance else owner_type + '.Payload'
        flavor_type = namespace + '.' + class_name + '_Flavor' if instance else owner_type + '.Flavor'
        handle_type = namespace + '.' + class_name + '_Handle' if instance else owner_type + '.Handle'
        alias_file = out / (label + '-aliases.cs')
        alias_file.write_text(f'global using RuntimePayload = global::{payload_type};\n'
                              f'global using RuntimeFlavor = global::{flavor_type};\n'
                              f'global using RuntimeHandle = global::{handle_type};\n'
                              f'global using RuntimeOwner = global::{owner_type};\n')
        modifier = '' if instance else 'static '
        owner_header = f'namespace {namespace}; public {("sealed" if instance else "static")} unsafe partial class {class_name} {{\n'
        typed_provider = out / (label + '-owner-runtime.cs')
        typed_provider.write_text(owner_header + '''
private MODreadonly RuntimePayload[] records = global::System.GC.AllocateArray<RuntimePayload>(1, pinned: true);
private MODpartial RuntimePayload host_transform(RuntimePayload arg0, RuntimeFlavor arg1)
    => new RuntimePayload { left = arg0.left + (int)arg0.right,
                            right = arg0.right + ((uint)arg1 == 0xf1234567u ? 100u : 3u) };
private MODpartial int host_inspect(RuntimePayload* arg0) {
    global::System.GC.Collect(2, global::System.GCCollectionMode.Forced, true, true);
    return arg0->left + (int)arg0->right;
}
private MODpartial RuntimeFlavor host_flavor(RuntimeFlavor arg0) => (RuntimeFlavor)0xf1234567u;
private MODpartial int host_opaque(RuntimeHandle* arg0) => arg0 == null ? 77 : 99;
private MODpartial RuntimePayload* __edg_unbound_address_host_record()
    => (RuntimePayload*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref records[0]);
}
'''.replace('MOD', modifier))
        record_callback_signature = ('delegate* managed<RuntimeOwner, RuntimePayload*, int>' if instance
                                     else 'delegate* managed<RuntimePayload*, int>')
        record_callback_arguments = 'this, &payload' if instance else '&payload'
        typed_provider.write_text(typed_provider.read_text().removesuffix('}\n') + f'''
private {modifier}partial int host_record_callback({record_callback_signature} arg0) {{
    RuntimePayload payload = new RuntimePayload {{ left = 4, right = 7 }};
    return arg0({record_callback_arguments});
}}
}}
''')
        callback_provider = out / (label + '-callback.cs')
        callback_signature = 'delegate* managed<RuntimeOwner, int, int>' if instance else 'delegate* managed<int, int>'
        callback_body = 'arg0(this, arg1) + 5' if instance else 'arg0(arg1) + 5'
        callback_header = owner_header if instance else 'namespace Managed; public unsafe partial class Runtime {\n'
        callback_access = 'private' if instance else 'internal'
        callback_provider.write_text(callback_header +
            f'{callback_access} partial int host_call({callback_signature} arg0, int arg1) => {callback_body};\n}}\n')
        directory = out / label
        run(label + '-translate', [sys.executable, ROOT / 'dotnet.py',
            ROOT / 'backend/tests/partial_external_contract_types.cpp', '--compiler', args.compiler.resolve(),
            '--emit', 'managedlib', '--namespace', namespace, '--class-name', class_name,
            '--split', 'none' if instance else 'source', '--unbound-externals', 'partial',
            '--runtime-source', provider, '--runtime-source', typed_provider, '--runtime-source', alias_file,
            '--runtime-source', callback_provider, '--output-dir', directory,
            *(['--instance-methods', '--no-nest-types'] if instance else [])])
        shared_contract = (directory / 'Managed.Runtime.generated.cs').read_text().replace('@', '')
        local_contract = (directory / 'Translated.Runtime.generated.cs').read_text().replace('@', '')
        assert all(name not in shared_contract for name in ('Payload', 'Flavor', 'Handle', 'LibraryOne', 'LibraryTwo', 'Contract.')), shared_contract
        assert 'partial int host_add' in shared_contract
        assert payload_type.rsplit('.', 1)[1] in local_contract and flavor_type.rsplit('.', 1)[1] in local_contract
        assert 'private ' + modifier + 'partial' in local_contract
        assert 'host_transform' in local_contract and '__edg_unbound_address_host_record' in local_contract
        assert ('host_call' in local_contract) == instance
        assert ('host_call' in shared_contract) != instance
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        locations = {item['symbol']: item for item in manifest['unboundExternals']['symbols']}
        for name in ('host_transform', 'host_inspect', 'host_flavor', 'host_record', 'host_opaque', 'host_record_callback'):
            assert locations[name]['placement'] == 'translated', locations[name]
            assert locations[name]['runtimeOwner'].replace('@', '').removeprefix('global::') == owner_type, locations[name]
            assert locations[name]['contractFile'] == 'Translated.Runtime.generated.cs', locations[name]
        assert locations['host_add']['placement'] == 'shared', locations['host_add']
        assert locations['host_call']['placement'] == ('translated' if instance else 'shared'), locations['host_call']
        build_host(label, directory, owner_type, 'check_external_contract_types', 20, 21, instance)
        projects.append({'label': label, 'namespace': namespace, 'class': class_name,
                         'instance': instance, 'nestedTypes': not instance,
                         'providerSha256': hashlib.sha256(typed_provider.read_bytes()).hexdigest(),
                         'projectSpecificAliases': alias_file.name,
                         'modeSpecificCallbackProvider': callback_provider.name})

    no_import_source = out / 'no-imports.cpp'
    no_import_source.write_text('extern "C" int no_imports() { return 37; }\n')
    directory = out / 'no-imports'
    run('no-imports-translate', [sys.executable, ROOT / 'dotnet.py', no_import_source,
        '--compiler', args.compiler.resolve(), '--emit', 'managedlib', '--namespace', 'Unrelated.Empty',
        '--class-name', 'Bare', '--unbound-externals', 'partial', '--runtime-source', provider,
        '--output-dir', directory])
    no_import_manifest = json.loads((directory / 'dot-gen.json').read_text())
    assert no_import_manifest['unboundExternals']['symbols'] == []
    assert no_import_manifest['externalRuntimeSources'][0]['sha256'] == shared_digest
    supplements = '\n'.join(p.read_text() for p in directory.glob('Managed.Runtime.Supplement*.cs'))
    assert 'partial int host_add' in supplements and 'partial int host_unused' in supplements
    build_host('no-imports', directory, 'Unrelated.Empty.Bare', 'no_imports', 37, 37, False, aot=False)
    checks[-1] = 'no-imports: exact same common provider compiles and runs when all implementations are unused'
    (out / 'results.json').write_text(json.dumps({'status': 'passed', 'nativeAot': args.nativeaot,
        'checks': checks, 'projects': projects,
        'compiler': {'path': str(args.compiler.resolve()),
                     'sha256': hashlib.sha256(args.compiler.resolve().read_bytes()).hexdigest()},
        'sharedProvider': {'path': str(provider), 'sha256': shared_digest},
        'reuseLimitations': ['Project-specific record/enum signatures are implemented on the translated owner, with matching namespace, type aliases and static/instance modifiers.',
                            'Incoming callbacks with an instance-owner parameter stay on the translated owner and use this; primitive static callbacks remain shared.']}, indent=2) + '\n')
    print('PASS: required partial extern implementations, static/instance JIT' + (' and NativeAOT' if args.nativeaot else ''))


if __name__ == '__main__':
    main()
