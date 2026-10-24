#!/usr/bin/env python3
"""Build/run the actual copied-source Npgsql acceptance matrix in private outputs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(command, log, cwd, timeout, records):
    started = time.monotonic()
    print('RUN ' + ' '.join(map(str, command)), flush=True)
    record = dict(command=list(map(str, command)), cwd=str(cwd), log=str(log))
    records.append(record)
    try:
        with log.open('w') as stream:
            completed = subprocess.run(command, cwd=cwd, stdout=stream, stderr=subprocess.STDOUT,
                                       timeout=timeout, check=False, env=dict(os.environ, DOTNET_PROCESSOR_COUNT=os.environ.get('DOTNET_PROCESSOR_COUNT', '4')))
        record['exit_code'] = completed.returncode
    except subprocess.TimeoutExpired:
        record['timeout_seconds'] = timeout
        raise RuntimeError('Timed out; inspect ' + str(log))
    finally:
        record['elapsed_seconds'] = round(time.monotonic() - started, 3)
        if log.exists():
            record['log_sha256'] = sha256(log)
    if completed.returncode:
        print(log.read_text(errors='replace')[-8000:], flush=True)
        raise RuntimeError('Command failed; inspect ' + str(log))


def input_snapshot(root, backend, initdb):
    paths = []
    for directory in ('src/Api', 'src/Npgsql', 'src/Host', 'src/Initialization', 'tests/NpgsqlAcceptance'):
        for path in (root / directory).rglob('*'):
            if path.is_file() and path.suffix in ('.cs', '.csproj', '.snbtxt', '.json'):
                if not {'obj', 'bin'}.intersection(path.relative_to(root).parts):
                    paths.append(path)
    for project in (backend, initdb):
        paths += list(project.parent.glob('*.cs')) + [project]
    paths += list((root / 'samples/ManagedConsumer').glob('*.targets'))
    return {str(path.relative_to(root)): sha256(path) for path in sorted(set(paths))}


def runtime_artifacts(executable):
    directory = executable.parent
    paths = [path for path in directory.rglob('*') if path.is_file() and (
        path == executable or path.suffix == '.dll' or path.name.endswith(('.deps.json', '.runtimeconfig.json')))]
    return {str(path.relative_to(directory)): sha256(path) for path in sorted(paths)}


def collect_rows(args, root, work, mapping):
    forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
    modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
    selected = {}
    mapping_hash = sha256(root / 'tests/NpgsqlAcceptance/source-mapping.json')
    for path in sorted(work.glob('matrix-*.json'), reverse=True):
        if path.name == 'matrix-final.json':
            continue
        source = json.loads(path.read_text())
        if source.get('status') != 'passed' or source.get('source_mapping_sha256') != mapping_hash:
            continue
        for row in source.get('rows', []):
            key = (row['form'], row['mode'])
            if key in selected or key[0] not in forms or key[1] not in modes:
                continue
            artifact_directory = str(work / (key[0] + '-' + key[1]) / 'artifacts')
            if not any(command.get('exit_code') == 0 and len(command.get('command', [])) > 1
                       and command['command'][:2] in (['dotnet', 'build'], ['dotnet', 'publish'])
                       and artifact_directory in command['command'] for command in source.get('commands', [])):
                continue
            manifest_path = Path(row.get('input_manifest', ''))
            if not manifest_path.is_file() or sha256(manifest_path) != row['input_manifest_sha256']:
                continue
            suffix = '.Raw' if key[0] == 'raw' else ''
            backend = root / ('generated/TranslatedPGlite' + suffix) / 'TranslatedPGlite.csproj'
            initdb = root / ('generated/TranslatedInitdb' + suffix) / 'TranslatedInitdb.csproj'
            manifest = json.loads(manifest_path.read_text())
            current = input_snapshot(root, backend, initdb)
            if manifest['before'] != current or manifest['after'] != current:
                continue
            if sha256(Path(row['report'])) != row['report_sha256'] or sha256(Path(row['executable'])) != row['executable_sha256']:
                continue
            report = json.loads(Path(row['report']).read_text())
            if report['status'] != 'passed' or report['case_count'] != len(mapping['cases']) or report['failed_cases']:
                continue
            artifacts_now = runtime_artifacts(Path(row['executable']))
            if row.get('runtime_artifact_hashes', artifacts_now) != artifacts_now:
                continue
            selected[key] = dict(row, row_receipt=str(path), row_receipt_sha256=sha256(path),
                                 runtime_artifact_hashes=artifacts_now,
                                 runtime_artifact_hash_phase=row.get('runtime_artifact_hash_phase', 'recorded by final collector after row execution'))
    expected = [(form, mode) for form in forms for mode in modes]
    missing = [key for key in expected if key not in selected]
    if missing:
        raise RuntimeError('No current passing evidence for matrix rows: ' + repr(missing))
    counts = {row['assertions'] for row in selected.values()}
    if len(counts) != 1:
        raise RuntimeError('Assertion counts differ across matrix rows')
    receipt = dict(schema=1, status='passed', core_pin=mapping['core_pin'], npgsql_release=mapping['release'],
                   npgsql_commit=mapping['commit'], source_mapping_sha256=mapping_hash,
                   work=str(work), assertions_per_row=counts.pop(), cases_per_row=len(mapping['cases']),
                   evidence='Independently executed current rows; source snapshots, reports and executable hashes revalidated.',
                   rows=[selected[key] for key in expected])
    target = work / 'matrix-final.json'
    target.write_text(json.dumps(receipt, indent=2) + '\n')
    print(f"PASS {len(expected)} current matrix rows; {receipt['assertions_per_row']} assertions/{receipt['cases_per_row']} cases each; report={target}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--form', choices=('raw', 'processed', 'all'), default='processed')
    parser.add_argument('--mode', choices=('jit', 'aot', 'all'), default='jit')
    parser.add_argument('--rid', default='linux-x64')
    parser.add_argument('--configuration', choices=('Debug', 'Release'), default='Release')
    parser.add_argument('--work-root', type=Path)
    parser.add_argument('--collect-only', action='store_true', help='Validate and combine current passing row receipts without rebuilding or rerunning')
    parser.add_argument('--no-restore', action='store_true')
    parser.add_argument('--timeout', type=int, default=900)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    repo = root.parent
    work = (args.work_root or Path(tempfile.mkdtemp(prefix='dotcc-npgsql-matrix-'))).resolve()
    work.mkdir(parents=True, exist_ok=True)
    attempt = str(time.time_ns())
    receipt_path = work / ('matrix-' + attempt + '.json')
    project = root / 'tests/NpgsqlAcceptance/NpgsqlAcceptance.csproj'
    mapping = json.loads((project.parent / 'source-mapping.json').read_text())
    receipt = dict(schema=1, status='running', core_pin=mapping['core_pin'],
                   npgsql_release=mapping['release'], work=str(work), rows=[], commands=[])
    if args.collect_only:
        return collect_rows(args, root, work, mapping)
    expected_ids = {case['id'] for case in mapping['cases']}
    receipt['source_mapping_sha256'] = sha256(project.parent / 'source-mapping.json')
    receipt['expected_cases'] = len(expected_ids)
    receipt['npgsql_commit'] = mapping['commit']
    if not (root / 'src/Npgsql/Npgsql.csproj').is_file():
        raise RuntimeError('Copied-source Npgsql project is required: ' + str(root / 'src/Npgsql/Npgsql.csproj'))
    forms = ('raw', 'processed') if args.form == 'all' else (args.form,)
    modes = ('jit', 'aot') if args.mode == 'all' else (args.mode,)
    try:
        for form in forms:
            suffix = '.Raw' if form == 'raw' else ''
            backend = root / ('generated/TranslatedPGlite' + suffix) / 'TranslatedPGlite.csproj'
            initdb = root / ('generated/TranslatedInitdb' + suffix) / 'TranslatedInitdb.csproj'
            for mode in modes:
                before_inputs = input_snapshot(root, backend, initdb)
                label = form + '-' + mode
                row_dir = work / label
                row_dir.mkdir(exist_ok=True)
                artifacts = row_dir / 'artifacts'
                output = row_dir / 'publish'
                selection = ['-p:TranslatedProject=' + str(backend), '-p:InitdbTranslatedProject=' + str(initdb)]
                command = ['dotnet', 'publish' if mode == 'aot' else 'build', str(project),
                           '-c', args.configuration, '--artifacts-path', str(artifacts), '-m:1',
                           '-p:UseSharedCompilation=false', *selection]
                if args.no_restore:
                    command.append('--no-restore')
                if mode == 'aot':
                    command += ['-r', args.rid, '-p:PublishAot=true', '-o', str(output)]
                    if os.name != 'nt' and shutil.which('clang') is None:
                        compiler = next((shutil.which('clang-' + version) for version in ('22', '21', '19', '18')
                                         if shutil.which('clang-' + version)), None)
                        if compiler:
                            command.append('-p:CppCompilerAndLinker=' + compiler)
                run(command, row_dir / ('build-' + attempt + '.log'), repo, max(args.timeout, 3600), receipt['commands'])
                after_inputs = input_snapshot(root, backend, initdb)
                manifest_path = row_dir / ('inputs-' + attempt + '.json')
                manifest_path.write_text(json.dumps(dict(before=before_inputs, after=after_inputs), indent=2) + '\n')
                if after_inputs != before_inputs:
                    raise RuntimeError('Source changed during build; use fresh private outputs before qualification: ' + str(manifest_path))
                if mode == 'jit':
                    executable = artifacts / 'bin/NpgsqlAcceptance' / args.configuration.lower() / 'NpgsqlAcceptance.dll'
                    command = ['dotnet', str(executable)]
                else:
                    executable = output / ('NpgsqlAcceptance.exe' if os.name == 'nt' else 'NpgsqlAcceptance')
                    command = [str(executable)]
                report = row_dir / ('acceptance-' + attempt + '.json')
                # Intentionally execute outside the repository. Bootstrap assets must
                # come from the ordinary project build/publish output.
                unrelated = row_dir / 'unrelated-cwd'
                unrelated.mkdir(exist_ok=True)
                command += ['--work', str(row_dir / ('run-' + str(time.time_ns()))), '--report', str(report)]
                before_runtime = runtime_artifacts(executable)
                run(command, row_dir / ('run-' + attempt + '.log'), unrelated, args.timeout, receipt['commands'])
                after_runtime = runtime_artifacts(executable)
                if before_runtime != after_runtime:
                    raise RuntimeError('Runtime artifacts changed during acceptance: ' + str(executable))
                data = json.loads(report.read_text())
                if data['status'] != 'passed' or data['failed_cases'] != 0 or data['assertions'] <= 0:
                    raise RuntimeError('Semantic acceptance failed: ' + str(report))
                if data['case_count'] != len(expected_ids) or {case['id'] for case in data['cases']} != expected_ids:
                    raise RuntimeError('Executed case inventory differs from source mapping: ' + str(report))
                if receipt['rows'] and data['assertions'] != receipt['rows'][0]['assertions']:
                    raise RuntimeError('Assertion count differs across matrix rows: ' + str(report))
                if data['runtime'] != ('nativeaot' if mode == 'aot' else 'jit'):
                    raise RuntimeError('Unexpected execution mode: ' + str(report))
                receipt['rows'].append(dict(form=form, mode=mode, status='passed', assertions=data['assertions'],
                                            cases=data['case_count'], report=str(report), report_sha256=sha256(report),
                                            executable=str(executable), executable_sha256=sha256(executable),
                                            input_manifest=str(manifest_path), input_manifest_sha256=sha256(manifest_path),
                                            runtime_artifact_hashes=after_runtime,
                                            runtime_artifact_hash_phase='unchanged before and after row execution'))
                receipt_path.write_text(json.dumps(receipt, indent=2) + '\n')
        receipt['status'] = 'passed'
    except Exception as error:
        receipt['status'] = 'failed'
        receipt['error'] = str(error)
        raise
    finally:
        receipt_path.write_text(json.dumps(receipt, indent=2) + '\n')
        (work / 'matrix.json').write_text(json.dumps(receipt, indent=2) + '\n')
        print('Receipt: ' + str(receipt_path), flush=True)
    for row in receipt['rows']:
        print(f"PASS {row['form']}/{row['mode']}: {row['assertions']} assertions, {row['cases']} cases")


if __name__ == '__main__':
    main()
