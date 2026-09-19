#!/usr/bin/env python3
"""Probe unchanged libsmb2 with dotcc's lexer/preprocessor and parser, without IR.

Requires Python 3.12+, .NET 10, CMake, and a native C compiler. Exit 1 indicates
blocked units, not a failed probe invocation. Detailed diagnostics are retained.
"""
import hashlib
import json
from pathlib import Path
import shlex
import subprocess
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
REVISION = '99d5cffc85e4aa8d517649568ff8ec2008e35e90'
ARCHIVE_HASH = 'e67e8803969336a50b3c7063870376f944a0ab9638cd39629fc8fcd76bff9031'
URL = f'https://codeload.github.com/sahlberg/libsmb2/tar.gz/{REVISION}'
SOURCE = ROOT / 'ref' / f'libsmb2-{REVISION}'
OUT = ROOT / 'artifacts/parse-probe'
BUILD = ROOT / 'build/native-probe'
OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'manifest.json').unlink(missing_ok=True)
(ROOT / 'ref').mkdir(exist_ok=True)
archive = ROOT / 'ref' / f'libsmb2-{REVISION}.tar.gz'
if not archive.exists():
    urllib.request.urlretrieve(URL, archive)
assert hashlib.sha256(archive.read_bytes()).hexdigest() == ARCHIVE_HASH, 'Archive hash mismatch'
if not SOURCE.exists():
    with tarfile.open(archive) as bundle:
        bundle.extractall(ROOT / 'ref', filter='data')
# Refuse modified inputs instead of making a false unchanged-source claim.
with tarfile.open(archive) as bundle:
    for member in bundle:
        if member.isfile():
            assert (ROOT / 'ref' / member.name).read_bytes() == bundle.extractfile(member).read(), member.name

commands = []


def run(command, log, timeout=180, cwd=REPO):
    commands.append(dict(command=command, cwd=str(cwd), log=str(log)))
    with log.open('w') as output:
        proc = subprocess.run(command, cwd=cwd, stdout=output,
                              stderr=subprocess.STDOUT, timeout=timeout)
    return proc.returncode


assert run(['cmake', '-S', str(SOURCE), '-B', str(BUILD),
            '-DENABLE_LIBKRB5=OFF', '-DENABLE_GSSAPI=OFF',
            '-DENABLE_LIBDCERPC=OFF', '-DENABLE_EXAMPLES=OFF',
            '-DCMAKE_EXPORT_COMPILE_COMMANDS=ON'], OUT / 'configure.log') == 0
assert run(['dotnet', 'build', str(ROOT / 'tests/ParseProbe/ParseProbe.csproj'),
            '-c', 'Release', '--nologo'], OUT / 'harness-build.log') == 0
compilations = json.loads((BUILD / 'compile_commands.json').read_text())
flags = [shlex.split(entry['command']) for entry in compilations]
defines = [[arg[2:] for arg in command if arg.startswith('-D')] for command in flags]
assert all(value == defines[0] for value in defines), 'Per-unit defines differ'
includes = list(dict.fromkeys(arg[2:] for command in flags for arg in command if arg.startswith('-I')))
# Explicit diagnostic Linux profile selects portable-endian.h's Linux branch.
# This is not a claim that dotcc already implements a Linux host runtime.
request = dict(Units=[entry['file'] for entry in compilations], Includes=includes,
               Defines=defines[0] + ['__linux__=1'], Output=str(OUT / 'results.json'))
(OUT / 'request.json').write_text(json.dumps(request, indent=2) + '\n')

native = []
for entry, command in zip(compilations, flags):
    filtered = []
    skip = False
    for arg in command:
        if skip:
            skip = False
            continue
        if arg == '-o':
            skip = True
        elif arg != '-c':
            filtered.append(arg)
    code = run([*filtered, '-std=c17', '-fsyntax-only'],
               OUT / (Path(entry['file']).stem + '.native.log'), cwd=Path(entry['directory']))
    native.append(dict(unit=entry['file'], exit_code=code))

harness = ROOT / 'tests/ParseProbe/bin/Release/net10.0/DotCC.Tests.dll'
# Self-controls: valid syntax with an unresolved function must pass parsing,
# malformed grammar must fail parsing, and an invalid token must fail lexing.
controls = OUT / 'controls'
controls.mkdir(exist_ok=True)
for name, content in {
    'valid.c': 'int f(void) { return unresolved(1); }\n',
    'parse-error.c': 'int f(void) { return 1 + ; }\n',
    'lex-error.c': 'int f(void) { return `; }\n',
}.items():
    (controls / name).write_text(content)
control_request = dict(Units=[str(controls / name) for name in
    ['valid.c', 'parse-error.c', 'lex-error.c']], Includes=[], Defines=[],
    Output=str(controls / 'results.json'))
(controls / 'request.json').write_text(json.dumps(control_request, indent=2) + '\n')
assert run(['dotnet', str(harness), str(controls / 'request.json')], controls / 'run.log') == 0
checks = json.loads((controls / 'results.json').read_text())
assert all(s['clean'] for s in checks[0]['stages'])
assert checks[1]['stages'][0]['clean'] and not checks[1]['stages'][1]['completed']
assert not checks[2]['stages'][0]['completed']

reducers = sorted((ROOT / 'tests/parse-blockers').glob('*.c'))
reducer_request = dict(Units=[str(p) for p in reducers], Includes=[], Defines=[],
                       Output=str(OUT / 'reducers.json'))
(OUT / 'reducers-request.json').write_text(json.dumps(reducer_request, indent=2) + '\n')
for reducer in reducers:
    assert run(['cc', '-std=c17', '-fsyntax-only', str(reducer)],
               OUT / (reducer.stem + '.native.log')) == 0
assert run(['dotnet', str(harness), str(OUT / 'reducers-request.json')], OUT / 'reducers.log') == 0

assert run(['dotnet', str(harness), str(OUT / 'request.json')], OUT / 'run.log', timeout=600) == 0
results = json.loads((OUT / 'results.json').read_text())
summary = {stage: dict(
    clean=sum(row['stages'][index]['clean'] for row in results),
    completed=sum(row['stages'][index]['completed'] for row in results),
    total=len(results)) for index, stage in enumerate(['preprocess_lex', 'parse'])}
manifest = dict(revision=REVISION, archive_url=URL, archive_sha256=ARCHIVE_HASH,
    repository_commit=subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=REPO, text=True).strip(),
    sdk=subprocess.check_output(['dotnet', '--version'], text=True).strip(),
    tool_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
        for p in harness.parent.glob('*.dll')},
    config_sha256=hashlib.sha256((BUILD / 'config.h').read_bytes()).hexdigest(),
    request=request, commands=commands, native=native, summary=summary,
    reducers=json.loads((OUT / 'reducers.json').read_text()))
(OUT / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print(json.dumps(summary, indent=2))
print(f'Native syntax: {sum(r["exit_code"] == 0 for r in native)}/{len(native)} passed')
print(f'Results: {OUT / "results.json"}')
raise SystemExit(int(any(not stage['clean'] for row in results for stage in row['stages'])
                     or any(row['exit_code'] != 0 for row in native)))
