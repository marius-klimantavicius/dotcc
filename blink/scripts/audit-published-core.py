#!/usr/bin/env python3
"""Inventory the exact Linux NativeAOT binaries from a passing core receipt.
This is an ELF dependency/layout inventory, not a runtime isolation proof.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import tempfile
ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('execution_receipt', type=Path)
args = parser.parse_args()
source = args.execution_receipt.resolve()
execution = json.loads(source.read_text())
if not execution.get('passed') or not execution.get('runtime_matrix_passed'):
    raise SystemExit('A canonical passing runtime matrix is required')
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
base = ROOT / 'artifacts/publication-audit'
base.mkdir(parents=True, exist_ok=True)
out = Path(tempfile.mkdtemp(prefix='attempt-', dir=base))
receipt = dict(kind='nativeaot-elf-publication-inventory', passed=False,
               execution_receipt=str(source), execution_receipt_sha256=sha(source),
               runner_sha256=sha(Path(__file__)), binaries={}, limitations=[
                   'Dynamic ELF imports do not expose statically linked code or indirect/dynamic native loading.',
                   'ELF executable segments contain the AOT host program; their presence does not imply executable guest allocations.',
                   'This inventory does not establish absence of later executable mappings or runtime isolation.',
                   'Only Linux x64 binaries executed by the referenced receipt are included; Windows is unrun.'])
def run(command, name):
    result = subprocess.run(list(map(str,command)), stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=30)
    (out / (name+'.txt')).write_bytes(result.stdout)
    if result.returncode: raise RuntimeError(name+' failed')
    return result.stdout.decode()
try:
    receipt['readelf'] = run(['readelf','--version'],'readelf-version').splitlines()[0]
    for mode in ['raw','optimized']:
        result = execution['results'][mode+'-aot']
        binary = Path(result['command'][0])
        expected = result.get('binary_sha256')
        if not expected or sha(binary) != expected: raise RuntimeError(mode+' lacks matching execution-time binary identity')
        header = run(['readelf','-W','-h',binary],mode+'-header')
        dynamic = run(['readelf','-W','-d',binary],mode+'-dynamic')
        segments = run(['readelf','-W','-l',binary],mode+'-segments')
        symbols = run(['readelf','-W','--dyn-syms',binary],mode+'-symbols')
        if 'ELF64' not in header or 'Advanced Micro Devices X86-64' not in header:
            raise RuntimeError('Unexpected publication format')
        needed = re.findall(r'\(NEEDED\).*\[([^\]]+)\]',dynamic)
        imports = sorted({line.split()[7] for line in symbols.splitlines() if ' UND ' in line and len(line.split()) >= 8})
        loads = [line.strip() for line in segments.splitlines() if line.strip().startswith('LOAD ')]
        if any(re.search(r'\bRWE\b|\bRW E\b',line) for line in loads):
            raise RuntimeError('Writable executable ELF load segment')
        if any('blink' in name.lower() or 'qemu' in name.lower() for name in needed):
            raise RuntimeError('Native emulator dynamic dependency')
        if sha(binary) != expected: raise RuntimeError('Publication changed while inspected')
        receipt['binaries'][mode] = dict(path=str(binary),sha256=expected,needed=needed,
            undefined_dynamic_symbols=imports,load_segments=loads)
    receipt['passed']=True
    print(out / 'receipt.json')
finally:
    (out / 'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n')
