#!/usr/bin/env python3
"""Fresh Lua and chibi JIT conformance using the existing campaign compiler.

Runs the repository's CI source/configuration and conformance recipes in private
source/build copies. This is not an AOT or cross-platform qualification.
"""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import time

ROOT = Path(__file__).resolve().parents[2]
BLINK = ROOT / 'blink'
base = BLINK / 'artifacts/language-regressions'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
work = BLINK / 'generated/language-regressions' / out.name
work.mkdir(parents=True)
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
cli = ROOT / 'DotCC/bin/Release/net10.0/dotcc.dll'
def compiler():
    return {p.name: sha(p) for p in sorted(cli.parent.iterdir())
            if p.is_file() and (p.suffix == '.dll' or p.name.endswith(('.deps.json', '.runtimeconfig.json')))}
names = subprocess.check_output(['git', 'ls-files', 'examples/lua', 'examples/chibi',
    '.github/workflows/lua.yml', '.github/workflows/chibi.yml'], cwd=ROOT, text=True).splitlines()
receipt = dict(kind='fresh-language-jit-regressions', passed=False,
    scope='Existing Lua user-test and chibi R7RS Linux x64 JIT recipes',
    compiler=compiler(), authored={n: sha(ROOT / n) for n in names},
    runner_sha256=sha(Path(__file__)), results={}, commands=[])
env = dict(os.environ, LC_ALL='C', TMPDIR=str(out / 'tmp'))
Path(env['TMPDIR']).mkdir()
def save(): (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
def run(command, label, cwd, timeout=1200, environment=env):
    row = dict(label=label, command=list(map(str, command)), cwd=str(cwd))
    receipt['commands'].append(row)
    save()
    print('RUN ' + label, flush=True)
    start = time.monotonic()
    with (out / (label + '.log')).open('wb') as log:
        result = subprocess.run(row['command'], cwd=cwd, env=environment,
            stdout=log, stderr=subprocess.STDOUT, timeout=timeout)
    row.update(exit_code=result.returncode, seconds=time.monotonic() - start)
    save()
    if compiler() != receipt['compiler']: raise RuntimeError('Compiler identity drift')
    if result.returncode: raise RuntimeError(label + ' failed; see retained log')
    return (out / (label + '.log')).read_text()
def tree(directory):
    return {str(p.relative_to(directory)): sha(p) for p in sorted(directory.rglob('*'))
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
def normalized(text):
    return re.sub(r' in [0-9.]+ seconds', '', re.sub(r'\x1b\[[0-9;]*m', '', text))
save()
try:
    for language in ('lua', 'chibi'):
        case = receipt['results'][language] = dict(passed=False)
        try:
            source = work / (language + '-src')
            shutil.copytree(ROOT / 'examples' / language / (language + '-src'), source,
                ignore=shutil.ignore_patterns('.git', '__pycache__', '*.o', '*.so', '*.dll'))
            case['source_before'] = tree(source)
            output = work / (language + '_managed')
            if language == 'lua':
                units = ('lapi lcode lctype ldebug ldo ldump lfunc lgc llex lmem lobject lopcodes lparser '
                    'lstate lstring ltable ltm lundump lvm lzio lauxlib lbaselib lcorolib ldblib liolib '
                    'lmathlib loadlib loslib lstrlib ltablib lutf8lib linit lua').split()
                flags = ['-I', source]
            else:
                run(['make', '-j4', 'chibi-scheme'], 'chibi-native-bootstrap', source)
                stubs = ['lib/chibi/filesystem.c', 'lib/chibi/io/io.c', 'lib/chibi/process.c', 'lib/chibi/time.c']
                run(['make', *stubs], 'chibi-native-stubs', source)
                case['generated_stubs'] = {n: sha(source / n) for n in stubs}
                units = 'gc sexp bignum gc_heap opcodes vm eval simplify main'.split()
                example = ROOT / 'examples/chibi'
                flags = ['-I', example / 'gen-include', '-I', source / 'include',
                    '-D', 'SEXP_USE_INTTYPES', '-D', 'SEXP_USE_NTPGETTIME', '-D', 'SEXP_USE_DL=0',
                    '-D', 'SEXP_USE_POLL_PORT=0', '-D', 'SEXP_USE_STATIC_LIBS=1',
                    '-D', 'SEXP_USE_STATIC_LIBS_NO_INCLUDE=0', '-I', example / 'gen-lib', '-I', source]
            run(['dotnet', cli, '--emit=build', *flags, *[source / (n + '.c') for n in units],
                '-o', output], language + '-fresh-build', ROOT, 2400)
            executable = output / 'bin/Release/net10.0' / (output.name + '.dll')
            binaries = {p.name: sha(p) for p in executable.parent.glob('*.dll')}
            if language == 'lua':
                transcript = run(['dotnet', executable, '-e', '_U=true', 'all.lua'],
                    'lua-conformance', source / 'testes', 3600)
                if 'final OK !!!' not in transcript: raise RuntimeError('Missing Lua final success marker')
            else:
                transcript = run(['dotnet', executable, 'tests/r7rs-tests.scm'], 'chibi-conformance', source,
                    environment=dict(env, CHIBI_IGNORE_SYSTEM_PATH='1', CHIBI_MODULE_PATH='lib'))
                expected = (ROOT / 'examples/chibi/baseline-r7rs.txt').read_text()
                if normalized(transcript) != normalized(expected) or '1225 out of 1225' not in transcript:
                    raise RuntimeError('chibi transcript differs from committed native baseline')
            if binaries != {p.name: sha(p) for p in executable.parent.glob('*.dll')}:
                raise RuntimeError('Executed binaries changed')
            case.update(passed=True, binaries=binaries, generated=tree(output))
        except Exception as error:
            case['error'] = str(error)
        save()
    receipt['compiler_after'] = compiler()
    receipt['authored_after'] = {n: sha(ROOT / n) for n in names}
    if receipt['compiler_after'] != receipt['compiler'] or receipt['authored_after'] != receipt['authored']:
        raise RuntimeError('Frozen compiler or authored inputs changed')
    receipt['passed'] = all(x['passed'] for x in receipt['results'].values())
finally:
    save()
print(out / 'receipt.json', flush=True)
raise SystemExit(0 if receipt['passed'] else 1)
