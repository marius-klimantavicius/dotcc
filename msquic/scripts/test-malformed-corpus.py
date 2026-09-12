#!/usr/bin/env python3
"""Selected pinned native decoder vectors versus frozen raw/optimized JIT/AOT.

No translation or reference mutation. This is a decoder corpus, not the complete
upstream tests, an endpoint malformed-packet campaign, or a fuzzer.
"""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
PICO = ROOT.parent / 'picotls'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def generated(directory):
    names = (directory / 'Dotcc.SourceFiles.txt').read_text().splitlines()
    if not names or len(names) != len(set(names)) or any(Path(n).name != n or not n.endswith('.cs') for n in names):
        raise RuntimeError('Invalid generated source manifest')
    return {n: sha(directory / n) for n in names}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--variants', nargs='+', choices=['raw', 'optimized'], default=['raw', 'optimized'])
    parser.add_argument('--jit-only', action='store_true')
    parser.add_argument('--native-only', action='store_true')
    args = parser.parse_args()
    if platform.system() != 'Linux' or platform.machine() not in ('x86_64', 'amd64'):
        raise RuntimeError('Native reference requires Linux x64 LP64')
    log = ROOT / 'artifacts/malformed-corpus'
    build = ROOT / 'build/malformed-corpus'
    log.mkdir(parents=True, exist_ok=True)
    build.mkdir(parents=True, exist_ok=True)
    receipt = dict(passed=False, targeted_passed=False, full_upstream_suite=False,
                   endpoint_malformed_input_validated=False, commands=[], variants=[])

    def run(command, name):
        command = list(map(str, command))
        print('RUN ' + name, flush=True)
        started = time.monotonic()
        record = dict(name=name, arguments=command)
        receipt['commands'].append(record)
        try:
            result = subprocess.run(command, capture_output=True, text=True, timeout=900)
            record.update(exit_code=result.returncode, seconds=time.monotonic() - started)
            (log / (name + '.log')).write_text(result.stdout + result.stderr)
            if result.returncode:
                raise RuntimeError(name + ' failed; see ' + str(log / (name + '.log')))
            return result.stdout
        except subprocess.TimeoutExpired:
            record.update(timed_out=True, seconds=time.monotonic() - started)
            raise

    try:
        corpus = json.loads((ROOT / 'tests/MalformedCorpus/corpus.json').read_text())
        pin = json.loads((ROOT / 'config/source.json').read_text())
        source = ROOT / 'ref' / pin['directory']
        if corpus['source_commit'] != pin['commit']:
            raise RuntimeError('Corpus pin differs from product source')
        for name, digest in corpus['source_sha256'].items():
            if sha(source / name) != digest:
                raise RuntimeError('Pinned corpus source changed: ' + name)
        cases = corpus['cases']
        if len({c['id'] for c in cases}) != len(cases):
            raise RuntimeError('Duplicate corpus case ID')
        table = build / 'corpus.tsv'
        table.write_text(''.join('\t'.join([c['id'], c['kind'], str(c['server']), str(c['repeat']),
            '1' if c['expected_success'] else '0', c['hex']]) + '\n' for c in cases))
        receipt.update(source_commit=pin['commit'], corpus_cases=len(cases),
                       corpus_sha256=sha(ROOT / 'tests/MalformedCorpus/corpus.json'),
                       reference_source_sha256=corpus['source_sha256'], scope=corpus['scope'])
        inputs = [Path(__file__).resolve(), ROOT / 'config/source.json']
        inputs += sorted(p for p in (ROOT / 'tests/MalformedCorpus').iterdir() if p.is_file())
        inputs += sorted((ROOT / 'src/BclHost').glob('*.cs')) + sorted((ROOT / 'src/BclHost').glob('*.csproj'))
        inputs += sorted((PICO / 'src/BclProvider').glob('*.cs')) + sorted((PICO / 'src/BclProvider').glob('*.csproj'))
        receipt['input_sha256'] = {os.path.relpath(p, ROOT): sha(p) for p in inputs}
        receipt['native_compiler'] = run(['gcc', '--version'], 'gcc-version').splitlines()[0]
        flags = ['-std=gnu17', '-fms-extensions', '-O2', '-ffunction-sections', '-fdata-sections',
                 '-DCX_PLATFORM_LINUX=1', '-D_GNU_SOURCE=1', '-DNDEBUG=1',
                 '-DQUIC_EVENTS_STUB=1', '-DQUIC_LOGS_STUB=1', '-DQUIC_BUILD_STATIC=1',
                 '-I' + str(source / 'src/inc'), '-I' + str(source / 'src/core')]
        objects = []
        dependencies = set()
        for path in [ROOT / 'tests/MalformedCorpus/native.c', source / 'src/core/frame.c', source / 'src/core/crypto_tls.c']:
            obj = build / (path.stem + '.o')
            dep = build / (path.stem + '.d')
            run(['gcc', *flags, '-MD', '-MF', dep, '-c', path, '-o', obj], path.stem + '-compile')
            objects.append(obj)
            for name in dep.read_text().replace('\\\n', ' ').split(':', 1)[1].split():
                dependency = Path(name).resolve()
                if dependency.is_file(): dependencies.add(dependency)
        receipt['native_dependency_sha256'] = {str(p): sha(p) for p in sorted(dependencies)}
        native = build / 'native'
        run(['gcc', '-Wl,--gc-sections', *objects, '-o', native], 'native-link')
        baseline = run([native, table], 'native')
        lines = baseline.splitlines()
        if len(lines) != len(cases) + 1 or lines[-1] != f'PASS malformed corpus cases={len(cases)} allocations=0':
            raise RuntimeError('Incomplete native corpus output')
        if [line.split('|', 1)[0] for line in lines[:-1]] != [c['id'] for c in cases]:
            raise RuntimeError('Native corpus IDs differ')
        receipt['native'] = dict(passed=True, output_sha256=hashlib.sha256(baseline.encode()).hexdigest(), executable_sha256=sha(native))
        if not args.native_only:
            closure = ROOT / 'config/product-closure.json'
            provenance = PICO / 'artifacts/translation/success.json'
            frozen, pico_frozen = json.loads(closure.read_text()), json.loads(provenance.read_text())
            receipt['product_closure_sha256'] = sha(closure)
            receipt['picotls_provenance_sha256'] = sha(provenance)
            for variant in args.variants:
                msquic = ROOT / 'generated' / variant / 'TranslatedMsQuic'
                pico = PICO / 'generated' / ('TranslatedPicotlsRaw' if variant == 'raw' else 'TranslatedPicotls')
                hashes, pico_hashes = generated(msquic), generated(pico)
                if any(frozen['generated'][variant].get(n) != h for n, h in hashes.items()):
                    raise RuntimeError('MsQuic source differs from frozen product closure')
                if any(pico_frozen[variant].get(n) != h for n, h in pico_hashes.items()):
                    raise RuntimeError('picotls source differs from successful translation')
                project = ROOT / 'tests/MalformedCorpus/MalformedCorpus.csproj'
                props = ['-p:MsQuicProject=' + str(msquic / 'TranslatedMsQuic.csproj'),
                         '-p:PicotlsProject=' + str(pico / 'TranslatedPicotls.csproj')]
                run(['dotnet', 'build', project, '-c', 'Release', '--nologo', *props], variant + '-build')
                runtimes = [('jit', ['dotnet', project.parent / 'bin/Release/net10.0/MalformedCorpus.dll', table])]
                if not args.jit_only:
                    destination = build / variant / 'aot'
                    run(['dotnet', 'publish', project, '-c', 'Release', '-r', 'linux-x64',
                         '-p:PublishAot=true', '-o', destination, '--nologo', *props], variant + '-aot-build')
                    runtimes.append(('aot', [destination / 'MalformedCorpus', table]))
                entry = dict(name=variant, passed=False, generated_sha256=hashes, picotls_generated_sha256=pico_hashes)
                for runtime, command in runtimes:
                    output = run(command, variant + '-' + runtime)
                    if output != baseline:
                        (log / (variant + '-' + runtime + '.diff')).write_text(''.join(difflib.unified_diff(
                            baseline.splitlines(True), output.splitlines(True), fromfile='native', tofile=variant + '-' + runtime)))
                        raise RuntimeError('Exact decoder result/offset/field/allocation mismatch: ' + variant + '-' + runtime)
                    entry[runtime] = dict(passed=True, output_sha256=hashlib.sha256(output.encode()).hexdigest())
                if generated(msquic) != hashes or generated(pico) != pico_hashes:
                    raise RuntimeError('Generated source changed during corpus')
                entry['passed'] = True
                receipt['variants'].append(entry)
            if sha(closure) != receipt['product_closure_sha256'] or sha(provenance) != receipt['picotls_provenance_sha256']:
                raise RuntimeError('Frozen source metadata changed during corpus')
        if any(sha(Path(name)) != digest for name, digest in receipt['native_dependency_sha256'].items()):
            raise RuntimeError('Native dependency changed during corpus')
        if receipt['input_sha256'] != {os.path.relpath(p, ROOT): sha(p) for p in inputs}:
            raise RuntimeError('Corpus or host inputs changed during execution')
        receipt['targeted_passed'] = True
        receipt['passed'] = not args.native_only and not args.jit_only and set(args.variants) == {'raw', 'optimized'}
    finally:
        (log / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps({'passed': receipt['passed'], 'targeted_passed': receipt['targeted_passed'], 'cases': receipt['corpus_cases']}))


if __name__ == '__main__':
    main()
