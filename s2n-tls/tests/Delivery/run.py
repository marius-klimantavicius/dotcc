"""Ordinary copied consumer, trimmed/rooted AOT and native-crypto dependency audit."""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import uuid
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]


def audit_notices(directory):
    declarations = json.loads((ROOT / 'config/notices.json').read_text())['sources']
    pins = json.loads((ROOT / 'config/inputs.json').read_text())
    manifest = directory / 'notices/manifest.json'
    summary = directory / 'THIRD-PARTY-NOTICES.txt'
    if not manifest.is_file() or not summary.is_file() or not summary.read_text().strip():
        raise RuntimeError('Required delivered notice summary/manifest missing: ' + str(directory))
    records = json.loads(manifest.read_text())
    expected = [{'source': key, 'revision': pins[key]['revision'], 'file': 'notices/' + key + '/' + name}
                for key, names in declarations.items() for name in names]
    if sorted(records, key=lambda r: r['file']) != sorted(expected, key=lambda r: r['file']):
        raise RuntimeError('Notice manifest differs from the complete declared source/license inventory')
    for key, names in declarations.items():
        for name in names:
            source = ROOT / 'ref' / pins[key]['directory'] / name
            packaged = directory / 'notices' / key / name
            if not packaged.is_file() or packaged.read_bytes() != source.read_bytes():
                raise RuntimeError('Missing or changed packaged original notice: ' + str(packaged))
        if key not in summary.read_text() or pins[key]['revision'] not in summary.read_text():
            raise RuntimeError('Notice summary omits source identity: ' + key)
    return [r['file'] for r in expected]


