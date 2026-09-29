"""Independent RFC2516 peer and translated PPPoE client; L2TP gate remains separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('PPP Ethernet qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/PppLinks/ethernet-native.py','--profile',ctx.profile],'ppp-ethernet-native',timeout=1200)
    native=ctx.root/('artifacts/pppoe-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'discovery':True,'host_unique_rejected':True,'cookie_echo':True,'malformed_pads_rejected':True,'dual_stack_negotiated':True,'udp_both_directions':True,'padt':True,'baseline_restored':True,'negative_exit':92}.items():
        if control.get(key)!=value:raise AssertionError('Native PPP Ethernet control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='ppp-ethernet-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProfile='+ctx.profile,'-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/PppLinks/PppLinks.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'PppLinks.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'PppLinks']
            transcript=ctx.run([*command,"--ethernet"],label+'-run',timeout=120)
            if 'PASS ppp-ethernet:' not in transcript:raise AssertionError('Missing completed PPP Ethernet assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'independent discovery/service/cookie/malformed peer, dual-stack control, bidirectional UDP, termination/PADT and resource baselines'})
    (ctx.artifacts/'ppp-ethernet-report.json').write_text(json.dumps(results,indent=2)+'\n')
