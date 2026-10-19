"""Serial unittest IN the selected interpreter, never subprocess regrtest.

Native controls are explicitly separate from translated qualification. Discovery
and case selection retain upstream methods, helpers, skips and assertions.
"""
import argparse
from collections import Counter
import contextlib
import importlib
import inspect
import json
import pathlib
import sys
import time
import traceback
import unittest


class DiscoveredCases(dict):
    def __init__(self):
        super().__init__()
        self.ordered = []


def flatten(suite):
    for test in suite:
        if isinstance(test, unittest.TestSuite):
            yield from flatten(test)
        else:
            yield test


def source_callable(function):
    """Find original source without changing the callable we execute.

    Upstream bigmem decorators intentionally lack __wrapped__; their f closure
    retains the original test method and its exact resource declaration.
    """
    seen = set()
    while inspect.isfunction(function) and function not in seen:
        seen.add(function)
        unwrapped = inspect.unwrap(function)
        if unwrapped is not function:
            function = unwrapped
            continue
        if '/test/support/' not in function.__code__.co_filename.replace('\\', '/'):
            break
        originals = [cell.cell_contents for name, cell in zip(function.__code__.co_freevars, function.__closure__ or ())
                     if name in ('f', 'func') and inspect.isfunction(cell.cell_contents)]
        if len(originals) != 1:
            break
        function = originals[0]
    return function


def case_detail(test):
    if type(test).__module__ == 'unittest.loader' and type(test).__name__ == 'ModuleSkipped':
        name = test._testMethodName
        spec = importlib.util.find_spec(name)
        path = pathlib.Path(spec.origin).resolve()
        parts = path.parts
        upstream = '/'.join(parts[parts.index('Lib'):]) if 'Lib' in parts else str(path)
        reason = getattr(type(test), '__unittest_skip_why__', '') or getattr(getattr(type(test), name), '__unittest_skip_why__', '')
        return {'case': test.id(), 'upstream_path': upstream, 'line': 1,
                'declaring_class': 'unittest.loader.ModuleSkipped', 'method': name,
                'skip_reason': reason, 'expected_failure': False, 'source': reason,
                'module_skip': True}
    doc = getattr(test, '_dt_test', None)
    if doc is not None:
        path = pathlib.Path(doc.filename).resolve()
        parts = path.parts
        upstream = '/'.join(parts[parts.index('Lib'):]) if 'Lib' in parts else str(path)
        return {"case": test.id(), "upstream_path": upstream, "line": doc.lineno + 1 if doc.lineno is not None else 1,
                "line_basis": "doctest location" if doc.lineno is not None else "upstream dynamic __test__ docstring; line unavailable",
                "declaring_class": 'doctest.DocTestCase', "method": doc.name,
                "skip_reason": '', "expected_failure": False, "source": doc.docstring,
                "doctest_examples": len(doc.examples)}
    method = getattr(type(test), test._testMethodName)
    original = source_callable(method)
    path = pathlib.Path(inspect.getsourcefile(original)).resolve()
    parts = path.parts
    upstream = '/'.join(parts[parts.index('Lib'):]) if 'Lib' in parts else str(path)
    source, line = inspect.getsourcelines(original)
    return {"case": test.id(), "upstream_path": upstream, "line": line,
            "declaring_class": type(test).__module__ + '.' + type(test).__qualname__,
            "method": test._testMethodName,
            "skip_reason": (getattr(type(test), '__unittest_skip_why__', '') if getattr(type(test), '__unittest_skip__', False)
                            else getattr(method, '__unittest_skip_why__', '') if getattr(method, '__unittest_skip__', False) else ''),
            "expected_failure": bool(getattr(method, '__unittest_expecting_failure__', False)),
            "source": ''.join(source)}


