"""Actual PPPoS authentication/data; broader PPPoE/L2TP link gates remain separate."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('PPP serial qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/PppLinks/native.py','--profile',ctx.profile],'ppp-serial-native',timeout=1200)
    native=ctx.root/('artifacts/ppp-links-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'serial_negotiations':6,'wrong_credentials':5,'ipv4_ipv6_control':True,'negative_exit':92,'original_eap_exit':92,'datagrams':6,'tcp_payloads':18,'vj_negotiated':True,'mppe_wire_private':True,'closed_and_freed':True}.items():
        if control.get(key)!=value:raise AssertionError('Native PPP serial control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='ppp-serial-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProfile='+ctx.profile,'-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/PppLinks/PppLinks.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'PppLinks.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'PppLinks']
            transcript=ctx.run([*command,ctx.profile],label+'-run',timeout=120)
            if 'PASS ppp-serial:' not in transcript:raise AssertionError('Missing completed PPP serial assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'six negotiated serial sessions, five credential failures, dual-stack control, six UDP and eighteen TCP payloads, independent FCS/VJ/MPPE wire checks, per-owner cleanup'})
    (ctx.artifacts/'ppp-serial-report.json').write_text(json.dumps(results,indent=2)+'\n')
