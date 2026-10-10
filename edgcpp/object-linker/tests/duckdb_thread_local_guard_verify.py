#!/usr/bin/env python3
"""Verify exact thread-local guard publication, retry after throw and thread isolation."""
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
    source = out / 'thread_local_guard.cpp'
    shutil.copy2(Path(__file__).with_name('duckdb_thread_local_guard.cpp'), source)
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
            check('one-thread-local-guard', db.execute("SELECT count(*) FROM local_static_guards WHERE guard_policy='thread-local-byte'").fetchone()[0] == 1)
            check('only-thread-probe-publication', db.execute("SELECT group_concat(kind,',') FROM (SELECT kind FROM guard_operations ORDER BY kind)").fetchone()[0] == 'probe,thread-publish')
            check('stock-thread-abort-cleanup-retained', db.execute('SELECT count(*) FROM guard_cleanup_actions').fetchone()[0] == 1)
            check('both-storages-are-thread-local', db.execute('SELECT count(*) FROM local_static_guards g JOIN storages s ON s.id=g.storage_id JOIN storages t ON t.id=g.guard_storage_id WHERE s.is_thread_local=1 AND t.is_thread_local=1').fetchone()[0] == 1)
        providers = [item for p in args.provider for item in ('--provider', p.resolve())]
        run('link', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--allow-externs', '--out', out / 'generated', obj, *providers])
        runtime = Path(__file__).resolve().parents[1] / 'runtime/bind.py'
        run('bind-runtime', ['python3', runtime, '--project', out / 'generated'])
        # Invoke the same emitted body on eight BCL threads and the initial
        # thread. Each must observe its own failed first attempt and publication.
        (out / 'generated/TlsDriver.cs').write_text("""
internal static class TlsDriver {
    public static int Run() {
        int[] results = new int[8];
        var threads = new global::System.Threading.Thread[8];
        for (int i = 0; i < threads.Length; ++i) {
            int index = i;
            threads[i] = new global::System.Threading.Thread(() => results[index] = global::Translated.Program.main());
            threads[i].Start();
        }
        foreach (var thread in threads) thread.Join();
        foreach (int result in results) if (result != 0) return result;
        return global::Translated.Program.main();
    }
}
""")
        entry = out / 'generated/Entry.generated.cs'
        entry.write_text(entry.read_text().replace('global::Translated.Program.main()', 'TlsDriver.Run()'))
        run('build-managed', ['dotnet', 'build', out / 'generated/Translated.csproj', '-c', 'Release', '-v:q'])
        run('run-managed', ['dotnet', out / 'generated/bin/Release/net10.0/Translated.dll'])
        if args.native_aot:
            run('publish-aot', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-c', 'Release', '-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', out / 'aot', '-v:q'])
            run('run-aot', [out / 'aot/Translated'])
        mutations = [
            ('false-process-policy', "UPDATE local_static_guards SET guard_policy='ia64-acquire-release'", (), 'Local static guard lacks'),
            ('nonthread-guard-storage', "UPDATE storages SET is_thread_local=0 WHERE id IN (SELECT guard_storage_id FROM local_static_guards)", (), 'Local static guard lacks'),
            ('nonthread-object-storage', "UPDATE storages SET is_thread_local=0 WHERE id IN (SELECT storage_id FROM local_static_guards)", (), 'Local static guard lacks'),
            ('missing-publication', "DELETE FROM guard_operations WHERE kind='thread-publish'", (), 'complete acquire/publication sequence'),
            ('false-acquire', "UPDATE guard_operations SET kind='acquire' WHERE kind='probe'", (), 'Guard semantic operation lacks'),
            ('false-release', "UPDATE guard_operations SET kind='release' WHERE kind='thread-publish'", (), 'Guard semantic operation lacks'),
            ('wrong-publication-type', "UPDATE nodes SET type_id=2 WHERE id IN (SELECT node_id FROM guard_operations WHERE kind='thread-publish')", (), 'Guard semantic operation lacks'),
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
