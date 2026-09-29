"""Focused native/managed enhanced-option behavior and owner lifetimes."""
import json
import sys
from campaigns.recipes import helper
from campaigns.testing import forms,modes
from campaigns.translation import check_product


def run(ctx,suite):
    if ctx.profile!='threaded-enhanced' or ctx.options.rid!='linux-x64':
        raise RuntimeError('Enhanced option qualification requires threaded-enhanced/linux-x64')
    output=ctx.work/'enhanced-native'
    ctx.run([sys.executable,ctx.root/'tests/Enhanced/native.py','--output',output],'enhanced-native',timeout=1200)
    native=json.loads((output/'report.json').read_text())
    if native['assert_control_exit']!=92 or native['result']['options']!=10 or native['result']['status']!='passed' or not native['result']['natural_baseline']:
        raise AssertionError('Native enhanced option contracts incomplete')
    if set(native['result']['baseline_resources'])!={'heap','pbuf_pool','tcp_pcb','udp_pcb','netconn','netbuf','tcp_pcb_listen'}:
        raise AssertionError('Native enhanced lifetime resources incomplete')
    (ctx.artifacts/'enhanced-native-report.json').write_text(json.dumps(native,indent=2)+'\n')
    results=[]
    try:
        for form in forms(ctx):
            product=check_product(ctx,form)
            for mode in modes(ctx):
                label='enhanced-'+form+'-'+mode;work=ctx.work/label;app=work/'app';wire=ctx.artifacts/(label+'-wire')
                args=['-p:LwipProject='+str(product),'-p:LwipProfile='+ctx.profile,'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',app]
                project=ctx.root/'tests/Enhanced/Enhanced.csproj'
                if mode=='jit':
                    ctx.managed('build',project,label+'-build',*args);command=['dotnet',app/'Enhanced.dll']
                else:
                    ctx.managed('publish',project,label+'-build',*args,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[app/'Enhanced']
                text=ctx.run([*command,output/'report.json',wire],label+'-run',timeout=180)
                if 'PASS enhanced-options:' not in text:raise AssertionError('Managed enhanced option assertions incomplete')
                evidence={path.name:helper(ctx.root/'tests/Enhanced/native.py').check_wire(path) for path in sorted(wire.glob('*.wire'))}
                if len(evidence)!=3:raise AssertionError('Both owners and post-disposal survivor wire records required')
                results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','options':10,'wire':evidence,'scope':'native-derived layout/window comparison, complete lawful independent peer packets, ordinary option APIs, deterministic timers, natural heap/pbuf/TCP/UDP PCB/netconn/netbuf/listener cleanup, two owners and survivor'})
    finally:
        (ctx.artifacts/'enhanced-options-report.json').write_text(json.dumps(results,indent=2)+'\n')
