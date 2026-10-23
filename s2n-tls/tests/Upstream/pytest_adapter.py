"""Pytest transport adapter; original test bodies and parameterization are untouched."""
import json
import hashlib
import inspect
import errno
from enum import Enum
import os
import shutil
from pathlib import Path
import pytest

_report = {'collected': [], 'deselected': [], 'outcomes': [], 'collection_errors': [], 'commands': []}
_worker_reports = []


def record_no_hrr_control(observation):
    _report.setdefault('hrr_no_retry_controls', []).append(observation)


def argument_files(arguments):
    """CLI values such as long PSK identities need not be filesystem paths."""
    paths = set()
    for argument in arguments:
        path = Path(str(argument))
        try:
            if path.is_file(): paths.add(path.resolve())
        except OSError as error:
            if error.errno != errno.ENAMETOOLONG: raise
    return paths


def observe_ports(iterator_type, report):
    original_next = iterator_type.__next__
    report['port_allocations'] = []

    def observed_next(iterator):
        port = original_next(iterator)
        report['port_allocations'].append({'port': port, 'worker': os.environ.get('PYTEST_XDIST_WORKER'),
                                          'nodeid': os.environ.get('PYTEST_CURRENT_TEST')})
        return port

    iterator_type.__next__ = observed_next


def pytest_configure(config):
    from readiness import install
    install(_report)
    from common import AvailablePorts
    import configuration
    explicit = os.environ.get('DOTCC_S2N_PORT_LOW') is not None
    low = int(os.environ.get('DOTCC_S2N_PORT_LOW', '8000'))
    high = int(os.environ.get('DOTCC_S2N_PORT_HIGH', '30000'))
    if not 1 <= low < high <= 65536: raise RuntimeError('Invalid explicit original AvailablePorts constructor bounds')
    if explicit: configuration.available_ports = AvailablePorts(low=low, high=high)
    _report['port_allocator'] = {'constructor': AvailablePorts.__module__ + '.' + AvailablePorts.__qualname__,
        'constructor_sha256': hashlib.sha256(inspect.getsource(AvailablePorts).encode()).hexdigest(),
        'low': low, 'high_exclusive': high, 'explicit_constructor_arguments': explicit,
        'worker': os.environ.get('PYTEST_XDIST_WORKER'), 'workers': os.environ.get('PYTEST_XDIST_WORKER_COUNT')}
    if os.environ.get('DOTCC_S2N_AVAILABLE_PORT_PROBE') == '1':
        from port_probe import install as install_port_probe
        install_port_probe(AvailablePorts, _report)
    observe_ports(AvailablePorts, _report)
    from providers import Provider
    mapping = json.loads(Path(os.environ['DOTCC_S2N_PYTHON_COMMANDS']).read_text())
    commands = mapping['commands']
    _report['measured_psk_peer'] = mapping.get('measured_psk_peer')
    _report['hrr_applicability'] = mapping.get('hrr_applicability')
    original = Provider.get_cmd_line

    def selected_command(self):
        argv = list(original(self))
        name = str(argv[0]); base = Path(name).name; provider = type(self).__name__
        keys = (provider + ':' + name, provider + ':' + base, name, base)
        selected = next((commands[k] for k in keys if k in commands), None)
        if base in ('s2nc', 's2nd', 's2nc_head', 's2nd_head') and selected is None:
            raise RuntimeError('Required exact translated/reference endpoint mapping missing: ' + base)
        command = [*selected, *argv[1:]] if selected is not None else argv
        binary = shutil.which(str(command[0]))
        if binary is None: raise RuntimeError('Actual endpoint executable is missing: ' + str(command[0]))
        paths = {Path(binary).resolve(), *argument_files(command[1:])}
        _report['commands'].append({'provider': provider, 'pytest_current_test': os.environ.get('PYTEST_CURRENT_TEST'),
            'requested': argv, 'executed': command,
            'execution_sha256': {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(paths)}})
        return command
    Provider.get_cmd_line = selected_command