def discover(modules, case_objects=None, platform_only_module_skips=None):
    output = []
    for module_name in modules:
        loader = unittest.TestLoader()
        try:
            module = importlib.import_module(module_name)
            suite = loader.loadTestsFromModule(module)
            if loader.errors:
                raise RuntimeError("\n".join(loader.errors))
            cases = list(flatten(suite))
            unique, details, duplicates = {}, {}, {}
            for test in cases:
                detail = case_detail(test)
                allowed_skip = (platform_only_module_skips or {}).get(detail['method'])
                platform_skip = allowed_skip is not None and detail['skip_reason'] == 'No module named ' + repr(allowed_skip['missing_module'])
                if detail.get('module_skip') and not platform_skip and ('No module named' in detail['skip_reason'] or 'requires _test' in detail['skip_reason']):
                    raise RuntimeError('Required import helper hidden by module SkipTest: ' + detail['method'] + ': ' + detail['skip_reason'])
                key = test.id()
                if key in unique:
                    if detail != details[key] :
                        raise RuntimeError('Conflicting upstream cases share ID: ' + key)
                    duplicates[key] = duplicates.get(key, 1) + 1
                else:
                    unique[key], details[key] = test, detail
                if case_objects is not None:
                    case_objects.setdefault(key, []).append(test)
                    if isinstance(case_objects, DiscoveredCases):
                        case_objects.ordered.append(test)
            if not unique:
                raise RuntimeError('Discovery returned zero test IDs')
            output.append({"module": module_name, "status": "discovered", "cases": list(unique),
                           "case_details": list(details.values()),
                           "loader_occurrences": len(cases), "duplicate_loader_occurrences": duplicates})
        except BaseException:
            output.append({"module": module_name, "status": "blocked", "traceback": traceback.format_exc(), "cases": []})
    return output


class CaseResult(unittest.TextTestResult):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.cases = []
        self.journal = None
        self.started = {}
        self.current = None
        self.interruption = None

    def startTest(self, test):
        super().startTest(test)
        self.current = test
        self.started[test.id()] = time.monotonic()
        if self.journal:
            self.journal.write(json.dumps({"case": test.id(), "status": "started"}) + '\n')
            self.journal.flush()

    def record(self, test, status, **extra):
        row = {"case": test.id(), "status": status, **extra}
        if test.id() in self.started:
            row['seconds'] = time.monotonic() - self.started[test.id()]
        self.cases.append(row)
        if self.journal:
            self.journal.write(json.dumps(row, ensure_ascii=True) + '\n')
            self.journal.flush()

    def addSuccess(self, test):
        super().addSuccess(test)
        self.record(test, "passed")

    def addError(self, test, error):
        super().addError(test, error)
        self.record(test, "error", traceback=self._exc_info_to_string(error, test))

    def addFailure(self, test, error):
        super().addFailure(test, error)
        self.record(test, "failed", traceback=self._exc_info_to_string(error, test))

    def addSkip(self, test, reason):
        super().addSkip(test, reason)
        self.record(test, "required-skip", reason=reason)

    def addExpectedFailure(self, test, error):
        super().addExpectedFailure(test, error)
        self.record(test, "required-expected-failure", traceback=self._exc_info_to_string(error, test))

    def addUnexpectedSuccess(self, test):
        super().addUnexpectedSuccess(test)
        self.record(test, "unexpected-success")

    def addSubTest(self, test, subtest, error):
        super().addSubTest(test, subtest, error)
        if error is not None:
            self.record(test, "failed-subtest", subtest=str(subtest), traceback=self._exc_info_to_string(error, test))


