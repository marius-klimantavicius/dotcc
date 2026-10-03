"""Exercise source navigation and whole-member splitting through the real driver."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys


def verify_source_layout_driver(root, tests, out, nativeaot, run):
    checks = []
    source = tests / 'source_correspondence.c'
    for mode in ('none', 'function', 'size', 'source'):
        directory = out / mode
        command = [sys.executable, root / 'dotnet.py', source, '--language', 'c',
                   '--namespace', 'Navigation.namespace', '--class-name', 'class',
                   '--split', mode, '--split-size', '2500', '--output-dir', directory,
                   *(['--instance-methods'] if mode == 'source' else [])]
        run(command)
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        mapping_bytes = (directory / 'dot-gen.sources.json').read_bytes()
        assert hashlib.sha256(mapping_bytes).hexdigest() == manifest['sourceMap']['sha256']
        mapping = json.loads(mapping_bytes)
        assert manifest['formatter']['mode'] == 'roslyn-whitespace-v1'
        assert mapping['symbols'] and mapping['members'] and mapping['publicTypes']
        names = {member.get('sourceName') for member in mapping['members']}
        assert {'sum', 'contribution', 'control_flow'} <= names
        for member in mapping['members']:
            filename = member['generatedFile']
            assert filename in manifest['files']
            lines = (directory / filename).read_text().splitlines()
            index = member['generatedLine'] - 1
            assert 0 <= index < len(lines)
            assert re.search(r'\b' + re.escape(member['managedName'].lstrip('@')) + r'\s*(?:\(|=>|\{)', lines[index]), (mode, member, lines[index])
            if member.get('sourceName') in ('sum', 'contribution', 'control_flow'):
                assert Path(member['sourceFile']).resolve() == source.resolve()
                assert member['sourceLine'] > 0
        filenames = [name for name in manifest['files'] if name.startswith('Translated') and name.endswith('.cs')]
        if mode == 'none':
            assert filenames == ['Translated.cs']
        else:
            prefix = {'function': 'Translated.function.', 'size': 'Translated.part', 'source': 'Translated.source.'}[mode]
            assert any(name.startswith(prefix) for name in filenames), filenames
        assert len({name.casefold() for name in filenames}) == len(filenames)
        from source_correspondence_verify import audit_source
        audit_source(directory)
        project = directory / 'Generated.csproj'
        run(['dotnet', 'build', project, '-c', 'Release', '--nologo', '-p:TreatWarningsAsErrors=true'])
        run(['dotnet', directory / 'bin/Release/net10.0/Generated.dll'])
        checks.append(mode + ': formatted source names, exact navigation, strict JIT')
        if mode == 'source' and nativeaot:
            run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', directory / 'aot'])
            run([directory / 'aot/Generated'])
            checks.append('source: instance namespace/keyword class Linux x64 NativeAOT')
        if mode == 'source':
            previous = {name: (directory / name).read_bytes() for name in manifest['files']}
            authored = directory / 'Authored.cs'
            authored.write_text('// User-authored companion remains untouched.\n')
            replacement = list(command)
            replacement[replacement.index('--split') + 1] = 'size'
            run(replacement)
            histories = list((directory / '.dot-gen-history').iterdir())
            assert len(histories) == 1
            for name, data in previous.items():
                current = directory / name
                retained = current if current.is_file() and current.read_bytes() == data else histories[0] / name
                assert retained.read_bytes() == data, name
            new_manifest = json.loads((directory / 'dot-gen.json').read_text())
            assert all(not (directory / name).exists() for name in set(previous) - set(new_manifest['files']))
            assert authored.read_text() == '// User-authored companion remains untouched.\n'
            checks.append('regeneration: prior generated source/index retained and authored file preserved')
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
        result = subprocess.run(command, capture_output=True, text=True, timeout=600, **kwargs)
        index = len(commands)
        (out / f'{index:03d}.stdout').write_text(result.stdout)
        (out / f'{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode:
            raise RuntimeError(f'{command}: exit {result.returncode}; see retained logs')
        return result
    root = Path(__file__).resolve().parents[2]
    checks = verify_source_layout_driver(root, Path(__file__).resolve().parent, out, args.nativeaot, run)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(json.dumps(checks, indent=2))
