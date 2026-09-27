#!/usr/bin/env python3
"""Build/run test-only native controls; source acquisition belongs to the recipe."""
from __future__ import annotations
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import subprocess
import shutil

ROOT = Path(__file__).resolve().parents[2]
SELECTED = ('test_closure.js', 'test_language.js', 'test_loop.js', 'test_bigint.js', 'test_builtin.js')
CALL = re.compile(r'^(test(?:_\w+)?)\(([^\n]*)\);$', re.M)


def main() -> int:
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--cc', default='cc')
    args = parser.parse_args()
    source, output = args.source.resolve(), args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    pin = json.loads((ROOT / 'config/source.json').read_text())
    observed_version = (source / 'VERSION').read_text().strip()
    spec = importlib.util.spec_from_file_location('quickjs_stage', ROOT / 'scripts/stage.py')
    stage = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(stage)
    embedded = output / 'embedded-source'
    if embedded.exists():
        shutil.rmtree(embedded)
    adaptations = stage.stage(source, embedded, native_dispatch=True)
    units = [s.strip() for s in (ROOT / 'config/core-sources.txt').read_text().splitlines() if s.strip() and not s.startswith('#')]
    staged_tests = output / 'upstream-tests'
    staged_tests.mkdir(exist_ok=True)
    manifest = {}
    for name in SELECTED:
        text = (source / 'tests' / name).read_text()
        cases = [m.group(1) for m in CALL.finditer(text) if m.group(1) != 'test_finalization_registry']
        if not cases or len(set(cases)) != len(cases):
            raise RuntimeError(f'ambiguous case inventory: {name}')
        manifest[name] = cases
        # Every original test/assertion remains. Wrap only upstream top-level entry calls.
        text = CALL.sub(lambda m: '// Excluded: test_finalization_registry needs os.setTimeout host event loop.' if m.group(1) == 'test_finalization_registry' else f'__case({json.dumps(name + ":" + m.group(1))}, () => {{ {m.group(0)} }});', text)
        (staged_tests / name).write_text(text)
    frozen = json.loads((ROOT / 'tests/Upstream/cases.json').read_text())
    if manifest != frozen['files']:
        raise RuntimeError('Bundled test entrypoints differ from frozen cases.json; review inventory')
    (output / 'upstream-manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    depth_modules = output / 'module-depth'
    depth_modules.mkdir(exist_ok=True)
    for i in range(512):
        (depth_modules / f'm{i}.js').write_text('export const last=1;' if i == 511 else f"import './m{i+1}.js';")
    env = dict(os.environ, TZ='UTC')
    records = []
    def execute(case: str, command: list[str], expected: int = 0, expected_cases: int = 0, marker: str | None = None, expected_ids: list[str] | None = None):
        try:
            result = subprocess.run(command, capture_output=True, text=True, env=env, timeout=180)
            stdout, stderr, code = result.stdout, result.stderr, result.returncode
        except subprocess.TimeoutExpired as e:
            stdout = e.stdout.decode() if isinstance(e.stdout, bytes) else e.stdout or ''
            stderr, code = 'TIMEOUT\n', None
        (output / f'{case}.stdout.log').write_text(stdout)
        (output / f'{case}.stderr.log').write_text(stderr)
        passed_cases = [line[len('CASE PASS '):] for line in stdout.splitlines() if line.startswith('CASE PASS ')]
        passed = code == expected and len(passed_cases) == expected_cases and (marker is None or marker in stdout + stderr) and (expected_ids is None or passed_cases == expected_ids)
        record = dict(case=case, command=command, exit_code=code, expected_exit_code=expected, status='pass' if passed else 'fail', passed_cases=passed_cases, expected_cases=expected_cases)
        records.append(record)
        print(f'{case}: {record["status"]} exit={code} cases={len(passed_cases)}/{expected_cases}', flush=True)
        return passed
    execute('compiler-version', [args.cc, '--version'])
    for name, root in [('stock', source), ('embedded', embedded)]:
        binary = output / f'quickjs-oracle-{name}'
        command = [args.cc, '-std=gnu11', '-funsigned-char', '-O2', '-g', '-D_GNU_SOURCE', f'-DCONFIG_VERSION="{observed_version}"', '-I', str(root), str(ROOT / 'tests/NativeOracle/oracle.c'), str(ROOT / 'tests/NativeOracle/atomics.c'), str(ROOT / 'tests/Abi/native.c')]
        command += [str(root / unit) for unit in units] + ['-lm', '-ldl', '-lpthread', '-o', str(binary)]
        if not execute(f'{name}-build', command):
            continue
        execute(f'{name}-workflow', [str(binary), '--workflow'], marker='callbacks=3 modules=1')
        execute(f'{name}-failure-control', [str(binary), '--fail'], expected=1, marker='deliberate assertion failure')
        execute(f'{name}-abi', [str(binary), '--abi'], marker='"valueSize":16')
        execute(f'{name}-behavior', [str(binary), '--file', str(ROOT / 'tests/fixtures/core.js')], expected_cases=10)
        execute(f'{name}-atomics', [str(binary), '--file', str(ROOT / 'tests/fixtures/atomics.js')], expected_cases=5)
        execute(f'{name}-atomics-wait', [str(binary), '--file', str(ROOT / 'tests/fixtures/atomics-wait.js'), '--can-block'], expected_cases=1)
        execute(f'{name}-atomics-threads', [str(binary), '--atomics-threads', str(ROOT / 'tests/fixtures/atomics-worker.js')], expected_cases=2)
        for filename, cases in manifest.items():
            execute(f'{name}-{filename}', [str(binary), '--file', str(staged_tests / filename)], expected_cases=len(cases), expected_ids=[filename + ':' + case for case in cases])
        execute(f'{name}-cyclic-import', [str(binary), '--file', str(source / 'tests/test_cyclic_import.js'), '--module', str(source / 'tests')])
        execute(f'{name}-module-depth', [str(binary), '--file', str(depth_modules / 'm0.js'), '--module', str(depth_modules)], expected=1, marker='stack overflow')
    for case in ('workflow', 'abi', 'atomics', 'atomics-wait', 'atomics-threads'):
        stock_log = output / f'stock-{case}.stdout.log'
        embedded_log = output / f'embedded-{case}.stdout.log'
        same = stock_log.exists() and embedded_log.exists() and stock_log.read_bytes() == embedded_log.read_bytes()
        records.append(dict(case=f'dispatch-comparison-{case}', status='pass' if same else 'fail'))
    report = dict(source=str(source), pinned_version=pin['version'], observed_version=observed_version,
                  source_sha256={u: hashlib.sha256((source / u).read_bytes()).hexdigest() for u in units},
                  harness_sha256={str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in
                                  [ROOT / 'tests/NativeOracle/oracle.c', ROOT / 'tests/NativeOracle/atomics.c', ROOT / 'tests/Abi/native.c',
                                   ROOT / 'tests/fixtures/atomics.js', ROOT / 'tests/fixtures/atomics-wait.js', ROOT / 'tests/fixtures/atomics-worker.js',
                                   ROOT / 'tests/fixtures/core.js', ROOT / 'tests/Upstream/cases.json']},
                  adaptations=adaptations, manifest=manifest, exclusions={'test_builtin.js:test_finalization_registry': 'Requires os.setTimeout host event loop, explicitly outside embedded profile'}, results=records,
                  status='pass' if all(r['status'] == 'pass' for r in records) else 'fail')
    (output / 'native-report.json').write_text(json.dumps(report, indent=2) + '\n')
    return 0 if report['status'] == 'pass' else 1

if __name__ == '__main__':
    raise SystemExit(main())
