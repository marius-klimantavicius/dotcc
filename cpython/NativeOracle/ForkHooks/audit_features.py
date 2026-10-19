"""Independent actual-receipt/source/object/compiled-profile feature audit.

Requires B's metadata/initializer inspection; does not build or run an engine.
"""
import argparse
import ast
import hashlib
import json
from pathlib import Path
import re
import sys

CPYTHON = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(CPYTHON.parent/'Scripts'))
from campaigns.recipes import helper


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def audit(receipt_path, previous_path, metadata_path, feature_stage='fork-only'):
    receipt_path, previous_path = Path(receipt_path).resolve(), Path(previous_path).resolve()
    receipt_bytes = receipt_path.read_bytes()
    receipt, previous = json.loads(receipt_bytes), json.loads(previous_path.read_bytes())
    assert receipt['profile'] == previous['profile'] == 'upstream-tests-static'
    assert receipt['status'] == 'passed' and len(receipt['units']) == 286
    policy = receipt['profile_feature_policy']
    assert policy['profile'] == receipt['profile']
    assert digest(policy['policy']) == policy['policy_sha256']
    assert digest(policy['resolver']) == policy['resolver_sha256']
    assert digest(policy['header']) == policy['header_sha256']
    assert feature_stage in ('fork-only', 'bootstrap-imports', 'bootstrap-identity')
    selected = {'HAVE_FORK': '1'}
    added = {'HAVE_FORK': '1'}
    protected = {'HAVE_FORK'}
    if feature_stage == 'bootstrap-imports':
        selected.update(HAVE_WAITPID='1', HAVE_SYS_WAIT_H='1')
        added = {'HAVE_STRFTIME': '1', 'HAVE_WAITPID': '1', 'HAVE_SYS_WAIT_H': '1'}
        protected.update(added)
        protected.add('HAVE_WCSFTIME')
        old_values = previous['effective_pyconfig']['values']
        assert old_values['HAVE_FORK'] == '1'
        assert not (set(added) & set(old_values))
        assert policy['uniform_features'] == {
            'HAVE_FORK': '1', 'HAVE_STRFTIME': '1', 'HAVE_SYS_WAIT_H': '1',
            'HAVE_WAITPID': '1', 'HAVE_WCSFTIME': None}
    if feature_stage == 'bootstrap-identity':
        selected.update(HAVE_WAITPID='1', HAVE_SYS_WAIT_H='1')
        added = {'HAVE_GETPID': '1'}
        protected.update(('HAVE_GETPID', 'HAVE_STRFTIME', 'HAVE_WAITPID', 'HAVE_SYS_WAIT_H', 'HAVE_WCSFTIME'))
        old_values = previous['effective_pyconfig']['values']
        assert old_values['HAVE_FORK'] == old_values['HAVE_STRFTIME'] == old_values['HAVE_WAITPID'] == old_values['HAVE_SYS_WAIT_H'] == '1'
        assert 'HAVE_GETPID' not in old_values
        assert policy['uniform_features'] == {
            'HAVE_FORK':'1', 'HAVE_GETPID':'1', 'HAVE_STRFTIME':'1',
            'HAVE_SYS_WAIT_H':'1', 'HAVE_WAITPID':'1', 'HAVE_WCSFTIME':None}
    assert policy['selected_features'] == selected
    effective = receipt['effective_pyconfig']
    assert hashlib.sha256(json.dumps(effective, sort_keys=True, separators=(',', ':')).encode()).hexdigest() == policy['effective_sha256']
    header = Path(policy['header']).read_text()
    definitions = dict(re.findall(r'^#define[ \t]+(\w+)[ \t]+(.+)$', header, re.M))
    assert definitions == effective['values']
    assert effective['values'] == {**previous['effective_pyconfig']['values'], **added}
    assert effective['undefined'] == previous['effective_pyconfig']['undefined']
    assert 'HAVE_WCSFTIME' not in effective['values']
    if feature_stage in ('bootstrap-imports', 'bootstrap-identity'):
        assert 'import_only_wait_contract' in policy['contract']
    source = CPYTHON/receipt['staging']/'source'
    old_source = CPYTHON/previous['staging']/'source'
    old_units = [Path(p) for p in previous['units']]
    commands = [row for row in receipt['commands'] if '--emit=obj' in row['command']]
    assert len(commands) == len(policy['units']) == len(old_units) == 286
    source_rows, layouts, runtime_layouts = [], [], []
    for index, (bound, old, name) in enumerate(zip(policy['units'], old_units, receipt['units'])):
        path = Path(name)
        selected = [row for row in commands if str(path) in row['command']]
        assert len(selected) == 1
        command = selected[0]
        assert bound['path'] == str(path)
        assert command['status'] == 'passed' and command['exit_code'] == 0
        argv = command['command']
        options = argv[2:argv.index(str(path))]
        report = options.index('--override-report')
        del options[report:report+2]
        assert options == bound['ordered_options']
        # Verify all actual ordered -D/-U, including helpers' source/API roles.
        macros = helper(CPYTHON/'tests/Audit/helper_profile.py').entry_macros(options)
        assert not (protected & set(macros))
        words = iter(options)
        directives = []
        for word in words:
            if word in ('-D', '-U'):
                directives.append((word, next(words)))
            elif word.startswith(('-D', '-U')):
                directives.append((word[:2], word[2:]))
        assert all(value.split('=', 1)[0] not in protected for _, value in directives)
        assert options[options.index('-I')+1] == str(source.parent/'config')
        assert digest(path) == digest(old)
        if path.is_relative_to(source):
            assert path.relative_to(source) == old.relative_to(old_source)
        else:
            assert path == old
        source_rows.append({'path': str(path), 'sha256': digest(path), 'ordered_options': options,
                            'ordered_macro_directives': directives, 'entry_macros': macros})
        obj = source.parent/'objects'/(str(index).zfill(3)+'-'+path.stem+'.cs')
        text = obj.read_text()
        signature = re.search(r'^//!!dotcc-obj aggregate:_is .+$', text, re.M)
        runtime_signature = re.search(r'^//!!dotcc-obj aggregate:pyruntimestate .+$', text, re.M)
        if re.search(r'public \w+ _main_interpreter;', text):
            assert runtime_signature
            runtime_layouts.append({'object': str(obj), 'sha256': digest(obj), 'signature': runtime_signature.group()})
        if 'before_forkers;' in text:
            assert signature
            assert 'after_forkers_parent;' in text and 'after_forkers_child;' in text
            layouts.append({'object': str(obj), 'sha256': digest(obj), 'signature': signature.group()})
    assert layouts and len({row['signature'] for row in layouts}) == 1
    assert runtime_layouts and len({row['signature'] for row in runtime_layouts}) == 1
    headers = []
    for path in sorted((source/'Include').rglob('*.h')):
        old = old_source/path.relative_to(source)
        assert digest(path) == digest(old)
        headers.append({'path': str(path), 'sha256': digest(path)})
    # Include source and header branches, including the actual pthread backend;
    # no selected file may redefine the globally chosen layout feature.
    paths = {Path(p) for p in receipt['units']}
    paths.update((source/'Include').rglob('*.h'))
    paths.update((source/'Python').glob('*.h'))
    paths.add(source/'Modules/clinic/posixmodule.c.h')
    overrides = []
    inert_undefs = []
    for path in sorted(paths):
        for match in re.finditer(r'^\s*#\s*(define|undef)\s+('+ '|'.join(sorted(protected))+r')\b', path.read_text(), re.M):
            if (match.groups() == ('undef', 'HAVE_WCSFTIME')
                    and path == source/'Modules/timemodule.c'
                    and 'HAVE_WCSFTIME' not in effective['values']):
                inert_undefs.append({'path': str(path), 'sha256': digest(path),
                                    'directive': match.group().strip()})
            else:
                overrides.append(str(path))
    assert not overrides
    table = source/'Modules/config.c'
    assert table.read_bytes() == (old_source/'Modules/config.c').read_bytes()
    module = source.parent/'objects/161-posixmodule.cs'
    hooks = ['PyOS_BeforeFork', 'PyOS_AfterFork_Parent', 'PyOS_AfterFork_Child']
    for hook in hooks:
        assert '//!!dotcc-obj def:'+hook+'\n' in module.read_text()
    assets = {}
    for form, product in receipt['outputs'].items():
        project = Path(product['project'])
        files = list((project.parent/'stdlib').glob('_sysconfigdata_*.py'))
        assert len(files) == 1
        tree = ast.parse(files[0].read_text())
        values = ast.literal_eval(next(n.value for n in tree.body if isinstance(n, ast.Assign)
                                      and any(isinstance(t, ast.Name) and t.id == 'build_time_vars' for t in n.targets)))
        assert values['HAVE_FORK'] == 1 and values['SIZEOF_WCHAR_T'] == 2
        if feature_stage in ('bootstrap-imports', 'bootstrap-identity'):
            assert all(values[name] == 1 for name in added)
            assert 'HAVE_WCSFTIME' not in values
        assets[form] = {'path': str(files[0]), 'sha256': digest(files[0]),
                        'features': {name: values.get(name) for name in sorted(protected)}}
    compiled = helper(CPYTHON/'tests/Audit/helper_profile.py').audit(receipt_path, CPYTHON, metadata_path)
    assert compiled['ready']
    assert receipt_path.read_bytes() == receipt_bytes
    return {'ready': True, 'scope': 'effective target policy, every actual ordered macro option/source, layout-sensitive object agreement, sysconfig and independent helper-role/actual compiled PE registration readiness; no upstream assertion pass',
            'receipt': str(receipt_path), 'receipt_sha256': digest(receipt_path),
            'feature_stage': feature_stage, 'exact_added_features': added,
            'previous_failed_receipt': str(previous_path), 'policy': policy,
            'source_rows': source_rows, 'layout_objects': layouts, 'runtime_layout_objects': runtime_layouts, 'unchanged_upstream_headers': headers,
            'feature_override_sources': overrides, 'inert_absent_service_undefs': inert_undefs,
            'sysconfig': assets, 'compiled_helper_review': compiled,
            'child_execution': 'unrun; no process creation support', 'inherited_v9_proofs': []}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--receipt', type=Path, required=True)
    parser.add_argument('--previous', type=Path, required=True)
    parser.add_argument('--metadata', type=Path, required=True)
    parser.add_argument('--feature-stage', choices=('fork-only', 'bootstrap-imports', 'bootstrap-identity'), default='fork-only')
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.receipt, args.previous, args.metadata, args.feature_stage)
    args.report.write_text(json.dumps(result, indent=2)+'\n')
    print(json.dumps({'ready': result['ready'], 'units': len(result['source_rows']), 'layouts': len(result['layout_objects'])}))
