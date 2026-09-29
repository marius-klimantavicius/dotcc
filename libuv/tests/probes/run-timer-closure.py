#!/usr/bin/env python3
"""Compare real timer storage/algorithms against native libuv on Linux glibc x64."""
import argparse
import hashlib
import json
import pathlib
import platform
import shutil
import subprocess
import sys

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--aot', action='store_true')
parser.add_argument('--postprocess', action='store_true')
parser.add_argument('--compiler', type=pathlib.Path, default=ROOT / 'libuv/build/compiler/bin/DotCC/release/dotcc.dll')
args = parser.parse_args()
if (platform.system(), platform.machine(), platform.libc_ver()[0]) != ('Linux', 'x86_64', 'glibc'):
    raise SystemExit('This ABI probe requires Linux glibc x64.')
config = json.loads((ROOT / 'libuv/config/sources.json').read_text())
ref = ROOT / 'libuv/ref' / config['product']['directory']
out = ROOT / 'libuv/artifacts/timer-closure'
out.mkdir(parents=True, exist_ok=True)
(out / 'result.json').write_text(json.dumps({'status': 'running', 'diagnostic_only': True}) + '\n')
commands = []
def run(command, capture=False):
    commands.append([str(item) for item in command])
    return subprocess.run(commands[-1], check=True, cwd=ROOT, capture_output=capture, text=True).stdout
run([sys.executable, HERE / 'materialize-native-closure.py', '--records', 'src/timer.c'])
closure = ROOT / 'libuv/artifacts/native-closures/linux-glibc-x64/src/timer'
run(['dotnet', args.compiler, closure / 'closure.c', '--emit=obj', '-o', closure / 'closure.o'])
run(['dotnet', args.compiler, closure / 'closure.o', '--emit=managedlib', '--class-name', 'TimerProbe',
     '--namespace', 'Managed.Libuv.Probes', '--nest-types', '--runtime=c', '-o', out])
project = out / 'timer-closure.csproj'
project.write_text(project.read_text().replace('<OutputType>Library</OutputType>', '<OutputType>Exe</OutputType>'))
# Exact named records selected from the native AST. Compare every direct field,
# not only those exercised by this particular timer workload.
ast = json.loads((closure / 'native.ast.json').read_text())
pairs = [('uv_loop_t', 'uv_loop_s'), ('uv_timer_t', 'uv_timer_s'), ('uv_handle_t', 'uv_handle_s'),
         ('uv_async_t', 'uv_async_s'), ('uv_signal_t', 'uv_signal_s'), ('uv__io_t', 'uv__io_s'),
         ('struct uv__queue', 'uv__queue'), ('struct heap', 'heap'), ('struct heap_node', 'heap_node'),
         ('pthread_mutex_t', 'pthread_mutex_t'), ('pthread_rwlock_t', 'pthread_rwlock_t')]
cs = ['using System;', 'using System.Runtime.InteropServices;', 'using Managed.Libuv.Probes;',
      'unsafe {', 'TimerBehavior.Run();']
c = ['#include "uv.h"', '#include "uv-common.h"', '#include "heap-inl.h"', '#include <stddef.h>',
     '#include <stdio.h>', 'void behavior(void);', 'int main(void) {', 'behavior();']
measurements = 0
for index, (native, managed) in enumerate(pairs):
    cs += [f'Console.WriteLine("{native}.size=" + sizeof(TimerProbe.{managed}));',
           f'Console.WriteLine("{native}.align=" + Alignment<TimerProbe.{managed}>.Value());',
           f'TimerProbe.{managed} value{index} = default;']
    c += [f'printf("{native}.size=%zu\\n{native}.align=%zu\\n", sizeof({native}), _Alignof({native}));']
    measurements += 2
    records = [node for node in ast['inner'] if node['kind'] == 'RecordDecl' and node.get('name') == managed and node.get('completeDefinition')]
    if not records:
        continue # pthread typedef union storage checked above; members in native record capture
    for field in records[-1].get('inner', []):
        if field['kind'] != 'FieldDecl':
            continue
        name = field['name']
        first_element = '[0]' if field.get('type', {}).get('qualType', '').endswith(']') else ''
        cs.append(f'Console.WriteLine("{native}.{name}=" + ((byte*)&value{index}.@{name}{first_element} - (byte*)&value{index}));')
        c.append(f'printf("{native}.{name}=%zu\\n", offsetof({native}, {name}));')
        measurements += 1
cs += ['}', 'return 0;', '[StructLayout(LayoutKind.Sequential)]',
       'public unsafe struct Alignment<T> where T : unmanaged { public byte Prefix; public T Item;',
       'public static long Value() { Alignment<T> value = default; return (byte*)&value.Item - (byte*)&value; } }']
c += ['return 0;', '}']
(out / 'Program.cs').write_text('\n'.join(cs) + '\n')
(out / 'native-layout.c').write_text('\n'.join(c) + '\n')
shutil.copyfile(HERE / 'TimerBehavior.cs', out / 'TimerBehavior.cs')
common = ['cc', '-std=gnu11', '-D_GNU_SOURCE', '-I' + str(ref / 'include'), '-I' + str(ref / 'src'),
          out / 'native-layout.c', HERE / 'timer-behavior.c']
run([*common, ref / 'src/timer.c', '-o', out / 'native'])
expected = run([out / 'native'], capture=True)
run([*common, closure / 'closure.c', '-o', out / 'native-snapshot'])
if run([out / 'native-snapshot'], capture=True) != expected:
    raise SystemExit('Snapshot native timer differs from original native timer.')
(out / 'native-expected.txt').write_text(expected)
results = []
def check(label, command):
    actual = run(command, capture=True)
    (out / (label + '.txt')).write_text(actual)
    if actual != expected:
        import difflib
        raise SystemExit(label + ' mismatch:\n' + ''.join(difflib.unified_diff(expected.splitlines(True), actual.splitlines(True))))
    results.append({'mode': label, 'status': 'passed'})
run(['dotnet', 'build', project, '-c', 'Release', '--verbosity', 'quiet'])
check('raw-jit', ['dotnet', out / 'bin/Release/net10.0/timer-closure.dll'])
if args.aot:
    run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot'])
    check('raw-nativeaot', [out / 'aot/timer-closure'])
if args.postprocess:
    run(['dotnet', ROOT / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll', project, '--in-place'])
    run(['dotnet', 'build', project, '-c', 'Release', '--verbosity', 'quiet'])
    check('processed-jit', ['dotnet', out / 'bin/Release/net10.0/timer-closure.dll'])
    if args.aot:
        run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', out / 'aot-processed'])
        check('processed-nativeaot', [out / 'aot-processed/timer-closure'])
(out / 'result.json').write_text(json.dumps({'status': 'passed', 'diagnostic_only': True, 'target': 'linux-glibc-x64',
    'source_revision': config['product']['revision'], 'source': 'src/timer.c', 'layout_measurements': measurements,
    'native_snapshot_matches_original': True, 'modes': results, 'commands': commands,
    'compiler_library_sha256': hashlib.sha256((args.compiler.parent / 'DotCC.Lib.dll').read_bytes()).hexdigest()}, indent=2) + '\n')
