"""Owning API lifetime controls using original socket/netconn/DNS algorithms."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('Owning API qualification currently requires linux-x64')
    native=['interface_native']
    if ctx.profile!='nosys':native+=['socket_refs_native','native']
    for name in native:
        ctx.run([sys.executable,ctx.root/('tests/OwnedApi/'+name+'.py'),'--profile',ctx.profile,'--output',ctx.artifacts/('owned-'+name)],'owned-'+name,timeout=300)
    if ctx.profile=='nosys':
        local=ctx.root/'tests/LocalReference/LocalReference.csproj'
        local_output=ctx.work/'local-reference-app'
        ctx.managed('build',local,'owned-local-reference-build','-p:UseLocalLalrCc=false',
                    '--artifacts-path',ctx.work/'local-reference-artifacts','-o',local_output)
        output=ctx.run(['dotnet',local_output/'LocalReference.dll'],'owned-local-reference-run',timeout=60)
        if 'PASS local project:' not in output:raise AssertionError('Local relative project selection failed')
    results=[]
    project=ctx.root/'tests/OwnedApi/OwnedApi.csproj'
    product=check_product(ctx,tuple(forms(ctx))[0])
    mismatch='threaded' if ctx.profile=='nosys' else 'nosys'
    rejected=ctx.run(['dotnet','build',project,'-p:LwipProject='+str(product),'-p:LwipProfile='+mismatch,
                      '-p:UseLocalLalrCc=false','--artifacts-path',ctx.work/'profile-mismatch-artifacts'],
                     'owned-profile-mismatch',check=False,timeout=180)
    if ctx.receipt['commands'][-1].get('exit_code')==0 or 'does not match the actual referenced product' not in rejected:
        raise AssertionError('Explicit mismatched profile hint was not rejected by the actual product metadata')
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='owned-api-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            # Deliberately omit LwipProfile: the actual project reference owns
            # capability selection, especially when NO_SYS omits socket symbols.
            props=['-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'OwnedApi.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'OwnedApi']
            transcript=ctx.run(command,label+'-run',timeout=180)
            if 'PASS owned-api: actual product capability selection and post-core-join callback root lifetime' not in transcript:raise AssertionError('Owning API lifecycle assertions did not complete')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','mismatched_profile_rejected':True,'profile_hint_omitted':True,'cases':[line for line in transcript.splitlines() if line.startswith('PASS ')]})
    (ctx.artifacts/'owned-api-report.json').write_text(json.dumps(results,indent=2)+'\n')
