"""Compile authored C# against non-executable declarations from actual linked C#.

This is a static signature check, not a product build or mock execution. Every
reference method is extern and /refonly output cannot execute. No emitted file
is changed and no provider, product or compiler project is built.
"""
import argparse
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]


def block(text, start):
    opening = text.index('{', start)
    depth = 1
    index = opening + 1
    while depth:
        if text[index] == '{': depth += 1
        elif text[index] == '}': depth -= 1
        index += 1
    return text[start:index]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--linked', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=False)
    linked = '\n'.join(p.read_text() for p in args.linked.parent.glob('*.cs'))
    authored = '\n'.join(p.read_text() for directory in ('src/Managed.S2nTls', 'tests/Lifecycle')
                         for p in (ROOT / directory).glob('*.cs'))
    calls = sorted(set(re.findall(r'Raw\.((?:s2n_|dotcc_s2n_)\w+)\s*(?:\(|[,;])', authored)))
    signatures = {}
    for declaration in re.findall(r'public static unsafe ([^\n]+)', linked):
        match = re.search(r'\b((?:s2n_|dotcc_s2n_)\w+)\(', declaration)
        if match and match[1] in calls: signatures[match[1]] = declaration
    if set(calls) != signatures.keys(): raise RuntimeError('Linked methods missing: ' + str(set(calls) - signatures.keys()))
    types = set(re.findall(r'Raw\.((?:s2n_|dotcc_s2n_)\w+)', authored)) - set(calls)
    for signature in signatures.values():
        types.update(set(re.findall(r'\b(?:s2n_|dotcc_s2n_)\w+\b', signature)) - set(calls))
    enums = {}
    for name in types:
        match = re.search(r'public enum ' + re.escape(name) + r'\b', linked)
        if match: enums[name] = block(linked, match.start())
        elif not re.search(r'public (?:unsafe )?(?:partial )?struct ' + re.escape(name) + r'\b', linked):
            raise RuntimeError('Actual public linked type is missing: ' + name)
    cbool = re.search(r'public readonly struct CBool\b', linked)
    if not cbool: raise RuntimeError('Actual linked CBool is missing')
    libc = re.search(r'public static (?:unsafe )?partial class Libc\b', linked)
    if not libc: raise RuntimeError('Actual linked nested Libc is missing')
    constants = {}
    for name in ('EAGAIN', 'EIO'):
        match = re.search(r'public const int ' + name + r'\s*=\s*([^;]+);', linked)
        if not match: raise RuntimeError('Actual libc constant missing: ' + name)
        constants[name] = match[1]
    reference = args.output / 'LinkedApiDeclarations.cs'
    defines = []
    for name in sorted(set(re.findall(r'Raw\.(S2N_\w+)', authored))):
        match = re.search(r'public const (\w+) ' + re.escape(name) + r'\s*=\s*([^;]+);', linked)
        if not match: raise RuntimeError('Actual linked constant missing: ' + name)
        defines.append('public const ' + match[1] + ' ' + name + ' = ' + match[2] + ';')
    reference.write_text('namespace Managed.Security.S2n;\npublic static unsafe class S2nTls {\n'
        + '\n'.join(defines) + '\n'
        + '\n'.join(enums.values()) + '\n'
        + '\n'.join('public struct ' + n + ' { }' for n in sorted(types - enums.keys())) + '\n'
        + block(linked, cbool.start()) + '\n'
        + '\n'.join('public static extern ' + signatures[n] + ';' for n in calls) + '\n'
        + 'public static class Libc { public static extern int errno { get; set; }\n'
        + '\n'.join('public const int ' + n + ' = ' + v + ';' for n, v in constants.items()) + '\n}}\n')
    global_usings = args.output / 'GlobalUsings.cs'
    global_usings.write_text('\n'.join('global using ' + n + ';' for n in
        ('System', 'System.Collections.Generic', 'System.Linq', 'System.IO', 'System.Threading', 'System.Threading.Tasks')))
    sdk_lines = subprocess.check_output(['dotnet', '--list-sdks'], text=True).splitlines()
    sdk = next(re.match(r'(10\.[^ ]+) \[([^]]+)\]', line) for line in reversed(sdk_lines) if line.startswith('10.'))
    sdk_path = Path(sdk[2]) / sdk[1]
    dotnet_root = Path(sdk[2]).parent
    packs = sorted((dotnet_root / 'packs/Microsoft.NETCore.App.Ref').glob('10.*'))
    pack = packs[-1]
    refs = sorted((pack / 'ref/net10.0').glob('*.dll'))
    analyzer = pack / 'analyzers/dotnet/cs/System.Text.Json.SourceGeneration.dll'
    sources = sorted((ROOT / 'src/Managed.S2nTls').glob('*.cs'))
    fixture = ROOT / 'samples/ManagedConsumer/Exchange.cs'
    groups = {
        'consumer': [*sources, fixture, ROOT / 'samples/ManagedConsumer/Program.cs'],
        'managed-tests': [*sources, fixture, ROOT / 'tests/Managed/Program.cs',
                          *sorted((ROOT / 'tests/Lifecycle').glob('*.cs'))],
    }
    report = {'evidence_kind': 'static-linked-signature-compilation', 'managed_execution': False,
              'reference_methods': len(calls), 'rows': []}
    for name, files in groups.items():
        response = args.output / (name + '.rsp')
        options = ['/noconfig', '/nostdlib+', '/target:exe', '/refonly', '/unsafe+', '/langversion:14', '/nullable:enable', '/nowarn:0626',
                   '/out:' + str(args.output / (name + '.dll')), '/analyzer:' + str(analyzer),
                   *('/reference:' + str(p) for p in refs), str(reference), str(global_usings), *map(str, files)]
        response.write_text('\n'.join('"' + o.replace('"', '\\"') + '"' for o in options))
        command = ['dotnet', str(sdk_path / 'Roslyn/bincore/csc.dll'), '@' + str(response)]
        result = subprocess.run(command, capture_output=True, text=True, timeout=60)
        log = args.output / (name + '.log'); log.write_text(result.stdout + result.stderr)
        report['rows'].append({'case': name, 'status': 'passed' if result.returncode == 0 else 'failed', 'command': command, 'log': str(log)})
    report['status'] = 'passed' if all(r['status'] == 'passed' for r in report['rows']) else 'failed'
    (args.output / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(report))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
