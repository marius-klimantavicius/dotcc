#!/usr/bin/env python3
"""Stage the pinned upstream core amalgamation plus every remaining configured TU.

The optional preprocessing step combines the real libc++ provider and the core.
This recipe records program inputs; it never launches a whole-program merge.
"""
import argparse
from collections import Counter, defaultdict
import difflib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
import types

# In particular, importing the upstream generator must not add __pycache__ to
# the frozen source. Set this before importing our own recipe modules as well.
sys.dont_write_bytecode = True
from libcxx import prepare_provider
from portable import verify_staged
from preflight import EDG, frontend_flags, sha
from translate import (dependency_receipt, included_files, preprocessing_clock,
                       preprocessing_command, preprocessing_inputs,
                       preprocessing_receipt_digest, verify_dependencies)

EXPECTED_EXTENSIONS = ['core_functions', 'parquet', 'icu', 'json', 'autocomplete', 'tpcds', 'tpch']
IMPLEMENTATION_SUFFIXES = {'.c', '.cc', '.cpp', '.cxx'}
LINE = re.compile(r'^#line (\d+) "([^"\n]+)"', re.M)


def record(path):
    path = Path(path).resolve()
    return dict(path=str(path), sha256=sha(path))


def assemble(source, out, describe):
    """Invoke pinned generator functions, retaining its unmodified raw output."""
    generator_path = source / 'scripts/amalgamation.py'
    original = generator_path.read_text()
    replacements = {
        "(linenr, current_file)": "(linenr, os.path.abspath(current_file))",
        "(current_file,)": "(os.path.abspath(current_file),)",
    }
    start, end = original.index('def write_file('), original.index('\ndef write_dir(')
    function = original[start:end]
    for before, after in replacements.items():
        if function.count(before) != 1:
            raise ValueError('Pinned generator line-directive expression changed: ' + before)
        function = function.replace(before, after)
    normalized = original[:start] + function + original[end:]
    overlay = out / 'amalgamation-absolute-lines.py'
    overlay.write_text(normalized)
    (out / 'generator-lines.patch').write_text(''.join(difflib.unified_diff(
        original.splitlines(keepends=True), normalized.splitlines(keepends=True),
        fromfile='upstream/scripts/amalgamation.py', tofile='amalgamation-absolute-lines.py')))
    previous_dir, previous_path = Path.cwd(), list(sys.path)
    old_environment = {key: os.environ.get(key) for key in ('OVERRIDE_GIT_DESCRIBE', 'MAIN_BRANCH_VERSIONING')}
    os.environ.update(OVERRIDE_GIT_DESCRIBE=describe, MAIN_BRANCH_VERSIONING='0')
    sys.path.insert(0, str(source / 'scripts'))
    os.chdir(source)
    assemblies = []
    try:
        for label, script, destination in [('raw', original, out / 'upstream-raw'),
                                            ('absolute-lines', normalized, out)]:
            destination.mkdir(exist_ok=True)
            module = types.ModuleType('duckdb_amalgamation_' + label.replace('-', '_'))
            module.__file__ = str(generator_path if label == 'raw' else overlay)
            exec(compile(script, module.__file__, 'exec'), module.__dict__)
            module.linenumbers = True
            if module.compile_directories.count('extension/loader') != 1:
                raise ValueError('Upstream loader assembly directory changed')
            module.compile_directories.remove('extension/loader')
            module.temp_header = str(destination / 'duckdb.hpp')
            module.generate_duckdb_hpp(str(destination / 'duckdb.hpp'))
            with (destination / 'duckdb.cpp').open('w') as stream:
                stream.write('#include "duckdb.hpp"\n#ifndef DUCKDB_AMALGAMATION\n#error header mismatch\n#endif\n'
                             '#if (!defined(DEBUG) && !defined NDEBUG)\n#define NDEBUG\n#endif\n')
                for directory in module.compile_directories:
                    stream.write(module.write_dir(directory))
                stream.write('\n/*\n')
                for index, license_path in enumerate(module.licenses):
                    stream.write('\n### THIRD PARTY LICENSE #' + str(index + 1) + '###\n')
                    stream.write(module.write_file(license_path))
                stream.write('\n*/\n')
            assemblies.append(dict(kind=label, files=[record(destination / name) for name in ('duckdb.cpp', 'duckdb.hpp')],
                sourceInputs=[record(source / name) for name in module.written_files],
                compileDirectories=module.compile_directories, includeDirectories=module.include_paths,
                upstreamAlwaysExcluded=module.always_excluded, upstreamExcludedBasenames=module.excluded_files))
        if assemblies[0]['sourceInputs'] != assemblies[1]['sourceInputs']:
            raise ValueError('Line-directive overlay changed the upstream input closure')
    finally:
        os.chdir(previous_dir)
        sys.path[:] = previous_path
        for key, value in old_environment.items():
            if value is None:
                os.environ.pop(key, None)
            else:
                os.environ[key] = value
    return dict(variants=assemblies,
        generatorInputs=[record(source / 'scripts' / name) for name in ('amalgamation.py', 'package_build.py', 'python_helpers.py')],
        generatorOverlay=record(overlay), generatorPatch=record(out / 'generator-lines.patch'),
        environment=dict(OVERRIDE_GIT_DESCRIBE=describe, MAIN_BRANCH_VERSIONING='0'),
        linePolicy='Only the upstream generator\'s two newly emitted #line filename expressions become absolute; existing source #line directives and source bodies remain untouched.')


