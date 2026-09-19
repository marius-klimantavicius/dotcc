#!/usr/bin/env python3
"""Translate and execute pinned upstream test programs against disposable Samba.

Shell assertions are orchestrated by the C# UpstreamRunner; native instrumentation
and unavailable server profiles remain explicit skips in its per-case receipt.
"""
import argparse
import json
from pathlib import Path
import secrets
import socket
import subprocess
import tempfile
import time
from common import ROOT, SOURCE_SPEC, fetch, run, sha

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--no-build', action='store_true', help='Use the existing upstream executable manifest')
parser.add_argument('--jit-only', action='store_true', help='Skip NativeAOT build/execution for a development run')
args = parser.parse_args()
logs = ROOT / 'artifacts/upstream-tests'
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, source=SOURCE_SPEC, scope='translated upstream cases; explicit skips are not passes')
receipt['authored_sources'] = {
    str(path.relative_to(ROOT)): sha(path)
    for directory in (ROOT / 'tests/UpstreamRunner', ROOT / 'scripts')
    for path in directory.glob('*') if path.is_file() and path.suffix in ('.cs', '.csproj', '.py', '.sh')
}
container = None
try:
    source = fetch()
    if not args.no_build:
        command = ['python3', ROOT / 'scripts/translate-upstream.py']
        if not args.jit_only:
            command.append('--aot')
        run(command, logs / 'translation.log', receipt, timeout=14400)
    manifest_path = ROOT / 'build/upstream/manifest.json'
    manifest = json.loads(manifest_path.read_text())
    receipt['build_manifest_sha256'] = sha(manifest_path)
    if manifest['source'] != SOURCE_SPEC or manifest['sourceRoot'] != str(source):
        raise RuntimeError('Upstream executable manifest uses a different source pin')
    if sha(manifest['buildReceipt']) != manifest['buildReceiptSha256']:
        raise RuntimeError('Upstream build receipt changed; rebuild the programs')
    if sha(ROOT / 'scripts/translate-upstream.py') != manifest['harnessSha256']:
        raise RuntimeError('Upstream build script changed; rebuild the programs')
    if sha(ROOT / 'artifacts/translation/result.json') != manifest['translationReceiptSha256']:
        raise RuntimeError('Library translation changed since upstream executables were built; rebuild them')
    for name, digest in manifest['configurationSha256'].items():
        if sha(ROOT / name) != digest:
            raise RuntimeError('Library translation configuration changed; regenerate first')
    for name, digest in manifest['librarySourceSha256'].items():
        if sha(source / name) != digest:
            raise RuntimeError('Library source changed: ' + name)
    for name, baseline in manifest.get('baselineBlockedPrograms', {}).items():
        for stream in ('stdout', 'stderr'):
            if sha(baseline['logs'][stream]) != baseline['native' + stream.capitalize() + 'Sha256']:
                raise RuntimeError('Native baseline evidence changed: ' + name)
    if args.jit_only:
        manifest['variants'] = [v for v in manifest['variants'] if not v['name'].endswith('-aot')]
    expected = {'native', 'raw-jit', 'processed-jit'}
    if not args.jit_only:
        expected |= {'raw-aot', 'processed-aot'}
    if {v['name'] for v in manifest['variants']} != expected:
        raise RuntimeError('Executable manifest does not contain the requested runtime matrix; rebuild it')
    required = {'prog_mkdir', 'prog_rmdir', 'smb2-cp', 'prog_cat', 'prog_cat_cancel',
                'aes128ccm-test', 'ntlmssp_generate_blob'}
    for variant in manifest['variants']:
        if not required.issubset(variant['programs']):
            raise RuntimeError('Executable manifest is missing the initial upstream program set; rebuild without --program filters')
    for name, info in manifest['programSources'].items():
        if sha(source / info['source']) != info['sha256']:
            raise RuntimeError('Upstream test source changed: ' + name)
        for adaptation in info['adaptations']:
            if sha(adaptation['path']) != adaptation['sha256']:
                raise RuntimeError('Upstream test wrapper changed: ' + name)
        for variant in manifest['variants']:
            command = variant['programs'][name]
            executable = command[1] if command[0] == 'dotnet' else command[0]
            if sha(executable) != info['outputSha256'][variant['name']]:
                raise RuntimeError('Upstream executable changed: ' + name + '/' + variant['name'])
    receipt['variants'] = sorted(expected)
    runner = ROOT / 'tests/UpstreamRunner/UpstreamRunner.csproj'
    run(['dotnet', 'build', runner, '-c', 'Release', '--nologo'], logs / 'runner-build.log', receipt)
    image = 'dotcc-libsmb2-samba:4.19.5'
    run(['docker', 'build', '-t', image, ROOT / 'tests/Samba'], logs / 'image-build.log', receipt)
    image_id = subprocess.check_output(['docker', 'image', 'inspect', '--format', '{{.Id}}', image], text=True).strip()
    receipt['samba_image'] = image_id
    receipt['samba_recipe_sha256'] = {p.name: sha(p) for p in sorted((ROOT / 'tests/Samba').iterdir())}
    with tempfile.TemporaryDirectory(prefix='libsmb2-upstream-secrets-') as private:
        private = Path(private)
        password = secrets.token_hex(24)
        password_file = private / 'password'
        credentials = private / 'NTLM'
        password_file.write_text(password + '\n')
        credentials.write_text('WORKGROUP:smbprobe:' + password + '\n127.0.0.1:smbprobe:' + password + '\n')
        password_file.chmod(0o600)
        credentials.chmod(0o600)
        container = 'dotcc-libsmb2-upstream-' + secrets.token_hex(6)
        run(['docker', 'run', '-d', '--name', container, '--read-only',
             '--tmpfs', '/run', '--tmpfs', '/var/lib/samba', '--tmpfs', '/var/cache/samba',
             '--tmpfs', '/var/log/samba', '--tmpfs', '/srv/share',
             '--mount', f'type=bind,src={password_file},dst=/oracle-password,readonly',
             '-p', '127.0.0.1::445', image_id], logs / 'container-start.log', receipt)
        port = int(subprocess.check_output(['docker', 'inspect', '--format',
            '{{(index (index .NetworkSettings.Ports "445/tcp") 0).HostPort}}', container], text=True).strip())
        deadline = time.monotonic() + 30
        while True:
            try:
                with socket.create_connection(('127.0.0.1', port), timeout=1):
                    break
            except OSError:
                if time.monotonic() >= deadline:
                    raise RuntimeError('Samba did not listen within 30 seconds')
                time.sleep(0.1)
        receipt['samba_version'] = run(['docker', 'exec', container, 'smbd', '--version'],
                                       logs / 'samba-version.log', receipt).strip()
        for variant in manifest['variants']:
            name = variant['name']
            run(['docker', 'exec', container, 'mkdir', '/srv/share/' + name],
                logs / (name + '-mkdir.log'), receipt)
            run(['docker', 'exec', container, 'chown', 'smbprobe:smbprobe', '/srv/share/' + name],
                logs / (name + '-chown.log'), receipt)
            variant['testUrl'] = f'smb://WORKGROUP;smbprobe@127.0.0.1:{port}/probe/{name}'
        manifest.update(sourceRoot=str(source), artifactRoot=str(logs / 'cases'),
                        timeoutSeconds=120, environment={'NTLM_USER_FILE': str(credentials)})
        execution_manifest = logs / 'execution-manifest.json'
        execution_manifest.write_text(json.dumps(manifest, indent=2) + '\n')
        run(['dotnet', runner.parent / 'bin/Release/net10.0/UpstreamRunner.dll',
             '--manifest', execution_manifest], logs / 'runner.log', receipt, timeout=3600)
        result = json.loads((logs / 'cases/result.json').read_text())
        receipt['runner_result_sha256'] = sha(logs / 'cases/result.json')
        if not result['passed']:
            raise RuntimeError('Upstream runner reported failed assertions')
        if any(sha(ROOT / name) != digest for name, digest in receipt['authored_sources'].items()):
            raise RuntimeError('Upstream runner/build scripts changed during execution; rerun')
        fetch()  # Revalidate that the immutable upstream inputs were not edited.
        receipt['passed'] = True
        print('Translated upstream test matrix passed; per-case passes/skips: ' + str(logs / 'cases/result.json'))
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    if container:
        with (logs / 'samba.log').open('w') as output:
            subprocess.run(['docker', 'logs', container], stdout=output, stderr=subprocess.STDOUT, timeout=15)
        subprocess.run(['docker', 'rm', '-f', container], stdout=subprocess.DEVNULL,
                       stderr=subprocess.DEVNULL, timeout=30)
    (logs / 'result.json').write_text(json.dumps(receipt, indent=2) + '\n')
