#!/usr/bin/env python3
"""Native synthetic proof of the selected musl formatter; never C# acceptance."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--gcc', default='gcc')
    parser.add_argument('--clang', default='clang')
    options = parser.parse_args()
    output = options.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    root = Path(__file__).resolve().parents[2]
    source = root / 'backend/runtime/musl/printf'
    snapshot = output / 'source'
    shutil.copytree(source, snapshot)
    shutil.copy2(Path(__file__).with_name('musl_printf_native.c'), snapshot)
    manifest = json.loads((snapshot / 'source.json').read_text())
    for name, field in [('vfprintf.original.c', 'originalSha256'), ('vfprintf.c', 'adaptedSha256')]:
        assert hashlib.sha256((snapshot / name).read_bytes()).hexdigest() == manifest[field], name
    checks = []
    try:
        # Patch verification uses an isolated copy; never rewrites the vendored source.
        patched = output / 'patch-check'
        patched.mkdir()
        shutil.copy2(snapshot / 'vfprintf.original.c', patched / 'vfprintf.c')
        command = ['patch', '--batch', 'vfprintf.c', str(snapshot / 'vfprintf.patch')]
        result = subprocess.run(command, cwd=patched, text=True, capture_output=True)
        (output / 'patch.log').write_text(result.stdout + result.stderr)
        result.check_returncode()
        assert (patched / 'vfprintf.c').read_bytes() == (snapshot / 'vfprintf.c').read_bytes()
        checks.append({'name': 'provenance-and-patch', 'passed': True})
        for name in ['gcc', 'clang']:
            compiler = shutil.which(getattr(options, name))
            if compiler is None:
                raise RuntimeError(f'{name} is required for this differential probe')
            binary = output / name
            command = [compiler, '-std=c11', '-D_GNU_SOURCE', '-U_FORTIFY_SOURCE',
                       '-D_FORTIFY_SOURCE=0', '-frounding-math', '-O2',
                       str(snapshot / 'vfprintf.c'), str(snapshot / 'vsnprintf.c'),
                       str(snapshot / 'musl_printf_native.c'), '-lm', '-o', str(binary)]
            # Host fortification uses a different MB_LEN_MAX. This flag is solely
            # for the isolated native UTF-8 bridge, not a production requirement.
            result = subprocess.run(command, text=True, capture_output=True)
            (output / f'{name}-build.log').write_text(result.stdout + result.stderr)
            (output / f'{name}-command.json').write_text(json.dumps(command, indent=2) + '\n')
            result.check_returncode()
            result = subprocess.run([str(binary)], text=True, capture_output=True, timeout=60)
            (output / f'{name}-run.log').write_text(result.stdout + result.stderr)
            result.check_returncode()
            assert '109 synthetic sink/glibc cases passed' in result.stdout
            checks.append({'name': name, 'passed': True, 'output': result.stdout.strip()})
    finally:
        (output / 'results.json').write_text(json.dumps({
            'scope': 'native selected algorithm and synthetic corpus only',
            'checks': checks,
            'generatedCSharpVerified': False,
        }, indent=2) + '\n')
    print(json.dumps(checks, indent=2))


if __name__ == '__main__':
    main()
