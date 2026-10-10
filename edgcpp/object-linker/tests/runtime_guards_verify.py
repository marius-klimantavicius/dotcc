#!/usr/bin/env python3
"""Compare canonical managed local-static guard ABI with native Itanium guards."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
EDG = ROOT.parent


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--output-dir',type=Path,required=True)
    p.add_argument('--compiler',type=Path,required=True)
    p.add_argument('--linker',type=Path,required=True)
    p.add_argument('--packager',type=Path,default=ROOT/'compile.py')
    p.add_argument('--nativeaot',action='store_true')
    p.add_argument('--rid',default='linux-x64')
    args=p.parse_args();out=args.output_dir.resolve();out.mkdir(parents=True,exist_ok=False)
    checks,commands=[],[]

    def check(name, condition):
        checks.append(dict(name=name,passed=bool(condition)))
        if not condition: raise AssertionError(name)

    def run(name,command,expected=0):
        r=subprocess.run(list(map(str,command)),cwd=out,capture_output=True,timeout=60)
        log=out/(name+'.log');log.write_bytes(r.stdout+r.stderr)
        commands.append(dict(command=list(map(str,command)),exitCode=r.returncode,path=str(log),sha256=sha(log)))
        check(name,r.returncode==expected);print(name+': passed',flush=True)
        return r

    def compile(name,source):
        obj=out/(name+'.dotobj')
        run(name+'-compile',[sys.executable,'-B',args.packager.resolve(),source,'--output',obj,'--work-dir',out/(name+'-work'),
            '--compiler',args.compiler.resolve(),'--std','c11','-I',EDG/'backend/runtime/libc/include'])
        return obj

    failure=None
    try:
        source=out/'guards.c';shutil.copy2(ROOT/'tests/runtime_guards.c',source)
        run('native-build',['cc','-std=c11','-O2','-pthread',source,'-lstdc++','-o',out/'native'])
        run('native',[out/'native'])
        obj=compile('guards',source)
        provider=compile('threads',EDG/'backend/runtime/libc/threads/threads.c')
        project=out/'generated'
        run('link',['dotnet',args.linker.resolve(),'link','--out',project,'--entry','main','--allow-externs',obj,'--provider',provider])
        run('bind',[sys.executable,'-B',ROOT/'runtime/bind.py','--project',project])
        receipt=json.loads((project/'runtime-bindings.json').read_text())
        check('all-guard-imports-bound',receipt['status']=='bound' and {'__cxa_guard_acquire','__cxa_guard_release','__cxa_guard_abort'} <= {row['name'] for row in receipt['bound']})
        check('canonical-guard-body-provenance',any(row['path'].endswith('CppGuards.cs') and row['sha256']==sha(row['path']) for row in receipt['implementationInputs']))
        run('build',['dotnet','build',project/'Translated.csproj','--nologo','-v:q'])
        run('jit',['dotnet',project/'bin/Debug/net10.0/Translated.dll'])
        if args.nativeaot:
            run('aot-build',['dotnet','publish',project/'Translated.csproj','-r',args.rid,'-p:PublishAot=true',
                '-p:IlcTreatWarningsAsErrors=true','-o',out/'aot','--nologo','-v:q'])
            run('aot',[out/'aot/Translated'])
        bad=out/'bad.c';bad.write_text('int __cxa_guard_acquire(unsigned*); int main(void) { unsigned g=0; return __cxa_guard_acquire(&g); }\n')
        bad_obj=compile('bad',bad);bad_project=out/'bad-generated'
        run('bad-link',['dotnet',args.linker.resolve(),'link','--out',bad_project,'--entry','main','--allow-externs',bad_obj])
        rejected=run('bad-bind',[sys.executable,'-B',ROOT/'runtime/bind.py','--project',bad_project],1)
        check('wrong-width-rejected',b'Local-static guard ABI does not match' in rejected.stderr and not list(bad_project.glob('Runtime.CppGuards.cs')))
    except Exception as error:failure=str(error)
    result=dict(schema=1,status='failed' if failure else 'passed',error=failure,checks=checks,commands=commands,
        nativeAotVerified=args.nativeaot and not failure,compiler=dict(path=str(args.compiler.resolve()),sha256=sha(args.compiler)),
        linker=dict(path=str(args.linker.resolve()),sha256=sha(args.linker)),
        scope='Portable Itanium guard ABI, contention/publication and abort/retry; not complete source C++ initialization/EH lowering')
    (out/'results.json').write_text(json.dumps(result,indent=2)+'\n')
    print(json.dumps(dict(status=result['status'],checks=len(checks),error=failure)))
    return 1 if failure else 0


if __name__=='__main__':raise SystemExit(main())
