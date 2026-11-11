"""Build and execute the standalone safe showcase through ordinary references."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location('gallery_product_runner', Path(__file__).with_name('run.py'))
runner = importlib.util.module_from_spec(spec); spec.loader.exec_module(runner)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True)
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    args = parser.parse_args()
    work = args.work_root.resolve()
    if work.is_relative_to(runner.ROOT.parent): parser.error('Choose external storage for all outputs.')
    work.mkdir(parents=True, exist_ok=True)
    temp = work / 'tmp'; temp.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temp), TMP=str(temp), TEMP=str(temp))
    for key in ('CUI_THEME', 'CUI_ACCENT', 'CUI_SCREENSHOT', 'CUI_GALLERY_EXPECTED_EPOCH', 'LD_PRELOAD', 'FAKETIME'):
        env.pop(key, None)
    report = dict(status='running', windows='unrun', variants=[])
    try:
        with runner.reference.display(work, env, True) as (runtime, display):
            report['display'] = display
            for form in (('raw', 'processed') if args.form == 'all' else (args.form,)):
                for mode in (('jit', 'aot') if args.mode == 'all' else (args.mode,)):
                    variant = work / f'{form}-{mode}'; variant.mkdir(exist_ok=True)
                    artifacts = variant / 'artifacts'
                    _, binary = runner.build(runner.ROOT / 'samples/ManagedGallery/ManagedGallery.csproj', artifacts, form, mode, variant / 'build.log', env)
                    isolated = variant / 'standalone'
                    shutil.copytree(binary.parent, isolated, dirs_exist_ok=True)
                    executable = isolated / binary.name
                    launch = [executable] if mode == 'aot' else ['dotnet', executable]
                    cwd = variant / 'empty-cwd'; cwd.mkdir(exist_ok=True)
                    runner.reference.execute([*launch, '--list-pages'], variant / 'pages.log', runtime, 60, cwd)
                    names = (variant / 'pages.log').read_text().splitlines()
                    if len(names) != 27 or not names[0].startswith('0\tOverview') or not names[-1].startswith('26\tInstruments'):
                        raise RuntimeError('Standalone page registry is incomplete')
                    bad = subprocess.run([*map(str, launch), '--page', '27'], cwd=cwd, env=runtime, capture_output=True, text=True, timeout=60)
                    if bad.returncode != 1 or 'Page index' not in bad.stderr:
                        raise RuntimeError('Invalid standalone page selection was accepted')
                    smoke = [*launch, '--gallery-smoke', '--capture', variant / 'last-page.rgba']
                    if shutil.which('strace'):
                        smoke = ['strace', '-f', '-e', 'trace=file', '-o', variant / 'files.log', *smoke]
                    runner.reference.execute(smoke, variant / 'smoke.log', runtime, 600, cwd)
                    text = (variant / 'smoke.log').read_text()
                    if '27 pages, 108 frames; disposal and reinitialization passed.' not in text:
                        raise RuntimeError('Standalone lifecycle/page smoke did not complete')
                    if (variant / 'last-page.rgba').stat().st_size != 1240 * 820 * 4:
                        raise RuntimeError('Standalone capture size is incorrect')
                    deps = isolated / 'ManagedGallery.deps.json'
                    dependencies = list(json.loads(deps.read_text())['libraries']) if deps.exists() else []
                    if any('TranslatedCuiGallery' in name for name in dependencies) or any(isolated.glob('TranslatedCuiGallery*')):
                        raise RuntimeError('Standalone showcase depends on the translated gallery oracle')
                    if mode == 'jit' and {name.split('/')[0] for name in dependencies} != {'ManagedGallery', 'ManagedCui', 'TranslatedCui', 'Glfw'}:
                        raise RuntimeError('Unexpected standalone managed dependency graph')
                    needed = ''
                    if mode == 'aot':
                        needed = subprocess.check_output(['readelf', '-d', executable], text=True)
                        if any('cui' in line.lower() or 'glfw' in line.lower() for line in needed.splitlines() if 'NEEDED' in line):
                            raise RuntimeError('Unexpected native CUI/GLFW dependency')
                        (variant / 'native-dependencies.log').write_text(needed)
                    trace = variant / 'files.log'
                    if trace.exists():
                        reads = trace.read_text()
                        if str(artifacts) in reads or str(runner.ROOT.resolve()) in reads:
                            raise RuntimeError('Standalone execution accessed source or original build output')
                    report['variants'].append(dict(form=form, mode=mode, status='passed',
                        binarySha256=hashlib.sha256(executable.read_bytes()).hexdigest(), dependencies=dependencies,
                        pages=27, frames=108, unsafeCallerBuild=False, captureBytes=1240 * 820 * 4,
                        lifecycle='dispose twice, disposed reuse rejected, reopen succeeds',
                        closure='Copied outputs executed from empty cwd; source/original-output file access rejected by strace' if trace.exists() else 'Copied outputs executed from empty cwd; managed/native dependency graph audited',
                        log=str(variant / 'smoke.log')))
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error)); raise
    finally:
        (work / 'evidence.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__': main()
