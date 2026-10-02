"""Inline-array value semantics and native frames beyond the former 64 KiB cap."""
import json,os,sys
from pathlib import Path


def verify_large_aggregates(root,tests,out,nativeaot,run):
    directory=out/'large-aggregates';directory.mkdir(parents=True)
    source=tests/'large_aggregates.cpp';lowered=directory/'oracle.c'
    run([root/'build/bin/cpfe-dot-oracle','--c++17','--gnu_version','130300','--gen_c_file_name',lowered,
         '-o',directory/'oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    run(['gcc','-std=gnu11','-w',lowered,root/'build/lib/libC.a','-o',directory/'oracle']);run([directory/'oracle'])
    checks=['large aggregate: original EDG C oracle for 160 KiB values, pointer arrays and 400 KiB initializer']
    for instance in (False,True):
        mode='instance' if instance else 'static';project=directory/mode
        run([sys.executable,root/'dotnet.py',source,'--output-dir',project,'--split','function',
             *(['--instance-methods'] if instance else ['--no-nest-types'])])
        generated='\n'.join(p.read_text() for p in project.glob('Translated*.cs'))
        assert 'InlineArray(20000)' in generated and 'InlineArray(24000)' in generated
        assert len(generated)<100000,len(generated)
        assert 'NativeMemory.Alloc(' in generated and 'NativeMemory.Free(__edg_frame_raw)' in generated
        run(['dotnet','build',project/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',project/'bin/Release/net10.0/Generated.dll'])
        checks.append(f'large aggregate/{mode}/JIT: compact typed arrays, heap frames, independent value copies, TLS and function pointers')
        if nativeaot:
            run(['dotnet','publish',project/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',project/'aot'])
            run([project/'aot/Generated'])
            checks.append(f'large aggregate/{mode}/NativeAOT: full array payload copies and tail preservation')
    source=tests/'repeated_initializers.c';lowered=directory/'repeat-oracle.c'
    run([root/'build/bin/cpfe-dot-oracle','--c11','--gnu_version','130300','--gen_c_file_name',lowered,
         '-o',directory/'repeat-oracle.il',source],env=dict(os.environ,EDG_BASE=str(root/'upstream/bases/docker/dev-env/gcc')))
    run(['gcc','-std=gnu11','-w',lowered,root/'build/lib/libC.a','-o',directory/'repeat-oracle']);run([directory/'repeat-oracle'])
    checks.append('repeated initializer: original EDG C scalar/record/string range designator oracle')
    for instance in (False,True):
        mode='instance' if instance else 'static';project=directory/('repeat-'+mode)
        run([sys.executable,root/'dotnet.py',source,'--language','c','--output-dir',project,
             *(['--instance-methods'] if instance else [])])
        generated=(project/'Translated.cs').read_text();assert len(generated)<100000,len(generated)
        assert '__edg_repeat' in generated
        run(['dotnet','build',project/'Generated.csproj','-c','Release','--nologo','-p:TreatWarningsAsErrors=true'])
        run(['dotnet',project/'bin/Release/net10.0/Generated.dll']);checks.append(f'repeated initializer/{mode}/JIT: compact loops preserve every element')
        if nativeaot:
            run(['dotnet','publish',project/'Generated.csproj','-c','Release','-r','linux-x64',
                 '-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',project/'aot'])
            run([project/'aot/Generated']);checks.append(f'repeated initializer/{mode}/NativeAOT: compact scalar/record/string range initialization')
    return checks


if __name__=='__main__':
    import argparse,subprocess
    parser=argparse.ArgumentParser();parser.add_argument('--output-dir',type=Path,required=True);parser.add_argument('--nativeaot',action='store_true');args=parser.parse_args()
    out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False);commands=[]
    def run(command,**kwargs):
        command=list(map(str,command));result=subprocess.run(command,text=True,capture_output=True,**kwargs)
        index=len(commands);(out/f'{index:03}.stdout').write_text(result.stdout);(out/f'{index:03}.stderr').write_text(result.stderr)
        commands.append({'command':command,'exit':result.returncode});(out/'commands.json').write_text(json.dumps(commands,indent=2))
        if result.returncode:raise RuntimeError(f'{command}\n{result.stdout}\n{result.stderr}')
        return result
    root=Path(__file__).resolve().parents[2]
    checks=verify_large_aggregates(root,root/'backend/tests',out,args.nativeaot,run)
    (out/'results.json').write_text(json.dumps(checks,indent=2)+'\n');print(json.dumps(checks,indent=2))
