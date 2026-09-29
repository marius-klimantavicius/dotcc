"""lwIP specialist suite dispatch; incomplete gates fail explicitly."""
import json
import sys

def run(ctx, suite):
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('lwIP host qualification currently requires linux-x64')
    if suite.name == 'oracle':
        ctx.run([sys.executable,ctx.root/'tests/NativeOracle/run.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-oracle'], 'native-oracle',timeout=180)
        return
    if suite.name not in ('consumer','aot-rooted','abi','sysarch','direct-apis','datagram-fixtures'):
        raise RuntimeError(f'lwIP required suite {suite.name} not yet implemented/qualified for {ctx.profile}; see docs/PLAN.md')
    from campaigns.testing import forms,modes
    from campaigns.translation import check_product
    project=ctx.root/({'abi':'tests/Abi/Abi.csproj','sysarch':'tests/SysArch/SysArch.csproj','direct-apis':'tests/SocketApi/SocketApi.csproj','datagram-fixtures':'tests/PacketScenarios/PacketScenarios.csproj'}.get(suite.name,'samples/ManagedConsumer/ManagedConsumer.csproj'))
    assembly={'abi':'Abi','sysarch':'SysArch','direct-apis':'SocketApi','datagram-fixtures':'PacketScenarios'}.get(suite.name,'ManagedConsumer')
    if suite.name in ('abi','datagram-fixtures'):
        ctx.run([sys.executable,ctx.root/'tests/NativeOracle/run.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-abi'], 'native-abi',timeout=180)
    results=[]
    try:
        for form in forms(ctx):
            product=check_product(ctx,form)
            for mode in modes(ctx):
                if suite.name=='aot-rooted' and mode!='aot':continue
                name=f'{suite.name}-{ctx.profile}-{form}-{mode}'
                output=ctx.work/name/'app'
                props=[f'-p:LwipProject={product}',f'-p:LwipProfile={ctx.profile}','-p:UseLocalLalrCc=false','--artifacts-path',ctx.root/'build/managed-consumer'/name]
                if suite.name=='aot-rooted':props+=['-p:LwipRootLibrary=true']
                if mode=='jit':
                    ctx.managed('build',project,name+'-build',*props,'-o',output)
                    command=['dotnet',output/(assembly+'.dll')]
                else:
                    ctx.managed('publish',project,name+'-publish',*props,'-r',ctx.options.rid,'-p:PublishAot=true','-o',output,timeout=1800)
                    command=[output/assembly]
                if suite.name=='abi':command.append(ctx.artifacts/'native-abi/report.json')
                if suite.name=='datagram-fixtures':command.append(ctx.artifacts/(name+'.pcap'))
                transcript=ctx.run(command,name+'-run',timeout=90)
                if suite.name=='abi':
                    if 'PASS abi:' not in transcript:raise RuntimeError('Missing ABI assertions')
                elif suite.name=='sysarch':
                    if 'PASS sysarch:' not in transcript:raise RuntimeError('Missing sys_arch assertions')
                elif suite.name=='direct-apis':
                    if 'PASS direct-apis:' not in transcript:raise RuntimeError('Missing direct API assertions')
                elif suite.name=='datagram-fixtures':
                    if 'PASS datagrams:' not in transcript:raise RuntimeError('Missing packet fixture assertions')
                elif 'PASS consumer:' not in transcript or 'PASS lifecycle:' not in transcript:raise RuntimeError('Missing completed consumer/lifecycle assertions')
                results.append({'profile':ctx.profile,'form':form,'mode':mode,'product':str(product),'status':'pass','cases':[line for line in transcript.splitlines() if line.startswith('PASS ')]})
    finally:
        (ctx.artifacts/(suite.name+'-cases.json')).write_text(json.dumps({'suite':suite.name,'results':results},indent=2)+'\n')
