#!/usr/bin/env python3
"""Check function-designator lvalue adjustments without adding data-pointer indirection."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'stock-compiler', 'stock-runtime', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    parser.add_argument('--native-aot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    source = out / 'duckdb_function_reference_adjust.cpp'
    source.write_bytes(Path(__file__).with_name(source.name).read_bytes())
    checks, commands = [], []
    def check(name, passed):
        if not passed: raise AssertionError(name)
        checks.append(name)
    def run(name, argv, env=None):
        argv = list(map(str, argv)); result = subprocess.run(argv, cwd=out, capture_output=True, timeout=300, env=env)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=argv, exitCode=result.returncode))
        check(name, result.returncode == 0)
    error = None
    try:
        for opt in ('0', '2'):
            binary = out / ('native-O' + opt)
            run('native-build-O' + opt, ['c++', '-std=c++17', '-O' + opt, source, '-o', binary])
            run('native-run-O' + opt, [binary])
        run('stock-lower', [args.stock_compiler.resolve(), '--c++17', '--instantiate', 'used', '--no_inlining',
            '--exceptions', '--clear_flag', 'use_predefined_macro_file', '--gen_c_file_name', out / 'stock.c',
            '-o', out / 'stock.il', source], env=dict(os.environ, PATH=str(args.stock_compiler.resolve().parent) + os.pathsep + os.environ['PATH']))
        for opt in ('0', '2'):
            binary = out / ('stock-O' + opt)
            run('stock-build-O' + opt, ['gcc', '-w', '-O' + opt, '-falign-functions=4', out / 'stock.c', args.stock_runtime.resolve(), '-lstdc++', '-lm', '-lpthread', '-o', binary])
            run('stock-run-O' + opt, [binary])
        obj = out / 'unit.dotobj'
        run('export', [sys.executable, '-B', args.packager.resolve(), source, '--output', obj,
            '--work-dir', out / 'work', '--compiler', args.compiler.resolve(), '--std', 'c++17', '--exceptions'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('original-function-references', db.execute("SELECT count(*) FROM source_types s JOIN source_types t ON t.id=s.underlying_type_id WHERE s.kind IN ('lvalue-reference','rvalue-reference') AND t.kind='routine'").fetchone()[0] > 0)
            check('function-pointer-object-addresses-retained', db.execute("SELECT count(*) FROM nodes n JOIN edges e ON e.parent_id=n.id AND e.role='operand' JOIN nodes c ON c.id=e.child_id JOIN types p ON p.id=c.type_id JOIN types t ON t.id=p.pointee_type_id WHERE n.kind='address' AND t.kind='routine'").fetchone()[0] > 0)
            check('all-address-types-match', db.execute("SELECT count(*) FROM nodes n JOIN types t ON t.id=n.type_id JOIN edges e ON e.parent_id=n.id AND e.role='operand' JOIN nodes c ON c.id=e.child_id WHERE n.kind='address' AND (t.kind<>'pointer' OR t.pointee_type_id<>c.type_id)").fetchone()[0] == 0)
        source.rename(source.with_suffix('.retained.cpp'))
        generated = out / 'linked'
        run('link-hidden-source', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', generated, obj])
        run('build-managed', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            aot = out / 'aot'
            run('publish-aot', ['dotnet', 'publish', generated / 'Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', aot, '-v:q'])
            run('run-aot', [aot / 'Translated'])
    except Exception as exc:
        error = str(exc)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, checkCount=len(checks), commands=commands,
        compilerSha256=hashlib.sha256(args.compiler.read_bytes()).hexdigest(), linkerSha256=hashlib.sha256(args.linker.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], error=error, checkCount=len(checks))))
    return 1 if error else 0


if __name__ == '__main__':
    sys.exit(main())