def validate_inventory(inventory, discovery):
    required = inventory['required_cases']
    discovered_list = [case for module in discovery for case in module['cases']]
    discovered = set(discovered_list)
    if len(discovered_list) != len(discovered):
        raise ValueError('Duplicate case IDs across discovery modules')
    excluded = set()
    for item in inventory['exclusions']:
        for key in ('case', 'upstream_path', 'reason', 'affected_contract'):
            if not item.get(key):
                raise ValueError(f'Exclusion is missing {key}: {item}')
        if item['case'] in excluded:
            raise ValueError('Duplicate exclusion: ' + item['case'])
        excluded.add(item['case'])
    if not required or len(set(required)) != len(required):
        raise ValueError('Required inventory is empty or contains duplicate cases')
    if set(required) & excluded:
        raise ValueError('Required/excluded cases overlap')
    unknown = (set(required) | excluded) - discovered
    if unknown:
        raise ValueError('Unknown/stale case IDs: ' + repr(sorted(unknown)))
    unclassified = discovered - set(required) - excluded
    if unclassified:
        raise ValueError(f'{len(unclassified)} discovered cases lack selection/exclusion decisions')
    if 'discovered_cases' in inventory and set(inventory['discovered_cases']) != discovered:
        raise ValueError('Discovery changed since inventory generation')


def run_selected(inventory, discovery, *, journal=None, selected=None, case_objects=None, stream=None):
    validate_inventory(inventory, discovery)
    required = inventory['required_cases'] if selected is None else selected
    if not required or not set(required) <= set(inventory['required_cases']):
        raise ValueError('Control selection must be a nonempty subset of required cases')
    if case_objects is None:
        loader = unittest.TestLoader()
        suite = loader.loadTestsFromNames(required)
        if loader.errors:
            raise RuntimeError('\n'.join(loader.errors))
    elif isinstance(case_objects, DiscoveredCases):
        # Keep original upstream suite order, including duplicate load_tests
        # occurrences and recreated class fixtures. No assertion is discarded.
        selected_set = set(required)
        suite = unittest.TestSuite(test for test in case_objects.ordered if test.id() in selected_set)
    else:
        # Retain actual load_tests/doctest instances. Many exact upstream IDs
        # cannot be reconstructed with unittest.loadTestsFromNames.
        suite = unittest.TestSuite(test for case in required
                                   for test in (case_objects[case] if isinstance(case_objects[case], list) else [case_objects[case]]))
    loaded = [test.id() for test in flatten(suite)]
    expected = [case for case in required for _ in range(len(case_objects[case]) if case_objects is not None and isinstance(case_objects[case], list) else 1)]
    if isinstance(case_objects, DiscoveredCases):
        if Counter(loaded) != Counter(expected):
            raise ValueError('Selected occurrence counts differ from discovery')
        expected = loaded
    elif loaded != expected:
        raise ValueError('Selected IDs did not load exactly and in manifest order')
    live = []
    def result_factory(*args, **kwargs):
        result = CaseResult(*args, **kwargs)
        result.journal = journal
        live.append(result)
        return result
    try:
        result = unittest.TextTestRunner(verbosity=2, resultclass=result_factory, stream=stream).run(suite)
    except BaseException:
        if not live:
            raise
        result = live[0]
        result.interruption = traceback.format_exc()
        if result.current is not None:
            result.record(result.current, 'interrupted', traceback=result.interruption)
    statuses = {row['status'] for row in result.cases}
    success = (not result.interruption and result.wasSuccessful() and statuses <= {'passed'} and result.testsRun == len(expected)
               and Counter(row['case'] for row in result.cases) == Counter(expected))
    return result, success


def unload_test_modules(baseline):
    # Same best-effort test-module lifetime policy as the pin's serial regrtest
    # loop (Lib/test/libregrtest/main.py). Interpreter/core modules stay real.
    for name in list(sys.modules):
        if name not in baseline and name.startswith(('test.', 'test_')):
            sys.modules.pop(name, None)
            parent, _, child = name.rpartition('.')
            owner = sys.modules.get(parent)
            if owner is not None:
                try: delattr(owner, child)
                except AttributeError: pass


