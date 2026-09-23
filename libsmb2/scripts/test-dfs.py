#!/usr/bin/env python3
"""Exercise managed referrals against an isolated ordinary Samba DFS fixture (NTLM)."""
import argparse
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import tempfile
import time
from common import ROOT, run, sha

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--raw', action='store_true')
parser.add_argument('--aot', action='store_true')
parser.add_argument('--generated-project', type=Path, help='Override the generated project for a staged check')
args = parser.parse_args()
variant = 'raw' if args.raw else 'processed'
runtime = 'aot' if args.aot else 'jit'
logs = ROOT / 'artifacts/dfs' / (variant + '-' + runtime)
logs.mkdir(parents=True, exist_ok=True)
fixture = ROOT / 'tests/DfsIntegration'
project = ROOT / 'generated' / ('TranslatedLibsmb2.Raw' if args.raw else 'TranslatedLibsmb2') / 'TranslatedLibsmb2.csproj'
if args.generated_project: project = args.generated_project.resolve()
output = ROOT / 'build/dfs' / (variant + '-' + runtime)
receipt = dict(passed=False, authentication='NTLMSSP', generated_project=str(project),
               scope='ordinary standalone DFS referrals; no Kerberos/domain-DFS claim')
container = None
password_path = None
try:
    receipt['sources'] = {str(p.relative_to(ROOT)): sha(p) for directory in (ROOT/'src', fixture)
                          for p in directory.rglob('*') if p.is_file() and not {'bin', 'obj'}.intersection(p.parts)}
    command = ['dotnet', 'publish' if args.aot else 'build', fixture / 'DfsIntegration.csproj',
               '-c', 'Release', '--nologo', '-p:Libsmb2GeneratedProject=' + str(project), '-o', output]
    if args.aot: command += ['-r', 'linux-x64', '-p:PublishAot=true']
    run(command, logs / 'build.log', receipt, timeout=1800)
    run(['docker', 'build', '-t', 'dotcc-libsmb2-samba:4.19.5', ROOT / 'tests/Samba'], logs / 'image.log', receipt)
    receipt['image'] = subprocess.check_output(['docker', 'image', 'inspect', '--format', '{{.Id}}', 'dotcc-libsmb2-samba:4.19.5'], text=True).strip()
    password = secrets.token_hex(24)
    fd, name = tempfile.mkstemp(prefix='password-', dir=logs)
    password_path = Path(name)
    with os.fdopen(fd, 'w') as f: f.write(password + '\n')
    container = 'dotcc-dfs-' + secrets.token_hex(6)
    run(['docker', 'run', '-d', '--name', container, '--read-only', '--tmpfs', '/run', '--tmpfs', '/var/lib/samba',
         '--tmpfs', '/var/cache/samba', '--tmpfs', '/var/log/samba', '--tmpfs', '/srv',
         '-v', str(password_path) + ':/oracle-password:ro',
         '-v', str(fixture/'smb.conf') + ':/etc/samba/smb.conf:ro',
         '-v', str(fixture/'entrypoint.sh') + ':/dfs-entrypoint.sh:ro',
         '--entrypoint', '/bin/sh', 'dotcc-libsmb2-samba:4.19.5', '/dfs-entrypoint.sh'], logs/'container.log', receipt)
    server = subprocess.check_output(['docker', 'inspect', '--format', '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}', container], text=True).strip()
    for attempt in range(100):
        try:
            with socket.create_connection((server, 445), timeout=0.2): break
        except OSError: time.sleep(0.1)
    else: raise RuntimeError('DFS server did not start')
    command = [output/'DfsIntegration'] if args.aot else ['dotnet', output/'DfsIntegration.dll']
    run([*command, server], logs/'run.log', receipt, env={**os.environ, 'LIBSMB2_PASSWORD':password}, timeout=120)
    if any(sha(ROOT/path) != digest for path,digest in receipt['sources'].items()): raise RuntimeError('Authored sources changed during DFS verification')
    receipt['passed'] = True
    print(f'DFS {variant}/{runtime}: PASS')
finally:
    if container:
        subprocess.run(['docker', 'logs', container], stdout=(logs/'samba.log').open('w'), stderr=subprocess.STDOUT)
        subprocess.run(['docker', 'rm', '-f', container], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if password_path: password_path.unlink(missing_ok=True)
    (logs/'result.json').write_text(json.dumps(receipt,indent=2)+'\n')
