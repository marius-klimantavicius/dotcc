#!/usr/bin/env python3
"""Compare owned cleanup states with native constructor/temporary unwinding."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

HOST = '''using Tests.Cleanup;
internal sealed class HostFailure(int point) : global::System.Exception { public int Point = point; }
internal static class CleanupHost
{
    internal static int Selected;
    private static bool Check(global::System.Action run, int point, ulong expected)
    {
        Selected = point; Core.cleanup_reset(); bool caught = false;
        try { run(); }
        catch (HostFailure error) { if (error.Point != point) return false; caught = true; }
        return caught == (point != 0) && Core.cleanup_trace() == expected;
    }
    public static int Main()
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        return Check(Core.cleanup_locals, 0, 12721) && Check(Core.cleanup_locals, 1, 1) &&
            Check(Core.cleanup_locals, 2, 121) && Check(Core.cleanup_locals, 3, 1221) &&
            Check(Core.cleanup_constructor, 0, 3436875) && Check(Core.cleanup_constructor, 3, 3) &&
            Check(Core.cleanup_constructor, 4, 343) && Check(Core.cleanup_constructor, 6, 34365) &&
            Check(Core.cleanup_constructor, 8, 3436875) &&
            Check(Core.cleanup_members, 0, 4268725) && Check(Core.cleanup_members, 4, 4) &&
            Check(Core.cleanup_members, 2, 425) && Check(Core.cleanup_members, 6, 42625) &&
            Check(Core.cleanup_members, 8, 4268725) &&
            Check(Core.cleanup_temporary_result, 0, 333) && Check(Core.cleanup_temporary_result, 3, 3) &&
            Check(Core.cleanup_delegating, 0, 1283) && Check(Core.cleanup_delegating, 1, 1) &&
            Check(Core.cleanup_delegating, 2, 123) && Check(Core.cleanup_delegating, 8, 1283) &&
            Check(Core.cleanup_conditional_both, 0, 128383261) &&
            Check(Core.cleanup_conditional_left, 0, 128261) &&
            Check(Core.cleanup_conditional_right, 0, 138361) &&
            Check(Core.cleanup_conditional_none, 0, 161) &&
            Check(Core.cleanup_conditional_both, 1, 1) &&
            Check(Core.cleanup_conditional_both, 2, 121) &&
            Check(Core.cleanup_conditional_both, 3, 128321) &&
            Check(Core.cleanup_conditional_both, 72, 12821) &&
            Check(Core.cleanup_conditional_both, 73, 12838321) &&
            Check(Core.cleanup_conditional_both, 9, 12838321) &&
            Check(Core.cleanup_conditional_none, 9, 11) &&
            Check(Core.cleanup_conditional_left, 9, 12821) &&
            Check(Core.cleanup_conditional_right, 9, 13831) &&
            Check(Core.cleanup_choice_left, 0, 128261) &&
            Check(Core.cleanup_choice_right, 0, 138361) &&
            Check(Core.cleanup_choice_left, 9, 12821) &&
            Check(Core.cleanup_choice_right, 9, 13831) &&
            Check(Core.cleanup_conditional_loop, 0, 282662826) &&
            Check(Core.cleanup_conditional_loop, 2, 2) &&
            Check(Core.cleanup_conditional_loop, 72, 282) &&
            Check(Core.cleanup_conditional_loop, 10, 282) &&
            Check(Core.cleanup_conditional_loop, 11, 2826) &&
            Check(Core.cleanup_conditional_loop, 12, 28266282) ? 0 : 1;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void cleanup_throw_point(int argument0)
        { if (argument0 == global::CleanupHost.Selected) throw new global::HostFailure(argument0); }
    }
}
'''

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path)
    parser.add_argument('--linker', type=Path)
    parser.add_argument('--packager', type=Path)
    parser.add_argument('--native-only', action='store_true')
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    if not args.native_only and not all((args.compiler, args.linker, args.packager)):
        parser.error('owned acceptance requires --compiler, --linker, --packager')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    source = inputs / 'cpp_cleanup_states.cpp'
    native = inputs / 'cpp_cleanup_states_native.cpp'
    for target in (source, native):
        shutil.copy2(Path(__file__).parent / target.name, target)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0)
        return result

    error = None
    try:
        for optimization in ('0', '2'):
            binary = out / ('native-O' + optimization)
            run('native-build-O' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                source, native, '-o', binary])
            result = run('native-run-O' + optimization, [binary])
            check('native-silent-O' + optimization, not result.stdout and not result.stderr)
        if not args.native_only:
            tooling = out / 'tooling'
            tooling.mkdir()
            for filename in ('compile.py', 'schema.sql'):
                shutil.copy2(args.packager.resolve().parent / filename, tooling / filename)
            obj = out / 'cleanup.dotobj'
            run('export', [sys.executable, '-B', tooling / 'compile.py', source,
                '--output', obj, '--work-dir', out / 'work', '--compiler', args.compiler.resolve(),
                '--std', 'c++17', '--exceptions'])
            run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
            with sqlite3.connect(obj) as db:
                check('owned-expression-lifetime-boundary', db.execute('SELECT count(*) FROM expression_lifetimes').fetchone()[0] > 0)
                check('owned-conditional-cleanup-actions', db.execute('SELECT count(*) FROM cleanup_conditions').fetchone()[0] >= 5)
                check('multiple-conditional-actions-in-one-function', db.execute('SELECT max(n) FROM (SELECT count(*) AS n FROM cleanup_conditions c JOIN cleanup_actions a ON a.id=c.action_id GROUP BY a.function_entity_id)').fetchone()[0] >= 2)
            inputs.rename(out / 'retained-source')
            generated = out / 'generated'
            run('source-hidden-link', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
                '--namespace', 'Tests.Cleanup', '--class', 'Core', '--allow-externs', obj])
            (generated / 'CleanupHost.cs').write_text(HOST)
            project = generated / 'Translated.csproj'
            text = project.read_text()
            check('library-project', '<OutputType>Library</OutputType>' in text)
            project.write_text(text.replace('<OutputType>Library</OutputType>',
                '<OutputType>Exe</OutputType><StartupObject>CleanupHost</StartupObject>'))
            run('build', ['dotnet', 'build', project, '-c', 'Release', '--nologo', '-v:q'])
            result = run('jit', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
            check('jit-silent', not result.stdout and not result.stderr)
            if args.nativeaot:
                publish = out / 'publish'
                run('publish', ['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                    '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', publish, '--nologo', '-v:q'])
                result = run('nativeaot', [publish / 'Translated'])
                check('nativeaot-silent', not result.stdout and not result.stderr)
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'native-oracle-only' if args.native_only else 'passed',
        error=error, checks=checks, commands=commands, cases=43,
        limitations=['Host throws are ordinary CLR exceptions, not implemented source C++ throw/catch/type matching',
                     'No destructor-throws-during-unwind, arrays, VTT, guards or thread-exit coverage'])
    for name in ('compiler', 'linker', 'packager'):
        path = getattr(args, name)
        if path:
            path = path.resolve()
            receipt[name] = dict(path=str(path), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return 1 if error else 0

if __name__ == '__main__':
    sys.exit(main())