def line_inventory(path, source):
    mappings = []
    for number, line in enumerate(Path(path).read_text().splitlines(), 1):
        match = LINE.match(line)
        if not match:
            continue
        name = match.group(2)
        target = Path(name) if Path(name).is_absolute() else source / name
        mappings.append(dict(generatedLine=number, sourceLine=int(match.group(1)), spelling=name,
                             sourcePath=str(target), sourceExists=target.is_file()))
    return mappings


def closure(portable, assembly, source, source_id, version):
    units = [unit for unit in portable['units'] if unit['role'] == 'library']
    included = {item['path'] for item in assembly['variants'][0]['sourceInputs']}
    covered = [i for i, unit in enumerate(units) if unit['file'] in included]
    remaining = [i for i, unit in enumerate(units) if unit['file'] not in included]
    if (len(units), len(covered), len(remaining)) != (2207, 1476, 731):
        raise ValueError('Pinned configured closure changed: ' + str((len(units), len(covered), len(remaining))))
    if portable['staticExtensions'] != EXPECTED_EXTENSIONS:
        raise ValueError('Configured bundled static extensions changed')
    selected = {unit['file'] for unit in units}
    if len(selected) != len(units):
        raise ValueError('Repeated source files require explicit handling of their distinct native TU profiles')
    extras = sorted(name for name in included if Path(name).suffix in IMPLEMENTATION_SUFFIXES and name not in selected)
    classifications = {
        str(source / 'src/common/allocator/allocator_jemalloc.cpp'):
            'Disabled by DUCKDB_ENABLE_JEMALLOC; configured allocation uses the managed runtime allocator.',
        str(source / 'third_party/utf8proc/utf8proc_data.cpp'):
            'Textual input of selected third_party/utf8proc/utf8proc.cpp, not an additional standalone TU.',
    }
    if set(extras) != set(classifications):
        raise ValueError('Unclassified extra implementation inputs: ' + repr(extras))
    if any('DUCKDB_ENABLE_JEMALLOC' in define for unit in units for define in unit['defines']):
        raise ValueError('This pinned assembly requires the explicitly selected non-jemalloc allocator')
    template = next(unit for unit in units if unit['file'] == str(source / 'src/common/types/hugeint.cpp'))
    base_defines = set(template['defines']) | {'RE2_ON_VALGRIND'}
    grouped = defaultdict(list)
    for index in covered:
        grouped[tuple(sorted(set(units[index]['defines'])))].append(index)
    profiles = []
    for native_defines, indices in sorted(grouped.items()):
        profiles.append(dict(nativeDefines=list(native_defines), sourceIndices=indices,
            targets=sorted({units[i]['target'] for i in indices}),
            addedCommandDefines=sorted(base_defines - set(native_defines)),
            nativeDefinesNotOnCombinedCommand=sorted(set(native_defines) - base_defines)))
    version_unit = next(unit for unit in units if unit['file'].endswith('/src/function/table/version/pragma_version.cpp'))
    for name, expected in [('DUCKDB_SOURCE_ID', source_id), ('DUCKDB_VERSION', version)]:
        definitions = [value.split('=', 1)[1].strip('"') for value in version_unit['defines'] if value.startswith(name + '=')]
        if definitions != [expected]:
            raise ValueError('Native ' + name + ' does not match pinned amalgamation metadata')
    remaining_targets = Counter(units[i]['target'] for i in remaining)
    if not any('/codegen/' in units[i]['file'] and 'extension_loader' in units[i]['file'] for i in remaining):
        raise ValueError('Real generated static extension loader is missing from remaining TUs')
    return units, template, dict(configuredLibraryTUs=len(units), coveredLibraryTUs=len(covered),
        remainingLibraryTUs=len(remaining), coveredSourceIndices=covered, remainingSourceIndices=remaining,
        configuredOmissions=[], includedNonStandaloneImplementationInputs=[dict(record(path), classification=classifications[path]) for path in extras],
        remainingTargetCounts=dict(sorted(remaining_targets.items())), nativeMacroProfiles=profiles,
        combinedCommandDefines=sorted(base_defines),
        macroDeviations=[
            dict(macros=['DUCKDB', 'DUCKDB_MAIN_LIBRARY'], policy='Core build macros become visible to bundled dependency implementations in the upstream amalgamation; native per-TU differences are listed above, not claimed equivalent by assembly alone.'),
            dict(macros=['RE2_ON_VALGRIND'], policy='Explicitly enabled for the combined core so RE2 preserves the configured native initialization branch; visible to the whole combined TU.'),
            dict(macros=['DUCKDB_SOURCE_ID', 'DUCKDB_VERSION'], policy='Generated upstream header defines the exact native ten-character source ID and release globally.'),
            dict(macros=['DISABLE_CORE_FUNCTIONS_EXTENSION'], policy='Native catalog-only definition has no C/C++ use in pinned core input; extension implementations remain selected separately.'),
            dict(macros=['DUCKDB_EXTENSION_DIRECTORIES'], policy='Native value ~/.duckdb/extensions matches the pinned Unix default in extension_install.cpp; portable profile does not define _WIN32.'),
            dict(macros=['DUCKDB_AMALGAMATION'], policy='Defined by the upstream generated header; its supported amalgamation branches remain active.'),
        ],
        replacedUnconfiguredLoader=dict(path=str(source / 'extension/loader'), reason='Upstream dummy loader excluded; real frozen CMake-generated seven-extension loader stays in remaining units.'),
        policy='Core plus all remaining configured TUs; no extension sources are supplied merely by --extended, which expands public headers only.')


