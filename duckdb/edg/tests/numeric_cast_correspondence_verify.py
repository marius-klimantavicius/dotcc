#!/usr/bin/env python3
"""Audit the pinned private rename and compare native/unchanged EDG C behavior."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from portable import adapt_numeric_cast

ROOT = Path(__file__).resolve().parents[3]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--oracle', type=Path, default=ROOT / 'edgcpp/build/bin/cpfe')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    header = Path('src/include/duckdb/common/operator/numeric_cast.hpp')
    staged = out / 'staged'
    (staged / header).parent.mkdir(parents=True)
    shutil.copy2(args.source / header, staged / header)
    report = dict(passed=False, commands=[], adaptation=adapt_numeric_cast(args.source.resolve(), staged))
    original = (args.source / header).read_text()
    assert (staged / header).read_text().replace('struct NumericCastOperator {', 'struct NumericCast {') == original
    report['implementationUnchanged'] = True
    primary = ('namespace duckdb { template<class TO, class FROM> TO NumericCast(FROM x) { return static_cast<TO>(x); } '
               'struct NumericCast { static int Operation(int x) { return x; } }; }\n'
               'extern int other(); int main() { return duckdb::NumericCast<int>(3) != other(); }\n')
    secondary = ('namespace duckdb { template<class TO, class FROM> TO NumericCast(FROM x) { return static_cast<TO>(x); } }\n'
                 'int other() { return duckdb::NumericCast<int>(3); }\n')
    (out / 'primary.cpp').write_text(primary)
    (out / 'secondary.cpp').write_text(secondary)
    (out / 'adapted.cpp').write_text(primary.replace('struct NumericCast {', 'struct NumericCastOperator {')
        .replace('!= other();', '!= other() || duckdb::NumericCastOperator::Operation(7) != 7;'))
    env = dict(os.environ, EDG_BASE=str(ROOT / 'edgcpp/upstream/bases/docker/dev-env/gcc'))

    def run(name, command, reject=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, env=env, capture_output=True, timeout=120)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        report['commands'].append(dict(stage=name, command=command, exitCode=result.returncode))
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        if reject:
            assert result.returncode and reject in result.stderr.decode()
        else:
            assert result.returncode == 0, name

    for name, source in [('original', 'primary.cpp'), ('adapted', 'adapted.cpp')]:
        run(name + '-native-build', ['g++', '-std=c++17', '-O2', out / source, out / 'secondary.cpp', '-o', out / (name + '-native')])
        run(name + '-native-run', [out / (name + '-native')])
        run(name + '-c-lower', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--gen_c_file_name', out / (name + '.c'), '-o', out / (name + '.il'), out / source, out / 'secondary.cpp'],
            reject='incompatible with a declaration in another' if name == 'original' else None)
        if name == 'adapted':
            run('adapted-c-build', ['gcc', '-std=gnu11', '-w', out / 'adapted.c', ROOT / 'edgcpp/build/lib/libC.a', '-o', out / 'adapted-c'])
            run('adapted-c-run', [out / 'adapted-c'])
    collision = out / 'collision'
    collision.mkdir()
    (collision / 'collision.hpp').write_text('struct NumericCastOperator {};\n')
    try:
        adapt_numeric_cast(collision, staged)
    except ValueError as error:
        assert 'replacement name already exists' in str(error)
        report['replacementCollisionRejected'] = True
    else:
        raise AssertionError('Replacement collision accepted')
    report['inputs'] = [dict(path=str(path.resolve()), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
                        for path in (Path(__file__), Path(__file__).parents[1] / 'portable.py', args.oracle,
                                     args.source / header, out / 'primary.cpp', out / 'secondary.cpp', out / 'adapted.cpp')]
    report['passed'] = True
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print('NumericCast source audit, native/C correspondence, and collision guard passed')


if __name__ == '__main__':
    main()
