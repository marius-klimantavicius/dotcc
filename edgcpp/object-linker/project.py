#!/usr/bin/env python3
"""Compile a project as independent original-source semantic .dotobj files.

Every attempt retains its compiler, command, diagnostics, wire and SQLite object.
The coordinator holds only receipts; each worker releases its semantic wire after
packaging one TU. No preprocessing subprocess or whole-program EDG merge is used.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import gzip
import importlib.util
import json
import os
from pathlib import Path
import re
import shutil
import signal
import sqlite3
import subprocess
import sys
import threading
import time

HERE = Path(__file__).resolve().parent
EDG = HERE.parent


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def save(path, value):
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, indent=2) + '\n')
    temporary.replace(path)


def environment(edg_base=None):
    # Never inherit legacy-backend selectors from a developer's shell.
    env = {k: v for k, v in os.environ.items() if not k.startswith('EDG_')}
    env.update(EDG_BASE=str(edg_base or EDG / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_OBJECT='1', EDG_DOT_TARGET='portable-lp64-binary80-v2',
               EDG_DOT_NAMESPACE='@DotObject', EDG_DOT_CLASS='@Unit',
               EDG_DOT_EMIT='managedlib', EDG_DOT_INSTANCE='0', EDG_DOT_NEST='1',
               EDG_DOT_LIBC='none', EDG_DOT_RUNTIME='none', EDG_DOT_POSIX='0')
    # This opt-in changes only rejection diagnostics, never translation policy.
    # The campaign fingerprint below records it alongside the fixed selectors.
    if os.environ.get('EDG_DOTOBJ_DIAG_SOURCE_ERRORS') == '1':
        env['EDG_DOTOBJ_DIAG_SOURCE_ERRORS'] = '1'
    return env


def frontend_arguments(arguments):
    """Accept semantic frontend options, never codegen/input/preprocess switches."""
    paths = []
    index = 0
    while index < len(arguments):
        arg = arguments[index]
        index += 1
        if arg in ('-I', '--preinclude'):
            if index == len(arguments):
                raise ValueError('Missing value for ' + arg)
            path = Path(arguments[index])
            index += 1
            if not path.is_absolute() or arg == '--preinclude' and not path.is_file():
                raise ValueError('Frontend paths must be absolute; preincludes must exist: ' + str(path))
            paths.append(path)
        elif arg in ('-D', '-U'):
            if index == len(arguments) or not arguments[index]:
                raise ValueError('Missing macro argument')
            index += 1
        elif arg.startswith(('-D', '-U')) and len(arg) > 2:
            pass
        elif arg in ('--no_exceptions', '--exceptions', '--no_rtti', '--rtti'):
            pass
        else:
            raise ValueError('Unsupported semantic frontend argument: ' + arg)
    return paths


def load_manifest(path):
    manifest = json.loads(path.read_text())
    if manifest.get('schema') != 1 or not isinstance(manifest.get('units'), list) or not manifest['units']:
        raise ValueError('Expected schema 1 with a nonempty units list')
    seen = set()
    for unit in manifest['units']:
        if set(unit) - {'id', 'source', 'standard', 'arguments', 'role'}:
            raise ValueError('Unknown unit fields')
        if not isinstance(unit['id'], str) or not unit['id'] or unit['id'] in seen:
            raise ValueError('Unit IDs must be nonempty and unique')
        seen.add(unit['id'])
        source = Path(unit['source'])
        if not source.is_absolute() or not source.is_file() or source.suffix in ('.i', '.ii'):
            raise ValueError('Expected an absolute original-source path: ' + str(source))
        if unit['standard'] not in ('c11', 'c17', 'c++17', 'c++20'):
            raise ValueError('Unsupported source standard')
        frontend_arguments(unit['arguments'])
    return manifest


def inventory(units):
    """Include directory contents catch added/shadowing headers on resume too."""
    roots = {Path(environment()['EDG_BASE']), EDG / 'backend/runtime'}
    files = {Path(unit['source']) for unit in units}
    for unit in units:
        # Quoted includes can be relative to the original source directory.
        roots.add(Path(unit['source']).parent)
        for path in frontend_arguments(unit['arguments']):
            if path.is_dir() or not path.exists():
                roots.add(path)
            else:
                files.add(path)
                roots.add(path.parent)
    minimal = [root for root in roots if not any(root != p and root.is_relative_to(p) for p in roots)]
    visited = set()
    entries = {}
    pending = list(minimal)
    while pending:
        root = pending.pop()
        entries[str(root)] = dict(directory=root.is_dir(), link=str(root.readlink()) if root.is_symlink() else None)
        real = root.resolve()
        if real in visited:
            continue
        visited.add(real)
        for path in root.rglob('*'):
            if path.is_symlink():
                entries[str(path)] = dict(link=str(path.readlink()))
                if path.is_dir():
                    pending.append(path.resolve())
            if path.is_file():
                files.add(path)
    for path in sorted(files):
        before = path.stat()
        digest = sha(path)
        after = path.stat()
        if (before.st_size, before.st_mtime_ns, before.st_ctime_ns) != (after.st_size, after.st_mtime_ns, after.st_ctime_ns):
            raise ValueError('Input changed during inventory: ' + str(path))
        entries.setdefault(str(path), {}).update(sha256=digest, mtimeNs=after.st_mtime_ns)
    return entries


def packager():
    spec = importlib.util.spec_from_file_location('dotobj_compile', HERE / 'compile.py')
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def compile_unit(request_path):
    request = json.loads(request_path.read_text())
    unit, work = request['unit'], request_path.parent
    source, compiler = Path(unit['source']), Path(request['compiler'])
    if request.get('memoryLimitMiB') and os.name == 'posix':
        import resource
        limit = request['memoryLimitMiB'] * 1024 * 1024
        resource.setrlimit(resource.RLIMIT_AS, (limit, limit))
    packaging_hashes = (sha(HERE / 'compile.py'), sha(HERE / 'schema.sql'))
    module = packager()
    command = [str(compiler), '--' + unit['standard'], '--gnu_version', '130300',
               '--clear_flag', 'use_predefined_macro_file']
    if unit['standard'].startswith('c++'):
        command += ['--instantiate', 'used']
    # A single reproducible clock applies to all TUs and resumed attempts.
    command += ['-D__DATE__="Jan  1 1970"', '-D__TIME__="00:00:00"', *unit['arguments'], '--', str(source)]
    env = environment(request.get('edgBase'))
    env['EDG_DOT_OBJECT_UNIT_ID'] = unit['id']
    started_ns = time.time_ns()
    before = sha(source)
    compression = request.get('wireCompression', 'none')
    if compression not in ('none', 'gzip'):
        raise ValueError('Unsupported wire compression')
    wire = work / ('wire.json.gz' if compression == 'gzip' else 'wire.json')
    invocation = dict(command=command, sourceSha256=before, compilerSha256=sha(compiler),
                      wireCompression=compression, wire=str(wire),
                      environment={k: v for k, v in env.items() if k.startswith('EDG_')})
    save(work / 'invocation.json', invocation)
    with wire.open('wb') as stdout, (work / 'stderr').open('wb') as stderr:
        if compression == 'gzip':
            process = subprocess.Popen(command, cwd=work, env=env, stdout=subprocess.PIPE, stderr=stderr)
            try:
                with gzip.GzipFile(filename='', mode='wb', fileobj=stdout, compresslevel=1, mtime=0) as encoded:
                    shutil.copyfileobj(process.stdout, encoded, 1024 * 1024)
            except BaseException:
                process.kill()
                process.wait()
                raise
            finally:
                process.stdout.close()
            code = process.wait()
        else:
            code = subprocess.run(command, cwd=work, env=env, stdout=stdout, stderr=stderr).returncode
    invocation['frontendExitCode'] = code
    save(work / 'invocation.json', invocation)
    if code:
        raise RuntimeError('Frontend failed: ' + str(work / 'stderr'))
    if sha(source) != before or sha(compiler) != invocation['compilerSha256']:
        raise ValueError('Source/compiler changed during compilation')
    partial, output = work / 'object.partial.sqlite', work / 'unit.dotobj'
    provenance = [('producer', 'dot-gen-owned-semantic-project-v1'),
                  ('compiler-sha256', invocation['compilerSha256']),
                  ('arguments', json.dumps(command[1:])), ('packager-sha256', packaging_hashes[0]),
                  ('schema-sha256', packaging_hashes[1]), ('project-driver-sha256', sha(__file__)),
                  ('environment', json.dumps(invocation['environment'], sort_keys=True))]
    if compression == 'gzip':
        with gzip.open(wire, 'rt', encoding='utf-8') as stream:
            packaged = module.package_entries(module.WireReader(stream).entries(), partial, source, unit['id'], provenance, started_ns)
    else:
        packaged = module.package_file(wire, partial, source, unit['id'], provenance, started_ns)
    if packaging_hashes != (sha(HERE / 'compile.py'), sha(HERE / 'schema.sql')):
        raise ValueError('Packager/schema changed during compilation')
    os.link(partial, output)
    save(work / 'result.json', dict(status='packaged', object=str(output), sha256=sha(output),
         frontendInvocations=1, profile=packaged['profile'], counts=packaged['counts']))


def object_unchanged(result):
    path = Path(result['object'])
    if not path.is_file() or sha(path) != result['sha256']:
        return False
    with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
        for source, digest in db.execute('SELECT path,sha256 FROM source_files WHERE sha256 IS NOT NULL'):
            if not Path(source).is_file() or sha(source) != digest:
                return False
    return True


def build(args):
    manifest_path, out = args.manifest.resolve(), args.output_dir.resolve()
    manifest = load_manifest(manifest_path)
    units = [u for u in manifest['units'] if re.search(args.select, u['source'])]
    if args.limit is not None:
        units = units[:args.limit]
    if not units:
        raise ValueError('No source units selected')
    out.mkdir(parents=True, exist_ok=args.resume)
    work_roots = [path.resolve() for path in args.work_root] or [out]
    if len(work_roots) != len(set(work_roots)):
        raise ValueError('Repeated work root')
    for root in work_roots:
        if root != out:
            root.mkdir(parents=True, exist_ok=True)
    inputs = inventory(units)
    fingerprint = dict(manifestSha256=sha(manifest_path), selectedIds=[u['id'] for u in units],
                       compilerSha256=sha(args.compiler), driverSha256=sha(__file__),
                       packagerSha256=sha(HERE / 'compile.py'), schemaSha256=sha(HERE / 'schema.sql'),
                       wireCompression=args.wire_compression,
                       inputs=inputs, environment={k: v for k, v in environment(args.edg_base.resolve()).items()
                           if k.startswith(('EDG_', 'LC_')) or k in ('LANG', 'TZ', 'CPATH', 'C_INCLUDE_PATH', 'CPLUS_INCLUDE_PATH')})
    fingerprint_path = out / 'inputs.json'
    if args.resume:
        if json.loads(fingerprint_path.read_text()) != fingerprint:
            raise ValueError('Compiler, manifest, headers, source, options or packaging changed; use a fresh output directory')
        report = json.loads((out / 'results.json').read_text())
    else:
        save(fingerprint_path, fingerprint)
        shutil.copy2(args.compiler, out / 'cpfe-dot')
        report = dict(schema=1, project=manifest.get('project'), totalManifestUnits=len(manifest['units']),
                      selectedUnits=len(units), completeSelection=len(units) == len(manifest['units']),
                      status='running', units={}, executionVerified=False, nativeAotVerified=False)
    if sha(out / 'cpfe-dot') != fingerprint['compilerSha256']:
        raise ValueError('Retained compiler changed')
    report['status'] = 'running'
    report['workRoots'] = list(map(str, work_roots))
    save(out / 'results.json', report)
    allocation_lock = threading.Lock()

    def run(unit):
        key = unit['id']
        previous = report['units'].get(key)
        if previous and previous.get('status') == 'packaged' and object_unchanged(previous):
            return key, previous
        # A scheduling floor, not a per-process disk limit: already running
        # workers finish and retain their evidence. A resumed campaign can
        # lower the floor or use newly available space without changing inputs.
        stem = hashlib.sha256(key.encode()).hexdigest()[:24]
        with allocation_lock:
            spaces = [(shutil.disk_usage(root).free, root) for root in work_roots]
            free, root = max(spaces, key=lambda item: item[0])
            if free < args.min_free_space_mib * 1024 * 1024:
                return key, dict(status='deferred', source=unit['source'],
                                 reason='minimum-free-space', freeBytes=free,
                                 freeSpaceByRoot={str(path): available for available, path in spaces},
                                 minimumFreeBytes=args.min_free_space_mib * 1024 * 1024)
            attempt = 1
            while any((path / f'{stem}-{attempt:03d}').exists() for path in work_roots):
                attempt += 1
            work = root / f'{stem}-{attempt:03d}'
            work.mkdir()
        request = work / 'request.json'
        save(request, dict(unit=unit, compiler=str(out / 'cpfe-dot'), memoryLimitMiB=args.memory_limit_mib,
                           wireCompression=args.wire_compression, edgBase=str(args.edg_base.resolve())))
        start = time.monotonic()
        timed_out = False
        with (work / 'worker.stdout').open('wb') as stdout, (work / 'worker.stderr').open('wb') as stderr:
            process = subprocess.Popen([sys.executable, '-B', str(Path(__file__).resolve()), '_unit', str(request)],
                                       stdout=stdout, stderr=stderr, start_new_session=os.name == 'posix')
            try:
                code = process.wait(timeout=args.timeout)
            except subprocess.TimeoutExpired:
                timed_out = True
                if os.name == 'posix':
                    os.killpg(process.pid, signal.SIGKILL)
                else:
                    process.kill()
                code = process.wait()
        result = json.loads((work / 'result.json').read_text()) if code == 0 else dict(status='failed')
        result.update(source=unit['source'], work=str(work), exitCode=code, timeout=timed_out,
                      seconds=round(time.monotonic() - start, 3))
        invocation_path = work / 'invocation.json'
        invocation = json.loads(invocation_path.read_text()) if invocation_path.exists() else {}
        result['frontendExitCode'] = invocation.get('frontendExitCode')
        if code:
            result['failureStage'] = 'package' if result['frontendExitCode'] == 0 else 'frontend-or-export'
            diagnostics = [path.read_text(errors='replace')[-4000:]
                           for path in (work / 'stderr', work / 'worker.stderr') if path.exists()]
            result['diagnostic'] = '\n'.join(value for value in diagnostics if value)
        return key, result

    with ThreadPoolExecutor(max_workers=args.jobs) as pool:
        futures = [pool.submit(run, unit) for unit in units]
        for future in as_completed(futures):
            key, result = future.result()
            report['units'][key] = result
            save(out / 'results.json', report)
            print(result['status'].upper() + ' ' + result['source'], flush=True)
    # A changed include lookup must not be published as a successful campaign.
    if (inventory(units) != inputs or sha(__file__) != fingerprint['driverSha256'] or
            sha(HERE / 'compile.py') != fingerprint['packagerSha256'] or
            sha(HERE / 'schema.sql') != fingerprint['schemaSha256']):
        report['status'] = 'inputs-changed'
    else:
        report['packagedUnits'] = sum(x['status'] == 'packaged' for x in report['units'].values())
        report['deferredUnits'] = sum(x['status'] == 'deferred' for x in report['units'].values())
        report['status'] = ('packaged' if report['packagedUnits'] == len(units)
                            else 'storage-limited' if report['deferredUnits'] else 'failed')
    save(out / 'results.json', report)
    if report['status'] != 'packaged':
        return 1
    objects = [report['units'][u['id']]['object'] for u in units]
    save(out / 'objects.json', objects)
    return 0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest='command', required=True)
    worker = commands.add_parser('_unit', help=argparse.SUPPRESS)
    worker.add_argument('request', type=Path)
    project = commands.add_parser('build')
    project.add_argument('--manifest', type=Path, required=True)
    project.add_argument('--output-dir', type=Path, required=True)
    project.add_argument('--work-root', action='append', type=Path, default=[],
                         help='Retain TU artifacts here; repeat to use the root with most free space')
    project.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    project.add_argument('--edg-base', type=Path, default=EDG / 'upstream/bases/docker/dev-env/gcc')
    project.add_argument('--wire-compression', choices=('none', 'gzip'), default='none',
                         help='Retain a lossless streamed wire archive instead of expanded JSON')
    project.add_argument('--resume', action='store_true')
    project.add_argument('--select', default='.*')
    project.add_argument('--limit', type=int)
    project.add_argument('--jobs', type=int, default=1)
    project.add_argument('--timeout', type=float, default=600)
    project.add_argument('--memory-limit-mib', type=int, default=8192,
                         help='per-worker address-space cap on POSIX; 0 disables')
    project.add_argument('--min-free-space-mib', type=int, default=0,
                         help='defer new workers below this free-space floor; running workers finish; 0 disables')
    args = parser.parse_args()
    if args.command == '_unit':
        compile_unit(args.request)
        return 0
    if (args.jobs < 1 or args.timeout <= 0 or args.memory_limit_mib < 0 or
            args.min_free_space_mib < 0 or args.limit is not None and args.limit < 1):
        parser.error('jobs, timeout and limit must be positive; memory and free-space limits must be nonnegative')
    return build(args)


if __name__ == '__main__':
    raise SystemExit(main())
