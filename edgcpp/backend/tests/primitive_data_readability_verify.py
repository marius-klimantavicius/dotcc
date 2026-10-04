"""Differential typed primitive initializers and PE-backed static span data checks."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys


def audit_generated(directory):
    text = '\n'.join(p.read_text() for p in sorted(directory.glob('Translated*.cs')))
    properties = re.findall(r'private static (?:global::System\.)?ReadOnlySpan<([^>]+)>\s+(__edg_initializer\w*)\s*=>', text)
    assert properties, 'initializer data must be static span properties'
    kinds = {kind for kind, name in properties}
    assert {'short', 'ushort', 'int', 'uint', 'long', 'ulong', 'global::System.Half', 'float', 'double'} <= kinds, kinds
    assert '1116352408U' in text or '1116352408u' in text
    assert '.CopyTo(' in text and 'MemoryMarshal.Cast<' in text
    return {'property_count': len(properties), 'element_types': sorted(kinds), 'generated_bytes': len(text.encode())}


def build_pe_inspector(out, run):
    directory = out / 'pe-inspector'
    directory.mkdir()
    (directory / 'Inspector.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup></Project>''')
    # Reflection is confined to this test tool for reading IL opcode metadata.
    # It is not linked into the generated library or its NativeAOT host.
    (directory / 'Program.cs').write_text(r'''using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.Json;

using var stream = File.OpenRead(args[0]);
using var pe = new PEReader(stream);
var metadata = pe.GetMetadataReader();
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(o => unchecked((ushort)o.Value));
var getters = metadata.MethodDefinitions.Where(h =>
    metadata.GetString(metadata.GetMethodDefinition(h).Name).StartsWith("get___edg_initializer", StringComparison.Ordinal)).ToHashSet();
if (getters.Count == 0) throw new Exception("No initializer getters in generated assembly");
var direct = new Dictionary<MethodDefinitionHandle, HashSet<FieldDefinitionHandle>>();
var calls = new Dictionary<MethodDefinitionHandle, HashSet<MethodDefinitionHandle>>();
string OwnerName(EntityHandle owner) {
    if (owner.Kind == HandleKind.TypeSpecification) {
        var reader = metadata.GetBlobReader(metadata.GetTypeSpecification((TypeSpecificationHandle)owner).Signature);
        if (reader.ReadByte() != 0x15) return "other-type-specification"; // GENERICINST
        reader.ReadByte(); // VALUETYPE or CLASS
        owner = reader.ReadTypeHandle();
    }
    if (owner.Kind == HandleKind.TypeReference) {
        var type = metadata.GetTypeReference((TypeReferenceHandle)owner);
        return metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
    }
    if (owner.Kind == HandleKind.TypeDefinition) {
        var type = metadata.GetTypeDefinition((TypeDefinitionHandle)owner);
        return metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name);
    }
    return owner.Kind.ToString();
}
foreach (var handle in getters) {
    var method = metadata.GetMethodDefinition(handle);
    string name = metadata.GetString(method.Name);
    if ((method.Attributes & MethodAttributes.Static) == 0) throw new Exception(name + " is not static");
    byte[] il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? throw new Exception("Missing IL");
    var fields = direct[handle] = new();
    var children = calls[handle] = new();
    for (int position = 0; position < il.Length;) {
        ushort code = il[position++];
        if (code == 0xfe) code = (ushort)(0xfe00 | il[position++]);
        OpCode op = opcodes[code];
        int count = op.OperandType switch {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineI or OperandType.ShortInlineBrTarget or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + 4 * BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(position)),
            _ => 4
        };
        if (op == OpCodes.Newarr || op == OpCodes.Box) throw new Exception(name + " allocates with " + op.Name);
        if (op.OperandType is OperandType.InlineField or OperandType.InlineTok or OperandType.InlineMethod) {
            EntityHandle target = MetadataTokens.EntityHandle(BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(position)));
            if (target.Kind == HandleKind.MethodSpecification)
                target = metadata.GetMethodSpecification((MethodSpecificationHandle)target).Method;
            if (target.Kind == HandleKind.FieldDefinition) {
                var field = metadata.GetFieldDefinition((FieldDefinitionHandle)target);
                if (field.GetRelativeVirtualAddress() != 0) fields.Add((FieldDefinitionHandle)target);
            }
            if (target.Kind == HandleKind.MethodDefinition) {
                var child = (MethodDefinitionHandle)target;
                if (!getters.Contains(child)) throw new Exception(name + " calls non-initializer method");
                children.Add(child);
            }
            if (target.Kind == HandleKind.MemberReference) {
                var member = metadata.GetMemberReference((MemberReferenceHandle)target);
                string owner = OwnerName(member.Parent), called = metadata.GetString(member.Name);
                bool allowed = (owner == "System.Runtime.CompilerServices.RuntimeHelpers" && called == "CreateSpan")
                    || (owner == "System.Runtime.InteropServices.MemoryMarshal" && called == "Cast")
                    || (owner == "System.ReadOnlySpan`1" && called == ".ctor");
                if (!allowed) throw new Exception(name + " calls unaudited external operation " + owner + "::" + called);
                if (op == OpCodes.Newobj && owner != "System.ReadOnlySpan`1")
                    throw new Exception(name + " allocates a managed object");
            }
        }
        position += count;
    }
}
HashSet<FieldDefinitionHandle> Reach(MethodDefinitionHandle method, HashSet<MethodDefinitionHandle> path) {
    if (!path.Add(method)) throw new Exception("Cyclic initializer getters");
    var result = new HashSet<FieldDefinitionHandle>(direct[method]);
    foreach (var child in calls[method]) result.UnionWith(Reach(child, path));
    path.Remove(method);
    return result;
}
var reachable = new HashSet<FieldDefinitionHandle>();
foreach (var method in getters) {
    var fields = Reach(method, new());
    if (fields.Count == 0) throw new Exception(metadata.GetString(metadata.GetMethodDefinition(method).Name) + " lacks RVA data");
    reachable.UnionWith(fields);
}
uint[] expected = [1116352408U, 1899447441U, 3049323471U, 3921009573U,
                   961987163U, 1508970993U, 2453635748U, 2870763221U];
byte[] bytes = new byte[4 * expected.Length];
for (int i = 0; i < expected.Length; ++i) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), expected[i]);
bool exact = reachable.Any(h => pe.GetSectionData(metadata.GetFieldDefinition(h).GetRelativeVirtualAddress())
    .GetContent(0, bytes.Length).AsSpan().SequenceEqual(bytes));
if (!exact) throw new Exception("The complete uint initializer was not found in referenced assembly RVA storage");
Console.WriteLine(JsonSerializer.Serialize(new { initializerGetterCount = getters.Count,
    referencedRvaFieldCount = reachable.Count, exactUInt32DataFound = exact,
    managedHeapAllocationOpcodes = 0, unauditedCalls = 0 }));
''')
    run(['dotnet', 'build', directory / 'Inspector.csproj', '-c', 'Release', '--nologo',
         '-p:TreatWarningsAsErrors=true'])
    return directory / 'bin/Release/net10.0/Inspector.dll'


def verify(root, tests, out, nativeaot, run, compiler=None):
    source = tests / 'primitive_data_readability.cpp'
    oracle = out / 'oracle.cpp'
    oracle.write_text('#include <cstdio>\n#include <thread>\n#include <initializer_list>\n'
                      f'#include "{source}"\n' + '''
int main() {
  std::printf("initial:%u\\n", pd_snapshot());
  std::printf("tls-initial:%u\\n", pd_tls_snapshot());
  for (unsigned n : {0u, 65u, 65535u}) std::printf("local:%u\\n", pd_local(n));
  std::printf("floatbits:%u\\n", pd_floatbits());
  std::printf("fallback:%u\\n", pd_fallback());
  pd_mutate(); pd_tls_mutate();
  std::printf("mutated:%u\\n", pd_snapshot());
  std::printf("independent:%u\\n", pd_independent());
  std::printf("tls-mutated:%u\\n", pd_tls_snapshot());
  unsigned fresh = 0;
  std::thread worker([&] { fresh = pd_tls_snapshot(); pd_tls_mutate(); });
  worker.join();
  std::printf("tls-fresh:%u\\n", fresh);
  std::printf("tls-retained:%u\\n", pd_tls_snapshot());
}
''')
    clang = shutil.which('clang++') or shutil.which('clang++-18') or shutil.which('clang++-19')
    if not clang:
        raise RuntimeError('Clang required for the second independent source oracle')
    traces = []
    for label, frontend in [('gcc', 'g++'), ('clang', clang)]:
        executable = out / ('oracle-' + label)
        run([frontend, '-std=c++17', '-O2', '-pthread', oracle, '-o', executable])
        traces.append(run([executable]).stdout)
    assert traces[0] == traces[1], traces
    expected = traces[0]
    (out / 'expected.txt').write_text(expected)
    values = dict(line.split(':') for line in expected.splitlines() if not line.startswith('local:'))
    assert values['initial'] != values['mutated']
    assert values['tls-initial'] == values['tls-fresh']
    assert values['tls-mutated'] == values['tls-retained'] != values['tls-initial']
    assert values['independent'] == values['fallback'] == '1'
    inspector = build_pe_inspector(out, run)
    checks = ['GCC and Clang agree on primitive representations, local copies, independent arrays and per-thread storage']
    audits = {}
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', source, '--std', 'c++17', '--emit', 'managedlib',
             '--class-name', 'Core', '--namespace', 'Primitive.Data', '--split', 'function',
             '--output-dir', directory, *(['--compiler', compiler] if compiler else []),
             *(['--instance-methods'] if instance else [])])
        audits[mode] = audit_generated(directory)
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        def call(name, receiver='owner'):
            matches = [item['method'] for item in manifest['exports'] if item['symbol'] == name]
            assert len(matches) == 1, (name, matches)
            return (receiver if instance else 'Core') + '.' + matches[0]
        host = out / ('host-' + mode)
        host.mkdir()
        (host / 'Host.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>
<Nullable>enable</Nullable></PropertyGroup><ItemGroup>
<ProjectReference Include="../{mode}/Generated.csproj" /></ItemGroup></Project>''')
        owner_decl = 'using var owner = new Core(); using var second = new Core();' if instance else ''
        isolation = f'''
if ({call('pd_snapshot', 'second')}() != {values['initial']}u) return 21;
if ({call('pd_tls_snapshot', 'second')}() != {values['tls-initial']}u) return 22;
{call('pd_tls_mutate', 'second')}();
if ({call('pd_tls_snapshot', 'second')}() != {values['tls-mutated']}u) return 23;
''' if instance else ''
        release = 'owner.ReleaseThread();' if instance else 'Core.ReleaseThread();'
        (host / 'Program.cs').write_text(f'''using System;
using System.Threading;
using Primitive.Data;
unsafe class EntryPoint {{
static int Main() {{
{owner_decl}
Console.WriteLine($"initial:{{{call('pd_snapshot')}()}}");
Console.WriteLine($"tls-initial:{{{call('pd_tls_snapshot')}()}}");
foreach (uint seed in new uint[] {{0, 65, 65535}}) Console.WriteLine($"local:{{{call('pd_local')}(seed)}}");
Console.WriteLine($"floatbits:{{{call('pd_floatbits')}()}}");
Console.WriteLine($"fallback:{{{call('pd_fallback')}()}}");
{call('pd_mutate')}(); {call('pd_tls_mutate')}();
GC.Collect(2, GCCollectionMode.Forced, true, true);
Console.WriteLine($"mutated:{{{call('pd_snapshot')}()}}");
Console.WriteLine($"independent:{{{call('pd_independent')}()}}");
Console.WriteLine($"tls-mutated:{{{call('pd_tls_snapshot')}()}}");
uint fresh = 0;
var worker = new Thread(() => {{fresh = {call('pd_tls_snapshot')}(); {call('pd_tls_mutate')}(); {release}}});
worker.Start(); worker.Join();
Console.WriteLine($"tls-fresh:{{fresh}}");
Console.WriteLine($"tls-retained:{{{call('pd_tls_snapshot')}()}}");
{isolation}
{release}
return 0;
}}
}}
''')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo',
             '-p:TreatWarningsAsErrors=true'])
        pe_result = run(['dotnet', inspector, directory / 'bin/Release/net10.0/Generated.dll'])
        audits[mode]['pe'] = json.loads(pe_result.stdout)
        checks.append(f'{mode}/PE: all initializer getters are heap-free and transitively backed by FieldRVA data')
        commands = [('JIT', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])]
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=None',
                 '-p:DebugSymbols=false', '-o', host / 'aot'])
            commands.append(('NativeAOT', [host / 'aot/Host']))
        for runtime, command in commands:
            actual = run(command).stdout
            (host / (runtime + '-trace.txt')).write_text(actual)
            assert actual == expected, (mode, runtime, actual, expected)
            checks.append(f'{mode}/{runtime}: native primitive traces, forced GC, independent copies, owner and TLS isolation')
    return {'status': 'passed', 'checks': checks, 'audits': audits,
            'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest()}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=600, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(result.stdout)
        (output / f'command-{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{command}: exit {result.returncode}; see retained logs')
        return result
    results = verify(Path(__file__).resolve().parents[2], Path(__file__).resolve().parent,
                     output, args.nativeaot, run, args.compiler.resolve() if args.compiler else None)
    (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    print(json.dumps(results, indent=2))
