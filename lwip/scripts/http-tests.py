"""HTTP baseline and actual generated-file roundtrip; other applications remain separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('HTTP qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/Applications/native.py','--profile',ctx.profile],'http-native',timeout=300)
    native=ctx.root/('artifacts/http-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'http_transactions':5,'ssi':1,'cgi_raw_parameters':1,'post':1,'custom_files_released':True,'negative_exit':92,'generated_files':3 if ctx.profile=='http-content' else 0}.items():
        if control.get(key)!=value:raise AssertionError('Native HTTP control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='http-transactions-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/Applications/Applications.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'Applications.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'Applications']
            transcript=ctx.run([*command,ctx.profile,native],label+'-run',timeout=30)
            if 'PASS http-transactions:' not in transcript:raise AssertionError('Missing completed HTTP assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'custom GET, IPv6 SSI, CGI raw parameters,404,POST, fragmented headers; http-content additionally serves three files produced by translated makefsdata'})
    (ctx.artifacts/'http-transactions-report.json').write_text(json.dumps(results,indent=2)+'\n')
