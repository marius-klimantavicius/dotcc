#!/usr/bin/env python3
"""Verify independent TU packaging, restart integrity and process timeout cleanup."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs with spaces'
    inputs.mkdir()
    (inputs / 'value.h').write_text('#define PROJECT_VALUE 23\nint project_value(void);\n')
    (inputs / 'provider.c').write_text('#include "value.h"\nint project_value(void) { return PROJECT_VALUE; }\n')
    (inputs / 'consumer.c').write_text('#include "value.h"\nint main(void) { return project_value() == 23 ? 0 : 1; }\n')
    missing = out / 'optional includes'
    manifest = dict(schema=1, project='project-driver-verification', units=[
        dict(id=name, source=str(inputs / name), standard='c11', arguments=['-I', str(missing)], role='library')
        for name in ('provider.c', 'consumer.c')])
    manifest_path = out / 'project.json'
    manifest_path.write_text(json.dumps(manifest, indent=2) + '\n')
    report = dict(checks=[], commands=[])

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        report['checks'].append(name)

    def run(label, directory, extra=(), expected=0, compiler=None):
        command = [sys.executable, '-B', str(ROOT / 'project.py'), 'build', '--manifest', str(manifest_path),
                   '--output-dir', str(directory), '--compiler', str(compiler or args.compiler.resolve()), *extra]
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr)
        report['commands'].append(dict(label=label, command=command, exitCode=result.returncode))
        check(label + ' exit', result.returncode == expected)
        return json.loads((directory / 'results.json').read_text())

    try:
        built = out / 'build'
        first = run('package', built, ('--jobs', '2'))
        check('complete project packaged', first['status'] == 'packaged' and first['packagedUnits'] == 2 and first['completeSelection'])
        objects = []
        for result in first['units'].values():
            path = Path(result['object'])
            objects.append(path)
            invocation = json.loads((Path(result['work']) / 'invocation.json').read_text())
            check(path.parent.name + ' one original input', '--preprocess' not in invocation['command'] and invocation['command'][-1].endswith('.c'))
            with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
                check(path.parent.name + ' includes retained', db.execute("SELECT count(*) FROM source_files WHERE path LIKE '%value.h' AND bytes IS NOT NULL").fetchone()[0] == 1)
                check(path.parent.name + ' macro retained', db.execute("SELECT count(*) FROM macro_definitions WHERE name='PROJECT_VALUE'").fetchone()[0] == 1)
        before = {str(p): p.stat().st_mtime_ns for p in objects}
        resumed = run('resume', built, ('--resume',))
        check('resume reuses successful objects', before == {str(p): p.stat().st_mtime_ns for p in objects})
        check('resume retains attempts', resumed['units'] == first['units'])
        impossible_floor = str(shutil.disk_usage(out).total // (1024 * 1024) + 1)
        retained = run('storage-floor-existing', built, ('--resume', '--min-free-space-mib', impossible_floor))
        check('storage floor retains completed units', retained['units'] == first['units'])
        limited_dir = out / 'storage-limited'
        limited = run('storage-floor', limited_dir, ('--jobs', '2', '--min-free-space-mib', impossible_floor), expected=1)
        check('storage floor defers all unscheduled units', limited['status'] == 'storage-limited' and
              limited['packagedUnits'] == 0 and limited['deferredUnits'] == 2)
        check('storage floor starts no compiler attempts', not list(limited_dir.glob('*/request.json')))
        recovered = run('storage-floor-resume', limited_dir, ('--resume',))
        check('storage-limited project resumes completely', recovered['status'] == 'packaged' and
              recovered['packagedUnits'] == 2 and recovered['deferredUnits'] == 0)
        victim = objects[0]
        with victim.open('ab') as stream:
            stream.write(b'object corruption')
        repaired = run('corrupt-retry', built, ('--resume',))
        check('corruption gets fresh attempt', any(x['object'] not in before for x in repaired['units'].values()))
        check('corrupt intermediate retained', victim.read_bytes().endswith(b'object corruption'))
        missing.mkdir()
        (missing / 'value.h').write_text('#define PROJECT_VALUE 99\nint project_value(void);\n')
        run('new-header-rejects-cache', built, ('--resume',), expected=1)
        check('header lookup change diagnosed', 'headers' in (out / 'new-header-rejects-cache.stderr').read_text())

        spec = importlib.util.spec_from_file_location('project_driver', ROOT / 'project.py')
        driver = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(driver)
        for bad in ('--preprocess', '--no_code_gen', '/tmp/unexpected.c'):
            try:
                driver.frontend_arguments([bad])
            except ValueError:
                check('reject ' + bad, True)
            else:
                raise AssertionError('Accepted forbidden frontend switch ' + bad)

        if os.name == 'posix':
            fake = out / 'sleeping-compiler'
            fake.write_text('#!' + sys.executable + '\nimport os,time\n'
                            'from pathlib import Path\n'
                            'Path("compiler.pid").write_text(str(os.getpid()))\n'
                            'if os.fork() == 0:\n'
                            ' Path("child.pid").write_text(str(os.getpid()))\n'
                            'time.sleep(60)\n')
            fake.chmod(0o755)
            timed = run('timeout', out / 'timeout', ('--timeout', '0.5', '--limit', '1'), expected=1, compiler=fake)
            check('timeout is not packaged', timed['status'] == 'failed' and timed['packagedUnits'] == 0)
            for result in timed['units'].values():
                check('timeout receipt', result['timeout'])
                for name in ('compiler.pid', 'child.pid'):
                    pid = int((Path(result['work']) / name).read_text())
                    if Path('/proc').exists():
                        for _ in range(20):
                            status = Path('/proc') / str(pid) / 'stat'
                            if not status.exists() or status.read_text().split()[2] == 'Z':
                                break
                            time.sleep(0.05)
                        else:
                            raise AssertionError('Timed-out compiler descendant still running')
                        check(name + ' stopped', True)
        report['status'] = 'passed'
    finally:
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(str(len(report['checks'])) + ' project driver checks passed')


if __name__ == '__main__':
    main()
