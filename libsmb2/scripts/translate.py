#!/usr/bin/env python3
"""Translate the pinned closure, preserve raw output, postprocess and build.

No generated product is promoted until every required generation stage succeeds.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import json
from pathlib import Path
import shutil
import tempfile
import time
from common import ROOT, REPO, SOURCE_SPEC, fetch, run, sha

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--no-build-tools', action='store_true', help='Reuse existing built compiler/postprocessor')
parser.add_argument('--jobs', type=int, default=4, help='Independent translation-unit workers (default: 4)')
args = parser.parse_args()
if args.jobs < 1 or args.jobs > 16:
    parser.error('--jobs must be between 1 and 16')
logs = ROOT / 'artifacts/translation'
logs.mkdir(parents=True, exist_ok=True)
build = ROOT / 'build'
build.mkdir(exist_ok=True)
receipt = dict(passed=False, source=SOURCE_SPEC, started_unix=time.time(), stages=[])
stage = Path(tempfile.mkdtemp(prefix='translation-', dir=build))
raw = stage / 'raw/TranslatedLibsmb2'
product = stage / 'product/TranslatedLibsmb2'


def snapshot(directory, destination):
    destination.mkdir(parents=True)
    inputs = {p.name: p.read_bytes() for p in directory.iterdir() if p.suffix in ('.dll', '.json')}
    for name, content in inputs.items():
        (destination / name).write_bytes(content)
    if any((directory / name).read_bytes() != content for name, content in inputs.items()):
        raise RuntimeError('Build tools changed while snapshotting; retry after build completion')
    return {name: sha(destination / name) for name in inputs}


def promote(source, destination):
    # Only this dedicated generated directory is replaced. Keep its previous
    # contents beside staging until the rename succeeds; no user-tree globbing.
    backup = stage / (destination.name + '.previous')
    if destination.exists():
        destination.rename(backup)
    try:
        source.rename(destination)
    except BaseException:
        if backup.exists():
            backup.rename(destination)
        raise


try:
    source = fetch()
    units = json.loads((ROOT / 'config/sources.json').read_text())
    defines = json.loads((ROOT / 'config/defines.json').read_text())
    if not units or len(units) != len(set(units)):
        raise RuntimeError('Empty or duplicate source manifest')
    receipt['units'] = {unit: sha(source / unit) for unit in units}
    receipt['config'] = {str(p.relative_to(ROOT)): sha(p) for p in sorted((ROOT / 'config').rglob('*')) if p.is_file()}
    if not args.no_build_tools:
        for name in ('DotCC', 'DotCC.PostProcess'):
            run(['dotnet', 'build', REPO / name / (name + '.csproj'), '-c', 'Release', '--nologo'],
                logs / (name + '-build.log'), receipt)
    compiler_dir = stage / 'tools/compiler'
    post_dir = stage / 'tools/postprocessor'
    receipt['compiler'] = snapshot(REPO / 'DotCC/bin/Release/net10.0', compiler_dir)
    receipt['postprocessor'] = snapshot(REPO / 'DotCC.PostProcess/bin/Release/net10.0', post_dir)
    compiler = compiler_dir / 'dotcc.dll'
    includes = [ROOT / 'config/managed', source / 'include', source / 'include/smb2', source / 'lib']
    flags = ['-std=c17', *['-D' + value for value in defines]]
    for include in includes:
        flags += ['-I', include]
    (stage / 'objects').mkdir()
    def emit(index_unit):
        index, unit = index_unit
        output = stage / 'objects' / (f'{index:02d}-' + Path(unit).stem + '.cs')
        run(['dotnet', compiler, *flags, '--emit=obj', source / unit, '-o', output],
            logs / (Path(unit).stem + '-translate.log'), receipt)
        print(f'translated {unit}', flush=True)
        return output
    with ThreadPoolExecutor(max_workers=args.jobs) as workers:
        objects = list(workers.map(emit, enumerate(units)))
    receipt['stages'].append('all-objects')
    run(['dotnet', compiler, *objects, '--emit=managedlib', '--literal-pool', '--nest-types',
         '--runtime=c', '--class-name', 'LibSmb2', '--namespace', 'Managed.Smb',
         '--split=size', '--split-size=102400', '-o', raw], logs / 'link.log', receipt)
    project_name = 'TranslatedLibsmb2.csproj'
    run(['dotnet', 'build', raw / project_name, '-c', 'Release', '--nologo'], logs / 'raw-build.log', receipt)
    receipt['stages'].append('raw-build')
    shutil.copytree(raw, product, ignore=shutil.ignore_patterns('bin', 'obj'))
    run(['dotnet', 'restore', product / project_name, '--nologo'], logs / 'restore.log', receipt)
    run(['dotnet', post_dir / 'dotcc-postprocess.dll', product / project_name, '--in-place'],
        logs / 'postprocess.log', receipt)
    run(['dotnet', 'build', product / project_name, '-c', 'Release', '--nologo'], logs / 'product-build.log', receipt)
    receipt['stages'].append('postprocessed-build')
    for directory in (raw, product):
        for name in ('bin', 'obj'):
            shutil.rmtree(directory / name, ignore_errors=True)
    receipt['output_sha256'] = {str(p.relative_to(product)): sha(p) for p in product.rglob('*') if p.is_file()}
    receipt['raw_output_sha256'] = {str(p.relative_to(raw)): sha(p) for p in raw.rglob('*') if p.is_file()}
    generated = ROOT / 'generated'
    generated.mkdir(exist_ok=True)
    promote(raw, generated / 'TranslatedLibsmb2.Raw')
    promote(product, generated / 'TranslatedLibsmb2')
    receipt['passed'] = True
    print(generated / 'TranslatedLibsmb2' / project_name)
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    receipt['staging_directory'] = str(stage.relative_to(ROOT))
    (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
