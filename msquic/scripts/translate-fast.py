#!/usr/bin/env python3
"""Translate and postprocess the selected MsQuic core without qualification."""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
STAGE = ROOT / 'build/product-source'
BUILD = ROOT / 'build/fast-translate'


def run(command):
    subprocess.run([str(part) for part in command], check=True)


def generated_files(directory):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(set(names)) != len(names):
        raise RuntimeError('Invalid source manifest: ' + str(directory))
    if any(not re.fullmatch(r'[A-Za-z_][A-Za-z_0-9.]*\.cs', name) or '..' in name for name in names):
        raise RuntimeError('Unsafe generated source manifest: ' + str(directory))
    return names


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--jobs', type=int, default=min(8, os.cpu_count() or 1),
                        help='parallel object translations (default: min(8, CPU count))')
    args = parser.parse_args()
    if args.jobs < 1:
        parser.error('--jobs must be positive')

    compiler = REPO / 'DotCC/bin/Release/net10.0/dotcc.dll'
    postprocessor = REPO / 'DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll'
    for tool in (compiler, postprocessor):
        if not tool.is_file():
            raise RuntimeError(f'Missing {tool}; omit --no-build-tools or build it first')

    # A fresh checkout has no local qualification evidence to preserve. Once a
    # product has been qualified, archive it before changing its staged/generated
    # files; an existing archive also permits subsequent fast regenerations.
    closure = ROOT / 'config/product-closure.json'
    archive = ROOT / 'artifacts' / ('closure-' + hashlib.sha256(closure.read_bytes()).hexdigest()[:16])
    if (ROOT / 'artifacts/product-build/results.json').is_file() or (archive / 'archive.json').is_file():
        run([sys.executable, ROOT / 'scripts/freeze-product.py', '--archive-current'])
    run([sys.executable, ROOT / 'scripts/generate-host-contract.py'])
    run([sys.executable, ROOT / 'scripts/stage-product.py', '--no-fetch'])
    manifest = json.loads((STAGE / 'manifest.json').read_text())
    defines = ['-D' + value for value in manifest['defines']]
    includes = ['-I' + str(STAGE / path)
                for path in ['system', 'src/inc', 'src/core', 'src/platform', 'host']]
    includes.extend(['-I' + str(ROOT / 'tests/Abi'), '-I' + str(ROOT / 'tests/HostContract')])
    profile = ROOT / 'config/dotcc-overrides.json'

    objects = BUILD / 'core-objects'
    if objects.exists():
        shutil.rmtree(objects)
    objects.mkdir(parents=True)

    def translate(index, unit):
        output = objects / f'{index:02d}-{Path(unit).stem}.cs'
        command = ['dotnet', compiler, '--emit=obj', '--emit-define', 'QUIC_STATUS_*',
                   '--overrides-file', profile, *defines, *includes, STAGE / unit, '-o', output]
        result = subprocess.run([str(part) for part in command], capture_output=True, text=True)
        if result.returncode:
            detail = (result.stdout + result.stderr).strip()
            raise RuntimeError(f'{unit}: translation failed ({result.returncode})' +
                               (f'\n{detail}' if detail else ''))
        return index, unit, output

    translated = []
    with ThreadPoolExecutor(max_workers=args.jobs) as executor:
        futures = [executor.submit(translate, index, unit)
                   for index, unit in enumerate(manifest['units'], 1)]
        try:
            for completed, future in enumerate(as_completed(futures), 1):
                index, unit, output = future.result()
                translated.append((index, output))
                print(f'core object {completed}/{len(futures)}: {unit}', flush=True)
        except Exception:
            for future in futures:
                future.cancel()
            raise
    translated.sort()

    inline_exports = [line.strip() for line in (ROOT / 'config/inline-exports.txt').read_text().splitlines()
                      if line.strip() and not line.lstrip().startswith('#')]
    if not inline_exports or len(set(inline_exports)) != len(inline_exports):
        raise RuntimeError('Inline export selectors must be nonempty and unique')
    inline_flags = ['--deduplicate-inline',
                    *[part for pattern in inline_exports for part in ['--export-inline', pattern]]]
    raw = ROOT / 'generated/raw/TranslatedMsQuic'
    optimized = ROOT / 'generated/TranslatedMsQuic'
    run(['dotnet', compiler, '--emit=managedlib', '--literal-pool', '--nest-types', '--runtime=c',
         '--class-name', 'MsQuic', '--namespace', 'Managed.Transport', '--split=size',
         *inline_flags, *[path for _, path in translated], '-o', raw])

    optimized.mkdir(parents=True, exist_ok=True)
    if (optimized / 'Dotcc.SourceFiles.txt').exists():
        for name in set(generated_files(optimized)) - set(generated_files(raw)):
            (optimized / name).unlink()
    for name in generated_files(raw) + ['Dotcc.SourceFiles.txt', 'TranslatedMsQuic.csproj']:
        shutil.copyfile(raw / name, optimized / name)
    run(['dotnet', 'restore', optimized / 'TranslatedMsQuic.csproj', '--nologo'])
    run(['dotnet', postprocessor, optimized / 'TranslatedMsQuic.csproj', '--in-place'])
    print(f'Fast unqualified translation written to {optimized}')


if __name__ == '__main__':
    try:
        main()
    except (OSError, RuntimeError, subprocess.CalledProcessError) as error:
        raise SystemExit(f'fast translation failed: {error}')
