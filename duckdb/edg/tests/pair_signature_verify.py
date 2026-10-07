#!/usr/bin/env python3
"""Check the pinned libc++ pair overlay with ten actual DuckDB two-TU cases.

This is a bounded stock-C frontend correspondence check, not C emission,
execution, or a complete DuckDB translation. All generated inputs are retained.
"""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from portable import verify_staged

ROOT = Path(__file__).resolve().parents[3]
EDG = ROOT / 'edgcpp'
ORIGINAL_SHA = '833c64ed6631ba6c0cb2444c57d3499ea74101fb24b6ed0777c1b602e3d89699'
CANDIDATE_SHA = '517ff7c6075b10e0f588d6978d17c6b318708ec69265995ba46d07b20896c71a'
ASSERTION = '''  static_assert(sizeof(static_cast<pair& (pair::*)(
                    __conditional_t<is_copy_assignable<first_type>::value &&
                                        is_copy_assignable<second_type>::value,
                                    pair, __nat> const&)>(&pair::operator=)) > 0,
                "pair copy-assignment signature");

'''
PAIRS = ('idx_t,idx_t', 'double,idx_t', 'string,idx_t', 'string,double',
         'string,bool', 'string,CatalogType', 'LogicalType,Value', 'Value,Value',
         'LogicalType,LogicalType', 'string,string')
MARKER = re.compile(r'^\s*#\s*(?:line\s+)?\d+\s+("(?:[^"\\]|\\.)*")', re.M)


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def record(path):
    return dict(path=str(Path(path).resolve()), sha256=sha(path))


def verify_record(item):
    if sha(item['path']) != item['sha256']:
        raise ValueError('Changed recorded input: ' + item['path'])


def pair_origin(data, expected):
    origins = {str(Path(json.loads(m.group(1))).resolve()) for m in MARKER.finditer(data)
               if json.loads(m.group(1)).endswith('/__utility/pair.h')}
    if origins != {str(expected.resolve())}:
        raise ValueError('Missing, mixed, or unexpected pair header origins: ' + repr(sorted(origins)))
    return sorted(origins)


def acquisition_inputs(acquisition):
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    pin = json.loads((EDG / 'backend/runtime/libcxx/source.json').read_text())
    if manifest['source'] != pin:
        raise ValueError('Acquisition does not match canonical libc++ source pin')
    original = acquisition / pin['archiveRoot'] / 'include/__utility/pair.h'
    candidate = acquisition / 'config/__utility/pair.h'
    if sha(original) != ORIGINAL_SHA or sha(candidate) != CANDIDATE_SHA:
        raise ValueError('Unreviewed original or candidate pair header')
    if candidate.read_text().count(ASSERTION) != 1 or candidate.read_text().replace(ASSERTION, '') != original.read_text():
        raise ValueError('Pair overlay is not exactly the reviewed assertion insertion')
    overlays = json.loads((acquisition / 'config/edg-overlays.json').read_text())
    entries = [e for e in overlays['overlays'] if e['upstream'] == 'include/__utility/pair.h']
    if overlays['release'] != pin['release'] or len(entries) != 1:
        raise ValueError('Pair vendor-overlay entry missing or ambiguous')
    entry = entries[0]
    if (entry['configuration'] != '__utility/pair.h' or entry['upstreamSha256'] != ORIGINAL_SHA
            or entry['sha256'] != CANDIDATE_SHA or sha(acquisition / 'config' / entry['patch']) != entry['patchSha256']):
        raise ValueError('Pair vendor-overlay metadata or patch hash mismatch')
    expected_patch = ''.join(difflib.unified_diff(
        original.read_text().splitlines(True), candidate.read_text().splitlines(True),
        fromfile='a/include/__utility/pair.h', tofile='b/include/__utility/pair.h'))
    if (acquisition / 'config' / entry['patch']).read_text() != expected_patch:
        raise ValueError('Recorded patch is not the exact original-to-candidate unified diff')
    if dict(entry, kind='vendor-header-overlay') not in manifest['patches']:
        raise ValueError('Acquisition did not record the pair overlay')
    actual_files = {str(path.relative_to(acquisition))
                    for parent in (acquisition / pin['archiveRoot'], acquisition / 'config')
                    for path in parent.rglob('*') if path.is_file()}
    if actual_files != set(manifest['fileHashes']):
        raise ValueError('Acquisition source/config file inventory differs from its receipt')
    records = [record(acquisition / 'acquisition.json'), record(EDG / 'backend/runtime/libcxx/source.json')]
    for name, digest in manifest['fileHashes'].items():
        path = (acquisition / name).resolve()
        if not path.is_relative_to(acquisition):
            raise ValueError('Acquisition input escapes its root')
        item = dict(path=str(path), sha256=digest)
        verify_record(item)
        records.append(item)
    for needed in (original, candidate, acquisition / 'config/edg-overlays.json', acquisition / 'config' / entry['patch']):
        if not any(r['path'] == str(needed.resolve()) for r in records):
            raise ValueError('Unrecorded acquisition input: ' + str(needed))
    return pin, original, candidate, entry, records


