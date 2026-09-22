#!/usr/bin/env python3
"""Translate the frozen selected Blink core into the stable owning-host library."""
import argparse
import fcntl
import hashlib
import json
import os
import re
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET
from core_inputs import compiler_identity, profile_sources

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--offline', action='store_true', help='Require the pinned Blink archive in ref; NuGet restore remains normal')
parser.add_argument('--profile', choices=('threaded', 'single-thread'), default='threaded',
                    help='Selected product profile; threaded supports the qualified NativeAOT guest prerequisites')
parser.add_argument('--jobs', type=int, choices=range(1, 5), default=4)
parser.add_argument('--timeout', type=int, default=300, help='Per translation-unit timeout in seconds')
args = parser.parse_args()
def terminate(signum, frame):
    # Let run() drain its independently grouped child when an outer runner stops us.
    raise InterruptedError('Translation received SIGTERM')
signal.signal(signal.SIGTERM, terminate)
if not 1 <= args.timeout <= 1800: parser.error('--timeout must be 1..1800')
generated = ROOT / 'generated'
generated.mkdir(exist_ok=True)
lock = (generated / '.translate.lock').open('a')
try:
    fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
except BlockingIOError:
    raise SystemExit('Another Blink translation owns the delivery lock')
base = generated / 'translation-delivery'; base.mkdir(exist_ok=True)
attempt = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
out = ROOT / 'artifacts/translation' / attempt.name; out.mkdir(parents=True)
tmp = out / 'tmp'; tmp.mkdir()
raw = attempt / 'raw'
candidate = attempt / 'postprocessed'
stable = generated / 'TranslatedBlink'
publication_committed = False
receipt = {'kind': 'complete-core-translation-delivery', 'passed': False,
           'attempt': str(attempt), 'stable_output': str(stable), 'raw_snapshot': str(raw),
           'results': {}, 'scope': 'Translation, semantic post-processing and project builds only; no guest execution or service claim',
           'runner_sha256': sha(Path(__file__)), 'wrapper_sha256': sha(ROOT/'scripts/translate.sh'),
           'selected_profile': args.profile,
           'temporary_directory': str(tmp)}
def save():
    target = out/'receipt.tmp'; target.write_text(json.dumps(receipt, indent=2)+'\n')
    target.replace(out/'receipt.json')
def manifest(directory):
    return {str(p.relative_to(directory)):sha(p) for p in sorted(directory.rglob('*'))
            if p.is_file() and not any(part in {'bin','obj'} for part in p.relative_to(directory).parts)}
def tool_identity(directory):
    return {p.name:sha(p) for p in sorted(directory.iterdir())
            if p.is_file() and p.suffix in {'.dll','.json'}}
def stop_group(process):
    # Children include compiler/AOT processes; terminate the complete owned group.
    try: os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError: return
    try: process.wait(timeout=3)
    except subprocess.TimeoutExpired: pass
    try: os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError: pass
    process.wait()
def run(command, label, timeout=600):
    command = list(map(str, command)); start = time.monotonic(); code = None
    log = out/(label+'.log'); process = None
    try:
        with log.open('wb') as stream:
            process = subprocess.Popen(command, cwd=REPO, stdout=stream, stderr=subprocess.STDOUT,
                                       env=dict(os.environ, LC_ALL='C', TMPDIR=str(tmp)), start_new_session=True)
            try: code = process.wait(timeout=timeout)
            except BaseException:
                stop_group(process); raise
    finally:
        receipt['results'][label] = {'command': command, 'cwd': str(REPO), 'exit_code': code,
                                     'seconds': time.monotonic()-start,
                                     'log': str(log), 'log_sha256': sha(log) if log.exists() else None}
        save()
    if code: raise RuntimeError(label+' failed; see '+str(log))
    return log.read_text()
