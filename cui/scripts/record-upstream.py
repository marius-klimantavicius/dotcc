"""Bind a completed upstream-refresh matrix to portable source/product evidence."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--work-root', type=Path, required=True)
    p.add_argument('--upstream-work', type=Path, required=True, help='Pinned archive/compare and independently passed native controls')
    p.add_argument('--translation-receipt', type=Path, required=True)
    p.add_argument('--integration-receipt', type=Path, required=True)
    p.add_argument('--baseline-commit', required=True)
    p.add_argument('--source-commit', required=True)
    p.add_argument('--compiler-commit', required=True)
    p.add_argument('--compiler-implementation-log', type=Path, required=True)
    p.add_argument('--output', type=Path, required=True)
    args = p.parse_args()
    work = args.work_root.resolve()
    upstream_work = args.upstream_work.resolve()
    source = json.loads((ROOT / 'config/source.json').read_text())
    receipts = {}

    def read(name, path, passing=True):
        d = json.loads(path.read_text())
        if passing and d.get('status') != 'passed': raise RuntimeError(name + ' did not pass')
        receipts[name] = dict(path=str(path.resolve()), sha256=sha(path))
        return d

    translation = read('translation', args.translation_receipt)
    integration = read('integration', args.integration_receipt)
    native = read('native', upstream_work / 'native-qualification/result.json')
    initial = read('preliminaryCore', upstream_work / 'core-qualification/result.json', False)
    current_core = read('core', work / 'core-final/result.json')
    relocation = read('relocation', work / 'relocated-reproduction/evidence.json')
    library = read('library', work / 'library-matrix/evidence.json')
    gallery = read('gallery', work / 'gallery-final/evidence.json')
    product = read('product', work / 'product-final/evidence.json')
    native_gallery = read('nativeGallery', upstream_work / 'native-gallery/report.json')
    compare = json.loads((upstream_work / 'upstream-compare.json').read_text())
    head = json.loads((work / 'upstream-head-final.json').read_text())
    if head['revision'] != source['revision']: raise RuntimeError('Upstream head advanced after selection')
    expected = {(f, m) for f in ('raw', 'processed') for m in ('jit', 'aot')}
    for name, report in [('library', library), ('gallery', gallery), ('product', product)]:
        if len(report['variants']) != 4 or {(v['form'], v['mode']) for v in report['variants']} != expected:
            raise RuntimeError(name + ': incomplete execution matrix')
        if any(v['status'] != 'passed' for v in report['variants']): raise RuntimeError(name + ': incomplete variant')
    for v in library['variants']:
        for kind in ['surface', 'safety']:
            log = Path(v[kind + 'Log'])
            v[kind + 'Summary'] = [line for line in log.read_text().splitlines()
                                  if line.startswith(('SURFACE\t', 'SAFETY\t', 'PASS assertions='))]
            v[kind + 'LogSha256'] = sha(log)
    if gallery['captureOnly'] or gallery['palettes'] != 'all': raise RuntimeError('Incomplete gallery palette/input scope')
    if current_core['source']['revision'] != source['revision'] or native['source']['revision'] != source['revision']:
        raise RuntimeError('Core receipt source mismatch')
    core = current_core['programs']
    if any(v['status'] != 'passed' for v in core.values()): raise RuntimeError('Unresolved core program failure')
    result = dict(status='passed', scope='Upstream source upgrade and complete current Linux qualification',
        baselineCommit=args.baseline_commit, sourceCommit=args.source_commit, compilerCommit=args.compiler_commit,
        upstream=source, upstreamHeadVerification=head, windows='unrun', receipts=receipts,
        upstreamCommits=[dict(revision=c['sha'], title=c['commit']['message'].splitlines()[0], url=c['html_url']) for c in compare['commits']],
        upstreamFiles=[dict(path=f['filename'], status=f['status'], additions=f['additions'], deletions=f['deletions']) for f in compare['files']],
        archiveSha256=sha(upstream_work / ('cui-' + source['revision'] + '.tar.gz')),
        regeneration=dict(status=translation['status'], coreUnits=58, galleryUnits=2,
                          adaptations=translation['adaptations'], tools=translation['tools'], sdk=translation['dotnet_sdk'],
                          sourceIdentity=translation['source_identity'], relocation=relocation),
        native=dict(status=native['status'], ctest=native['native_ctest'], programs=native['programs']),
        core=dict(status='passed', programs=core, harnessSha256=current_core['harness_sha256'],
                  toolSha256=current_core['tool_sha256'], sourceEmissions=current_core.get('source_emissions', []),
                  libraryObjects=current_core.get('library_objects', {}),
                  initialAttempt=dict(status=initial['status'], sourceHarnessSha256=initial['harness_sha256'],
                    finding='All actual core checks and all 70 groups matched. Historical fixed group-count gate expected 69; source-inventoried group gate added and rerun.',
                    initialCore=initial['programs']['cui_tests'])),
        integration=dict(status=integration['status'], dependencyAudit=integration['dependency_audit'],
                         commands=[dict(label=c['label'], status=c['status'], log=c.get('stdout')) for c in integration['commands']]),
        library=library, product=product, gallery=dict(status='passed', variants=[]))
    for v in gallery['variants']:
        row = {k:v[k] for k in ('form', 'mode', 'status', 'translatedBinarySha256', 'wrapperBinarySha256', 'wrapperInputReport', 'extendedState', 'responsiveState', 'emojiComparisons', 'galleryBenchmarks')}
        row['palettes'] = []
        if len(v['palettes']) != 16: raise RuntimeError('Missing palette')
        for palette in v['palettes']:
            cases = []
            for page, (a, b) in enumerate(zip(palette['translatedComparisons'], palette['nativeComparisons'], strict=True)):
                if any(c['status'] != 'passed' or c['page'] != page or not c['textEqual'] or not c['layoutEqual'] for c in (a,b)):
                    raise RuntimeError('Failed or misordered page comparison')
                cases.append([page, a['maximumChannelError'], b['maximumChannelError']])
            if len(cases) != 27: raise RuntimeError('Missing page')
            row['palettes'].append(dict(theme=palette['theme'], accent=palette['accent'], cases=cases))
        result['gallery']['variants'].append(row)
    result['gallery']['caseSchema'] = '[page, translated maximum channel error, native maximum channel error]; all text and float32 node-layout comparisons pass exactly; no masks or omitted pages'
    result['gallery']['clockFixture'] = gallery['clockFixture']
    result['gallery']['nativeBinarySha256'] = native_gallery['nativeBinarySha256']
    result['gallery']['totals'] = dict(wrapperCaptures=1728, translatedCaptures=1728, nativePaletteCaptures=432)
    result['focusedLogs'] = {}
    for name in ['native-app-smoke.log', 'recipe-tests.log', 'solution-final.log', 'compiler-tests.log']:
        log = (upstream_work if name == 'native-app-smoke.log' else work) / name
        result['focusedLogs'][name] = dict(path=str(log), sha256=sha(log), tail=log.read_text().splitlines()[-12:])
    result['focusedLogs']['compiler-implementation-tests'] = dict(path='${COMPILER_IMPLEMENTATION_TEST_LOG}',
        sha256=sha(args.compiler_implementation_log), tail=args.compiler_implementation_log.read_text().splitlines()[-5:])
    dirs = ['config', 'src/Host', 'src/ManagedCui', 'src/Glfw', 'samples/ManagedGallery', 'samples/ManagedConsumer',
            'tests/app-id', 'tests/host-glfw', 'tests/managed', 'tests/managed-gallery', 'tests/gallery', 'tests/recipe', 'tests/qualification', 'tests/source-identity', 'scripts/managed-api']
    files = [p for folder in dirs for p in (ROOT / folder).rglob('*')
             if p.is_file() and not any(part in ('bin','obj','__pycache__') for part in p.relative_to(ROOT).parts)]
    files += [ROOT / 'scripts/qualify.py', ROOT / 'scripts/suites.py', ROOT / 'scripts/record-upstream.py',
              ROOT / 'scripts/source_identity.py', ROOT / 'scripts/campaign.py', ROOT / 'scripts/gallery.py']
    result['authoredSourceHashes'] = {str(p.relative_to(ROOT.parent)):sha(p) for p in sorted(set(files))}
    result['productHashes'] = {str(p.relative_to(ROOT.parent)):sha(p) for p in sorted((ROOT / 'generated').rglob('*'))
        if p.is_file() and not any(part in ('bin','obj') for part in p.relative_to(ROOT / 'generated').parts)}
    result['limitations'] = ['Actual Windows execution remains unrun', 'Native Wayland and direct CUI platform backends remain outside scope',
        'About trailing-newline caret issue and static Markdown hint remain upstream behavior',
        'Performance gates concern measured UI phases; GL/vsync/rasterization are not timed; full gallery pages still allocate',
        'Prior validation JSON and performance artifact bindings remain historical']
    text = json.dumps(result, indent=2)
    for path, token in [(work, '${CUI_REFRESH_WORK}'), (upstream_work, '${CUI_UPSTREAM_INPUT_WORK}'), (args.translation_receipt.resolve().parent, '${CUI_TRANSLATION_RECEIPT_DIR}'),
                        (args.integration_receipt.resolve().parent, '${CUI_INTEGRATION_RECEIPT_DIR}'), (ROOT.parent.resolve(), '${REPO_ROOT}')]:
        text = text.replace(str(path), token)
    args.output.write_text(text + '\n')


if __name__ == '__main__': main()
