#!/usr/bin/env python3
"""Valid pinned image only: native LoadProgram vs derived actual managed core."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import struct
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'scripts'))
from core_inputs import compiler_identity
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--assembly-receipt',type=Path,required=True)
parser.add_argument('--native-only',action='store_true')
args=parser.parse_args()
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
base=ROOT/'generated/elf-loading';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base))
out=ROOT/'artifacts/elf-loading'/a.name;out.mkdir(parents=True)
assembly_path=args.assembly_receipt.resolve();assembly=json.loads(assembly_path.read_text())
profile=Path(assembly['profile']);inputs=json.loads((profile/'inputs.json').read_text())
cli=ROOT.parent/'DotCC/bin/Release/net10.0/dotcc.dll'
post=ROOT.parent/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
identity=compiler_identity(cli.parent)
if identity!=inputs['compiler']:raise RuntimeError('compiler differs from qualified baseline')
for name,digest in inputs['staged_headers'].items():
    if sha(profile/name)!=digest:raise RuntimeError('frozen input changed: '+name)
if not assembly['linked'] or sha(profile/'inputs.json')!=assembly['identity']['profile_inputs_sha256']:
    raise RuntimeError('invalid baseline assembly identity')
guest=ROOT/'build/guest/service'
pins=json.loads((ROOT/'tests/ServiceFixture/qualified-build.json').read_text())
if sha(guest)!=pins['executable_sha256']:raise RuntimeError('pinned service hash differs')
shutil.copyfile(guest,a/'service');(a/'service').chmod(0o555)
image=guest.read_bytes()
if image[:6]!=b'\x7fELF\x02\x01' or struct.unpack_from('<HH',image,16)!=(2,62):raise RuntimeError('pinned ELF kind')
phoff=struct.unpack_from('<Q',image,32)[0];phent,phnum=struct.unpack_from('<HH',image,54)
segments=[]
for i in range(phnum):
    p=struct.unpack_from('<IIQQQQQQ',image,phoff+i*phent)
    if p[0]==3:raise RuntimeError('static fixture required')
    if p[0]!=1:continue
    digest=14695981039346656037
    for value in image[p[2]:p[2]+p[5]]:digest=((digest^value)*1099511628211)&((1<<64)-1)
    segments.append(dict(address=p[3],file_size=p[5],memory_size=p[6],digest=digest,
                         protection=(1 if p[1]&4 else 0)|(2 if p[1]&2 else 0)|(4 if p[1]&1 else 0)))
initializer='static const struct ExpectedSegment expected_segments[] = {\n'+''.join(
    '  {%dUL,%dUL,%dUL,%dUL,%d},\n'%(s['address'],s['file_size'],s['memory_size'],s['digest'],s['protection']) for s in segments)+'};'
probe=(ROOT/'tests/ElfLoading/probe.c').read_text()
if probe.count('/* EXPECTED_SEGMENTS */')!=1:raise RuntimeError('expected segment insertion differs')
(a/'probe.c').write_text(probe.replace('/* EXPECTED_SEGMENTS */',initializer))
shutil.copyfile(ROOT/'tests/ElfLoading/Program.cs',a/'Program.cs')
shutil.copyfile(ROOT/'src/HostMemory/HostMemory.c',a/'HostMemory.c')
receipt=dict(kind='derived actual-core valid pinned ELF loading; no guest instructions',passed=False,
    runner_sha256=sha(Path(__file__)),assembly_receipt=str(assembly_path),assembly_sha256=sha(assembly_path),
    profile=str(profile),compiler=identity,segments=segments,results={},
    authored_inputs={p.name:sha(p) for p in a.iterdir() if p.is_file()},
    source_template_sha256=sha(ROOT/'tests/ElfLoading/probe.c'),
    header_contract='Exact frozen canonical headers retained; HostMemory implementation replaced without ABI/layout change; added protection-query definition unused by this harness')
def save():(out/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
def run(cmd,label,timeout=180):
    cmd=list(map(str,cmd));start=time.monotonic()
    with (out/(label+'.log')).open('wb') as log:
        try:code=subprocess.run(cmd,stdout=log,stderr=subprocess.STDOUT,timeout=timeout,env=dict(os.environ,LC_ALL='C')).returncode
        except subprocess.TimeoutExpired:code=124
    receipt['results'][label]=dict(command=cmd,exit_code=code,seconds=time.monotonic()-start);save()
    if code:raise RuntimeError(label+' failed: '+str(out/(label+'.log')))
    return (out/(label+'.log')).read_bytes()
receipt['postprocessor']=compiler_identity(post.parent) if (post.parent/'dotcc.dll').exists() else {p.name:sha(p) for p in post.parent.glob('*.dll')}
cc=Path(shutil.which('cc')).resolve()
receipt['native_compiler']=dict(path=str(cc),sha256=sha(cc),version=subprocess.check_output([str(cc),'--version'],text=True).splitlines()[0])
save()
native=ROOT/'build/native/source';archive=native/'o/blink/blink.a'
closure=json.loads((profile/'closure.json').read_text())
if sha(archive)!=closure['native_archive_sha256'] or sha(native/'config.h')!=closure['native_config_sha256']:
    raise RuntimeError('native archive/config drift')
receipt['native_archive_sha256']=sha(archive);receipt['native_config_sha256']=sha(native/'config.h')
run([cc,'-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-Werror','-I',native,a/'probe.c',archive,
     '-lz','-lrt','-lm','-pthread','-o',a/'native'],'native-build',60)
expected=run([a/'native',a/'service'],'native',30)
receipt['native_binary_sha256']=sha(a/'native');receipt['native_output_sha256']=hashlib.sha256(expected).hexdigest();save()
if args.native_only:
    receipt['native_passed']=True;save();print(str(out/'receipt.json'));sys.exit(0)
# Reuse exact producer objects. The two new objects use the same physical
# header tree, retaining anonymous aggregate identity across this derived link.
objects={name:Path(row['object_path']) for name,row in assembly['objects'].items()}
for name,path in objects.items():
    if sha(path)!=assembly['objects'][name]['object_sha256']:raise RuntimeError('baseline object changed: '+name)
receipt['retained_objects']={name:dict(path=str(path),sha256=sha(path),producer=assembly['objects'][name]['receipt'])
                              for name,path in objects.items() if name!='authored/HostMemory.c'}
def emit(template,source,name,extra=()):
    row=assembly['objects'][template];old=row['canonical_source'];command=row['command'].copy()
    command[command.index(old)]=str(source)
    command[command.index('-o')+1]=str(a/(name+'.cs'))
    if '--override-report' in command:command[command.index('--override-report')+1]=str(out/(name+'.overrides.jsonl'))
    command[3:3]=list(extra)
    run(command,name+'-emit',180)
    if compiler_identity(cli.parent)!=identity:raise RuntimeError('compiler changed')
    result=a/(name+'.cs')
    receipt.setdefault('new_objects',{})[name]=dict(source=str(source),source_sha256=sha(source),object=str(result),object_sha256=sha(result),template_command_from=template)
    save();return result
objects['authored/HostMemory.c']=emit('authored/HostMemory.c',a/'HostMemory.c','HostMemory')
objects['authored/ElfLoading.c']=emit('blink/loader.c',a/'probe.c','ElfLoading',['-DBLINK_ELF_MANAGED'])
generated=a/'raw-generated'
run(['dotnet',cli,*objects.values(),'--emit=managedlib','--nest-types','--class-name','BlinkCore',
     '--namespace','Managed.Emulation','--runtime=c','-o',generated],'link',180)
receipt['generated']={p.name:sha(p) for p in generated.glob('*.cs')}
shutil.copytree(generated,a/'optimized-generated')
shutil.copytree(profile/'host-project',a/'host')
bindings=json.loads((profile/'binding-sources.json').read_text());(a/'bridges').mkdir()
for name in bindings['authored_managed']:shutil.copyfile(profile/name,a/'bridges'/Path(name).name)
receipt['consumer_snapshot']={str(p.relative_to(a)):sha(p) for parent in [a/'host',a/'bridges'] for p in parent.rglob('*') if p.is_file()};save()
def project(path,name,kind,sources,refs,root=None):
    tree=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(tree,'PropertyGroup')
    for key,value in dict(TargetFramework='net10.0',AssemblyName=name,OutputType=kind,AllowUnsafeBlocks='true',Nullable='disable',EnableDefaultCompileItems='false',DefineConstants='BLINK_FULL_CORE',WarningsAsErrors='CS8500').items():ET.SubElement(props,key).text=value
    items=ET.SubElement(tree,'ItemGroup')
    for source in sources:ET.SubElement(items,'Compile',Include=str(source))
    for ref in refs:ET.SubElement(items,'ProjectReference',Include=str(ref))
    if root:ET.SubElement(items,'TrimmerRootAssembly',Include=root)
    ET.ElementTree(tree).write(path,encoding='unicode')
for mode in ['raw','optimized']:
    folder=a/mode;folder.mkdir();lib=folder/'ElfLibrary.csproj';consumer=folder/'ElfLoading.csproj'
    project(lib,'ElfLibrary','Library',[a/(mode+'-generated')/'*.cs',a/'bridges/*.cs'],[a/'host/Managed.Emulation.Host.csproj'])
    # Separate output/intermediate directories avoid a shared-project obj clash.
    c=folder/'consumer';c.mkdir();consumer=c/'ElfLoading.csproj'
    project(consumer,'ElfLoading','Exe',[a/'Program.cs'],[lib],'ElfLibrary')
    if mode=='optimized':
        run(['dotnet','restore',lib],mode+'-restore')
        run(['dotnet',post,lib,'--in-place'],mode+'-postprocess',300)
    run(['dotnet','build',consumer,'-c','Release'],mode+'-build',300)
    value=run(['dotnet',c/'bin/Release/net10.0/ElfLoading.dll',a/'service'],mode+'-jit',60)
    if value!=expected:raise RuntimeError(mode+' JIT differs from native loader state')
    target=folder/'publish'
    run(['dotnet','publish',consumer,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',target],mode+'-aot-build',600)
    value=run([target/'ElfLoading',a/'service'],mode+'-aot',60)
    if value!=expected:raise RuntimeError(mode+' AOT differs from native loader state')
if receipt['generated']!={p.name:sha(p) for p in generated.glob('*.cs')}:raise RuntimeError('raw generated source changed')
if compiler_identity(cli.parent)!=identity:raise RuntimeError('compiler changed during test')
receipt['binaries']={str(p.relative_to(a)):sha(p) for p in [a/'native',a/'raw/publish/ElfLoading',a/'optimized/publish/ElfLoading']}
receipt.update(passed=True,limitations=['Only the pinned valid static ELF image; no malformed corpus.','No guest instructions executed.','One process-discarded worker; no upstream static-state reuse after disposal.','Derived retained-object provenance, not a fresh canonical 110-source emission.'])
save();print(str(out/'receipt.json'))
