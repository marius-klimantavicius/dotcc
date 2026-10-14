#!/usr/bin/env python3
"""Export pristine positive EDG inputs and one real C++ translation unit."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import sqlite3
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT.parent / 'artifacts/roslynobj'
PIN = '89e67e07c7f6d0fc38622f9778d8e79f40deaa0f'
CASES = {
    'upstream_templates': ('tests/tests/edg/cmeerw/gnats-23971.sft.cpp', ['--c++20', '--strict']),
    'upstream_lifetimes': ('tests/tests/edg/wcc/gnats27497-missing-dtor-instantiation.sft.cpp', ['--c++11']),
    # The pristine test has a separate NEG branch; this is its positive invocation.
    'upstream_inheritance': ('tests/tests/edg/ccs/inhctor/gnats17687c.sft.cpp', ['--c++17']),
    'upstream_types': ('src/types.c', None),
}


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n')


def source_identity(upstream, relative):
    path = upstream / relative
    data = path.read_bytes()
    actual = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
    expected = subprocess.check_output(
        ['git', '-C', str(upstream), 'rev-parse', PIN + ':' + relative], text=True).strip()
    assert actual == expected, ('Input differs from pristine Git pin', relative)
    return dict(path=str(path), relative_path=relative, bytes=len(data),
                sha256=hashlib.sha256(data).hexdigest(), git_blob=actual, revision=PIN)


def types_options(compiler_root, upstream):
    build = compiler_root / 'build'
    target = 'src/cmake/cpfe/CMakeFiles/roslyn-cpfe.dir/__/__/types.c.o'
    host = shlex.split(subprocess.check_output(
        ['ninja', '-C', str(build), '-t', 'commands', target], text=True).splitlines()[-1])
    version_text = subprocess.check_output([host[0], '-dumpfullversion'], text=True).strip()
    version = [int(n) for n in version_text.split('.')]
    version += [0] * (3 - len(version))
    search_argv = [host[0], '-E', '-x', 'c++', '-v', '-']
    search = subprocess.run(search_argv, input='', text=True, capture_output=True, check=True)
    paths = search.stderr.split('#include <...> search starts here:', 1)[1].split('End of search list.', 1)[0]
    system = [line.strip().removesuffix(' (framework directory)') for line in paths.splitlines() if line.strip()]
    defines = [arg for arg in host if arg.startswith('-D')]
    includes = [arg for arg in host if arg.startswith('-I')]
    # These real GCC limits.h dependencies are not EDG predefined macros.
    dump_argv = [host[0], '-std=c++14', '-fno-exceptions', '-fno-rtti', '-dM', '-E', '-x', 'c++', '-']
    dump = subprocess.run(dump_argv, input='', text=True, capture_output=True, check=True)
    macros = {parts[1]: parts[2] for line in dump.stdout.splitlines()
              if len(parts := line.split(maxsplit=2)) == 3 and parts[0] == '#define'}
    delta = {name: macros[name] for name in ('__CHAR_BIT__', '__SCHAR_MAX__')}
    options = ['--c++14', '--g++', '--gnu_version', str(version[0]*10000 + version[1]*100 + version[2]),
               '--no_exceptions', '--no_rtti', *defines, *includes]
    for path in system:
        options.extend(['--sys_include', path])
    options.extend('-D' + name + '=' + value for name, value in delta.items())
    recipe = dict(host_compile_argv=host, host_version=version_text, ninja_target=target,
                  source=str(upstream / 'src/types.c'), edg_options=options,
                  system_include_discovery_argv=search_argv, system_include_directories=system,
                  host_predefined_discovery_argv=dump_argv, required_host_predefined_delta=delta,
                  host_only_options=[arg for arg in host if arg not in defines + includes
                                     and arg not in (host[0], host[-1])],
                  mapping='C++14/GNU version/no-exceptions/no-RTTI are supported EDG equivalents; '
                          'actual defines/includes and two discovered host limits macros are preserved. '
                          'Host optimization/debug/warning/dependency/code-generation/output options '
                          'are not exporter options. This is not literal compiler-command equivalence.')
    return options, recipe


def storage_metrics(row, expected):
    path = Path(row['path']).resolve()
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        tables = db.execute("SELECT name FROM sqlite_schema WHERE type='table'").fetchall()
        caps = dict(db.execute('SELECT n.value,v.value FROM build_capability b '
                               'JOIN text_atom n ON n.id=b.name_id JOIN text_atom v ON v.id=b.value_id'))
        assert len(tables) == 251 and caps['normalized_semantic_schema_v1'] == expected
        assert caps['roslyn_build_profile'] == row['profile']
        assert db.execute('SELECT count(*) FROM object_header WHERE complete=1').fetchone()[0] == 1
        assert not db.execute('PRAGMA foreign_key_check').fetchall()
        assert db.execute('PRAGMA page_size').fetchone()[0] == 1024
        for (table,) in tables:
            assert all(column[2].upper() not in ('BLOB', 'REAL')
                       for column in db.execute('PRAGMA table_info("' + table + '")'))
        sizes = [dict(name=n, schema_type=t, pages=p, bytes=b, payload_bytes=s)
                 for n,t,p,b,s in db.execute(
                     "SELECT d.name,coalesce(s.type,'schema'),count(*),sum(d.pgsize),sum(d.payload) "
                     "FROM dbstat d LEFT JOIN sqlite_schema s ON s.name=d.name GROUP BY d.name,s.type")]
        atom_queries = []
        for (table,) in tables:
            for fk in db.execute('PRAGMA foreign_key_list("' + table + '")'):
                if fk[2] == 'text_atom':
                    atom_queries.append('SELECT "' + fk[3] + '" atom_id FROM "' + table
                                        + '" WHERE "' + fk[3] + '" IS NOT NULL')
        atom_usage = 'SELECT atom_id,count(*) uses FROM (' + ' UNION ALL '.join(atom_queries) + ') GROUP BY atom_id'
        atom = db.execute('SELECT count(*),sum(uses),sum(uses>1),max(uses),'
                          'sum(length(cast(t.value AS BLOB))*uses) FROM (' + atom_usage
                          + ') a JOIN text_atom t ON t.id=a.atom_id').fetchone()
        value = db.execute('SELECT count(*),sum(uses),sum(uses>1),max(uses) FROM '
                           '(SELECT value_id,count(*) uses FROM constant GROUP BY value_id)').fetchone()
        table_counts = {table: db.execute('SELECT count(*) FROM "' + table + '"').fetchone()[0]
                        for (table,) in tables}
        result = dict(path=str(path), bytes=path.stat().st_size, sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                      entities=table_counts['entity'], tables=len(tables), page_size=1024,
                      page_count=db.execute('PRAGMA page_count').fetchone()[0],
                      freelist_pages=db.execute('PRAGMA freelist_count').fetchone()[0], fk_errors=0,
                      schema_contract=expected, complete_headers=1, blob_real_columns=0,
                      table_bytes=sum(r['bytes'] for r in sizes if r['schema_type']=='table'),
                      index_bytes=sum(r['bytes'] for r in sizes if r['schema_type']=='index'),
                      schema_bytes=sum(r['bytes'] for r in sizes if r['schema_type']=='schema'),
                      storage_objects=sizes, table_counts=table_counts,
                      text_atoms=dict(rows=table_counts['text_atom'], referenced_atoms=atom[0],
                                      total_references=atom[1], multiply_referenced_atoms=atom[2],
                                      max_references_per_atom=atom[3],
                                      unique_text_bytes=db.execute('SELECT sum(length(cast(value AS BLOB))) FROM text_atom').fetchone()[0],
                                      text_bytes_if_repeated_per_reference=atom[4]),
                      constant_values=dict(rows=table_counts['constant_value'], occurrences=table_counts['constant'],
                                           referenced_values=value[0], total_references=value[1],
                                           multiply_referenced_values=value[2], max_references_per_value=value[3]))
    return result


def measured_times(row):
    text = Path(row['timing_log']).read_text()
    phases = {name: dict(cpu_seconds=float(cpu), reported_elapsed_seconds=float(elapsed))
              for name,cpu,elapsed in re.findall(
                  r'^(Front end time|Back end time|Total compilation time)\s+([0-9.]+) \(CPU\)\s+([0-9.]+) \(elapsed\)', text, re.M)}
    wall,rss,user,system = Path(row['time_file']).read_text().splitlines()[-1].split()
    return dict(stock_timing=phases, combined_wrapper_wall_seconds=float(wall),
                wrapper_monotonic_wall_seconds=row['seconds'], max_producer_rss_kib=int(rss),
                process_user_cpu_seconds=float(user), process_system_cpu_seconds=float(system),
                timing_scope='Wall/RSS cover wrapper + frontend + exporter; stock phase CPU is separate, rounded to 0.01s.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--upstream', type=Path, default=Path('/tmp/roslynobj-edg-upstream-20261012'))
    parser.add_argument('--profile', choices=('native', 'source'), default='native')
    parser.add_argument('--case', choices=tuple(CASES), action='append')
    parser.add_argument('--output-directory', type=Path)
    parser.add_argument('--inspect-existing', action='store_true', help='Collect metrics/pin evidence without repeating compiler runs.')
    args = parser.parse_args()
    upstream = args.upstream.resolve()
    output = (args.output_directory or ARTIFACTS / 'root251/real-inputs').resolve()
    output.mkdir(parents=True, exist_ok=True)
    compiler_root = ARTIFACTS if args.profile=='native' else ARTIFACTS / 'profiles' / args.profile
    selected = args.case or list(CASES)
    identities = {case: source_identity(upstream, CASES[case][0]) for case in selected}
    save(output / 'input-pin-proof.json', identities)
    manifest = output / 'all-profile-results-final.json'
    attempts = output / 'attempt-results.json'
    records = json.loads(attempts.read_text()) if attempts.exists() else []
    if not args.inspect_existing:
        folder = output / args.profile
        folder.mkdir(exist_ok=True)
        environment = os.environ.copy()
        environment['ROSLYNOBJ_COMPILER'] = str(compiler_root / 'build/bin/roslyn-cpfe')
        for case in selected:
            options = CASES[case][1]
            if options is None:
                options,recipe = types_options(compiler_root, upstream)
                save(output / 'types-host-edg-recipe.json', recipe)
            destination = folder / (case + '.roslynobj')
            timefile = folder / (case + '-time.txt')
            logfile = folder / (case + '.log')
            command = ['python3', str(ROOT / 'build/roslyn-cpfe.py'), *options,
                       '--timing', identities[case]['path'], '-o', str(destination)]
            timed = ['/usr/bin/time', '-f', '%e %M %U %S', '-o', str(timefile), 'nice', '-n', '10', *command]
            started = time.monotonic()
            with logfile.open('w') as log:
                result = subprocess.run(timed, env=environment, cwd=folder, stdout=log, stderr=subprocess.STDOUT)
            row = dict(profile=args.profile, fixture=case, source=identities[case]['path'],
                       source_relative_path=identities[case]['relative_path'], fixture_sha256=identities[case]['sha256'],
                       argv=command, time_argv=timed, compiler=environment['ROSLYNOBJ_COMPILER'],
                       path=str(destination), timing_log=str(logfile), time_file=str(timefile),
                       exit_code=result.returncode, seconds=round(time.monotonic()-started, 3))
            records.append(row)
            save(attempts, records)
            if result.returncode:
                print(json.dumps(row), flush=True)
                raise SystemExit(result.returncode)
    accepted = {}
    for row in records:
        if row['exit_code']==0:
            accepted[row['profile'],row['fixture']] = row
    missing = {(args.profile, case) for case in selected} - accepted.keys()
    if missing:
        parser.error('Missing successful exporter runs for: ' +
                     ', '.join(profile + '/' + case for profile, case in sorted(missing)))
    metrics = []
    expected = (ROOT / 'schema/contract.sha256').read_text().strip()
    for key,row in accepted.items():
        if key[0]!=args.profile or key[1] not in selected:
            continue
        assert row['fixture_sha256']==identities[key[1]]['sha256']
        stats = storage_metrics(row, expected)
        stats.update(profile=row['profile'], fixture=row['fixture'], **measured_times(row))
        row.update({name:stats[name] for name in ('entities','tables','page_size','fk_errors','schema_contract')})
        metrics.append(stats)
        print(json.dumps({name:stats[name] for name in ('fixture','entities','bytes','table_bytes','index_bytes','combined_wrapper_wall_seconds','max_producer_rss_kib')}), flush=True)
    save(output / 'storage-timing-metrics.json', metrics)
    save(manifest, list(accepted.values()))


if __name__ == '__main__':
    main()
