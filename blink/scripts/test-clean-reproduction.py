#!/usr/bin/env python3
"""Prepare an empty detached checkout, then explicitly resume its core reproduction."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
parser = argparse.ArgumentParser(description=__doc__)
action = parser.add_mutually_exclusive_group(required=True)
action.add_argument('--prepare', action='store_true', help='No compilation or execution gates')
action.add_argument('--resume', type=Path, help='Explicitly start builds from a prepared receipt')
parser.add_argument('--revision', default='717ba668cb0575e459b0ff21dfdf16fba1cdb58a')
parser.add_argument('--worktree-parent', type=Path, default=Path('/home/marius/p'))
args = parser.parse_args()
def output(command, cwd=REPO):
    return subprocess.check_output(list(map(str, command)), cwd=cwd, text=True).strip()
def tools_in(repo):
    return {str(p.relative_to(repo)): sha(p)
            for directory in ['DotCC/bin/Release/net10.0', 'DotCC.PostProcess/bin/Release/net10.0']
            for p in (repo / directory).glob('*') if p.is_file() and p.suffix in {'.dll', '.json'}}
def clean(worktree):
    status = output(['git', 'status', '--porcelain', '--untracked-files=all'], worktree)
    if status: raise RuntimeError('Checkout is not clean: ' + status)
    return status
if args.prepare:
    base = ROOT / 'artifacts/clean-reproduction'; base.mkdir(parents=True, exist_ok=True)
    out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
    r = {'kind': 'clean-detached-checkout-valid-core-reproduction', 'passed': False,
         'prepared': False, 'phase': 'prepare', 'commands': {}, 'origin': str(REPO),
         'origin_tools_before': tools_in(REPO), 'runner_sha256': sha(Path(__file__)),
         'requested_revision': args.revision,
         'limitations': ['Linux x64 valid core only; no service or malformed-input execution.',
                        'Installed SDK/native tools and host NuGet cache may be reused; no compiler, emitted object or native build output is copied.',
                        'Direct IL/publication inventories retain their indirect/framework limitations; not a complete runtime isolation proof.']}
else:
    receipt_path = args.resume.resolve(); out = receipt_path.parent
    r = json.loads(receipt_path.read_text())
    if not r.get('prepared') or r.get('phase') != 'awaiting-build-release' or r.get('passed'):
        raise RuntimeError('Resume requires an unstarted preparation receipt')
    if r['runner_sha256'] != sha(Path(__file__)): raise RuntimeError('Runner changed after preparation')
def save():
    temporary = out / 'receipt.tmp'
    temporary.write_text(json.dumps(r, indent=2) + '\n')
    temporary.replace(out / 'receipt.json')
def run(command, label, cwd, timeout=1800):
    start = time.monotonic(); command = list(map(str, command)); code = None
    env = dict(os.environ, LC_ALL='C', BLINK_JOBS='4')
    try:
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            code = subprocess.run(command, cwd=cwd, env=env, stdout=stdout, stderr=stderr,
                                  timeout=timeout).returncode
    finally:
        r['commands'][label] = {'argv': command, 'cwd': str(cwd), 'exit': code,
                                'seconds': time.monotonic() - start,
                                'stdout_sha256': sha(out / (label + '.stdout')),
                                'stderr_sha256': sha(out / (label + '.stderr'))}
        save()
    if code: raise RuntimeError(label + ' failed; see ' + str(out))
    return (out / (label + '.stdout')).read_text()
def evidence(path):
    return {'path': str(path), 'sha256': sha(path)}
try:
    if args.prepare:
        commit = output(['git', 'rev-parse', '--verify', args.revision + '^{commit}'])
        worktree = Path(tempfile.mkdtemp(prefix='dotcc-blink-clean-', dir=args.worktree_parent))
        r['worktree'] = str(worktree); r['commit'] = commit; save()
        run(['git', 'worktree', 'add', '--detach', worktree, commit], 'worktree-add', REPO, 120)
        if output(['git', 'rev-parse', 'HEAD'], worktree) != commit: raise RuntimeError('Wrong checkout HEAD')
        detached = subprocess.run(['git', 'symbolic-ref', '-q', 'HEAD'], cwd=worktree, capture_output=True)
        if detached.returncode != 1: raise RuntimeError('Checkout is not detached')
        r['initial_status'] = clean(worktree)
        forbidden = ['blink/ref', 'blink/generated', 'blink/build', 'blink/artifacts',
                     'DotCC/bin', 'DotCC/obj', 'DotCC.Lib/bin', 'DotCC.Lib/obj',
                     'DotCC.PostProcess/bin', 'DotCC.PostProcess/obj']
        r['initial_absent_paths'] = forbidden
        for name in forbidden:
            if (worktree / name).exists(): raise RuntimeError('Unexpected initial output: ' + name)
        outputs = [str(p.relative_to(worktree)) for p in worktree.rglob('*')
                   if p.is_dir() and p.name in {'bin', 'obj'}]
        if outputs: raise RuntimeError('Unexpected initial compiler/build output directories: ' + str(outputs))
        r['initial_bin_obj_directories'] = outputs
        manifest_path = worktree / 'blink/config/source-manifest.json'
        manifest = json.loads(manifest_path.read_text()); upstream = manifest['upstream']
        source = ROOT / 'ref' / upstream['archive']
        if sha(source) != upstream['sha256']: raise RuntimeError('Cached source archive mismatch')
        ref = worktree / 'blink/ref'; ref.mkdir()
        target = ref / upstream['archive']; shutil.copyfile(source, target)
        if sha(target) != upstream['sha256'] or list(ref.iterdir()) != [target]: raise RuntimeError('Archive copy failed')
        r['only_copied_asset'] = {'origin': str(source), **evidence(target)}
        r['source_manifest'] = evidence(manifest_path)
        r['readme'] = evidence(worktree / 'blink/README.md')
        r['nuget_reuse'] = {'allowed': True, 'packages_path': str(Path(os.environ.get('NUGET_PACKAGES', str(Path.home()/'.nuget/packages'))).resolve()),
                           'policy': 'Normal SDK restore may reuse installed packages or fetch declared packages; all produced compiler/runtime/package identities are recorded.'}
        r['installed_tools'] = {}
        for name in ['dotnet', 'python3', 'gcc', 'cc', 'as', 'ld', 'ar', 'make', 'readelf']:
            path = Path(shutil.which(name) or name).resolve()
            r['installed_tools'][name] = {'path': str(path), 'sha256': sha(path)}
        r['documented_commands'] = [
            ['dotnet', 'build', 'dotcc.sln', '-c', 'Release', '-p:UseLocalLalrCc=false'],
            ['bash', 'blink/scripts/fetch.sh', '--offline'],
            ['bash', 'blink/scripts/native-oracle.sh', '--offline'],
            ['bash', 'blink/scripts/probe-core.sh', '--stage-only'],
            ['python3', 'blink/scripts/assemble-core.py', '--profile', '<fresh-profile>', '--jobs', '4', '--timeout', '300'],
            ['python3', 'blink/tests/CoreExecution/run.py', '--assembly-receipt', '<fresh-109-object-receipt>'],
            ['python3', 'blink/scripts/audit-published-core.py', '<fresh-execution-receipt>']]
        r['prepared_status'] = clean(worktree)
        r['prepared'] = True; r['phase'] = 'awaiting-build-release'; save()
        print('Prepared without builds: ' + str(out / 'receipt.json'))
    else:
        worktree = Path(r['worktree'])
        if output(['git', 'rev-parse', 'HEAD'], worktree) != r['commit']: raise RuntimeError('HEAD changed')
        clean(worktree)
        for name in r['initial_absent_paths']:
            if name != 'blink/ref' and (worktree/name).exists(): raise RuntimeError('Pre-build outputs appeared: '+name)
        archive = Path(r['only_copied_asset']['path'])
        if sha(archive) != r['only_copied_asset']['sha256'] or list(archive.parent.iterdir()) != [archive]:
            raise RuntimeError('Pre-build archive assets changed')
        r['phase'] = 'building'; save()
        run(r['documented_commands'][0], 'compiler-build', worktree, 1800)
        r['fresh_compiler'] = tools_in(worktree)
        r['nuget_assets'] = {str(p.relative_to(worktree)): sha(p) for p in worktree.rglob('project.assets.json')}
        run(['dotnet','--info'], 'dotnet-info', worktree, 60)
        run(r['documented_commands'][1], 'offline-fetch', worktree, 120)
        run(r['documented_commands'][2], 'native-oracle', worktree, 1800)
        native_path = worktree/'blink/artifacts/native/receipt.json'; native = json.loads(native_path.read_text())
        if len(native['tests']) != 25 or not all(row['pass'] for row in native['tests']): raise RuntimeError('Native25 gate incomplete')
        r['native'] = evidence(native_path); save()
        run(r['documented_commands'][3], 'stage', worktree, 180)
        profile = Path((worktree/'blink/artifacts/core/latest-profile.txt').read_text().strip())
        if not profile.is_relative_to(worktree): raise RuntimeError('Profile escaped clean checkout')
        run(['python3','blink/scripts/assemble-core.py','--profile',profile,'--jobs','4','--timeout','300'], 'assemble', worktree, 3600)
        assemblies = list((worktree/'blink/artifacts/core/objects').glob('*/receipt.json'))
        if len(assemblies) != 1: raise RuntimeError('Expected exactly one fresh assembly receipt')
        assembly_path = assemblies[0]; assembly = json.loads(assembly_path.read_text())
        if not assembly['linked'] or assembly['failures'] or len(assembly['selected']) != 109 or len(assembly['objects']) != 109 or assembly['reused_objects'] != 0:
            raise RuntimeError('Fresh109/no-reuse gate failed')
        for name, row in assembly['objects'].items():
            if not Path(row['object_path']).is_relative_to(worktree) or sha(Path(row['object_path'])) != row['object_sha256']:
                raise RuntimeError('Invalid fresh object identity: '+name)
        r['assembly'] = evidence(assembly_path); save()
        run(['python3','blink/tests/CoreExecution/run.py','--assembly-receipt',assembly_path], 'core-execution', worktree, 1800)
        executions = list((worktree/'blink/artifacts/core-execution').glob('attempt-*/receipt.json'))
        if len(executions) != 1: raise RuntimeError('Expected one fresh core execution')
        execution_path = executions[0]; execution = json.loads(execution_path.read_text())
        if not execution['passed'] or not execution['runtime_matrix_passed'] or execution['diagnostic_replay']:
            raise RuntimeError('Actual core all4 gate failed')
        r['execution'] = evidence(execution_path); save()
        run(['python3','blink/scripts/audit-published-core.py',execution_path], 'publication', worktree, 120)
        publications = list((worktree/'blink/artifacts/publication-audit').glob('attempt-*/receipt.json'))
        if len(publications) != 1 or not json.loads(publications[0].read_text())['passed']: raise RuntimeError('Publication gate incomplete')
        r['publication'] = evidence(publications[0]); r['final_status'] = clean(worktree)
        r['origin_tools_after'] = tools_in(Path(r['origin']))
        if r['origin_tools_after'] != r['origin_tools_before']: raise RuntimeError('Origin compiler tools changed during independent reproduction')
        if tools_in(worktree) != r['fresh_compiler']: raise RuntimeError('Fresh compiler changed during gates')
        r['passed'] = True; r['phase'] = 'complete'; save()
        print('Clean-checkout native25/fresh109/core-all4/publication passed: '+str(out/'receipt.json'))
except BaseException as error:
    r['passed'] = False; r['failure'] = {'type': type(error).__name__, 'message': str(error), 'phase': r['phase']}
    raise
finally:
    save()
