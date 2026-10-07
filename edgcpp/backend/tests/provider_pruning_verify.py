#!/usr/bin/env python3
"""Provider-only reachability, with public roots, callback tables and lifecycle."""
import argparse
import hashlib
import json
from pathlib import Path
import shlex
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'passed': False, 'compiler': str(args.compiler.resolve()),
              'compilerSha256': hashlib.sha256(args.compiler.read_bytes()).hexdigest(),
              'commands': [], 'checks': [], 'modes': []}

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command):
        command = list(map(str, command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=900)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        report['commands'].append({'stage': label, 'command': command, 'exitCode': result.returncode})
        save()
        if result.returncode:
            raise RuntimeError(f'{label} failed: {out / (label + ".stderr")}')
        return result.stdout

    for language in ('c', 'c++'):
        suffix = 'c' if language == 'c' else 'cpp'
        base = out / suffix
        base.mkdir()
        header = base / 'api.h'
        header.write_text('''struct Callback { int (*call)(int); };
extern struct Callback provider_callbacks[];
extern int startup_count;
extern int destruction_count;
int provider_direct(int);
int provider_tls_value(void);
int provider_local_callback(void);
int provider_inline_dependency(int);
#ifdef __cplusplus
extern "C"
#endif
double sqrt(double);
static inline int public_header_inline(int value) { return provider_inline_dependency(value); }
''')
        provider = base / ('provider.' + suffix)
        provider.write_text('''#include "api.h"
int startup_count = 0;
int destruction_count = 0;
int provider_direct(int x) { return x + 10; }
#ifdef __cplusplus
extern "C"
#endif
double sqrt(double x) { return x + 50; }
int provider_callback(int x) { return x + 20; }
struct Callback provider_callbacks[] = {{ &provider_callback }};
__thread int provider_tls = 7;
int provider_tls_value(void) { return provider_tls++; }
int provider_inline_dependency(int x) { return x + 30; }
int provider_local_only(int x) { return x + 40; }
int provider_local_callback(void) {
    static struct Callback local[] = {{ &provider_local_only }};
    return local[0].call(3);
}
int unused_provider(int x) { return x + 1000; }
struct Callback unused_callbacks[] = {{ &unused_provider }};
int unused_storage[512] = {91};
__attribute__((constructor)) static void provider_startup(void) { startup_count += 5; }
''' + ('''
struct Guard { ~Guard() { ++destruction_count; } };
Guard provider_guard;
int provider_guard_cycle(void) { { Guard local; } return destruction_count; }
''' if language == 'c++' else ''))
        project = base / ('project.' + suffix)
        project.write_text('''#include "api.h"
int public_api(void) {
    return provider_direct(1) + provider_callbacks[0].call(2)
        + provider_tls_value() + provider_local_callback() + startup_count;
}
int public_uncalled_api(void) { return public_header_inline(9); }
int public_destroyed(void) { return destruction_count; }
int public_builtin(double x) { return __builtin_sqrt(x)==54; }
#ifdef __cplusplus
int provider_guard_cycle(void);
int public_cleanup(void) { return provider_guard_cycle(); }
#endif
#ifdef NATIVE_MAIN
int main(void) {
    if (public_api()!=88) return 71;
    if (public_api()!=89) return 72;
    if (public_uncalled_api()!=39) return 73;
    if (!public_builtin(4)) return 75;
#ifdef __cplusplus
    if (public_cleanup()!=1) return 76;
#endif
    return 0;
}
#endif
''')
        native = base / 'native'
        run(suffix + '-native-build', ['gcc' if language == 'c' else 'g++',
            '-std=c11' if language == 'c' else '-std=c++17', '-DNATIVE_MAIN', project, provider, '-o', native])
        run(suffix + '-native', [native])
        sources = base / 'prunable-sources.txt'
        sources.write_text(str(provider) + '\n')
        # Exercise the backend protocol independently of musl snapshot assembly.
        # The public driver deliberately only accepts provider pruning for musl.
        wrapper = base / 'compiler-wrapper'
        wrapper.write_text('#!/bin/sh\nexport EDG_DOT_PRUNABLE_SOURCES=' + shlex.quote(str(sources)) + '\nexec ' +
                           shlex.quote(str(args.compiler.resolve())) + ' "$@"\n')
        wrapper.chmod(0o755)
        all_generated = out / (suffix + '-all')
        run(suffix + '-all-translate', [sys.executable, ROOT / 'dotnet.py', project, provider,
            '--compiler', args.compiler.resolve(), '--language', language, '--libc', 'dotcc',
            '--emit', 'managedlib', '--namespace', 'ProviderReachability', '--class-name', 'Core',
            '--output-dir', all_generated])
        all_manifest = json.loads((all_generated / 'dot-gen.json').read_text())
        assert all_manifest['providerPruning'] is None
        assert 'unused_provider' in {row['method'] for row in all_manifest['exports']}
        report['checks'].append(suffix + ': default/all preserves provider exports')
        for instance in (False, True):
            mode = suffix + ('-instance' if instance else '-static')
            generated = out / mode
            raw = out / (mode + '.raw.cs')
            run(mode + '-translate', [sys.executable, ROOT / 'dotnet.py', project, provider,
                '--compiler', wrapper, '--language', language, '--libc', 'dotcc',
                '--emit', 'managedlib', '--namespace', 'ProviderReachability', '--class-name', 'Core',
                '--output-dir', generated, '--raw-output', raw, '--split', 'source',
                *(['--instance-methods'] if instance else [])])
            manifest = json.loads((generated / 'dot-gen.json').read_text())
            exports = {row['method'] for row in manifest['exports']}
            for name in ('public_api', 'public_uncalled_api', 'provider_direct', 'provider_callback',
                         'provider_inline_dependency', 'provider_local_only', 'provider_startup', 'sqrt'):
                assert name in exports, (mode, name, exports)
            assert 'unused_provider' not in exports
            source_rows = json.loads((generated / manifest['sourceMap']['file']).read_text())['symbols']
            assert not any(row['sourceName'] in ('unused_provider', 'unused_callbacks', 'unused_storage')
                           for row in source_rows)
            pruning = manifest['providerPruning']
            assert pruning['candidateRoutines'] > pruning['retainedRoutines']
            assert pruning['candidateVariables'] > pruning['retainedVariables']
            report['modes'].append({'mode': mode, 'providerPruning': pruning})
            report['checks'].append(mode + ': removes unreferenced provider function, callback table and global storage; preserves uncalled public API')
            host = out / (mode + '-host')
            host.mkdir()
            (host / 'Host.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible><TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup><ItemGroup><ProjectReference Include="../{mode}/Generated.csproj" /></ItemGroup></Project>''')
            receiver = 'owner' if instance else 'Core'
            setup = 'using var owner = new Core();' if instance else ''
            cleanup = f'if({receiver}.public_cleanup()!=1) return 76;' if language == 'c++' else ''
            second = 'using var other = new Core(); if(other.public_api()!=88) return 74;' if instance else ''
            (host / 'Program.cs').write_text(f'''using System; using ProviderReachability;
class Entry {{ static int Main() {{
{setup}
if({receiver}.public_api()!=88) return 71;
GC.Collect(2, GCCollectionMode.Forced, true, true);
if({receiver}.public_api()!=89) return 72;
if({receiver}.public_uncalled_api()!=39) return 73;
if({receiver}.public_builtin(4)!=1) return 75;
{cleanup}
{second}
return 0;
}} }}''')
            run(mode + '-build', ['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
            run(mode + '-jit', ['dotnet', host / 'bin/Release/net10.0/Host.dll'])
            report['checks'].append(mode + '/JIT: direct calls, callback globals, static local callback, TLS, startup and compacting GC')
            if args.nativeaot:
                run(mode + '-publish', ['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
                run(mode + '-aot', [host / 'aot/Host'])
                report['checks'].append(mode + '/NativeAOT: same lifecycle and dependency checks')
    report['passed'] = True
    save()
    print(f"{len(report['checks'])} provider pruning checks passed")


if __name__ == '__main__':
    main()
