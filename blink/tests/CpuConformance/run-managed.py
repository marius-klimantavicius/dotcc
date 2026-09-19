#!/usr/bin/env python3
"""Derive only the CPU frontend object; qualify actual translated core all4."""
import argparse,difflib,hashlib,json,os,shutil,subprocess,sys,tempfile,time
from pathlib import Path
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2];REPO=ROOT.parent
sys.path.insert(0,str(ROOT/'scripts'))
from core_inputs import compiler_identity
from features import inventory
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--core-receipt',type=Path,required=True)
parser.add_argument('--observe-differences',action='store_true')
args=parser.parse_args();core_path=args.core_receipt.resolve();core=json.loads(core_path.read_text())
if not core.get('passed') or core.get('diagnostic_replay'):raise SystemExit('requires qualified actual-core execution receipt')
assembly_path=Path(core['assembly_receipt']);assembly=json.loads(assembly_path.read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if sha(assembly_path)!=core['assembly_receipt_sha256'] or not assembly['linked']:raise SystemExit('assembly receipt changed')
profile=Path(assembly['profile']);inputs=json.loads((profile/'inputs.json').read_text())
if sha(profile/'inputs.json')!=assembly['identity']['profile_inputs_sha256']:raise SystemExit('profile identity changed')
for name,digest in inputs['staged_headers'].items():
    if sha(profile/name)!=digest:raise SystemExit('profile header changed: '+name)
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
compiler=compiler_identity(cli.parent)
if compiler!=assembly['identity']['compiler_sha256']:raise SystemExit('compiler does not match qualified object producers')
base=ROOT/'generated/cpu-conformance-managed';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base));out=ROOT/'artifacts/cpu-conformance-managed'/a.name;out.mkdir(parents=True)
r={'kind':'actual-translated-core-cpu-corpus','passed':False,'core_receipt':str(core_path),'core_receipt_sha256':sha(core_path),'assembly_receipt':str(assembly_path),'assembly_receipt_sha256':sha(assembly_path),'profile':str(profile),'compiler':compiler,'replacement':'only authored/managed-driver.c replaced by authored CPU driver; all other objects unchanged','results':{},'comparisons':[]}
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def run(cmd,name,timeout=180):
    start=time.monotonic()
    with(out/(name+'.stdout')).open('wb')as stdout,(out/(name+'.stderr')).open('wb')as stderr:
        code=subprocess.run(list(map(str,cmd)),stdout=stdout,stderr=stderr,timeout=timeout,env=dict(os.environ,LC_ALL='C')).returncode
    r['results'][name]={'command':list(map(str,cmd)),'exit':code,'seconds':time.monotonic()-start,'stdoutSha256':sha(out/(name+'.stdout')),'stderrSha256':sha(out/(name+'.stderr'))};save()
    if code:raise RuntimeError(name+' failed; see '+str(out))
    return(out/(name+'.stdout')).read_text()
def project(path,name,output,sources,references=(),root=False):
    xml=ET.Element('Project',Sdk='Microsoft.NET.Sdk');props=ET.SubElement(xml,'PropertyGroup')
    for key,value in [('TargetFramework','net10.0'),('OutputType',output),('AssemblyName',name),('AllowUnsafeBlocks','true'),('Nullable','disable'),('EnableDefaultCompileItems','false'),('DefineConstants','BLINK_FULL_CORE'),('WarningsAsErrors','CS8500')]:ET.SubElement(props,key).text=value
    items=ET.SubElement(xml,'ItemGroup')
    for source in sources:ET.SubElement(items,'Compile',Include=str(source))
    for reference in references:ET.SubElement(items,'ProjectReference',Include=str(reference))
    if root:ET.SubElement(items,'TrimmerRootAssembly',Include='ManagedCpuCore')
    ET.ElementTree(xml).write(path,encoding='unicode')