def execute_groups(inventory, discovery, report, journal, selected, stream=None, checkpoint=None):
    validate_inventory(inventory, discovery)
    wanted = inventory['required_cases'] if not selected else selected
    if not set(wanted) <= set(inventory['required_cases']):
        raise ValueError('Native subset includes an unselected case')
    # Initial discovery is metadata, not an execution instance cache. Load each
    # upstream seed immediately before its serial run, like upstream regrtest.
    baseline = set(sys.modules) - {name for name in sys.modules if name.startswith(('test.', 'test_'))}
    unload_test_modules(baseline)
    success = True
    started = set()
    report['groups'] = []
    report['selected_occurrences'] = 0
    for module in discovery:
        selected_ids = [case for case in wanted if case in set(module['cases'])]
        if not selected_ids:
            continue
        live = DiscoveredCases()
        fresh = discover([module['module']], live, inventory.get('platform_only_module_skips'))[0]
        if (fresh['status'] != 'discovered' or fresh['cases'] != module['cases']
                or fresh['duplicate_loader_occurrences'] != module['duplicate_loader_occurrences']):
            raise RuntimeError('Fresh per-module discovery differs: ' + repr(fresh))
        result, passed = run_selected(inventory, discovery, journal=journal, selected=selected_ids, case_objects=live, stream=stream)
        report['cases'].extend(result.cases)
        report['executed'] += result.testsRun
        report['selected_occurrences'] += sum(len(live[key]) for key in selected_ids)
        started.update(result.started)
        report['groups'].append({'module': module['module'], 'selected': len(selected_ids),
                                 'executed': result.testsRun, 'status': 'passed' if passed else 'blocked-or-failed'})
        report['started_cases'] = sorted(started)
        report['required_unexecuted'] = sorted(set(wanted) - started)
        if checkpoint:
            checkpoint()
        success = success and passed
        unload_test_modules(baseline)
        if result.interruption:
            report['failure'] = result.interruption
            break
    report['started_cases'] = sorted(started)
    report['required_unexecuted'] = sorted(set(wanted) - started)
    return success and not report['required_unexecuted']


def report_counts(report, inventory, selected):
    discovery = report['discovery']
    return {'discovered': sum(len(module['cases']) for module in discovery),
            'discovered_occurrences': sum(module.get('loader_occurrences', len(module['cases'])) for module in discovery),
            'blocked_modules': sum(module['status'] == 'blocked' for module in discovery),
            'selected': len(selected or inventory['required_cases']),
            'configured_required': len(inventory['required_cases']),
            'unselected_required': len(inventory['required_cases']) - len(selected or inventory['required_cases']),
            'executed_unique': len(report.get('started_cases', [])), 'excluded': len(inventory['exclusions']),
            'executed': report['executed'],
            **{status: sum(case['status'] == status for case in report['cases'])
               for status in ('passed', 'failed', 'error', 'required-skip', 'required-expected-failure',
                              'unexpected-success', 'failed-subtest', 'interrupted')}}


def save_report(report_path, report, inventory, selected):
    """Keep completed groups if a later translated operation terminates the host.

    The per-case journal additionally retains the current started case. A
    checkpoint always remains running/unqualified until the full runner returns.
    """
    report['counts'] = report_counts(report, inventory, selected)
    pending = report_path.with_suffix(report_path.suffix+'.pending')
    pending.write_text(json.dumps(report, ensure_ascii=True, indent=2)+'\n', encoding='utf-8')
    pending.replace(report_path)


def run_embedded(inventory, report, action='qualify'):
    """Called through the owning .NET API in the actual translated test profile."""
    if action not in ('qualify', 'discover'):
        raise ValueError('Embedded entrypoint accepts qualify/discover only')
    args = ['--inventory', inventory, '--report', report, '--implementation', 'translated']
    if action == 'discover':
        args.append('--discover')
    return main(args)


