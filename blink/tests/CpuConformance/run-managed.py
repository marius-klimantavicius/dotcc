#!/usr/bin/env python3
"""Derive CPU frontend and optional reviewed source corrections; qualify actual core all4."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_SOURCE = _campaign_json.loads((_CAMPAIGN_ROOT / "config/source-manifest.json").read_text())["upstream"]

import argparse,difflib,hashlib,json,os,shutil,subprocess,sys,tempfile,time
from pathlib import Path
import xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[2];REPO=ROOT.parent
sys.path.insert(0,str(ROOT/'scripts'))
from core_inputs import compiler_identity
from semantic_delivery import pin_semantic_delivery
from features import inventory
from selection import select_normal
from contracts import compared_fields, invariants, RDTSC
parser=argparse.ArgumentParser(description=__doc__)
baseline=parser.add_mutually_exclusive_group(required=True)
baseline.add_argument('--core-receipt',type=Path)
baseline.add_argument('--delivery-receipt',type=Path)
parser.add_argument('--delivery-sha256')
parser.add_argument('--observe-differences',action='store_true')
parser.add_argument('--staged-fp',action='store_true')
parser.add_argument('--staged-integer',action='store_true')
args=parser.parse_args()
if bool(args.delivery_receipt)!=bool(args.delivery_sha256):
    parser.error('--delivery-receipt and --delivery-sha256 must be supplied together')
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
baseline_files={};baseline_trees={}
def pin(path,expected=None):
    path=Path(path).resolve();digest=sha(path)
    if expected is not None and digest!=expected:raise SystemExit('baseline identity differs: '+str(path))
    if str(path) in baseline_files and baseline_files[str(path)]!=digest:raise SystemExit('baseline changed: '+str(path))
    baseline_files[str(path)]=digest
    return path
def manifest(path):
    return {str(p.relative_to(path)):sha(p) for p in sorted(path.rglob('*'))
            if p.is_file() and not {'bin','obj'}.intersection(p.relative_to(path).parts)}
def tree(path,expected):
    path=Path(path).resolve()
    if manifest(path)!=expected:raise SystemExit('baseline tree differs: '+str(path))
    baseline_trees[str(path)]=expected
def check_baseline():
    for path,digest in baseline_files.items():
        if sha(path)!=digest:raise RuntimeError('baseline changed during CPU qualification: '+path)
    for path,expected in baseline_trees.items():
        if manifest(Path(path))!=expected:raise RuntimeError('baseline tree changed during CPU qualification: '+path)
if args.core_receipt:
    core_path=pin(args.core_receipt);core=json.loads(core_path.read_text())
    if not core.get('passed') or core.get('diagnostic_replay'):raise SystemExit('requires qualified actual-core execution receipt')
    assembly_path=pin(core['assembly_receipt'],core['assembly_receipt_sha256'])
    baseline_record={'baseline_kind':'qualified-core-execution','core_receipt':str(core_path),'core_receipt_sha256':sha(core_path)}
else:
    delivery_path=pin(args.delivery_receipt,args.delivery_sha256);delivery=json.loads(delivery_path.read_text())
    if (delivery.get('passed') is not True or delivery.get('authored_sources_unchanged') is not True or
            delivery.get('selected_profile')!='threaded' or
            Path(delivery['stable_output']).resolve()!=(ROOT/'generated/TranslatedBlink').resolve()):
        raise SystemExit('requires passing compiled public threaded delivery')
    tree(delivery['stable_output'],delivery['final_files']);tree(delivery['raw_snapshot'],delivery['raw_files'])
    for name,digest in delivery['authored_sources'].items():pin(ROOT/name,digest)
    for name,digest in delivery['compiler'].items():pin(REPO/'DotCC/bin/Release/net10.0'/name,digest)
    for name,digest in delivery['postprocessor'].items():pin(REPO/'DotCC.PostProcess/bin/Release/net10.0'/name,digest)
    for row in delivery['results'].values():
        if row['exit_code']!=0:raise SystemExit('public delivery has failed producer command')
        pin(row['log'],row['log_sha256'])
    assembly_path=pin(delivery['assembly']['path'],delivery['assembly']['sha256'])
    baseline_record={'baseline_kind':'compiled-public-delivery','delivery_receipt':str(delivery_path),
        'delivery_receipt_sha256':args.delivery_sha256,
        'baseline_scope':'Compiled public product only; no CoreExecution execution pass is asserted'}
assembly=json.loads(assembly_path.read_text())
if not assembly['linked'] or assembly['failures']:raise SystemExit('requires linked actual object assembly without failures')
profile=Path(assembly['profile']);profile_inputs=pin(profile/'inputs.json',assembly['identity']['profile_inputs_sha256'])
inputs=json.loads(profile_inputs.read_text())
if args.delivery_receipt:
    if (len(assembly['objects'])!=108 or Path(delivery['profile']).resolve()!=profile.resolve() or
            delivery['profile_inputs_sha256']!=sha(profile_inputs) or inputs['compiler']!=delivery['compiler']):
        raise SystemExit('public product and assembly identities differ')
    for name,row in assembly['objects'].items():
        public=delivery['objects'][name]
        if public['object_path']!=row['object_path'] or public['object_sha256']!=row['object_sha256']:
            raise SystemExit('public object identity differs: '+name)
    pin(ROOT/'scripts/core_inputs.py',assembly['identity']['compiler_identity_script_sha256'])
    baseline_record['semantic_intrinsics']=pin_semantic_delivery(delivery,assembly,pin)
for name,digest in inputs['staged_headers'].items():pin(profile/name,digest)
for row in assembly['objects'].values():
    pin(row['object_path'],row['object_sha256'])
    pin(row.get('canonical_source',row['command'][row['command'].index('-o')-1]),row['emission_identity']['source_sha256'])
cli=REPO/'DotCC/bin/Release/net10.0/dotcc.dll';post=REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
compiler=compiler_identity(cli.parent)
if compiler!=assembly['identity']['compiler_sha256']:raise SystemExit('compiler does not match qualified object producers')
base=ROOT/'generated/cpu-conformance-managed';base.mkdir(parents=True,exist_ok=True)
a=Path(tempfile.mkdtemp(prefix='attempt-',dir=base));out=ROOT/'artifacts/cpu-conformance-managed'/a.name;out.mkdir(parents=True)
r={'kind':'actual-translated-core-cpu-corpus','passed':False,**baseline_record,
   'baseline_files':baseline_files,'baseline_trees':baseline_trees,
   'assembly_receipt':str(assembly_path),'assembly_receipt_sha256':sha(assembly_path),'profile':str(profile),
   'compiler':compiler,'replacement':'CPU frontend only; retained objects checked against the selected baseline','results':{},'comparisons':[]}
def save():(out/'receipt.json').write_text(json.dumps(r,indent=2)+'\n')
def pin_cpu_semantics(report,source):
    if not args.delivery_receipt:return None  # Preserve historical qualified-core runs.
    # The shared public-delivery validator already checked the exact six typed
    # signatures and physical header. Require that same contract for each new
    # CPU producer, with its own actual translation unit and report bytes.
    expected={event['name']:event for event in baseline_record['semantic_intrinsics']['coverage']['blink/syscall.c']['selected']}
    report=pin(report);source=pin(source)
    events=[json.loads(line) for line in report.read_text().splitlines() if line.strip()]
    selected=[event for event in events if event.get('event')=='function-override' and event.get('name') in expected]
    unmatched=[event for event in events if event.get('event')=='function-override-unmatched' and event.get('name') in expected]
    if unmatched or len(selected)!=6 or {event['name'] for event in selected}!=set(expected):
        raise RuntimeError('CPU producer must select all six typed endian helpers: '+str(source))
    for event in selected:
        reference=expected[event['name']]
        if (event['translationUnit']!=str(source) or any(event[key]!=reference[key]
                for key in ('target','signature','declarationFile','matches'))):
            raise RuntimeError('CPU producer endian selection differs: '+str(source))
    from core_inputs import managed_boundary_selection
    boundaries=managed_boundary_selection(profile,report,'authored/cpu-driver.c')
    if boundaries:
        reference={event['name']:event for row in assembly['managed_boundaries']['coverage'].values() for event in row['selected']}
        for event in boundaries['selected']:
            if event['translationUnit']!=str(source) or any(event[key]!=reference[event['name']][key]
                    for key in ('target','signature','declarationFile','matches')):
                raise RuntimeError('CPU producer managed boundary differs: '+str(source))
    return dict(report=str(report),report_sha256=sha(report),source=str(source),source_sha256=sha(source),
                specification_sha256=baseline_record['semantic_intrinsics']['specification_sha256'],selected=selected,
                managed_boundaries=boundaries)
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
implementation_names = ['run.py','run-managed.py','selection.py','contracts.py','hardware.c','hardware-capture.S',
                        'interpreter.c','managed-driver.c','describe.c','corpus.h','fp-cases.h',
                        'make-fp-cases.py','output.h','Program.cs','features.py']
r['implementation']={name:sha(ROOT/'tests/CpuConformance'/name) for name in implementation_names}
try:
    check_baseline()
    # Fresh native/hardware reference, retaining its complete input identity.
    native_output=run(['python3',ROOT/'tests/CpuConformance/run.py',*(['--observe-differences']if args.observe_differences else[]),*(['--staged-fp']if args.staged_fp else[]),*(['--staged-integer']if args.staged_integer else[])],'native-corpus')
    native_receipt=Path(native_output.strip().rsplit('receipt ',1)[1]);native=json.loads(native_receipt.read_text())
    if not native.get('completed') or (not native['passed'] and not args.observe_differences):raise RuntimeError('native corpus incomplete or nonconformant')
    r['native_receipt']=str(native_receipt);r['native_receipt_sha256']=sha(native_receipt)
    native_artifacts=native_receipt.parent
    corpus=json.loads((native_artifacts/'corpus.stdout').read_text())
    cases,selection=select_normal(corpus,ROOT/'tests/CpuConformance')
    if native.get('selection')!=selection:raise RuntimeError('Native normal selection differs')
    if [row['case'] for row in native['comparisons']]!=[row['name'] for row in cases]:raise RuntimeError('Native comparison coverage differs')
    r['selection']=selection
    (a/'source').mkdir();(a/'objects').mkdir()
    for name in ['managed-driver.c','selection.py','contracts.py','interpreter.c','corpus.h','output.h','Program.cs','features.py','fp-cases.h','make-fp-cases.py']:shutil.copyfile(ROOT/'tests/CpuConformance'/name,a/'source'/name)
    adding_frontend='authored/managed-driver.c' not in assembly['objects']
    template_name='blink/syscall.c' if adding_frontend else 'authored/managed-driver.c'
    prior=assembly['objects'][template_name]
    r['frontend_action']='added' if adding_frontend else 'replaced'
    r['frontend_template']=template_name
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
    for name in bindings['authored_managed']:
        if Path(name).name=='HostGuestSignalsBridge.cs':
            r['omitted_product_callback']={'path':name,'sha256':sha(profile/name)}
            continue
        shutil.copyfile(profile/name,a/'bridges'/Path(name).name)
    source_replacements=[]
    # The reference policy must equal the CPUID producer retained in this exact profile.
    cpuid_source=Path(assembly['objects']['blink/cpuid.c']['command'][assembly['objects']['blink/cpuid.c']['command'].index('-o')-1])
    cpuid_stage=json.loads((native_artifacts/'cpuid-stage.json').read_text())
    cpuid_bytes=cpuid_source.read_bytes();prefix=b'#include "host-bindings.h"\n'
    if sha(cpuid_source)!=assembly['objects']['blink/cpuid.c']['emission_identity']['source_sha256'] or not cpuid_bytes.startswith(prefix) or hashlib.sha256(cpuid_bytes[len(prefix):]).hexdigest()!=cpuid_stage['stagedSha256']:
        raise RuntimeError('native CPUID policy differs from qualified canonical producer')
    r['cpuid_policy']=cpuid_stage
    for enabled, family, directory, receipt_key in [
        (args.staged_fp, 'fp', 'UpstreamScalarFp', 'scalar_fp_stage'),
        (args.staged_integer, 'integer', 'UpstreamInteger', 'integer_stage'),
    ]:
        if not enabled:continue
        stage_name='scalar-fp' if family=='fp' else 'integer'
        run(['python3',ROOT/'src'/directory/'stage.py','--output',a/stage_name,'--receipt',out/(stage_name+'-stage.json')],'stage-'+stage_name)
        staged=json.loads((out/(stage_name+'-stage.json')).read_text())
        if staged!=native[receipt_key]:raise RuntimeError('native and managed staged '+family+' inputs differ')
        r[receipt_key]=staged
        prepared_dir=a/('prepared-'+family);prepared_dir.mkdir()
        prepared_key='prepared_'+family+'_sources';r[prepared_key]={}
        for filename in staged['sources']:
            old=assembly['objects']['blink/'+filename]
            old_source=Path(old['command'][old['command'].index('-o')-1])
            immutable=ROOT/("ref/" + _CAMPAIGN_SOURCE["directory"] + '/blink')/filename
            if sha(old_source)!=old['emission_identity']['source_sha256']:
                raise RuntimeError(family+' producer source changed: '+filename)
            corrected=prefix+(a/stage_name/filename).read_bytes()
            if old_source.read_bytes()==corrected:
                boundary=profile/(stage_name+'-boundary.json')
                if not boundary.exists() or json.loads(boundary.read_text())!=staged:
                    raise RuntimeError('canonical '+family+' correction provenance differs')
                r.setdefault('reused_reviewed_'+family,{})[filename]={'source_sha256':sha(old_source),'object_sha256':old['object_sha256'],'boundary_sha256':sha(boundary)}
                continue
            if family=='integer':
                raise RuntimeError('integer correction requires an exactly matching qualified canonical producer: '+filename)
            if old_source.read_bytes()!=prefix+immutable.read_bytes():
                raise RuntimeError('original '+family+' source preamble differs: '+filename)
            prepared=prepared_dir/filename
            prepared.write_bytes(corrected);source_replacements.append((family,filename,prepared))
            r[prepared_key][filename]={'original_source':str(old_source),'original_sha256':sha(old_source),'prefix_sha256':hashlib.sha256(prefix).hexdigest(),'prepared_sha256':sha(prepared)}
        r['replacement']=('add' if adding_frontend else 'replace')+' authored CPU driver; explicitly selected reviewed scalar/integer objects reused when identical to qualified canonical producers, otherwise explicitly replaced'
    r['inputs']={str(p.relative_to(a)):sha(p)for p in a.rglob('*')if p.is_file()}
    r['runner_sha256']=sha(Path(__file__));r['frontend_template_object']=prior
    if not adding_frontend:r['replaced_object']=prior
    upstream=ROOT/("ref/" + _CAMPAIGN_SOURCE["directory"])
    cpu_object=a/'objects/cpu-driver.cs'
    command=list(prior['command'])
    old_source=prior.get('canonical_source', command[command.index('-o')-1])
    command[command.index(str(old_source))]=str(a/'source/managed-driver.c')
    command[command.index('-o')+1]=str(cpu_object)
    if '--override-report' in command:
        command[command.index('--override-report')+1]=str(out/'overrides.jsonl')
    command[3:3]=['-I',str(a/'source')]
    run(command,'cpu-emission',180)
    if args.delivery_receipt:
        r['cpu_frontend_semantic_intrinsics']=pin_cpu_semantics(out/'overrides.jsonl',a/'source/managed-driver.c')
        save()
    if compiler_identity(cli.parent)!=compiler:raise RuntimeError('compiler changed during CPU emission')
    for name,digest in prior['emission_identity']['dependencies'].items():
        if sha(canonical/name)!=digest:raise RuntimeError('canonical dependency changed during emission: '+name)
    replacements={'authored/managed-driver.c':cpu_object}
    r['replaced_objects']={} if adding_frontend else {'authored/managed-driver.c':prior}
    if adding_frontend:r['added_objects']={'authored/managed-driver.c':str(cpu_object)}
    for family,filename,prepared in source_replacements:
        name='blink/'+filename;old=assembly['objects'][name];old_command=old['command']
        source_canonical=Path(old_command[old_command.index('-I')+1])
        if source_canonical!=canonical:raise RuntimeError(family+' source header identity differs')
        for dependency,digest in old['emission_identity']['dependencies'].items():
            if sha(source_canonical/dependency)!=digest:raise RuntimeError(family+' canonical dependency changed')
        obj=a/'objects'/('staged-'+filename[:-2]+'.cs')
        cmd=old_command.copy();cmd[cmd.index('-o')-1]=str(prepared);cmd[cmd.index('-o')+1]=str(obj)
        cmd[cmd.index('--override-report')+1]=str(out/(filename+'.overrides.jsonl'))
        run(cmd,'emission-'+filename,180)
        if args.delivery_receipt:
            r.setdefault('replacement_semantic_intrinsics',{})[name]=pin_cpu_semantics(out/(filename+'.overrides.jsonl'),prepared)
            save()
        replacements[name]=obj;r['replaced_objects'][name]=old
    if compiler_identity(cli.parent)!=compiler:raise RuntimeError('compiler changed during staged emission')
    r['replacement_objects']={name:sha(obj)for name,obj in replacements.items()}
    retained=[];r['retained_objects']={}
    for name in assembly['selected']:
        if name in replacements:retained.append(replacements[name]);continue
        row=assembly['objects'][name];original=Path(row['object_path'])
        if sha(original)!=row['object_sha256']:raise RuntimeError('retained object changed: '+name)
        target=a/'objects'/('retained-'+str(len(retained))+'.cs');shutil.copyfile(original,target);retained.append(target)
        r['retained_objects'][name]={'sha256':sha(target),'original':str(original),'producer_receipt':row['receipt']}
    if adding_frontend:retained.append(cpu_object)
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
                actual=json.loads(rows[0]);owner=json.loads(rows[1]);reference=json.loads((native_artifacts/f'hardware-{i:02}.stdout').read_text());baseline=json.loads((native_artifacts/f'{"staged"if args.staged_fp or args.staged_integer else"interpreter"}-{i:02}.stdout').read_text())
                if not(0<owner['ownerMappings'] and 0<owner['ownerBytes']<=64*1024*1024):raise RuntimeError('invalid memory accounting')
                fields=['name','signal','ip','memory','ax','cx','dx','xmm','mxcsr']
                if case['profileReference']:
                    fields.append('bx');cpuid_rows[case['name']]=actual;hardware_cpuid[case['name']]=reference;reference=baseline
                fields=compared_fields(case,fields)
                actual_invariants=invariants(case,actual,corpus)
                if case['name']==RDTSC:
                    r.setdefault('rdtsc_invariants',{})[label+'-'+mode]=actual_invariants
                differences=[key for key in fields if actual[key]!=reference[key]]+actual_invariants
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
    for name,digest in r['implementation'].items():
        if sha(ROOT/'tests/CpuConformance'/name)!=digest:raise RuntimeError('CPU implementation changed: '+name)
    expected_coverage=[(mode,case['name']) for mode in ['raw-jit','raw-aot','optimized-jit','optimized-aot'] for case in cases]
    if [(row['mode'],row['case']) for row in r['comparisons']]!=expected_coverage:raise RuntimeError('Managed selected coverage differs')
    r['expected_comparisons']=len(expected_coverage)
    r['optimized_generated']={p.name:sha(p)for p in(a/'optimized-generated').glob('*.cs')}
    check_baseline();r['baseline_identities_stable']=True
    r['completed']=True;r['known_failing_rows']=[row for row in r['comparisons']if not row['matched']]
    r['passed']=not r['known_failing_rows'];r['native_agreement']=all(not row['nativeDifferences']for row in r['comparisons'])
    r['binaries']={str(p.relative_to(a)):sha(p)for p in [a/'raw-aot/CpuConformance',a/'optimized-aot/CpuConformance',a/'raw/bin/Release/net10.0/ManagedCpuCore.dll',a/'optimized/bin/Release/net10.0/ManagedCpuCore.dll']}
    save();print(str(len(r['comparisons']))+' actual core comparisons completed; conformance='+str(r['passed'])+'; receipt '+str(out/'receipt.json'))
    if not r['passed'] and not args.observe_differences:raise SystemExit(1)
finally:save()
