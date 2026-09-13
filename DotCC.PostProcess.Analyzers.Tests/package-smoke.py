#!/usr/bin/env python3
"""Load the local analyzer package through the compiler; no source edits or publishing."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET
import zipfile

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--package', required=True, type=Path)
parser.add_argument('--sqlite-project', type=Path)
args = parser.parse_args()
package = args.package.resolve()


def run(*command, env=None):
    result = subprocess.run([str(c) for c in command], text=True, stdout=subprocess.PIPE,
                            stderr=subprocess.STDOUT, env=env, timeout=120)
    assert result.returncode == 0, result.stdout
    return result.stdout


with zipfile.ZipFile(package) as archive:
    assemblies = sorted(n for n in archive.namelist() if n.endswith('.dll'))
    assert assemblies == ['analyzers/dotnet/cs/DotCC.PostProcess.Analyzers.dll',
                          'analyzers/dotnet/cs/DotCC.PostProcess.CodeFixes.dll'], assemblies
    nuspec = ET.fromstring(archive.read(next(n for n in archive.namelist() if n.endswith('.nuspec'))))
    namespace = {'n': nuspec.tag.split('}')[0][1:]}
    package_id = nuspec.find('n:metadata/n:id', namespace).text
    version = nuspec.find('n:metadata/n:version', namespace).text
    assert not nuspec.findall('.//n:dependency', namespace)

with tempfile.TemporaryDirectory(prefix='dotcc-analyzer-package-') as temporary:
    root = Path(temporary)
    config = ET.Element('configuration')
    feeds = ET.SubElement(config, 'packageSources')
    ET.SubElement(feeds, 'clear')
    ET.SubElement(feeds, 'add', key='local', value=str(package.parent))
    (root / 'NuGet.Config').write_text(ET.tostring(config, encoding='unicode'))
    project = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    properties = ET.SubElement(project, 'PropertyGroup')
    ET.SubElement(properties, 'TargetFramework').text = 'net10.0'
    ET.SubElement(properties, 'OutputType').text = 'Exe'
    items = ET.SubElement(project, 'ItemGroup')
    ET.SubElement(items, 'PackageReference', Include=package_id, Version=version, PrivateAssets='all')
    path = root / 'Consumer.csproj'
    path.write_text(ET.tostring(project, encoding='unicode'))
    source = root / 'Program.cs'
    source.write_text('''static class Cond { public static bool B(int x) => x != 0; }
static class Program { static void Main() { {} System.Console.WriteLine((Cond.B(1) ? 1 : 0) != 0); } }
''')
    (root / '.editorconfig').write_text('''root = true
[*.cs]
dotnet_diagnostic.DCCPP001.severity = warning
dotnet_diagnostic.DCCPP002.severity = warning
dotnet_diagnostic.DCCPP003.severity = warning
''')
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    # A fresh cache ensures the test cannot accidentally use an older package.
    run('dotnet', 'restore', path, '--packages', root / 'packages')
    output = run('dotnet', 'build', path, '--no-restore', '-c', 'Release', '-v:minimal')
    assert all('warning ' + rule + ':' in output for rule in ('DCCPP001', 'DCCPP002', 'DCCPP003')), output
    for failure in ['AD0001', 'CS8032', 'CS9057']:
        assert failure not in output, output
    assert hashlib.sha256(source.read_bytes()).hexdigest() == before
    assert run('dotnet', root / 'bin/Release/net10.0/Consumer.dll').strip() == 'True'
    dependencies = (root / 'bin/Release/net10.0/Consumer.deps.json').read_text()
    assert 'Microsoft.CodeAnalysis' not in dependencies and 'DotCC.PostProcess' not in dependencies

if args.sqlite_project:
    project = args.sqlite_project.resolve()
    for design_time in ('false', 'true'):
        evaluated = json.loads(run('dotnet', 'msbuild', project, '-nologo', '-getItem:Analyzer',
                                   '-p:DesignTimeBuild=' + design_time))
        analyzers = [i for i in evaluated['Items']['Analyzer'] if 'DotCC.PostProcess.' in i['Identity']]
        assert not analyzers, analyzers

print('PASS package: analyzer discovery, all three diagnostics, no runtime dependency, unchanged source, SQLite analyzer isolation')
