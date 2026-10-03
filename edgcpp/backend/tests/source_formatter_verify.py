"""Check whitespace-only SDK Roslyn formatting and generated syntax diagnostics."""
import argparse
import json
from pathlib import Path
import subprocess
import sys


def verify_source_formatter(root, tests, out, nativeaot, run):
    del tests
    out.mkdir(parents=True, exist_ok=True)
    tool = out / 'tool'
    run(['dotnet', 'build', root / 'backend/formatter/Edg.SourceFormatter.csproj',
         '-c', 'Release', '--nologo', '-o', tool,
         '-p:BaseIntermediateOutputPath=' + str(out / 'obj') + '/'])
    assembly = tool / 'Edg.SourceFormatter.dll'
    fixtures = {
        'Program.cs': '''#define SELECTED
// dot-gen:origin whitespace_fixture
using System;unsafe class Program{public static int Main(){int* values=stackalloc int[3];for(int index=0;index<3;++index){values[index]=index+1;}string raw="""
first
second
""";string escaped="π\\nquoted\\\"";
#if SELECTED
if(values[2]!=3||raw!="first\\nsecond"||escaped.Length!=9)return 7;
#endif
Console.WriteLine(raw.Replace('\\n','|')+"|"+values[2]);return 0;}}
''',
        'Extra.cs': '// preserved comment\nnamespace Sample{public struct Record{public uint mode{get{return 3;}set{}}}}\n'
    }
    def format_batch(label, request, expected=0):
        raw = json.dumps(request) if isinstance(request, dict) else request
        (out / (label + '.input.json')).write_text(raw)
        command = ['dotnet', str(assembly)]
        result = subprocess.run(command, input=raw, text=True, capture_output=True)
        (out / (label + '.output.json')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        assert result.returncode == expected, (label, result.returncode, result.stdout, result.stderr)
        return json.loads(result.stdout)
    result = format_batch('valid', fixtures)
    assert not result['diagnostics'] and result['files'].keys() == fixtures.keys()
    assert '\n    public static int Main()\n    {\n' in result['files']['Program.cs']
    assert '// dot-gen:origin whitespace_fixture' in result['files']['Program.cs']
    assert '// preserved comment' in result['files']['Extra.cs']
    assert all(value.endswith('\n') and '\r' not in value for value in result['files'].values())
    assert format_batch('idempotent', result['files'])['files'] == result['files']
    checks = ['formatter: batch indentation, metadata/comments, raw/escaped Unicode literals and idempotence']
    malformed = {'Bad.cs': 'class Broken { void M( {', 'Good.cs': 'class Good{}'}
    error = format_batch('syntax-error', malformed, 2)
    assert error['files']['Bad.cs'] == malformed['Bad.cs']
    assert error['diagnostics'] and all(d['file'] == 'Bad.cs' and d['line'] > 0
                                      and d['column'] > 0 and d['severity'] == 'error'
                                      for d in error['diagnostics'])
    for label, request in [('malformed-json', '{'), ('not-object', '[]'),
                           ('not-string', '{"bad":12}'), ('duplicate', '{"x":"","x":""}')]:
        error = format_batch(label, request, 1)
        assert error['diagnostics'][0]['id'] == 'FORMAT_INPUT'
    checks.append('formatter: syntax errors preserved and malformed request diagnostics')
    for label, source in [('original', fixtures), ('formatted', result['files'])]:
        project = out / label
        project.mkdir()
        (project / 'Fixture.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
            '<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>'
            '<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>'
            '<TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>')
        for name, text in source.items():
            (project / name).write_text(text)
        run(['dotnet', 'build', project / 'Fixture.csproj', '-c', 'Release', '--nologo'])
        run(['dotnet', project / 'bin/Release/net10.0/Fixture.dll'])
        checks.append('formatter ' + label + ': JIT executable literals/directives/pointers')
        if nativeaot:
            run(['dotnet', 'publish', project / 'Fixture.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:DebugType=none',
                 '-p:DebugSymbols=false', '-o', project / 'aot'])
            run([project / 'aot/Fixture'])
            checks.append('formatter ' + label + ': Linux x64 NativeAOT')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command))
        result = subprocess.run(command, text=True, capture_output=True, **kwargs)
        index = len(commands)
        (out / f'{index:03d}.stdout').write_text(result.stdout)
        (out / f'{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'command {index} failed: {result.returncode}; see retained logs')
        return result
    root = Path(__file__).resolve().parents[2]
    checks = verify_source_formatter(root, Path(__file__).resolve().parent, out, args.nativeaot, run)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} formatter checks passed')