def main():
    parser = argparse.ArgumentParser(description=__doc__, allow_abbrev=False)
    parser.add_argument('--acquisition', type=Path, required=True)
    parser.add_argument('--composition', type=Path, required=True,
                        help='Verified composition containing the actual read_json.cpp preprocessing contract')
    parser.add_argument('--compiler', type=Path, help='Override preprocessor; default is the composition snapshot')
    parser.add_argument('--oracle', type=Path, default=EDG / 'build/bin/cpfe')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--timeout', type=int, default=180, help='Seconds per preprocessing/semantic command')
    args = parser.parse_args()
    if args.timeout <= 0:
        parser.error('--timeout must be positive')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    report = dict(schema=1, status='running', passed=False, cases=[], commands=[],
                  executionVerified=False, claim='Ten concrete DuckDB pair specializations: stock-C two-TU semantic checks only')

    def save():
        path = out / 'results.json.tmp'
        path.write_text(json.dumps(report, indent=2) + '\n')
        path.replace(out / 'results.json')

    save()
    try:
        acquisition = args.acquisition.resolve()
        pin, original, candidate, overlay, inputs = acquisition_inputs(acquisition)
        composition = json.loads(args.composition.read_text())
        if composition.get('status') != 'preprocessed-composition' or not composition.get('frozenInputsUnchanged'):
            raise ValueError('Composition is not a completed frozen preprocessing receipt')
        portable_record = composition['portableManifest']
        verify_record(portable_record)
        portable = json.loads(Path(portable_record['path']).read_text())
        verify_staged(portable)
        selected = [u for u in composition['preprocessingContract']['selected']
                    if u['source'].endswith('/json_functions/read_json.cpp')]
        if len(selected) != 1 or composition['standard'] != 'c++17':
            raise ValueError('Expected one C++17 read_json.cpp contract')
        selected = selected[0]
        verify_record(dict(path=selected['source'], sha256=selected['sourceSha256']))
        argv = selected['arguments']
        if argv[-1] != selected['source'] or '--preprocess' not in argv or '--c++17' not in argv:
            raise ValueError('Unexpected preprocessing contract')
        old_roots = {Path(argv[i + 1]).parent.parent for i, value in enumerate(argv[:-1])
                     if value == '-I' and argv[i + 1].endswith('/' + pin['archiveRoot'] + '/include')}
        if len(old_roots) != 1:
            raise ValueError('Missing or mixed libc++ acquisition include roots')
        old_root = old_roots.pop()
        flags = [str(acquisition / Path(value).relative_to(old_root))
                 if Path(value).is_absolute() and Path(value).is_relative_to(old_root) else value
                 for value in argv[:-1]]
        include_order = [flags[i + 1] for i, value in enumerate(flags[:-1]) if value == '-I']
        if include_order[:3] != [str(acquisition / name) for name in
                                ('config', pin['archiveRoot'] + '/include', pin['archiveRoot'] + '/src')]:
            raise ValueError('Acquisition configuration include missing')
        compiler_source = args.compiler.resolve() if args.compiler else Path(composition['preprocessingCompiler']['path'])
        if not args.compiler:
            verify_record(composition['preprocessingCompiler'])
        tools = out / 'tools'
        tools.mkdir()
        compiler, oracle = tools / 'cpfe-dot', tools / 'cpfe'
        for source, target in ((compiler_source, compiler), (args.oracle.resolve(), oracle)):
            before = record(source)
            shutil.copy2(source, target)
            verify_record(before)
            if sha(target) != before['sha256']:
                raise ValueError('Compiler snapshot differs')
            inputs.append(record(target))
        inputs.extend([record(args.composition), portable_record, record(__file__),
                       record(Path(__file__).parents[1] / 'portable.py')])
        original_overlay = out / 'original-include/__utility/pair.h'
        original_overlay.parent.mkdir(parents=True)
        shutil.copy2(original, original_overlay)
        inputs.append(record(original_overlay))
        # Reject a mixed old/new pair definition before handing two TUs to EDG.
        mixed = '# 1 ' + json.dumps(str(original_overlay)) + '\n# 1 ' + json.dumps(str(candidate)) + '\n'
        try:
            pair_origin(mixed, candidate)
        except ValueError:
            report['mixedHeaderProvenanceRejected'] = True
        else:
            raise ValueError('Mixed-header provenance control unexpectedly accepted')
        env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'),
                   EDG_DOT_TARGET='portable-lp64-binary80-v2')
        report.update(composition=record(args.composition), acquisition=record(acquisition / 'acquisition.json'),
                      original=record(original), candidate=record(candidate), overlay=overlay,
                      compiler=record(compiler), oracle=record(oracle), selectedContract=selected,
                      effectiveFlags=flags, environment={k: env.get(k) for k in
                          ('EDG_BASE', 'EDG_DOT_TARGET', 'CPATH', 'CPLUS_INCLUDE_PATH', 'C_INCLUDE_PATH')})
        # Pin existing include/preinclude inputs before reading any headers.
        known = {item['path']: item['sha256'] for item in inputs}
        for i, value in enumerate(flags[:-1]):
            if value in ('-I', '--preinclude'):
                path = Path(flags[i + 1])
                paths = path.rglob('*') if path.is_dir() else [path]
                for entry in paths:
                    if entry.is_file():
                        known.setdefault(str(entry.resolve()), sha(entry))
        dependencies = {}

        def run(label, command, output=None):
            stdout = output or out / (label + '.stdout')
            stderr = out / (label + '.stderr')
            item = dict(stage=label, command=list(map(str, command)), cwd=str(out))
            report['commands'].append(item)
            start = time.monotonic()
            try:
                with stdout.open('wb') as so, stderr.open('wb') as se:
                    result = subprocess.run(item['command'], cwd=out, env=env, stdout=so, stderr=se, timeout=args.timeout)
                item['exitCode'] = result.returncode
            except subprocess.TimeoutExpired:
                item['timeout'] = True
                raise
            finally:
                item.update(seconds=round(time.monotonic() - start, 3), stdout=record(stdout), stderr=record(stderr))
                save()
            return result.returncode, ' '.join(stderr.read_text(errors='replace').split())

        common = ('#include "duckdb/common/types/value.hpp"\n#include "duckdb/common/pair.hpp"\n'
                  '#include "duckdb/common/enums/catalog_type.hpp"\nusing namespace duckdb;\n')
        for index, pair in enumerate(PAIRS):
            row = dict(pair=pair, modes={})
            report['cases'].append(row)
            for mode in ('original', 'candidate'):
                paths = []
                origins = []
                for name, body in (('copy', 'void first(P*a,const P*b){*a=*b;}'),
                                   ('size', 'unsigned long second(){return sizeof(P);}')):
                    label = f'{index:02d}-{mode}-{name}'
                    source = out / (label + '.cpp')
                    source.write_text(common + 'using P=std::pair<' + pair + '>;\n' + body + '\n')
                    known[str(source)] = sha(source)
                    output = source.with_suffix('.ii')
                    prefix = ['-I', str(original_overlay.parents[1])] if mode == 'original' else []
                    code, _ = run(label + '-preprocess', [compiler, *prefix, *flags, source], output)
                    if code:
                        raise ValueError('Preprocessing failed: ' + label)
                    data = output.read_text()
                    origins.append(pair_origin(data, original_overlay if mode == 'original' else candidate))
                    for spelling in {match.group(1) for match in MARKER.finditer(data)}:
                        path = Path(json.loads(spelling))
                        if path.is_absolute() and path.is_file():
                            path = path.resolve()
                            digest = sha(path) if str(path) not in dependencies else dependencies[str(path)]
                            if known.get(str(path)) != digest:
                                raise ValueError('Untracked or changed preprocessing input: ' + str(path))
                            dependencies[str(path)] = digest
                    paths.append(output)
                label = f'{index:02d}-{mode}-semantic'
                code, diagnostics = run(label, [oracle, '--c++17', '--gnu_version', '130300',
                    '--instantiate', 'used', '--clear_flag', 'use_predefined_macro_file',
                    '--no_code_gen', '--no_warnings', *paths])
                expected_failure = mode == 'original' and index >= 2
                diagnostic_class = 'incompatible with a declaration in another translation unit' in diagnostics
                if (expected_failure and (code <= 0 or not diagnostic_class)) or (not expected_failure and code != 0):
                    raise ValueError('Unexpected stock-C correspondence result: ' + label)
                row['modes'][mode] = dict(exitCode=code, expectedCorrespondenceFailure=expected_failure,
                    correspondenceDiagnostic=diagnostic_class, inputs=[record(path) for path in paths], pairHeaderOrigins=origins)
                save()
        acquisition_inputs(acquisition)
        verify_staged(portable)
        report['inputs'] = inputs
        report['preprocessingDependencies'] = [dict(path=path, sha256=digest) for path, digest in sorted(dependencies.items())]
        for item in inputs + report['preprocessingDependencies']:
            verify_record(item)
        report.update(status='passed', passed=True, inputsUnchanged=True,
                      originalPassed=2, originalExpectedCorrespondenceFailures=8, candidatePassed=10)
        save()
    except Exception as error:
        report.update(status='failed', error=type(error).__name__ + ': ' + str(error))
        save()
        raise
    print('Pair signature: original 2 passes / 8 expected correspondence failures; candidate 10 passes')


if __name__ == '__main__':
    main()
