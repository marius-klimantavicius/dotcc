#!/usr/bin/env python3
"""Build and verify the musl-backed EDG SQLite filesystem/threading profile.

Retains inputs, raw/generated C#, commands and verification logs. Uses the
repository's authored SQLite OS services; no native SQLite dependency is linked.
"""
import argparse
import hashlib
import json
import re
import runpy
import shutil
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SQLITE = ROOT.parent / 'sqlite'
HOOKS = '''
/* EDG managed OS_OTHER/APPDEF port. SQLite algorithms above are unchanged. */
extern const sqlite3_mutex_methods *edg_sqlite_mutex_methods(void);
extern void edg_sqlite_memory_barrier(void);
SQLITE_PRIVATE const sqlite3_mutex_methods *sqlite3DefaultMutex(void) {
  return edg_sqlite_mutex_methods();
}
SQLITE_PRIVATE void sqlite3MemoryBarrier(void) {
  edg_sqlite_memory_barrier();
}
sqlite3_int64 edg_sqlite_mmap_default(void) { return sqlite3Config.szMmap; }
sqlite3_int64 edg_sqlite_mmap_max(void) { return sqlite3Config.mxMmap; }
'''


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def prepare(out):
    preparer = runpy.run_path(str(SQLITE / 'scripts/prepare-host-source.py'))
    source = preparer['prepare'](output=out / 'input', hash_mode='strict')
    engine = source / 'sqlite3.c'
    engine.write_text(engine.read_text() + HOOKS)
    definitions = [line.strip() for line in (SQLITE / 'config/defines.txt').read_text().splitlines()
                   if line.strip() and not line.lstrip().startswith('#')]
    overrides = {'SQLITE_THREADSAFE': '1', 'SQLITE_MUTEX_APPDEF': '1',
                 'SQLITE_TEMP_STORE': '1', 'SQLITE_DEFAULT_MMAP_SIZE': '67108864',
                 'SQLITE_MAX_MMAP_SIZE': '268435456'}
    definitions = [value for value in definitions if value.split('=')[0] not in overrides]
    definitions += [name + '=' + value for name, value in overrides.items()]
    # Public object constants only: exclude annotation/configuration macros and
    # function-pointer sentinels, whose C ABI differs from owner-aware callbacks.
    macros = []
    for name, replacement in re.findall(r'^#define (SQLITE_\w+)[ \t]+([^\n]+)',
                                         (source / 'sqlite3.h').read_text(), re.M):
        if replacement[0] in '0123456789("' and name not in ('SQLITE_STATIC', 'SQLITE_TRANSIENT'):
            macros.append(name)
    return engine, definitions, '|'.join(sorted(set(macros)))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--translate-only', action='store_true')
    parser.add_argument('--resume', action='store_true', help='reuse retained successful translation')
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--process-tests', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=args.resume)
    report_path = out / 'results.json'
    report = json.loads(report_path.read_text()) if args.resume else {
        'schema': 1, 'project': 'EDG SQLite host', 'commands': [],
        'profile': 'musl; OS_OTHER; APPDEF BCL mutexes; THREADSAFE=1; disk temp; WAL; mmap',
        'nativeExtensionLoading': False,
    }
    report['status'] = 'running'
    report.pop('failure', None)

    def save():
        report_path.write_text(json.dumps(report, indent=2) + '\n')

    def run(label, command, timeout=1200):
        command = list(map(str, command))
        log_name = label
        retry = 0
        while (out / (log_name + '.stdout')).exists():
            retry += 1
            log_name = label + '.retry' + str(retry)
        entry = {'label': label, 'command': command, 'logPrefix': log_name}
        report['commands'].append(entry)
        save()
        start = time.monotonic()
        with (out / (log_name + '.stdout')).open('wb') as stdout, (out / (log_name + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=timeout)
        entry.update(exitCode=result.returncode, seconds=round(time.monotonic() - start, 3))
        save()
        if result.returncode:
            raise RuntimeError(f'{label} failed ({result.returncode}); see {out / (log_name + ".stdout")} and .stderr')

    try:
        generated = out / 'generated'
        if not args.resume:
            engine, definitions, macros = prepare(out)
            report.update(definitions=definitions, compiler=str(args.compiler.resolve()),
                          compilerSha256=digest(args.compiler.resolve()), sourceSha256=digest(engine))
            run('translate', [sys.executable, ROOT / 'dotnet.py', engine,
                '--compiler', args.compiler.resolve(), '--language', 'c', '--std', 'c17',
                '--libc', 'musl', '--emit', 'managedlib', '--namespace', 'Edg.SQLite',
                '--class-name', 'SqliteCore', '--instance-methods', '--split', 'size',
                '--split-size', '262144', '--unbound-externals', 'partial',
                '-I', engine.parent, *['-D' + value for value in definitions],
                '--export-macros', macros, '--macro-source', engine,
                '--output-dir', generated, '--raw-output', out / 'raw.cs'])
        if args.translate_only:
            report['status'] = 'translated'
            return
        run('adapter', [sys.executable, ROOT / 'backend/sqlite/prepare.py', '--output-dir', generated])
        run('library-build', ['dotnet', 'build', generated / 'Generated.csproj', '-c', 'Release',
                             '--nologo', '-p:TreatWarningsAsErrors=true'])
        consumer = out / 'consumer'
        shutil.copytree(ROOT / 'backend/tests/sqlite_host', consumer, dirs_exist_ok=True,
                        ignore=shutil.ignore_patterns('bin', 'obj'))
        project = consumer / 'SqliteHostTests.csproj'
        reference = '-p:TranslatedProject=' + str(generated / 'Generated.csproj')
        run('consumer-build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', reference,
                              '-p:TreatWarningsAsErrors=true'])
        managed = ['dotnet', consumer / 'bin/Release/net10.0/SqliteHostTests.dll']
        run('jit-run', managed, timeout=300)
        native = out / 'native-oracle'
        if args.process_tests:
            config = json.loads((SQLITE / 'config/sources.json').read_text())['product']
            source = SQLITE / 'ref' / config['directory']
            run('native-build', ['gcc', '-std=c17', '-O1', '-DSQLITE_THREADSAFE=1',
                '-DSQLITE_OMIT_LOAD_EXTENSION', '-DSQLITE_ENABLE_FTS5', '-I', source,
                source / 'sqlite3.c', SQLITE / 'tests/host_vfs_native.c', '-lm', '-pthread', '-o', native])
            run('jit-processes', [sys.executable, SQLITE / 'scripts/test-host-vfs-processes.py',
                                 '--managed', *managed, '--native', native], timeout=1200)
        if args.nativeaot:
            publish = out / 'aot'
            run('aot-build', ['dotnet', 'publish', project, '-c', 'Release', '-r', args.rid,
                reference, '-p:PublishAot=true', '-p:TreatWarningsAsErrors=true',
                '-p:IlcTreatWarningsAsErrors=true', '--nologo', '-o', publish])
            run('aot-run', [publish / 'SqliteHostTests'], timeout=300)
            if args.process_tests:
                run('aot-processes', [sys.executable, SQLITE / 'scripts/test-host-vfs-processes.py',
                    '--managed', publish / 'SqliteHostTests', '--native', native], timeout=1200)
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        save()


if __name__ == '__main__':
    main()
