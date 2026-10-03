"""Source enum signatures, fields, casts, pointers, bitfields and promotions."""
import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
from pathlib import Path


def verify_source_enums(root, tests, out, nativeaot, run):
    checks=[]
    source=tests/'source_enums.c'
    for language,standard,compiler in [('c','c17','gcc'),('c++','c++20','g++')]:
        native=out/(language.replace('+','p')+'-native')
        run([compiler,'-x',language,'-std='+standard,'-O2',source,'-o',native]);run([native])
        preprocessed=out/(language.replace('+','p')+('-input.i' if language=='c' else '-input.ii'))
        preprocessed.write_text(run([compiler,'-x',language,'-std='+standard,'-E',source]).stdout)
        oracle=out/(language.replace('+','p')+'-oracle.c')
        run([root/'build/bin/cpfe','--c17' if language=='c' else '--c++20','--gnu_version','130300',
             '--gen_c_file_name',oracle,'-o',oracle.with_suffix('.il'),*(['--instantiate','used'] if language=='c++' else []),preprocessed],
            env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
        executable=oracle.with_suffix('')
        run(['gcc','-std=gnu11','-w',oracle,root/'build/lib/libC.a','-lm','-o',executable]);run([executable])
        checks.append(language+': native and original EDG C enum arithmetic/promotion oracles')
        # Pair no-nest/static and nested/instance to exercise both type domains.
        for instance in (False,True):
            mode=language.replace('+','p')+('-instance' if instance else '-static')
            directory=out/mode
            run([sys.executable,root/'dotnet.py',source,'--language',language,'--std',standard,
                 '--libc','dotcc','--output-dir',directory,'--namespace','Source.Identity','--class-name','Core',
                 '--split','size','--split-size','3500',
                 *(['--instance-methods'] if instance else ['--no-nest-types'])])
            text='\n'.join(p.read_text() for p in directory.glob('Translated*.cs'))
            prefix='' if instance else 'Core_'
            enum_name=prefix+'Mode'
            assert re.search(r'\b'+enum_name+r' choose_mode\('+enum_name+r' ',text),mode
            assert re.search(r'\b'+enum_name+r' mode;',text),mode
            assert re.search(r'\b'+prefix+r'Flag flags\s*\{',text),mode
            assert re.search(r'\b'+prefix+r'Alias alias;',text),mode
            assert enum_name+'.ModeAdd' in text and enum_name+'.ModeDouble' in text
            assert re.search(r'delegate\*\s*managed<[^>]*'+enum_name+r'[^>]*>',text),mode
            assert re.search(r'\b'+enum_name+r'\* mode_address\(',text),mode
            assert re.search(r'ref '+enum_name+r' default_mode',text),mode
            manifest=json.loads((directory/'dot-gen.json').read_text())
            assert any(row['sourceName']=='Mode' for row in manifest['publicTypes']),manifest['publicTypes']
            if language=='c++':
                assert 'Model_Strong' in text and 'Model_Wide' in text
            run(['dotnet','build',directory/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true','-p:Nullable=enable'])
            run(['dotnet',directory/'bin/Release/net10.0/Generated.dll'])
            checks.append(mode+': JIT enum signatures, fields, pointers, varargs, arithmetic promotions and bitfields')
            if nativeaot:
                run(['dotnet','publish',directory/'Generated.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true',
                     '-p:IlcTreatWarningsAsErrors=true','-p:Nullable=enable','-p:DebugType=None','-p:DebugSymbols=false','-o',directory/'aot'])
                run([directory/'aot/Generated']);checks.append(mode+': NativeAOT typed enum semantics')
    for instance in (False,True):
        directory=out/('library-instance' if instance else 'library-static')
        run([sys.executable,root/'dotnet.py',source,'--language','c','--std','c17','--emit','managedlib',
             '--libc','dotcc','--output-dir',directory,'--namespace','Source.Identity','--class-name','Core',
             *(['--instance-methods'] if instance else [])])
        host=directory/'host';host.mkdir()
        (host/'Host.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>')
        receiver='core' if instance else 'Core';setup='using var core=new Core();' if instance else ''
        (host/'Program.cs').write_text(f'using Source.Identity; class EntryPoint {{static int Main(){{{setup}return {receiver}.enum_api(Core.Mode.ModeAdd)==Core.Mode.ModeDouble && {receiver}.source_enum_cases()==0?0:99;}}}}')
        run(['dotnet','build',host/'Host.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',host/'bin/Release/net10.0/Host.dll'])
        checks.append(directory.name+': direct typed public enum API')
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
    checks=verify_source_enums(Path(__file__).resolve().parents[2],Path(__file__).resolve().parent,output,args.nativeaot,run)
    (output/'results.json').write_text(json.dumps({'status':'passed','checks':checks},indent=2)+'\n');print(f'{len(checks)} source enum checks passed')
