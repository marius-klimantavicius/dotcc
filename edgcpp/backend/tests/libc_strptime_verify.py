"""Musl strptime parsing, bounded input handling and owner-local timezone state."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys


def verify_libc_strptime(root, tests, out, nativeaot, run, translate=True):
    out.mkdir(parents=True, exist_ok=True)
    source = out / 'strptime'; shutil.copytree(root / 'backend/runtime/musl/strptime', source)
    fixture = out / 'fixture.c'; shutil.copy2(tests / 'libc_strptime.c', fixture)
    case_source = out / 'string_case'; shutil.copytree(root / 'backend/runtime/musl/string_case', case_source)
    case_fixture = out / 'case_fixture.c'; shutil.copy2(tests / 'libc_string_case.c', case_fixture)
    manifest = json.loads((source / 'source.json').read_text())
    for name, entry in manifest['sources'].items():
        assert hashlib.sha256((source / name).read_bytes()).hexdigest() == entry['adaptedSha256']
        assert hashlib.sha256((source / 'originals' / name).read_bytes()).hexdigest() == entry['originalSha256']
    for name, digest in manifest['adapters'].items():
        assert hashlib.sha256((source / name).read_bytes()).hexdigest() == digest
    assert hashlib.sha256((source / manifest['patch']).read_bytes()).hexdigest() == manifest['patchSha256']
    replay = out / 'replay'; shutil.copytree(source / 'originals', replay)
    run(['patch', '--batch', '--fuzz=0', '-p1', '-i', source / manifest['patch']], cwd=replay)
    assert (replay / 'strptime.c').read_bytes() == (source / 'strptime.c').read_bytes()
    case_manifest = json.loads((case_source / 'source.json').read_text())
    for name, entry in case_manifest['sources'].items():
        assert hashlib.sha256((case_source / name).read_bytes()).hexdigest() == entry['adaptedSha256']
        assert hashlib.sha256((case_source / 'originals' / name).read_bytes()).hexdigest() == entry['originalSha256']
        assert hashlib.sha256((case_source / entry['patch']).read_bytes()).hexdigest() == entry['patchSha256']
    native = out / 'native.c'
    native.write_text('''#include <stdio.h>
#include <locale.h>
int __edg_locale_validate(locale_t value){return value!=0&&value!=LC_GLOBAL_LOCALE;}
int string_case_cases(void);
int strptime_cases(const char*);int strptime_extensions(void);unsigned long strptime_digest(void);
int main(void){int result=string_case_cases();if(result)return result+120;result=strptime_cases(0);if(result){fprintf(stderr,"case %d\\n",result);return result;}
printf("%lu\\n",strptime_digest());
#ifdef ADAPTED
result=strptime_extensions();if(result)fprintf(stderr,"extension %d\\n",result);
#endif
return result;}
''')
    digests = []
    for original in (True, False):
        label = 'original' if original else 'adapted'; binary = out / ('native-' + label)
        run(['gcc', '-D_GNU_SOURCE', '-D__tm_gmtoff=tm_gmtoff', '-std=c11', '-O2', '-fno-builtin',
             '-Werror=implicit-function-declaration', *(['-Dweak_alias(a,b)=', '-Dstrcasecmp_l=__strcasecmp_l', '-Dstrncasecmp_l=__strncasecmp_l'] if original else ['-DADAPTED']),
             *[case_source / ('originals' if original else '.') / name for name in case_manifest['sources']], case_fixture,
             source / ('originals/strptime.c' if original else 'strptime.c'), fixture, native, '-o', binary])
        result = run([binary]); digests.append(int(result.stdout.strip()))
    assert digests[0] == digests[1] != 0
    asan = out / 'native-asan'
    run(['gcc', '-D_GNU_SOURCE', '-D__tm_gmtoff=tm_gmtoff', '-DADAPTED', '-std=c11', '-O1', '-g',
         '-fno-builtin', '-fsanitize=address,undefined', source / 'strptime.c', fixture,
         *[case_source / name for name in case_manifest['sources']], case_fixture, native, '-o', asan])
    run([asan])
    checks = ['Native original/adapted date-field/locale-format matrix, exact digest and zero-fuzz source patch',
              'Native ASan/UBSan: bounded unknown-zone scan, integer year bounds and alternative-format modifiers']
    if not translate: return checks
    selected={c['directory'] for c in json.loads((root/'backend/runtime/musl/provider.json').read_text())['components']}
    extra_sources=([source/'strptime.c'] if 'strptime' not in selected else []) + ([case_source/name for name in case_manifest['sources']] if 'string_case' not in selected else [])
    for language in ('c', 'c++'):
        for instance in (False, True):
            directory = out / (language.replace('+', 'x') + ('-instance' if instance else '-static'))
            run([sys.executable, root / 'dotnet.py', fixture, case_fixture, *extra_sources, '--language', language,
                 '--libc', 'musl', '--emit', 'managedlib', '--namespace', '', '--class-name', 'Core',
                 '--split', 'size', '--output-dir', directory, *(['--instance-methods'] if instance else [])])
            exports = {e['symbol']: e['method'] for e in json.loads((directory / 'dot-gen.json').read_text())['exports']}
            host = directory / 'host'; host.mkdir()
            (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
            receiver = 'first' if instance else 'Core'
            isolation = ''
            if instance:
                isolation = f'''fixed(byte* zone="CCC-3DDD,M3.2.0,M11.1.0"u8)second.{exports['strptime_select']}((sbyte*)zone);
fixed(byte* name="CCC"u8)if(second.{exports['strptime_zone']}((sbyte*)name)!=0)throw new Exception("second zone");
fixed(byte* name="BBB"u8)if(first.{exports['strptime_zone']}((sbyte*)name)!=1)throw new Exception("first zone");'''
            (host / 'Program.cs').write_text(f'''using System;
unsafe class EntryPoint {{static void Main() {{
{('using var first=new Core();using var second=new Core();' if instance else '')}
int result={receiver}.{exports['string_case_cases']}();if(result!=0)throw new Exception($"string case {{result}}");
result={receiver}.{exports['strptime_cases']}(null);if(result!=0)throw new Exception($"case {{result}}");
if({receiver}.{exports['strptime_digest']}()!={digests[0]}UL)throw new Exception("digest");
result={receiver}.{exports['strptime_extensions']}();if(result!=0)throw new Exception($"extension {{result}}");
GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
{isolation}
{('' if instance else 'Core.Shutdown();')}
}}}}''')
            run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '-p:TreatWarningsAsErrors=true'])
            run(['dotnet', host / 'bin/Release/net10.0/Host.dll'])
            checks.append(directory.name + ': JIT parsing, bounded fields/inputs, timezone refresh and owner isolation')
            if nativeaot:
                run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                     '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
                run([host / 'aot/Host'])
                checks.append(directory.name + ': NativeAOT parsing and owner isolation')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--native-only', action='store_true')
    args = parser.parse_args(); out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); result = subprocess.run(command, capture_output=True, text=True, timeout=420, **kwargs)
        index = len(commands); (out / f'{index:03d}.stdout').write_text(result.stdout); (out / f'{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command': command, 'exit': result.returncode})
        (out / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if result.returncode: raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    root = Path(__file__).resolve().parents[2]
    checks = verify_libc_strptime(root, Path(__file__).resolve().parent, out, args.nativeaot, run, not args.native_only)
    (out / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} strptime checks passed')
