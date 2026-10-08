"""Observed default-constructor adapters, real array runtime and source-free linking."""
from pathlib import Path
import hashlib
import shutil
import sqlite3
import sys


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    source_directory = out / 'default-adapter-source'
    source_directory.mkdir()
    source = source_directory / 'project_cpp_default_ctor_adapter.cpp'
    shutil.copy2(inputs / source.name, source)
    obj = compile_source('cpp-default-adapters', source)
    run('verify-cpp-default-adapters', ['dotnet', tool, 'verify', obj])
    with sqlite3.connect(obj) as db:
        adapters = db.execute("""
            SELECT a.entity_id,a.primary_entity_id,a.target_entity_id,e.linkage,
                   own.exception_policy,original.exception_policy
            FROM abi_routine_origins a JOIN entities e ON e.id=a.entity_id
            JOIN routine_types own ON own.type_id=e.routine_type_id
            JOIN entities p ON p.id=a.primary_entity_id
            JOIN routine_types original ON original.type_id=p.routine_type_id
            WHERE a.entry_kind='constructor-default-adapter'
            """).fetchall()
        check('default-adapter-observed-owned-origins', len(adapters) > 0 and all(row[2] is not None and row[3] == 'internal' for row in adapters))
        check('default-adapter-distinct-private-wrappers', len(adapters) >= 2 and len({row[0] for row in adapters}) == len(adapters) and len({row[1] for row in adapters}) == 1)
        check('default-adapter-independent-exception-boundary', any(row[4] == 'unwind' and row[5] == 'terminate' for row in adapters))
        check('default-adapter-original-default-facts', db.execute(
            'SELECT count(*) FROM source_parameters WHERE has_default_argument=1').fetchone()[0] > 0)
        check('default-adapter-no-fabricated-declarations', db.execute("""
            SELECT count(*) FROM source_declarations d JOIN abi_routine_origins a
            ON a.entity_id=d.entity_id OR a.entity_id=d.function_entity_id
            WHERE a.entry_kind='constructor-default-adapter'
            """).fetchone()[0] == 0)
        adapter, primary = adapters[0][:2]
        main = db.execute("SELECT id FROM entities WHERE source_name='main'").fetchone()[0]

    for optimization in ['0', '2']:
        native = out / ('default-adapters-native-' + optimization)
        run('default-adapters-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, source, '-o', native])
        run('default-adapters-native-' + optimization, [native])

    generated = out / 'link-default-adapters'
    run('link-default-adapters', ['dotnet', tool, 'link', '--allow-externs', '--out', generated, '--entry', 'main', obj])
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in generated.glob('*.cs')}
    run('bind-default-adapters', [sys.executable, Path(__file__).resolve().parents[1] / 'runtime/bind.py', '--project', generated])
    execute('default-adapters', generated)

    source_directory.rename(out / 'retained-default-adapter-source')
    source_free = out / 'link-default-adapters-source-free'
    run('link-default-adapters-source-free', ['dotnet', tool, 'link', '--allow-externs', '--out', source_free, '--entry', 'main', obj])
    check('default-adapter-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in source_free.glob('*.cs')})

    mutations = [
        ('default-adapter-missing-default',
         [('UPDATE source_parameters SET has_default_argument=0 WHERE entity_id=?', (primary,))],
         'omitted parameter has no original default'),
        ('default-adapter-unrelated-target',
         [('UPDATE abi_routine_origins SET target_entity_id=? WHERE entity_id=?', (main, adapter))],
         'target must be its primary'),
        ('default-adapter-external-identity',
         [("UPDATE entities SET linkage='external',external_key='forged-default-adapter' WHERE id=?", (adapter,))],
         'owned private body'),
        ('default-adapter-forged-receiver-source',
         [("UPDATE bindings SET is_synthetic=0 WHERE function_id=? AND kind='parameter' AND ordinal=0", (adapter,))],
         'bindings must be synthetic'),
    ]
    for name, sql, diagnostic in mutations:
        bad = mutate(name, obj, sql)
        run('reject-' + name, ['dotnet', tool, 'verify', bad], None, diagnostic)
    return obj
