"""Prepare the HTTP profile from the actual translated generator, without recursion."""
import copy
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import time
from campaigns.translation import check_product


def prepare(ctx,includes):
    dependency=copy.copy(ctx);dependency.profile='makefsdata';dependency.receipt={}
    try:project=check_product(dependency,'processed')
    except RuntimeError as error:raise RuntimeError('http-content requires a published makefsdata profile; translate --profile makefsdata first') from error
    ctx.receipt['http_generator_dependency']=dependency.receipt
    work=ctx.work/'http-generator';work.mkdir(parents=True,exist_ok=True);content=work/'content';content.mkdir()
    cases={'empty.txt':b'','odd.txt':b'abcde','multiple.txt':bytes(i%251 for i in range(4097))}
    for name,data in cases.items():(content/name).write_bytes(data)
    binary=work/'app'
    ctx.managed('build',ctx.root/'src/Tools.MakeFsData/Tools.MakeFsData.csproj','http-content-generator-build',
                '-p:LwipToolsProject='+str(project),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',binary)
    command=['dotnet',str(binary/'Tools.MakeFsData.dll'),'content','-f:dotcc_fsdata.c','-11','-c']
    # Portable orchestration: avoid POSIX-only preexec_fn/resource imports.
    # The known upstream progress failure is bounded on every Python host.
    with (ctx.artifacts/'http-generator.log').open('w') as log:
        process=subprocess.Popen(command,cwd=work,stdout=log,stderr=subprocess.STDOUT,text=True)
        deadline=time.monotonic()+30
        try:
            while process.poll() is None:
                if time.monotonic()>deadline:raise TimeoutError('HTTP generator exceeded30seconds')
                for path in (work/'dotcc_fsdata.c',work/'fsdata.tmp',work/'fshdr.tmp'):
                    if path.exists() and path.stat().st_size>8*1024*1024:raise RuntimeError('HTTP generator exceeded8MiB file bound: '+path.name)
                try:process.wait(timeout=.05)
                except subprocess.TimeoutExpired:pass
        finally:
            if process.poll() is None:process.kill();process.wait()
    if process.returncode:raise RuntimeError('Translated HTTP input generation failed: '+str(process.returncode))
    image=work/'dotcc_fsdata.c';data=image.read_bytes()
    shutil.copyfile(image,includes/'dotcc_fsdata.c');shutil.copyfile(image,ctx.artifacts/'dotcc_fsdata.c')
    report={'generator_project':str(project),'generator_form':'processed','generator_mode':'jit','command':command,'exit':process.returncode,
            'image':str(ctx.artifacts/'dotcc_fsdata.c'),'image_sha256':hashlib.sha256(data).hexdigest(),
            'inputs':{name:{'bytes':len(value),'sha256':hashlib.sha256(value).hexdigest()} for name,value in cases.items()}}
    ctx.receipt['generated_http_content']=report
    (ctx.artifacts/'http-generator.json').write_text(json.dumps(report,indent=2)+'\n')
