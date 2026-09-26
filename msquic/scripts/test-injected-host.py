#!/usr/bin/env python3
"""Run the actual core with controlled clock and BCL I/O failures, in isolation."""
import argparse
import hashlib
import json
from pathlib import Path
import resource
import subprocess
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[1]
import sys
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import policy, observed_digest
from provenance import picotls_provenance
REPO = ROOT.parent
SCENARIOS = ['virtual-timeout', 'send-allocation', 'send-error', 'receive-error', 'socket-create']


def sha(path):
    return observed_digest(path)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    parser.add_argument('--jit-only', action='store_true')
    parser.add_argument('--families', nargs='+', choices=['ipv4', 'ipv6'], default=['ipv4', 'ipv6'])
    parser.add_argument('--scenarios', nargs='+', choices=SCENARIOS + ['keepalive'], default=SCENARIOS)
    parser.add_argument('--certificate', type=Path, default=ROOT / 'build/managed-peer/ecdsa.pem')
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/injected-host')
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    build = ROOT / 'build/injected-host'
    build.mkdir(parents=True, exist_ok=True)
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    receipt = dict(passed=False, targeted_passed=False, keepalive_qualified=False, entire_p7_qualified=False, selected_scenarios=args.scenarios, cases=[], commands=[])
    authored = sorted((ROOT / 'src/BclHost').glob('*.cs')) + sorted((ROOT / 'tests/InjectedHost').glob('*.cs')) + [ROOT / 'tests/TlsAdapter/Credentials.cs']
    # Source linking permits test-only partial implementations without exposing
    # injection controls in the delivered host or modifying generated libraries.
    tracked = authored + sorted((REPO / 'picotls/src/BclProvider').glob('*.cs')) + [Path(__file__).resolve(), ROOT / 'config/product-closure.json']
    for variant in args.variants:
        generated = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
        picotls = REPO / 'picotls/generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
        tracked += sorted(generated.glob('*.cs')) + sorted(generated.glob('*.csproj'))
        tracked += sorted(picotls.glob('*.cs')) + sorted(picotls.glob('*.csproj'))
    tracked += sorted((REPO / 'picotls/src/BclProvider').glob('*.csproj'))
    receipt['input_sha256'] = {str(path.relative_to(REPO)): sha(path) for path in tracked}

    def run(command, name, timeout=600):
        command = [str(item) for item in command]
        receipt['commands'].append(dict(name=name, arguments=command))
        result = subprocess.run(command, capture_output=True, text=True, timeout=timeout)
        (args.output / (name + '.log')).write_text(result.stdout + result.stderr)
        if result.returncode:
            raise RuntimeError(name + ' failed: ' + str(args.output / (name + '.log')))
        return result.stdout

    try:
        # The original five controls need the supplied trust input for their UDP
        # sink. Keepalive creates its own actual authenticated peer credentials.
        if any(scenario != 'keepalive' for scenario in args.scenarios) and not args.certificate.is_file():
            raise RuntimeError('Missing trust certificate: pass --certificate from the native peer setup')
        receipt['certificate_sha256'] = sha(args.certificate) if args.certificate.is_file() else None
        closure = json.loads((ROOT / 'config/product-closure.json').read_text())
        for variant in args.variants:
            generated = ROOT / 'generated' / ('TranslatedMsQuic.Raw' if variant == 'raw' else 'TranslatedMsQuic')
            if any(closure['generated'][variant].get(p.name) != sha(p) for p in generated.glob('*.cs')):
                policy().issue(variant + ' generated sources differ from frozen product closure')
            picotls = REPO / 'picotls/generated' / ('TranslatedPicotls.Raw' if variant == 'raw' else 'TranslatedPicotls')
            project = build / variant
            project.mkdir(exist_ok=True)
            project_file = project / 'InjectedHost.csproj'
            project_file.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
                '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>'
                '<IsAotCompatible>true</IsAotCompatible><PicotlsProject>' + escape(str(picotls / 'TranslatedPicotls.csproj')) +
                '</PicotlsProject></PropertyGroup><ItemGroup>' +
                '<ProjectReference Include="' + escape(str(generated / 'TranslatedMsQuic.csproj')) + '"/>' +
                '<ProjectReference Include="' + escape(str(REPO / 'picotls/src/BclProvider/BclProvider.csproj')) +
                '" AdditionalProperties="PicotlsProject=$(PicotlsProject)"/>' +
                ''.join('<Compile Include="' + escape(str(p)) + '"/>' for p in authored) +
                '<TrimmerRootAssembly Include="TranslatedMsQuic"/><TrimmerRootAssembly Include="TranslatedPicotls"/>'
                '</ItemGroup></Project>\n')
            run(['dotnet', 'build', project_file, '-c', 'Release', '--nologo'], variant + '-build')
            commands = [('jit', ['dotnet', project / 'bin/Release/net10.0/InjectedHost.dll'])]
            if not args.jit_only:
                run(['dotnet', 'publish', project_file, '-c', 'Release', '-r', 'linux-x64', '-p:PublishAot=true', '-o', project / 'aot', '--nologo'], variant + '-aot-build')
                commands.append(('aot', [project / 'aot/InjectedHost']))
            for runtime, command in commands:
                for family in args.families:
                    for scenario in args.scenarios:
                        name = '-'.join([variant, runtime, family, scenario])
                        output = run([*command, args.certificate, family, scenario], name, timeout=120 if scenario == 'keepalive' else 30)
                        case = json.loads(output.strip().splitlines()[-1])
                        case.update(name=name, variant=variant, runtime=runtime)
                        if scenario == 'keepalive':
                            case['observations'] = [line for line in output.splitlines() if line.startswith('EVIDENCE keepalive ')]
                            if case.get('completed_controls') != 6 or len(case['observations']) != 6:
                                raise RuntimeError(name + ' did not exercise both AES suites and all three timer settings')
                        receipt['cases'].append(case)
                        if not case.get('passed') or case.get('aot') != (runtime == 'aot'):
                            raise RuntimeError(name + ' did not qualify the requested runtime')
                        print(name + ': PASS', flush=True)
            receipt.setdefault('binary_sha256', {}).update({str(p.relative_to(REPO)): sha(p)
                for folder in [project / 'bin/Release/net10.0', project / 'aot'] if folder.exists()
                for p in sorted(folder.rglob('*')) if p.is_file()})
        if receipt['input_sha256'] != {str(path.relative_to(REPO)): sha(path) for path in tracked}:
            policy().issue('Inputs changed during injected host qualification')
        receipt['targeted_passed'] = True
        receipt['keepalive_qualified'] = not args.jit_only and set(args.variants) == {'raw', 'optimized'} and set(args.families) == {'ipv4', 'ipv6'} and 'keepalive' in args.scenarios and all(case.get('completed_controls') == 6 for case in receipt['cases'] if case.get('scenario') == 'keepalive')
        receipt['passed'] = not args.jit_only and set(args.variants) == {'raw', 'optimized'} and set(args.families) == {'ipv4', 'ipv6'} and set(args.scenarios) == set(SCENARIOS)
    except Exception as error:
        receipt['error'] = str(error)
        raise
    finally:
        (args.output / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({key: receipt[key] for key in ['passed', 'targeted_passed', 'keepalive_qualified', 'entire_p7_qualified']}), flush=True)


if __name__ == '__main__':
    main()
