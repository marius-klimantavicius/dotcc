#!/usr/bin/env python3
"""Translate the verified, pinned picotls core with EDG; retain every attempt."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import tarfile
import time

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / 'picotls'
EDG = ROOT / 'edgcpp'


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def verified_source():
    pin = json.loads((CAMPAIGN / 'config/inputs.json').read_text())['picotls']
    source = CAMPAIGN / 'ref' / pin['directory']
    archive = CAMPAIGN / 'ref' / (pin['directory'] + '.tar.gz')
    if sha(archive) != pin['sha256']:
        raise ValueError('Pinned picotls archive hash mismatch')
    count = 0
    with tarfile.open(archive) as contents:
        for entry in contents:
            if not entry.isfile():
                continue
            relative = Path(*Path(entry.name).parts[1:])
            expected = hashlib.sha256(contents.extractfile(entry).read()).hexdigest()
            if sha(source / relative) != expected:
                raise ValueError('Modified pinned upstream file: ' + str(relative))
            count += 1
    return source, dict(pin, verifiedFiles=count)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--libc-pruning', choices=('reachable', 'all'), default='reachable',
                        help='trim unused provider definitions by default; all retains the complete provider for comparison')
    parser.add_argument('--timeout', type=int, default=900)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = {'project':'picotls', 'translator':'EDG dot_gen_be', 'status':'running',
              'commands':[], 'limitations':['Selected TLS profile; optional logging and DTrace disabled.']}
    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    def run(label, command):
        command = list(map(str, command))
        step = {'label':label, 'command':command}
        report['commands'].append(step); save()
        start = time.monotonic()
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=ROOT, stdout=stdout, stderr=stderr, timeout=args.timeout)
        step.update(exitCode=result.returncode, seconds=round(time.monotonic()-start, 3)); save()
        print(label + ': exit ' + str(result.returncode), flush=True)
        if result.returncode:
            raise RuntimeError(label + ' failed; retained logs in ' + str(out))
    try:
        source, pin = verified_source()
        sources = [CAMPAIGN/'src/Host/picotls-quic.c', source/'lib/hpke.c',
                   source/'lib/pembase64.c', CAMPAIGN/'src/Host/picotls-host.c']
        definitions = [v.strip() for v in (CAMPAIGN/'config/core-defines.txt').read_text().splitlines()
                       if v.strip() and not v.lstrip().startswith('#')]
        # Export the object-like public constants actually referenced by the existing
        # provider/tests; function-like macros require separate typed C contracts.
        referenced = set()
        for folder in [CAMPAIGN/'src/BclProvider', CAMPAIGN/'tests', ROOT/'msquic/src/BclHost']:
            for path in folder.rglob('*.cs'):
                if 'obj' in path.parts or 'bin' in path.parts: continue
                referenced.update(re.findall(r'\bPTLS_[A-Z0-9_]+\b', path.read_text()))
        # pembase64 includes the public header and defines the PEM certificate
        # count limit consumed by the existing provider's boundary checks.
        macro_source = source/'lib/pembase64.c'
        macro_text = (source/'include/picotls.h').read_text() + '\n' + macro_source.read_text()
        object_macros = set(re.findall(r'^\s*#\s*define\s+(PTLS_\w+)[ \t]+', macro_text, re.M))
        exported = sorted(referenced & object_macros)
        report.update(source=pin, definitions=definitions, exportedMacros=exported, libcPruning=args.libc_pruning,
                      compiler={'path':str(args.compiler.resolve()), 'sha256':sha(args.compiler)},
                      inputs=[{'path':str(p.relative_to(ROOT)), 'sha256':sha(p)} for p in sources])
        command = [sys.executable, EDG/'dotnet.py', *sources, '--language','c','--std','c11',
                   '-I',source,'-I',source/'include', *['-D'+v for v in definitions],
                   '--libc','musl','--libc-pruning',args.libc_pruning,'--posix','--emit','managedlib',
                   '--namespace','Managed.Security','--class-name','PicoTls','--split','source',
                   '--compiler',args.compiler.resolve(), '--output-dir',out/'managed',
                   '--raw-output',out/'raw.cs','--frontend-invocation',out/'invocation.json']
        if exported:
            command += ['--export-macros','^('+'|'.join(exported)+')$', '--macro-source',macro_source]
        run('translate', command)
        if args.build:
            run('build', [sys.executable, EDG/'build_managed.py',out/'managed/Generated.csproj',
                          '--output-dir',out/'build','--warnings-as-errors'])
        report['status']='passed'
        report['raw']={'bytes':(out/'raw.cs').stat().st_size, 'sha256':sha(out/'raw.cs')}
    except Exception as error:
        report.update(status='failed', failure=str(error)); raise
    finally:
        save()


if __name__ == '__main__':
    main()
