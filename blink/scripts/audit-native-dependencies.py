#!/usr/bin/env python3
"""Audit the actual native CLI link; this does not select a managed closure."""
import collections
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import subprocess

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'build/native/source'
OUT = ROOT / 'artifacts/dependencies'
OUT.mkdir(parents=True, exist_ok=True)
ENV = dict(os.environ, LC_ALL='C')


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(command, log=None):
    result = subprocess.check_output(command, cwd=SOURCE, env=ENV, text=True)
    if log:
        (OUT / log).write_text(result)
    return result


def symbols(path):
    result = []
    for line in run(['nm', '-P', str(path)]).splitlines():
        fields = line.split()
        if len(fields) >= 2:
            result.append((fields[0], fields[1]))
    return result


def host_class(name):
    if name in {'_Exit', '_exit', 'exit', 'abort', 'fork', 'execv', 'waitpid', 'kill', 'pause', 'prctl'}:
        return 'process-lifecycle'
    if name in {'__sigsetjmp', '_setjmp', 'longjmp', 'siglongjmp'}:
        return 'nonlocal-control-flow'
    if name.startswith(('sig', '__libc_current_sigrt')):
        return 'signals'
    if name in {'socket', 'accept', 'bind', 'connect', 'listen', 'shutdown', 'getsockname', 'getpeername', 'getsockopt', 'setsockopt', 'recvmsg', 'sendmsg', 'sockatmark', 'inet_ntop'}:
        return 'network'
    if name == 'poll':
        return 'readiness'
    if name.startswith(('tc', 'cfget', 'cfset')) or name == 'ioctl':
        return 'terminal-device'
    if name in {'mmap64', 'munmap', 'mprotect', 'msync', 'malloc', 'calloc', 'realloc', 'free', 'posix_memalign'}:
        return 'memory'
    if name.startswith(('clock_', 'gettime', 'localtime', 'nanosleep')) or name in {'time', 'times', 'getitimer', 'setitimer', 'alarm'}:
        return 'clock-timer'
    if name.startswith(('getuid', 'geteuid', 'getgid', 'getegid', 'getpid', 'getppid', 'getpgid', 'getres', 'getgroups', 'getrlimit', 'getrusage', 'gethostname', 'getdomainname', 'setuid', 'setgid', 'setegid', 'setgroups', 'setres', 'setrlimit', 'setpgid', 'sched_')) or name in {'getenv', 'getsid', 'setsid', 'sysconf', 'sysinfo'}:
        return 'identity-environment-resource'
    if name == 'syscall':
        return 'raw-host-syscall-needs-callsite-audit'
    if name.startswith(('__ctype', '__errno', 'mem', 'str', 'stpcpy', 'sqrt')) or name in {'qsort', 'snprintf', 'vsnprintf', 'perror', 'setlocale'}:
        return 'libc-algorithm-locale-error'
    if name.startswith(('_ITM_', '__cxa_', '__gmon_', '__libc_start_', '__stack_chk_')):
        return 'native-runtime-toolchain'
    if name.startswith(('open', 'close', 'read', 'write', 'pread', 'pwrite', 'lseek', 'fstat', 'fchmod', 'fchown', 'faccess', 'fdopen', 'fcntl', 'ftruncate', 'fdatasync', 'fsync', 'futimens', 'mkdir', 'mkfifo', 'linkat', 'symlink', 'unlink', 'rename', 'utimens', 'statvfs', 'dup')) or name in {'chdir', 'fchdir', 'getcwd', 'flock', 'pipe', 'realpath', 'rewinddir', 'seekdir', 'telldir', 'sync', 'umask'}:
        return 'filesystem-descriptor'
    return 'unclassified-review-required'


DIAGNOSTICS = {'debug', 'debug2', 'demangle', 'dis', 'disarg', 'diself', 'disfree', 'disinst', 'disspec', 'describeflags', 'describehosterrno', 'describeprot', 'describesignal', 'log', 'logcpu', 'strace', 'stats', 'pml4tfmt', 'formatint64thousands', 'formatsize', 'high', 'cp437'}
CLI = {'blink', 'getopt', 'commandv', 'startdir', 'prog'}
UI = {'blinkenlights', 'panel', 'cga', 'mda', 'readansi', 'pty', 'strwidth', 'wcwidth'}


def disposition(stem):
    if stem in CLI:
        return 'exclude-cli-entry-or-helper'
    if stem in UI:
        return 'exclude-tui-device'
    if stem in DIAGNOSTICS:
        return 'diagnostic-optional-review-and-adapt'
    return 'embedding-candidate-needs-reachability-and-host-review'


native = ROOT / 'build/native/blink'
receipt = json.loads((ROOT / 'artifacts/native/receipt.json').read_text())
if digest(native) != receipt['binarySha256']:
    raise SystemExit('native binary does not match its receipt; rebuild native oracle')
lines = (ROOT / 'artifacts/native/build.log').read_text().splitlines()
command = next(shlex.split(line) for line in reversed(lines)
               if line.startswith('gcc ') and line.endswith('-o o//blink/blink'))
command[-1] = str(OUT / 'blink-audit')
command.append('-Wl,-Map,' + str(OUT / 'native-link.map'))
run(command, 'relink.log')
if digest(OUT / 'blink-audit') != digest(native):
    raise SystemExit('audit relink differs from tested native binary; preserve logs and investigate')

