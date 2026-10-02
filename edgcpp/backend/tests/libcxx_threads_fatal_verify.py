"""First-pass worker termination must precede translated RAII cleanup."""
import argparse
import json
from pathlib import Path
import resource
import subprocess
import sys


def verify_libcxx_threads_fatal(root, tests, out, nativeaot, run):
    out.mkdir(parents=True, exist_ok=True)
    checks = []
    def fatal(command, directory, label):
        command = list(map(str, command))
        # Keep full stdout/stderr; a huge core dump is not needed for an
        # intentionally fatal semantic test in this Linux acceptance harness.
        def child_setup():
            resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
        result = subprocess.run(command, capture_output=True, text=True, timeout=60, preexec_fn=child_setup)
        (directory / (label + '.stdout')).write_text(result.stdout)
        (directory / (label + '.stderr')).write_text(result.stderr)
        (directory / (label + '.json')).write_text(json.dumps({'command': command, 'exit': result.returncode}, indent=2))
        assert result.returncode != 0, (label, result.returncode)
        assert result.stdout == 'body\n', (label, result.stdout, result.stderr)
        assert 'Exception escaped translated thread' in result.stderr, result.stderr
    for instance in (False, True):
        mode = 'instance' if instance else 'static'
        directory = out / mode
        run([sys.executable, root / 'dotnet.py', tests / 'libcxx_threads_fatal.cpp', '--libc', 'dotcc',
             '-I', root / 'backend/runtime/libcxx/config', '--emit', 'managedlib', '--namespace', '',
             '--class-name', 'Core', '--output-dir', directory,
             *(['--instance-methods'] if instance else [])])
        manifest = json.loads((directory / 'dot-gen.json').read_text())
        method = next(e['method'] for e in manifest['exports'] if e['symbol'] == 'thread_fatal')
        host = directory / 'host'; host.mkdir()
        (host / 'Host.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<IsAotCompatible>true</IsAotCompatible></PropertyGroup>
<ItemGroup><ProjectReference Include="../Generated.csproj" /></ItemGroup></Project>''')
        (host / 'Program.cs').write_text('class EntryPoint { static int Main() { return ' +
                                       ('new Core().' if instance else 'Core.') + method + '(); } }')
        run(['dotnet', 'build', host / 'Host.csproj', '-c', 'Release', '--nologo'])
        fatal(['dotnet', host / 'bin/Release/net10.0/Host.dll'], host, 'jit-fatal')
        checks.append(f'{mode}/JIT: unhandled thread exception terminates before translated RAII cleanup')
        if nativeaot:
            run(['dotnet', 'publish', host / 'Host.csproj', '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-o', host / 'aot'])
            fatal([host / 'aot/Host'], host, 'aot-fatal')
            checks.append(f'{mode}/NativeAOT: unhandled thread exception terminates before translated RAII cleanup')
    return checks


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args(); output = args.output_dir.resolve(); output.mkdir(parents=True, exist_ok=True)
    commands = []
    def run(command, **kwargs):
        command = list(map(str, command)); completed = subprocess.run(command, capture_output=True, text=True, **kwargs)
        index = len(commands)
        (output / f'command-{index:03d}.stdout').write_text(completed.stdout)
        (output / f'command-{index:03d}.stderr').write_text(completed.stderr)
        commands.append({'command': command, 'exit': completed.returncode})
        (output / 'commands.json').write_text(json.dumps(commands, indent=2) + '\n')
        if completed.returncode: raise RuntimeError(f'{command}: {completed.returncode}\n{completed.stdout}\n{completed.stderr}')
        return completed
    root = Path(__file__).resolve().parents[2]
    checks = verify_libcxx_threads_fatal(root, Path(__file__).resolve().parent, output, args.nativeaot, run)
    (output / 'results.json').write_text(json.dumps({'checks': checks}, indent=2) + '\n')
    print(f'{len(checks)} generated fatal-thread checks passed')