def project(path, authored_sources=None):
    tree = ET.Element('Project', Sdk='Microsoft.NET.Sdk'); props = ET.SubElement(tree,'PropertyGroup')
    for key,value in [('TargetFramework','net10.0'),('LangVersion','14'),('OutputType','Library'),
                      ('AssemblyName','TranslatedBlink'),('RootNamespace','Managed.Emulation'),
                      ('AllowUnsafeBlocks','true'),('Nullable','disable'),('ImplicitUsings','enable'),
                      ('EnableDefaultCompileItems','false'),('DefineConstants','$(DefineConstants);BLINK_FULL_CORE'),
                      ('WarningsAsErrors','$(WarningsAsErrors);CS8500')]:
        ET.SubElement(props,key).text=value
    items = ET.SubElement(tree,'ItemGroup')
    ET.SubElement(items,'Compile',Include='Sources/**/*.cs')
    if authored_sources is None:
        ET.SubElement(items,'Compile',Include='Bridges/*.cs')
        ET.SubElement(items,'ProjectReference',Include='Host/Managed.Emulation.Host.csproj')
    else:
        # These paths are relative to the final generated/TranslatedBlink
        # directory, not this deeper private postprocessing directory.
        for source in authored_sources:
            item=ET.SubElement(items,'Compile',Include='../../'+source)
            ET.SubElement(item,'Link').text='Bindings/'+Path(source).name
        ET.SubElement(items,'ProjectReference',Include='../../src/Managed.Emulation.Host/Managed.Emulation.Host.csproj')
    ET.indent(tree); ET.ElementTree(tree).write(path,encoding='unicode')
def verify_profile(profile, inputs):
    for name,digest in inputs['staged_headers'].items():
        if sha(profile/name)!=digest: raise RuntimeError('Frozen profile changed: '+name)
def verify_authored():
    for name,digest in receipt.get('authored_sources',{}).items():
        if sha(ROOT/name)!=digest:
            raise RuntimeError('Authored product source changed: '+name)
def verify_tools():
    if compiler_identity(cli.parent)!=receipt['compiler']: raise RuntimeError('Compiler changed during translation')
    if tool_identity(post.parent)!=receipt['postprocessor']: raise RuntimeError('Postprocessor changed during translation')
    for name,digest in receipt['scripts'].items():
        if sha(ROOT/name)!=digest: raise RuntimeError('Pipeline script changed: '+name)
