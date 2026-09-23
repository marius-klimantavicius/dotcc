#!/usr/bin/env python3
"""Run the implemented Linux x64 qualification suites against existing output.

This receipt covers the listed suites, not every pending gate in docs/PLAN.md.
"""
import argparse
import json
from pathlib import Path
import shutil
import tempfile
from common import ROOT, run, sha

argparse.ArgumentParser(description=__doc__).parse_args()
logs = ROOT / 'artifacts/qualification'
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, scope='implemented Linux x64 suites', plan_complete=False)
def authored_sources():
    return {str(p.relative_to(ROOT)): sha(p) for directory in ('src', 'samples', 'tests')
            for p in (ROOT / directory).rglob('*') if p.is_file() and p.suffix in ('.cs', '.csproj')
            and not {'bin', 'obj'}.intersection(p.relative_to(ROOT).parts)}


def host_sources():
    paths = [ROOT / 'Directory.Build.targets', ROOT / 'src/LibSmb2.Bcl.cs',
             *sorted((ROOT / 'src').glob('HostSockets*.cs')), *sorted((ROOT / 'src/Kerberos').glob('*.cs'))]
    return {**{str(path.relative_to(ROOT)): sha(path) for path in paths if path.is_file()},
            '../Directory.Packages.props': sha(ROOT.parent / 'Directory.Packages.props')}

try:
    receipt['authored_sources'] = authored_sources()
    generation = json.loads((ROOT / 'artifacts/translation/result.json').read_text())
    if not generation['passed'] or generation.get('profile') != 'async':
        raise RuntimeError('Regenerate the async product before qualification')
    if not generation.get('host_sources') or generation['host_sources'] != host_sources():
        raise RuntimeError('Authored socket host changed since translation; regenerate before qualification')
    receipt['host_sources'] = host_sources()
    product = ROOT / 'generated/TranslatedLibsmb2'
    for name, key in (('TranslatedLibsmb2', 'output_sha256'), ('TranslatedLibsmb2.Raw', 'raw_output_sha256')):
        directory = ROOT / 'generated' / name
        actual = {str(p.relative_to(directory)): sha(p) for p in directory.rglob('*')
                  if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
        if actual != generation[key]:
            raise RuntimeError('Generated output changed since translation: ' + name)
    receipt['generation_receipt_sha256'] = sha(ROOT / 'artifacts/translation/result.json')
    run(['dotnet', 'build', ROOT / 'ManagedConsumer.slnx', '-c', 'Release', '--nologo'],
        logs / 'solution-build.log', receipt)
    run(['dotnet', 'run', '--project', ROOT / 'samples/ManagedConsumer', '-c', 'Release',
         '--no-build', '--', '--help'], logs / 'sample-help.log', receipt)
    run(['dotnet', 'run', '--project', ROOT / 'tests/DfsCodec', '-c', 'Release'],
        logs / 'dfs-codec.log', receipt)
    for dfs_flags in ([], ['--raw'], ['--aot'], ['--raw', '--aot']):
        run(['python3', ROOT / 'scripts/test-dfs.py', *dfs_flags],
            logs / ('dfs-' + ('-'.join(flag[2:] for flag in dfs_flags) or 'processed-jit') + '.log'), receipt, timeout=1800)
    run(['python3', ROOT / 'scripts/test-host-services.py'], logs / 'host-services.log', receipt, timeout=1800)
    for variant in ('TranslatedLibsmb2', 'TranslatedLibsmb2.Raw'):
        host_output = ROOT / 'build' / ('AsyncHost-' + variant)
        run(['dotnet', 'build', ROOT / 'tests/AsyncHost/AsyncHost.csproj', '-c', 'Release',
             '-p:Libsmb2GeneratedProject=' + str(ROOT / 'generated' / variant / 'TranslatedLibsmb2.csproj'),
             '-o', host_output, '--nologo'], logs / (variant + '-async-host-build.log'), receipt)
        run(['dotnet', host_output / 'AsyncHost.dll'],
            logs / (variant + '-async-host.log'), receipt, timeout=120)
        run([ROOT / 'scripts/crypto-abi.sh', '--aot', '--label', 'qualification-' + variant,
             '--generated-project', ROOT / 'generated' / variant / 'TranslatedLibsmb2.csproj'],
            logs / (variant + '-crypto.log'), receipt, timeout=1800)
    run(['python3', ROOT / 'scripts/audit-product.py'], logs / 'product-audit.log', receipt, timeout=1800)
    run(['python3', ROOT / 'scripts/inventory-upstream.py'], logs / 'upstream-inventory.log', receipt)
    # Unchanged upstream synchronous C tests use an explicitly separate Libc
    # transport profile; their success does not qualify the async product host.
    run([ROOT / 'scripts/translate.sh', '--profile', 'legacy', '--no-build-tools'],
        logs / 'legacy-translation.log', receipt, timeout=3600)
    run([ROOT / 'scripts/upstream-tests.sh'], logs / 'upstream-tests.log', receipt, timeout=14400)
    # Idempotence is tested on a private copy with the original semantic tool.
    with tempfile.TemporaryDirectory(prefix='idempotence-', dir=ROOT / 'build') as temporary:
        copied = Path(temporary) / 'TranslatedLibsmb2'
        shutil.copytree(product, copied, ignore=shutil.ignore_patterns('bin', 'obj'))
        before = {p.name: sha(p) for p in copied.glob('*.cs')}
        run(['dotnet', 'restore', copied / 'TranslatedLibsmb2.csproj', '--nologo'], logs / 'idempotence-restore.log', receipt)
        post = ROOT / generation['staging_directory'] / 'tools/postprocessor/dotcc-postprocess.dll'
        run(['dotnet', post, copied / 'TranslatedLibsmb2.csproj', '--in-place'], logs / 'idempotence.log', receipt)
        if before != {p.name: sha(p) for p in copied.glob('*.cs')}:
            raise RuntimeError('Postprocessor changed an already processed product')
    run([ROOT / 'scripts/oracle.sh'], logs / 'native-oracle.log', receipt, timeout=1200)
    for suite in ('sample', 'lifecycle'):
        for runtime in ('jit', 'aot'):
            for raw in (False, True):
                suffix = suite + '-' + runtime + ('-raw' if raw else '-processed')
                command = [ROOT / 'scripts/oracle.sh', '--managed', runtime, '--suite', suite]
                if raw:
                    command.append('--raw')
                run(command, logs / (suffix + '.log'), receipt, timeout=1800)
    for variant in ('processed', 'raw'):
        run(['python3', ROOT / 'scripts/facade-lifetime.py', '--label', 'qualification-' + variant,
             '--assemblies', ROOT / 'build' / ('ManagedConsumer-jit-' + variant)],
            logs / (variant + '-facade-lifetime.log'), receipt)
    if receipt['authored_sources'] != authored_sources():
        raise RuntimeError('Authored consumers/tests changed during qualification; rerun')
    if receipt['host_sources'] != host_sources():
        raise RuntimeError('Authored socket host/build inputs changed during qualification; rerun')
    receipt['passed'] = True
    print('Implemented Linux x64 qualification suites passed; remaining acceptance gates are listed in docs/PLAN.md.')
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
