"""Independent RFC2661 tunnel peer and actual authenticated PPP data."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('PPP L2TP qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/PppL2tp/native.py','--profile',ctx.profile],'ppp-l2tp-native',timeout=1200)
    native=ctx.root/('artifacts/l2tp-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'tunnel_auth':True,'wrong_challenge_rejected':True,'mutual_challenge':True,'dual_stack_negotiated':True,'udp_both_directions':True,'wrong_session_rejected':True,'stopccn':True,'baseline_restored':True,'negative_exit':92}.items():
        if control.get(key)!=value:raise AssertionError('Native PPP L2TP control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='ppp-l2tp-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProfile='+ctx.profile,'-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/PppL2tp/PppL2tp.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'PppL2tp.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'PppL2tp']
            transcript=ctx.run(command,label+'-run',timeout=120)
            if 'PASS ppp-l2tp:' not in transcript:raise AssertionError('Missing completed PPP L2TP assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'independent RFC2661 mutual/wrong challenge, dual-stack control, bidirectional UDP, invalid session IDs, StopCCN and resource baselines'})
    (ctx.artifacts/'ppp-l2tp-report.json').write_text(json.dumps(results,indent=2)+'\n')
