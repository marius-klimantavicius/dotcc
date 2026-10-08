#!/usr/bin/env python3
"""Execute real virtual calls/deletion using independently translated RTTI providers."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import resource
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def load(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, required=True)
    parser.add_argument('--linker', type=Path, required=True)
    parser.add_argument('--project-driver', type=Path, default=ROOT / 'project.py')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--nativeaot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    parser.add_argument('--fixture', choices=('base', 'abstract'), default='base')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands = [], []
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    environment = os.environ.copy()
    environment.update(DOTNET_DbgEnableMiniDump='0', COMPlus_DbgEnableMiniDump='0')

    def run(label, command, fatal_reason=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, env=environment, text=True, capture_output=True, timeout=300)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        commands.append(dict(label=label, command=command, exit=result.returncode))
        if fatal_reason is not None:
            if result.returncode in (0, 99) or fatal_reason not in result.stderr:
                raise ValueError(label + ': expected ABI abort: ' + result.stderr)
        elif result.returncode:
            raise ValueError(label + ': ' + result.stdout + result.stderr)
        checks.append(label)

    error = None
    try:
        provider = load(ROOT / 'libcxx_provider.py', 'rtti_libcxx_provider')
        manifest = load(ROOT / 'project_manifest.py', 'rtti_manifest')
        units, receipt = provider.prepare_libcxx_units(ROOT.parent, args.acquisition, manifest.libc_arguments())
        units = [unit for unit in units if unit['id'] in ('libcxx:src/typeinfo.cpp', 'libcxx:adapter/abi_typeinfo.cpp')]
        (out / 'inputs').mkdir()
        filename = 'project_cpp_base_subobjects.cpp' if args.fixture == 'base' else 'project_cpp_abstract_interface.cpp'
        source = out / 'inputs' / filename
        shutil.copy2(ROOT / 'tests' / source.name, source)
        units.append(dict(id='fixture:virtual-delete', source=str(source), standard='c++17',
                          arguments=['--no_exceptions', '--no_rtti'], role='fixture'))
        (out / 'project.json').write_text(json.dumps(dict(schema=1, project='real-rtti-provider', units=units,
                                                        libcxxProvider=receipt), indent=2) + '\n')
        for optimization in ('0', '2'):
            run('native-build-' + optimization, ['c++', '-std=c++17', '-O' + optimization,
                '-fno-exceptions', '-fno-rtti', source, '-o', out / ('native-' + optimization)])
            run('native-' + optimization, [out / ('native-' + optimization)])
        run('package', [sys.executable, '-B', args.project_driver.resolve(), 'build', '--manifest', out / 'project.json',
                       '--output-dir', out / 'objects', '--compiler', args.compiler.resolve(), '--jobs', '1'])
        packaged = json.loads((out / 'objects/results.json').read_text())['units']
        for key, item in packaged.items():
            run('verify-' + key.replace(':', '-').replace('/', '-'), ['dotnet', args.linker.resolve(), 'verify', item['object']])
        # The main input is hidden before link. Provider implementations are
        # canonical shared inputs; they are recorded by hash in the receipt.
        source.rename(out / 'retained-main.cpp')
        command = ['dotnet', args.linker.resolve(), 'link', '--out', out / 'generated', '--entry', 'main',
                   '--allow-externs', packaged['fixture:virtual-delete']['object']]
        for key, item in packaged.items():
            if key != 'fixture:virtual-delete':
                command += ['--provider', item['object']]
        run('link', command)
        report = json.loads((out / 'generated/link-report.json').read_text())
        if report['runtimeStorageRequirements']:
            raise ValueError('RTTI vtable storage remains unresolved')
        expected = {'_ZN10__cxxabiv117__class_type_infoD1Ev', '_ZN10__cxxabiv120__si_class_type_infoD1Ev'}
        if not expected <= {item['externalKey'] for item in report['symbols']}:
            raise ValueError('Real RTTI provider key functions were not selected')
        checks.append('real-rtti-definitions-selected-with-no-storage-stubs')
        run('bind', [sys.executable, '-B', ROOT / 'runtime/bind.py', '--project', out / 'generated'])
        if args.fixture == 'abstract':
            (out / 'generated/RuntimeAbstractProbe.cs').write_text('''// Test host for isolated ABI abort calls.
internal static class RuntimeAbstractProbe
{
    public static int Main(string[] arguments)
    {
        if (arguments.Length == 0) return __DotObjEntry.Main();
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        if (arguments[0] == "pure") global::Translated.Program.fatal_pure();
        else global::Translated.Program.fatal_deleted();
        return 99;
    }
}
''')
            project = out / 'generated/Translated.csproj'
            project.write_text(project.read_text().replace('<StartupObject>__DotObjEntry</StartupObject>',
                                                           '<StartupObject>RuntimeAbstractProbe</StartupObject>'))
        run('build', ['dotnet', 'build', out / 'generated/Translated.csproj', '--nologo', '-v:q'])
        run('jit', ['dotnet', out / 'generated/bin/Debug/net10.0/Translated.dll'])
        if args.fixture == 'abstract':
            for mode in ('pure', 'deleted'):
                run('jit-' + mode, ['dotnet', out / 'generated/bin/Debug/net10.0/Translated.dll', mode],
                    fatal_reason='EDG ' + mode + ' virtual call')
        if args.nativeaot:
            run('publish', ['dotnet', 'publish', out / 'generated/Translated.csproj', '-r', args.rid,
                           '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', out / 'aot', '--nologo', '-v:q'])
            run('aot', [out / 'aot/Translated'])
            if args.fixture == 'abstract':
                for mode in ('pure', 'deleted'):
                    run('aot-' + mode, [out / 'aot/Translated', mode], fatal_reason='EDG ' + mode + ' virtual call')
    except Exception as failure:
        error = str(failure)
    result = dict(status='failed' if error else 'passed', error=error, checks=checks, commands=commands,
                  compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)),
                  linker=dict(path=str(args.linker.resolve()), sha256=sha(args.linker)),
                  nativeAotVerified=args.nativeaot and not error,
                  fixture=args.fixture,
                  scope='Actual virtual dispatch, RTTI provider selection and class lifetimes; not dynamic_cast or source EH coverage')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
