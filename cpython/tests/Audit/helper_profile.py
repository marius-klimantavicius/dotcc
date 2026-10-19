"""Read-only helper-profile readiness review; no preprocessing or execution.

Entry macro options are evaluated in recorded argv order. Header/source guards
are retained as evidence, not simulated by a second C preprocessor. Successful
real emissions plus compiled registrations remain required for readiness.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re

ROLES = ('_testcapi', '_testlimitedcapi', '_testinternalcapi')


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def entry_macros(argv):
    """Interpret only -D/-U options, including split options, in actual order."""
    result = {}
    words = iter(argv)
    for word in words:
        if word in ('-D', '-U'):
            option, value = word, next(words)  # malformed options must fail
        elif word.startswith(('-D', '-U')):
            option, value = word[:2], word[2:]
        else:
            continue
        name, _, definition = value.partition('=')
        if not re.fullmatch(r'[A-Za-z_]\w*', name):
            raise ValueError('Invalid macro option: ' + word)
        if option == '-U':
            result.pop(name, None)
        else:
            result[name] = definition if '=' in value else '1'
    return result


def role(name, source):
    if name != '_testcapi':
        return 'limited' if name == '_testlimitedcapi' else 'internal'
    # Only the directive before parts.h selects this upstream exception.
    prefix = source.split('#include', 1)[0]
    return ('public-with-internal-opt-in' if re.search(
        r'^\s*#\s*define\s+PYTESTCAPI_NEED_INTERNAL_API\b', prefix, re.M) else 'public')


def source_readiness(receipt_path, campaign):
    """Check source/emission guards only; no compiled initializer claim.

    Distinct consumer products require their own exact-input PE/IL review.
    The ordinary audit retains its canonical-output restriction unchanged.
    """
    return _audit(receipt_path, campaign, source_only=True)


def audit(receipt_path, campaign, metadata_path=None):
    return _audit(receipt_path, campaign, metadata_path)


def _audit(receipt_path, campaign, metadata_path=None, *, source_only=False):
    receipt_path, campaign = Path(receipt_path).resolve(), Path(campaign).resolve()
    receipt_bytes = receipt_path.read_bytes()
    receipt = json.loads(receipt_bytes)
    if receipt.get('profile') != 'upstream-tests-static' or receipt.get('project') != 'cpython':
        raise ValueError('Expected actual CPython test-profile receipt')
    definition = json.loads((campaign/'tests/Upstream/helper-contract.json').read_text())
    modules = receipt['test_profile']['modules']
    expected = {row['module']: row for row in definition['modules']}
    if len(modules) != len(expected) or {row['name'] for row in modules} != set(expected):
        raise ValueError('Helper manifest module closure differs')
    source = campaign/receipt['staging']/'source'
    commands = [row for row in receipt['commands'] if '--emit=obj' in row['command']]
    issues, rows, guards = [], [], []
    units = [Path(path).resolve() for path in receipt['units']]
    if len(units) != len(set(units)) or len(units) != receipt['test_profile']['selected_units']:
        raise ValueError('Selected profile unit inventory differs')
    base = json.loads((campaign/'config/sources.json').read_text())['units']
    authored = json.loads((campaign/'config/embedding.json').read_text())['units']
    required_units = {source/row['path'] for row in base}
    required_units.update(campaign/row['path'] for row in authored)
    required_units.update(source/path for row in expected.values() for path in row['sources'])
    if set(units) != {path.resolve() for path in required_units}:
        raise ValueError('Actual profile drops/adds units relative to product/Host/helper manifests')
    if len(commands) != len(units) or any(row['status'] != 'passed' or row.get('exit_code') != 0 for row in commands):
        issues.append({'error': 'Complete selected profile emission has not passed'})
    for name in ROLES:
        path = source/'Modules'/name/'parts.h'
        text = path.read_text()
        required = ['Python.h', 'Py_BUILD_CORE_BUILTIN']
        required += (['PYTESTCAPI_NEED_INTERNAL_API', '#  error'] if name == '_testcapi'
                     else ['Py_LIMITED_API', '#  error'] if name == '_testlimitedcapi'
                     else ['Py_BUILD_CORE_MODULE'])
        if any(value not in text for value in required):
            raise ValueError('Actual helper API guard missing: ' + str(path))
        guards.append({'path': str(path), 'sha256': digest(path), 'text': text})
    port = source/'Include/pyport.h'
    guards.append({'path': str(port), 'sha256': digest(port),
                   'core_implication': 'Py_BUILD_CORE_BUILTIN/Py_BUILD_CORE_MODULE imply Py_BUILD_CORE in actual pyport.h'})
    for module in modules:
        name = module['name']
        inventory = module['sources']
        wanted = expected[name]['sources']
        if len(inventory) != len(wanted) or [row['path'] for row in inventory] != wanted:
            raise ValueError('Helper manifest source closure differs: ' + name)
        for item in inventory:
            path = source/item['path']
            if digest(path) != item['sha256']:
                raise ValueError('Helper source differs from actual manifest: ' + str(path))
            if sum(Path(unit).resolve() == path for unit in receipt['units']) != 1:
                raise ValueError('Helper source missing/duplicated in selected units: ' + str(path))
            if name not in ROLES:
                continue
            text = path.read_text()
            api_role = role(name, text)
            actual = [row for row in commands if str(path) in row['command']]
            record = {'module': name, 'source': str(path), 'sha256': item['sha256'], 'role': api_role}
            if len(actual) != 1:
                record.update(status='unrun-or-ambiguous', entry_macros=None)
                issues.append({'source': str(path), 'error': 'Missing/ambiguous actual emission command'})
            else:
                command = actual[0]
                macros = entry_macros(command['command'])
                record.update(status=command['status'], label=command['label'],
                              argv=command['command'], entry_macros=macros)
                planned = entry_macros([*receipt['translation_arguments']['common'], *module['flags']])
                if macros != planned:
                    issues.append({'source': str(path), 'error': 'Actual macro options differ from manifest/common flags'})
                if command['status'] != 'passed' or command.get('exit_code') != 0:
                    issues.append({'source': str(path), 'error': 'Actual emission did not pass',
                                   'status': command['status']})
                # Public guards undef MODULE/BUILTIN before Python.h. They cannot
                # undo an already supplied CORE. Internal opt-ins remain explicit.
                if api_role in ('public', 'limited') and 'Py_BUILD_CORE' in macros:
                    issues.append({'source': str(path), 'error': 'Inherited Py_BUILD_CORE reaches public/limited guard'})
                if api_role in ('internal', 'public-with-internal-opt-in') and not any(
                        flag in macros for flag in ('Py_BUILD_CORE', 'Py_BUILD_CORE_BUILTIN', 'Py_BUILD_CORE_MODULE')):
                    issues.append({'source': str(path), 'error': 'Internal helper has no core role at entry'})
                if any(flag in macros for flag in ('Py_LIMITED_API', 'PYTESTCAPI_NEED_INTERNAL_API', 'Py_GIL_DISABLED')):
                    issues.append({'source': str(path), 'error': 'Command overrides source-selected API/profile role'})
            rows.append(record)
    config = source/'Modules/config.c'
    match = re.search(r'\b_PyImport_Inittab\s*\[\s*\]\s*=\s*\{(.*?)\n\};', config.read_text(), re.S)
    if not match:
        raise ValueError('Actual static registration table missing')
    pairs = re.findall(r'\{\s*"([^"]+)"\s*,\s*(\w+)\s*\}', match.group(1))
    registrations = dict(pairs)
    required_names = set(json.loads((campaign/'config/stdlib.json').read_text())['builtins']) | set(expected)
    if len(pairs) != len(registrations) or set(registrations) != required_names:
        raise ValueError('Actual static registration closure differs')
    compiled = None
    if metadata_path is None and not source_only:
        issues.append({'error': 'Actual compiled profile PE/registration inspection unrun'})
    elif metadata_path is not None:
        metadata = json.loads(Path(metadata_path).read_text())
        assembly = Path(metadata['path'])
        if metadata['assembly'] != 'TranslatedCPython' or digest(assembly) != metadata['sha256']:
            raise ValueError('Actual compiled profile PE identity mismatch')
        projects = [Path(row['project']) for row in receipt.get('outputs', {}).values()]
        if not projects or not any(assembly.is_relative_to(project.parent) for project in projects):
            raise ValueError('Compiled PE is outside actual published profile outputs')
        methods = {row['name'] for row in metadata['methods'] if row['type'] == 'Managed.Interpreters.CPython'}
        if any(init != 'NULL' and init not in methods for init in registrations.values()):
            raise ValueError('Static helper initializer missing from actual compiled profile')
        compiled = {'metadata': str(Path(metadata_path).resolve()), 'metadata_sha256': digest(metadata_path),
                    'assembly': str(assembly), 'sha256': digest(assembly), 'mvid': metadata['mvid']}
    if receipt.get('status') != 'passed' or not receipt.get('outputs'):
        issues.append({'error': 'Actual full profile has not published successfully', 'status': receipt.get('status')})
    if receipt_path.read_bytes() != receipt_bytes:
        raise ValueError('Receipt changed during readiness audit')
    scope = ('source guards/recorded entry options/emission/source static registrations only; compiled initialization and assertion execution unreviewed'
             if source_only else 'source guards/recorded entry options/emission/static-registration readiness only; no upstream assertion execution')
    return {'scope': scope,
            'ready': not issues, 'receipt': str(receipt_path),
            'receipt_sha256': hashlib.sha256(receipt_bytes).hexdigest(), 'run_id': receipt['run_id'],
            'selected_units': len(receipt['units']), 'helper_modules': len(modules),
            'role_units': rows, 'guards': guards,
            'static_registrations': {'path': str(config), 'sha256': digest(config), 'entries': registrations},
            'compiled': compiled, 'issues': issues}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--receipt', required=True)
    parser.add_argument('--campaign', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--metadata')
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.receipt, args.campaign, args.metadata)
    args.report.write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'ready': result['ready'], 'role_units': len(result['role_units']),
                      'issues': len(result['issues']), 'report': str(args.report)}))
    raise SystemExit(0 if result['ready'] else 1)
