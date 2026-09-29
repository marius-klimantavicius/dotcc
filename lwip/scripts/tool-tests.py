"""Uncompressed makefsdata raw/processed JIT/AOT native comparison."""
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite):
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('makefsdata runtime qualification currently requires linux-x64')
    spec=importlib.util.spec_from_file_location('lwip_native_tools',ctx.root/'scripts/native-tools.py')
    native=importlib.util.module_from_spec(spec);spec.loader.exec_module(native)
    native.run()
    native_work=ctx.root/'build/native-tools-qualified'
    expected=(native_work/'fsdata.c').read_bytes()
    reports=[]
    project=ctx.root/'src/Tools.MakeFsData/Tools.MakeFsData.csproj'
    for form in forms(ctx):
        library=check_product(ctx,form)
        if not library.exists():raise RuntimeError('Translate required makefsdata profile first: '+str(library))
        for mode in modes(ctx):
            label='makefsdata-'+form+'-'+mode
            work=ctx.work/label;work.mkdir(parents=True,exist_ok=True)
            binary=work/'bin'
            args=['-p:LwipToolsProject='+str(library),'-p:UseLocalLalrCc=false','--artifacts-path',str(work/'artifacts'),'-o',str(binary)]
            if mode=='aot':ctx.managed('publish',project,label+'-build',*args,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800)
            else:ctx.managed('build',project,label+'-build',*args)
            shutil.copytree(native_work/'content',work/'content')
            command=([str(binary/'Tools.MakeFsData')] if mode=='aot' else ['dotnet',str(binary/'Tools.MakeFsData.dll')])+['content','-f:fsdata.c','-11','-c']
            result=subprocess.run(command,cwd=work,text=True,capture_output=True,timeout=30,preexec_fn=native.limits)
            (ctx.artifacts/(label+'-command.json')).write_text(json.dumps({'command':command,'cwd':str(work),'exit':result.returncode},indent=2)+'\n')
            (ctx.artifacts/(label+'.log')).write_text(result.stdout+result.stderr)
            if result.returncode:raise RuntimeError(label+' failed: '+str(result.returncode)+' '+result.stderr)
            actual=(work/'fsdata.c').read_bytes()
            if actual!=expected:raise AssertionError(label+' generated content differs from native byte-for-byte')
            for temporary in ('fsdata.tmp','fshdr.tmp'):
                if (work/temporary).exists():raise AssertionError(label+' retained temporary '+temporary)
            reports.append({'form':form,'mode':mode,'generated_bytes':len(actual),'files':3,'status':'passed',
                            'command':str(ctx.artifacts/(label+'-command.json')),'log':str(ctx.artifacts/(label+'.log')),
                            'checks':'exact native image; independently validated checksums/offsets in native fixture'})
    report=ctx.artifacts/'makefsdata-report.json';report.write_text(json.dumps(reports,indent=2)+'\n');ctx.receipt['makefsdata_report']=str(report)