link_map = (OUT / 'native-link.map').read_text()
extraction_text = link_map.split('Merging program properties', 1)[0].split('As-needed library', 1)[0]
# GNU ld wraps long archive member names before the reference that extracted it.
extractions = {}
for match in re.finditer(r'^o//blink/blink\.a\(([^)]+)\)\s+([^\n]+?) \(([^\n]+)\)$', extraction_text, re.M):
    extractions[match[1]] = {'referencedBy':match[2].strip(), 'triggerSymbol':match[3]}
if not extractions:
    raise SystemExit('no archive extractions parsed from GNU ld map')
archive = SOURCE / 'o/blink/blink.a'
members = run(['ar', 't', str(archive)], 'archive-members.txt').splitlines()
run(['nm', '-A', '-P', str(archive)], 'archive-symbols.txt')
run(['readelf', '-Ws', str(native)], 'native-symbols.txt')
run(['readelf', '-dW', str(native)], 'native-dynamic.txt')
dynamic = run(['nm', '-D', '--undefined-only', '-P', str(native)], 'native-imports.txt')

objects = {}
all_definitions = set()
for member in sorted(set(extractions) | {'blink.o'}):
    obj = SOURCE / 'o/blink' / member
    rows = symbols(obj)
    # A local definition cannot satisfy another object's external reference.
    defined = {name for name, kind in rows if (kind.isupper() and kind != 'U') or kind == 'u'}
    all_definitions.update(defined)
    undefined = sorted(name for name, kind in rows if kind in {'U', 'w', 'v'})
    sections = {}
    for line in run(['readelf', '-SW', str(obj)]).splitlines():
        match = re.match(r'\s*\[\s*(\d+)\]\s+(\S+)\s+\S+\s+[0-9a-f]+\s+[0-9a-f]+\s+[0-9a-f]+\s+[0-9a-f]+\s+(\S+)', line)
        if match:
            sections[match[1]] = (match[2], match[3])
    state = []
    for line in run(['readelf', '-Ws', str(obj)]).splitlines():
        fields = line.split()
        if len(fields) != 8 or fields[3] not in {'OBJECT', 'TLS'}:
            continue
        section, flags = sections.get(fields[6], ('', ''))
        if 'W' not in flags:
            continue
        state.append({'symbol':fields[7], 'size':int(fields[2]), 'binding':fields[4],
                      'threadLocal':fields[3] == 'TLS', 'section':section,
                      'category':'relocation-readonly-candidate' if section.startswith('.data.rel.ro') else 'mutable-storage-candidate'})
    objects[member] = {'source':'blink/' + Path(member).with_suffix('.c').name,
                       'sourceSha256':digest(SOURCE / 'blink' / Path(member).with_suffix('.c')),
                       'objectSha256':digest(obj), 'disposition':disposition(Path(member).stem),
                       'extractedBy':extractions.get(member), 'undefinedSymbols':undefined,
                       'stateCandidates':state}
users = collections.defaultdict(list)
for member, obj in objects.items():
    for name in obj['undefinedSymbols']:
        if name not in all_definitions:
            users[name].append(member)
imports = []
for line in dynamic.splitlines():
    name, kind, *_ = line.split()
    plain = name.split('@')[0]
    imports.append({'symbol':name, 'binding':'weak' if kind == 'w' else 'strong',
                    'category':host_class(plain), 'referencedBy':users.get(plain, []),
                    'origin':'blink-object-reference' if plain in users else 'native-linker-startup-or-toolchain'})
for obj in objects.values():
    obj['unresolvedHostSymbols'] = [name for name in obj.pop('undefinedSymbols') if name not in all_definitions]

summary = {'archiveMembers':len(members), 'extractedArchiveMembers':len(extractions),
           'unextractedArchiveMembers':len(set(members)-set(extractions)),
           'cliEntryObjects':1, 'dynamicImports':len(imports),
           'hostObjectSymbols':len(users),
           'mutableStorageCandidates':sum(s['category']=='mutable-storage-candidate' for obj in objects.values() for s in obj['stateCandidates']),
           'relocationReadonlyCandidates':sum(s['category']=='relocation-readonly-candidate' for obj in objects.values() for s in obj['stateCandidates'])}
report = {'schemaVersion':1, 'kind':'observed-native-cli-link-not-managed-embedding-closure',
          'upstreamRevision':receipt['upstreamRevision'], 'binarySha256':digest(native),
          'archiveSha256':digest(archive), 'nativeConfigSha256':digest(SOURCE / 'config.h'),
          'sourceManifestSha256':digest(ROOT / 'config/source-manifest.json'),
          'summary':summary, 'objects':objects,
          'unextractedArchiveMembers':sorted(set(members)-set(extractions)),
          'embeddingCandidatesExcludingCliTui':[obj['source'] for obj in objects.values() if not obj['disposition'].startswith('exclude-')],
          'explicitCliTuiExclusions':['blink/' + name + '.c' for name in sorted(CLI | UI)],
          'dynamicImports':imports,
          'unresolvedObjectSymbols':dict(sorted(users.items())),
          'notes':['Archive extraction is object-granularity link evidence, not function or workload execution reachability.',
                   'CLI/TUI exclusions are proposed embedding dispositions; removing them requires adapting references, not deleting source files blindly.',
                   'Native undefined symbols resolve through libc/libm/startup; none is permission to retain native imports in the managed product.',
                   'Writable OBJECT/TLS symbols are candidates; data.rel.ro pointer tables are reported separately and source writes require review.']}
(ROOT / 'config/native-closure.json').write_text(json.dumps(report, indent=2)+'\n')
(OUT / 'audit-command.json').write_text(json.dumps(command, indent=2)+'\n')
print(json.dumps(summary, indent=2))
print('Host classes:', dict(collections.Counter(row['category'] for row in imports)))
