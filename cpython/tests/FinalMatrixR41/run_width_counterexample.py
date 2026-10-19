"""One same-source native/translated JIT diagnostic, no library generation."""
import types
from pathlib import Path
from run import ART,CPY,HERE,Matrix,read,ref,write,bind

m=Matrix(types.SimpleNamespace(authority=ART/'direct-a32-authority.json',profile='upstream-tests-static',
    form='raw',mode='jit',phase='width-counterexample',attempt='initial',resume=False))
source=HERE/'locale_width_counterexample.c'
source_ref=ref(source)
native=m.work/'native-control'
m.run(['cc','-std=c11','-Wall','-Wextra','-Werror',source,'-o',native],'native-build',native=True)
m.run([native],'native-run',native=True)
compiler=CPY/'build/owner-a-r31-20261018/v22/tools/compiler/dotcc.dll'
emitted=m.work/'emitted'
m.run(['dotnet',compiler,'--emit=csproj','--runtime=c','-std=c17',source,'-o',emitted],'target-emit',cwd=CPY.parent)
project=next(emitted.glob('*.csproj'))
m.run(['dotnet','build',project,'-c','Release','--disable-build-servers','-m:1',
    '-p:UseSharedCompilation=false','-p:NuGetAudit=false'],'target-build')
pe=emitted/'bin/Release/net10.0/emitted.dll'
m.run(['dotnet',pe],'target-run')
assert ref(source)==source_ref
n=[__import__('json').loads(s) for s in (m.art/'native-run.log').read_text().splitlines()]
t=[__import__('json').loads(s) for s in (m.art/'target-run.log').read_text().splitlines()]
assert n[0]['wcharBytes']==4 and t[0]['wcharBytes']==2 and t[0]['wcharUnsigned']==1
for native_row,target_row in zip(n[1:5],t[1:5]):
    assert native_row['inputBytes']==target_row['inputBytes']
    assert native_row['roundtrip']==target_row['roundtrip']==1
    assert native_row['wholeErrno']==target_row['wholeErrno']==0
for index in (3,4):
    assert n[index]['genericPerUnitValidation']==1 and t[index]['genericPerUnitValidation']==0
    assert any(row['result']==-1 and row['errno']==84 for row in t[index]['perUnitEncoding'])
assert n[3]['wideUnits']==[0x10ffff] and t[3]['wideUnits']==[0xdbff,0xdfff]
for index in (5,6,7):
    assert n[index]['decoded']==t[index]['decoded']==-1 and n[index]['decodeErrno']==t[index]['decodeErrno']==84
proof=bind(pe,project,None)
write(m.art/'counterexample-result.json',dict(status='Confirmed ABI-specific generic caller mismatch; shared whole conversions PASS',
    source=source_ref,compiler=ref(compiler),native=ref(m.art/'native-run.log'),target=ref(m.art/'target-run.log'),
    actualTarget=proof,observations=dict(native=n,target=t),
    conclusion='Valid UTF16 pairs roundtrip in shared mbstowcs/wcstombs but cannot pass scalar-per-wchar validation or encode after splitting into isolated wchar units. No shared runtime fix is justified by this control.'))
m.finish(status='reduced-target-ABI-caller-mismatch',counterexample=ref(m.art/'counterexample-result.json'))
