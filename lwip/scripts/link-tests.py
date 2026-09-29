"""SLIP framing and owner-safe worker shutdown; complete links gate remains separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('SLIP qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/Links/native.py','--profile',ctx.profile],'slip-native',timeout=300)
    native=ctx.root/('artifacts/slip-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'slip_frames':1,'output_families':2,'payload_bytes':300,'negative_exit':92,'ip_datagrams':2,'decoder_pbufs_released':True}.items():
        if control.get(key)!=value:raise AssertionError('Native SLIP control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='slip-framing-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/Links/Links.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'Links.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'Links']
            transcript=ctx.run([*command,ctx.profile],label+'-run',timeout=30)
            if 'PASS slip-framing:' not in transcript:raise AssertionError('Missing completed SLIP assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'one receive frame, valid split escape stream, chained IPv4/IPv6 output, IPv4/IPv6 UDP ingress, partial decoder cleanup, owner worker stop/join'})
    (ctx.artifacts/'slip-framing-report.json').write_text(json.dumps(results,indent=2)+'\n')
