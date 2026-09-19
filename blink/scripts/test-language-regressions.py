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
    runner_sha256=sha(Path(__file__)), results={}, commands=[],
    repository_head=subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip(),
    provenance_limit='Local vendored bytes and declared fetch recipes; no upstream fetch or new origin-equivalence proof')
shutil.copyfile(Path(__file__), out / 'runner.py')
env = dict(os.environ, LC_ALL='C', TMPDIR=str(out / 'tmp'))
Path(env['TMPDIR']).mkdir()
def save(): (out / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
def run(command, label, cwd, timeout=1200, environment=env):
    path = out / (label + '.log')
    row = dict(label=label, command=list(map(str, command)), cwd=str(cwd),
               log=str(path), timeout_seconds=timeout, exit_code=None)
    receipt['commands'].append(row)
    save()
    print('RUN ' + label, flush=True)
    start = time.monotonic()
    try:
        with path.open('wb') as log:
            result = subprocess.run(row['command'], cwd=cwd, env=environment,
                stdout=log, stderr=subprocess.STDOUT, timeout=timeout)
        row['exit_code'] = result.returncode
    except BaseException as error:
        row['error'] = repr(error)
        row['timed_out'] = isinstance(error, subprocess.TimeoutExpired)
        raise
    finally:
        row['seconds'] = time.monotonic() - start
        if path.is_file():
            row['log_sha256'] = sha(path)
        save()
    if compiler() != receipt['compiler']: raise RuntimeError('Compiler identity drift')
    if result.returncode: raise RuntimeError(label + ' failed; see retained log')
    return path.read_text()

def tree(directory):
    return {str(p.relative_to(directory)): sha(p) for p in sorted(directory.rglob('*'))
            if p.is_file() and not {'bin', 'obj'}.intersection(p.relative_to(directory).parts)}
def file_record(name):
    return dict(path=name, sha256=sha(ROOT / name))


def one_match(pattern, text, label):
    values = re.findall(pattern, text, re.MULTILINE)
    if len(values) != 1:
        raise RuntimeError('Ambiguous/missing vendored provenance: ' + label)
    return values[0]


def provenance(language):
    prefix = 'examples/' + language + '/'
    fetch = prefix + 'fetch.sh'
    recipe = (ROOT / fetch).read_text()
    values = dict(fetch_recipe=file_record(fetch),
        workflow=file_record('.github/workflows/' + language + '.yml'),
        tracked_local_inputs={n: receipt['authored'][n] for n in names if n.startswith(prefix)},
        executed_fetch_recipe=False, origin_verified_against_remote=False)
    if language == 'lua':
        header = (ROOT / 'examples/lua/lua-src/lua.h').read_text()
        version = '.'.join(one_match(r'^#define LUA_VERSION_' + part + r'_N\s+(\d+)$', header, part)
                           for part in ('MAJOR', 'MINOR', 'RELEASE'))
        tag = one_match(r'^LUA_TAG="\$\{LUA_TAG:-([^}]+)\}"$', recipe, 'Lua tag')
        values.update(declared_fetch_tag=tag, declared_fetch_commit=None, version=version,
            version_file=file_record('examples/lua/lua-src/lua.h'),
            suite=file_record('examples/lua/lua-src/testes/all.lua'),
            memory_error_suite=file_record('examples/lua/lua-src/testes/memerr.lua'),
            selection=dict(arguments=['-e', '_U=true', 'all.lua'], upstream_user_test_mode=True,
                interpretation='all.lua sets _soft/_port/_nomsg=true and T=nil; memerr.lua returns before internal allocation-failure tests',
                exclusion='Internal T-based memory-failure tests are skipped by the existing upstream user mode; no authored subset rewrite'),
            expected='Interpreter exit0 and final OK !!!; no fixed test count or fresh native differential')
    else:
        commit = one_match(r'^CHIBI_COMMIT="\$\{CHIBI_COMMIT:-([0-9a-f]{40})\}"', recipe, 'chibi commit')
        values.update(declared_fetch_commit=commit,
            version=(ROOT / 'examples/chibi/chibi-src/VERSION').read_text().strip(),
            release=(ROOT / 'examples/chibi/chibi-src/RELEASE').read_text().strip(),
            version_file=file_record('examples/chibi/chibi-src/VERSION'),
            release_file=file_record('examples/chibi/chibi-src/RELEASE'),
            suite=file_record('examples/chibi/chibi-src/tests/r7rs-tests.scm'),
            native_baseline=file_record('examples/chibi/baseline-r7rs.txt'),
            selection=dict(arguments=['tests/r7rs-tests.scm'], CHIBI_IGNORE_SYSTEM_PATH='1', CHIBI_MODULE_PATH='lib',
                interpretation='Existing upstream R7RS suite including ordinary language exceptions; no custom fault injection'),
            expected='1225 out of1225 and committed native transcript, ANSI color/timing normalized; bootstrap does not rerun native conformance')
    return values


def normalized(text):
    return re.sub(r' in [0-9.]+ seconds', '', re.sub(r'\x1b\[[0-9;]*m', '', text))
save()
try:
    for language in ('lua', 'chibi'):
        case = receipt['results'][language] = dict(passed=False)
        try:
            case['provenance'] = provenance(language)
            source = work / (language + '-src')
            shutil.copytree(ROOT / 'examples' / language / (language + '-src'), source,
                ignore=shutil.ignore_patterns('.git', '__pycache__', '*.o', '*.so', '*.dll'))
            case['source_before'] = tree(source)
            suite = source / ('testes/all.lua' if language == 'lua' else 'tests/r7rs-tests.scm')
            case['executed_suite'] = dict(path=str(suite), sha256=sha(suite))
            if case['executed_suite']['sha256'] != case['provenance']['suite']['sha256']:
                raise RuntimeError('Copied suite differs from recorded vendored source')
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
            case['executed_suite']['sha256_after'] = sha(suite)
            if case['executed_suite']['sha256_after'] != case['executed_suite']['sha256']:
                raise RuntimeError('Executed suite changed')
            case.update(passed=True, binaries=binaries, generated=tree(output))
        except Exception as error:
            case['error'] = str(error)
        save()
    receipt['passed'] = list(receipt['results']) == ['lua', 'chibi'] and all(
        x['passed'] for x in receipt['results'].values())
except BaseException as error:
    receipt['passed'] = False
    receipt['error'] = repr(error)
    raise
finally:
    receipt['final_identity_stable'] = False
    try:
        receipt['compiler_after'] = compiler()
        receipt['authored_after'] = {n: sha(ROOT / n) for n in names}
        receipt['runner_sha256_after'] = sha(Path(__file__))
        receipt['final_identity_stable'] = (receipt['compiler_after'] == receipt['compiler']
            and receipt['authored_after'] == receipt['authored']
            and receipt['runner_sha256_after'] == receipt['runner_sha256'])
        if not receipt['final_identity_stable']:
            receipt['passed'] = False
            receipt['identity_error'] = 'Frozen compiler, authored inputs or runner changed'
    except Exception as error:
        receipt['passed'] = False
        receipt['identity_error'] = repr(error)
    save()
print(out / 'receipt.json', flush=True)
raise SystemExit(0 if receipt['passed'] else 1)
