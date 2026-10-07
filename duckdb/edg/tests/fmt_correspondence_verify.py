#!/usr/bin/env python3
"""Verify fmt enum promotions without changing its packing or formatting algorithms."""
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
REAL_FIRST = '#include "fmt/format.h"\nstruct Visitor {\n int operator()(int value) const { return value; }\n template<class T> int operator()(const T&) const { return -999; }\n};\ntemplate<class... Args> int check(const Args&... values) {\n auto store=duckdb_fmt::make_format_args(values...);\n duckdb_fmt::format_args args(store);\n int sum=0;\n for(unsigned i=0;i<sizeof...(Args);++i) sum += duckdb_fmt::visit_format_arg(Visitor{},args.get(i));\n if(args.get(sizeof...(Args)))return -1000;\n return sum;\n}\nextern unsigned long second();\nint main() {\n if(check()!=0)return 1;\n if(check(1)!=1)return 2;\n if(check(1,2,3,4,5,6,7,8,9,10,11)!=66)return 3;\n if(check(1,2,3,4,5,6,7,8,9,10,11,12)!=78)return 4;\n if(check(1,2,3,4,5,6,7,8,9,10,11,12,13)!=91)return 5;\n auto store=duckdb_fmt::make_format_args(1);\n if(second()!=sizeof(store))return 6;\n std::printf("fmt store size: %lu\\n", (unsigned long)sizeof(store));\n return 0;\n}\n'
REAL_SECOND = '#include "fmt/format.h"\nunsigned long second() {return sizeof(duckdb_fmt::format_arg_store<duckdb_fmt::format_context,int>);}\n'
MINIMAL = '''namespace fmt {
enum { max_packed_args = 12 };
enum : unsigned long long { is_unpacked_bit = 1ULL << 63 };
template<class... Args> struct Store {
 static const unsigned long n = sizeof...(Args);
 static const bool packed = n < MAX;
 static constexpr unsigned long long types = packed ? 0 : BIT | n;
 int data[n+(n==0?1:0)];
};
template<class... Args> int check(Args...args) {
 using T=Store<Args...>;
 return T::packed == (sizeof...(Args)<12) && T::types == (sizeof...(Args)<12 ? 0 : (1ULL<<63)|sizeof...(Args));
}
}
'''
MINIMAL_FIRST = '''extern int second(); int main(){return !(fmt::check() && fmt::check(1) && fmt::check(1,2,3,4,5,6,7,8,9,10,11) && fmt::check(1,2,3,4,5,6,7,8,9,10,11,12) && fmt::check(1,2,3,4,5,6,7,8,9,10,11,12,13) && second());}
'''


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
    includes = ['-I', str(args.acquisition / 'config'), '-I', str(args.acquisition / 'libcxx-18.1.8.src/include')]
    preprocess = [str(args.compiler.resolve()), '--c++17', '--gnu_version', '130300', '--clear_flag',
                  'use_predefined_macro_file', '--preprocess', *includes,
                  '-I', str(EDG / 'backend/runtime/libcxx/include'), '-I', str(EDG / 'backend/runtime/posix/include'),
                  '-I', str(EDG / 'backend/runtime/libc/include'), '--preinclude', str(ROOT / 'duckdb/edg/libcxx-preinclude.h')]
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'), EDG_DOT_TARGET='portable-lp64-binary80-v2')
    report = dict(passed=False, commands=[], checks=[], knownLimitations=[])

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(name, command, reject=None, output=None, known_failure=None):
        command = list(map(str, command))
        with (output or out / (name + '.stdout')).open('wb') as stdout, (out / (name + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, env=env, stdout=stdout, stderr=stderr, timeout=180)
        report['commands'].append(dict(stage=name, command=command, exitCode=result.returncode))
        save()
        error = (out / (name + '.stderr')).read_text()
        if known_failure and result.returncode and known_failure in error:
            report['knownLimitations'].append(dict(stage=name, diagnostic=known_failure, exitCode=result.returncode))
        elif reject:
            assert result.returncode and reject in ' '.join(error.split()), name
        else:
            assert result.returncode == 0, name
        return result.returncode

    def c_command(name, inputs, semantic=False):
        return [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
                '--clear_flag', 'use_predefined_macro_file',
                *(['--no_code_gen'] if semantic else ['--gen_c_file_name', out / (name + '.c'), '-o', out / (name + '.il')]), *inputs]

    def c_execute(name):
        run(name + '-build', ['gcc', '-std=gnu11', '-w', '-ffunction-sections', '-fdata-sections', out / (name + '.c'),
                             EDG / 'build/lib/libC.a', '-Wl,--gc-sections', '-o', out / name])
        run(name + '-run', [out / name])

    for mode in ('original', 'adapted'):
        common = MINIMAL.replace('MAX', 'max_packed_args' if mode == 'original' else 'static_cast<int>(max_packed_args)')
        common = common.replace('BIT', 'is_unpacked_bit' if mode == 'original' else 'static_cast<unsigned long long>(is_unpacked_bit)')
        files = []
        for part, body in [('first', MINIMAL_FIRST), ('second', 'int second(){return fmt::check(1,2);}\n')]:
            path = out / ('minimal-' + mode + '-' + part + '.cpp')
            path.write_text(common + body)
            files.append(path)
        run('minimal-' + mode + '-native-build', ['g++', '-std=c++17', '-O2', *files, '-o', out / ('minimal-' + mode + '-native')])
        run('minimal-' + mode + '-native-run', [out / ('minimal-' + mode + '-native')])
        run('minimal-' + mode + '-emit', c_command('minimal-' + mode, files),
            reject='incompatible with a declaration in another' if mode == 'original' else None)
        if mode == 'adapted':
            c_execute('minimal-adapted')
    report['checks'].append('Reduced dependent enum arithmetic: original native succeeds and stock C rejects; adapted native and two-TU stock-C emission/build/run agree for0/1/11/12/13 argument cases.')

    (out / 'first.cpp').write_text(REAL_FIRST)
    (out / 'second.cpp').write_text(REAL_SECOND)
    (out / 'unity.cpp').write_text('#include "first.cpp"\n#include "second.cpp"\n')
    outputs = []
    for mode in ('original', 'adapted'):
        original_include = ['-I', str(Path(portable['source']) / 'third_party/fmt/include')] if mode == 'original' else []
        run(mode + '-native-build', ['g++', '-std=c++17', '-O2', '-nostdinc++', *includes, *original_include, *flags,
                                    out / 'first.cpp', out / 'second.cpp', '-o', out / (mode + '-native')])
        run(mode + '-native-run', [out / (mode + '-native')])
        outputs.append((out / (mode + '-native-run.stdout')).read_bytes())
        for part in ('first', 'second', 'unity'):
            run(mode + '-' + part + '-preprocess', [*preprocess, *original_include, *flags, out / (part + '.cpp')],
                output=out / (mode + '-' + part + '.ii'))
        inputs = [out / (mode + '-' + part + '.ii') for part in ('first', 'second')]
        run(mode + '-semantic-merge', c_command(mode, inputs, semantic=True),
            reject='incompatible with a declaration in another' if mode == 'original' else None)
        run(mode + '-unity-emit', c_command(mode + '-unity', [out / (mode + '-unity.ii')]))
        c_execute(mode + '-unity')
        outputs.append((out / (mode + '-unity-run.stdout')).read_bytes())
        if mode == 'adapted':
            status = run('adapted-two-tu-emit', c_command('adapted-two-tu', inputs),
                         known_failure='read_memory_region: not all expected entries were read')
            if status == 0:
                c_execute('adapted-two-tu')
                outputs.append((out / 'adapted-two-tu-run.stdout').read_bytes())
    assert all(output == outputs[0] for output in outputs), 'Runtime observations changed'
    report['checks'].append('Actual fmt: native original/adapted and single-TU stock-C emission/build/run preserve0/1/11/12/13 stored argument values, packing boundary, out-of-range behavior and observed layout. Adapted two-TU semantic merge passes; any later stock-C reader failure is separately reported.')
    verify_staged(portable)
    report['inputs'] = [dict(path=str(p.resolve()), sha256=hashlib.sha256(p.read_bytes()).hexdigest())
                       for p in (Path(__file__), args.portable_manifest, args.compiler, args.oracle,
                                 Path(portable['source']) / 'third_party/fmt/include/fmt/core.h',
                                 Path(portable['stagedSource']) / 'third_party/fmt/include/fmt/core.h')]
    report['passed'] = True
    save()
    print('fmt source adaptation verified; known lowering limitations:', len(report['knownLimitations']))


if __name__ == '__main__':
    main()