def stored_inputs(receipt_path, current, units, remaining):
    """Consume previously built .ii artifacts; do not assert a fresh cache hit."""
    previous = json.loads(receipt_path.read_text())
    if previous.get('preprocessingReceiptSha256') != preprocessing_receipt_digest(previous):
        raise ValueError('Retained preprocessing receipt integrity mismatch')
    if not previous.get('preprocessingComplete') or not previous.get('preprocessingInputsUnchanged') or previous.get('preprocessingVolatileInputs'):
        raise ValueError('Retained preprocessing receipt is incomplete or time-dependent')
    old = previous['preprocessingInputs']
    if {k: v for k, v in old.items() if k != 'headerInventory'} != {k: v for k, v in current.items() if k != 'headerInventory'}:
        raise ValueError('Retained inputs use different source, flags, standard, compiler, clock or preprocessing configuration')
    old_inventory = {item['path']: item for item in old['headerInventory']}
    new_inventory = {item['path']: item for item in current['headerInventory']}
    differences = [dict(path=path, previous=old_inventory.get(path), current=new_inventory.get(path))
                   for path in sorted(old_inventory.keys() | new_inventory.keys()) if old_inventory.get(path) != new_inventory.get(path)]
    if any('sha256' in (item['previous'] or {}) or 'sha256' in (item['current'] or {}) or
           (item['previous'] or {}).get('kind') == 'directory-link' or
           (item['current'] or {}).get('kind') == 'directory-link' for item in differences):
        raise ValueError('Public header/configuration inventory differs from retained input contract')
    verify_dependencies(previous['preprocessingDependencies'])
    preprocessor = previous.get('preprocessingCompiler', previous['compiler'])
    verify_dependencies([preprocessor, previous['portableManifest']])
    if len(previous['preprocessing']) != len(units):
        raise ValueError('Retained receipt does not describe the entire configured source closure')
    chosen = []
    for index, (unit, entry, selected) in enumerate(zip(units, previous['preprocessing'], current['selected'])):
        if (entry['index'] != index or entry['source'] != unit['file'] or not entry['passed'] or entry['exitCode'] != 0 or
            entry['sourceSha256'] != selected['sourceSha256'] or entry['command'][1:] != selected['arguments']):
            raise ValueError('Retained source order, identity or preprocessing command mismatch')
        if index in remaining:
            verify_dependencies([entry])
            chosen.append(dict(entry, sourceIndex=index, target=unit['target']))
    return chosen, dict(receipt=record(receipt_path), priorPreprocessingCompiler=preprocessor,
        selectedArtifacts=len(chosen), freshPreprocessingCacheHit=False,
        inventoryDifferences=differences,
        policy='Already-built .ii program inputs, verified against their own complete receipt and hashes. Inventory differences are recorded explicitly; this is not the stricter translate.py fresh-cache eligibility claim.')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--portable-manifest', required=True, type=Path)
    parser.add_argument('--preprocessed-receipt', required=True, type=Path)
    parser.add_argument('--acquisition', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--preprocessing-epoch', required=True, type=int)
    parser.add_argument('--preprocess', action='store_true')
    parser.add_argument('--timeout', type=int, default=600)
    parser.add_argument('--unity-inventory', type=Path, help='optional recorded upstream unity fallback investigation; never selected automatically')
    args = parser.parse_args()
    if not 0 <= args.preprocessing_epoch <= 253402300799 or args.timeout <= 0:
        parser.error('epoch must be between 0 and 253402300799 and timeout positive')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    portable = json.loads(args.portable_manifest.read_text())
    source = Path(portable['stagedSource']).resolve()
    receipt_path = args.preprocessed_receipt.resolve()
    cache = json.loads(receipt_path.read_text())
    report = dict(schema=1, status='assembling', executionVerified=False,
                  portableManifest=record(args.portable_manifest), retainedInputsReceipt=record(receipt_path),
                  preprocessingClock=preprocessing_clock(args.preprocessing_epoch))
    def save():
        temporary = out / 'composition.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(out / 'composition.json')
    save()
    try:
        verify_staged(portable)
        acquisition_receipt = Path(portable['source']).parent / 'source-manifest.json'
        if sha(acquisition_receipt) != portable['sourceManifestSha256']:
            raise ValueError('Pinned DuckDB acquisition receipt changed')
        pin = json.loads(acquisition_receipt.read_text())['pin']
        configured_pin = json.loads(Path(__file__).with_name('config').joinpath('source.json').read_text())
        if pin != configured_pin:
            raise ValueError('DuckDB source pin does not match this recipe')
        source_id = pin['commit'][:10]
        report.update(pin=pin, sourceId=source_id, standard='c++17', staticExtensions=portable['staticExtensions'])
        assembly = assemble(source, out, pin['tag'] + '-0-g' + source_id)
        report['assembly'] = assembly
        units, template, report['closure'] = closure(portable, assembly, source, source_id, pin['tag'])
        report['lineMappings'] = {name: line_inventory(out / name, source) for name in ('duckdb.cpp', 'duckdb.hpp')}
        if args.unity_inventory:
            report['unselectedUnityFallback'] = dict(record(args.unity_inventory),
                policy='Investigative upstream per-target unity groups only; no change to the selected731-TU composition.')
        provider = prepare_provider(EDG, args.acquisition, out / 'provider', standard='c++17', unity_provider=True,
                                    primary_consumers=[out / 'duckdb.cpp'])
        report['provider'] = provider
        compiler_record = cache.get('preprocessingCompiler', cache['compiler'])
        verify_dependencies([compiler_record])
        compiler = out / 'cpfe-dot-preprocess'
        shutil.copy2(compiler_record['path'], compiler)
        report['preprocessingCompiler'] = dict(record(compiler), originalPath=compiler_record['path'])
        includes = [*map(Path, provider['includeDirectories']), EDG / 'backend/runtime/libcxx/include',
                    EDG / 'backend/runtime/posix/include', EDG / 'backend/runtime/libc/include']
        preincludes = list(map(Path, provider['preincludeHeaders']))
        env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'), EDG_DOT_TARGET='portable-lp64-binary80-v2')
        fingerprint, initial_hashes = preprocessing_inputs(portable, args.portable_manifest, compiler, 'c++17',
            includes, preincludes, units, env, 'library', args.preprocessing_epoch)
        report['currentPreprocessingContract'] = fingerprint
        retained, report['retainedInputProvenance'] = stored_inputs(receipt_path, fingerprint, units,
            set(report['closure']['remainingSourceIndices']))
        report['remainingInputs'] = retained
        _, ignored = frontend_flags(template)
        combined_unit = dict(template, file=provider['translationUnits'][0], directory=str(out),
            arguments=[*template['arguments'], '-DRE2_ON_VALGRIND'])
        command, _ = preprocessing_command(compiler, 'c++17', includes, preincludes, combined_unit, args.preprocessing_epoch)
        report['combinedCorePreprocessing'] = dict(command=command, nativeTemplateSource=template['file'],
            nativeFlagsNotForwarded=ignored)
        report['status'] = 'assembled'
        save()
        if args.preprocess:
            output = out / 'combined-core.ii'
            started = time.monotonic()
            with output.open('wb') as stdout, (out / 'preprocess.stderr').open('wb') as stderr:
                result = subprocess.run(command, cwd=out, env=env, stdout=stdout, stderr=stderr, timeout=args.timeout)
            report['combinedCorePreprocessing'].update(exitCode=result.returncode, output=record(output),
                                                       elapsedSeconds=round(time.monotonic() - started, 3))
            if result.returncode:
                raise RuntimeError('Combined core/provider preprocessing failed')
            records = [*provider['sources'], *provider['generatedInputs'],
                       *[item for item in provider['adaptations'] if 'path' in item and 'sha256' in item],
                       *[item for variant in assembly['variants'] for item in variant['files']]]
            initial_hashes.update({item['path']: item['sha256'] for item in records})
            report['combinedCoreDependencies'], volatile = dependency_receipt(included_files(output), initial_hashes, args.preprocessing_epoch)
            if volatile:
                raise ValueError('Combined core contains unfixed time-dependent or embedded preprocessing inputs')
            report['programInputs'] = [dict(kind='core-and-libcxx', **record(output),
                coveredSourceIndices=report['closure']['coveredSourceIndices']),
                *[dict(kind='retained-configured-tu', sourceIndex=item['sourceIndex'], source=item['source'],
                       path=item['path'], sha256=item['sha256']) for item in retained]]
            report['status'] = 'preprocessed-composition'
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        try:
            verify_staged(portable)
            verify_dependencies([report['portableManifest'], report['retainedInputsReceipt']])
            if 'assembly' in report:
                for variant in report['assembly']['variants']:
                    verify_dependencies(variant['sourceInputs'] + variant['files'])
                verify_dependencies(report['assembly']['generatorInputs'])
                verify_dependencies([report['assembly']['generatorOverlay'], report['assembly']['generatorPatch']])
            if 'currentPreprocessingContract' in report:
                verify_dependencies([item for item in report['currentPreprocessingContract']['headerInventory'] if 'sha256' in item])
            verify_dependencies(report.get('combinedCoreDependencies', []))
            verify_dependencies(report.get('remainingInputs', []))
            if 'preprocessingCompiler' in report:
                verify_dependencies([report['preprocessingCompiler']])
            if 'provider' in report:
                p = report['provider']
                verify_dependencies([*p['sources'], *p['generatedInputs'],
                                     *[item for item in p['adaptations'] if 'path' in item and 'sha256' in item]])
            if 'combinedCorePreprocessing' in report and 'output' in report['combinedCorePreprocessing']:
                verify_dependencies([report['combinedCorePreprocessing']['output']])
            report['frozenInputsUnchanged'] = True
        except Exception as error:
            report.update(status='failed', provenanceFailure=str(error))
            save()
            raise
        save()


if __name__ == '__main__':
    main()
