#!/usr/bin/env python3
"""Verify virtual wrappers forward caller-owned result storage without copies or lifetimes."""
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
    source = out / 'wrapper_result.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_wrapper_result_buffer.cpp'), source)
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
            rows = db.execute("SELECT w.entity_id,w.target_entity_id,p.ordinal FROM thunk_wrappers w JOIN parameters p ON p.entity_id=w.entity_id WHERE p.parameter_role='result-buffer'").fetchall()
            check('one-owned-indirect-result-wrapper', len(rows) == 1)
            wrapper, target, result_ordinal = rows[0]
            check('void-wrapper-and-target', db.execute("SELECT count(*) FROM entities e JOIN types t ON t.id=e.return_type_id WHERE e.id IN (?,?) AND t.kind='void'", (wrapper,target)).fetchone()[0] == 2)
            check('nonzero-this-adjustment-no-result-adjustment', db.execute("SELECT this_delta<>0 AND return_delta=0 AND vbase_index=0 FROM thunk_wrappers WHERE entity_id=?", (wrapper,)).fetchone()[0] == 1)
            forwarding = db.execute("SELECT node_id FROM wrapper_calls WHERE wrapper_entity_id=?", (wrapper,)).fetchone()[0]
            argument = db.execute("SELECT child_id FROM edges WHERE parent_id=? AND role='argument' AND ordinal=?", (forwarding,result_ordinal)).fetchone()[0]
            check('result-pointer-exact-formal-forwarding', db.execute("SELECT count(*) FROM nodes n JOIN bindings b ON b.id=n.binding_id WHERE n.id=? AND n.kind='binding' AND b.ordinal=? AND b.function_id=?", (argument,result_ordinal,wrapper)).fetchone()[0] == 1)
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / 'generated', obj, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'generated'])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        mutations = [
            ('result-buffer-adjustment', 'UPDATE thunk_wrappers SET return_delta=1 WHERE entity_id=?', (wrapper,), 'Virtual wrapper result buffer requires the same original indirect return without result adjustment'),
            ('missing-forwarding', 'DELETE FROM wrapper_calls WHERE wrapper_entity_id=?', (wrapper,), 'Virtual wrapper forwarding count differs from declaration/definition status'),
        ]
        for name, statement, parameters, diagnostic in mutations:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, parameters))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', diagnostic in (out / ('reject-' + name + '.stderr')).read_text())

        def duplicate_result_use(db):
            db.row_factory = sqlite3.Row
            original = dict(db.execute('SELECT * FROM nodes WHERE id=?', (argument,)).fetchone())
            next_id = db.execute('SELECT max(id)+1 FROM nodes').fetchone()[0]
            original['id'] = next_id
            db.execute('INSERT INTO nodes ('+','.join(original)+') VALUES ('+','.join('?' for _ in original)+')', tuple(original.values()))
            db.execute("INSERT INTO nodes (id,function_id,kind,line,column) VALUES (?,?,'expr-stmt',0,0)", (next_id+1,wrapper))
            db.execute("INSERT INTO edges VALUES (?,'value',0,?)", (next_id+1,next_id))
            root = db.execute('SELECT body_node_id FROM functions WHERE entity_id=?', (wrapper,)).fetchone()[0]
            db.execute("UPDATE edges SET ordinal=ordinal+1000 WHERE parent_id=? AND role='statement'", (root,))
            db.execute("UPDATE edges SET ordinal=ordinal-999 WHERE parent_id=? AND role='statement'", (root,))
            db.execute("INSERT INTO edges VALUES (?,'statement',0,?)", (root,next_id+1))
        damaged = out / 'extra-result-buffer-use.dotobj'
        reseal(damaged, duplicate_result_use)
        run('reject-extra-result-buffer-use', ['dotnet', args.linker.resolve(), 'verify', damaged], False)
        check('extra-result-buffer-use-exact-diagnostic', 'Virtual wrapper must use its unchanged result buffer only in the selected forwarding call' in (out / 'reject-extra-result-buffer-use.stderr').read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
