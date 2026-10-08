#!/usr/bin/env python3
"""Detached Linux x64 native/JIT/NativeAOT empty-memory-asm lowering audit."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import subprocess

HERE = Path(__file__).resolve().parent


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    if platform.system() != 'Linux' or platform.machine() not in ('x86_64', 'AMD64'):
        parser.error('Disassembly assertions currently audit Linux x64 only')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    checks, commands = [], []

    def check(name, condition):
        if not condition:
            raise AssertionError(name)
        checks.append(name)

    def run(name, command, env=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, env=env, capture_output=True, timeout=300)
        (out / (name + '.stdout')).write_bytes(result.stdout)
        (out / (name + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=name, command=command, exitCode=result.returncode))
        check(name, result.returncode == 0)
        return result.stdout

    failure = None
    try:
        for source, name in [('runtime_barriers.cs', 'Probe.cs'), ('runtime_barriers.cpp', 'native.cpp')]:
            shutil.copy2(HERE / source, out / name)
        (out / 'Barriers.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
            '<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>'
            '<AllowUnsafeBlocks>true</AllowUnsafeBlocks><IsAotCompatible>true</IsAotCompatible>'
            '<Optimize>true</Optimize><StripSymbols>false</StripSymbols></PropertyGroup></Project>\n')
        run('dotnet-info', ['dotnet', '--info'])
        run('native-compiler-info', ['c++', '--version'])
        run('native-build', ['c++', '-std=gnu++17', '-O2', out / 'native.cpp', '-o', out / 'native'])
        run('native', [out / 'native'])
        run('native-assembly', ['c++', '-std=gnu++17', '-O2', '-S', '-masm=intel', out / 'native.cpp', '-o', out / 'native.s'])
        run('build', ['dotnet', 'build', out / 'Barriers.csproj', '-c', 'Release', '--nologo', '-v:q'])
        env = os.environ.copy()
        env.update(DOTNET_TieredCompilation='0', DOTNET_JitDisasm='BarrierProbe:*', DOTNET_JitStdOutFile=str(out / 'jit-disassembly.txt'))
        jit = run('jit', ['dotnet', out / 'bin/Release/net10.0/Barriers.dll'], env)
        check('jit-seven-runtime-checks', len(jit.splitlines()) == 7)
        run('publish', ['dotnet', 'publish', out / 'Barriers.csproj', '-c', 'Release', '-r', 'linux-x64',
            '-p:PublishAot=true', '-p:IlcTreatWarningsAsErrors=true', '-p:StripSymbols=false', '-o', out / 'aot', '--nologo', '-v:q'])
        aot = run('nativeaot', [out / 'aot/Barriers'])
        check('nativeaot-runtime-parity', aot == jit)
        assembly = run('nativeaot-disassembly', ['objdump', '-d', '-M', 'intel', out / 'aot/Barriers']).decode()
        jit_assembly = (out / 'jit-disassembly.txt').read_text()
        methods = ('Reload', 'CoalescedRead', 'Scrub', 'ScrubIndirect', 'BarrierOnly', 'OperandEffects')
        selected = []
        for mode, text in [('jit', jit_assembly), ('aot', assembly)]:
            bodies = {}
            for method in methods:
                if mode == 'jit':
                    marker = '; Assembly listing for method BarrierProbe:' + method + '('
                    begin = text.index(marker)
                    end = text.find('; Assembly listing', begin + len(marker))
                else:
                    marker = '<Barriers_BarrierProbe__' + method + '>:'
                    begin = text.index(marker)
                    end = text.find('\n\n', begin)
                bodies[method] = text[begin:end if end != -1 else None]
                selected.append(bodies[method])
            # Assert the observed optimized instruction shapes, not timing.
            # These deliberately scoped assertions flag future JIT changes for
            # review rather than asserting a universal .NET machine-code ABI.
            check(mode + '-reload-retained', bodies['Reload'].count('[rdi]') == 2 and re.search(r'\block\b', bodies['Reload']) is not None)
            check(mode + '-control-coalesced', bodies['CoalescedRead'].count('[rdi]') == 1 and re.search(r'\block\b', bodies['CoalescedRead']) is None)
            clear = bodies['Scrub'].find('ClearWithoutReferences')
            fence = re.search(r'\block\b', bodies['Scrub'])
            check(mode + '-zeroization-before-fence', clear >= 0 and fence is not None and fence.start() > clear)
            indirect = bodies['ScrubIndirect'].find('call')
            indirect_fence = re.search(r'\block\b', bodies['ScrubIndirect'])
            check(mode + '-volatile-callback-before-fence', indirect >= 0 and indirect_fence is not None and indirect_fence.start() > indirect)
            check(mode + '-no-operand-access', re.search(r'\block\b', bodies['BarrierOnly']) is not None and
                  '[rdi' not in bodies['BarrierOnly'] and '[rsi' not in bodies['BarrierOnly'])
            check(mode + '-effects-before-barrier', '[rsi]' in bodies['OperandEffects'] and '[rdx]' in bodies['OperandEffects'] and
                  bodies['OperandEffects'].rfind('BarrierOnly') > bodies['OperandEffects'].rfind('[rdx]'))
        (out / 'selected-disassembly.txt').write_text('\n\n'.join(selected))
        native = (out / 'native.s').read_text()
        reload = native.split('probe_reload:', 1)[1].split('.size\tprobe_reload', 1)[0]
        coalesced = native.split('probe_coalesced:', 1)[1].split('.size\tprobe_coalesced', 1)[0]
        check('native-empty-asm-reloads-without-hardware-fence', reload.count('[rdi]') == 2 and
              not re.search(r'\b(lock|mfence)\b', reload) and coalesced.count('[rdi]') == 1)
    except Exception as error:
        failure = str(error)
    result = dict(status='failed' if failure else 'passed', error=failure, checks=checks, commands=commands,
                  fixtureHashes={name: sha(out / name) for name in ('Probe.cs', 'native.cpp')},
                  scope='Detached Linux x64 optimized code audit; not dotobj-owned asm support or other architecture execution',
                  conclusion=None if failure else 'Thread.MemoryBarrier preserves reload/zeroization without touching operand memory; it adds a stronger hardware fence than native empty asm')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=failure)))
    return 1 if failure else 0


if __name__ == '__main__':
    raise SystemExit(main())
