#!/usr/bin/env bash
set -euo pipefail
campaign=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
"$campaign/scripts/fetch.sh" "${1:-}"
python3 - "$campaign" <<'PY'
import hashlib, json, os, pathlib, platform, shutil, subprocess, sys, time
root = pathlib.Path(sys.argv[1])
manifest = json.loads((root / 'config/source-manifest.json').read_text())
build = root / 'build/native'
artifacts = root / 'artifacts/native'
artifacts.mkdir(parents=True, exist_ok=True)
# A failed rebuild must not leave a previous successful receipt looking current.
(artifacts / 'receipt.json').unlink(missing_ok=True)
build.mkdir(parents=True, exist_ok=True)
source = build / 'source'
if source.exists():
    shutil.rmtree(source)
shutil.copytree(root / 'ref' / manifest['upstream']['directory'], source)
env = os.environ.copy()
env.update(CC='gcc', AR='ar', LC_ALL='C', TZ='UTC', SOURCE_DATE_EPOCH='1765324800')
for key in ['MODE', 'm', 'CFLAGS', 'CPPFLAGS', 'LDFLAGS', 'UOPFLAGS', 'MAKEFLAGS']:
    env.pop(key, None)
def run(argv, name, cwd=source):
    with (artifacts / name).open('wb') as log:
        subprocess.run(argv, cwd=cwd, env=env, stdout=log, stderr=subprocess.STDOUT, check=True)
def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
tools = {}
for name in ['gcc', 'as', 'ld', 'ar', 'make']:
    path = pathlib.Path(shutil.which(name) or name).resolve()
    tools[name] = dict(path=str(path), sha256=digest(path), version=subprocess.check_output([str(path), '--version'], text=True).splitlines()[0])
run(['./configure'] + manifest['nativeProfile']['configureArguments'], 'configure.log')
make_args = ['make', '-j' + os.environ.get('BLINK_JOBS', '4'), manifest['nativeProfile']['makeTarget'],
    'BLINK_GITSHA=-DBLINK_GITSHA="\\"' + manifest['upstream']['revision'] + '\\""',
    'BLINK_COMMITS=-DBLINK_COMMITS="\\"pinned-archive\\""',
    'BUILD_TIMESTAMP=-DBUILD_TIMESTAMP="\\"2025-12-10T00:00:00Z\\""']
# This explicit native target has no guest GCC/QEMU/download prerequisites.
# Never invoke upstream make check: its test targets download other toolchains.
run(make_args, 'build.log')
binary = source / 'o/blink/blink'
shutil.copy2(binary, build / 'blink')
shutil.copy2(source / 'config.h', artifacts / 'config.h')
shutil.copy2(source / 'config.mk', artifacts / 'config.mk')
run(['ldd', str(binary)], 'ldd.log')
tests = build / 'tests'
tests.mkdir(exist_ok=True)
rows = []
for record in manifest['selectedAssemblyTests']:
    name = pathlib.Path(record['path']).stem
    obj, elf = tests / (name + '.o'), tests / (name + '.elf')
    row = dict(case=name, sourceSha256=digest(source / record['path']))
    try:
        run(['gcc', '-c', '-I.', record['path'], '-o', str(obj)], name + '.compile.log')
        run(['ld', '-static', '--omagic', '-z', 'noexecstack', '-z', 'max-page-size=65536', '-z', 'common-page-size=65536', str(obj), '-o', str(elf)], name + '.link.log')
        row['elfSha256'] = digest(elf)
        for runner, command in [('linux-x64', [str(elf)]), ('blink-interpreter', [str(binary), '-jm', str(elf)])]:
            result = subprocess.run(command, cwd=source, env=env, capture_output=True, timeout=15)
            (artifacts / (name + '.' + runner + '.stdout')).write_bytes(result.stdout)
            (artifacts / (name + '.' + runner + '.stderr')).write_bytes(result.stderr)
            row[runner] = dict(exitCode=result.returncode, stdoutSha256=hashlib.sha256(result.stdout).hexdigest(), stderrSha256=hashlib.sha256(result.stderr).hexdigest())
        row['pass'] = all(row[r]['exitCode'] == 0 for r in ['linux-x64', 'blink-interpreter']) and row['linux-x64']['stdoutSha256'] == row['blink-interpreter']['stdoutSha256']
    except (subprocess.CalledProcessError, subprocess.TimeoutExpired) as error:
        row.update({'pass':False, 'error':str(error)})
    rows.append(row)
receipt = dict(schemaVersion=1, executedAt=time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()), host=platform.platform(), machine=platform.machine(), upstreamRevision=manifest['upstream']['revision'], sourceManifestSha256=digest(root / 'config/source-manifest.json'), tools=tools, configureArguments=manifest['nativeProfile']['configureArguments'], makeCommand=make_args, binarySha256=digest(binary), configSha256=digest(source / 'config.h'), tests=rows)
(artifacts / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(f'Native Blink: {binary}')
print(f'Assembly cases: {sum(row["pass"] for row in rows)}/{len(rows)} passed; receipt: {artifacts / "receipt.json"}')
if not all(row['pass'] for row in rows):
    raise SystemExit(1)
PY
