#!/usr/bin/env python3
"""Reproduce the committed Blink delivery and its sample in an empty worktree."""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import signal
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--revision', required=True, help='Committed delivery revision to reproduce')
parser.add_argument('--worktree-parent', type=Path, default=Path(tempfile.gettempdir()))
args = parser.parse_args()
base = ROOT / 'artifacts/clean-delivery'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
temporary = out / 'tmp'
temporary.mkdir()
r = {'kind': 'clean-committed-delivery-and-sample', 'passed': False, 'phase': 'prepare',
     'requested_revision': args.revision, 'runner_sha256': sha(Path(__file__)),
     'origin': str(REPO), 'commands': {}, 'temporary_directory': str(temporary),
     'limitations': ['Linux x64 normal core sample only; no HTTP or translated service worker.',
                    'Installed SDK/native tools and normal NuGet restore/cache are reused; not hermetic.',
                    'No custom fault-injection or invalid-ELF cases; no Windows execution.',
                    'This delivery check does not replace the separate full runtime dependency audit.']}


def interrupted(signum, frame):
    raise KeyboardInterrupt('Received signal ' + str(signum))


signal.signal(signal.SIGTERM, interrupted)


def save():
    pending = out / 'receipt.tmp'
    pending.write_text(json.dumps(r, indent=2) + '\n')
    pending.replace(out / 'receipt.json')


def stop_group(process):
    try:
        os.killpg(process.pid, signal.SIGTERM)
    except ProcessLookupError:
        return
    try:
        # Nested delivery orchestration needs time to stop its own child group.
        process.wait(timeout=10)
    except subprocess.TimeoutExpired:
        pass
    try:
        os.killpg(process.pid, signal.SIGKILL)
    except ProcessLookupError:
        pass
    process.wait()


def run(command, label, cwd, timeout=1800):
    command = list(map(str, command))
    start = time.monotonic()
    code = None
    stdout_path, stderr_path = out / (label + '.stdout'), out / (label + '.stderr')
    try:
        with stdout_path.open('wb') as stdout, stderr_path.open('wb') as stderr:
            process = subprocess.Popen(command, cwd=cwd, stdout=stdout, stderr=stderr,
                                       env=dict(os.environ, LC_ALL='C', TMPDIR=str(temporary), BLINK_JOBS='4'),
                                       start_new_session=True)
            try:
                code = process.wait(timeout=timeout)
            except BaseException:
                stop_group(process)
                raise
    finally:
        r['commands'][label] = {'argv': command, 'cwd': str(cwd), 'exit_code': code,
                                'seconds': time.monotonic() - start,
                                'stdout': str(stdout_path), 'stderr': str(stderr_path),
                                'stdout_sha256': sha(stdout_path), 'stderr_sha256': sha(stderr_path)}
        save()
    if code:
        raise RuntimeError(label + ' failed; see ' + str(out))
    return stdout_path.read_text()


def evidence(path):
    return {'path': str(path), 'sha256': sha(path)}


def files(directory):
    return {str(p.relative_to(directory)): sha(p) for p in sorted(directory.rglob('*'))
            if p.is_file() and not any(part in {'bin', 'obj'} for part in p.relative_to(directory).parts)}


def binaries(directory):
    return {str(p.relative_to(directory)): sha(p) for p in sorted(directory.rglob('*')) if p.is_file()}


def compiler_files(repo):
    return {str(p.relative_to(repo)): sha(p)
            for folder in ('DotCC', 'DotCC.PostProcess')
            for p in sorted((repo / folder / 'bin/Release/net10.0').glob('*'))
            if p.is_file() and p.suffix in {'.dll', '.json'}}


def clean(tree, label):
    status = run(['git', 'status', '--porcelain', '--untracked-files=all'], label, tree, 30)
    if status.strip():
        raise RuntimeError('Checkout is not clean: ' + status)
    return status


def owned_path(value):
    path = Path(value).resolve()
    if not path.is_relative_to(worktree):
        raise RuntimeError('Artifact escaped clean checkout: ' + str(path))
    return path


def execute(command, label, binary_directory, expected=None):
    before = binaries(binary_directory)
    actual = run(command, label, worktree, 60)
    after = binaries(binary_directory)
    r['commands'][label]['binaries_before'] = before
    r['commands'][label]['binaries_after'] = after
    if before != after:
        raise RuntimeError(label + ' binaries changed during execution')
    if (out / (label + '.stderr')).read_bytes():
        raise RuntimeError(label + ' wrote unexpected diagnostics')
    if expected is not None:
        rows = actual.splitlines()
        footer = re.fullmatch(r'core retained-mappings=(\d+) charged-bytes=(\d+)', rows[-1]) if rows else None
        if not footer or int(footer[2]) > 64 * 1024 * 1024:
            raise RuntimeError(label + ' missing/invalid ownership accounting footer')
        if rows[:-1] != expected:
            (out / (label + '.diff')).write_text('\n'.join(difflib.unified_diff(expected, rows[:-1], fromfile='native-profile', tofile=label)) + '\n')
            raise RuntimeError(label + ' transcript differs from native/profile reference')
        r['commands'][label]['retained_mappings'] = int(footer[1])
        r['commands'][label]['charged_bytes'] = int(footer[2])
    save()
    return actual


