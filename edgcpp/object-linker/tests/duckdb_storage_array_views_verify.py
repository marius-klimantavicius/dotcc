#!/usr/bin/env python3
"""Verify late-completed major array bounds preserve exact expression/storage views."""
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
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'array_views.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_storage_array_views.cpp'), source)
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
            rows = db.execute("SELECT n.id,n.type_id,s.id,s.type_id,s.source_name FROM nodes n JOIN storages s ON s.id=n.storage_id JOIN types t ON t.id=n.type_id JOIN types d ON d.id=s.type_id WHERE n.kind='storage' AND t.kind='array' AND t.element_count IS NULL AND d.element_count IS NOT NULL AND t.element_type_id=d.element_type_id").fetchall()
            check('three-owned-incomplete-expression-views', len(rows) == 3)
            values = next(row for row in rows if row[4] == 'values')
            exponents = next(row for row in rows if row[4] == 'exponents')
            grid = next(row for row in rows if row[4] == 'grid')
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', out / 'generated', obj])
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        def known_bound_mismatch(db):
            element = db.execute('SELECT element_type_id FROM types WHERE id=?', (values[1],)).fetchone()[0]
            ty = db.execute('SELECT max(id)+1 FROM types').fetchone()[0]
            db.execute("INSERT INTO types(id,kind,pointee_type_id,element_type_id,element_count) VALUES(?,'array',NULL,?,4)", (ty, element))
            db.execute('UPDATE nodes SET type_id=? WHERE id=?', (ty, values[0]))
        def wrong_element(db): db.execute('UPDATE nodes SET type_id=? WHERE id=?', (exponents[1], values[0]))
        def wrong_inner_bound(db):
            inner = db.execute('SELECT element_type_id FROM types WHERE id=?', (grid[1],)).fetchone()[0]
            element = db.execute('SELECT element_type_id FROM types WHERE id=?', (inner,)).fetchone()[0]
            ty = db.execute('SELECT max(id)+1 FROM types').fetchone()[0]
            db.execute("INSERT INTO types(id,kind,pointee_type_id,element_type_id,element_count) VALUES(?,'array',NULL,?,3)", (ty, element))
            db.execute("INSERT INTO types(id,kind,pointee_type_id,element_type_id,element_count) VALUES(?,'array',NULL,?,NULL)", (ty + 1, ty))
            db.execute('UPDATE nodes SET type_id=? WHERE id=?', (ty + 1, grid[0]))
        for name, mutate in [('known-bound-mismatch', known_bound_mismatch), ('element-mismatch', wrong_element), ('inner-bound-mismatch', wrong_inner_bound)]:
            damaged = out / (name + '.dotobj'); reseal(damaged, mutate)
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            diagnostic = (out / ('reject-' + name + '.stderr')).read_text()
            check(name + '-diagnostic', 'Storage place does not match its declaration type' in diagnostic or
                name != 'known-bound-mismatch' and 'Array decay requires an array place and pointer to its element' in diagnostic)
        def reverse_view(db):
            db.execute('UPDATE nodes SET type_id=? WHERE id=?', (values[3], values[0]))
            db.execute("UPDATE storages SET type_id=?,definition='declaration' WHERE id=?", (values[1], values[2]))
            db.execute('DELETE FROM storage_data WHERE storage_id=?', (values[2],))
        reverse = out / 'known-view-unknown-external.dotobj'; reseal(reverse, reverse_view)
        run('verify-known-view-unknown-external', ['dotnet', args.linker.resolve(), 'verify', reverse])
        run('link-known-view-unknown-external', ['dotnet', args.linker.resolve(), 'link', '--allow-externs', '--out', out / 'external-view', reverse])
        report = json.loads((out / 'external-view/link-report.json').read_text())
        obligation = next(row for row in report['runtimeStorageRequirements'] if row['SourceName'] == 'values')
        check('known-view-retains-provider-extent', obligation['size'] == 24 and obligation['declaredSize'] == 0)
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
