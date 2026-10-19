"""Source-backed selection: require every discovered case unless a concrete
unsupported execution facility, platform or bounded-resource requirement occurs.

Runs on the private native discovery interpreter. This does not run assertions,
change upstream code, bless native failures, or translate the selected profile.
The generated candidate stays unfrozen until its evidence has been reviewed.
"""
import argparse
import ast
import inspect
import json
from pathlib import Path
import sys
import textwrap
import types

sys.path.insert(0, str(Path(__file__).resolve().parent))
from run_inventory import discover, source_callable

# Terminal operations are product exclusions in PLAN, not missing test helpers.
OPERATIONS = {
    'assert_python_ok': 'separate Python process / command-line interpreter',
    'assert_python_failure': 'separate Python process / command-line interpreter',
    'spawn_python': 'separate Python process / command-line interpreter',
    'kill_python': 'separate Python process / command-line interpreter',
    'run_python_until_end': 'separate Python process / command-line interpreter',
    'run_test_script': 'separate Python process / command-line interpreter',
    'assert_python_isolated': 'separate Python process / command-line interpreter',
    'run_in_subinterp': 'multiple interpreters / subinterpreter execution',
    'run_in_subinterp_with_config': 'multiple interpreters / subinterpreter execution',
    'requires_subprocess': 'separate Python process / command-line interpreter',
    'requires_fork': 'fork execution',
    'requires_subinterpreters': 'multiple interpreters / subinterpreter execution',
    'bigmemtest': 'optional large-memory upstream resource',
    'bigaddrspacetest': 'optional large-address-space upstream resource',
    'requires_resource': 'explicit optional upstream test resource',
}
QUALIFIED = {
    'subprocess.': 'fork/exec/subprocess execution',
    'multiprocessing.': 'multiprocessing execution',
    '_interpreters.': 'multiple interpreters / subinterpreter execution',
    'interpreters.': 'multiple interpreters / subinterpreter execution',
    'ctypes.': 'native FFI / dynamic native library access',
}
EXACT = {
    'os.fork': 'fork execution', 'os.forkpty': 'fork execution',
    'os.execv': 'exec execution', 'os.execve': 'exec execution',
    'os.posix_spawn': 'subprocess execution', 'os.posix_spawnp': 'subprocess execution',
    'signal.signal': 'OS signal-handler installation', 'signal.alarm': 'OS signal-handler installation',
    'signal.setitimer': 'OS signal-handler installation', 'signal.raise_signal': 'OS signals',
    'os.kill': 'OS signals', 'os.killpg': 'OS signals',
    'socket.socket': 'network socket execution', 'socket.create_connection': 'network socket execution',
    'ExtensionFileLoader': 'dynamic native extension loading',
    'importlib.machinery.ExtensionFileLoader': 'dynamic native extension loading',
}
PLATFORM_REASONS = ('specific to Windows', 'specific to windows', '32-bit', 'code pages are specific',
                    'only applies to 32', '64-bit platform', 'requires debug build', 'built with -NDEBUG')


def dotted(node):
    if isinstance(node, ast.Name): return node.id
    if isinstance(node, ast.Attribute):
        before = dotted(node.value)
        return before + '.' + node.attr if before else node.attr
    return ''


def inspect_operation(function, cls, seen, depth=0):
    if depth > 12 or not inspect.isfunction(function): return []
    function = source_callable(function)
    if function in seen: return []
    seen.add(function)
    try:
        source, line = inspect.getsourcelines(function)
        path = Path(inspect.getsourcefile(function)).resolve()
        if 'Lib' not in path.parts: return []
        relative = '/'.join(path.parts[path.parts.index('Lib'):])
        tree = ast.parse(textwrap.dedent(''.join(source)))
    except (OSError, TypeError, SyntaxError):
        return []
    evidence = []
    for function_node in tree.body:
        if isinstance(function_node, (ast.FunctionDef, ast.AsyncFunctionDef)):
            for decorator in function_node.decorator_list:
                name = dotted(decorator)
                short = name.rsplit('.', 1)[-1]
                if short in OPERATIONS:
                    evidence.append({'upstream_path': relative, 'line': line + decorator.lineno - 1,
                                     'operation': name, 'source': ast.get_source_segment(textwrap.dedent(''.join(source)), decorator),
                                     'reason': OPERATIONS[short]})
    for node in ast.walk(tree):
        if isinstance(node, ast.Attribute) and node.attr in ('ExtensionFileLoader', 'EXTENSION_SUFFIXES'):
            evidence.append({'upstream_path': relative, 'line': line + node.lineno - 1, 'operation': dotted(node), 'source': ast.get_source_segment(textwrap.dedent(''.join(source)), node), 'reason': 'dynamic native extension loading'})
        if not isinstance(node, ast.Call): continue
        name = dotted(node.func)
        short = name.rsplit('.', 1)[-1]
        reason = OPERATIONS.get(short) or EXACT.get(name)
        if short == 'bigmemtest' and not any(k.arg == 'dry_run' and isinstance(k.value, ast.Constant) and k.value.value is False for k in node.keywords):
            reason = None  # Upstream's normal small dry-run executes real algorithms.
        if reason is None:
            reason = next((value for prefix, value in QUALIFIED.items() if name.startswith(prefix)), None)
        if reason:
            evidence.append({'upstream_path': relative, 'line': line + node.lineno - 1,
                             'operation': name, 'source': ast.get_source_segment(textwrap.dedent(''.join(source)), node),
                             'reason': reason})
            continue
        target = None
        if name.startswith('self.') and cls is not None and name.count('.') == 1:
            target = getattr(cls, short, None)
        elif '.' not in name:
            target = function.__globals__.get(name)
        if inspect.isfunction(target):
            nested = inspect_operation(target, cls, seen, depth + 1)
            for item in nested:
                item = dict(item)
                item['via'] = name
                evidence.append(item)
    return evidence


