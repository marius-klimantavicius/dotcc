#!/usr/bin/env python3
"""Fixed-loop ExecuteInstruction throughput; correctness is a separate hard gate."""
import argparse, hashlib, json, os, platform, shutil, statistics, subprocess, sys, tempfile, time
from pathlib import Path
import xml.etree.ElementTree as ET
ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sys.path.insert(0, str(ROOT / 'scripts'))
from core_inputs import compiler_identity
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
MODES = {'native', 'raw-jit', 'raw-aot', 'optimized-jit', 'optimized-aot'}
TUNING = ['TieredCompilation', 'TieredPGO', 'TC_QuickJit', 'TC_QuickJitForLoops',
          'TC_CallCountThreshold', 'TC_CallCountingDelayMs', 'ReadyToRun',
          'gcServer', 'gcConcurrent', 'GCHeapHardLimit', 'GCHeapHardLimitPercent',
          'GCLatencyLevel', 'GCConserveMemory', 'GCRetainVM', 'GCHeapCount',
          'GCAffinitizeMask', 'JitMinOpts', 'JitStress', 'JitStressRegs']
def runtime_context():
    dotnet = Path(shutil.which('dotnet')).resolve()
    installed = subprocess.check_output([str(dotnet), '--list-runtimes'], text=True)
    files = {str(dotnet): sha(dotnet)}
    for row in installed.splitlines():
        framework, version, location = row.split(' ', 2)
        if framework != 'Microsoft.NETCore.App': continue
        directory = Path(location.strip('[]')) / version
        for item in directory.iterdir():
            if item.is_file() and item.suffix in {'.dll', '.so', '.json'}:
                files[str(item)] = sha(item)
    return {'dotnet': str(dotnet), 'installed_runtimes': installed, 'binary_sha256': files,
            'tuning_allowlist': {prefix + name: os.environ.get(prefix + name)
                                for prefix in ['DOTNET_', 'COMPlus_'] for name in TUNING},
            'culture_override': 'LC_ALL=C'}
def expected_executables(directory):
    commands = {'native': [str(directory/'native')]}
    for variant in ['raw', 'optimized']:
        commands[variant+'-jit'] = ['dotnet', str(directory/variant/'consumer/bin/Release/net10.0/CoreThroughput.dll')]
        commands[variant+'-aot'] = [str(directory/variant/'publish/CoreThroughput')]
    return commands

def frozen_artifacts(directory):
    return {str(item.relative_to(directory)): sha(item) for item in directory.rglob('*')
            if item.is_file() and 'obj' not in item.relative_to(directory).parts}

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--core-receipt', type=Path)
p.add_argument('--prepare-only', action='store_true')
p.add_argument('--measure-existing', type=Path)
p.add_argument('--native-only', action='store_true')
p.add_argument('--iterations', type=int, default=250000)
p.add_argument('--samples', type=int, default=5)
p.add_argument('--processes', type=int, default=3)
p.add_argument('--cpu', type=int, default=max(os.sched_getaffinity(0)))
args = p.parse_args()
if args.native_only and args.prepare_only: p.error('Native-only preparation cannot qualify a five-mode measurement')
existing = None
if args.measure_existing:
    existing = json.loads(args.measure_existing.read_text())
    if not existing.get('ready_for_measurement') or existing.get('passed'): p.error('Requires an unmeasured prepared receipt')
    if set(existing.get('executables', {})) != MODES or set(existing.get('semantic_preflight', {})) != MODES:
        p.error('Preparation requires exactly all five executable/preflight modes')
    args.core_receipt = Path(existing['core_receipt'])
    args.iterations = existing['iterations']; args.samples = existing['samples_per_process']; args.processes = existing['processes_per_mode']; args.cpu = existing['cpu']
    if args.prepare_only or args.native_only: p.error('Measurement cannot also prepare or run native-only')
if not args.core_receipt: p.error('--core-receipt or --measure-existing required')
if not (1 <= args.iterations <= 1000000 and 1 <= args.samples <= 20 and 1 <= args.processes <= 10):
    p.error('iterations 1..1000000, samples 1..20, processes 1..10')
if args.cpu not in os.sched_getaffinity(0): p.error('CPU is outside allowed affinity')
core_path = args.core_receipt.resolve(); core = json.loads(core_path.read_text())
if not core.get('passed') or core.get('diagnostic_replay'): raise RuntimeError('Requires qualified actual-core receipt')
assembly_path = Path(core['assembly_receipt']); assembly = json.loads(assembly_path.read_text())
if sha(assembly_path) != core['assembly_receipt_sha256'] or not assembly['linked']: raise RuntimeError('Assembly drift')
profile = Path(assembly['profile']); inputs = json.loads((profile / 'inputs.json').read_text())
if sha(profile / 'inputs.json') != assembly['identity']['profile_inputs_sha256']: raise RuntimeError('Profile drift')
cli = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
post = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
compiler = compiler_identity(cli.parent)
if compiler != assembly['identity']['compiler_sha256']: raise RuntimeError('Compiler differs from qualified producers')
for name, digest in inputs['staged_headers'].items():
    if sha(profile / name) != digest: raise RuntimeError('Changed frozen header: ' + name)
