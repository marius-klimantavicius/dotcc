#!/usr/bin/env python3
"""Refresh lexical header/type users for the observed native core closure."""
import hashlib
import json
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
profile = root / 'config/managed-host'
source_manifest = json.loads((root / 'config/source-manifest.json').read_text())
ref = root / 'ref' / source_manifest['upstream']['directory']
closure_path = root / 'artifacts/core/closure.json'
closure = json.loads(closure_path.read_text())
pending = [s['path'] for s in closure['sources']]
seen, includes, users = set(), {}, {}
types = ['sigset_t', 'siginfo_t', 'sigjmp_buf', 'jmp_buf', 'stack_t', 'sig_atomic_t',
         'pthread_t', 'pthread_attr_t', 'pthread_mutex_t', 'pthread_once_t', 'nfds_t',
         'socklen_t', 'ssize_t', 'off_t', 'pid_t', 'struct iovec', 'struct termios',
         'struct msghdr', 'struct cmsghdr', 'struct sockaddr', 'struct sockaddr_storage',
         'struct pollfd', 'struct sigaction', 'struct linger', 'struct ucred',
         'struct flock', 'struct timeval', 'struct timezone', 'struct itimerval',
         'rlim_t', 'struct rlimit', 'struct rusage']
while pending:
    name = pending.pop()
    if name in seen:
        continue
    seen.add(name)
    text = (ref / name).read_text()
    for header in re.findall(r'^\s*#\s*include\s*<([^>]+)>', text, re.M):
        includes.setdefault(header, []).append(name)
    for header in re.findall(r'^\s*#\s*include\s*"([^"]+)"', text, re.M):
        if (ref / header).is_file():
            pending.append(header)
    for typename in types:
        if re.search(r'\b' + re.escape(typename) + r'\b', text):
            users.setdefault(typename, []).append(name)
headers = []
for name, sources in sorted(includes.items()):
    status = 'missing-or-native-conditional-requires-profile-review'
    if (root.parent / 'DotCC.Lib/include' / name).is_file():
        status = 'shared-header-exists-needs-semantics-review'
    if (profile / name).is_file():
        status = 'campaign-declaration-only'
    headers.append(dict(header=name, referencedBy=sorted(sources), status=status))
functions = {}
for path in sorted(profile.rglob('*.h')):
    for name, target in re.findall(r'^#define\s+(\w+)\s+(blink_host_\w+)\s*$', path.read_text(), re.M):
        if name not in {'iovec', 'termios', 'pollfd', 'sockaddr', 'sockaddr_storage', 'msghdr', 'cmsghdr', 'linger', 'ucred', 'flock'}:
            functions[name] = dict(target=target, header=str(path.relative_to(profile)),
                                   implementation=('qualified-virtual-mask-unwind-adapter' if name == 'siglongjmp'
                                                   else 'unimplemented-must-remain-unresolved'))
report = dict(kind='core-closure-recursive-lexical-header-and-type-inventory-not-managed-runtime-coverage',
              coreClosureSha256=hashlib.sha256(closure_path.read_bytes()).hexdigest(),
              upstreamRevision=source_manifest['upstream']['revision'],
              headers=headers, typeUsers={k:sorted(v) for k,v in sorted(users.items())},
              redirectedFunctions=functions,
              unsupported=['pthread ABI and guest threads', 'signal delivery and handler lifecycle',
                           'socket and ancillary operation implementations', 'remaining host syscall implementations'])
(profile / 'inventory.json').write_text(json.dumps(report, indent=2)+'\n')
print(f'Inventoried {len(headers)} header names, {len(users)} type names, {len(functions)} unresolved host operations.')