def stable_value(value):
    """Record parameter values independently of Python object addresses."""
    if value is None or isinstance(value, (str, int, float, bool)): return value
    if isinstance(value, bytes): return {'bytes_hex': value.hex()}
    if isinstance(value, type): return {'type': value.__module__ + '.' + value.__qualname__}
    if isinstance(value, Enum): return {'enum': type(value).__module__ + '.' + type(value).__qualname__, 'name': value.name}
    if isinstance(value, (list, tuple)): return [stable_value(v) for v in value]
    if isinstance(value, dict): return {str(k): stable_value(v) for k, v in sorted(value.items(), key=lambda x: str(x[0]))}
    if isinstance(value, (set, frozenset)):
        return sorted((stable_value(v) for v in value), key=lambda v: json.dumps(v, sort_keys=True))
    if isinstance(value, Path): return str(value)
    if hasattr(value, '__dict__'):
        return {'type': type(value).__module__ + '.' + type(value).__qualname__, 'fields': stable_value(vars(value))}
    raise TypeError('Parameter requires a deterministic identity adapter: ' + type(value).__qualname__)


def parameters(item):
    if not hasattr(item, 'callspec'): return {}
    return {k: stable_value(v) for k, v in item.callspec.params.items()}


@pytest.hookimpl(hookwrapper=True, tryfirst=True)
def pytest_collection_modifyitems(session, config, items):
    if _report.get('measured_psk_peer'):
        from measured_psk import install
        install(items, _report, _report['measured_psk_peer'])
    original = list(items)
    yield
    from hrr_applicability import classify
    inapplicable = classify(items, _report, _report.get('hrr_applicability'), parameters)
    retained = set(items)
    for item in original:
        if item in inapplicable:
            continue
        row = {'nodeid': item.nodeid, 'parameters': parameters(item)}
        if item in retained:
            _report['collected'].append(row)
        else:
            marker = item.get_closest_marker('uncollect_if')
            predicate = marker.kwargs.get('func') if marker else None
            row['reason'] = 'Original upstream uncollect_if parameter predicate' if marker else 'Unexplained deselection'
            row['predicate'] = (predicate.__module__ + '.' + predicate.__qualname__) if predicate else None
            from providers import S2N
            params = item.callspec.params if hasattr(item, 'callspec') else {}
            vetoes = []
            for parameter in ('protocol', 'cipher', 'signature'):
                value = params.get(parameter)
                if value is None: continue
                checker = getattr(S2N, 'supports_' + parameter)
                supported = checker(value, with_curve=params.get('curve')) if parameter == 'cipher' else checker(value)
                if not supported: vetoes.append(parameter)
            for parameter in ('certificate', 'client_certificate'):
                if params.get(parameter) is not None and not S2N.supports_certificate(params[parameter]): vetoes.append(parameter)
            row['product_capability_vetoes'] = vetoes
            _report['deselected'].append(row)


def pytest_collectreport(report):
    if report.failed:
        _report['collection_errors'].append({'nodeid': report.nodeid, 'error': str(report.longrepr)})


def pytest_runtest_logreport(report):
    if report.when == 'call' or report.failed or report.skipped:
        _report['outcomes'].append({'nodeid': report.nodeid, 'phase': report.when, 'outcome': report.outcome,
                                    'detail': str(report.longrepr) if report.outcome != 'passed' else None,
                                    'captured_sections': report.sections if report.outcome != 'passed' else []})
        if report.failed or report.skipped or len(_report['outcomes']) % 25 == 0:
            destination = Path(os.environ['DOTCC_S2N_PYTHON_REPORT'])
            worker = os.environ.get('PYTEST_XDIST_WORKER', 'controller')
            destination = destination.with_name(destination.stem + '-' + worker + '-live.json')
            progress = {'outcomes': len(_report['outcomes']),
                        'passed': sum(r['phase'] == 'call' and r['outcome'] == 'passed' for r in _report['outcomes']),
                        'failed_or_skipped': sum(r['outcome'] != 'passed' for r in _report['outcomes']),
                        'failures': [r for r in _report['outcomes'] if r['outcome'] != 'passed'],
                        'last': _report['outcomes'][-1]}
            temporary = destination.with_suffix('.tmp')
            temporary.write_text(json.dumps(progress, indent=2) + '\n'); temporary.replace(destination)


