#!/usr/bin/env python3
"""Verify owned lambda static entries preserve source signatures and exact forwarding."""
import argparse
import importlib.util
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir', 'stock-compiler', 'stock-runtime'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--provider', type=Path, action='append', default=[])
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'lambda_entry.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_lambda_static_entry.cpp'), source)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec); spec.loader.exec_module(packager)
    checks, commands = [], []

    def check(name, value):
        if not value: raise AssertionError(name)
        checks.append(name)

    def run(name, command, success=True):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, (result.returncode == 0) == success)

    def reseal(path, mutate):
        shutil.copy2(obj, path)
        with sqlite3.connect(path) as db:
            mutate(db)
            db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))

    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-' + opt)
            run('native-build-' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', binary])
            run('native-run-' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--instantiate', 'used', '--no_inlining', '--exceptions',
            '--gen_c_file_name', out / 'stock.c', '-o', out / 'stock.il', source])
        for opt in ('0', '2'):
            binary = out / ('stock-' + opt)
            run('stock-build-' + opt, ['gcc', '-w', '-falign-functions=4', '-O' + opt, out / 'stock.c',
                args.stock_runtime.resolve(), '-lstdc++', '-lpthread', '-lm', '-o', binary])
            run('stock-run-' + opt, [binary])
        obj = out / 'unit.dotobj'
        run('export', ['python3', '-B', tooling / 'compile.py', source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        source.rename(out / 'retained-source.cpp')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            entries = db.execute("SELECT entity_id,target_entity_id,closure_source_type_id,entry_source_routine_type_id FROM lambda_entry_origins ORDER BY entity_id").fetchall()
            check('five-owned-static-entries', len(entries) == 5)
            check('one-forwarding-per-entry', db.execute("SELECT count(*) FROM lambda_entry_calls").fetchone()[0] == len(entries))
            check('no-fabricated-source-declarations', db.execute("SELECT count(*) FROM source_declarations d JOIN lambda_entry_origins e ON e.entity_id=d.entity_id OR e.entity_id=d.function_entity_id").fetchone()[0] == 0)
            check('result-buffer-entry-retained', db.execute("SELECT count(*) FROM parameters p JOIN lambda_entry_origins e ON e.entity_id=p.entity_id WHERE p.parameter_role='result-buffer'").fetchone()[0] == 1)
            check('noexcept-original-signature-retained', db.execute("SELECT count(*) FROM lambda_entry_origins e JOIN source_routine_types t ON t.type_id=e.entry_source_routine_type_id WHERE t.exception_policy='terminate'").fetchone()[0] == 1)
            check('indirect-original-parameter-retained', db.execute("SELECT count(*) FROM lambda_entry_origins e JOIN source_routine_abi_parameters p ON p.source_routine_type_id=e.entry_source_routine_type_id WHERE p.parameter_role='indirect-source'").fetchone()[0] == 1)
            entry, target, closure, signature = entries[0]
            call = db.execute("SELECT node_id FROM lambda_entry_calls WHERE entry_entity_id=?", (entry,)).fetchone()[0]
            receiver = db.execute("SELECT n.id FROM edges e JOIN nodes n ON n.id=e.child_id JOIN parameters p ON p.entity_id=? AND p.ordinal=e.ordinal WHERE e.parent_id=? AND e.role='argument' AND p.parameter_role='this'", (target,call)).fetchone()[0]
            check('exact-null-receiver', db.execute("SELECT kind,constant_text FROM nodes WHERE id=?", (receiver,)).fetchone() == ('constant','0'))
            forwarded = db.execute("SELECT n.id,n.binding_id FROM edges e JOIN nodes n ON n.id=e.child_id WHERE e.parent_id=? AND e.role='argument' AND n.kind='binding' ORDER BY e.ordinal LIMIT 1", (call,)).fetchone()
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / 'generated', obj, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'generated'])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        other_entry, other_target, other_closure, other_signature = next(row for row in entries if row[1] != target and row[2] != closure)
        mutations = [
            ('wrong-original-closure', 'UPDATE lambda_entry_origins SET closure_source_type_id=? WHERE entity_id=?', (other_closure,entry), 'Lambda static entry must retain its selected original closure/call signature'),
            ('wrong-selected-original-target', 'UPDATE lambda_entry_origins SET target_entity_id=? WHERE entity_id=?', (other_target,entry), 'Lambda static entry must retain its selected original closure/call signature'),
            ('wrong-original-static-signature', 'UPDATE lambda_entry_origins SET entry_source_routine_type_id=(SELECT e.entry_source_routine_type_id FROM lambda_entry_origins e JOIN source_routine_types r ON r.type_id=e.entry_source_routine_type_id WHERE r.exception_policy=\'terminate\') WHERE entity_id=?', (entry,), 'Lambda static entry must retain its selected original closure/call signature'),
            ('missing-forwarding', 'DELETE FROM lambda_entry_calls WHERE entry_entity_id=?', (entry,), 'Lambda static entry forwarding count differs from its body status'),
        ]
        for name, statement, parameters, diagnostic in mutations:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, parameters))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', diagnostic in (out / ('reject-' + name + '.stderr')).read_text())

        def nonnull_receiver(db):
            new_id = db.execute('SELECT max(id)+1 FROM nodes').fetchone()[0]
            integer = db.execute("SELECT id FROM types WHERE kind='i64'").fetchone()[0]
            db.execute("INSERT INTO nodes(id,function_id,kind,type_id,constant_text,value_category,evaluation_order,line,column) VALUES(?,?,'constant',?,'1','value','none',0,0)", (new_id,entry,integer))
            db.execute("UPDATE nodes SET kind='convert',constant_text=NULL,evaluation_order='unspecified' WHERE id=?", (receiver,))
            db.execute("INSERT INTO edges VALUES(?,'operand',0,?)", (receiver,new_id))
        damaged = out / 'nonnull-closure.dotobj'
        reseal(damaged, nonnull_receiver)
        run('reject-nonnull-closure', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('nonnull-closure-exact-diagnostic', 'Lambda static entry must pass its exact null closure receiver' in (out / 'reject-nonnull-closure.stderr').read_text())

        def duplicate_formal_use(db):
            db.row_factory = sqlite3.Row
            original = dict(db.execute('SELECT * FROM nodes WHERE id=?', (forwarded[0],)).fetchone())
            next_id = db.execute('SELECT max(id)+1 FROM nodes').fetchone()[0]
            original['id'] = next_id
            db.execute('INSERT INTO nodes ('+','.join(original)+') VALUES ('+','.join('?' for _ in original)+')', tuple(original.values()))
            db.execute("INSERT INTO nodes (id,function_id,kind,line,column) VALUES (?,?,'expr-stmt',0,0)", (next_id+1,entry))
            db.execute("INSERT INTO edges VALUES (?,'value',0,?)", (next_id+1,next_id))
            root = db.execute('SELECT body_node_id FROM functions WHERE entity_id=?', (entry,)).fetchone()[0]
            db.execute("UPDATE edges SET ordinal=ordinal+1000 WHERE parent_id=? AND role='statement'", (root,))
            db.execute("UPDATE edges SET ordinal=ordinal-999 WHERE parent_id=? AND role='statement'", (root,))
            db.execute("INSERT INTO edges VALUES (?,'statement',0,?)", (root,next_id+1))
        damaged = out / 'extra-formal-use.dotobj'
        reseal(damaged, duplicate_formal_use)
        run('reject-extra-formal-use', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('extra-formal-use-exact-diagnostic', 'Lambda static entry must forward each unchanged formal once in order' in (out / 'reject-extra-formal-use.stderr').read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
