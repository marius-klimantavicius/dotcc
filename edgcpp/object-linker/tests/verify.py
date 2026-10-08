#!/usr/bin/env python3
"""Real independent EDG objects, managed linking, JIT/AOT and rejection checks."""
import argparse
import hashlib
import importlib.util
import json
import platform
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location('dotobj_compile', ROOT / 'compile.py')
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    for name in ['fixture.h', 'provider.cpp', 'consumer.cpp', 'scopes.cpp', 'conversions.cpp', 'empty.cpp', 'loops.cpp']:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)
    shutil.copy2(ROOT / 'tests/loops.cpp', inputs / 'loops-c.c')
    checks = []

    def save(status):
        (out / 'results.json').write_text(json.dumps(dict(status=status, checks=checks,
            profile='scalar-control-v2', nativeAotRequested=args.nativeaot,
            platform=platform.platform(), implementation='fresh-owned-semantic-graph',
            limitations=['No prelower source view, macro history, lifetime/EH or globals yet.',
                         'No complete DuckDB or general C++ acceptance.']), indent=2) + '\n')

    def run(label, command, expected=0, contains=None):
        command = [str(x) for x in command]
        result = subprocess.run(command, cwd=out, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        passed = (result.returncode != 0 if expected is None else result.returncode == expected)
        if contains is not None:
            passed = passed and contains.lower() in (result.stdout + result.stderr).lower()
        checks.append(dict(name=label, command=command, exitCode=result.returncode, passed=passed))
        save('running' if passed else 'failed')
        if not passed:
            raise RuntimeError(label + ' failed; inspect retained logs at ' + str(out))
        print(label + ': passed', flush=True)
        return result

    def check(label, condition):
        checks.append(dict(name=label, passed=bool(condition)))
        save('running' if condition else 'failed')
        if not condition:
            raise RuntimeError(label)

    def compile_source(name, source, output=None, expected=0, unit_id=None, std='c++17'):
        output = output or out / (name + '.dotobj')
        run('compile-' + name, [sys.executable, '-B', ROOT / 'compile.py', source,
            '--output', output, '--work-dir', out / (name + '-compile'), '--unit-id', unit_id or name, '--std', std], expected)
        return output

    try:
        run('tool-build', ['dotnet', 'build', ROOT / 'managed/DotObj.Linker.csproj',
            '-c', 'Release', '--artifacts-path', out / 'tool'])
        tool = out / 'tool/bin/DotObj.Linker/release/dotobj-link.dll'
        if not tool.exists():
            raise RuntimeError('Expected linker build output missing')
        provider = compile_source('provider', inputs / 'provider.cpp')
        consumer = compile_source('consumer', inputs / 'consumer.cpp')
        scopes = compile_source('scopes', inputs / 'scopes.cpp')
        for name, obj in [('provider', provider), ('consumer', consumer), ('scopes', scopes)]:
            run('verify-' + name, ['dotnet', tool, 'verify', obj])
            invocation = json.loads((out / (name + '-compile/invocation.json')).read_text())
            check('direct-source-' + name, invocation['arguments'][-1] == str(inputs / (name + '.cpp'))
                  and '--preprocess' not in invocation['arguments'])
        run('native-build', ['c++', '-std=c++17', inputs / 'provider.cpp', inputs / 'consumer.cpp', '-o', out / 'native'])
        run('native-run', [out / 'native'])
        run('native-scopes-build', ['c++', '-std=c++17', inputs / 'scopes.cpp', '-o', out / 'native-scopes'])
        run('native-scopes-run', [out / 'native-scopes'])

        def link(label, objects, entry='main', namespace='Tests.Linked', cls='Core', expected=0, contains=None):
            directory = out / label
            run(label, ['dotnet', tool, 'link', '--out', directory, '--namespace', namespace,
                        '--class', cls, '--entry', entry, *objects], expected, contains)
            return directory

        def execute(label, directory, expected=0):
            run(label + '-build', ['dotnet', 'build', directory / 'Translated.csproj', '-c', 'Release'])
            run(label + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Translated.dll'], expected)
            if args.nativeaot:
                machine = {'x86_64': 'x64', 'aarch64': 'arm64'}.get(platform.machine())
                system = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}.get(platform.system())
                if not machine or not system:
                    raise RuntimeError('Unrecognized NativeAOT host RID')
                publish = out / (label + '-aot')
                run(label + '-publish', ['dotnet', 'publish', directory / 'Translated.csproj',
                    '-c', 'Release', '-r', system + '-' + machine, '-p:PublishAot=true', '-o', publish])
                run(label + '-aot-run', [publish / ('Translated.exe' if system == 'win' else 'Translated')], expected)

        linked = link('link', [provider, consumer])
        execute('linked', linked)
        scoped = link('link-scopes', [scopes])
        execute('scopes', scoped)
        for name in ['conversions', 'empty', 'loops']:
            obj = compile_source(name, inputs / (name + '.cpp'))
            run('native-' + name + '-build', ['c++', '-std=c++17', inputs / (name + '.cpp'), '-o', out / ('native-' + name)])
            run('native-' + name + '-run', [out / ('native-' + name)])
            execute(name, link('link-' + name, [obj]))
        loops_object = out / 'loops.dotobj'
        loops_cs = '\n'.join(p.read_text() for p in (out / 'link-loops').glob('*.cs'))
        check('structured-loop-source', all(token in loops_cs for token in ['for (', 'do', 'while (', 'break;', 'continue;']) and 'goto ' not in loops_cs)
        with sqlite3.connect(loops_object) as db:
            check('explicit-loop-targets', db.execute("SELECT count(*) FROM nodes WHERE kind IN ('break','continue')").fetchone()[0] > 0 and db.execute("SELECT count(*) FROM nodes n LEFT JOIN edges e ON e.parent_id=n.id AND e.role='target' LEFT JOIN nodes t ON t.id=e.child_id WHERE n.kind IN ('break','continue') AND (t.id IS NULL OR t.kind NOT IN ('for','while','do-while'))").fetchone()[0] == 0)
        loops_c = compile_source('loops-c', inputs / 'loops-c.c', std='c11')
        run('native-loops-c-build', ['cc', '-std=c11', inputs / 'loops-c.c', '-o', out / 'native-loops-c'])
        run('native-loops-c-run', [out / 'native-loops-c'])
        execute('loops-c', link('link-loops-c', [loops_c]))
        reverse = link('link-reversed', [consumer, provider])
        cs = lambda path: {p.name: p.read_bytes() for p in path.glob('*.cs')}
        check('stable-output-input-order', cs(linked) == cs(reverse))

        # Objects contain source snapshots. Linking must not reopen source paths.
        inputs.rename(out / 'inputs-preserved')
        relocated = link('link-without-sources', [provider, consumer], namespace='Different.Output', cls='Engine')
        execute('without-sources', relocated)
        check('source-files-remain-unavailable', not inputs.exists())

        run('unresolved-import', ['dotnet', tool, 'link', '--out', out / 'bad-unresolved', consumer], None, 'Unresolved')
        run('duplicate-unit', ['dotnet', tool, 'link', '--out', out / 'bad-unit', provider, provider], None, 'translation unit')

        def mutate(name, original, sql, repair=True):
            path = out / (name + '.dotobj')
            shutil.copy2(original, path)
            with sqlite3.connect(path) as db:
                for query, params in sql:
                    db.execute(query, params)
                if repair:
                    db.execute('UPDATE content_digest SET sha256=?', (MODULE.logical_hash(db),))
            return path

        duplicate = mutate('duplicate', provider, [('UPDATE object_info SET unit_id=?', ('duplicate',))])
        run('duplicate-strong', ['dotnet', tool, 'link', '--out', out / 'bad-strong', provider, duplicate], None, 'Duplicate strong')
        mismatch = mutate('signature', consumer, [('UPDATE parameters SET type_id=3 WHERE entity_id=(SELECT id FROM entities WHERE source_name=?)', ('from_a',))])
        run('signature-mismatch', ['dotnet', tool, 'link', '--out', out / 'bad-signature', provider, mismatch], None, 'Signature mismatch')
        damaged = mutate('damaged', provider, [('UPDATE entities SET source_name=? WHERE id=1', ('tampered',))], False)
        run('digest-mismatch', ['dotnet', tool, 'verify', damaged], None, 'digest')
        bad_target = mutate('target', provider, [('UPDATE object_info SET target_abi=?', ('unknown-target',))])
        run('unknown-target', ['dotnet', tool, 'verify', bad_target], None)
        bad_edge = mutate('edge', provider, [('UPDATE edges SET role=? WHERE rowid=(SELECT min(rowid) FROM edges)', ('invented',))])
        run('invalid-edge', ['dotnet', tool, 'verify', bad_edge], None)
        bad_kind = mutate('node', provider, [('UPDATE nodes SET kind=? WHERE id=(SELECT min(id) FROM nodes)', ('raw-csharp',))])
        run('invalid-node-kind', ['dotnet', tool, 'verify', bad_kind], None)
        bad_snapshot = mutate('snapshot', provider, [('UPDATE source_files SET bytes=? WHERE id=1', (b'changed',))])
        run('snapshot-mismatch', ['dotnet', tool, 'verify', bad_snapshot], None)
        bad_primary = mutate('primary-hash', provider, [('UPDATE object_info SET source_sha256=?', ('0' * 64,))])
        run('primary-provenance-mismatch', ['dotnet', tool, 'verify', bad_primary], None)
        bad_schema = mutate('schema', provider, [('ALTER TABLE nodes ADD COLUMN hidden_behavior TEXT', ())])
        run('extra-schema-payload', ['dotnet', tool, 'verify', bad_schema], None)

        wrong_target = mutate('loop-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT body_node_id FROM functions WHERE entity_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('nonloop-exit-target', ['dotnet', tool, 'verify', wrong_target], None)
        missing_target = mutate('loop-missing-target', loops_object, [(
            "DELETE FROM edges WHERE role='target' AND parent_id=(SELECT min(id) FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('missing-loop-exit-target', ['dotnet', tool, 'verify', missing_target], None)
        outer_target = mutate('loop-outer-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT min(id) FROM nodes WHERE kind='for' AND function_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue')) AND child_id IN (SELECT id FROM nodes WHERE kind='for')", ())])
        run('wrong-lexical-loop-target', ['dotnet', tool, 'verify', outer_target], None)

        negatives = {
            'global': 'int storage=1; int f(){return storage;}\n',
            'pointer': 'int f(int* value){return *value;}\n',
            'volatile': 'int f(){volatile int value=1; return value;}\n',
            'inline': 'inline int f(){return 1;} int g(){return f();}\n',
            'noexcept': 'int f() noexcept {return 1;}\n',
            'switch': 'int f(int n){switch(n){case 1:return 2;default:return 3;}}\n',
            'goto': 'int f(){int n=0; again: ++n; if(n<3)goto again; return n;}\n',
        }
        for name, code in negatives.items():
            source = out / ('unsupported-' + name + '.cpp')
            source.write_text(code)
            obj = compile_source('unsupported-' + name, source, expected=None)
            check('not-published-' + name, not obj.exists())

        # One changed TU can be recompiled and relinked with the unchanged other object.
        shutil.copytree(out / 'inputs-preserved', inputs)
        changed = inputs / 'provider.cpp'
        changed.write_text(changed.read_text().replace('event + 1', 'event + 2'))
        changed_object = compile_source('provider-new', changed, unit_id='provider')
        incremental = link('link-incremental', [changed_object, consumer])
        run('native-new-build', ['c++', '-std=c++17', changed, inputs / 'consumer.cpp', '-o', out / 'native-new'])
        run('native-new-run', [out / 'native-new'], 3)
        execute('incremental', incremental, 3)
        save('passed')
        print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
    except Exception:
        save('failed')
        raise


if __name__ == '__main__':
    main()
