#!/usr/bin/env python3
"""Compare cross-TU storage, function-pointer fields and TLS against native C.

The native oracle uses pthreads; this acceptance runner targets Unix hosts.
Generated code uses BCL threads and can additionally be verified with NativeAOT.
"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]
SOURCES = {
    'shared.h': '''struct Item { int value; struct Item *next; int (*apply)(int); };
extern struct Item first, second;
extern int numbers[3];
extern int *interior;
int tick(void);
int thread_step(int amount);
int check_storage(void);
''',
    'provider.c': '''#include "shared.h"
static int add_three(int value) { return value + 3; }
static int subtract_two(int value) { return value - 2; }
struct Item first = { 3, &second, add_three };
struct Item second = { 7, &first, subtract_two };
int numbers[3] = { 1, 2, 3 };
int *interior = numbers + 2;
static int calls;
int tick(void) { return ++calls; }
__thread int thread_value = 23;
__thread int *thread_root = numbers + 1;
int thread_step(int amount) { thread_value += amount; return thread_value + *thread_root; }
''',
    'consumer.c': '''#include "shared.h"
int check_storage(void) {
 if (first.next != &second || second.next != &first) return 1;
 if (first.apply(5) != 8 || second.apply(5) != 3) return 2;
 if (interior != numbers + 2 || *interior != 3) return 3;
 if (tick() != 1 || tick() != 2) return 4;
 first.next->value = 19;
 if (second.value != 19) return 5;
 return 0;
}
''',
    'native_harness.c': '''#include "shared.h"
#include <pthread.h>
#include <stdio.h>
static void *worker(void *opaque) {
 int *result = opaque;
 *result = thread_step(10) == 35 && thread_step(1) == 36;
 return 0;
}
int main(void) {
 if (check_storage()) return 1;
 if (thread_step(0) != 25) return 2;
 int one=0, two=0;
 pthread_t a,b;
 if (pthread_create(&a,0,worker,&one) || pthread_create(&b,0,worker,&two)) return 3;
 pthread_join(a,0); pthread_join(b,0);
 if (!one || !two || thread_step(0)!=25) return 4;
 puts("storage cross-unit + thread isolation OK");
 return 0;
}
''',
}
MANAGED = '''using CrossUnitSuite;
if (Core.check_storage() != 0) return 1;
if (Core.thread_step(0) != 25) return 2;
bool one=false, two=false;
var a=new Thread(() => one=Core.thread_step(10)==35 && Core.thread_step(1)==36);
var b=new Thread(() => two=Core.thread_step(10)==35 && Core.thread_step(1)==36);
a.Start();b.Start();a.Join();b.Join();
if (!one || !two || Core.thread_step(0)!=25) return 3;
GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
if (Core.thread_step(0)!=25) return 4;
Console.WriteLine("storage cross-unit + thread isolation OK");
return 0;
'''


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    for name, data in SOURCES.items():
        (inputs / name).write_text(data)
    report = dict(status='running', checks=[], commands=[], compilerSha256=sha(args.compiler), linkerSha256=sha(args.linker),
                  sources={name: sha(inputs / name) for name in SOURCES}, nativeAotVerified=False)

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        report['checks'].append(name)

    def run(label, command, timeout=600):
        command = list(map(str, command))
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=timeout)
        report['commands'].append(dict(label=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == 0)
        return (out / (label + '.stdout')).read_bytes()

    try:
        run('native-build', ['gcc', '-std=gnu11', '-O2', '-pthread', inputs / 'provider.c', inputs / 'consumer.c',
                             inputs / 'native_harness.c', '-o', out / 'native'])
        expected = run('native-run', [out / 'native'])
        manifest = dict(schema=1, project='cross-unit-storage', units=[dict(id=name, source=str(inputs / name),
                        standard='c11', arguments=[], role='library') for name in ('provider.c', 'consumer.c')])
        (out / 'project.json').write_text(json.dumps(manifest, indent=2) + '\n')
        run('compile-objects', [sys.executable, '-B', ROOT / 'project.py', 'build', '--manifest', out / 'project.json',
                               '--output-dir', out / 'objects', '--compiler', args.compiler.resolve()])
        objects = json.loads((out / 'objects/objects.json').read_text())
        report['objects'] = {path: sha(path) for path in objects}

        def link(label, directory):
            run(label, ['dotnet', args.linker.resolve(), 'link', '--out', directory,
                        '--namespace', 'CrossUnitSuite', '--class', 'Core', *objects])

        link('link', out / 'generated')
        inputs.rename(out / 'inputs-preserved')
        link('link-without-original-source', out / 'relinked')
        first = {p.name: sha(p) for p in (out / 'generated').glob('*.cs')}
        second = {p.name: sha(p) for p in (out / 'relinked').glob('*.cs')}
        check('identical C# without source files', first == second)
        report['generatedCSharp'] = first
        runner = out / 'runner'
        runner.mkdir()
        (runner / 'Program.cs').write_text(MANAGED)
        (runner / 'Runner.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>'
            '<ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'
            '<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="' +
            escape(str(out / 'relinked/Translated.csproj'), {'"': '&quot;'}) + '" /></ItemGroup></Project>\n')
        run('managed-build', ['dotnet', 'build', runner / 'Runner.csproj', '-c', 'Release', '--nologo'])
        actual = run('managed-run', ['dotnet', runner / 'bin/Release/net10.0/Runner.dll'])
        check('JIT/native output equality', actual == expected)
        if args.nativeaot:
            run('aot-publish', ['dotnet', 'publish', runner / 'Runner.csproj', '-c', 'Release', '-r', args.rid,
                                '-p:PublishAot=true', '-o', out / 'aot', '--nologo'])
            actual = run('aot-run', [out / 'aot/Runner'])
            check('NativeAOT/native output equality', actual == expected)
            report['nativeAotVerified'] = True
        report['status'] = 'passed'
    finally:
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    print(str(len(report['checks'])) + ' cross-unit storage checks passed')


if __name__ == '__main__':
    main()
