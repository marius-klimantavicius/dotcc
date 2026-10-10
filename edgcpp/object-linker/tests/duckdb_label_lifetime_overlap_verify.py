#!/usr/bin/env python3
"""Verify goto label lifetime overlaps retain exact predecessor destruction ordering."""
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
    source = out / 'label_lifetime.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_label_lifetime_overlap.cpp'), source)
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
            rows = db.execute("SELECT l.id,l.parent_lifetime_id,l.parent_obligation_id,o.lifetime_id FROM source_lifetimes l JOIN source_destruction_obligations o ON o.id=l.parent_obligation_id WHERE l.kind='block-after-label'").fetchall()
            check('one-observed-label-overlap', len(rows) == 1)
            label, parent, obligation, obligation_owner = rows[0]
            check('overlap-belongs-to-exact-predecessor', parent == obligation_owner)
            foreign = db.execute('SELECT id FROM source_destruction_obligations WHERE lifetime_id=?', (label,)).fetchone()[0]
            try_child = db.execute("SELECT l.id FROM source_lifetimes l JOIN source_lifetimes p ON p.id=l.parent_lifetime_id WHERE l.kind='block' AND p.kind='try-block'").fetchone()[0]
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
            ('foreign-destruction-sublist', 'UPDATE source_lifetimes SET parent_obligation_id=? WHERE id=?', (foreign, label), "Original lifetime overlap must reference its parent's destruction sublist"),
            ('nonblock-label-predecessor', "UPDATE source_lifetimes SET kind='block-after-label' WHERE id=?", (try_child,), 'Original label lifetime requires its block or label predecessor'),
        ]
        for name, statement, parameters, diagnostic in mutations:
            damaged = out / (name + '.dotobj')
            reseal(damaged, lambda db: db.execute(statement, parameters))
            run('reject-' + name, ['dotnet', args.linker.resolve(), 'verify', damaged], False)
            check(name + '-exact-diagnostic', diagnostic in (out / ('reject-' + name + '.stderr')).read_text())
    except Exception as exception:
        error = str(exception)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checkCount=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
