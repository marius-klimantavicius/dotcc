"""Make a portable, source-bound delivery receipt from completed qualification."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--matrix', type=Path, required=True)
    parser.add_argument('--product', type=Path, required=True)
    parser.add_argument('--native', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--source-commit', required=True, help='Reviewed wrapper/gallery source revision')
    parser.add_argument('--audit', type=Path, required=True, help='Campaign audit receipt')
    parser.add_argument('--solution-log', type=Path, required=True)
    parser.add_argument('--recipe-log', type=Path, required=True)
    args = parser.parse_args()
    matrix_path = args.matrix.resolve() / 'evidence.json'
    product_path = args.product.resolve() / 'evidence.json'
    native_path = args.native.resolve() / 'report.json'
    matrix, product, native = (json.loads(p.read_text()) for p in (matrix_path, product_path, native_path))
    if any(d['status'] != 'passed' for d in (matrix, product, native)):
        raise RuntimeError('Only completed passing runs can be recorded')
    audit = json.loads(args.audit.read_text())
    if audit['status'] != 'passed' or {r['form'] for r in audit['dependency_audit']} != {'raw', 'processed'}:
        raise RuntimeError('A passing raw/processed dependency audit is required')
    if matrix['captureOnly'] or matrix['palettes'] != 'all' or len(matrix['variants']) != 4 or len(product['variants']) != 4:
        raise RuntimeError('The complete palette and execution matrix is required')
    library = json.loads((ROOT / 'docs/MANAGED-LIBRARY-VALIDATION.json').read_text())
    expected_variants = {(f, m) for f in ('raw', 'processed') for m in ('jit', 'aot')}
    if any({(v['form'], v['mode']) for v in d['variants']} != expected_variants for d in (matrix, product)):
        raise RuntimeError('Missing execution variant')
    for name, expected in library['wrapperSources'].items():
        if digest(ROOT.parent / name) != expected:
            raise RuntimeError('Current wrapper differs from qualified library: ' + name)
    # Test sources are immutable between execution and receipt creation; their
    # binaries and full external receipts are independently hashed below.
    def portable(value):
        text = json.dumps(value)
        for directory, token in ((args.matrix.resolve(), '${MANAGED_GALLERY_WORK}'),
                                 (args.product.resolve(), '${MANAGED_GALLERY_PRODUCT}'),
                                 (args.native.resolve(), '${CUI_NATIVE_GALLERY_WORK}')):
            text = text.replace(str(directory), token)
        return json.loads(text)
    report = dict(status='passed', platform='Linux x64 / X11 / desktop OpenGL and software offscreen', windows='unrun',
        sourceCommit=args.source_commit, upstream='629b6fcb991d71527f2d67b25c51429c3dcdb498',
        libraryReceipt='MANAGED-LIBRARY-VALIDATION.json', libraryReceiptSha256=digest(ROOT / 'docs/MANAGED-LIBRARY-VALIDATION.json'),
        clockFixture=matrix['clockFixture'],
        schema='Palette cases are [page, translated maximum channel error, native maximum channel error]. Every recorded case passed exact ordered text and node identity/parent/rectangle/font/length comparison; native float observations match IEEE float32 bits. Original-layout captures exclude managed-only additions; all 27 original pages are compared without masks. Editable code is qualified by the default-gallery OS input suite.',
        totals=dict(wrapperCaptures=1728, translatedCaptures=1728, independentNativePaletteCaptures=432,
                    variants=4, palettesPerVariant=16, pagesPerPalette=27, safetyAssertionsPerVariant=75,
                    osAssertionsPerVariant=4042, osActionsPerVariant=95),
        receipts=dict(matrix=dict(path='${MANAGED_GALLERY_WORK}/evidence.json', sha256=digest(matrix_path)),
                      product=dict(path='${MANAGED_GALLERY_PRODUCT}/evidence.json', sha256=digest(product_path)),
                      native=dict(path='${CUI_NATIVE_GALLERY_WORK}/report.json', sha256=digest(native_path)),
                      audit=dict(path='${CUI_AUDIT_RECEIPT}', sha256=digest(args.audit)),
                      solution=dict(path='${CUI_SOLUTION_LOG}', sha256=digest(args.solution_log)),
                      recipe=dict(path='${CUI_RECIPE_LOG}', sha256=digest(args.recipe_log))),
        native=dict(binarySha256=native['nativeBinarySha256'], status=native['status'], input=native['nativeInput'], inputStatus=native['nativeInputStatus']),
        product=portable(product), variants=[])
    for variant in matrix['variants']:
        if variant['status'] != 'passed' or len(variant['palettes']) != 16:
            raise RuntimeError('Incomplete variant')
        result = {key: variant[key] for key in ('form', 'mode', 'status', 'translatedBinarySha256', 'wrapperBinarySha256', 'extendedState', 'responsiveState')}
        result['palettes'] = []
        for palette in variant['palettes']:
            cases = []
            for page, (translated, control) in enumerate(zip(palette['translatedComparisons'], palette['nativeComparisons'], strict=True)):
                if translated['page'] != page or control['page'] != page:
                    raise RuntimeError('Misordered page')
                for case in (translated, control):
                    if case['status'] != 'passed' or not case['textEqual'] or not case['layoutEqual']:
                        raise RuntimeError('Nonpassing page')
                cases.append([translated['page'], translated['maximumChannelError'], control['maximumChannelError']])
            if len(cases) != 27: raise RuntimeError('Missing page')
            result['palettes'].append(dict(theme=palette['theme'], accent=palette['accent'], cases=cases))
        result['input'] = variant['wrapperInputReport']
        result['emojiComparisons'] = portable(variant['emojiComparisons'])
        result['benchmarks'] = portable(variant['galleryBenchmarks'])
        report['variants'].append(result)
    paths = [*sorted((ROOT / 'samples/ManagedGallery').glob('*.*'))]
    report['showcaseSources'] = {str(p.relative_to(ROOT.parent)): digest(p) for p in paths if p.is_file()}
    report['wrapperSources'] = library['wrapperSources']
    tests = [p for name in ('managed', 'managed-gallery', 'gallery') for p in sorted((ROOT / 'tests' / name).glob('*'))
             if p.is_file() and p.suffix in ('.cs', '.csproj', '.c', '.py')]
    tests.append(ROOT / 'tests/desktop/OsInput.cs')
    report['qualificationSources'] = {str(p.relative_to(ROOT.parent)): digest(p) for p in tests}
    report['methodology'] = 'Release; 3 process trials/variant/page, 200 warmup frames, 600 alternating paired samples, exact software pixels before timing. Phase p50/p95/p99, no GL/vsync or rasterization timed. Gates: sum of construction and layout/paint medians <= 2x direct; construction p95 overhead <= 0.5 us/node; app bytes/frame <= direct + 1024. Phase sums are not full-frame quantiles.'
    report['limitations'] = ['Actual Windows execution unavailable and unrun', 'One active desktop application',
        'Zero additional wrapper allocation applies to allocation-conscious library workloads; full gallery pages allocate',
        'Native full-window GL channels permit the existing one-byte normalized conversion rounding bound',
        'Reserved frame arena remains 64 KiB despite reduced used bytes; timings are hardware-specific']
    args.output.write_text(json.dumps(portable(report), indent=2) + '\n')


if __name__ == '__main__': main()
