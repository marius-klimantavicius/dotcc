#!/usr/bin/env python3
"""Translate the complete configured DuckDB library or its upstream test runner.

Preprocess each CMake TU separately so target-specific defines/include order do
not leak into other sources. Keep those files and the exact command receipts.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import shutil
import time

from preflight import EDG, frontend_flags, sha
from libcxx import prepare_provider
from portable import verify_staged


HEADER_SUFFIXES = {'', '.h', '.hh', '.hpp', '.hxx', '.inc', '.inl', '.ipp', '.tcc', '.def'}
LINE_MARKER = re.compile(rb'^#\s*(?:line\s+)?\d+\s+("(?:\\.|[^"\\])*")', re.M)
VOLATILE_MACRO = re.compile(rb'\b__(?:DATE|TIME|TIMESTAMP)__\b|^\s*#\s*embed\b', re.M)
UNFIXED_VOLATILE_MACRO = re.compile(rb'\b__TIMESTAMP__\b|^\s*#\s*embed\b', re.M)


def preprocessing_clock(epoch):
    if epoch is None:
        return None
    value = datetime.fromtimestamp(epoch, timezone.utc)
    month = ('Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec')[value.month - 1]
    return dict(epochSeconds=epoch, timezone='UTC',
                date=f'{month} {value.day:2d} {value.year:04d}',
                time=f'{value.hour:02d}:{value.minute:02d}:{value.second:02d}')


def preprocessing_command(compiler, standard, includes, preincludes, unit, epoch=None):
    if unit['language'] != 'c++':
        raise ValueError('Mixed-language whole-program input needs an explicit bridge: ' + unit['file'])
    flags, recorded = frontend_flags(unit)
    clock = preprocessing_clock(epoch)
    if clock is not None:
        flags += ['-D__DATE__=' + json.dumps(clock['date']), '-D__TIME__=' + json.dumps(clock['time'])]
    return ([str(compiler), '--' + standard, '--gnu_version', '130300',
             '--clear_flag', 'use_predefined_macro_file', '--preprocess',
             *[v for inc in includes for v in ('-I', str(inc))],
             *[v for inc in preincludes for v in ('--preinclude', str(inc))],
             *flags, unit['file']], recorded)


def preprocessing_inputs(portable, portable_path, compiler, standard, includes,
                         preincludes, units, env, target, epoch=None):
    """Fingerprint preprocessing inputs independently of provider source assembly.

    All external search-directory entries are inventoried: adding a file can
    change include lookup or __has_include. Header contents are always hashed.
    Other file contents only affect eligibility if a line marker shows they were
    actually consumed (so merely editing a provider .cpp does not invalidate a
    DuckDB preprocessing cache). We still hash these files before preprocessing
    to check any subsequently discovered dependency against the initial inputs.
    """
    staged = Path(portable['stagedSource']).parent.resolve()
    initial_hashes = {str(staged / item['path']): item['sha256']
                      for item in portable['stagedInputs'] if 'sha256' in item}
    selected, roots = [], set(map(lambda p: Path(p).resolve(), includes))
    roots.update(Path(p).resolve().parent for p in preincludes)
    for index, unit in enumerate(units):
        command, recorded = preprocessing_command(compiler, standard, includes, preincludes, unit, epoch)
        source = Path(unit['file']).resolve()
        selected.append(dict(index=index, source=str(source), sourceSha256=sha(source),
                             language=unit['language'], role=unit['role'], arguments=command[1:]))
        roots.add(source.parent)
        for i, argument in enumerate(command[:-1]):
            if argument == '-I':
                roots.add(Path(command[i + 1]).resolve())
            elif argument == '--preinclude':
                roots.add(Path(command[i + 1]).resolve().parent)
    roots.add(Path(env['EDG_BASE']).resolve())
    # Public libc/POSIX headers also use relative includes into sibling musl
    # ABI directories. They are preprocessing inputs despite not being -I
    # search roots themselves; record the shared runtime tree before reading
    # any TU so discovered dependencies can be checked against initial hashes.
    roots.add((EDG / 'backend/runtime').resolve())
    external = sorted(root for root in roots if not root.is_relative_to(staged))
    # Nested search roots remain in the commands; inventory each physical tree
    # once to avoid repeatedly hashing the same libc++ or adapter headers.
    minimal = [root for root in external if not any(root != parent and root.is_relative_to(parent)
                                                   for parent in external)]
    inventory, visited = [], set()
    pending = list(minimal)
    while pending:
        root = pending.pop(0)
        if root in visited:
            continue
        visited.add(root)
        if not root.is_dir():
            inventory.append(dict(path=str(root), kind='missing-search-directory'))
            continue
        for path in sorted(root.rglob('*')):
            if path.is_symlink() and path.is_dir():
                # EDG_BASE/include is itself a symlink. Record both its target
                # identity and the complete target inventory, including nested
                # links, without recursively traversing directory cycles.
                inventory.append(dict(path=str(path), kind='directory-link',
                                      linkTarget=str(path.readlink()), resolvedTarget=str(path.resolve())))
                pending.append(path.resolve())
                continue
            if path.is_dir():
                continue
            item = dict(path=str(path))
            if path.is_symlink():
                item['linkTarget'] = str(path.readlink())
            if path.is_file():
                digest = sha(path)
                initial_hashes[str(path)] = digest
                initial_hashes[str(path.resolve())] = digest
                item['kind'] = 'file'
                if path.suffix.lower() in HEADER_SUFFIXES or path.is_relative_to(Path(env['EDG_BASE'])):
                    item['sha256'] = digest
            else:
                item['kind'] = 'missing-link-target'
            inventory.append(item)
    forced = []
    for path in preincludes:
        path = Path(path).resolve()
        digest = sha(path)
        initial_hashes[str(path)] = digest
        forced.append(dict(path=str(path), sha256=digest))
    relevant_env = {key: hashlib.sha256(value.encode()).hexdigest() for key, value in sorted(env.items())
                    if key.startswith(('EDG_', 'LC_')) or key in
                    ('LANG', 'TZ', 'SOURCE_DATE_EPOCH', 'USR_INCLUDE', 'CPATH',
                     'C_INCLUDE_PATH', 'CPLUS_INCLUDE_PATH', 'LIBPATH')}
    target_files = [EDG / 'backend/target/portable-lp64-binary80-v2.def',
                    EDG / 'backend/target/portable-lp64-binary80-v2.json']
    fingerprint = dict(schema=1, target=target, standard=standard, compilerSha256=sha(compiler),
        preprocessingClock=preprocessing_clock(epoch),
        portableManifestSha256=sha(portable_path), selected=selected,
        environmentValueHashes=relevant_env, headerInventory=sorted(inventory, key=lambda item: item['path']), preincludeHeaders=forced,
        targetInputs=[dict(path=str(path), sha256=sha(path)) for path in target_files])
    return fingerprint, initial_hashes


def included_files(path):
    """Read EDG line markers, without modifying the preprocessed program."""
    data = Path(path).read_bytes()
    files = set()
    # EDG repeats a header's line marker many times. Resolve each distinct
    # spelling once per TU instead of doing filesystem lookups for every line.
    for spelling in {match.group(1) for match in LINE_MARKER.finditer(data)}:
        name = json.loads(spelling.decode())
        candidate = Path(name)
        # #line can name a virtual source. Actual absolute file paths are the
        # dependencies; synthetic <built-in> and diagnostic-only labels are not.
        if candidate.is_absolute() and candidate.is_file():
            files.add(str(candidate.resolve()))
    return files


def dependency_receipt(paths, initial_hashes, epoch=None):
    records, volatile = [], []
    for name in sorted(paths):
        path = Path(name)
        data = path.read_bytes()
        digest = hashlib.sha256(data).hexdigest()
        if initial_hashes.get(name) != digest:
            raise ValueError('Preprocessing read an untracked or changed input: ' + name)
        records.append(dict(path=name, sha256=digest))
        volatile_pattern = VOLATILE_MACRO if epoch is None else UNFIXED_VOLATILE_MACRO
        if volatile_pattern.search(data):
            volatile.append(name)
    return records, volatile


def verify_dependencies(records):
    for item in records:
        if sha(item['path']) != item['sha256']:
            raise ValueError('Preprocessing dependency changed: ' + item['path'])


def preprocessing_receipt_digest(report):
    try:
        payload = {key: report[key] for key in ('preprocessingInputs', 'preprocessingDependencies',
                   'preprocessingVolatileInputs', 'preprocessingComplete', 'preprocessingInputsUnchanged',
                   'preprocessing')}
    except KeyError as error:
        raise ValueError('Incomplete preprocessing receipt: ' + str(error)) from error
    return hashlib.sha256(json.dumps(payload, sort_keys=True, separators=(',', ':')).encode()).hexdigest()


def reuse_preprocessed(receipt_path, fingerprint):
    """Validate a complete preprocessing result, never a prior codegen claim."""
    receipt_path = Path(receipt_path).resolve()
    receipt_bytes = receipt_path.read_bytes()
    digest = hashlib.sha256(receipt_bytes).hexdigest()
    previous = json.loads(receipt_bytes)
    if previous.get('preprocessingInputs', {}).get('schema') != 1:
        raise ValueError('Legacy receipt lacks a complete preprocessing fingerprint; preprocess once with this driver')
    if previous.get('preprocessingReceiptSha256') != preprocessing_receipt_digest(previous):
        raise ValueError('Preprocessing receipt integrity digest does not match')
    if previous['preprocessingInputs'] != fingerprint:
        raise ValueError('Preprocessing configuration, sources, compiler, headers or environment differ from the reuse receipt')
    if not previous.get('preprocessingComplete') or not previous.get('preprocessingInputsUnchanged'):
        raise ValueError('Reuse receipt does not establish completed preprocessing with unchanged inputs')
    if previous.get('preprocessingVolatileInputs'):
        raise ValueError('Preprocessing uses time-dependent macros or embedded input; automatic reuse is disabled')
    # Earlier schema-1 receipts used a single compiler for both stages. The
    # fallback preserves their exact preprocessing compiler identity.
    prior_preprocessor = previous.get('preprocessingCompiler', previous.get('compiler', {}))
    if prior_preprocessor.get('sha256') != fingerprint['compilerSha256']:
        raise ValueError('Reuse compiler provenance does not match')
    if sha(prior_preprocessor['path']) != fingerprint['compilerSha256']:
        raise ValueError('Retained preprocessing compiler changed')
    if previous.get('portableManifest', {}).get('sha256') != fingerprint['portableManifestSha256']:
        raise ValueError('Reuse portable source manifest does not match')
    if sha(previous['portableManifest']['path']) != fingerprint['portableManifestSha256']:
        raise ValueError('Retained portable source manifest changed')
    verify_dependencies(previous['preprocessingDependencies'])
    entries = previous.get('preprocessing', [])
    if len(entries) != len(fingerprint['selected']) or previous.get('selectedUnits') != len(entries):
        raise ValueError('Reuse receipt has an incomplete source list')
    reused = []
    for entry, selected in zip(entries, fingerprint['selected']):
        if (not entry.get('passed') or entry.get('exitCode') != 0 or
            entry.get('index') != selected['index'] or entry.get('source') != selected['source'] or
            entry.get('sourceSha256') != selected['sourceSha256'] or
            entry.get('command', [])[1:] != selected['arguments']):
            raise ValueError('Reuse receipt source order, identity or preprocessing command changed')
        if sha(entry['path']) != entry['sha256']:
            raise ValueError('Retained preprocessed input changed: ' + entry['path'])
        reused.append(dict(entry, reusedFrom=dict(receipt=str(receipt_path), receiptSha256=digest,
                                                 path=entry['path'])))
    if sha(receipt_path) != digest:
        raise ValueError('Reuse receipt changed during verification')
    return reused, previous['preprocessingDependencies']


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--portable-manifest', type=Path, required=True)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG / 'build/bin/cpfe-dot')
    parser.add_argument('--preprocessing-compiler', type=Path,
                        help='explicit compiler for retained .ii inputs; defaults to --compiler, independently verified from final codegen')
    parser.add_argument('--target', choices=['library', 'tests'], default='library')
    parser.add_argument('--std', choices=['c++17', 'c++20'], default='c++17')
    parser.add_argument('--libcxx-layout', choices=['unity', 'separate'], default='unity',
                        help='source assembly of the real libc++ implementation; does not merge DuckDB source scopes')
    parser.add_argument('--jobs', type=int, default=4)
    parser.add_argument('--timeout', type=int, default=3600)
    parser.add_argument('--preprocess-only', action='store_true')
    parser.add_argument('--preprocessing-epoch', type=int,
                        help='explicit UNIX seconds for UTC __DATE__/__TIME__; unset uses the compiler clock and rejects time-dependent cache reuse')
    parser.add_argument('--reuse-preprocessed', type=Path,
                        help='reuse verified application .ii inputs from a compatible results.json; provider preprocessing and codegen are fresh')
    args = parser.parse_args()
    if args.jobs < 1 or args.timeout < 1:
        parser.error('--jobs and --timeout must be positive')
    if args.preprocessing_epoch is not None and not 0 <= args.preprocessing_epoch <= 253402300799:
        parser.error('--preprocessing-epoch must be between 0 and 253402300799 (UTC years 1970 through 9999)')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    portable = json.loads(args.portable_manifest.read_text())
    roles = ('library', 'test') if args.target == 'tests' else ('library',)
    units = [unit for unit in portable['units'] if unit['role'] in roles]
    compiler_input = args.compiler.resolve()
    compiler = out / 'cpfe-dot'
    shutil.copy2(compiler_input, compiler)
    preprocessing_compiler_input = (args.preprocessing_compiler or compiler_input).resolve()
    preprocessor = out / 'cpfe-dot-preprocess'
    shutil.copy2(preprocessing_compiler_input, preprocessor)
    report = dict(status='running', target=args.target, selectedUnits=len(units),
                  portableManifest=dict(path=str(args.portable_manifest.resolve()), sha256=sha(args.portable_manifest)),
                  compiler=dict(path=str(compiler), originalPath=str(compiler_input), sha256=sha(compiler)),
                  preprocessingCompiler=dict(path=str(preprocessor), originalPath=str(preprocessing_compiler_input),
                                             sha256=sha(preprocessor)),
                  preprocessing=[], providerPreprocessing=[], commands=[], executionVerified=False,
                  preprocessingComplete=False, preprocessingInputsUnchanged=False)
    def save():
        temporary = out / 'results.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(out / 'results.json')
    save()
    env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'),
               EDG_DOT_TARGET='portable-lp64-binary80-v2')
    provider, includes, preincludes, fingerprint = {}, [], [], None
    dependency_paths = set()
    pp = out / 'preprocessed'
    pp.mkdir()

    def preprocess(index, unit, prefix=''):
        source = Path(unit['file'])
        path = pp / f'{prefix}{index:05d}-{source.stem}.ii'
        work = pp / f'{prefix}{index:05d}'
        work.mkdir()
        command, recorded = preprocessing_command(preprocessor, provider['standard'], includes, preincludes, unit,
                                                  args.preprocessing_epoch)
        entry = dict(index=index, source=str(source), sourceSha256=sha(source), command=command,
                     path=str(path), nativeFlagsNotForwarded=recorded)
        start = time.monotonic()
        try:
            with path.open('wb') as stdout, (work / 'stderr').open('wb') as stderr:
                result = subprocess.run(command, env=env, cwd=work, stdout=stdout, stderr=stderr, timeout=args.timeout)
            entry.update(exitCode=result.returncode, passed=result.returncode == 0)
        except subprocess.TimeoutExpired:
            entry.update(passed=False, timeout=True)
        entry.update(seconds=round(time.monotonic() - start, 3), sha256=sha(path))
        (work / 'command.json').write_text(json.dumps(entry, indent=2) + '\n')
        return entry, included_files(path) if entry['passed'] else set()

    def run(label, command):
        command = list(map(str, command))
        step = dict(label=label, command=command)
        report['commands'].append(step)
        save()
        with (out / (label + '.stdout')).open('wb') as stdout, (out / (label + '.stderr')).open('wb') as stderr:
            result = subprocess.run(command, cwd=out, stdout=stdout, stderr=stderr, timeout=args.timeout)
        step['exitCode'] = result.returncode
        save()
        if result.returncode:
            raise RuntimeError(label + ' failed; retained logs in ' + str(out))

    try:
        verify_staged(portable)
        provider = prepare_provider(EDG, args.acquisition, out / 'libcxx', standard=args.std,
                                    unity_provider=args.libcxx_layout == 'unity')
        report['provider'] = provider
        includes = [*map(Path, provider['includeDirectories']), EDG / 'backend/runtime/libcxx/include',
                    EDG / 'backend/runtime/posix/include', EDG / 'backend/runtime/libc/include']
        preincludes = list(map(Path, provider.get('preincludeHeaders', [])))
        fingerprint, initial_hashes = preprocessing_inputs(portable, args.portable_manifest, preprocessor,
            provider['standard'], includes, preincludes, units, env, args.target, args.preprocessing_epoch)
        report['preprocessingInputs'] = fingerprint
        save()
        if args.reuse_preprocessed:
            report['preprocessing'], report['preprocessingDependencies'] = reuse_preprocessed(args.reuse_preprocessed, fingerprint)
            report['preprocessingVolatileInputs'] = []
            report['reusedPreprocessing'] = dict(path=str(args.reuse_preprocessed.resolve()), sha256=sha(args.reuse_preprocessed))
            print(str(len(units)) + ' verified application preprocessing inputs reused; provider/codegen remain fresh', flush=True)
        else:
            with ThreadPoolExecutor(max_workers=args.jobs) as pool:
                futures = [pool.submit(preprocess, i, unit) for i, unit in enumerate(units)]
                for future in as_completed(futures):
                    entry, dependencies = future.result()
                    dependency_paths.update(dependencies)
                    report['preprocessing'].append(entry)
                    report['preprocessing'].sort(key=lambda r: r['index'])
                    if len(report['preprocessing']) % 25 == 0 or not entry['passed']:
                        save()
                    if not entry['passed'] or len(report['preprocessing']) % 100 == 0:
                        print(str(len(report['preprocessing'])) + '/' + str(len(units)) + ' preprocessed; ' +
                              ('PASS ' if entry['passed'] else 'FAIL ') + entry['source'], flush=True)
        failed = [r for r in report['preprocessing'] if not r['passed']]
        if failed:
            raise RuntimeError(str(len(failed)) + ' preprocessing failures; all source logs retained')
        if not args.reuse_preprocessed:
            report['preprocessingDependencies'], report['preprocessingVolatileInputs'] = dependency_receipt(
                dependency_paths, initial_hashes, args.preprocessing_epoch)
        report['preprocessingComplete'] = True
        save()
        # The real implementation provider is always freshly preprocessed with
        # the same public ABI header contract. Its raw C++ and include paths must
        # not leak into dotnet.py's separate musl source preprocessing.
        provider_initial_hashes = dict(initial_hashes)
        provider_records = [*provider['sources'], *provider['generatedInputs'],
                            *[item for item in provider.get('adaptations', []) if 'path' in item and 'sha256' in item]]
        provider_initial_hashes.update({str(Path(item['path']).resolve()): item['sha256'] for item in provider_records})
        provider_dependencies = set()
        for index, source in enumerate(provider['translationUnits']):
            unit = dict(file=source, language='c++', directory=str(Path(source).parent), arguments=['c++'])
            entry, dependencies = preprocess(index, unit, prefix='provider-')
            report['providerPreprocessing'].append(entry)
            provider_dependencies.update(dependencies)
            report['providerPreprocessingDependencies'], _ = dependency_receipt(
                provider_dependencies, provider_initial_hashes, args.preprocessing_epoch)
            save()
            if not entry['passed']:
                raise RuntimeError('libc++ provider preprocessing failed; application cache and provider logs retained')
        if args.preprocess_only:
            report['status'] = 'preprocessed'
            return
        command = [sys.executable, EDG / 'dotnet.py', *[r['path'] for r in report['providerPreprocessing']],
                   *[r['path'] for r in report['preprocessing']], '--compiler', compiler,
                   '--std', provider['standard'], '--libc', 'musl', '--posix', '--unbound-externals', 'reject',
                   '--namespace', 'Managed.Analytics', '--class-name', 'DuckDb', '--split', 'source',
                   '--emit', 'exe' if args.target == 'tests' else 'managedlib',
                   '--output-dir', out / 'managed', '--raw-output', out / 'raw.cs',
                   '--frontend-invocation', out / 'invocation.json']
        run('translate', command)
        report['status'] = 'translated'
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        try:
            verify_staged(portable)
            if sha(compiler) != report['compiler']['sha256']:
                raise ValueError('EDG codegen compiler changed during translation')
            if sha(preprocessor) != report['preprocessingCompiler']['sha256']:
                raise ValueError('EDG preprocessing compiler changed during translation')
            if fingerprint is not None:
                current, _ = preprocessing_inputs(portable, args.portable_manifest, preprocessor,
                    provider['standard'], includes, preincludes, units, env, args.target, args.preprocessing_epoch)
                if current != fingerprint:
                    raise ValueError('Preprocessing inputs changed during translation')
                verify_dependencies(report.get('preprocessingDependencies', []))
                verify_dependencies(report.get('providerPreprocessingDependencies', []))
                if provider:
                    verify_dependencies(provider['sources'])
                    verify_dependencies(provider['generatedInputs'])
                    verify_dependencies([item for item in provider.get('adaptations', [])
                                         if 'path' in item and 'sha256' in item])
                for entry in report['preprocessing'] + report['providerPreprocessing']:
                    if sha(entry['path']) != entry['sha256']:
                        raise ValueError('Preprocessed output changed during translation: ' + entry['path'])
                report['preprocessingInputsUnchanged'] = True
                if report['preprocessingComplete']:
                    report['preprocessingReceiptSha256'] = preprocessing_receipt_digest(report)
            report['stagedInputsUnchanged'] = True
        except Exception as error:
            report.update(status='failed', provenanceFailure=str(error))
            save()
            raise
        save()


if __name__ == '__main__':
    main()
