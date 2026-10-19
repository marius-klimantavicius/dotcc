"""Exercise unchanged same-pin BLAKE2 reference sources with shared erasure.

Uses existing campaign process/identity support and an immutable compiler;
does not build tools, edit configuration, or publish a profile.
"""
import argparse
import json
from pathlib import Path
import sys

REPO = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO / 'Scripts'))
from campaigns.identity import digest, write_json
from campaigns.process import execute


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work', type=Path, required=True)
    parser.add_argument('--artifacts', type=Path, required=True)
    parser.add_argument('--tools', type=Path)
    parser.add_argument('--source', type=Path, required=True, help='Unchanged source tree from actual failed profile receipt')
    parser.add_argument('--failed-receipt', type=Path)
    args = parser.parse_args()
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    work.mkdir(parents=True, exist_ok=False)
    artifacts.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, cmd):
        record = {}
        commands.append(record)
        try:
            execute(cmd, label=label, cwd=REPO, output=artifacts / (label + '.log'), timeout=600, record=record)
        finally:
            write_json(artifacts / 'commands.json', commands)

    impl = args.source.resolve() / 'Modules/_blake2/impl'
    fixture = REPO / 'cpython/tests/Abi/blake2_secure_zero_r13.c'
    inputs = [fixture, impl / 'blake2b-ref.c', impl / 'blake2s-ref.c']
    flags = ['-std=c11', '-DHAVE_EXPLICIT_BZERO=1', '-I', impl]
    source_hashes = {str(p): digest(p) for p in impl.iterdir() if p.is_file()}
    source_hashes[str(fixture)] = digest(fixture)
    write_json(artifacts / 'inputs.json', {'sha256': source_hashes, 'macro': 'HAVE_EXPLICIT_BZERO=1',
        'scope': 'unchanged actual BLAKE2 source/service control; not a full profile or interpreter qualification'})
    run('native-build', ['flock', '--close', '/tmp/dotcc-cpython-native-build.lock', 'cc',
        '-O3', '-D_DEFAULT_SOURCE', '-Wall', '-Wextra', '-Werror', *flags, *inputs, '-o', work / 'native'])
    run('native-run', [work / 'native'])
    if not args.tools:
        return
    cli = args.tools.resolve() / 'dotcc.dll'
    lock = ['flock', '--close', '/tmp/dotcc-cpython-shared-build.lock']
    # Normal per-unit emission/link path, including all actual algorithm units.
    objects = []
    for i, source in enumerate(inputs):
        obj = work / f'{i}-{source.stem}.cs'
        objects.append(obj)
        run('emit-' + source.stem, [*lock, 'dotnet', cli, *flags, source, '--emit=obj', '-o', obj])
    run('link', [*lock, 'dotnet', cli, *objects, '--emit=csproj', '--runtime=c', '-o', work / 'emitted'])
    project = work / 'emitted/emitted.csproj'
    for mode in ('jit', 'aot'):
        cmd = ['dotnet', 'build' if mode == 'jit' else 'publish', project, '-c', 'Release',
               '--disable-build-servers', '-m:1', '--artifacts-path', work / mode]
        if mode == 'aot':
            cmd += ['-r', 'linux-x64', '-p:PublishAot=true', '-p:StripSymbols=false', '-o', work / 'aot-app']
        run(mode + '-build', [*lock, *cmd])
        app = ['dotnet', work / 'jit/bin/emitted/release/emitted.dll'] if mode == 'jit' else [work / 'aot-app/emitted']
        run(mode + '-run', [*lock, *app])
        assert (artifacts / (mode + '-run.log')).read_bytes() == (artifacts / 'native-run.log').read_bytes()
    if args.failed_receipt:
        receipt = json.loads(args.failed_receipt.read_text())
        original = [c for c in receipt['commands'] if c['label'] == '280-blake2module']
        assert len(original) == 1 and original[0]['exit_code'] != 0
        cmd = original[0]['command'].copy()
        cmd[1] = str(cli)
        cmd[cmd.index('-o') + 1] = str(work / 'actual-blake2module.cs')
        cmd[cmd.index('--override-report') + 1] = str(artifacts / 'actual-overrides.jsonl')
        cmd += ['-DHAVE_EXPLICIT_BZERO=1']
        run('actual-failed-unit', [*lock, *cmd])
        write_json(artifacts / 'actual-failed-unit-provenance.json', {
            'receipt': str(args.failed_receipt.resolve()), 'receiptSha256': digest(args.failed_receipt),
            'originalCommand': original[0], 'candidateCommand': cmd,
            'onlyChanges': ['immutable tools', 'private object/report destinations', 'HAVE_EXPLICIT_BZERO=1'],
            'source': {str(args.source.resolve() / 'Modules/_blake2/blake2module.c'): digest(args.source.resolve() / 'Modules/_blake2/blake2module.c')},
            'scope': 'actual failed unit diagnostic; no full286/profile pass'})
    for p, expected in source_hashes.items():
        assert digest(Path(p)) == expected
    write_json(artifacts / 'comparison.json', {'status': 'passed', 'cells': ['native', 'jit', 'aot'],
        'stdout': (artifacts / 'native-run.log').read_text().strip(),
        'sourceHashesUnchanged': True, 'actualFailedUnitPassed': bool(args.failed_receipt)})


if __name__ == '__main__':
    main()
