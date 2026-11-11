"""Run safe-wrapper qualification and repeated equivalent-work benchmarks."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import platform
import sys

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('gallery_runner', ROOT / 'tests/gallery/run.py')
gallery_runner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gallery_runner)


def parse_bench(path):
    rows = []
    for line in path.read_text().splitlines():
        if line.startswith(('BENCH\t', 'BENCH_MEMORY\t')):
            fields = dict(part.split('=', 1) for part in line.split('\t')[1:])
            rows.append(dict(kind=line.split('\t')[0], **fields))
    if len(rows) != 6:
        raise RuntimeError(f'{path}: expected three timing and three memory rows')
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True)
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--repetitions', type=int, default=3)
    args = parser.parse_args()
    if platform.system() != 'Linux':
        parser.error('The current execution matrix is Linux x64; actual Windows remains unrun.')
    if args.repetitions < 3:
        parser.error('Qualification requires at least three benchmark repetitions.')
    work = args.work_root.resolve()
    if work.is_relative_to(ROOT.parent):
        parser.error('Select external storage for all outputs.')
    work.mkdir(parents=True, exist_ok=True)
    temporary = work / 'tmp'; temporary.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary),
               DOTNET_TieredCompilation='0', PYTHONDONTWRITEBYTECODE='1')
    report = dict(status='running', windows='unrun', platform=platform.platform(), variants=[],
                  methodology='Release; tiered compilation disabled for optimized JIT; 200 warmup frames, 1200 alternating paired samples per workload; no GL/vsync; exact software RGBA equality; separate allocator instrumentation',
                  gates=['Zero steady-state wrapper managed bytes over identical direct workload',
                         'Construction p95 overhead <= 0.5 us per emitted node',
                         'Build + layout/paint median <= twice direct CUI',
                         'Stable wrapper arena and core allocation retention; observed core allocations freed at disposal'])
    try:
        forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
        modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
        for form in forms:
            for mode in modes:
                variant = work / f'{form}-{mode}'; variant.mkdir(exist_ok=True)
                artifacts = variant / 'artifacts'
                command = ['dotnet', 'publish' if mode == 'aot' else 'build', ROOT / 'tests/managed/ManagedCui.Tests.csproj',
                           '-c', 'Release', '--artifacts-path', artifacts, '-m:1', '--nologo',
                           '-p:CuiVariant=' + ('Raw' if form == 'raw' else 'Processed')]
                if mode == 'aot': command += ['-r', 'linux-x64', '-p:PublishAot=true']
                gallery_runner.execute(command, variant / 'build.log', env, 3600)
                binary = artifacts / ('publish/ManagedCui.Tests/release_linux-x64/ManagedCui.Tests' if mode == 'aot' else 'bin/ManagedCui.Tests/release/ManagedCui.Tests.dll')
                launch = [binary] if mode == 'aot' else ['dotnet', binary]
                gallery_runner.execute([*launch, '--surface-audit'], variant / 'surface.log', env)
                gallery_runner.execute([*launch, '--safety'], variant / 'safety.log', env)
                row = dict(form=form, mode=mode, status='running', binarySha256=hashlib.sha256(binary.read_bytes()).hexdigest(),
                           surfaceLog=str(variant / 'surface.log'), safetyLog=str(variant / 'safety.log'), benchmarks=[])
                report['variants'].append(row)
                for repetition in range(args.repetitions):
                    log = variant / f'benchmark-{repetition}.log'
                    gallery_runner.execute([*launch, '--benchmark'], log, env)
                    row['benchmarks'].append(dict(log=str(log), rows=parse_bench(log)))
                row['status'] = 'passed'
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error))
        raise
    finally:
        (work / 'evidence.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__':
    main()
