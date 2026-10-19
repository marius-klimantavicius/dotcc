"""Check real lifecycle call edges in each consumer's exact compilation input PE.

Consumes B's unmodified metadata-reader output. This is a hook-algorithm and
input-identity check, not a full D10/ABI or post-trimming child-execution proof.
"""
import argparse
import hashlib
import json
from pathlib import Path

CONTRACTS = {
    'PyOS_BeforeFork': ['_PyImport_AcquireLock', '_PyEval_StopTheWorldAll', 'PyMutex_LockFlags', 'run_at_forkers'],
    'PyOS_AfterFork_Parent': ['_PyMutex_Unlock', '_PyEval_StartTheWorldAll', '_PyImport_ReleaseLock', 'run_at_forkers'],
    'PyOS_AfterFork_Child': ['_PyRuntimeState_ReInitThreads', '_PyEval_ReInitThreads', '_PyImport_ReInitLock', '_PySignal_AfterFork', 'run_at_forkers'],
    'os_fork_impl': ['PyOS_BeforeFork', 'PyOS_AfterFork_Parent', 'PyOS_AfterFork_Child'],
    'subprocess_fork_exec_impl': ['PyOS_BeforeFork', 'PyOS_AfterFork_Parent'],
}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def inspect(report_path):
    report = json.loads(Path(report_path).read_text())
    assert len(report['cells']) == 4
    assert {(c['form'], c['mode']) for c in report['cells']} == {
        (form, mode) for form in ['raw', 'processed'] for mode in ['jit', 'aot']}
    rows = []
    for cell in report['cells']:
        assert cell['status'] == 'passed'
        binding = cell['actual_compilation_input']
        metadata_path = cell['metadata']['path']
        metadata = json.loads(Path(metadata_path).read_text())
        assert sha(binding['pe']) == binding['sha256'] == metadata['sha256']
        assert sha(metadata_path) == cell['metadata']['sha256']
        assert metadata['mvid'] == cell['metadata']['mvid']
        algorithms = []
        for name, expected in CONTRACTS.items():
            candidates = [m for m in metadata['methods'] if m['type'] == 'Managed.Interpreters.CPython'
                          and (m['name'] == name or m['name'].startswith(name+'__unit_'))]
            assert len(candidates) == 1, (name, candidates)
            method = candidates[0]
            assert method['il_bytes'] > 1
            edges = [c for c in metadata['calls'] if c['caller_type'] == method['type']
                     and c['caller_method'] == method['name']]
            for target in expected:
                assert any(c['target_method'] == target or c['target_method'].startswith(target+'__unit_')
                           for c in edges), (name, target)
            algorithms.append({'method': method, 'actual_call_edges': edges})
        for item in metadata['native_imports']:
            assert not any(word in str(item).lower() for word in ['__wrap_fork', '__wrap_vfork', 'libpython'])
        if cell['mode'] == 'aot':
            assert sha(binding['ilc_response']) == binding['ilc_response_sha256']
            assert sha(binding['native_object']) == binding['native_object_sha256']
            assert sha(binding['entry_assembly']) == binding['entry_assembly_sha256']
            assert sha(binding['executable']) == binding['executable_sha256']
        rows.append({'form': cell['form'], 'mode': cell['mode'], 'binding': binding,
                     'metadata_sha256': sha(metadata_path), 'algorithms': algorithms,
                     'native_imports': metadata['native_imports']})
    return {'scope': __doc__, 'controls_report': str(Path(report_path).resolve()),
            'controls_report_sha256': sha(report_path), 'inputs': rows,
            'child_execution': 0, 'inherited_v9_qualification': [],
            'full_profile_D10_ABI_qualification': 'open'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--controls', type=Path, required=True)
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    result = inspect(args.controls)
    args.report.write_text(json.dumps(result, indent=2)+'\n')
    print('actual compilation input hook-algorithm reviews:', len(result['inputs']))