def main(argv=None):
    parser = argparse.ArgumentParser()
    parser.add_argument('--inventory', required=True)
    parser.add_argument('--report', required=True)
    parser.add_argument('--discover', action='store_true')
    parser.add_argument('--native-control', action='store_true', help='Allow classified partial inventory for native controls; never qualification')
    parser.add_argument('--case', action='append', default=[], help='Native control required-case subset; repeatable')
    parser.add_argument('--implementation', choices=('native-oracle', 'translated'), required=True)
    args = parser.parse_args(argv)
    if not __debug__:
        raise RuntimeError('Upstream assertions require Python optimization disabled')
    if (args.native_control or args.case) and args.implementation != 'native-oracle':
        raise ValueError('Partial/subset controls cannot qualify translated execution')
    if args.case and not args.native_control:
        raise ValueError('Subset requires explicit --native-control')
    inventory_path = pathlib.Path(args.inventory).resolve()
    report_path = pathlib.Path(args.report).resolve()
    inventory = json.loads(inventory_path.read_text(encoding='utf-8'))
    expected = tuple(int(part) for part in inventory['python_version'].split('.'))
    if sys.version_info[:3] != expected:
        raise RuntimeError(f"Exact CPython {inventory['python_version']} required, observed {sys.version}")
    report_path.parent.mkdir(parents=True, exist_ok=True)
    work = report_path.parent / (report_path.stem + '.work')
    work.mkdir(exist_ok=True)
    with contextlib.chdir(work):
        return run_manifest(args, inventory, inventory_path, report_path)


def run_manifest(args, inventory, inventory_path, report_path):
    discovery = discover(inventory['discovery_modules'], platform_only_module_skips=inventory.get('platform_only_module_skips'))
    scope = 'discovery-only' if args.discover else 'native-control' if args.native_control else 'selected-upstream-cases'
    report = {'scope': scope, 'implementation': args.implementation, 'python': sys.version,
              'interpreter_configuration': {'utf8_mode': sys.flags.utf8_mode,
                  'isolated': sys.flags.isolated, 'ignore_environment': sys.flags.ignore_environment,
                  'optimize': sys.flags.optimize, 'filesystem_encoding': sys.getfilesystemencoding(),
                  'filesystem_errors': sys.getfilesystemencodeerrors(),
                  'builtin_modules': sorted(sys.builtin_module_names)},
              'discovery': discovery, 'exclusions': inventory['exclusions'], 'cases': [],
              'inventory': str(inventory_path), 'inventory_status': inventory['status'],
              'executed': 0, 'qualifies_translated_interpreter': False,
              'selected_case_ids': args.case or inventory['required_cases'],
              'work_directory': str(pathlib.Path.cwd()), 'status': 'running',
              'required_unexecuted': sorted(args.case or inventory['required_cases'])}
    def checkpoint():
        save_report(report_path, report, inventory, args.case)
    checkpoint()
    success = False
    try:
        report['helper_origins'] = {}
        for name in inventory.get('required_helper_modules', []):
            helper = importlib.import_module(name)
            origin = helper.__spec__.origin
            report['helper_origins'][name] = origin
            if origin != 'built-in':
                raise RuntimeError('Required static test helper is not built-in: ' + name + ': ' + repr(origin))
        if any(module['status'] == 'blocked' for module in discovery):
            raise RuntimeError('Required upstream module discovery blocked; see per-module diagnostics')
        if args.discover:
            success = True
        else:
            if inventory['status'] != 'frozen' and not args.native_control:
                raise RuntimeError('Qualification inventory has not been frozen/reviewed')
            with report_path.with_suffix('.journal.jsonl').open('w', encoding='utf-8') as journal, report_path.with_suffix('.unittest.log').open('w', encoding='utf-8') as stream:
                report['unittest_log'] = str(report_path.with_suffix('.unittest.log'))
                success = execute_groups(inventory, discovery, report, journal, args.case,
                                         stream=stream, checkpoint=checkpoint)
            report['qualifies_translated_interpreter'] = success and scope == 'selected-upstream-cases' and args.implementation == 'translated'
    except BaseException:
        report['failure'] = traceback.format_exc()
    report['status'] = 'passed' if success else 'blocked-or-failed'
    checkpoint()
    print(json.dumps({'scope': report['scope'], 'status': report['status'], 'counts': report['counts']}))
    return 0 if success else 1


if __name__ == '__main__':
    raise SystemExit(main())
