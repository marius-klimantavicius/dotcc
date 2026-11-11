"""Re-emit CUI at a second physical root and compare exact objects/executable C#."""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
from types import SimpleNamespace

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.recipes import helper


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--source', type=Path, required=True)
    p.add_argument('--reference-work', type=Path, required=True, help='Completed translation staging directory, containing objects and raw products')
    p.add_argument('--compiler', type=Path, required=True)
    p.add_argument('--reference-core', type=Path, help='Published raw core if transactional delivery moved it from reference work')
    p.add_argument('--reference-gallery', type=Path, help='Published raw gallery if transactional delivery moved it from reference work')
    p.add_argument('--work-root', type=Path, required=True)
    p.add_argument('--jobs', type=int, default=4)
    args = p.parse_args()
    work, reference = args.work_root.resolve(), args.reference_work.resolve()
    if work.is_relative_to(ROOT.parent) or work.is_relative_to(args.source.resolve()) or work.is_relative_to(reference):
        p.error('Use separate external storage for the relocated attempt')
    work.mkdir(parents=True, exist_ok=True)
    (work / 'tmp').mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(work / 'tmp'), TMP=str(work / 'tmp'), TEMP=str(work / 'tmp'), PYTHONDONTWRITEBYTECODE='1')
    report = dict(status='running', sourceRoot=str(work / 'source'), referenceRoot=str(reference / 'source'),
                  comparison='Exact complete bytes, without redaction or identifier masks', rows=[], windows='unrun', compilerSha256=sha(args.compiler))
    def run(command, label, timeout=600):
        log = work / 'logs' / (label + '.log'); log.parent.mkdir(exist_ok=True)
        with log.open('wb') as output:
            subprocess.run(list(map(str, command)), check=True, stdout=output, stderr=subprocess.STDOUT, env=env, timeout=timeout)
    try:
        recipe = helper(ROOT / 'scripts/campaign.py')
        ctx = SimpleNamespace(root=ROOT, work=work, sources={'product':args.source.resolve()}, receipt={},
                              recipe=recipe.recipe, compiler=args.compiler.resolve(), options=SimpleNamespace(timeout=600), run=run)
        spec = recipe.prepare(ctx)
        objects = work / 'objects'; objects.mkdir()
        def emit(row):
            i, unit = row
            output = objects / f'{i:03d}-{unit.path.stem}.cs'
            run(['dotnet', ctx.compiler, *spec.flags, *unit.flags, unit.path, '--emit=obj', '-o', output], f'emit-{i:03d}')
            return output
        with ThreadPoolExecutor(max_workers=args.jobs) as pool: paths = list(pool.map(emit, enumerate(spec.units)))
        run(['dotnet', ctx.compiler, *paths, '--emit=managedlib', *spec.link_flags, '-o', work / 'raw/TranslatedCui'], 'core-link')
        spec.companions(ctx)
        for directory, pattern in [('objects','*.cs'), ('gallery-objects','*.cs'),
                                   ('raw/TranslatedCui','*.cs'), ('raw/TranslatedCuiGallery','*.cs'),
                                   ('raw/TranslatedCui','Dotcc.ManagedSymbols.json')]:
            expected_directory = reference / directory
            if directory == 'raw/TranslatedCui' and args.reference_core: expected_directory = args.reference_core
            if directory == 'raw/TranslatedCuiGallery' and args.reference_gallery: expected_directory = args.reference_gallery
            expected = sorted(expected_directory.glob(pattern))
            actual = sorted((work / directory).glob(pattern))
            if not expected or [f.name for f in expected] != [f.name for f in actual]:
                raise RuntimeError('Missing or different source file inventory: ' + directory)
            for a,b in zip(expected,actual,strict=True):
                equal = a.read_bytes() == b.read_bytes()
                identifiers = lambda f: sorted(set(re.findall(rb'__unit_[A-F0-9]{64}|__Anon[A-F0-9]{64}',f.read_bytes())))
                names_equal = identifiers(a) == identifiers(b)
                report['rows'].append(dict(file=str(b.relative_to(work)), referenceSha256=sha(a), relocatedSha256=sha(b),
                                           bytes=b.stat().st_size, exact=equal, generatedIdentifiersExact=names_equal))
                if not equal: raise RuntimeError('Relocation changed emitted bytes: ' + str(b.relative_to(work)))
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', error=str(error)); raise
    finally:
        (work / 'evidence.json').write_text(json.dumps(report,indent=2)+'\n')


if __name__ == '__main__': main()
