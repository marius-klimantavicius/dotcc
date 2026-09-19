#!/usr/bin/env python3
"""Build the pinned native client and exercise an isolated Samba container."""
import argparse
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import tempfile
import time
from common import ROOT, SOURCE_SPEC, fetch, run, sha

OUT = ROOT / 'artifacts/native-oracle'
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--managed', choices=['jit', 'aot'], help='Exercise the managed sample instead of the native client')
parser.add_argument('--raw', action='store_true', help='Use the separate unprocessed project')
parser.add_argument('--suite', choices=['sample', 'lifecycle'], default='sample',
                    help='Managed consumer suite (default: sample)')
args = parser.parse_args()
if args.raw and not args.managed:
    parser.error('--raw requires --managed')
if args.suite != 'sample' and not args.managed:
    parser.error('--suite requires --managed')
if args.managed:
    OUT = ROOT / 'artifacts' / (('managed-oracle-' if args.suite == 'sample' else 'managed-lifecycle-')
                              + args.managed + ('-raw' if args.raw else '-processed'))
OUT.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, source=SOURCE_SPEC, cases=[])
container = None
password_path = None
try:
    source = fetch()
    build = ROOT / 'build/native-oracle'
    run(['cmake', '-S', source, '-B', build, '-DENABLE_LIBKRB5=OFF',
         '-DENABLE_GSSAPI=OFF', '-DENABLE_LIBDCERPC=OFF', '-DENABLE_EXAMPLES=OFF',
         '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_EXPORT_COMPILE_COMMANDS=ON'],
        OUT / 'configure.log', receipt)
    run(['cmake', '--build', build, '--parallel', '4'], OUT / 'build.log', receipt)
    client = build / 'native-client'
    run(['cc', '-std=c17', '-Wall', '-Wextra', '-Werror',
         '-I', source / 'include/smb2', '-I', source / 'include',
         ROOT / 'tests/NativeClient/client.c', '-L', build / 'lib',
         '-Wl,-rpath,' + str(build / 'lib'), '-lsmb2', '-o', client],
        OUT / 'client-build.log', receipt)
    receipt['client_sha256'] = sha(client)
    receipt['library_sha256'] = sha(build / 'lib/libsmb2.so')
    receipt['native_compiler'] = subprocess.check_output(['cc', '--version'], text=True).splitlines()[0]
    if args.managed:
        variant = 'TranslatedLibsmb2.Raw' if args.raw else 'TranslatedLibsmb2'
        generated = ROOT / 'generated' / variant / 'TranslatedLibsmb2.csproj'
        assembly_name = 'ManagedConsumer' if args.suite == 'sample' else 'ManagedLifecycle'
        sample = ROOT / ('samples/ManagedConsumer/ManagedConsumer.csproj' if args.suite == 'sample'
                         else 'tests/ManagedLifecycle/ManagedLifecycle.csproj')
        managed_output = ROOT / 'build' / (assembly_name + '-' + args.managed + ('-raw' if args.raw else '-processed'))
        command = ['dotnet', 'publish' if args.managed == 'aot' else 'build', sample,
                   '-c', 'Release', '--nologo', '-o', managed_output,
                   '-p:Libsmb2GeneratedProject=' + str(generated)]
        if args.managed == 'aot': command += ['-r', 'linux-x64', '-p:PublishAot=true']
        run(command, OUT / 'managed-build.log', receipt)
        client_command = [managed_output / assembly_name] if args.managed == 'aot' else ['dotnet', managed_output / (assembly_name + '.dll')]
        receipt['managed_suite'] = args.suite
        receipt['managed_runtime'] = args.managed
        receipt['managed_variant'] = variant
        receipt['generated_sha256'] = {p.name: sha(p) for p in generated.parent.glob('*.cs')}
        receipt['managed_sha256'] = {p.name: sha(p) for p in managed_output.iterdir() if p.is_file()}
    image = 'dotcc-libsmb2-samba:4.19.5'
    run(['docker', 'build', '-t', image, ROOT / 'tests/Samba'], OUT / 'image-build.log', receipt)
    image_id = subprocess.check_output(['docker', 'image', 'inspect', '--format', '{{.Id}}', image], text=True).strip()
    receipt['samba_image'] = image_id
    receipt['samba_recipe_sha256'] = {p.name: sha(p) for p in sorted((ROOT / 'tests/Samba').iterdir())}
    password = secrets.token_hex(24)
    fd, temporary = tempfile.mkstemp(prefix='password-', dir=OUT)
    password_path = Path(temporary)
    with os.fdopen(fd, 'w') as secret:
        secret.write(password + '\n')
    container = 'dotcc-libsmb2-' + secrets.token_hex(6)
    run(['docker', 'run', '-d', '--name', container, '--read-only',
         '--tmpfs', '/run', '--tmpfs', '/var/lib/samba', '--tmpfs', '/var/cache/samba',
         '--tmpfs', '/var/log/samba', '--tmpfs', '/srv/share',
         '--mount', f'type=bind,src={password_path},dst=/oracle-password,readonly',
         '-p', '127.0.0.1::445', image_id], OUT / 'container-start.log', receipt)
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
    receipt['samba_version'] = run(['docker', 'exec', container, 'smbd', '--version'], OUT / 'samba-version.log', receipt).strip()
    run(['docker', 'exec', container, 'dpkg-query', '-W'], OUT / 'samba-packages.log', receipt)
    cases = [(dialect + '-sign', dialect, 'sign', 'probe', False, 'accept')
             for dialect in ['0202', '0210', '0300', '0302', '0311']]
    cases += [(dialect + '-encrypt', dialect, 'encrypt', 'encrypted', False, 'accept')
              for dialect in ['0300', '0302', '0311']]
    cases += [('bad-password', '0311', 'sign', 'probe', True, 'reject'),
              ('missing-share', '0311', 'sign', 'missing', False, 'reject'),
              ('smb2-encrypted-share', '0210', 'sign', 'encrypted', False, 'reject')]
    for name, dialect, protection, share, bad_password, expected in cases:
        env = dict(os.environ, LIBSMB2_TEST_PASSWORD='wrong-password' if bad_password else password)
        if args.managed:
            env['LIBSMB2_PASSWORD'] = env['LIBSMB2_TEST_PASSWORD']
            env['LIBSMB2_DOMAIN'] = 'WORKGROUP'
            env['LIBSMB2_TRACE'] = '1'
            command = [*map(str, client_command), f'127.0.0.1:{port}', share, 'smbprobe', dialect]
            if protection == 'encrypt': command += ['--encrypt']
            proc = subprocess.run(command, env=env, capture_output=True, text=True, timeout=45)
            output = (proc.stdout + proc.stderr).strip()
            (OUT / (name + '.log')).write_text(output + '\n')
            receipt.setdefault('commands', []).append(dict(command=command, exit_code=proc.returncode))
            if expected == 'accept':
                if proc.returncode != 0 or not output.startswith('passed:'):
                    raise RuntimeError('Managed case failed: ' + name)
            elif proc.returncode == 0 or 'connect:' not in output:
                raise RuntimeError('Managed case did not reject at connection: ' + name)
        else:
            output = run([client, f'127.0.0.1:{port}', share, dialect, protection, expected],
                         OUT / (name + '.log'), receipt, timeout=30, env=env).strip()
            if not output.startswith('passed:' if expected == 'accept' else 'rejected:'):
                raise RuntimeError('Unexpected client result: ' + name)
        receipt['cases'].append(dict(name=name, passed=True, result=output))
        print(name + ': PASS', flush=True)
    receipt['passed'] = True
except BaseException as error:
    receipt['failure'] = str(error)
    raise
finally:
    if container:
        with (OUT / 'samba.log').open('w') as output:
            subprocess.run(['docker', 'logs', container], stdout=output, stderr=subprocess.STDOUT, timeout=15)
        subprocess.run(['docker', 'rm', '-f', container], stdout=subprocess.DEVNULL,
                       stderr=subprocess.DEVNULL, timeout=30)
    if password_path:
        password_path.unlink(missing_ok=True)
    (OUT / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
