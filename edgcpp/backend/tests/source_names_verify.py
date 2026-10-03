"""Source symbol identity, C# keyword/collision domains and arena-backed views."""
import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
from pathlib import Path


def verify_source_names(root, tests, out, nativeaot, run):
    checks=[]
    source=tests/'source_names.c'
    for language,standard,compiler in [('c','c17','gcc'),('c++','c++20','g++')]:
        native=out/(language.replace('+','p')+'-native')
        run([compiler,'-x',language,'-std='+standard,'-O2',source,'-o',native]);run([native])
        oracle=out/(language.replace('+','p')+'-oracle.c')
        run([root/'build/bin/cpfe','--c17' if language=='c' else '--c++20','--gnu_version','130300',
             '--gen_c_file_name',oracle,'-o',oracle.with_suffix('.il'),*(['--instantiate','used'] if language=='c++' else []),source],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        executable=oracle.with_suffix('')
        run(['gcc','-std=gnu11','-w',oracle,root/'build/lib/libC.a','-lm','-o',executable]);run([executable])
        checks.append(language+': native source and original EDG C symbol/lifetime oracles')
        # Pair no-nest/static and nested/instance to exercise both type domains.
        for instance in (False,True):
            mode=language.replace('+','p')+('-instance' if instance else '-static')
            directory=out/mode
            run([sys.executable,root/'dotnet.py',source,'--language',language,'--std',standard,
                 '--libc','dotcc','--output-dir',directory,'--namespace','Source.Identity','--class-name','Core',
                 '--split','size','--split-size','3500',
                 *(['--instance-methods'] if instance else ['--no-nest-types'])])
            text='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
            for token in ['contribution','keyword_names','@params','@out','@lock','@event','ModeSkip','ModeAdd','adjustment','current','echo_handle']:
                assert token in text,(mode,token)
            assert 'source_f123' in text and not re.search(r'private (?:static )?int f123\(',text)
            assert 'ref int adjustment' in text and 'ref int current' in text
            assert 'case ' in text and '.ModeAdd' in text
            manifest=json.loads((directory/'dot-gen.json').read_text())
            records=json.loads((directory/manifest['sourceMap']['file']).read_text())['symbols']
            assert records,(mode,'source metadata missing')
            named={row['sourceName']:row['managedName'] for row in records}
            assert named['params']=='@params' and named['out']=='@out'
            assert named['_']!='_' and named['_'].startswith('source_')
            assert named['contribution']=='contribution' and named['f123']=='source_f123'
            assert any(row['kind']=='label' and row['managedName']=='@event' for row in records)
            assert any(row['kind']=='local' and row['sourceLine']>0 and row['scope'] for row in records)
            if language=='c++':
                assert 'Left_Item' in text and 'Right_Item' in text
                assert 'Left_overload' in text and 'Right_overload' in text and 'Left_twice' in text
            run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true','-p:Nullable=enable'])
            run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
            checks.append(mode+': JIT source names, keywords, tag/function/owner collisions, shadowed locals, TLS/global addresses and enum cases')
            if nativeaot:
                run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                     '-p:IlcTreatWarningsAsErrors=true','-p:Nullable=enable','-p:DebugType=None','-p:DebugSymbols=false','-o',directory/'aot'])
                run([directory/'aot/Generated']);checks.append(mode+': NativeAOT source correspondence semantics')
    for instance in (False,True):
        directory=out/('library-instance' if instance else 'library-static')
        run([sys.executable,root/'dotnet.py',source,'--language','c','--std','c17','--emit','managedlib',
             '--libc','dotcc','--output-dir',directory,'--namespace','Source.Identity','--class-name','Core',
             *(['--instance-methods'] if instance else [])])
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
        receiver='core' if instance else 'Core'
        setup='using var core=new Core();' if instance else ''
        # Deliberately use direct source APIs: no manifest/reflection lookup.
        body=f"using Source.Identity; unsafe class EntryPoint {{static int Main(){{{setup} Core.handle* handle=null;return {receiver}.source_names_check()==0 && {receiver}.Run__export(3)==4 && {receiver}.Shutdown__export(3)==5 && {receiver}.Core__export(3)==6 && {receiver}.keyword_names(2,5)==7 && {receiver}.source_f123(3)==7 && {receiver}.echo_handle(handle)==null?0:91;}}}}"
        (host/'Program.cs').write_text(body)
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll'])
        checks.append(directory.name+': direct source API/opaque type and deterministic lifecycle/class-name collision wrappers')
        if nativeaot:
            run(['dotnet','publish',host/'Host.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                 '-p:IlcTreatWarningsAsErrors=true','-p:DebugType=None','-p:DebugSymbols=false','-o',host/'aot'])
            run([host/'aot/Host']);checks.append(directory.name+': NativeAOT direct source API')
    (out/'input-hashes.json').write_text(json.dumps({str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in
        [source,root/'backend/dot_gen_source_names.h',root/'backend/dot_gen_source_storage.h',root/'build/bin/cpfe-dot']},indent=2)+'\n')
    return checks


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output-dir',required=True,type=Path);parser.add_argument('--nativeaot',action='store_true');args=parser.parse_args()
    output=args.output_dir.resolve();output.mkdir(parents=True,exist_ok=False);commands=[]
    def run(command,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,capture_output=True,text=True,**kwargs);index=len(commands)
        (output/f'{index:03d}.stdout').write_text(result.stdout);(output/f'{index:03d}.stderr').write_text(result.stderr)
        commands.append({'command':command,'exit':result.returncode});(output/'commands.json').write_text(json.dumps(commands,indent=2)+'\n')
        if result.returncode:raise RuntimeError(f'{command}: {result.returncode}\n{result.stdout}\n{result.stderr}')
        return result
    checks=verify_source_names(Path(__file__).resolve().parents[2],Path(__file__).resolve().parent,output,args.nativeaot,run)
    (output/'results.json').write_text(json.dumps({'status':'passed','checks':checks},indent=2)+'\n');print(f'{len(checks)} source naming checks passed')
