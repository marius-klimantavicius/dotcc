"""Independent RFC6282 BLE/radio frame vectors through actual IPv6/UDP."""
import json
import sys
from campaigns.testing import forms,modes
from campaigns.translation import check_product

def run(ctx,suite):
    if ctx.options.rid!='linux-x64':raise RuntimeError('6LoWPAN qualification currently requires linux-x64')
    ctx.run([sys.executable,ctx.root/'tests/Lowpan/native.py','--profile',ctx.profile],'lowpan-vectors-native',timeout=1200)
    native=ctx.root/('artifacts/lowpan-native/'+ctx.profile+'/report.json');results=[]
    control=json.loads(native.read_text())
    for key,value in {'ble_and_radio_inputs':10,'independent_iphc_vectors':True,'ble_address_vectors':True,'crc_vector':True,'baseline_restored':True,'negative_exit':92,'short_source_original_exit':92}.items():
        if control.get(key)!=value:raise AssertionError('Native lowpan control mismatch: '+key)
    for form in forms(ctx):
        product=check_product(ctx,form)
        for mode in modes(ctx):
            label='lowpan-vectors-'+form+'-'+mode;work=ctx.work/label;output=work/'app'
            props=['-p:LwipProfile='+ctx.profile,'-p:LwipProject='+str(product),'-p:UseLocalLalrCc=false','--artifacts-path',work/'artifacts','-o',output]
            project=ctx.root/'tests/Lowpan/Lowpan.csproj'
            if mode=='jit':
                ctx.managed('build',project,label+'-build',*props);command=['dotnet',output/'Lowpan.dll']
            else:
                ctx.managed('publish',project,label+'-build',*props,'-r',ctx.options.rid,'-p:PublishAot=true',timeout=1800);command=[output/'Lowpan']
            transcript=ctx.run(command,label+'-run',timeout=120)
            if 'PASS lowpan-vectors:' not in transcript:raise AssertionError('Missing completed 6LoWPAN assertions')
            results.append({'profile':ctx.profile,'form':form,'mode':mode,'status':'passed','scope':'independent raw/IPHC IEEE802.15.4 and BLE receive vectors, address/CRC checks and owner/resource isolation'})
    (ctx.artifacts/'lowpan-vectors-report.json').write_text(json.dumps(results,indent=2)+'\n')
