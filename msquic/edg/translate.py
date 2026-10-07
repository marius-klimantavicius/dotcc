#!/usr/bin/env python3
"""Translate the pinned MsQuic core with the existing managed-host contract."""
import argparse
import hashlib
import json
import re
from pathlib import Path
import subprocess
import sys
import tarfile
import time

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / 'msquic'
EDG = ROOT / 'edgcpp'
sys.path.insert(0, str(CAMPAIGN / 'scripts'))
from source_inputs import source_units, portable_fragments


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def stage_source(output):
    pin = json.loads((CAMPAIGN / 'config/source.json').read_text())
    source = CAMPAIGN / 'ref' / pin['directory']
    archive = CAMPAIGN / 'ref' / pin['archive']
    if sha(archive) != pin['sha256']:
        raise ValueError('Pinned MsQuic archive hash mismatch')
    count = 0
    with tarfile.open(archive) as contents:
        for entry in contents:
            if not entry.isfile():
                continue
            relative = Path(*Path(entry.name).parts[1:])
            if sha(source / relative) != hashlib.sha256(contents.extractfile(entry).read()).hexdigest():
                raise ValueError('Modified upstream input: ' + str(relative))
            count += 1
    host = CAMPAIGN / 'config/managed-host'
    overlay = json.loads((host / 'overlay.json').read_text())
    replacements = {item['source']: host / item['overlay'] for item in overlay['overlays']}
    manifest = dict(pin=pin, verifiedUpstreamFiles=count, files=[], units=[],
        defines=['CX_PLATFORM_LINUX=1', '__linux__=1', '_GNU_SOURCE=1', 'NDEBUG=1',
                 'QUIC_BUILD_STATIC=1', 'QUIC_EVENTS_STUB=1', 'QUIC_LOGS_STUB=1',
                 'VER_GIT_HASH=' + pin['commit']],
        include_dirs=['src/inc', 'src/core', 'src/platform', 'host', 'system'])
    def write(relative, data, origin, role):
        path = output / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        manifest['files'].append(dict(path=relative, sha256=sha(path), origin=str(origin), role=role))
    for folder in ('src/inc', 'src/core', 'src/platform'):
        for path in sorted((source / folder).rglob('*')):
            if path.is_file():
                relative = path.relative_to(source).as_posix()
                selected = replacements.get(relative, path)
                write(relative, selected.read_bytes(), selected.relative_to(ROOT),
                      'host overlay' if relative in replacements else 'unchanged upstream')
    fragments = portable_fragments(source, overlay)
    for prefix, folder in [('system', host / 'system'), ('host', CAMPAIGN / 'src/Host')]:
        for path in sorted(folder.rglob('*')):
            if path.is_file():
                data = fragments[path.name].encode() if prefix == 'host' and path.name in fragments else path.read_bytes()
                write((Path(prefix) / path.relative_to(folder)).as_posix(), data, path.relative_to(ROOT),
                      'unchanged extracted upstream' if path.name in fragments else 'authored host contract')
    manifest['units'] = [s for s in source_units(source) if s != 'src/platform/pcp.c']
    manifest['units'] += ['host/' + p.name for p in sorted((CAMPAIGN / 'src/Host').glob('*.c'))]
    roots = CAMPAIGN / 'edg/api_roots.c'
    write('host/edg_api_roots.c', roots.read_bytes(), roots.relative_to(ROOT), 'managed consumer API roots')
    manifest['units'].append('host/edg_api_roots.c')
    (output / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--timeout', type=int, default=1800)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = dict(project='msquic', translator='EDG dot_gen_be', status='running', commands=[])
    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    def run(label, command):
        command = list(map(str, command))
        step = dict(label=label, command=command)
        report['commands'].append(step); save()
        start = time.monotonic()
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=ROOT, stdout=stdout, stderr=stderr, timeout=args.timeout)
        step.update(exitCode=result.returncode, seconds=round(time.monotonic()-start, 3)); save()
        print(label + ': exit ' + str(result.returncode), flush=True)
        if result.returncode:
            raise RuntimeError(label + ' failed; retained logs in ' + str(out))
    try:
        stage = out / 'source'
        manifest = stage_source(stage)
        report.update(source=manifest['pin'], unitCount=len(manifest['units']),
                      compiler=dict(path=str(args.compiler.resolve()), sha256=sha(args.compiler)))
        sources = [stage / unit for unit in manifest['units']]
        object_macros = set()
        for header in stage.rglob('*.h'):
            object_macros.update(re.findall(r'^\s*#\s*define\s+(\w+)[ \t]+', header.read_text(), re.M))
        referenced = set()
        for folder in ('src/BclHost', 'src/ManagedApi', 'tests'):
            for path in (CAMPAIGN / folder).rglob('*.cs'):
                if 'bin' not in path.parts and 'obj' not in path.parts:
                    referenced.update(re.findall(r'\b(?:QUIC|CXPLAT|MSQUIC)_[A-Z0-9_]+\b', path.read_text()))
        macros = sorted((referenced | {'TLS_ERROR_BASE', 'CERT_ERROR_BASE'} |
                         {m for m in object_macros if m.startswith('QUIC_STATUS_')}) & object_macros)
        report['exportedMacros'] = macros
        command = [sys.executable, EDG / 'dotnet.py', *sources, '--language', 'c', '--std', 'c11', '--ms-extensions',
                   *[v for folder in manifest['include_dirs'] for v in ('-I', stage / folder)],
                   *['-D' + value for value in manifest['defines']],
                   '--libc', 'musl', '--posix', '--emit', 'managedlib', '--namespace', 'Managed.Transport',
                   '--class-name', 'MsQuic', '--split', 'source', '--compiler', args.compiler.resolve(),
                   '--output-dir', out / 'managed', '--raw-output', out / 'raw.cs',
                   '--frontend-invocation', out / 'invocation.json',
                   '--export-macros', '(' + '|'.join(macros) + ')', '--macro-source', stage / 'host/edg_api_roots.c']
        run('translate', command)
        if args.build:
            run('build', [sys.executable, EDG / 'build_managed.py', out / 'managed/Generated.csproj',
                          '--output-dir', out / 'build', '--warnings-as-errors'])
        report['status'] = 'passed'
    except Exception as error:
        report.update(status='failed', failure=str(error)); raise
    finally:
        save()


if __name__ == '__main__':
    main()
