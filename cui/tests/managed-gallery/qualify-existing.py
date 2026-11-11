"""Recheck retained gallery captures with native node observations and timing gates."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import sys

sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location('managed_gallery_runner', Path(__file__).with_name('run.py'))
runner = importlib.util.module_from_spec(spec); spec.loader.exec_module(runner)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--matrix-root', type=Path, required=True)
    parser.add_argument('--native-reference', type=Path, required=True)
    parser.add_argument('--faketime-library', type=Path, required=True)
    parser.add_argument('--work-root', type=Path, required=True)
    args = parser.parse_args()
    matrix = json.loads((args.matrix_root / 'evidence.json').read_text())
    if matrix['status'] != 'passed' or len(matrix['variants']) != 4 or matrix['palettes'] != 'all':
        raise RuntimeError('Requires a passing full four-variant, sixteen-palette matrix')
    native = json.loads((args.native_reference / 'report.json').read_text())
    binary = args.native_reference / 'native/build/gallery-control'
    if native['status'] != 'passed' or runner.sha(binary) != native['nativeBinarySha256']:
        raise RuntimeError('Requires a passed native observer binary with matching hash')
    fixture = Path(matrix['nativeReport']).parent / 'files-fixture'
    work = args.work_root.resolve(); work.mkdir(parents=True, exist_ok=True)
    temporary = work / 'tmp'; temporary.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temporary), TMP=str(temporary), TEMP=str(temporary))
    report = dict(status='running', variants=[], nativeBinarySha256=runner.sha(binary),
                  nativeReport=str(args.native_reference / 'report.json'),
                  layoutComparison='All node identities, parents, rectangle/font IEEE single-precision bits and text lengths exact; C nine-digit and C# round-trip formatting decoded to the same float bits',
                  clockFixture=matrix['clockFixture'])
    try:
        with runner.reference.display(work, env, True) as (runtime, display):
            report['display'] = display
            runner.reference.execute(['xdotool', 'mousemove', '1439', '999'], work / 'pointer.log', runtime)
            captures = {}
            for theme in range(2):
                for accent in range(8):
                    label = f'theme-{theme}-accent-{accent}'
                    folder = work / label; folder.mkdir(exist_ok=True)
                    selected = dict(runner.reference.fixed_clock_environment(runtime, args.faketime_library),
                                    CUI_GALLERY_TEST_THEME=str(theme), CUI_GALLERY_TEST_ACCENT=str(accent))
                    runner.reference.execute([binary, folder], work / (label + '.log'), selected, 600, fixture)
                    captures[(theme, accent)] = folder
            for variant in matrix['variants']:
                rows = []
                for palette in variant['palettes']:
                    theme, accent = palette['theme'], palette['accent']
                    label = f'theme-{theme}-accent-{accent}'
                    folder = args.matrix_root / (variant['form'] + '-' + variant['mode']) / label / 'wrapper'
                    comparison = runner.compare(captures[(theme, accent)], folder, maximum=1, native_layout=True)
                    rows.append(dict(theme=theme, accent=accent, comparisons=comparison))
                for trial in variant['galleryBenchmarks']:
                    runner.validate_gallery_bench(trial['rows'])
                failed = [(p['theme'], p['accent'], row['page']) for p in rows for row in p['comparisons'] if row['status'] != 'passed']
                report['variants'].append(dict(form=variant['form'], mode=variant['mode'], palettes=rows,
                                              galleryBenchmarkGates='passed', status='failed' if failed else 'passed'))
                if failed: raise RuntimeError(f'Native node/pixel mismatch: {failed}')
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error)); raise
    finally:
        (work / 'evidence.json').write_text(json.dumps(report, indent=2) + '\n')


if __name__ == '__main__': main()
