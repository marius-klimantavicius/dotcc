"""Native differential subprocess byte/descriptor contract for CLI host services."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import uuid
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from campaigns.delivery import ignore_build_caches, source_files
from campaigns.translation import relocate_project


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--product', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args(); output = args.output.resolve(); output.parent.mkdir(parents=True, exist_ok=True)
    work = ROOT / 'build/managed-stdfd' / uuid.uuid4().hex[:12]; work.mkdir(parents=True)
    report = {'evidence_kind': 'test-only-native-stdfd-contract', 'status': 'failed', 'commands': []}
    payload = bytes([0, 128, 255, 65, 0, 10, 127, 129, 254, 255, 0])

    def command(argv, label, input=None):
        result = subprocess.run(list(map(str, argv)), input=input, stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=180)
        stdout = work / (label + '.stdout'); stdout.write_bytes(result.stdout)
        stderr = work / (label + '.stderr'); stderr.write_bytes(result.stderr)
        report['commands'].append({'command': list(map(str, argv)), 'exit_code': result.returncode,
                                   'stdout': str(stdout), 'stderr': str(stderr)})
        if result.returncode: raise RuntimeError('Native stdfd contract command failed: ' + str(stderr))
        return result

    try:
        product = args.product.resolve()
        report['canonical_sha256'] = {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in source_files(product.parent)}
        snapshot = work / 'product' / product.parent.name
        shutil.copytree(product.parent, snapshot, ignore=ignore_build_caches)
        project = snapshot / product.name; relocate_project(project, product.parent)
        tree = ET.parse(project); props = ET.SubElement(tree.getroot(), 'PropertyGroup')
        for key, value in {'OutputType': 'Exe', 'AssemblyName': 'StdfdProbe', 'StartupObject': 'Managed.Security.S2n.StdfdProbe'}.items():
            ET.SubElement(props, key).text = value
        group = ET.SubElement(tree.getroot(), 'ItemGroup')
        for name in ('StdfdBindings.cs', 'StdfdProbe.cs'):
            ET.SubElement(group, 'Compile', Include=str(ROOT / 'tests/Upstream' / name), Link=name)
        ET.indent(tree); tree.write(project, encoding='unicode')
        native_source = ROOT / 'tests/Upstream/stdfd-native.c'; native = work / 'native'; helper = work / 'libstdfd-probe.so'
        command(['cc', '-std=c99', '-pthread', native_source, '-o', native], 'native-build')
        command(['cc', '-std=c99', '-pthread', '-DSTDFD_HELPER_ONLY=1', '-shared', '-fPIC', native_source, '-o', helper], 'helper-build')
        expected = command([native], 'native', payload)
        command(['dotnet', 'build', project, '-c', 'Release', '--artifacts-path', work / 'artifacts', '-m:1', '--nologo'], 'managed-build')
        observed = command(['dotnet', work / 'artifacts/bin' / project.stem / 'release/StdfdProbe.dll', helper], 'managed', payload)
        if expected.stdout != payload or observed.stdout != payload: raise RuntimeError('Standard descriptors changed actual NUL/high-bit binary bytes')
        if expected.stderr != observed.stderr: raise RuntimeError('Native/mapped mixed-descriptor observations differ')
        report.update(status='passed', payload_hex=payload.hex(), observations=observed.stderr.decode().splitlines())
    except Exception as error:
        report['error'] = str(error)
    output.write_text(json.dumps(report, indent=2) + '\n'); print('REPORT ' + str(output))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