def audit_authored_references(product):
    provider = json.loads((ROOT / 'config/provider.json').read_text())
    tree = ET.parse(product)
    references = [(product.parent / n.get('Include')).resolve() for n in tree.iter('ProjectReference')]
    expected_provider = (ROOT / provider['project_reference']).resolve()
    if references.count(expected_provider) != 1:
        raise RuntimeError('Product must reference the original authored provider project exactly once')
    for reference in references:
        if not reference.is_relative_to(ROOT / 'src'):
            raise RuntimeError('Product project reference points outside original authored source: ' + str(reference))
    includes = [(product.parent / n.get('Include')).resolve() for n in tree.iter('Compile') if n.get('Include')]
    for original in [*provider['authored_bindings'], 'src/Boundary/S2nHost.cs']:
        if includes.count((ROOT / original).resolve()) != 1:
            raise RuntimeError('Generated project must compile original authored binding, without copied/generated edits: ' + original)
    owning = ET.parse(ROOT / 'src/Managed.S2nTls/Managed.S2nTls.csproj')
    if [n.get('Include') for n in owning.iter('ProjectReference')] != ['$(S2nTlsProject)']:
        raise RuntimeError('Owning API does not use the ordinary selected product reference')
    consumer = ET.parse(ROOT / 'samples/ManagedConsumer/ManagedConsumer.csproj')
    if [n.get('Include') for n in consumer.iter('ProjectReference')] != ['$(S2nManagedProject)']:
        raise RuntimeError('Consumer does not reference the original owning API project')
    for project in (owning, consumer):
        for node in project.iter('Compile'):
            if 'generated' in node.get('Include', '').replace('\\', '/').split('/'):
                raise RuntimeError('Authored project embeds generated C# instead of an ordinary project reference')
    return {'provider': str(expected_provider), 'bindings': provider['authored_bindings'] + ['src/Boundary/S2nHost.cs']}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--product', type=Path, required=True)
    parser.add_argument('--form', choices=('raw', 'processed'), required=True)
    parser.add_argument('--mode', choices=('jit', 'aot'), required=True)
    parser.add_argument('--rooted', action='store_true')
    parser.add_argument('--fixtures', type=Path, default=ROOT / 'build/native/credentials')
    parser.add_argument('--source', type=Path, default=ROOT / 'ref/s2n-tls')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    run_id = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
    work = ROOT / 'build/managed-delivery' / run_id
    work.mkdir(parents=True)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    report = {'status': 'failed', 'form': args.form, 'mode': args.mode, 'commands': [], 'rows': []}
    case = 'whole-library-rooted-aot' if args.rooted else 'external-ordinary-consumer'
    row = {'suite': 'rooted-aot' if args.rooted else 'delivery', 'case': case, 'form': args.form, 'mode': args.mode, 'status': 'failed'}
    report['rows'].append(row)

    def command(argv, timeout=1200, cwd=work):
        argv = list(map(str, argv)); log = args.output.parent / (args.output.stem + f'-{len(report["commands"]):03d}.log')
        with log.open('w') as stream:
            proc = subprocess.run(argv, cwd=cwd, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout)
        report['commands'].append({'command': argv, 'cwd': str(cwd), 'exit_code': proc.returncode, 'log': str(log)})
        if proc.returncode: raise RuntimeError('Delivery command failed: ' + str(log))
        return log

    try:
        product = args.product.resolve()
        if not product.is_file(): raise RuntimeError('Actual translated product missing: ' + str(product))
        report['original_authored_references'] = audit_authored_references(product)
        report['product_notice_files'] = audit_notices(product.parent)
        if args.rooted and args.mode != 'aot': raise RuntimeError('Whole-library rooting requires an actual NativeAOT execution')
        with tempfile.TemporaryDirectory(prefix='dotcc-s2n-external-') as directory:
            external = Path(directory) / 'consumer'
            shutil.copytree(ROOT / 'samples/ManagedConsumer', external,
                            ignore=shutil.ignore_patterns('bin', 'obj'))
            project = external / 'ManagedConsumer.csproj'
            if args.rooted:
                tree = ET.parse(project)
                group = ET.SubElement(tree.getroot(), 'ItemGroup')
                product_tree = ET.parse(product)
                assembly = next((p.text for p in product_tree.iter('AssemblyName') if p.text), product.stem)
                ET.SubElement(group, 'TrimmerRootAssembly', Include=assembly)
                ET.indent(tree); tree.write(project, encoding='unicode')
            argv = ['dotnet', 'publish' if args.mode == 'aot' else 'build', project, '-c', 'Release',
                    '--artifacts-path', work / 'artifacts', '-m:1', '-p:UseSharedCompilation=false',
                    '-p:S2nTlsProject=' + str(product),
                    '-p:S2nManagedProject=' + str(ROOT / 'src/Managed.S2nTls/Managed.S2nTls.csproj')]
            if args.mode == 'aot': argv += ['-r', 'linux-x64', '--self-contained', 'true', '-p:PublishAot=true', '-o', work / 'publish']
            command(argv, cwd=external)
            executable = work / 'publish/ManagedConsumer' if args.mode == 'aot' else work / 'artifacts/bin/ManagedConsumer/release/ManagedConsumer.dll'
            launch = [str(executable)] if args.mode == 'aot' else ['dotnet', str(executable)]
            log = command([*launch, '--fixtures', args.fixtures.resolve(), '--source', args.source.resolve()], timeout=120, cwd=external)
            if 'PASS: SSLv3/RC4, SSLv3/3DES, TLS1.3' not in log.read_text(): raise RuntimeError('Missing complete real TLS consumer result')
            report['delivered_notice_files'] = audit_notices(executable.parent)
            row.update(status='passed', log=str(log))
            audit = {'suite': 'delivery', 'case': 'dependencies-and-imports', 'form': args.form, 'mode': args.mode, 'status': 'failed'}
            report['rows'].append(audit)
            closure = [product.parent, ROOT / 'src/Managed.S2nTls', ROOT / 'src/CryptoProvider']
            disallowed = re.compile(r'(?:DllImport|LibraryImport|NativeLibrary\.Load)\s*\(\s*"[^"\n]*(?:libcrypto|libssl|libs2n)|\b(?:Org\.BouncyCastle\.Tls|SslStream)\b')
            for directory in closure:
                for source in directory.rglob('*.cs'):
                    if {'bin', 'obj'} & set(source.parts): continue
                    if disallowed.search(source.read_text()): raise RuntimeError('Disallowed replacement TLS/native product crypto reference: ' + str(source))
            for deps in executable.parent.glob('*.deps.json'):
                data = json.loads(deps.read_text())
                for target in data.get('targets', {}).values():
                    for library in target.values():
                        for asset in (*library.get('native', {}), *library.get('runtimeTargets', {})):
                            if re.search(r'(^|/)(?:lib)?(?:crypto|ssl|s2n)(?:[.\-]|$)', asset, re.I):
                                raise RuntimeError('Native TLS/crypto dependency in product closure: ' + asset)
            if args.mode == 'aot':
                dynamic = command(['ldd', executable], timeout=30)
                if re.search(r'\b(?:libs2n|libcrypto|libssl)[.\-]', dynamic.read_text()): raise RuntimeError('Native crypto/TLS dependency in AOT image')
            audit['status'] = 'passed'
        report['status'] = 'passed'
    except Exception as error:
        report['error'] = str(error)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print('REPORT ' + str(args.output))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
