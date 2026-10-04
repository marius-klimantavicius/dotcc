#!/usr/bin/env python3
"""Execute chunked global/TLS tables against a native oracle, including startup order."""
import argparse
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]


def fixture():
    def rows(count):
        return ',\n'.join('{%d,"row-%d",targets + %d}' % (i * 7 + 3, i, i % 4) for i in range(count))
    return '''
struct Row { unsigned value; const char *name; int *target; };
int targets[] = {11, 23, 47, 89};
Row rows[] = {''' + rows(2048) + '''};
struct Nested { int marker; Row rows[1024]; };
Nested nested = {19, {''' + rows(1024) + '''}};
thread_local Row tls_rows[] = {''' + rows(1024) + '''};
unsigned sum_rows(const Row *p, int n) {
  unsigned result = 0;
  for (int i=0; i<n; ++i) result += p[i].value + *p[i].target + p[i].name[4];
  return result;
}
extern "C" unsigned large_sum() { return sum_rows(rows,2048) + sum_rows(nested.rows,1024) + nested.marker; }
unsigned startup;
struct Startup { Startup() { startup = large_sum(); } } startup_object;
extern "C" unsigned large_startup() { return startup; }
extern "C" unsigned large_tls_sum() { return sum_rows(tls_rows,1024); }
extern "C" void large_mutate() { rows[1023].value += 17; nested.rows[1023].value += 29; }
extern "C" void large_tls_mutate() { tls_rows[511].value += 31; }
'''


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    commands = []

    def run(label, cmd):
        cmd = list(map(str, cmd))
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=900)
        (out / (label+'.stdout')).write_text(result.stdout)
        (out / (label+'.stderr')).write_text(result.stderr)
        commands.append({'label': label, 'command': cmd, 'exitCode': result.returncode})
        (out/'commands.json').write_text(json.dumps(commands, indent=2)+'\n')
        if result.returncode:
            raise RuntimeError(f'{label} failed: see {out}')
        return result.stdout

    source = out/'large.cpp'
    source.write_text(fixture())
    oracle = out/'oracle.cpp'
    oracle.write_text('#include <cstdio>\n#include <thread>\n#include "large.cpp"\n' + '''
int main() {
  std::printf("%u,%u,%u\\n",large_sum(),large_startup(),large_tls_sum());
  large_mutate(); large_tls_mutate();
  std::printf("%u,%u,%u\\n",large_sum(),large_startup(),large_tls_sum());
  unsigned other=0; std::thread t([&]{other=large_tls_sum();large_tls_mutate();});t.join();
  std::printf("%u,%u\\n",other,large_tls_sum());
}
''')
    run('native-build',['g++','-std=c++17','-O2','-pthread',oracle,'-o',out/'oracle'])
    expected = run('native-run',[out/'oracle'])
    initial, startup, initial_tls = map(int,expected.splitlines()[0].split(','))
    assert initial == startup
    checks=[]
    for instance in (False,True):
        mode='instance' if instance else 'static'
        generated=out/mode
        run(mode+'-translate',[sys.executable,ROOT/'dotnet.py',source,'--compiler',args.compiler.resolve(),
            '--std','c++17','--emit','managedlib','--namespace','Large','--class-name','Core',
            '--split','function','--output-dir',generated,*(['--instance-methods'] if instance else [])])
        text='\n'.join(p.read_text() for p in generated.glob('Translated*.cs'))
        count=len(re.findall(r'void __edg_initialize_storage_\d+\(',text))
        assert count > 20, count
        assert re.search(r'__edg_initialize_storage_\d+\([^)]*\bRow\s*\*',text)
        assert re.search(r'__edg_initialize_storage_\d+\([^)]*\bbyte\s*\*\s*p',text)
        # Readability remains source-oriented: tables still use typed row initializers.
        assert re.search(r'rows\[\d+\]\s*=\s*new\s*\(\s*\)',text)
        host=out/('host-'+mode);host.mkdir()
        (host/'Host.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible><Nullable>enable</Nullable></PropertyGroup><ItemGroup>
<ProjectReference Include="../{mode}/Generated.csproj" /></ItemGroup></Project>''')
        receiver='owner' if instance else 'Core'
        owner='using var owner = new Core();' if instance else ''
        isolation=f'using var other = new Core(); if(other.large_sum()!={initial}U || other.large_startup()!={startup}U || other.large_tls_sum()!={initial_tls}U) return 9;' if instance else ''
        (host/'Program.cs').write_text('''using System; using System.Threading; using Large;
unsafe class Entry { static int Main() {
''' + owner + f'''
Console.WriteLine($"{{{receiver}.large_sum()}},{{{receiver}.large_startup()}},{{{receiver}.large_tls_sum()}}");
{receiver}.large_mutate(); {receiver}.large_tls_mutate();
GC.Collect(2,GCCollectionMode.Forced,true,true);
Console.WriteLine($"{{{receiver}.large_sum()}},{{{receiver}.large_startup()}},{{{receiver}.large_tls_sum()}}");
uint fresh=0; var thread=new Thread(()=>{{fresh={receiver}.large_tls_sum();{receiver}.large_tls_mutate();{receiver}.ReleaseThread();}});
thread.Start();thread.Join();
Console.WriteLine($"{{fresh}},{{{receiver}.large_tls_sum()}}");
{isolation}
{receiver}.ReleaseThread();
return 0;
}}}}
''')
        run(mode+'-build',['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true','-p:UseSharedCompilation=false'])
        assert run(mode+'-jit',['dotnet',host/'bin/Release/net10.0/Host.dll']) == expected
        checks.append(mode+'/JIT: chunked nested tables, relocations, startup ordering, GC and independent TLS/owners')
        if args.nativeaot:
            run(mode+'-aot-build',['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true','-p:DebugType=None','-o',host/'aot'])
            assert run(mode+'-aot',[host/'aot/Host']) == expected
            checks.append(mode+'/NativeAOT: matches native trace')
    results={'status':'passed','checks':checks,'expected':expected}
    (out/'results.json').write_text(json.dumps(results,indent=2)+'\n')
    print(json.dumps(results,indent=2))

if __name__=='__main__': main()
