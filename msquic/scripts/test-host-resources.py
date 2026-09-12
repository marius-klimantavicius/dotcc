#!/usr/bin/env python3
"""Exercise the authored registry directly, including fatal invalid-token children."""
import hashlib
import json
from pathlib import Path
import resource
import subprocess

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / 'tests/HostResources/HostResources.csproj'
LOGS = ROOT / 'artifacts/host-resources'
LOGS.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, commands=[], variants=[])


def run(command, name, fatal=False):
    arguments = [str(x) for x in command]
    receipt['commands'].append(dict(name=name, arguments=arguments))
    result = subprocess.run(arguments, capture_output=True, text=True, timeout=120)
    (LOGS / (name + '.log')).write_text(result.stdout + result.stderr)
    if fatal:
        if result.returncode == 0 or 'MsQuic managed host invariant:' not in result.stderr:
            raise RuntimeError('Missing fatal token rejection: ' + name)
    elif result.returncode:
        raise RuntimeError('Failure: ' + name)
    return result.stdout


try:
    # Invalid-token probes must fail the child without producing huge core files.
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    run(['dotnet', 'build', PROJECT, '-c', 'Release', '--nologo'], 'build')
    for variant in ['jit', 'nativeaot']:
        if variant == 'nativeaot':
            run(['dotnet', 'publish', PROJECT, '-c', 'Release', '-r', 'linux-x64',
                 '-p:PublishAot=true', '-o', LOGS / 'nativeaot', '--nologo'], 'publish')
            command = [LOGS / 'nativeaot/HostResources']
        else:
            command = ['dotnet', PROJECT.parent / 'bin/Release/net10.0/HostResources.dll']
        result = run(command, variant)
        if result.strip() != variant + ': registry passed':
            raise RuntimeError('Unexpected runtime identity or receipt')
        for mode in ['stale', 'foreign', 'wrong-type']:
            run(command + [mode], variant + '-' + mode, fatal=True)
        receipt['variants'].append(dict(name=variant, passed=True, concurrent_tokens=2048,
            add_close_races=128, fatal_cases=['stale', 'foreign', 'wrong-type']))
    receipt['source_sha256'] = {str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest()
        for p in [ROOT / 'src/BclHost/MsQuicHost.Resources.cs', ROOT / 'src/BclHost/Status.cs',
                  PROJECT, PROJECT.parent / 'Program.cs']}
    receipt['passed'] = True
finally:
    (LOGS / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print('Host registry JIT/NativeAOT: PASS')
