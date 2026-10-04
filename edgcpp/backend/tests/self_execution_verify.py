#!/usr/bin/env python3
"""Execute the translated EDG compiler against its native compiler oracle.

Only run-directory paths, line endings, and the C backend's dated banner are
normalized. Compiler diagnostics and preprocessor token output remain exact.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[2]

CPP_PROGRAM = r'''extern "C" int printf(const char*, ...);
static int trace;
struct Guard {
  int digit;
  Guard(int n):digit(n){ trace=trace*10+n; }
  ~Guard(){ trace=trace*10+digit; }
};
struct Base { virtual int value() const {return 2;} virtual ~Base(){} };
struct Child:Base { int value()const override{return 7;} };
template<class T> concept Addable = requires(T a){a+a;};
template<Addable T> T add(T a,T b){return a+b;}
int main(){
  Child child; Base* base=&child;
  try { Guard first(1); Guard second(2); throw 9; }
  catch(int value){trace=trace*10+value;}
  printf("%d %d %d\n",trace,base->value(),add(3,4));
  return trace==12219 && base->value()==7 && add(3,4)==7 ? 0 : 1;
}
'''
C_PROGRAM = r'''#include "local-api.h"
extern int printf(const char*, ...);
struct Record { unsigned flag:3; int value; };
static struct Record rows[] = {{.flag=1,.value=API_CONSTANT},{.flag=2,.value=7}};
_Static_assert(sizeof(long)>=4,"long width");
int main(void){
  int total=0;
  for(int i=0;i<2;++i){ if(rows[i].flag) total+=rows[i].value; }
  printf("%d %d\n",total,_Generic(total,int:1,default:0));
  return total==48?0:1;
}
'''


def cases():
    # Keep the extension probes aligned with edgcpp/verify.py. They exercise
    # the compiled gates in the running compiler, not in its translation host.
    return [
        ('c11-parse', ['--c11'], C_PROGRAM, 0),
        ('c11-preprocess', ['--c11', '-E'], '#include "local-api.h"\nAPI_CONSTANT API_JOIN(public_,name)\n', 0),
        ('syntax-error', ['--c++20'], 'int broken() { return missing_name; }\n', 'error'),
        ('cpp20-language', ['--c++20'], CPP_PROGRAM, 0),
        ('cpp26-reflection', ['--c++26', '--set_flag', 'reflection'], 'using T = [:^^int:]; static_assert(sizeof(T) == sizeof(int));\n', 0),
        ('gnu', ['--c++20', '--gnu_version=130300'], 'typedef int V __attribute__((vector_size(16))); __int128 n; __float128 f; int main() { return ({int x=1; x;})-1; }\n', 0),
        ('float16-bfloat16', ['--c++20', '--gnu_version=130300'], 'constexpr _Float16 half = (_Float16)1.5f;\nconstexpr __bf16 brain = (__bf16)1.5f;\nstatic_assert(sizeof(half)==2 && sizeof(brain)==2, "16-bit formats");\nstatic_assert((float)(half+(_Float16)0.5f)==2.0f, "half arithmetic");\nstatic_assert((float)(brain+(__bf16)0.5f)==2.0f, "bfloat arithmetic");\n_Float16 half_add(_Float16 x){return x+(_Float16)0.5f;}\n__bf16 brain_add(__bf16 x){return x+(__bf16)0.5f;}\n', 0),
        ('clang', ['--c++20', '--clang', '--clang_version=180000'], 'int main() { return __has_builtin(__builtin_trap) ? 0 : 1; }\n', 0),
        ('microsoft', ['--microsoft', '--microsoft_version=1930'], '__declspec(align(16)) struct S { __int64 n; };\n', 0),
        ('sun', ['--sun', '--sun_linker_scope'], '__global int value;\n', 0),
        ('upc', ['--c', '--upc'], 'shared int value;\n', 0),
        ('embedded-c', ['--c', '--embedded_c'], '_Accum value = 0.5k;\n', 0),
        ('vla', ['--c99', '--vla'], 'int f(int n) { int a[n]; a[0]=1; return a[0]; }\n', 0),
        ('cppcli', ['--microsoft', '--cppcli'], 'ref class R { public: int n; };\n', 'missing-metadata:mscorlib.dll'),
        ('cppcx', ['--microsoft', '--cppcx'], 'ref class R sealed { public: int n; };\n', 'missing-metadata:platform.winmd'),
        ('coroutine', ['--c++20', '-I' + str(ROOT / 'upstream/include_c++')], '#include <coroutine>\nstruct Task { struct promise_type { Task get_return_object() { return {}; } std::suspend_never initial_suspend() { return {}; } std::suspend_never final_suspend() noexcept { return {}; } void return_void() {} void unhandled_exception() {} }; }; Task f() { co_return; }\n', 0),
    ]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--native-compiler', type=Path, default=ROOT / 'build/bin/cpfe')
    parser.add_argument('--managed-command', nargs='+', help='dotnet ABSOLUTE_DLL or ABSOLUTE_AOT_BINARY')
    parser.add_argument('--native-only', action='store_true', help='Establish fixture and lowered-C native oracles only')
    parser.add_argument('--timeout', type=int, default=180)
    parser.add_argument('--case', action='append', help='Run only named cases (repeatable), useful for a bounded failure reduction')
    args = parser.parse_args()
    if not args.native_only and not args.managed_command:
        parser.error('--managed-command is required unless --native-only is selected')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'; inputs.mkdir()
    (inputs / 'local-api.h').write_text('#define API_CONSTANT 41\n#define API_JOIN(a,b) a ## b\n')
    report = {'scope': 'Running compiler frontend and C backend parity',
              'nativeOnly': args.native_only, 'nativeCompiler': str(args.native_compiler.resolve()),
              'nativeCompilerSha256': hashlib.sha256(args.native_compiler.read_bytes()).hexdigest(),
              'managedCommand': args.managed_command, 'cases': [], 'commands': [],
              'normalization': ['CRLF to LF', 'per-run directory paths', 'dated C backend banner only']}
    commands = {'native': [str(args.native_compiler.resolve())]}
    if not args.native_only:
        commands['managed'] = args.managed_command
    base = ['--clear_flag', 'use_predefined_macro_file']

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, cwd):
        command = list(map(str, command)); start = time.monotonic()
        try:
            process = subprocess.run(command, cwd=cwd, capture_output=True, text=True, timeout=args.timeout)
        except subprocess.TimeoutExpired as error:
            process = subprocess.CompletedProcess(command, -999,
                (error.stdout or b'').decode(errors='replace') if isinstance(error.stdout, bytes) else error.stdout or '',
                (error.stderr or b'').decode(errors='replace') if isinstance(error.stderr, bytes) else error.stderr or '')
        (cwd / (label + '.stdout')).write_text(process.stdout)
        (cwd / (label + '.stderr')).write_text(process.stderr)
        report['commands'].append({'label': label, 'command': command, 'cwd': str(cwd),
            'exitCode': process.returncode, 'seconds': round(time.monotonic() - start, 3)})
        save()
        return process

    def normalized(value, run_dir):
        return value.replace('\r\n', '\n').replace(str(run_dir), '<run>')

    def compare(native, managed, native_dir, managed_dir):
        return native.returncode == managed.returncode and all(
            normalized(getattr(native, stream), native_dir) == normalized(getattr(managed, stream), managed_dir)
            for stream in ('stdout', 'stderr'))

    for name, flags, source, expected in cases():
        if args.case and name not in args.case:
            continue
        path = inputs / (name + '.cpp'); path.write_text(source)
        entry = {'name': name, 'kind': 'frontend', 'expected': expected, 'passed': False}
        report['cases'].append(entry)
        results = {}; directories = {}
        for kind, command in commands.items():
            directory = out / name / kind; directory.mkdir(parents=True)
            directories[kind] = directory
            results[kind] = run('compile', [*command, *base, '--no_code_gen', *flags, path], directory)
        native = results['native']
        oracle_ok = native.returncode == expected if isinstance(expected, int) else native.returncode > 0 and 'error' in native.stderr.lower()
        if isinstance(expected, str) and expected.startswith('missing-metadata:'):
            oracle_ok = native.returncode > 0 and ('cannot open metadata file "' + expected.split(':', 1)[1] + '"') in native.stderr
            entry['scope'] = 'Mode selection and missing platform metadata diagnostic; not CLI/CX program acceptance'
        entry['nativeOraclePassed'] = oracle_ok
        entry['passed'] = oracle_ok and ('managed' not in results or compare(native, results['managed'], directories['native'], directories['managed']))
        save(); print(f"{name}: {'passed' if entry['passed'] else 'FAILED'}", flush=True)

    for name, flags, source, expected_stdout in [
        ('lower-c11', ['--c11'], C_PROGRAM, '48 1\n'),
        ('lower-cpp20', ['--c++20', '--gnu_version', '130300', '--instantiate', 'used'], CPP_PROGRAM, '12219 7 7\n'),
    ]:
        if args.case and name not in args.case:
            continue
        path = inputs / (name + '.cpp'); path.write_text(source)
        entry = {'name': name, 'kind': 'lower-compile-execute', 'passed': True}
        report['cases'].append(entry)
        results = {}; directories = {}; lowered_text = {}
        for kind, command in commands.items():
            directory = out / name / kind; directory.mkdir(parents=True)
            directories[kind] = directory
            lowered = directory / 'generated.c'
            result = run('lower', [*command, *base, *flags, '--gen_c_file_name', lowered,
                        '-o', directory / 'lowered.il', path], directory)
            results[kind] = result
            if result.returncode or not lowered.is_file():
                entry['passed'] = False
                continue
            text = normalized(lowered.read_text(), directory)
            # EDG emits ctime only as its second banner line. Strip just this
            # documented incidental line; retain source names and all C code.
            text = re.sub(r'(?m)^/\* (?:Mon|Tue|Wed|Thu|Fri|Sat|Sun) [A-Z][a-z]{2} +\d{1,2} \d\d:\d\d:\d\d \d{4} \*/\n', '', text, count=1)
            lowered_text[kind] = text
            entry[kind + 'LoweredSha256'] = hashlib.sha256(text.encode()).hexdigest()
            build = run('c-build', ['gcc', lowered, ROOT / 'build/lib/libC.a', '-lstdc++', '-lpthread', '-lm', '-o', directory / 'program'], directory)
            if build.returncode:
                entry['passed'] = False
                continue
            executed = run('c-run', [directory / 'program'], directory)
            if executed.returncode or executed.stdout != expected_stdout or executed.stderr:
                entry['passed'] = False
        if 'managed' in results:
            entry['compilerOutputParity'] = compare(results['native'], results['managed'], directories['native'], directories['managed'])
            entry['loweredSourceParity'] = lowered_text.get('native') == lowered_text.get('managed') and bool(lowered_text)
            # c_gen_be.c:dump_temp_name calls il_to_str.c's
            # form_temporary_name_for_back_end, which derives __T names from
            # IL addresses. Even two native processes differ. Record source
            # equality as information; require diagnostics and executed
            # program parity without erasing identifiers or rewriting code.
            entry['loweredSourceParityRequired'] = False
            entry['passed'] &= entry['compilerOutputParity']
        save(); print(f"{name}: {'passed' if entry['passed'] else 'FAILED'}", flush=True)
    if not report['cases']:
        parser.error('No matching cases')
    report['passed'] = all(case['passed'] for case in report['cases'])
    save()
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
