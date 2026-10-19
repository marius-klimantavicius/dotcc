"""Bind candidate policy to complete existing unit/header/asset closures.

Reads prior receipts as ordered source inputs only; never reuses a product PASS.
No compiler, subprocess, native call or lock is used.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import shutil
import sys
from types import SimpleNamespace

CPY = Path(__file__).resolve().parents[2]
WORK = CPY / 'build/owner-b-r42-20261018'
sys.path.insert(0, str(CPY.parent / 'Scripts'))
from campaigns.recipes import helper


def ref(path):
    path = Path(path).resolve()
    return dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest(), bytes=path.stat().st_size)


def read(path):
    return json.loads(Path(path).read_text())


def review(document_path, report):
    document = read(document_path)
    before = read(WORK / 'before/config/pyconfig-policy.json')
    expected = dict(before['values'], _Py_FORCE_UTF8_LOCALE='1')
    assert document['values'] == expected
    assert document['undefined'] == before['undefined'] and document['profiles'] == before['profiles']
    for key in set(before) - {'values'}:
        assert document[key] == before[key], key
    assert set(document) - set(before) == {'target_locale_contract'}
    config = helper(CPY / 'scripts/pyconfig.py')
    result = dict(scope='Complete source/effective-policy/header/asset-plan review only; no new product PASS',
                  candidate=ref(document_path), resolver=ref(CPY / 'scripts/pyconfig.py'), profiles={})
    paths = set()
    for profile, count, builtins, assets in (('embedded-gil', 193, 46, 188), ('upstream-tests-static', 286, 75, 2329)):
        publication_path = CPY / ('artifacts/owner-a-r32-20261018/final-' + profile + '-publication.json')
        publication = read(publication_path)
        receipt = read(publication['receipt']['path'])
        rows = receipt['profile_feature_policy']['units']
        assert len(rows) == count and [r['path'] for r in rows] == receipt['units']
        source = Path(rows[0]['path']).parents[1]
        private = WORK / 'policy-review' / profile
        (private / 'config').mkdir(parents=True, exist_ok=True)
        (private / 'scripts').mkdir(exist_ok=True)
        shutil.copyfile(document_path, private / 'config/pyconfig-policy.json')
        shutil.copyfile(CPY / 'scripts/pyconfig.py', private / 'scripts/pyconfig.py')
        policy = config.resolve(document, profile)
        header = private / 'config/pyconfig.h'
        header.write_text(config.render(policy))
        ctx = SimpleNamespace(root=private, profile=profile, receipt={})
        units = [SimpleNamespace(path=Path(row['path']), flags=tuple(row['ordered_options'])) for row in rows]
        config.bind(ctx, policy, header, units, [])
        for unit in units:
            paths.add(unit.path)
        paths.update(source.joinpath('Include').rglob('*.h'))
        table = (source / 'Modules/config.c').read_text()
        names = re.findall(r'\{"([^"]+)",\s*(?:Py\w+|_Py\w+|NULL)\}', table)
        assert len(names) == builtins, (profile, names)
        manifests = receipt['asset_manifests']
        assert manifests['raw']['assets'] == manifests['processed']['assets']
        entries = manifests['raw']['assets']
        assert len(entries) == assets
        sysconfigs = [r for r in entries if '/_sysconfigdata_' in r['path']]
        assert len(sysconfigs) == (profile == 'upstream-tests-static')
        old_vars = new_vars = None
        if sysconfigs:
            # Generate each policy with the actual target sysconfig generator.
            generator = helper(CPY / 'scripts/test_profile.py')
            for label, selected in (('before', before), ('candidate', document)):
                target = private / label
                (target / 'stdlib').mkdir(parents=True, exist_ok=True)
                (private / 'config/pyconfig-policy.json').write_text(json.dumps(selected))
                shutil.copyfile(CPY / 'config/profile.json', private / 'config/profile.json')
                output = generator.sysconfig_data(ctx, target)
                namespace = {'__file__': str(output)}
                exec(compile(output.read_text(), str(output), 'exec'), namespace)
                values = namespace['build_time_vars']
                values.pop('LIBDEST'); values.pop('BINLIBDEST')
                if label == 'before':
                    old_vars = values
                else:
                    new_vars = values
            assert new_vars == dict(old_vars, _Py_FORCE_UTF8_LOCALE=1)
            assert '_Py_FORCE_UTF8_FS_ENCODING' not in new_vars
            shutil.copyfile(document_path, private / 'config/pyconfig-policy.json')
        result['profiles'][profile] = dict(
            referenceReceipt=publication['receipt'], sourceRoot=str(source), header=ref(header),
            binding=ctx.receipt['profile_feature_policy'], orderedSources=[ref(unit.path) for unit in units],
            builtins=names, assetCount=len(entries), assetPlan=entries,
            sysconfigDelta={'_Py_FORCE_UTF8_LOCALE': 1} if sysconfigs else {},
            embeddedHasSysconfig=bool(sysconfigs) if profile == 'embedded-gil' else None,
            layout='No changed target size/layout values; full actual product ABI remains a fresh gate')
    native_source = CPY / 'build/owner-b-r37-20261018/native-full/source'
    paths.update(native_source.rglob('*.h'))
    paths.update(native_source / name for name in ('Python/preconfig.c', 'Python/pylifecycle.c',
        'Python/initconfig.c', 'Python/fileutils.c', 'Objects/unicodeobject.c', 'Modules/_localemodule.c'))
    paths.update((CPY / 'src/Host').glob('*'))
    paths.update((CPY / 'src/Managed.CPython').glob('*.cs'))
    paths.update(CPY.parent / name for name in ('DotCC.Libc/MultibyteLib.cs', 'DotCC.Libc/LocaleLib.cs',
        'DotCC.Lib/include/wchar.h', 'DotCC.Lib/include/locale.h', 'DotCC.Lib/include/langinfo.h'))
    files = [ref(p) for p in sorted(paths) if p.is_file()]
    hits = []
    for item in files:
        path = Path(item['path'])
        sites = [dict(line=n, text=line) for n, line in enumerate(path.read_text(errors='replace').splitlines(), 1)
                 if re.search(r'\b_Py_FORCE_UTF8_(?:LOCALE|FS_ENCODING)\b', line)]
        if sites:
            hits.append(dict(file=item, sites=sites))
    plan = (CPY / 'docs/PLAN.md').read_text()
    d10 = plan[plan.index('## Generated code simplification and constant data\n'):plan.index('## Milestones and evidence\n')]
    result.update(reviewedFiles=files, featureSites=hits, normativeD10Sha256=hashlib.sha256(d10.encode()).hexdigest(),
        conclusions={
            'fileutils': 'Both current_locale values select unchanged _Py_DecodeUTF8Ex/_Py_EncodeUTF8Ex; force-ASCII workaround returns zero. Generic per-unit routines are unreachable through these public boundaries.',
            'unicodeobject': 'FS fast/bootstrap UTF8 paths; upstream fs_codec.utf8==1 invariant. Authored Host exposes no custom filesystem encoding. Python length-aware FS decode retains embedded NUL; locale C helpers reject or truncate according to original API.',
            'initconfig': 'FS UTF8; default locale/stdio via _Py_GetLocaleEncoding; filesystem surrogateescape remains. No preconfig utf8_mode macro branch.',
            'pylifecycle': 'Only legacy locale-coercion codeset check is bypassed; isolated Host configure_locale=0 avoids coercion. No __ANDROID__/__APPLE__/__VXWORKS__ identity is introduced.',
            'lower_locale': 'nl_langinfo/strcoll/localeconv and shared C/UTF8 multibyte services stay unchanged. Remaining fileutils wcstombs converts ASCII fopen mode, not filename.',
            'unsupported': 'No new host/runtime functions or layout fields; dynamic loading is native-control-only. Native test DSOs are not target inputs.',
            'proof': 'No new product/native parity claim; actual A32/B41 gates remain open'})
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(report=ref(report), profiles={k:len(v['orderedSources']) for k,v in result['profiles'].items()},
                          files=len(files), featureOwners=len(hits))))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--policy', type=Path, required=True)
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    review(args.policy, args.report)
