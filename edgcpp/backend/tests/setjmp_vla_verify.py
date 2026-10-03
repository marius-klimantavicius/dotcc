"""Versioned source nonlocal-jump VLA continuations and exact storage lifetime."""
import json
from pathlib import Path
import resource
import shutil
import subprocess
import sys


def verify_setjmp_vla(root,tests,out,nativeaot,run):
    source=out/'input.c';shutil.copy2(tests/'setjmp_vla.c',source);checks=[]
    clang=shutil.which('clang') or next((str(p) for p in sorted(Path('/usr/lib').glob('llvm-*/bin/clang'))),None)
    if not clang:raise RuntimeError('Clang required for source VLA nonlocal-jump oracle')
    for label,compiler in [('gcc','gcc'),('clang',clang)]:
        for optimization in ('-O0','-O2'):
            executable=out/(label+optimization)
            run([compiler,'-std=c11',optimization,source,'-o',executable]);run([executable])
    checks.append('VLA nonlocal jumps: native GCC/Clang O0/O2 before/after allocation, repeated jumps, VLA buffers and VM pointers')
    # The demonstration original-C runtime uses stack-address heuristics, which
    # can reclaim surviving same-frame VLAs after a jump and another allocation.
    # Its distinct observation is retained/documented separately; it is not an
    # equivalent oracle for this lifetime contract. No generated C is patched.
    for instance in (False,True):
        mode='instance' if instance else 'static';directory=out/mode
        run([sys.executable,root/'dotnet.py',source,'--language','c','--libc','dotcc',
             '--output-dir',directory,'--namespace','','--class-name','Core','--split','size','--split-size','3200',
             *(['--instance-methods'] if instance else [])])
        manifest=json.loads((directory/'dot-gen.json').read_text())
        assert manifest['nonlocalJump']['version']==2 and manifest['nonlocalJump']['bufferSize']==32,manifest.get('nonlocalJump')
        entry=directory/'EntryPoint.cs';(directory/'EntryPoint.generated.cs.saved').write_bytes(entry.read_bytes())
        receiver='owner' if instance else 'Core'
        entry.write_text(f'''internal static class EntryPoint {{
  public static int Main() {{
    {('using var owner=new Core();' if instance else '')}
    var mark=global::Edg.Runtime.Vla.Mark();
    for(int i=0;i<3;++i) {{
      int result={receiver}.Run(global::System.Environment.GetCommandLineArgs());
      if(result!=0)return result;
      if(global::Edg.Runtime.Vla.Mark()!=mark)throw new global::System.Exception("VLA storage survived source activation");
    }}
    {receiver}.Shutdown();return 0;
  }}
}}
''')
        def verify(command,label):
            run(command);checks.append(label+': exact destination VLA checkpoint, surviving buffers and activation reclamation')
            def no_core():resource.setrlimit(resource.RLIMIT_CORE,(0,0))
            invocation=list(map(str,[*command,'e']))
            try:result=subprocess.run(invocation,text=True,capture_output=True,preexec_fn=no_core,timeout=60)
            except subprocess.TimeoutExpired as error:
                (directory/(label+'-escaped.stdout')).write_bytes(error.stdout or b'');(directory/(label+'-escaped.stderr')).write_bytes(error.stderr or b'')
                (directory/(label+'-escaped.json')).write_text(json.dumps({'command':invocation,'timeout':60},indent=2));raise
            (directory/(label+'-escaped.stdout')).write_text(result.stdout);(directory/(label+'-escaped.stderr')).write_text(result.stderr)
            (directory/(label+'-escaped.json')).write_text(json.dumps({'command':invocation,'exit':result.returncode},indent=2))
            assert result.returncode!=0 and 'Nonlocal jump VLA scope has ended' in result.stderr,(label,result.returncode,result.stderr[:1000])
            checks.append(label+': diagnoses expired allocated-VLA target before resuming source UB')
        project=directory/'Generated.csproj';run(['dotnet','build',project,'-c','Release','-p:TreatWarningsAsErrors=true'])
        verify(['dotnet',directory/'bin/Release/net10.0/Generated.dll'],mode+'-jit')
        if nativeaot:
            run(['dotnet','publish',project,'-c','Release','-r','linux-x64','-p:PublishAot=true','-p:IlcTreatWarningsAsErrors=true','-o',directory/'aot'])
            verify([directory/'aot/Generated'],mode+'-aot')
    return checks