base = ROOT / 'generated/core-throughput'; base.mkdir(parents=True, exist_ok=True)
a = Path(existing['build']) if existing else Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/core-throughput' / a.name; out.mkdir(parents=True, exist_ok=True)
r = dict(kind='hot integer-loop actual interpreter throughput', passed=False, native_passed=False,
         core_receipt=str(core_path), core_sha256=sha(core_path), assembly_receipt=str(assembly_path),
         assembly_sha256=sha(assembly_path), profile=str(profile), profile_inputs_sha256=sha(profile/'inputs.json'), compiler=compiler,
         iterations=args.iterations, instructions_per_sample=4 * args.iterations,
         samples_per_process=args.samples, processes_per_mode=args.processes, warmup_instructions=40000,
         loop_hex='48ffc048010348ffc975f5', cpu=args.cpu, allowed_affinity=sorted(os.sched_getaffinity(0)),
         platform=platform.platform(), runner_sha256=sha(Path(__file__)), build=str(a), commands={}, measurements={}, summaries={})
if existing:
    if existing['compiler'] != compiler or existing['runner_sha256'] != sha(Path(__file__)): raise RuntimeError('Prepared tool/runner identity changed')
    for key in ['core_receipt', 'core_sha256', 'assembly_receipt', 'assembly_sha256', 'profile', 'profile_inputs_sha256']:
        if existing.get(key) != r[key]: raise RuntimeError('Prepared provenance chain changed: ' + key)
    if existing['executables'] != expected_executables(a): raise RuntimeError('Prepared executable paths differ from frozen build')
    r = existing
def save(): (out / 'receipt.json').write_text(json.dumps(r, indent=2) + '\n')
def run(command, name, timeout=300):
    command = list(map(str, command)); start = time.monotonic()
    with (out / (name + '.stdout')).open('wb') as stdout, (out / (name + '.stderr')).open('wb') as stderr:
        result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=timeout,
                                env=dict(os.environ, LC_ALL='C'))
    r['commands'][name] = dict(argv=command, seconds=time.monotonic() - start, exit=result.returncode,
                              stdout_sha256=sha(out / (name + '.stdout')), stderr_sha256=sha(out / (name + '.stderr')))
    save()
    if result.returncode: raise RuntimeError(name + ' failed; see ' + str(out))
    return (out / (name + '.stdout')).read_text()
def measure(label, command):
    r.setdefault('executables', {})[label] = list(map(str, command))
    if args.prepare_only:
        text = run(['taskset', '-c', str(args.cpu), *command, '1000', '1'], label + '-semantic', 60)
        rows = [json.loads(line) for line in text.splitlines()]
        if len(rows) != 1 or not rows[0]['verified'] or rows[0]['ax'] != 1000 or rows[0]['cx'] != 0 or rows[0]['sum'] != 500500 or rows[0]['flags'] != 0x44 or rows[0]['instructions'] != 4000 or rows[0]['ip'] != 0x40000b:
            raise RuntimeError('Preparation semantic mismatch: ' + label)
        r.setdefault('semantic_preflight', {})[label] = rows[0]
        print(label + ': semantic preflight passed (not a final throughput measurement)', flush=True)
        save(); return
    values = []
    for process in range(args.processes):
        name = label + '-' + str(process)
        before_load = os.getloadavg()
        text = run(['taskset', '-c', str(args.cpu), *command, str(args.iterations), str(args.samples)], name, 300)
        rows = [json.loads(line) for line in text.splitlines()]
        if len(rows) != args.samples: raise RuntimeError('Wrong sample count: ' + name)
        expected = dict(iterations=args.iterations, instructions=4 * args.iterations, ax=args.iterations,
                        cx=0, ip=0x40000b, sum=args.iterations * (args.iterations + 1) // 2,
                        flags=0x44, verified=True)
        for index, row in enumerate(rows):
            if row['sample'] != index or any(row[k] != v for k, v in expected.items()):
                raise RuntimeError('Semantic mismatch: ' + name)
            if row['ticks'] <= 0 or row['frequency'] <= 0: raise RuntimeError('Invalid clock observation')
            row['seconds'] = row['ticks'] / row['frequency']
            row['instructions_per_second'] = row['instructions'] / row['seconds']
            values.append(row['instructions_per_second'])
        r['measurements'][name] = dict(samples=rows, process_wall_seconds=r['commands'][name]['seconds'],
                                        load_before=before_load, load_after=os.getloadavg())
        save()
    r['summaries'][label] = dict(samples=len(values), median_ips=statistics.median(values),
                                  min_ips=min(values), max_ips=max(values), mean_ips=statistics.mean(values),
                                  stdev_ips=statistics.stdev(values) if len(values)>1 else 0)
    print(label + ': semantic checks passed; median ' + str(round(statistics.median(values))) + ' instructions/s', flush=True)
    save()
