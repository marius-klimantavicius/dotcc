#!/usr/bin/env python3
"""Verify ICU's private operator rename with native and unchanged stock C."""
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
from portable import adapt_icu_calendar_sub, verify_staged
from preflight import frontend_flags

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
    source = Path(portable['source'])
    staged_source = Path(portable['stagedSource'])
    relative = Path('extension/icu/icu-dateadd.cpp')
    staged = out / 'adapted'
    (staged / relative).parent.mkdir(parents=True)
    shutil.copy2(source / relative, staged / relative)
    report = dict(passed=False, commands=[], checks=[], adaptation=adapt_icu_calendar_sub(source, staged))
    original = (source / relative).read_text()
    updated = (staged / relative).read_text()
    assert updated.replace('ICUCalendarIntervalSub', 'ICUCalendarSub') == original
    assert updated.count('"Unimplemented type for ICUCalendarSub"') == 1
    report['implementationAndDiagnosticUnchanged'] = True
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'), EDG_DOT_TARGET='portable-lp64-binary80-v2')

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(name, command, reject=None, output=None):
        command = list(map(str, command))
        with (output or out / (name + '.stdout')).open('wb') as stdout, (out / (name + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, env=env, stdout=stdout, stderr=stderr, timeout=300)
        report['commands'].append(dict(stage=name, command=command, exitCode=result.returncode))
        save()
        error = ' '.join((out / (name + '.stderr')).read_text().split())
        if reject:
            assert result.returncode and reject in error, name
        else:
            assert result.returncode == 0, name

    first = '''namespace duckdb {
struct ICUDateFunc {};
struct ICUCalendarSub : ICUDateFunc {
 template<class A,class B,class R> static R Operation(A a,B b) { return R(a-b); }
};
template<> long ICUCalendarSub::Operation<long,int,long>(long a,int b){return a-b;}
long interval_sub(long a,int b){return ICUCalendarSub::Operation<long,int,long>(a,b);}
}
extern int date_sub(int,int);
int main(){return duckdb::interval_sub(36,7)!=29 || date_sub(11,16)!=5;}
'''
    second = '''namespace duckdb {
struct ICUDateFunc {};
struct ICUCalendarSub : ICUDateFunc {
 static int SubtractMicrosecond(int a,int b){return b-a;}
};
}
int date_sub(int a,int b){return duckdb::ICUCalendarSub::SubtractMicrosecond(a,b);}
'''
    (out / 'second.cpp').write_text(second)
    for mode in ('original', 'adapted'):
        path = out / (mode + '-first.cpp')
        path.write_text(first if mode == 'original' else first.replace('ICUCalendarSub', 'ICUCalendarIntervalSub'))
        files = [path, out / 'second.cpp']
        run(mode + '-native-build', ['g++', '-std=c++17', '-O2', *files, '-o', out / (mode + '-native')])
        run(mode + '-native-run', [out / (mode + '-native')])
        run(mode + '-c-emit', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--gen_c_file_name', out / (mode + '.c'), '-o', out / (mode + '.il'), *files],
            reject='incompatible with a declaration in another translation unit' if mode == 'original' else None)
        if mode == 'adapted':
            run('adapted-c-build', ['gcc', '-std=gnu11', '-w', out / 'adapted.c', EDG / 'build/lib/libC.a', '-o', out / 'adapted-c'])
            run('adapted-c-run', [out / 'adapted-c'])
    report['checks'].append('Native original/adapted and adapted two-TU stock-C emission/build/run preserve both distinct helpers; original stock C rejects conflicting class definitions.')

    includes = ['-I', str(args.acquisition / 'config'), '-I', str(args.acquisition / 'libcxx-18.1.8.src/include')]
    units = {Path(unit['file']).name: unit for unit in portable['units'] if unit['file'].endswith(('/icu-dateadd.cpp', '/icu-datesub.cpp'))}
    for mode in ('original', 'adapted'):
        inputs = []
        for name in ('icu-dateadd.cpp', 'icu-datesub.cpp'):
            unit = units[name]
            flags, _ = frontend_flags(unit)
            path = staged / relative if mode == 'adapted' and name == 'icu-dateadd.cpp' else Path(unit['file'])
            # The copied implementation retains its original quoted-include lookup.
            search = ['-I', str(staged_source / 'extension/icu')]
            stage = mode + '-' + Path(name).stem
            run(stage + '-native-syntax', ['g++', '-std=c++17', '-nostdinc++', *includes, *search, *flags, '-fsyntax-only', path])
            output = out / (stage + '.ii')
            run(stage + '-preprocess', [args.compiler.resolve(), '--c++17', '--gnu_version', '130300', '--clear_flag',
                'use_predefined_macro_file', '--preprocess', *includes,
                '-I', EDG / 'backend/runtime/libcxx/include', '-I', EDG / 'backend/runtime/posix/include',
                '-I', EDG / 'backend/runtime/libc/include', '--preinclude', ROOT / 'duckdb/edg/libcxx-preinclude.h',
                *search, *flags, path], output=output)
            inputs.append(output)
        run(mode + '-real-semantic-merge', [args.oracle.resolve(), '--c++17', '--gnu_version', '130300', '--instantiate', 'used',
            '--clear_flag', 'use_predefined_macro_file', '--no_code_gen', *inputs],
            reject='incompatible with a declaration in another translation unit' if mode == 'original' else None)
    report['checks'].append('Actual ICU dateadd/datesub implementations pass native syntax; stock-C semantic merge rejects original helper collision and accepts the adapted sources. No full ICU/DuckDB execution claim.')
    collision = out / 'collision'
    collision.mkdir()
    (collision / 'collision.hpp').write_text('struct ICUCalendarIntervalSub {};\n')
    try:
        adapt_icu_calendar_sub(collision, staged)
    except ValueError as error:
        assert 'replacement name already exists' in str(error)
        report['replacementCollisionRejected'] = True
    else:
        raise AssertionError('Private replacement collision was not rejected')
    verify_staged(portable)
    report['inputs'] = [dict(path=str(p.resolve()), sha256=hashlib.sha256(p.read_bytes()).hexdigest())
        for p in (Path(__file__), Path(__file__).parents[1] / 'portable.py', args.portable_manifest, args.compiler, args.oracle,
                  source / relative, staged / relative, staged_source / 'extension/icu/icu-datesub.cpp')]
    report['passed'] = True
    save()
    print('ICU private-name audit, native/C reduced execution and actual-header correspondence checks passed')


if __name__ == '__main__':
    main()