try:
    cli = REPO/'DotCC/bin/Release/net10.0/dotcc.dll'
    post = REPO/'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    if not cli.is_file() or not post.is_file():
        raise RuntimeError('Build dotcc.sln -c Release -p:UseLocalLalrCc=false before translation')
    receipt['compiler'] = compiler_identity(cli.parent)
    receipt['postprocessor'] = tool_identity(post.parent)
    receipt['scripts'] = {str(p.relative_to(ROOT)):sha(p) for p in
        [Path(__file__), ROOT/'scripts/translate.sh', ROOT/'scripts/fetch.sh', ROOT/'scripts/native-oracle.sh',
         ROOT/'scripts/probe-core.sh', ROOT/'scripts/assemble-core.py', ROOT/'scripts/isolate-core.py',
         ROOT/'scripts/core_inputs.py', ROOT/'scripts/stage-host-bindings.py', ROOT/'scripts/stage-threaded-core.py']}
    receipt['git_head'] = run(['git','rev-parse','HEAD'],'git-head',30).strip()
    receipt['git_status'] = run(['git','status','--short'],'git-status',30)
    receipt['configuration']={str(p.relative_to(ROOT)):sha(p) for p in sorted((ROOT/'config').rglob('*')) if p.is_file()}
    source_manifest=json.loads((ROOT/'config/source-manifest.json').read_text())
    receipt['upstream'] = source_manifest['upstream']
    receipt['pinned_upstream_tests'] = source_manifest['selectedAssemblyTests']
    offline = ['--offline'] if args.offline else []
    run(['bash',ROOT/'scripts/fetch.sh',*offline],'fetch',180)
    run(['bash',ROOT/'scripts/native-oracle.sh','--offline'],'native-oracle',1800)
    native_path=ROOT/'artifacts/native/receipt.json'; native=json.loads(native_path.read_text())
    if not native['tests'] or not all(row['pass'] for row in native['tests']): raise RuntimeError('Pinned native tests did not pass')
    shutil.copyfile(native_path,out/'native-receipt.json')
    receipt['native'] = {'receipt':str(out/'native-receipt.json'),'sha256':sha(out/'native-receipt.json'),'cases':len(native['tests'])}
    staged = run(['bash',ROOT/'scripts/probe-core.sh','--stage-only'],'stage',180)
    profile = Path(staged.strip().splitlines()[-1]).resolve()
    if not profile.is_relative_to(generated/'core-profile'): raise RuntimeError('Unexpected staged profile path')
    receipt['base_profile'] = str(profile)
    if args.profile == 'threaded':
        threaded = generated/'threaded-core'/attempt.name
        boundary = out/'threaded-stage.json'
        run(['python3',ROOT/'scripts/stage-threaded-core.py','--base-profile',profile,
             '--output',threaded,'--receipt',boundary,'--mremap-validation','--empty-epoll'],
            'threaded-stage',180)
        derivation = json.loads(boundary.read_text())
        if not derivation.get('staged') or derivation.get('profile') != str(threaded):
            raise RuntimeError('Threaded product derivation did not complete')
        receipt['threaded_derivation'] = {'receipt':str(boundary),'sha256':sha(boundary)}
        profile = threaded
    inputs = json.loads((profile/'inputs.json').read_text()); verify_profile(profile,inputs)
    if inputs['compiler']!=receipt['compiler']: raise RuntimeError('Profile compiler differs')
    for name,digest in receipt['configuration'].items():
        if sha(ROOT/name)!=digest: raise RuntimeError('Configuration changed during staging: '+name)
    entries = profile_sources(profile,ROOT,inputs)
    receipt['profile'] = str(profile); receipt['profile_inputs_sha256']=sha(profile/'inputs.json')
    output = run(['python3',ROOT/'scripts/assemble-core.py','--profile',profile,'--jobs',args.jobs,'--timeout',args.timeout],
                 'assemble',max(3600,len(entries)*(args.timeout+30)//args.jobs+300))
    assembly_path = Path(output.strip().splitlines()[-1].split(': ',1)[1]).resolve()
    assembly = json.loads(assembly_path.read_text())
    expected = [entry['path'] for entry in entries]
    if not assembly['linked'] or assembly['failures'] or assembly.get('diagnostic_replay') or assembly['selected']!=expected or set(assembly['objects'])!=set(expected):
        raise RuntimeError('Incomplete or diagnostic-only selected closure')
    if assembly['identity']['profile_inputs_sha256']!=receipt['profile_inputs_sha256'] or assembly['identity']['compiler_sha256']!=receipt['compiler']:
        raise RuntimeError('Assembly identity differs from frozen inputs')
    objects=[]; receipt['objects']={}
    for name in expected:
        row=assembly['objects'][name]; obj=Path(row['object_path'])
        if sha(obj)!=row['object_sha256']: raise RuntimeError('Object changed: '+name)
        objects.append(obj)
        receipt['objects'][name]={key:row[key] for key in ['object_path','object_sha256','receipt','emission_key','producing_profile','producing_profile_inputs_sha256']}
    receipt['assembly']={'path':str(assembly_path),'sha256':sha(assembly_path),'count':len(objects),'reused':assembly['reused_objects']}
    receipt['semantic_intrinsics'] = assembly.get('semantic_intrinsics')
    if (profile/'semantic-intrinsics.json').exists() and not receipt['semantic_intrinsics']:
        raise RuntimeError('Product is missing reviewed semantic selection evidence')
    if any(name in expected for name in ['authored/GuestExecution.c','authored/managed-driver.c']):
        raise RuntimeError('Product must exclude authored C execution and test frontends')
    receipt['product_surface'] = {'execution_owner':'separate authored C# consumer',
                                  'test_frontend_excluded':True,'c_execution_driver_excluded':True}
    linked=attempt/'linked'
    run(['dotnet',cli,*objects,'--emit=managedlib','--literal-pool','--deduplicate-inline','--nest-types','--class-name','BlinkCore',
         '--namespace','Managed.Emulation','--runtime=c','--split=size','--split-size=102400','-o',linked],'delivery-link',300)
    receipt['linked_output']=manifest(linked)
    raw.mkdir(); (raw/'Sources').mkdir(); (raw/'Bridges').mkdir()
    sources=sorted(linked.glob('*.cs'))
    if not sources: raise RuntimeError('Link produced no C# source')
    if any(re.search(r'\bCoreProbe\s*\(', source.read_text()) for source in sources):
        raise RuntimeError('Campaign CoreProbe entrypoint leaked into product sources')
    for source in sources: shutil.copyfile(source,raw/'Sources'/source.name)
    bindings=json.loads((profile/'binding-sources.json').read_text()); names=set()
    host_bindings=json.loads((profile/'host-bindings.json').read_text())
    bridge_sources=host_bindings['managedSources']
    source_by_name={Path(name).name:name for name in bridge_sources}
    if len(source_by_name)!=len(bridge_sources): raise RuntimeError('Authored bridge basename collision')
    receipt['authored_sources']={name:sha(ROOT/name) for name in bridge_sources}
    original_host=ROOT/'src/Managed.Emulation.Host'
    for path in original_host.rglob('*'):
        if path.is_file() and not any(part in {'bin','obj'} for part in path.relative_to(original_host).parts):
            name=str(path.relative_to(ROOT)); digest=sha(path)
            if digest!=sha(profile/'host-project'/path.relative_to(original_host)):
                raise RuntimeError('Authored Host differs from frozen profile: '+name)
            receipt['authored_sources'][name]=digest
    receipt['product_source_links']=list(bridge_sources)
    receipt['product_host_project']='src/Managed.Emulation.Host/Managed.Emulation.Host.csproj'
    receipt['postprocess_scope']='Private frozen project for semantic context; retain only transformed generated sources. Original authored sources are never postprocessed.'

    for name in bindings['authored_managed']:
        source=profile/name
        if source.name in names: raise RuntimeError('Bridge basename collision: '+source.name)
        if source.name not in source_by_name or sha(source)!=receipt['authored_sources'][source_by_name[source.name]]:
            raise RuntimeError('Authored bridge differs from frozen profile: '+source.name)
        names.add(source.name); shutil.copyfile(source,raw/'Bridges'/source.name)
    shutil.copytree(profile/'host-project',raw/'Host',ignore=shutil.ignore_patterns('bin','obj'))
    project(raw/'TranslatedBlink.csproj')
    receipt['raw_files']=manifest(raw)
    shutil.copytree(raw,candidate)
    for item in raw.rglob('*'):
        item.chmod(0o555 if item.is_dir() else 0o444)
    raw.chmod(0o555)
    # Only the postprocessed copy participates in MSBuild/postprocessor writes.
    run(['dotnet','restore',candidate/'TranslatedBlink.csproj'],'restore')
    run(['dotnet','build',candidate/'TranslatedBlink.csproj','-c','Release','--no-restore'],'raw-project-build')
    run(['dotnet',post,candidate/'TranslatedBlink.csproj','--in-place'],'postprocess',900)
    # Only generated transformations are deliverables. Restore exact authored
    # context even if a future processor rewrites one of these private copies.
    receipt['postprocess_private_authored']={'before':manifest(raw/'Bridges'),
                                            'after':manifest(candidate/'Bridges')}
    for name in ('Bridges','Host'):
        shutil.rmtree(candidate/name)
        shutil.copytree(raw/name,candidate/name)
        for path in [candidate/name, *(candidate/name).rglob('*')]:
            path.chmod(0o755 if path.is_dir() else 0o644)
        if manifest(candidate/name)!=manifest(raw/name):
            raise RuntimeError('Authored postprocess context was not restored: '+name)
    # Host restoration also removes its private build caches. Restore project
    # assets before validating generated transformations with original sources.
    run(['dotnet','restore',candidate/'TranslatedBlink.csproj'],'restored-authored-restore')
    run(['dotnet','build',candidate/'TranslatedBlink.csproj','-c','Release','--no-restore'],'postprocessed-project-build')
    verify_authored()
    if manifest(raw)!=receipt['raw_files']: raise RuntimeError('Raw snapshot changed')
    verify_profile(profile,inputs); verify_tools()
    for name,row in receipt['objects'].items():
        if sha(Path(row['object_path']))!=row['object_sha256']: raise RuntimeError('Producer object changed: '+name)
    # Publish source/project inputs only; build caches contain temporary absolute paths.
    for directory in sorted(candidate.rglob('*'),key=lambda p:len(p.parts),reverse=True):
        if directory.is_dir() and directory.name in {'bin','obj'}: shutil.rmtree(directory)
    # The active delivery and IDE use src originals, never archival copies.
    shutil.rmtree(candidate/'Bridges'); shutil.rmtree(candidate/'Host')
    project(candidate/'TranslatedBlink.csproj',bridge_sources)
    receipt['final_files']=manifest(candidate)
    receipt['previous_output']=None
    backup=attempt/'previous-output'
    if stable.exists():
        if stable.is_symlink() or not stable.is_dir(): raise RuntimeError('Stable output is not an ordinary directory')
        receipt['previous_output']={'path':str(backup),'files':manifest(stable)}
    latest=ROOT/'artifacts/translation/latest.json'
    previous_latest=latest.read_bytes() if latest.exists() else None
    try:
        if stable.exists(): stable.rename(backup)
        candidate.rename(stable)
        if manifest(stable)!=receipt['final_files']: raise RuntimeError('Published output differs')
        receipt['project']=str(stable/'TranslatedBlink.csproj')
        verify_authored()
        # Validate the real parent-relative source links at their final path.
        # Roll back the generated directory on failure; src is never overwritten.
        run(['dotnet','build',stable/'TranslatedBlink.csproj','-c','Release'],'direct-source-project-build')
        verify_authored(); verify_profile(profile,inputs); verify_tools()
        if manifest(stable)!=receipt['final_files']: raise RuntimeError('Final source/project changed during direct build')
        receipt['authored_sources_unchanged']=True
        receipt['passed']=True; save()
        pending=latest.with_suffix('.tmp')
        pending.write_text(json.dumps({'receipt':str(out/'receipt.json'),'sha256':sha(out/'receipt.json')},indent=2)+'\n')
        pending.replace(latest)
        publication_committed=True
    except BaseException:
        if publication_committed: raise
        receipt['passed']=False
        if not candidate.exists() and stable.exists():
            stable.rename(attempt/'failed-published-output')
        if backup.exists(): backup.rename(stable)
        if previous_latest is not None:
            pending=latest.with_suffix('.rollback'); pending.write_bytes(previous_latest); pending.replace(latest)
        else: latest.unlink(missing_ok=True)
        raise
    print('Postprocessed library: '+str(stable/'TranslatedBlink.csproj'))
    print('Immutable raw snapshot: '+str(raw))
    print('Delivery receipt: '+str(out/'receipt.json'))
except BaseException as error:
    if not publication_committed:
        receipt['passed']=False; receipt['error']={'type':type(error).__name__,'message':str(error)}
    raise
finally:
    # The successful receipt is now addressed by latest.json and must stay immutable.
    if not publication_committed: save()