try:
    if platform.system() != 'Linux' or platform.machine() not in {'x86_64', 'AMD64'}:
        raise RuntimeError('This execution check requires Linux x64')
    r['origin_compiler_before'] = compiler_files(REPO)
    commit = run(['git', 'rev-parse', '--verify', args.revision + '^{commit}'], 'resolve-revision', REPO, 30).strip()
    r['commit'] = commit
    worktree = Path(tempfile.mkdtemp(prefix='dotcc-blink-delivery-', dir=args.worktree_parent)).resolve()
    r['worktree'] = str(worktree)
    run(['git', 'worktree', 'add', '--detach', worktree, commit], 'worktree-add', REPO, 120)
    if run(['git', 'rev-parse', 'HEAD'], 'initial-head', worktree, 30).strip() != commit:
        raise RuntimeError('Wrong checkout revision')
    if run(['git', 'rev-parse', '--abbrev-ref', 'HEAD'], 'detached-head', worktree, 30).strip() != 'HEAD':
        raise RuntimeError('Expected detached checkout')
    r['initial_status'] = clean(worktree, 'initial-status')
    forbidden = ['blink/' + name for name in ('ref', 'generated', 'build', 'artifacts')]
    if any((worktree / name).exists() for name in forbidden):
        raise RuntimeError('Initial campaign outputs already exist')
    initial_build_dirs = [str(p.relative_to(worktree)) for p in worktree.rglob('*') if p.is_dir() and p.name in {'bin', 'obj'}]
    if initial_build_dirs:
        raise RuntimeError('Initial bin/obj directories already exist: ' + str(initial_build_dirs))
    r['initial_absent_paths'], r['initial_bin_obj_directories'] = forbidden, initial_build_dirs
    if sha(worktree / 'blink/scripts/test-clean-delivery.py') != r['runner_sha256']:
        raise RuntimeError('Requested commit does not contain this clean delivery runner')
    manifest_path = worktree / 'blink/config/source-manifest.json'
    manifest = json.loads(manifest_path.read_text())
    upstream = manifest['upstream']
    archive = ROOT / 'ref' / upstream['archive']
    if sha(archive) != upstream['sha256']:
        raise RuntimeError('Pinned source archive mismatch')
    destination = worktree / 'blink/ref' / upstream['archive']
    destination.parent.mkdir()
    shutil.copyfile(archive, destination)
    if sha(destination) != upstream['sha256'] or list(destination.parent.iterdir()) != [destination]:
        raise RuntimeError('Copied archive differs or extra assets appeared')
    r['only_copied_asset'] = {'origin': str(archive), **evidence(destination)}
    r['source_manifest'] = evidence(manifest_path)
    r['upstream_revision'] = upstream['revision']
    r['native_upstream_cases'] = manifest['selectedAssemblyTests']
    r['installed_tools'] = {}
    for name in ('dotnet', 'python3', 'gcc', 'cc', 'as', 'ld', 'ar', 'make'):
        path = Path(shutil.which(name) or name).resolve()
        r['installed_tools'][name] = evidence(path)
    r['nuget_reuse'] = {'allowed': True, 'packages_path': os.environ.get('NUGET_PACKAGES', str(Path.home() / '.nuget/packages')),
                       'network_restore_allowed': True}
    run(['dotnet', '--info'], 'dotnet-info', worktree, 60)
    r['phase'] = 'compiler-build'
    run(['dotnet', 'build', 'dotcc.sln', '-c', 'Release', '-p:UseLocalLalrCc=false'], 'compiler-build', worktree)
    r['fresh_compiler'] = compiler_files(worktree)
    r['phase'] = 'translate'
    run(['bash', 'blink/scripts/translate.sh', '--offline'], 'translate', worktree, 10800)
    latest = json.loads((worktree / 'blink/artifacts/translation/latest.json').read_text())
    delivery_path = owned_path(latest['receipt'])
    if sha(delivery_path) != latest['sha256']:
        raise RuntimeError('Delivery receipt pointer differs')
    delivery = json.loads(delivery_path.read_text())
    if not delivery['passed'] or delivery['assembly']['count'] != 109 or delivery['assembly']['reused'] != 0:
        raise RuntimeError('Fresh109 zero-reuse delivery gate failed')
    assembly_path = owned_path(delivery['assembly']['path'])
    if sha(assembly_path) != delivery['assembly']['sha256']:
        raise RuntimeError('Assembly receipt changed')
    assembly = json.loads(assembly_path.read_text())
    if not assembly['linked'] or assembly['failures'] or assembly.get('diagnostic_replay') or len(assembly['selected']) != 109 or len(assembly['objects']) != 109 or assembly['reused_objects'] != 0:
        raise RuntimeError('Incomplete fresh selected-source assembly')
    for name, row in assembly['objects'].items():
        if sha(owned_path(row['object_path'])) != row['object_sha256']:
            raise RuntimeError('Fresh producer object changed: ' + name)
        owned_path(row['producing_profile'])
    raw, final = owned_path(delivery['raw_snapshot']), owned_path(delivery['stable_output'])
    if final != worktree / 'blink/generated/TranslatedBlink':
        raise RuntimeError('Wrong stable final output')
    if files(raw) != delivery['raw_files'] or files(final) != delivery['final_files']:
        raise RuntimeError('Raw/final delivery manifest mismatch')
    if any(p.stat().st_mode & 0o222 for p in [raw, *raw.rglob('*')]):
        raise RuntimeError('Raw snapshot is writable')
    r['delivery'] = evidence(delivery_path)
    native_path = worktree / 'blink/artifacts/native/receipt.json'
    native = json.loads(native_path.read_text())
    if native['upstreamRevision'] != upstream['revision'] or len(native['tests']) != len(manifest['selectedAssemblyTests']) or not all(row['pass'] for row in native['tests']):
        raise RuntimeError('Pinned upstream native gate incomplete')
    r['native_upstream'] = evidence(native_path)
    profile = owned_path(delivery['profile'])
    closure = json.loads((profile / 'closure.json').read_text())
    native_binary = worktree / 'blink/build/core-native'
    if sha(native_binary) != closure['native_binary_sha256']:
        raise RuntimeError('Native core binary differs from frozen profile')
    native_before = sha(native_binary)
    native_rows = run([native_binary], 'native-core', worktree, 30).splitlines()
    if not native_rows or not native_rows[0].startswith('abi ') or sha(native_binary) != native_before:
        raise RuntimeError('Native reference changed or transcript missing')
    if (out / 'native-core.stderr').read_bytes():
        raise RuntimeError('Native reference wrote unexpected diagnostics')
    r['commands']['native-core']['binary_before_sha256'] = native_before
    r['commands']['native-core']['binary_after_sha256'] = sha(native_binary)
    abi_binary = worktree / 'blink/build/delivery-abi'
    run(['cc', '-std=c17', '-D_GNU_SOURCE', '-DNDEBUG', '-DNOLINEAR', '-I', profile,
         '-I', worktree / 'blink/ref' / upstream['directory'], '-iquote', profile / 'host',
         worktree / 'blink/tests/CoreExecution/abi.c', '-o', abi_binary], 'profile-abi-build', worktree, 120)
    abi_before = sha(abi_binary)
    profile_abi = run([abi_binary], 'profile-abi', worktree, 30).strip()
    if not profile_abi.startswith('abi ') or sha(abi_binary) != abi_before:
        raise RuntimeError('Profile ABI reference changed')
    if (out / 'profile-abi.stderr').read_bytes():
        raise RuntimeError('Profile ABI reference wrote unexpected diagnostics')
    r['commands']['profile-abi']['binary_before_sha256'] = abi_before
    r['commands']['profile-abi']['binary_after_sha256'] = sha(abi_binary)
    expected = [profile_abi, *native_rows[1:]]
    r['expected_sample_transcript_without_footer'] = expected
    r['phase'] = 'sample'
    run(['dotnet', 'build', 'blink/ManagedConsumer.slnx', '-c', 'Release'], 'sample-solution-build', worktree)
    sample = worktree / 'blink/ManagedConsumer/ManagedConsumer.csproj'
    execute(['dotnet', 'run', '--project', sample, '-c', 'Release', '--no-build'], 'sample-jit',
            sample.parent / 'bin/Release/net10.0', expected)
    publish = worktree / 'blink/build/managed-consumer-aot'
    run(['dotnet', 'publish', sample, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', publish],
        'sample-aot-publish', worktree)
    execute([publish / 'ManagedConsumer'], 'sample-aot', publish, expected)
    if files(raw) != delivery['raw_files'] or files(final) != delivery['final_files']:
        raise RuntimeError('Raw/final sources changed during sample builds or execution')
    if compiler_files(worktree) != r['fresh_compiler']:
        raise RuntimeError('Fresh compiler changed during delivery')
    r['origin_compiler_after'] = compiler_files(REPO)
    if r['origin_compiler_after'] != r['origin_compiler_before']:
        raise RuntimeError('Origin compiler changed during independent reproduction')
    r['nuget_assets'] = {str(p.relative_to(worktree)): sha(p) for p in worktree.rglob('project.assets.json')}
    r['final_status'] = clean(worktree, 'final-status')
    if run(['git', 'rev-parse', 'HEAD'], 'final-head', worktree, 30).strip() != commit:
        raise RuntimeError('Checkout revision changed')
    r['passed'], r['phase'] = True, 'complete'
    print('Clean delivery and JIT/NativeAOT sample passed: ' + str(out / 'receipt.json'))
except BaseException as error:
    r['passed'] = False
    r['failure'] = {'type': type(error).__name__, 'message': str(error), 'phase': r['phase']}
    raise
finally:
    save()