def project(path, name, kind, sources, refs, root=False):
    tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk'); props = ET.SubElement(tree, 'PropertyGroup')
    for key,value in dict(TargetFramework='net10.0',AssemblyName=name,OutputType=kind,AllowUnsafeBlocks='true',Nullable='disable',EnableDefaultCompileItems='false',DefineConstants='BLINK_FULL_CORE',WarningsAsErrors='CS8500').items():
        ET.SubElement(props,key).text=value
    items = ET.SubElement(tree,'ItemGroup')
    for source in sources: ET.SubElement(items,'Compile',Include=str(source))
    for ref in refs: ET.SubElement(items,'ProjectReference',Include=str(ref))
    if root: ET.SubElement(items,'TrimmerRootAssembly',Include='ThroughputCore')
    ET.ElementTree(tree).write(path,encoding='unicode')
try:
    context = runtime_context()
    if args.measure_existing:
        if r['runtime_preparation'] != context: raise RuntimeError('Runtime binaries/tuning changed since preparation')
        r['runtime_measurement'] = context
        if frozen_artifacts(a) != r['frozen_artifacts']: raise RuntimeError('Prepared artifact set changed')
        for name, digest in r['frozen_artifacts'].items():
            if sha(a/name) != digest: raise RuntimeError('Prepared artifact drift: '+name)
        for label, command in list(r['executables'].items()): measure(label, command)
        if set(r['summaries']) != MODES: raise RuntimeError('Incomplete five-mode measurement')
        if frozen_artifacts(a) != r['frozen_artifacts']: raise RuntimeError('Prepared artifacts changed during measurement')
        if runtime_context() != context: raise RuntimeError('Runtime context changed during measurement')
        r['passed'] = True
    else:
        r['runtime_preparation'] = context
        (a/'source').mkdir(); (a/'objects').mkdir()
        for name in ['probe.c','Program.cs','Clock.cs']: shutil.copyfile(ROOT/'tests/CoreThroughput'/name,a/'source'/name)
        r['authored_inputs']={p.name:sha(p) for p in (a/'source').iterdir()}
        for name in ['cpuinfo','meminfo']:
            if Path('/proc',name).exists(): shutil.copyfile(Path('/proc',name),out/(name+'.txt'))
        run(['dotnet','--info'],'dotnet-environment',30)
        cc=Path(shutil.which('cc')).resolve(); r['native_compiler']={'path':str(cc),'sha256':sha(cc)}
        run([cc,'--version'],'cc-environment',30)
        r['postprocessor']={p.name:sha(p) for p in post.parent.iterdir() if p.suffix in ['.dll','.json']}
        native=ROOT/'build/native/source'; archive=native/'o/blink/blink.a'
        closure=json.loads((profile/'closure.json').read_text())
        if sha(archive)!=closure['native_archive_sha256'] or sha(native/'config.h')!=closure['native_config_sha256']:
            raise RuntimeError('Original native archive/config changed')
        shutil.copyfile(archive,a/'blink.a'); shutil.copyfile(native/'config.h',a/'config.h')
        (a/'blink').mkdir()
        for source in list((native/'blink').glob('*.h'))+list((native/'blink').glob('*.inc')):
            shutil.copyfile(source,a/'blink'/source.name)
        r['native_inputs']={str(p.relative_to(a)):sha(p) for p in [a/'blink.a',a/'config.h',*(a/'blink').iterdir()]}
        run([cc,'-std=c17','-O2','-D_GNU_SOURCE','-D_DEFAULT_SOURCE','-DNOLINEAR','-I',a,a/'source/probe.c',a/'blink.a',
             '-lz','-lrt','-lm','-pthread','-o',a/'native'],'native-build',60)
        r['native_binary_sha256']=sha(a/'native')
        measure('native',[a/'native']); r['native_passed']=True; save()
        if not args.native_only:
            prior=assembly['objects']['authored/managed-driver.c']; old=prior['command']; canonical=Path(old[old.index('-I')+1])
            for name,digest in prior['emission_identity']['dependencies'].items():
                if sha(canonical/name)!=digest: raise RuntimeError('Canonical header changed: '+name)
            r['canonical_headers']=str(canonical); r['replaced_object']=prior
            command=old.copy(); command[command.index(prior['canonical_source'])]=str(a/'source/probe.c')
            command[command.index('-o')+1]=str(a/'objects/throughput.cs')
            command[3:3]=['-DTHROUGHPUT_MANAGED']
            if '--override-report' in command: command[command.index('--override-report')+1]=str(out/'overrides.jsonl')
            run(command,'frontend-emission',180)
            objects=[]; r['retained_objects']={}
            for name,row in assembly['objects'].items():
                if name=='authored/managed-driver.c': objects.append(a/'objects/throughput.cs');continue
                source=Path(row['object_path'])
                if sha(source)!=row['object_sha256']: raise RuntimeError('Retained object drift: '+name)
                target=a/'objects'/('retained-'+str(len(objects))+'.cs'); shutil.copyfile(source,target); objects.append(target)
                r['retained_objects'][name]={'path':str(source),'sha256':sha(target),'producer':row['receipt']}
            r['frontend_object_sha256']=sha(a/'objects/throughput.cs')
            run(['dotnet',cli,*objects,*assembly['identity']['link_options'],'-o',a/'raw-generated'],'link',180)
            r['raw_generated']={p.name:sha(p) for p in (a/'raw-generated').glob('*.cs')}
            shutil.copytree(a/'raw-generated',a/'optimized-generated')
            shutil.copytree(profile/'host-project',a/'host'); (a/'bridges').mkdir()
            for name in json.loads((profile/'binding-sources.json').read_text())['authored_managed']:
                shutil.copyfile(profile/name,a/'bridges'/Path(name).name)
            r['consumer_inputs']={str(p.relative_to(a)):sha(p) for directory in [a/'host',a/'bridges'] for p in directory.rglob('*') if p.is_file()}
            for variant in ['raw','optimized']:
                folder=a/variant; folder.mkdir(); consumer_dir=folder/'consumer'; consumer_dir.mkdir()
                library=folder/'ThroughputCore.csproj'; consumer=consumer_dir/'CoreThroughput.csproj'
                project(library,'ThroughputCore','Library',[a/(variant+'-generated')/'*.cs',a/'bridges/*.cs',a/'source/Clock.cs'],[a/'host/Managed.Emulation.Host.csproj'])
                project(consumer,'CoreThroughput','Exe',[a/'source/Program.cs'],[library],True)
                if variant=='optimized':
                    run(['dotnet','restore',library],'optimized-restore')
                    run(['dotnet',post,library,'--in-place'],'postprocess',600)
                run(['dotnet','build',consumer,'-c','Release'],variant+'-build',600)
                jit=consumer_dir/'bin/Release/net10.0/CoreThroughput.dll'
                measure(variant+'-jit',['dotnet',jit])
                publish=folder/'publish'
                run(['dotnet','publish',consumer,'-c','Release','-r','linux-x64','-p:PublishAot=true','-o',publish],variant+'-publish',900)
                measure(variant+'-aot',[publish/'CoreThroughput'])
                r.setdefault('binary_hashes',{}).update({str(p.relative_to(a)):sha(p) for directory in [jit.parent,publish] for p in directory.iterdir() if p.is_file()})
            if r['raw_generated']!={p.name:sha(p) for p in (a/'raw-generated').glob('*.cs')}: raise RuntimeError('Raw generated source changed')
            r['optimized_generated']={p.name:sha(p) for p in (a/'optimized-generated').glob('*.cs')}
            r['passed']=not args.prepare_only
    if compiler_identity(cli.parent)!=compiler: raise RuntimeError('Compiler changed')
    if args.prepare_only:
        if r.get('executables') != expected_executables(a) or set(r.get('semantic_preflight', {})) != MODES:
            raise RuntimeError('Preparation lacks exact five executable/preflight modes')
        if runtime_context() != context: raise RuntimeError('Runtime context changed during preparation')
        r['frozen_artifacts']=frozen_artifacts(a)
        r['ready_for_measurement']=True
    r['completed']=not args.prepare_only
    r['limitations']=['One fixed hot integer loop; not general emulator throughput or full ISA coverage.',
                      'Timing includes ExecuteInstruction call/loop overhead and runtime effects; excludes setup/reset/validation/output.',
                      'Samples follow fixed warmup, but no claim that tiered compilation or OS scheduling has reached a steady state.',
                      'Other qualification work may run concurrently; affinity/load and per-process/sample variation are retained.',
                      'Native interpreter uses the untouched pinned archive; managed core retains explicitly reviewed profile adapters.',
                      'Frontend-only derived linkage, not fresh translation of every retained source.']
    save(); print('receipt '+str(out/'receipt.json'))
except BaseException as error:
    r['passed'] = False
    r['completed'] = False
    r['error'] = str(error)
    raise
finally: save()
