#!/usr/bin/env python3
"""Fetch and install the hash-pinned, test-only Linux x64 aioquic oracle."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
pin = json.loads((ROOT / 'config/independent-inputs.json').read_text())
logs = ROOT / 'artifacts/independent-peer'
logs.mkdir(parents=True, exist_ok=True)
receipt = dict(passed=False, product_dependency=False, commands=[], inputs=pin)
(logs / 'setup-results.json').write_text(json.dumps(receipt, indent=2) + '\n')


def fetch(path, url, digest):
    if not path.exists():
        path.parent.mkdir(parents=True, exist_ok=True)
        with urllib.request.urlopen(url, timeout=60) as response:
            contents = response.read()
        if hashlib.sha256(contents).hexdigest() != digest:
            raise RuntimeError('Download hash mismatch: ' + str(path))
        path.write_bytes(contents)
    if hashlib.sha256(path.read_bytes()).hexdigest() != digest:
        raise RuntimeError('Existing input hash mismatch: ' + str(path))


def run(command, name):
    receipt['commands'].append(command)
    result = subprocess.run(command, text=True, capture_output=True, timeout=120)
    (logs / (name + '.log')).write_text(result.stdout + result.stderr)
    if result.returncode:
        raise RuntimeError('Failed: ' + name)
    return result.stdout


try:
    archive = ROOT / 'ref' / pin['archive']
    fetch(archive, pin['source_url'], pin['sha256'])
    directory = ROOT / 'ref' / pin['directory']
    with tarfile.open(archive) as source:
        if not directory.exists():
            source.extractall(ROOT / 'ref', filter='data')
        for member in source.getmembers():
            if member.isfile() and (ROOT / 'ref' / member.name).read_bytes() != source.extractfile(member).read():
                raise RuntimeError('Modified reference input: ' + member.name)
    wheels = ROOT / 'ref/independent-peer-wheels'
    for wheel in pin['wheels']:
        fetch(wheels / wheel['filename'], wheel['url'], wheel['sha256'])
    requirements = logs / 'requirements.txt'
    requirements.write_text(''.join(f"{w['name']}=={w['version']} --hash=sha256:{w['sha256']}\n" for w in pin['wheels']))
    venv = ROOT / 'build/independent-peer-venv'
    run([sys.executable, '-m', 'venv', '--without-pip', str(venv)], 'venv')
    python = venv / 'bin/python'
    run([sys.executable, '-m', 'pip', '--python', str(python), 'install', '--no-index',
         '--no-deps', '--force-reinstall', '--require-hashes', '--find-links', str(wheels),
         '-r', str(requirements)], 'install')
    run([sys.executable, '-m', 'pip', '--python', str(python), 'check'], 'dependency-check')
    site = Path(run([str(python), '-c', 'import aioquic; print(aioquic.__path__[0])'], 'package-path').strip())
    # Match the installed wheel's transport/TLS Python code to the source pin.
    # Native packet primitive extensions retain their separately pinned wheel hash.
    verified = []
    for reference in sorted((directory / 'src/aioquic').rglob('*.py')):
        relative = reference.relative_to(directory / 'src/aioquic')
        if reference.read_bytes() != (site / relative).read_bytes():
            raise RuntimeError('Wheel/source Python mismatch: ' + str(relative))
        verified.append(str(relative))
    receipt.update(passed=True, verified_python_sources=verified,
                   python=run([str(python), '--version'], 'python-version').strip())
finally:
    (logs / 'setup-results.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({'passed': receipt['passed'], 'version': pin['version'], 'wheels': len(pin['wheels'])}))