def decide(case, detail, platform_skips, decisions):
    decision = decisions.get(detail['case'])
    if decision is not None:
        if detail['upstream_path'] != decision['upstream_path'] or decision['source_guard'] not in detail['source']:
            raise RuntimeError('Resource decision source guard changed: ' + detail['case'])
        return [{'upstream_path': detail['upstream_path'], 'line': detail['line'], 'operation': 'reviewed individual resource requirement', 'source': decision['source_guard'], 'reason': decision['reason']}]
    reason = detail['skip_reason']
    platform = platform_skips.get(detail['method']) if detail.get('module_skip') else None
    if platform is not None:
        return [{'upstream_path': platform['upstream_path'], 'line': 1, 'operation': 'platform-only module import', 'source': reason, 'reason': platform['reason']}]
    if reason and any(text in reason for text in PLATFORM_REASONS):
        return [{'upstream_path': detail['upstream_path'], 'line': detail['line'],
                 'operation': 'upstream platform/build decorator', 'source': reason,
                 'reason': 'unavailable platform/build resource in Linux x64 release profile: ' + reason}]
    if reason and ('zlib' in reason):
        return [{'upstream_path': detail['upstream_path'], 'line': detail['line'],
                 'operation': 'upstream compression requirement', 'source': reason,
                 'reason': 'external zlib compression outside selected product stdlib closure'}]
    scoped_skips = {
        'dynamic loading not supported or test module not available': 'dynamic native extension-loading fixture; actual static import helpers are present',
        'requires a case-insensitive filesystem': 'case-insensitive filesystem resource unavailable on selected Linux filesystem',
        'test meaningful only where os.altsep is defined': 'alternate path separator platform resource absent on Linux',
        '-E was given': 'PYTHONBREAKPOINT environment interpretation outside isolated embedding configuration',
        'BytesWarning is needed for this test: use -bb option': 'optional -bb command-line diagnostic profile outside embedded release configuration',
    }
    if reason in scoped_skips:
        return [{'upstream_path': detail['upstream_path'], 'line': detail['line'],
                 'operation': 'explicit upstream feature/resource decorator', 'source': reason,
                 'reason': scoped_skips[reason]}]
    cls = type(case)
    if getattr(case, '_dt_test', None) is not None:
        # Doctest source is kept in discovery; ordinary language examples remain
        # selected. Their failures cannot be recategorized as exclusions.
        return []
    evidence = []
    for name in (case._testMethodName, 'setUp', 'tearDown', 'setUpClass', 'tearDownClass'):
        function = getattr(cls, name, None)
        if inspect.ismethod(function): function = function.__func__
        evidence += inspect_operation(function, cls, set())
    return evidence


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--seed', required=True)
    parser.add_argument('--output', required=True)
    parser.add_argument('--discovery', required=True)
    args = parser.parse_args()
    seed = json.loads(Path(args.seed).read_text())
    decisions = json.loads(Path(__file__).with_name('resource-decisions.json').read_text())['cases']
    cases = {}
    discovery = discover(seed['discovery_modules'], cases, seed.get('platform_only_module_skips'))
    if any(row['status'] != 'discovered' for row in discovery):
        raise RuntimeError('Full seed discovery is a prerequisite to classification')
    details = {t['case']: t for row in discovery for t in row['case_details']}
    required, exclusions, selection = [], [], []
    for row in discovery:
        for case_id in row['cases']:
            detail = details[case_id]
            evidence = decide(cases[case_id][0], detail, seed.get('platform_only_module_skips', {}), decisions)
            if evidence:
                exclusions.append({'case': case_id, 'upstream_path': detail['upstream_path'], 'line': detail['line'],
                    'reason': '; '.join(dict.fromkeys(e['reason'] for e in evidence)),
                    'affected_contract': row['module'] + ': this method requires the evidenced facility/resource; other methods remain required',
                    'evidence': evidence})
            else:
                required.append(case_id)
                selection.append({'case': case_id, 'upstream_path': detail['upstream_path'], 'line': detail['line'],
                                  'contract': row['module'], 'native_skip_at_discovery': detail['skip_reason'],
                                  'native_expected_failure': detail['expected_failure']})
    missing = set(seed['required_cases']) - set(required)
    if missing:
        raise RuntimeError('Mandatory case floor excluded: ' + repr(missing))
    root = Path(__file__).resolve().parents[2]
    pin = json.loads((root/'config/source.json').read_text())
    inventory = {'status': 'classified-candidate; source decisions need final review; not qualification',
                 'python_version': pin['version'], 'source_revision': pin['revision'], 'platform_only_module_skips': seed.get('platform_only_module_skips', {}), 'discovery_modules': seed['discovery_modules'],
                 'discovered_cases': list(details), 'required_cases': required,
                 'exclusions': exclusions, 'required_case_details': selection,
                 'mandatory_floor': seed['required_cases'],
                 'policy': 'Required skips/expected failures remain failures. Native failures are blockers, never expected normalized output. No missing-helper exclusions.',
                 'duplicate_loader_occurrences': {row['module']: row['duplicate_loader_occurrences'] for row in discovery if row['duplicate_loader_occurrences']},
                 'counts': {'discovered': len(details), 'required': len(required), 'excluded': len(exclusions)}}
    Path(args.output).write_text(json.dumps(inventory, indent=2)+'\n')
    Path(args.discovery).write_text(json.dumps(discovery, indent=2)+'\n')
    print(json.dumps(inventory['counts']))


if __name__ == '__main__': main()
