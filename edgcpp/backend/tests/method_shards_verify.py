#!/usr/bin/env python3
"""Exercise automatic CLR method-budget sharding without changing source APIs."""
import argparse
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT))
from backend.source_format import shard_methods, format_sources


def fixture(instance):
    mode = 'sealed' if instance else 'static'
    static = '' if instance else 'static '
    source = '#nullable enable\nnamespace ShardTests;\npublic ' + mode + ' unsafe partial class Engine {\n'
    source += 'private static int state; static Engine() { state = 7; }\n'
    source += 'private struct Hidden { public int Value; }\n'
    source += 'private static int __edg_expr_hidden(Hidden value) => value.Value;\n'
    source += 'public static int OriginalName(int value) => value + state;\n'
    source += 'public static class Nested { public static int Invoke() => Engine.__edg_assign2(1); }\n'
    source += 'public static delegate* managed<int, int> OriginalPointer() => &OriginalName;\n'
    source += 'private static int Property { get => state; set => state = value; }\n'
    source += 'private static event global::System.Action? Event;\n'
    source += 'public static int CheckEvents() { Event?.Invoke(); Event = null; return Property; }\n'
    if instance:
        source += 'private int instanceState; public Engine(int value) { instanceState = value; }\n'
        source += 'private int __edg_expr_instance(int value) => instanceState + value;\n'
    source += f'public {static}int Run() {{\n'
    if instance:
        source += 'if (__edg_expr_instance(2) != instanceState + 2) return 90;\n'
    source += 'int hidden = __edg_expr_hidden(new Hidden { Value = 5 });\n'
    source += 'return Engine.__edg_assign0(hidden) + __edg_assign29(2) + OriginalPointer()(3) + CheckEvents();\n}\n'
    for i in range(30):
        rhs = 'state + value' if i == 0 else 'Engine.__edg_assign0(value) + ' + str(i)
        source += '// dot-gen:member-begin\nprivate static int __edg_assign' + str(i) + '(int value) => ' + rhs + ';\n// dot-gen:member-end\n'
    # A signature exposing a private type must stay in the owner.
    source += '// dot-gen:member-begin\nprivate static int __edg_expr_private(Hidden value) => value.Value;\n// dot-gen:member-end\n'
    source += '}\n'
    call = 'new Engine(10).Run()' if instance else 'Engine.Run()'
    source += 'internal static class EntryPoint { public static int Main() { int answer = ' + call + '; global::System.Console.WriteLine(answer); return answer == 67 ? 0 : 1; } }\n'
    return source


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'cases': [], 'passed': False}
    for instance in (False, True):
        directory = out / ('instance' if instance else 'static')
        directory.mkdir()
        source = fixture(instance)
        untouched, no_op = shard_methods(ROOT, source, 'ShardTests.Engine')
        assert untouched == source and not no_op['shards']
        rewritten, metadata = shard_methods(ROOT, source, 'ShardTests.Engine', threshold=25, target=22)
        assert metadata['after'] <= 22 and metadata['shards']
        assert 'private static int __edg_expr_private' in rewritten
        assert 'private static int __edg_assign2' in rewritten
        assert 'global using static' not in rewritten
        if instance:
            assert 'private int __edg_expr_instance' in rewritten
        assert 'public static int OriginalName' in rewritten
        (directory / 'input.cs.txt').write_text(source)
        twin = rewritten.replace('Engine', 'SecondEngine').replace('EntryPoint', 'SecondEntryPoint')
        (directory / 'SecondOwner.cs').write_text(twin)
        twin_call = 'new SecondEngine(20).Run()' if instance else 'SecondEngine.Run()'
        rewritten = rewritten.replace('int answer = ', 'if (' + twin_call + ' != 67) return 20; int answer = ')
        formatted, _ = format_sources(ROOT, {'Translated.cs': rewritten})
        source = formatted['Translated.cs']
        # Exercise the exact marker protocol used by the driver's split modes:
        # a shard stays whole in one partial owner file.
        pattern = re.compile(r'^[ \t]*// dot-gen:member-begin\n(.*?)^[ \t]*// dot-gen:member-end\n', re.M | re.S)
        members = pattern.findall(source)
        (directory / 'Translated.cs').write_text(pattern.sub('', source))
        for i, member in enumerate(members):
            (directory / f'Member{i}.cs').write_text('#nullable enable\nnamespace ShardTests;\npublic ' + ('sealed' if instance else 'static') + ' unsafe partial class Engine {\n' + member + '}\n')
        (directory / 'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><StartupObject>ShardTests.EntryPoint</StartupObject><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
        commands = []
        def run(label, command):
            result = subprocess.run(list(map(str, command)), capture_output=True, text=True)
            (directory / (label + '.stdout')).write_text(result.stdout)
            (directory / (label + '.stderr')).write_text(result.stderr)
            commands.append({'stage': label, 'command': list(map(str, command)), 'exitCode': result.returncode})
            if result.returncode:
                raise RuntimeError(f'{label} failed; see {directory}')
            return result.stdout
        run('build', ['dotnet', 'build', directory / 'Test.csproj', '-c', 'Release', '--nologo'])
        assert run('jit', ['dotnet', directory / 'bin/Release/net10.0/Test.dll']) == '67\n'
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', directory / 'Test.csproj', '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            assert run('aot', [directory / 'aot/Test']) == '67\n'
        report['cases'].append({'instance': instance, 'sharding': metadata, 'commands': commands})
    try:
        shard_methods(ROOT, '#nullable enable\npublic sealed class C { public void A() {} public void B() {} }', 'C', threshold=2, target=1)
        raise AssertionError('Expected insufficient-safe-methods diagnostic')
    except RuntimeError as error:
        assert 'only 0 can move safely' in str(error)
    report['passed'] = True
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print('Static/instance method sharding checks passed')


if __name__ == '__main__':
    main()
