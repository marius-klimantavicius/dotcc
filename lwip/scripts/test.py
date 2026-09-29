"""lwIP specialist suite dispatch; incomplete gates fail explicitly."""
import json
import sys

def run(ctx, suite):
    if ctx.options.rid != 'linux-x64':
        raise RuntimeError('lwIP host qualification currently requires linux-x64')
    if suite.name == 'oracle':
        ctx.run([sys.executable,ctx.root/'tests/NativeOracle/run.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-oracle'], 'native-oracle',timeout=180)
        return
    if suite.name not in ('consumer','aot-rooted','abi','sysarch','direct-apis','datagram-fixtures','owner-lifecycle','socket-semantics','dns-fixtures','routing-ipv4','interop-kernel','pool-recovery','dhcp-fixtures','autoip-fixtures','ipv6-addressing','multicast-fixtures','routing-ipv6','bridge-fixtures'):
        raise RuntimeError(f'lwIP required suite {suite.name} not yet implemented/qualified for {ctx.profile}; see docs/PLAN.md')
    from campaigns.testing import forms,modes
    from campaigns.translation import check_product
    project=ctx.root/({'abi':'tests/Abi/Abi.csproj','sysarch':'tests/SysArch/SysArch.csproj','direct-apis':'tests/SocketApi/SocketApi.csproj','datagram-fixtures':'tests/PacketScenarios/PacketScenarios.csproj','owner-lifecycle':'tests/Lifecycle/Lifecycle.csproj','socket-semantics':'tests/SocketSemantics/SocketSemantics.csproj','dns-fixtures':'tests/Addressing/Addressing.csproj','routing-ipv4':'tests/Routing/Routing.csproj','interop-kernel':'tests/IndependentPeer/IndependentPeer.csproj','pool-recovery':'tests/PoolRecovery/PoolRecovery.csproj','dhcp-fixtures':'tests/Addressing/Addressing.csproj','autoip-fixtures':'tests/Addressing/Addressing.csproj','ipv6-addressing':'tests/Ipv6Addressing/Ipv6Addressing.csproj','multicast-fixtures':'tests/Multicast/Multicast.csproj','routing-ipv6':'tests/Routing6/Routing6.csproj','bridge-fixtures':'tests/Bridge/Bridge.csproj'}.get(suite.name,'samples/ManagedConsumer/ManagedConsumer.csproj'))
    assembly={'abi':'Abi','sysarch':'SysArch','direct-apis':'SocketApi','datagram-fixtures':'PacketScenarios','owner-lifecycle':'Lifecycle','socket-semantics':'SocketSemantics','dns-fixtures':'Addressing','routing-ipv4':'Routing','interop-kernel':'IndependentPeer','pool-recovery':'PoolRecovery','dhcp-fixtures':'Addressing','autoip-fixtures':'Addressing','ipv6-addressing':'Ipv6Addressing','multicast-fixtures':'Multicast','routing-ipv6':'Routing6','bridge-fixtures':'Bridge'}.get(suite.name,'ManagedConsumer')
    if suite.name in ('abi','datagram-fixtures'):
        ctx.run([sys.executable,ctx.root/'tests/NativeOracle/run.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-abi'], 'native-abi',timeout=180)
    if suite.name=='socket-semantics':
        ctx.run([sys.executable,ctx.root/'tests/SocketSemantics/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-sockets'], 'native-sockets',timeout=180)
    if suite.name=='dns-fixtures':
        ctx.run([sys.executable,ctx.root/'tests/Addressing/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-dns'], 'native-dns',timeout=180)
    if suite.name=='ipv6-addressing':
        ctx.run([sys.executable,ctx.root/'tests/Ipv6Addressing/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-ipv6-addressing'], 'native-ipv6-addressing',timeout=180)
    if suite.name=='multicast-fixtures':
        ctx.run([sys.executable,ctx.root/'tests/Multicast/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-multicast'], 'native-multicast',timeout=180)
    if suite.name=='routing-ipv6':
        ctx.run([sys.executable,ctx.root/'tests/Routing6/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-routing6'], 'native-routing6',timeout=180)
    if suite.name=='bridge-fixtures':
        ctx.run([sys.executable,ctx.root/'tests/Bridge/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-bridge'], 'native-bridge',timeout=180)
    if suite.name=='autoip-fixtures':
        ctx.run([sys.executable,ctx.root/'tests/Addressing/autoip-native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-autoip'], 'native-autoip',timeout=180)
    if suite.name=='dhcp-fixtures':
        ctx.run([sys.executable,ctx.root/'tests/Addressing/dhcp-native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-dhcp'], 'native-dhcp',timeout=180)
    if suite.name=='routing-ipv4':
        ctx.run([sys.executable,ctx.root/'tests/Routing/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-routing'], 'native-routing',timeout=180)
    if suite.name=='pool-recovery':
        ctx.run([sys.executable,ctx.root/'tests/PoolRecovery/native.py','--profile',ctx.profile,'--output',ctx.artifacts/'native-pools'], 'native-pools',timeout=180)
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
                if suite.name=='socket-semantics':command.append(ctx.artifacts/'native-sockets/report.json')
                if suite.name in ('datagram-fixtures','dns-fixtures','routing-ipv4','interop-kernel','dhcp-fixtures','autoip-fixtures','ipv6-addressing','multicast-fixtures','routing-ipv6','bridge-fixtures'):command.append(ctx.artifacts/(name+'.pcap'))
                if suite.name=='dhcp-fixtures':command.append('dhcp')
                if suite.name=='autoip-fixtures':command.append('autoip')
                if suite.name=='interop-kernel':command=[sys.executable,ctx.root/'tests/IndependentPeer/provision.py','--evidence',ctx.artifacts/(name+'-namespace.json'),'--',*command]
                transcript=ctx.run(command,name+'-run',timeout=150 if suite.name=='interop-kernel' else 90)
                if suite.name=='abi':
                    if 'PASS abi:' not in transcript:raise RuntimeError('Missing ABI assertions')
                elif suite.name=='sysarch':
                    if 'PASS sysarch:' not in transcript:raise RuntimeError('Missing sys_arch assertions')
                elif suite.name=='direct-apis':
                    if 'PASS direct-apis:' not in transcript:raise RuntimeError('Missing direct API assertions')
                elif suite.name=='datagram-fixtures':
                    if 'PASS datagrams:' not in transcript:raise RuntimeError('Missing packet fixture assertions')
                elif suite.name in ('owner-lifecycle','socket-semantics','dns-fixtures','routing-ipv4','interop-kernel','pool-recovery','dhcp-fixtures','autoip-fixtures','ipv6-addressing','multicast-fixtures','routing-ipv6','bridge-fixtures'):
                    if 'PASS '+suite.name+':' not in transcript:raise RuntimeError('Missing '+suite.name+' assertions')
                elif 'PASS consumer:' not in transcript or 'PASS lifecycle:' not in transcript:raise RuntimeError('Missing completed consumer/lifecycle assertions')
                results.append({'profile':ctx.profile,'form':form,'mode':mode,'product':str(product),'status':'pass','cases':[line for line in transcript.splitlines() if line.startswith('PASS ')]})
    finally:
        (ctx.artifacts/(suite.name+'-cases.json')).write_text(json.dumps({'suite':suite.name,'results':results},indent=2)+'\n')
