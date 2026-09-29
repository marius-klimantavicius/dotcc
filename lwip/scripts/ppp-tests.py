"""Bundled primitive/authentication/MPPE controls; complete PPP workflows remain separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('Bundled PPP qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/Ppp/native.py','--profile',ctx.profile],'ppp-native',timeout=300)
    native=ctx.root/('artifacts/ppp-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'primitives':5,'chap_rfc2759_positive':1,'mppe_packets':600,'negative_exit':92}.items():
        if control.get(key)!=value:raise AssertionError('Native PPP control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='bundled-ppp-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/Ppp/Ppp.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'Ppp.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'Ppp']
            transcript=ctx.run([*command,native],label+'-run',timeout=30)
            if 'PASS bundled-ppp:' not in transcript:raise AssertionError('Missing completed PPP assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'five primitive vectors, RFC2759 positive authentication, RFC3079 key, 600 MPPE packets'})
    (ctx.artifacts/'bundled-ppp-report.json').write_text(json.dumps(results,indent=2)+'\n')
