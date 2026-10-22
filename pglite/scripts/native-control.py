"""Native-only PGlite control. Import run(ctx, suite=None) from the recipe."""
import argparse
import importlib.util
import json
from pathlib import Path
import shlex
import struct
import sys
import time
from types import SimpleNamespace


def preparation():
    spec = importlib.util.spec_from_file_location('pglite_native_prepare', Path(__file__).with_name('prepare.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def run(ctx, suite=None):
    module = preparation()
    info = module.prepare(ctx)
    module.build_native(ctx, info)
    return workflow(ctx, info)


def link_command(output, target):
    candidates = [shlex.split(line) for line in output.splitlines()
                  if line.startswith(('gcc ', 'cc ', 'clang '))]
    candidates = [line for line in candidates if '-o' in line and line[line.index('-o') + 1] == target]
    if len(candidates) != 1:
        raise RuntimeError('Expected one upstream linker command for ' + target)
    return candidates[0]


def parse_wire(data):
    offset, rows, commands, errors, ready, fields = 0, [], [], [], [], []
    while offset < len(data):
        if len(data) - offset < 5:
            raise AssertionError('Truncated backend frame')
        kind = chr(data[offset])
        length = struct.unpack_from('!I', data, offset + 1)[0]
        if length < 4 or offset + 1 + length > len(data):
            raise AssertionError('Invalid backend frame length')
        body = data[offset + 5:offset + 1 + length]
        if kind == 'D':
            count = struct.unpack_from('!H', body)[0]
            pos, row = 2, []
            for _ in range(count):
                size = struct.unpack_from('!i', body, pos)[0]
                pos += 4
                if size == -1:
                    row.append(None)
                elif size >= 0 and pos + size <= len(body):
                    row.append(body[pos:pos + size].decode('utf-8'))
                    pos += size
                else:
                    raise AssertionError('Invalid row field length')
            if pos != len(body):
                raise AssertionError('Trailing row bytes')
            rows.append(row)
        elif kind == 'C':
            if not body.endswith(b'\0'):
                raise AssertionError('Unterminated command tag')
            commands.append(body[:-1].decode('utf-8'))
        elif kind == 'E':
            pos, error = 0, {}
            while pos < len(body) and body[pos]:
                tag = chr(body[pos]); pos += 1
                end = body.index(0, pos)
                error[tag] = body[pos:end].decode('utf-8'); pos = end + 1
            errors.append(error)
        elif kind == 'Z':
            if len(body) != 1 or body not in (b'I', b'T', b'E'):
                raise AssertionError('Invalid transaction readiness')
            ready.append(body.decode())
        elif kind not in ('T', 'N', 'S', 'A'):
            raise AssertionError('Unexpected message kind ' + repr(kind))
        offset += 1 + length
    return dict(rows=rows, commands=commands, errors=errors, ready=ready)


def compare(case, observed):
    if observed['rows'] != case.get('rows', []):
        raise AssertionError(case['name'] + ': wrong rows: ' + repr(observed['rows']))
    if observed['ready'] != [case['ready']]:
        raise AssertionError(case['name'] + ': wrong transaction state: ' + repr(observed['ready']))
    expected_errors = [case['error']] if 'error' in case else []
    if [x.get('C') for x in observed['errors']] != expected_errors:
        raise AssertionError(case['name'] + ': wrong SQLSTATE: ' + repr(observed['errors']))
    if observed['commands'] != case.get('commands', []):
        raise AssertionError(case['name'] + ': wrong completion: ' + repr(observed['commands']))


def workflow(ctx, info):
    stage, prefix = Path(info['source']), Path(info['prefix'])
    work = ctx.work / 'native-control'
    work.mkdir(parents=True, exist_ok=True)
    tests = Path(ctx.root) / 'tests/native'
    ctx.run(['cc', '-std=gnu11', '-O0', '-c', tests / 'initdb_host.c', '-o', work / 'initdb-host.o'],
            'native-initdb-host-compile', 120)
    text = ctx.run(['make', '-n', '-C', stage / 'src/bin/initdb', '-W', 'initdb.o', 'initdb'],
                   'native-initdb-link-plan', 120, cwd=stage)
    init_link = link_command(text, 'initdb')
    initdb = prefix / 'bin/initdb-control'
    init_link[init_link.index('-o') + 1] = str(initdb)
    init_link += [str(work / 'initdb-host.o')]
    ctx.run(init_link, 'native-initdb-host-link', 120, cwd=stage / 'src/bin/initdb')
    ctx.run(['cc', '-std=gnu11', '-O0', '-c', tests / 'embedding_control.c', '-o', work / 'embedding-control.o'],
            'native-embedding-host-compile', 120)
    ctx.run(['objcopy', '--redefine-sym', 'main=pgl_postgres_main',
             stage / 'src/backend/main/main.o', work / 'backend-main.o'], 'native-main-symbol', 120)
    text = ctx.run(['make', '-n', '-C', stage / 'src/backend', 'quick'], 'native-backend-link-plan', 120, cwd=stage)
    backend_link = link_command(text, 'postgres')
    if backend_link.count('main/main.o') != 1:
        raise RuntimeError('Expected one upstream backend main object')
    backend_link[backend_link.index('main/main.o')] = str(work / 'backend-main.o')
    exe = work / 'embedding-control'
    backend_link[backend_link.index('-o') + 1] = str(exe)
    backend_link += [str(work / 'embedding-control.o'), '-Wl,--wrap=exit']
    ctx.run(backend_link, 'native-embedding-host-link', 120, cwd=stage / 'src/backend')
    data = work / 'data'
    ctx.run([initdb, '-D', data, '-L', prefix / 'share/postgresql', '--no-locale',
             '--encoding=UTF8', '--auth=trust', '-U', 'postgres', '--no-sync'],
            'native-initdb', 300, env={'LC_ALL': 'C', 'TZ': 'UTC'})
    cases = json.loads((tests / 'cases.json').read_text())
    observations = []
    for phase, inventory in cases.items():
        directory = work / phase
        directory.mkdir()
        queries = directory / 'queries.sql'
        queries.write_text(''.join(row['sql'] + '\n' for row in inventory))
        ctx.run([exe, prefix / 'bin/postgres', data, queries, directory], 'native-embedded-' + phase, 180,
                env={'LC_ALL': 'C', 'TZ': 'UTC'})
        responses = sorted(directory.glob('*.wire'))
        if len(responses) != len(inventory):
            raise AssertionError('Wrong native response inventory for ' + phase)
        for case, path in zip(inventory, responses):
            observed = parse_wire(path.read_bytes())
            compare(case, observed)
            observations.append(dict(name=case['name'], phase=phase, status='passed',
                                     observed=observed, transcript=str(path)))
    # Prove wrong output is rejected, and a native harness error is not a pass.
    try:
        compare(dict(name='deliberate-wrong-row', rows=[['wrong']], ready='I'),
                dict(rows=[['actual']], ready=['I'], commands=[], errors=[]))
    except AssertionError:
        pass
    else:
        raise AssertionError('Assertion-negative control was accepted')
    ctx.run([exe], 'native-deliberate-harness-error', 30, check=False)
    if ctx.receipt['commands'][-1].get('exit_code') != 24:
        raise AssertionError('Harness error did not propagate exit 24')
    report = dict(status='passed', profile=info['profile'], classification=info['classification'],
                  passed=len(observations), failed=0, cases=observations,
                  setup='native initdb with test-only subprocess helpers',
                  execution='SQL executes in-process in the test host through PGlite callback transport',
                  persistence='clean shutdown and reopening in fresh native host processes',
                  same_process_reinitialization='unrun; fresh native process per lifetime',
                  managed_execution='unrun', upstream_wasm_parity=False,
                  negative_controls=['wrong row rejected', 'harness exit 24 propagated'])
    path = ctx.artifacts / 'native-control-report.json'
    path.write_text(json.dumps(report, indent=2) + '\n')
    ctx.receipt['pglite_native_control'] = dict(report=str(path), passed=len(observations), status='passed')
    ctx.save()
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--hashes', choices=('warn', 'off', 'strict'), default='warn')
    parser.add_argument('--fetch', choices=('missing', 'never'), default='never')
    stage_selection = parser.add_mutually_exclusive_group()
    stage_selection.add_argument('--prepare-only', action='store_true')
    stage_selection.add_argument('--build-only', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / 'Scripts'))
    from campaigns.runner import Context
    from campaigns.model import Recipe
    options = SimpleNamespace(profile='embedding-native', action='native-control',
                              hashes=args.hashes, fetch=args.fetch, restore='none')
    module = preparation()
    recipe = Recipe('pglite', 'TranslatedPGlite', ('embedding-native',), (), module.sources)
    ctx = Context(root.parent, recipe, options)
    # Standalone specialist runs retain framework command/cancellation/receipt
    # behavior without changing the supervisor's current campaign pointer.
    ctx.artifacts = root / 'artifacts/native-control' / ctx.run_id
    ctx.work = root / 'build/native-control/runs' / ctx.run_id
    ctx.receipt['staging'] = str(ctx.work.relative_to(root))
    scope = ('native preparation only; SQL unrun' if args.prepare_only else
             'native build only; SQL unrun' if args.build_only else
             'native embedded SQL control; managed SQL unrun')
    ctx.receipt['scope'] = scope
    ctx.receipt['selection'].update(prepare_only=args.prepare_only, build_only=args.build_only)
    ctx.env['TMPDIR'] = str(ctx.work / 'tmp')
    ctx.start()
    try:
        info = module.prepare(ctx)
        if not args.prepare_only:
            module.build_native(ctx, info)
            if not args.build_only:
                workflow(ctx, info)
        ctx.receipt['status'] = 'passed'
    except BaseException as exc:
        ctx.receipt.update(status='failed', error=str(exc))
        raise
    finally:
        ctx.receipt['finished'] = time.time()
        ctx.save()
        (root / 'artifacts/native-control/latest.json').write_text(json.dumps({
            'status': ctx.receipt['status'], 'scope': scope,
            'receipt': str(ctx.artifacts / 'receipt.json'),
            'control': ctx.receipt.get('pglite_native_control')}, indent=2) + '\n')


if __name__ == '__main__':
    main()
