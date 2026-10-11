#!/usr/bin/env python3
"""One EDG invocation writes a relational semantic .dotobj directly."""
import argparse
import os
from pathlib import Path
import shlex
import sqlite3
import subprocess

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--work-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--std', choices=['c11', 'c++14', 'c++17', 'c++20'], default='c++20')
    parser.add_argument('-I', dest='includes', type=Path, action='append', default=[])
    parser.add_argument('-D', dest='defines', action='append', default=[])
    args = parser.parse_args()
    source, output, work, compiler = [p.resolve() for p in (args.source, args.output, args.work_dir, args.compiler)]
    if not source.is_file():
        parser.error('Source does not exist')
    if output.exists() or output.suffix != '.dotobj':
        parser.error('--output must be a new .dotobj file')
    work.mkdir(parents=True, exist_ok=False)
    output.parent.mkdir(parents=True, exist_ok=True)
    partial = output.with_name(output.name + '.partial')
    if partial.exists():
        parser.error('Partial output already exists; use a new output path')
    command = [str(compiler), '--' + args.std, '--gnu_version', '130300',
               '--clear_flag', 'use_predefined_macro_file']
    if args.std.startswith('c++'):
        command += ['--instantiate', 'used', '--exceptions', '--rtti']
    for include in args.includes:
        command += ['-I', str(include.resolve())]
    for define in args.defines:
        command += ['-D' + define]
    command += ['--', str(source)]
    env = dict(os.environ)
    for key in list(env):
        if key.startswith('EDG_DOT_'):
            del env[key]
    env.update(EDG_BASE=str(ROOT / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_SEMANTIC=str(partial), EDG_DOT_TARGET='portable-lp64-binary80-v2')
    (work / 'command.txt').write_text(shlex.join(command) + '\n')
    with (work / 'stdout.txt').open('wb') as out, (work / 'stderr.txt').open('wb') as err:
        result = subprocess.run(command, env=env, cwd=work, stdout=out, stderr=err)
    if result.returncode:
        raise RuntimeError(f'EDG export failed ({result.returncode}); see {work / "stderr.txt"}')
    if not partial.is_file():
        raise RuntimeError('EDG did not produce an object')
    with sqlite3.connect(partial.as_uri() + '?mode=ro', uri=True) as db:
        row = db.execute('SELECT format FROM object_info WHERE singleton=1').fetchone()
        if row != ('relational-program-v1',):
            raise RuntimeError('Missing or unsupported semantic object header')
    # Publish without overwriting an existing output. Keep the original artifact.
    os.link(partial, output)
    print(output)


if __name__ == '__main__':
    main()
