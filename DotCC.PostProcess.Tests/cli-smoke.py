#!/usr/bin/env python3
"""Standalone CLI snapshot contracts; run serially after building the tool."""
import argparse
import hashlib
import json
import pathlib
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--tool', required=True, type=pathlib.Path)
args = parser.parse_args()
tool = args.tool.resolve()

def run(*command, expected=0):
    result = subprocess.run([str(x) for x in command], text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    if result.returncode != expected:
        raise AssertionError(f'{command}: expected {expected}, got {result.returncode}\n{result.stdout}')
    return result.stdout

with tempfile.TemporaryDirectory(prefix='dotcc-postprocess-tests-') as temporary:
    root = pathlib.Path(temporary)
    # The hostile destination parent proves snapshots isolate ancestor targets.
    (root / 'Directory.Build.targets').write_text('<Project><Target Name="Hostile" BeforeTargets="CoreCompile"><Error Text="ancestor imported" /></Target></Project>')
    source = root / 'input'
    source.mkdir()
    (source / 'Directory.Build.targets').write_text('<Project />')
    dependency = root / 'dependency'
    dependency.mkdir()
    (dependency / 'Directory.Build.targets').write_text('<Project />')
    (dependency / 'Dependency.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
    (dependency / 'Value.cs').write_text('public static class Value { public static int Number => 17; }')
    project = source / 'Sample.csproj'
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><DebugType>portable</DebugType><EmbedAllSources>true</EmbedAllSources></PropertyGroup><ItemGroup><ProjectReference Include="../dependency/Dependency.csproj"/><EmbeddedResource Include="data.txt" LogicalName="payload" /></ItemGroup></Project>''')
    (source / 'data.txt').write_text('resource-preserved')
    code = source / 'Program.cs'
    code.write_text('''using System.Runtime.CompilerServices;
static class Cond { public static bool B(int x) => x != 0; }
static class Program {
 static string File([CallerFilePath] string path = "") => path;
 static void Main() {
#if DEBUG
  throw new System.Exception("Changed Release preprocessor symbols");
#else
  using var reader = new System.IO.StreamReader(typeof(Program).Assembly.GetManifestResourceStream("payload")!);
  System.Console.WriteLine($"{Cond.B(Value.Number)}|{Value.Number}|{reader.ReadToEnd()}|{File()}");
#endif
 }
}
''')
    run('dotnet', 'build', project, '-c', 'Release', '-v:q')
    expected = run('dotnet', source / 'bin/Release/net10.0/Sample.dll')
    before = hashlib.sha256(code.read_bytes()).hexdigest()
    output = root / 'snapshot'
    run('dotnet', tool, project, '--output', output)
    manifest = json.loads((output / 'manifest.json').read_text())
    assert manifest['Rewritten'] == 1, manifest
    assert before == hashlib.sha256(code.read_bytes()).hexdigest()
    for item in manifest['Files']:
        assert hashlib.sha256((output / item['Path']).read_bytes()).hexdigest() == item['Sha256']
    for variant in ('OriginalProject', 'OptimizedProject'):
        snapshot = output / manifest[variant]
        # Building Debug must retain the evaluated Release source semantics.
        run('dotnet', 'build', snapshot, '-c', 'Debug', '-v:q')
        actual = run('dotnet', snapshot.parent / 'bin/Debug/net10.0/Sample.dll')
        assert actual == expected, (expected, actual)
        assert (snapshot.parent / 'bin/Debug/net10.0/Sample.pdb').exists()
    second = root / 'snapshot2'
    run('dotnet', tool, project, '--output', second)
    assert (output / 'manifest.json').read_bytes() == (second / 'manifest.json').read_bytes()
    for bad in (output, source / 'nested'):
        assert 'overlap' in run('dotnet', tool, project, '--output', bad, expected=1) or bad == output
    alias = root / 'alias'
    try:
        alias.symlink_to(source, target_is_directory=True)
    except OSError:
        pass
    else:
        assert 'overlap' in run('dotnet', tool, project, '--output', alias / 'nested', expected=1)
        project_link = root / 'Linked.csproj'
        project_link.symlink_to(project)
        assert 'overlap' in run('dotnet', tool, project_link, '--output', source / 'linked-nested', expected=1)
    code.write_text('class Broken { invalid syntax }')
    failure = root / 'failure'
    run('dotnet', tool, project, '--output', failure, expected=1)
    assert not failure.exists()
print('PASS CLI: references/resources/caller paths/symbols/PDBs/determinism/input protection/atomic failure')
