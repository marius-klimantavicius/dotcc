#!/usr/bin/env python3
"""Verify the unevaluated DuckDB child-pair signature anchor against stock C."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from preflight import frontend_flags
from portable import verify_staged

ROOT = Path(__file__).resolve().parents[3]
EDG = ROOT / 'edgcpp'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--portable-manifest', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--oracle', type=Path, default=EDG / 'build/bin/cpfe')
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    portable = json.loads(args.portable_manifest.read_text())
    verify_staged(portable)
    flags, _ = frontend_flags(next(u for u in portable['units'] if u['role'] == 'library'))
    upstream = args.acquisition / 'libcxx-18.1.8.src/include'
    native_includes = ['-I', str(args.acquisition / 'config'), '-I', str(upstream)]
    portable_includes = [*native_includes, '-I', str(EDG / 'backend/runtime/libcxx/include'),
                        '-I', str(EDG / 'backend/runtime/posix/include'), '-I', str(EDG / 'backend/runtime/libc/include')]
    prefix = [str(args.compiler.resolve()), '--c++17', '--gnu_version', '130300', '--clear_flag',
              'use_predefined_macro_file', '--preprocess', *portable_includes,
              '--preinclude', str(ROOT / 'duckdb/edg/libcxx-preinclude.h')]
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'), EDG_DOT_TARGET='portable-lp64-binary80-v2')
    report = dict(passed=False, commands=[], checks=[], inputs=[])

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(name, command, reject=None, output=None):
        command = list(map(str, command))
        with (output or out / (name + '.stdout')).open('wb') as stdout, (out / (name + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, env=env, stdout=stdout, stderr=stderr, timeout=180)
        report['commands'].append(dict(stage=name, command=command, exitCode=result.returncode))
        save()
        if reject:
            assert result.returncode and reject in (out / (name + '.stderr')).read_text(), name
        else:
            assert result.returncode == 0, name

    common = ('#include <utility>\nstruct X { int x; X& operator=(const X& other) { x=other.x; return *this; } };\n'
              'using P = std::pair<int,X>;\n')
    anchor = 'static_assert(sizeof(static_cast<P& (P::*)(const P&)>(&P::operator=)) > 0);\n'
    bodies = {
        'primary': 'extern unsigned long second(); void first(P*a,const P*b){*a=*b;}\n'
                   'int main(){P a(1,X{2}),b(3,X{4});first(&a,&b);return a.first!=3||a.second.x!=4||second()!=sizeof(P);}\n',
        'secondary': 'unsigned long second(){return sizeof(P);}\n',
    }
    for mode in ('original', 'anchored'):
        files = []
        for name, body in bodies.items():
            path = out / (mode + '-' + name + '.cpp')
            path.write_text(common + (anchor if mode == 'anchored' else '') + body)
            files.append(path)
            run(mode + '-' + name + '-preprocess', [*prefix, path], output=path.with_suffix('.ii'))
        run(mode + '-native-build', ['g++', '-std=c++17', '-O2', '-nostdinc++', *native_includes, *files, '-o', out / (mode + '-native')])
        run(mode + '-native-run', [out / (mode + '-native')])
        run(mode + '-c-emit', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / (mode + '.c'), '-o', out / (mode + '.il'),
            *[p.with_suffix('.ii') for p in files]], reject='incompatible with a declaration in another' if mode == 'original' else None)
        if mode == 'anchored':
            run('anchored-c-build', ['gcc', '-std=gnu11', '-w', out / 'anchored.c', EDG / 'build/lib/libC.a', '-o', out / 'anchored-c'])
            run('anchored-c-run', [out / 'anchored-c'])
    report['checks'].append('Original/anchored native LLVM pair behavior agrees; stock C rejects original name correspondence and emits/builds/runs anchored custom nontrivial pair.')

    real = '#include "duckdb/common/types.hpp"\nusing P=std::pair<duckdb::string,duckdb::LogicalType>;\n'
    for name, body in [('copy', 'void first(P*a,const P*b){*a=*b;}'),
                       ('size', 'unsigned long second(){return sizeof(P);}'),
                       ('ctor', 'void second(void*p){new(p)P();}')]:
        path = out / ('real-' + name + '.cpp')
        path.write_text(real + body + '\n')
        run('real-' + name + '-native-syntax', ['g++', '-std=c++17', '-nostdinc++', *native_includes, *flags, '-fsyntax-only', path])
        run('real-' + name + '-preprocess', [*prefix, *flags, path], output=path.with_suffix('.ii'))
    for name in ('size', 'ctor'):
        run('real-' + name + '-semantic-merge', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--clear_flag', 'use_predefined_macro_file', '--no_code_gen', out / 'real-copy.ii', out / ('real-' + name + '.ii')])
    report['checks'].append('Actual adapted DuckDB LogicalType headers pass native syntax and stock-C semantic merge for sizeof-only/default-construction secondary TUs. This check is not full DuckDB lowering or execution.')

    real_value = ('#include "duckdb/common/types/value.hpp"\n'
                  '#include "duckdb/common/pair.hpp"\n'
                  'using P=std::pair<duckdb::string,duckdb::Value>;\n')
    value_bodies = [
        ('copy', 'void first(P*a,const P*b){*a=*b;}'),
        ('size', 'unsigned long second(){return sizeof(P);}'),
        ('ctor', 'void second(void*p){new(p)P();}'),
        ('convert', 'void second(void*p,duckdb::Value v){new(p)P(std::make_pair("key",std::move(v)));}'),
        ('emplace', 'void second(duckdb::child_list_t<duckdb::Value>&v,duckdb::Value x)'
                    '{v.emplace_back(std::make_pair("key",std::move(x)));}'),
    ]
    for name, body in value_bodies:
        path = out / ('real-value-' + name + '.cpp')
        path.write_text(real_value + body + '\n')
        run('real-value-' + name + '-native-syntax',
            ['g++', '-std=c++17', '-nostdinc++', *native_includes, *flags, '-fsyntax-only', path])
        run('real-value-' + name + '-preprocess', [*prefix, *flags, path], output=path.with_suffix('.ii'))
    for name, _ in value_bodies[1:]:
        run('real-value-' + name + '-semantic-merge',
            [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
             '--clear_flag', 'use_predefined_macro_file', '--no_code_gen',
             out / 'real-value-copy.ii', out / ('real-value-' + name + '.ii')])
    report['checks'].append('Actual adapted DuckDB Value headers pass native syntax and stock-C semantic merge for '
                            'sizeof-only/default/converting-pair/vector-emplace secondary TUs. '
                            'These checks do not claim actual-header C lowering or execution.')
    verify_staged(portable)
    report['inputs'] = [dict(path=str(p.resolve()), sha256=hashlib.sha256(p.read_bytes()).hexdigest())
                       for p in (Path(__file__), args.portable_manifest, args.compiler, args.oracle,
                                 upstream / '__utility/pair.h',
                                 Path(portable['stagedSource']) / 'src/include/duckdb/common/types.hpp',
                                 Path(portable['stagedSource']) / 'src/include/duckdb/common/types/value.hpp')]
    report['passed'] = True
    save()
    print('Pair native/C emission/execution and real-header semantic correspondence checks passed')


if __name__ == '__main__':
    main()
