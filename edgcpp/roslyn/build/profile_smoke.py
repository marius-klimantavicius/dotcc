#!/usr/bin/env python3
"""Export genuine fixtures for the alternative stock EDG build families."""
import argparse
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT.parents[0] / 'artifacts/roslynobj'
PROFILES = ('sun', 'usl21', 'usl30', 'softfloat', 'host-long-double', 'host-double', 'instantiation', 'extern-inline', 'separate-init', 'tableless')
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--profile', choices=PROFILES, required=True)
parser.add_argument('--output-directory', type=Path)
args = parser.parse_args()
output = (args.output_directory or ARTIFACTS / 'profile-smoke' / args.profile).resolve()
output.mkdir(parents=True, exist_ok=True)
environment = os.environ.copy()
environment['ROSLYNOBJ_COMPILER'] = str(ARTIFACTS / 'profiles' / args.profile / 'build/bin/roslyn-cpfe')
if args.profile == 'sun':
    cases = [('profile_sun', ['--c++20', '--sun', '--sun_linker_scope']),
             ('basic', ['--c++20', '--sun'])]
elif args.profile in ('usl21', 'usl30'):
    cases = [('profile_usl', ['--c++03', '--cfront_2.1' if args.profile == 'usl21' else '--cfront_3.0']),
             ('throw_accessible', ['--c++03', '--cfront_2.1' if args.profile == 'usl21' else '--cfront_3.0', '--exceptions']),
             ('basic', ['--c++20'])]
elif args.profile == 'instantiation':
    for sidecar in (output / 'instantiation.ii', output / 'instantiation.ti'):
        sidecar.unlink(missing_ok=True)
    cases = [('instantiation_flags', ['--c++20', '--old_specializations', '--auto_instantiation', '--instantiate', 'used',
                                     '--ii_file', str(output / 'instantiation.ii'),
                                     '--template_info_file', str(output / 'instantiation.ti')])]
elif args.profile == 'extern-inline':
    cases = [('objects', ['--c++20']),
             ('routines', ['--c++20', '--g++', '--gnu_version', '130000'])]
    for name, options in cases:
        for suffix in ('.ii', '.ti'):
            (output / (name + suffix)).unlink(missing_ok=True)
        options.extend(['--auto_instantiation', '--instantiate', 'used',
                        '--ii_file', str(output / (name + '.ii')),
                        '--template_info_file', str(output / (name + '.ti'))])
elif args.profile == 'separate-init':
    cases = [('separate_initialization', ['--c++20', '--g++', '--gnu_version', '130000']),
             ('named_register', ['--c99', '--named_registers'])]
elif args.profile == 'tableless':
    cases = [('m0', ['--c++20']), ('cleanup_semantics', ['--c++20']),
             ('retained', ['--c++23']), ('basic', ['--c++20']),
             ('tableless_eh', ['--c++20'])]
else:
    cases = [('profile_numeric', ['--c++20', '--g++', '--gnu_version', '130000']),
             ('exact', ['--c++20', '--g++', '--gnu_version', '130000']),
             ('nonreal_float', ['--c99']), ('fixed_point', ['--c99', '--fixed_point'])]
    if args.profile == 'softfloat':
        cases.append(('float_kinds', ['--c++23', '--gnu_version', '130000']))
    if args.profile == 'host-double':
        cases = [case for case in cases if case[0] != 'fixed_point']
results = []
for name, options in cases:
    if args.profile == 'host-double':
        options = [*options, '--target', 'win64']
    source = (ROOT / 'managed/RoslynObj.Tests/Fixtures' if name == 'routines' else ROOT / 'tests/fixtures') / (name + ('.c' if name in ('nonreal_float', 'fixed_point', 'named_register') else '.cpp'))
    destination = output / (name + '.roslynobj')
    command = ['python3', str(ROOT / 'build/roslyn-cpfe.py'), *options, str(source), '-o', str(destination)]
    start = time.monotonic()
    with (output / (name + '.log')).open('w') as log:
        process = subprocess.run(command, env=environment, cwd=output, stdout=log, stderr=subprocess.STDOUT)
    row = dict(profile=args.profile, fixture=name, argv=command, path=str(destination),
               exit_code=process.returncode, seconds=round(time.monotonic() - start, 3))
    if not process.returncode:
        with sqlite3.connect(destination.resolve().as_uri() + '?mode=ro', uri=True) as database:
            row['entities'] = database.execute('SELECT count(*) FROM entity').fetchone()[0]
            row['fk_errors'] = len(database.execute('PRAGMA foreign_key_check').fetchall())
            row['page_size'] = database.execute('PRAGMA page_size').fetchone()[0]
            row['capabilities'] = dict(database.execute('SELECT n.value,v.value FROM build_capability b JOIN text_atom n ON n.id=b.name_id JOIN text_atom v ON v.id=b.value_id'))
        assert row['capabilities']['roslyn_build_profile'] == args.profile
        assert not row['fk_errors']
    results.append(row)
    print(json.dumps({key: value for key, value in row.items() if key != 'capabilities'}), flush=True)
    (output / 'results.json').write_text(json.dumps(results, indent=2) + '\n')
    if process.returncode:
        raise SystemExit(process.returncode)