@pytest.hookimpl(optionalhook=True)
def pytest_testnodedown(node, error):
    path = node.workeroutput.get('dotcc_s2n_parameter_receipt')
    if path:
        _worker_reports.append(json.loads(Path(path).read_text()))
    if error:
        _report['collection_errors'].append({'nodeid': node.gateway.id, 'error': str(error)})


def pytest_sessionfinish(session, exitstatus):
    destination = Path(os.environ['DOTCC_S2N_PYTHON_REPORT'])
    if hasattr(session.config, 'workerinput'):
        # A worker owns a partition of execution, while collecting the complete
        # unchanged parameter inventory. Only the controller can close a family.
        worker = session.config.workerinput['workerid']
        destination = destination.with_name(destination.stem + '-' + worker + destination.suffix)
        destination.write_text(json.dumps(_report, indent=2) + '\n')
        session.config.workeroutput['dotcc_s2n_parameter_receipt'] = str(destination)
        return
    if _worker_reports:
        first = _worker_reports[0]
        for receipt in _worker_reports:
            if receipt['collected'] != first['collected'] or receipt['deselected'] != first['deselected']:
                _report['collection_errors'].append({'nodeid': 'workers', 'error': 'Original worker parameter inventories differ'})
            _report['commands'].extend(receipt['commands'])
            _report.setdefault('worker_port_allocators', []).append(receipt['port_allocator'])
            _report['readiness'].extend(receipt.get('readiness', []))
            _report.setdefault('control_markers', []).extend(receipt.get('control_markers', []))
            _report['collection_errors'].extend(receipt['collection_errors'])
            _report['port_allocations'].extend(receipt.get('port_allocations', []))
            if receipt.get('measured_psk_adaptation'):
                _report.setdefault('worker_measured_psk_adaptations', []).append(receipt['measured_psk_adaptation'])
            _report.setdefault('hrr_no_retry_controls', []).extend(receipt.get('hrr_no_retry_controls', []))
            if receipt.get('available_port_probe'):
                _report.setdefault('worker_available_port_probes', []).append(receipt['available_port_probe'])
        _report['collected'] = first['collected']
        _report['deselected'] = first['deselected']
        _report['original_inapplicable'] = first.get('original_inapplicable', [])
        if any(receipt.get('original_inapplicable', []) != _report['original_inapplicable'] for receipt in _worker_reports):
            _report['collection_errors'].append({'nodeid': 'workers', 'error': 'Original inapplicable combinations differ'})
        # The controller receives every worker's pytest log report through
        # xdist, so its outcome list already contains the actual full execution.
    port_workers = {}
    for allocation in _report.get('port_allocations', []):
        port_workers.setdefault(allocation['port'], set()).add(allocation['worker'])
    _report['port_allocation_summary'] = {
        'allocations': len(_report.get('port_allocations', [])),
        'per_worker': {str(worker): sum(r['worker'] == worker for r in _report.get('port_allocations', []))
                       for worker in {r['worker'] for r in _report.get('port_allocations', [])}},
        'cross_worker_reused_ports': {str(port): sorted(map(str, workers)) for port, workers in port_workers.items() if len(workers) > 1}}
    passed = {r['nodeid'] for r in _report['outcomes'] if r['phase'] == 'call' and r['outcome'] == 'passed'}
    bad = [r for r in _report['outcomes'] if r['outcome'] != 'passed']
    unexplained = [r for r in _report['deselected'] if r['predicate'] is None or r['product_capability_vetoes']]
    _report.update(exit_code=int(exitstatus), passed=len(passed), failed=sum(r['outcome'] == 'failed' for r in bad),
                   skipped=sum(r['outcome'] == 'skipped' for r in _report['outcomes']))
    _report['status'] = 'passed' if (exitstatus == 0 and passed and not bad and not unexplained
        and not _report['collection_errors'] and len(passed) == len(_report['collected'])) else 'failed'
    destination.write_text(json.dumps(_report, indent=2) + '\n')
    if _report['status'] != 'passed': session.exitstatus = pytest.ExitCode.TESTS_FAILED
