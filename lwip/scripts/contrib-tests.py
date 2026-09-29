"""Finite extended-ABI/addon controls; whole contrib workflows remain separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('Contrib qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/Contrib/native.py'],'contrib-native',timeout=300)
    native=ctx.root/'artifacts/contrib-native/report.json';results=[]
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='contrib-addons-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/Contrib/Contrib.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'Contrib.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'Contrib']
            transcript=ctx.run([*command,native],label+'-run',timeout=30)
            if 'PASS contrib-addons:' not in transcript:raise AssertionError('Missing completed addon assertions')
            results.append({'profile':'contrib','form':form,'mode':mode,'status':'passed','scope':'seven ABI checks, extension callback ownership, route operations and ISN vectors'})
    (ctx.artifacts/'contrib-addons-report.json').write_text(json.dumps(results,indent=2)+'\n')
