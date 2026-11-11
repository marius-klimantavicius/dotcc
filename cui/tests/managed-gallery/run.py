"""Qualify authored ManagedGallery against translated and adapted native galleries."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import platform
import struct
import sys

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('original_gallery_runner', ROOT / 'tests/gallery/run.py')
reference = importlib.util.module_from_spec(spec)
spec.loader.exec_module(reference)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def layout_values(path):
    rows = []
    for line in path.read_text().splitlines():
        index, identity, parent, rect, font, size, length = line.split('\t')
        # C %.9g and C# Single:R are different text formats for the same
        # IEEE float. Compare their round-tripped single-precision bits.
        values = tuple(struct.pack('<f', float(value)) for value in (*rect.split(','), size))
        rows.append((int(index), int(identity), int(parent), int(font), int(length), values))
    return rows


def compare(expected, actual, maximum=0, layout=True, native_layout=False):
    rows = []
    for page in range(27):
        stem = f'page-{page:02}'
        a, b = (expected / (stem + '.rgba')).read_bytes(), (actual / (stem + '.rgba')).read_bytes()
        if len(a) != 1240 * 820 * 4 or len(b) != len(a):
            raise RuntimeError(f'{stem}: invalid framebuffer length')
        delta = max((abs(x - y) for x, y in zip(a, b, strict=True)), default=0) if a != b else 0
        text_equal = (expected / (stem + '.text')).read_bytes() == (actual / (stem + '.text')).read_bytes()
        layout_equal = (layout_values(expected / (stem + '.layout')) == layout_values(actual / (stem + '.layout'))) if native_layout else (not layout or (expected / (stem + '.layout')).read_bytes() == (actual / (stem + '.layout')).read_bytes())
        rows.append(dict(page=page, rgbaBytes=len(a), maximumChannelError=delta,
                         textEqual=text_equal, layoutEqual=layout_equal,
                         referenceSha256=sha(expected / (stem + '.rgba')), wrapperSha256=sha(actual / (stem + '.rgba')),
                         status='passed' if delta <= maximum and text_equal and layout_equal else 'failed'))
    return rows


def build(project, artifacts, form, mode, log, env):
    core = ROOT / ('generated/TranslatedCui.Raw/TranslatedCui.csproj' if form == 'raw' else 'generated/TranslatedCui/TranslatedCui.csproj')
    gallery = ROOT / ('generated/TranslatedCuiGallery.Raw/TranslatedCuiGallery.csproj' if form == 'raw' else 'generated/TranslatedCuiGallery/TranslatedCuiGallery.csproj')
    command = ['dotnet', 'publish' if mode == 'aot' else 'build', project, '-c', 'Release', '--artifacts-path', artifacts,
               '-m:1', '--nologo', '-p:CuiVariant=' + ('Raw' if form == 'raw' else 'Processed'),
               '-p:CuiProject=' + str(core), '-p:CuiCoreProject=' + str(core), '-p:GalleryProject=' + str(gallery)]
    if mode == 'aot': command += ['-r', 'linux-x64', '-p:PublishAot=true']
    reference.execute(command, log, env, 3600)
    name = project.stem
    binary = artifacts / (f'publish/{name}/release_linux-x64/{name}' if mode == 'aot' else f'bin/{name}/release/{name}.dll')
    return ([binary] if mode == 'aot' else ['dotnet', binary]), binary


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--work-root', type=Path, required=True)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--native-reference', type=Path, help='Reuse a passed native-only run with its verified binary hash')
    parser.add_argument('--faketime-library', type=Path,
                        default=Path(os.environ['CUI_FAKETIME_LIBRARY']) if os.environ.get('CUI_FAKETIME_LIBRARY') else None,
                        help='libfaketime shared object on external storage (or CUI_FAKETIME_LIBRARY)')
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='all')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='all')
    parser.add_argument('--palettes', choices=('all', 'baseline'), default='all', help='All 2 themes x 8 accents, or a diagnostic light/neutral selection')
    parser.add_argument('--xvfb', action='store_true')
    parser.add_argument('--capture-only', action='store_true', help='Diagnostic capture comparison only; does not qualify OS interactions')
    args = parser.parse_args()
    if platform.system() != 'Linux': parser.error('Linux/X11 execution only; actual Windows remains unrun.')
    if not args.faketime_library or not args.faketime_library.is_file(): parser.error('Supply --faketime-library or CUI_FAKETIME_LIBRARY for the required wall-clock fixture.')
    work = args.work_root.resolve()
    if work.is_relative_to(ROOT.parent): parser.error('Choose external storage for all outputs.')
    work.mkdir(parents=True, exist_ok=True)
    temp = work / 'tmp'; temp.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temp), TMP=str(temp), TEMP=str(temp), PYTHONDONTWRITEBYTECODE='1')
    for key in ('CUI_THEME', 'CUI_ACCENT', 'CUI_GALLERY_DIAGNOSTIC_INVISIBLE', 'CUI_GALLERY_CAPTURE_ONLY', 'CUI_GALLERY_INPUT_ONLY', 'CUI_GALLERY_EXPECTED_EPOCH', 'CUI_GALLERY_TEST_THEME', 'CUI_GALLERY_TEST_ACCENT', 'CUI_GALLERY_RESPONSIVE_ONLY'):
        env.pop(key, None)
    native = args.native_reference.resolve() if args.native_reference else work / 'native-reference'
    if not args.native_reference:
        reference.execute([sys.executable, ROOT / 'tests/gallery/run.py', '--source', args.source,
                           '--work-root', native, '--native-only', '--xvfb', '--faketime-library', args.faketime_library], work / 'native.log', env, 3600)
    native_report = json.loads((native / 'report.json').read_text())
    if native_report['status'] != 'passed' or sha(native / 'native/build/gallery-control') != native_report['nativeBinarySha256']:
        raise RuntimeError('The native reference must be a passing current binary, including its real OS input suite.')
    if native_report.get('clockFixture', {}).get('epoch') != reference.FIXED_EPOCH:
        raise RuntimeError('The native reference must use the same validated frozen wall-clock fixture.')
    fixture = native / 'files-fixture'
    report = dict(status='running', windows='unrun', variants=[], nativeReport=str(native / 'report.json'),
                  comparison='Full-window RGBA, ordered main text and every node id/parent/rectangle/font: wrapper vs translated exact. Full-window adapted-native RGBA within established GL channel rounding bound 1; text exact. Original-layout captures exclude managed-only additions via IncludeManagedExamples=false; all 27 upstream pages still compare without masks. Default-gallery OS input also tests the editable code example.',
                  captureOnly=args.capture_only, palettes=args.palettes)
    try:
        with reference.display(work, env, args.xvfb) as (runtime, display):
            capture_runtime = dict(reference.fixed_clock_environment(runtime, args.faketime_library), CUI_GALLERY_CAPTURE_ONLY='1', CUI_GALLERY_UPSTREAM_LAYOUT='1')
            report['clockFixture'] = dict(epoch=reference.FIXED_EPOCH, timezone='UTC', librarySha256=sha(args.faketime_library))
            report['display'] = display
            palettes = [(theme, accent) for theme in range(2) for accent in range(8)] if args.palettes == 'all' else [(0, 0)]
            native_palettes = {(0, 0): native / 'native/captures'}
            for theme, accent in palettes:
                if (theme, accent) == (0, 0): continue
                label = f'theme-{theme}-accent-{accent}'
                folder = work / 'native-palettes' / label; folder.mkdir(parents=True, exist_ok=True)
                selected = dict(capture_runtime, CUI_GALLERY_TEST_THEME=str(theme), CUI_GALLERY_TEST_ACCENT=str(accent))
                reference.execute([native / 'native/build/gallery-control', folder], work / f'native-{label}.log', selected, 600, fixture)
                native_palettes[(theme, accent)] = folder
            forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
            modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
            for form in forms:
                for mode in modes:
                    variant = work / f'{form}-{mode}'; variant.mkdir(exist_ok=True)
                    artifacts = variant / 'artifacts'
                    original, original_binary = build(ROOT / 'tests/gallery/Gallery.Tests.csproj', artifacts, form, mode, variant / 'translated-build.log', env)
                    wrapper, wrapper_binary = build(ROOT / 'tests/managed-gallery/ManagedGallery.Tests.csproj', artifacts, form, mode, variant / 'wrapper-build.log', env)
                    result = dict(form=form, mode=mode, status='running', translatedBinarySha256=sha(original_binary),
                                  wrapperBinarySha256=sha(wrapper_binary), palettes=[])
                    report['variants'].append(result)
                    for theme, accent in palettes:
                        label = f'theme-{theme}-accent-{accent}'
                        selected = dict(capture_runtime, CUI_GALLERY_TEST_THEME=str(theme), CUI_GALLERY_TEST_ACCENT=str(accent))
                        oracle_capture = variant / label / 'translated'; wrapper_capture = variant / label / 'wrapper'
                        reference.execute(['xdotool', 'mousemove', '1439', '999'], variant / f'pointer-{label}.log', runtime)
                        reference.execute([*original, oracle_capture], variant / f'translated-{label}.log', selected, 1200, fixture)
                        reference.execute([*wrapper, wrapper_capture], variant / f'wrapper-{label}.log', selected, 1200, fixture)
                        rows = compare(oracle_capture, wrapper_capture)
                        native_rows = compare(native_palettes[(theme, accent)], wrapper_capture, maximum=1, native_layout=True)
                        result['palettes'].append(dict(theme=theme, accent=accent,
                                                       wrapperReport=json.loads((wrapper_capture / 'report.json').read_text()),
                                                       translatedComparisons=rows, nativeComparisons=native_rows))
                        if any(row['status'] != 'passed' for row in rows + native_rows):
                            failed = [row['page'] for row in rows + native_rows if row['status'] != 'passed']
                            raise RuntimeError(f'{form}-{mode}/{label}: mismatched pages {failed}; inspect retained pixels/text/layout')
                    if not args.capture_only:
                        oracle_input = variant / 'translated-input'; wrapper_input = variant / 'wrapper-input'
                        input_runtime = dict(runtime, CUI_GALLERY_INPUT_ONLY='1', CUI_GALLERY_UPSTREAM_LAYOUT='0')
                        reference.execute([*original, oracle_input], variant / 'translated-input.log', input_runtime, 1200, fixture)
                        reference.execute([*wrapper, wrapper_input], variant / 'wrapper-input.log', input_runtime, 1200, fixture)
                        result['wrapperInputReport'] = json.loads((wrapper_input / 'report.json').read_text())
                        expected = reference.parse_emoji(variant / 'translated-input.log')
                        actual = reference.parse_emoji(variant / 'wrapper-input.log')
                        reference.validate_inline_pixels(actual, wrapper_input)
                        for key in ('messages', 'recent', 'edits'):
                            if actual[key] != expected[key]: raise RuntimeError(f'{form}-{mode}: wrapper emoji {key} differs')
                        result['emojiComparisons'] = reference.compare_emoji(expected, actual, oracle_input, wrapper_input)
                        if any(row['maximumError'] for row in result['emojiComparisons']):
                            raise RuntimeError('Wrapper/translated emoji crop comparison must be exact')
                        left_state = [line for line in (variant / 'translated-input.log').read_text().splitlines() if line.startswith('EXTENDED_STATE\t')]
                        right_state = [line for line in (variant / 'wrapper-input.log').read_text().splitlines() if line.startswith('EXTENDED_STATE\t')]
                        if len(left_state) != 1 or left_state != right_state: raise RuntimeError('Extended OS state transcript differs')
                        result['extendedState'] = right_state[0]
                        narrow_expected = [line for line in (variant / 'translated-input.log').read_text().splitlines() if line.startswith('NARROW_STATE\t')]
                        narrow_actual = [line for line in (variant / 'wrapper-input.log').read_text().splitlines() if line.startswith('NARROW_STATE\t')]
                        if len(narrow_expected) != 1 or narrow_expected != narrow_actual: raise RuntimeError('Responsive navigation state differs')
                        result['responsiveState'] = narrow_actual[0]
                        result['galleryBenchmarks'] = []
                        for repetition in range(3):
                            log = variant / f'gallery-benchmark-{repetition}.log'
                            reference.execute([*wrapper, '--benchmark'], log, dict(runtime, DOTNET_TieredCompilation='0'), 600, fixture)
                            bench_rows = [dict(part.split('=', 1) for part in line.split('\t')[1:]) for line in log.read_text().splitlines() if line.startswith('GALLERY_BENCH\t')]
                            if len(bench_rows) != 3: raise RuntimeError('Expected three actual gallery benchmark pages')
                            validate_gallery_bench(bench_rows)
                            result['galleryBenchmarks'].append(dict(log=str(log), rows=bench_rows))
                    result['status'] = 'captures-only' if args.capture_only else 'passed'
        report['status'] = 'captures-only' if args.capture_only else 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error)); raise
    finally:
        (work / 'evidence.json').write_text(json.dumps(report, indent=2) + '\n')


def validate_gallery_bench(rows):
    for row in rows:
        wb = list(map(float, row['buildWrapperUs'].split(','))); db = list(map(float, row['buildDirectUs'].split(',')))
        we = list(map(float, row['endWrapperUs'].split(','))); de = list(map(float, row['endDirectUs'].split(',')))
        if wb[0] + we[0] > 2 * (db[0] + de[0]):
            raise RuntimeError(f'Gallery page {row["page"]}: median complete UI work exceeds twice direct CUI')
        if wb[1] - db[1] > int(row['nodes']) * .5:
            raise RuntimeError(f'Gallery page {row["page"]}: construction p95 overhead exceeds 0.5 us per node')
        if float(row['wrapperBytesPerFrame']) > float(row['directBytesPerFrame']) + 1024:
            raise RuntimeError(f'Gallery page {row["page"]}: additional authored-app allocation exceeds 1 KiB/frame')


if __name__ == '__main__': main()