try:
    # Fresh native/hardware reference, retaining its complete input identity.
    native_output=run(['python3',ROOT/'tests/CpuConformance/run.py',*(['--observe-differences']if args.observe_differences else[])],'native-corpus')
    native_receipt=Path(native_output.strip().rsplit('receipt ',1)[1]);native=json.loads(native_receipt.read_text())
    if not native.get('completed') or (not native['passed'] and not args.observe_differences):raise RuntimeError('native corpus incomplete or nonconformant')
    r['native_receipt']=str(native_receipt);r['native_receipt_sha256']=sha(native_receipt)
    native_artifacts=native_receipt.parent
    cases=json.loads((native_artifacts/'corpus.stdout').read_text())['cases']
    (a/'source').mkdir();(a/'objects').mkdir()
    for name in ['managed-driver.c','interpreter.c','corpus.h','output.h','Program.cs','features.py']:shutil.copyfile(ROOT/'tests/CpuConformance'/name,a/'source'/name)
    prior=assembly['objects']['authored/managed-driver.c']
    # Use precisely the canonical include snapshot used for the replaced object;
    # validate every original dependency, then copy it to a new immutable input.
    prior_command=prior['command'];canonical=Path(prior_command[prior_command.index('-I')+1])
    for name,digest in prior['emission_identity']['dependencies'].items():
        if sha(canonical/name)!=digest:raise RuntimeError('canonical dependency changed: '+name)
    r['canonical_headers']=str(canonical)
    shutil.copytree(canonical,a/'headers')
    shutil.copytree(profile/'host-project',a/'host')
    (a/'bridges').mkdir()
    bindings=json.loads((profile/'binding-sources.json').read_text())
    for name in bindings['authored_managed']:shutil.copyfile(profile/name,a/'bridges'/Path(name).name)
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['runner_sha256']=sha(Path(__file__));r['replaced_object']=prior
    upstream=ROOT/'ref/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
    cpu_object=a/'objects/cpu-driver.cs'
    command=['dotnet',cli,'-std=c17','-D_GNU_SOURCE','-DNDEBUG','-DNOLINEAR','--emit=obj','-I',canonical,'-I',upstream,'-I',canonical/'authored','-I',canonical/'host','--overrides-file',canonical/'overrides.json',a/'source/managed-driver.c','-o',cpu_object,'--override-report',out/'overrides.jsonl']
    run(command,'cpu-emission',180)
    if compiler_identity(cli.parent)!=compiler:raise RuntimeError('compiler changed during CPU emission')
    for name,digest in prior['emission_identity']['dependencies'].items():
        if sha(canonical/name)!=digest:raise RuntimeError('canonical dependency changed during emission: '+name)
    retained=[];r['retained_objects']={}
    for name in assembly['selected']:
        if name=='authored/managed-driver.c':retained.append(cpu_object);continue
        row=assembly['objects'][name];original=Path(row['object_path'])
        if sha(original)!=row['object_sha256']:raise RuntimeError('retained object changed: '+name)
        target=a/'objects'/('retained-'+str(len(retained))+'.cs');shutil.copyfile(original,target);retained.append(target)
        r['retained_objects'][name]={'sha256':sha(target),'original':str(original),'producer_receipt':row['receipt']}
    r['cpu_object_sha256']=sha(cpu_object)
    raw=a/'raw-generated'
    run(['dotnet',cli,*retained,*assembly['identity']['link_options'],'-o',raw],'cpu-link',180)
    if compiler_identity(cli.parent)!=compiler:raise RuntimeError('compiler changed during CPU link')
    r['raw_generated']={p.name:sha(p)for p in raw.glob('*.cs')}
    shutil.copytree(raw,a/'optimized-generated')
    r['postprocessor']={p.name:sha(p)for p in post.parent.glob('*.dll')};save()
    for label in ['raw','optimized']:
        directory=a/label;directory.mkdir();shutil.copytree(a/'bridges',directory/'bridges')
        library=directory/'ManagedCpuCore.csproj';consumer=directory/'CpuConformance.csproj'
        project(library,'ManagedCpuCore','Library',[a/(label+'-generated')/'*.cs',directory/'bridges/*.cs'],[a/'host/Managed.Emulation.Host.csproj'])
        # Distinct obj dirs avoid two projects overwriting each other's assets.
        consumer_dir=directory/'consumer';consumer_dir.mkdir();consumer=consumer_dir/'CpuConformance.csproj'
        project(consumer,'CpuConformance','Exe',[a/'source/Program.cs'],[library],True)
        if label=='optimized':
            run(['dotnet','restore',library],'optimized-restore')
            run(['dotnet',post,library,'--in-place'],'postprocess',600)
        run(['dotnet','build',consumer,'-c','Release'],label+'-build',600)
        for mode in ['jit','aot']:
            if mode=='aot':
                publish=a/(label+'-aot')
                run(['dotnet','publish',consumer,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],label+'-aot-build',900)
                executable=[publish/'CpuConformance']
            else:executable=['dotnet',consumer_dir/'bin/Release/net10.0/CpuConformance.dll']
            cpuid_rows={};hardware_cpuid={}
            for case in cases:
                i=case['index'];rows=run([*executable,str(i)],f'{label}-{mode}-{i:02}',30).splitlines()
                if len(rows)!=2:raise RuntimeError('unexpected CPU worker output')
                actual=json.loads(rows[0]);owner=json.loads(rows[1]);reference=json.loads((native_artifacts/f'hardware-{i:02}.stdout').read_text());baseline=json.loads((native_artifacts/f'interpreter-{i:02}.stdout').read_text())
                if not(0<owner['ownerMappings'] and 0<owner['ownerBytes']<=64*1024*1024):raise RuntimeError('invalid memory accounting')
                fields=['name','signal','ip','memory']+([]if case['fault']else['ax','cx','dx','xmm','mxcsr'])
                if case['profileReference']:
                    fields.append('bx');cpuid_rows[case['name']]=actual;hardware_cpuid[case['name']]=reference;reference=baseline
                differences=[key for key in fields if actual[key]!=reference[key]]
                mask=int(case['flagMask'],16)
                if (int(actual['flags'],16)^int(reference['flags'],16))&mask:differences.append('definedFlags')
                native_differences=[key for key in fields if actual[key]!=baseline[key]]
                if (int(actual['flags'],16)^int(baseline['flags'],16))&mask:native_differences.append('definedFlags')
                for key in ['halt','completed','rawSignal','rawCode']:
                    if actual[key]!=baseline[key]:differences.append('nativeBlink.'+key)
                r['comparisons'].append({'mode':label+'-'+mode,'case':case['name'],'matched':not differences,'reference':'native-profile-CPUID'if case['profileReference']else'hardware','differences':differences,'nativeDifferences':native_differences,'owner':owner});save()
                if differences and not args.observe_differences:raise RuntimeError('CPU mismatch '+case['name']+' '+str(differences))
            r.setdefault('feature_inventory',{})[label+'-'+mode]=inventory(cpuid_rows,hardware_cpuid);save()
    if r['raw_generated']!={p.name:sha(p)for p in raw.glob('*.cs')}:raise RuntimeError('raw generated source changed')
    for name,digest in r['inputs'].items():
        if sha(a/name)!=digest:raise RuntimeError('frozen input changed: '+name)
    r['optimized_generated']={p.name:sha(p)for p in(a/'optimized-generated').glob('*.cs')}
    r['completed']=True;r['known_failing_rows']=[row for row in r['comparisons']if not row['matched']]
    r['passed']=not r['known_failing_rows'];r['native_agreement']=all(not row['nativeDifferences']for row in r['comparisons'])
    r['binaries']={str(p.relative_to(a)):sha(p)for p in [a/'raw-aot/CpuConformance',a/'optimized-aot/CpuConformance',a/'raw/bin/Release/net10.0/ManagedCpuCore.dll',a/'optimized/bin/Release/net10.0/ManagedCpuCore.dll']}
    save();print(str(len(r['comparisons']))+' actual core comparisons completed; conformance='+str(r['passed'])+'; receipt '+str(out/'receipt.json'))
    if not r['passed'] and not args.observe_differences:raise SystemExit(1)
finally:save()
