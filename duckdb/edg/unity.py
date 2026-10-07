#!/usr/bin/env python3
"""Compose exact upstream CMake unity groups with an existing core composition.

Only complete groups inside the remaining source set are selected. Mixed groups
stay separate. This tool never launches a whole-program merge or claims runtime
verification. Use --preprocess to produce programInputs, otherwise inspect the
inventory-only receipt and generated wrappers/commands.
"""
import argparse
from collections import Counter
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time

sys.dont_write_bytecode = True
from portable import inventory, verify_staged
from preflight import EDG, frontend_flags, sha
from translate import (dependency_receipt, included_files, preprocessing_clock,
                       preprocessing_command, preprocessing_inputs, verify_dependencies)


def record(path):
    path = Path(path).resolve()
    return dict(path=str(path), sha256=sha(path))


def unique(items, key, label):
    result = {}
    for item in items:
        identity = item[key]
        if identity in result:
            raise ValueError('Duplicate ' + label + ': ' + str(identity))
        result[identity] = item
    return result


def coverage(inputs, count):
    indices = []
    for item in inputs:
        indices.extend(item['coveredSourceIndices'] if 'coveredSourceIndices' in item else [item['sourceIndex']])
    if any(type(index) is not int for index in indices) or Counter(indices) != Counter(range(count)):
        raise ValueError('Configured source coverage has omissions, duplicates or invalid indices')


def validate_base(composition, portable):
    if composition.get('status') != 'preprocessed-composition' or not composition.get('frozenInputsUnchanged'):
        raise ValueError('Base composition is not complete and frozen')
    units = [unit for unit in portable['units'] if unit['role'] == 'library']
    by_source = unique(units, 'file', 'configured source')
    remaining = unique(composition['remainingInputs'], 'source', 'remaining source')
    program = composition['programInputs']
    if not program or program[0].get('kind') != 'core-and-libcxx':
        raise ValueError('Base composition must start with its core/provider input')
    coverage(program, len(units))
    rest = unique(program[1:], 'source', 'program source')
    if set(rest) != set(remaining):
        raise ValueError('Remaining input set differs from program inputs')
    selected = composition['currentPreprocessingContract']['selected']
    if len(selected) != len(units):
        raise ValueError('Preprocessing contract omits configured sources')
    for index, (unit, contract) in enumerate(zip(units, selected)):
        if contract['source'] != unit['file'] or contract['index'] != index or contract['sourceSha256'] != sha(unit['file']):
            raise ValueError('Configured source order or checksum differs from preprocessing contract')
    for source, entry in remaining.items():
        if source not in by_source:
            raise ValueError('Unconfigured remaining source: ' + source)
        index = entry['sourceIndex']
        if type(index) is not int or not 0 <= index < len(units) or units[index]['file'] != source:
            raise ValueError('Remaining source index mismatch: ' + source)
        for key in ('path', 'sha256', 'sourceIndex'):
            if rest[source][key] != entry[key]:
                raise ValueError('Remaining program artifact mismatch: ' + source)
    return units, remaining


def verify_source_mapping(portable, native):
    """Accept only the acquired root attested by the portable source receipt."""
    source = Path(portable['source']).resolve()
    if Path(native['source']).resolve() != source:
        raise ValueError('Native unity source root differs from acquired portable source root')
    manifest = source.parent / 'source-manifest.json'
    if sha(manifest) != portable['sourceManifestSha256']:
        raise ValueError('Portable acquisition receipt changed')
    acquisition = json.loads(manifest.read_text())
    expected = sorted([*acquisition['files'], *acquisition.get('symlinks', [])], key=lambda item: item['path'])
    if inventory(source) != expected:
        raise ValueError('Acquired native source inventory changed')
    if native['staticExtensions'] != portable['staticExtensions']:
        raise ValueError('Native unity static extensions differ from portable configuration')
    native_codegen = Path(native['build']).resolve() / 'codegen'
    staged_codegen = Path(portable['stagedSource']).resolve().parent / 'codegen'
    if inventory(native_codegen) != inventory(staged_codegen):
        raise ValueError('Native unity generated code differs from staged codegen inputs')
    return record(manifest)


def upstream_members(path):
    """Do not silently ignore new generated statements, guards or directives."""
    members = []
    for line in path.read_text().splitlines():
        if not line.strip() or line.strip() == '// Unity Build generated by CMake':
            continue
        match = re.fullmatch(r'#include "([^"\n]+)"', line)
        if not match:
            raise ValueError('Unexpected upstream unity wrapper syntax: ' + str(path))
        members.append(str((path.parent / match[1]).resolve()))
    if not members or len(members) != len(set(members)):
        raise ValueError('Empty or duplicate upstream unity members: ' + str(path))
    return members


def select_groups(native, portable, remaining):
    source = Path(native['source']).resolve()
    build = Path(native['build']).resolve()
    staged = Path(portable['stagedSource']).resolve()
    def relocate(value):
        return value.replace(str(build / 'codegen'), str(staged.parent / 'codegen')).replace(str(source), str(staged))
    original = unique([u for u in portable['units'] if u['role'] == 'library'], 'file', 'configured source')
    groups, mixed, seen, wrappers = [], [], set(), []
    for unit in native['units']:
        path = Path(unit['file']).resolve()
        if unit['role'] != 'library' or not path.name.startswith('ub_'):
            continue
        if not path.is_relative_to(build):
            raise ValueError('Generated unity wrapper lies outside native build: ' + str(path))
        members = [relocate(value) for value in upstream_members(path)]
        wrappers.append(record(path))
        if any(member not in original for member in members):
            raise ValueError('Unity group contains an unconfigured source: ' + str(path))
        if seen.intersection(members):
            raise ValueError('Duplicate source across upstream unity groups: ' + str(path))
        seen.update(members)
        selected = set(members).intersection(remaining)
        if not selected:
            continue
        flags, ignored = frontend_flags(unit)
        flags = [relocate(flag) for flag in flags]
        for member in members:
            if original[member]['target'] != unit['target'] or original[member]['language'] != unit['language']:
                raise ValueError('Unity member target/language mismatch: ' + member)
            if frontend_flags(original[member])[0] != flags:
                raise ValueError('Unity member frontend flags/include order mismatch: ' + member)
        if selected != set(members):
            mixed.append(dict(target=unit['target'], unitySource=record(path),
                remainingMembers=sorted(selected), alreadyRepresentedMembers=sorted(set(members) - selected),
                policy='Reject partial grouping; retain every remaining member as its original separate TU.'))
            continue
        groups.append(dict(target=unit['target'], unitySource=record(path), orderedMembers=members,
                           frontendFlags=flags, nativeFlagsNotForwarded=ignored, nativeCompileCommand=unit))
    grouped = {member for group in groups for member in group['orderedMembers']}
    return dict(groups=groups, excludedMixedGroups=mixed, inspectedUnitySources=wrappers,
                groupCount=len(groups), representedRemainingTUs=len(grouped),
                ungroupedRemainingTUs=sorted(set(remaining) - grouped))


def compose_inputs(base, groups, outputs, count):
    grouped = {}
    for index, group in enumerate(groups):
        for source in group['orderedMembers']:
            if source in grouped:
                raise ValueError('Repeated selected unity member: ' + source)
            grouped[source] = index
    inputs, seen = [base[0]], set()
    remaining = {item['source'] for item in base[1:]}
    if not set(grouped).issubset(remaining):
        raise ValueError('Selected unity group overlaps core or unconfigured sources')
    for old in base[1:]:
        index = grouped.get(old['source'])
        if index is None:
            inputs.append(old)
        elif index not in seen:
            seen.add(index)
            group = groups[index]
            inputs.append(dict(kind='upstream-unity-group', groupIndex=index, target=group['target'],
                               coveredSourceIndices=group['coveredSourceIndices'], **outputs[index]))
    coverage(inputs, count)
    return inputs


def verify_contract(previous, current):
    # Non-header implementation additions cannot affect include lookup of a
    # known header; preserve the same explicit exception used by amalgamate.py.
    if {k: v for k, v in previous.items() if k != 'headerInventory'} != {
            k: v for k, v in current.items() if k != 'headerInventory'}:
        raise ValueError('Preprocessing source/flags/compiler/clock/environment contract changed')
    before = unique(previous['headerInventory'], 'path', 'previous header inventory path')
    after = unique(current['headerInventory'], 'path', 'current header inventory path')
    changes = [dict(path=path, previous=before.get(path), current=after.get(path))
               for path in sorted(before.keys() | after.keys()) if before.get(path) != after.get(path)]
    for change in changes:
        for entry in (change['previous'], change['current']):
            if entry and ('sha256' in entry or entry.get('kind') == 'directory-link'):
                raise ValueError('Preprocessing header/search inventory changed: ' + change['path'])
    return changes


def prepare(composition_path, unity_path, output, preprocess=False, timeout=600):
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = dict(schema=1, status='preparing', executionVerified=False,
                  baseComposition=record(composition_path), nativeUnityManifest=record(unity_path),
                  recipe=record(__file__),
                  recipeModules=[record(Path(__file__).with_name(name)) for name in
                                 ('portable.py', 'preflight.py', 'translate.py')],
                  preprocessing=[], programInputs=[])
    started = time.monotonic()
    def save():
        temporary = output / 'composition.json.tmp'
        temporary.write_text(json.dumps(report, indent=2) + '\n')
        temporary.replace(output / 'composition.json')
    save()
    try:
        composition = json.loads(Path(composition_path).read_text())
        native = json.loads(Path(unity_path).read_text())
        verify_dependencies([composition['portableManifest']])
        portable_path = Path(composition['portableManifest']['path'])
        portable = json.loads(portable_path.read_text())
        verify_staged(portable)
        report['acquisitionReceipt'] = verify_source_mapping(portable, native)
        units, remaining = validate_base(composition, portable)
        source_inputs = [report['baseComposition'], report['nativeUnityManifest'], report['acquisitionReceipt'],
                         report['recipe'], *report['recipeModules'],
                         composition['portableManifest'], composition['preprocessingCompiler'],
                         *composition['programInputs'], *composition.get('combinedCoreDependencies', [])]
        for item in composition['remainingInputs']:
            source_inputs.extend(item.get('dependencies', []))
        verify_dependencies(source_inputs)
        selected = select_groups(native, portable, remaining)
        report.update(selected)
        source_inputs.extend(selected['inspectedUnitySources'])
        compiler = output / 'cpfe-dot-preprocess'
        shutil.copy2(composition['preprocessingCompiler']['path'], compiler)
        report['preprocessingCompiler'] = record(compiler)
        clock = composition['preprocessingClock']
        epoch = clock['epochSeconds'] if clock else None
        if clock != preprocessing_clock(epoch):
            raise ValueError('Invalid recorded preprocessing clock')
        standard = composition['standard']
        includes = [*map(Path, composition['provider']['includeDirectories']), EDG / 'backend/runtime/libcxx/include',
                    EDG / 'backend/runtime/posix/include', EDG / 'backend/runtime/libc/include']
        preincludes = list(map(Path, composition['provider']['preincludeHeaders']))
        env = dict(os.environ, EDG_BASE=str(EDG / 'upstream/bases/docker/dev-env/gcc'),
                   EDG_DOT_TARGET='portable-lp64-binary80-v2')
        fingerprint, initial = preprocessing_inputs(portable, portable_path, compiler, standard,
            includes, preincludes, units, env, 'library', epoch)
        report['inventoryDifferences'] = verify_contract(composition['currentPreprocessingContract'], fingerprint)
        report.update(portableManifest=composition['portableManifest'], preprocessingClock=clock,
                      standard=standard, preprocessingContract=fingerprint,
                      configuredOriginalTUs=len(units), groupedOriginalTUs=selected['representedRemainingTUs'],
                      separateOriginalTUs=len(selected['ungroupedRemainingTUs']))
        by_source = {unit['file']: unit for unit in units}
        for index, group in enumerate(report['groups']):
            wrapper = output / ('unity-%04d.cpp' % index)
            wrapper.write_text(''.join('#include ' + json.dumps(source) + '\n' for source in group['orderedMembers']))
            initial[str(wrapper)] = sha(wrapper)
            unit = dict(by_source[group['orderedMembers'][0]], file=str(wrapper))
            command, _ = preprocessing_command(compiler, standard, includes, preincludes, unit, epoch)
            group.update(wrapper=record(wrapper), command=command,
                coveredSourceIndices=[remaining[source]['sourceIndex'] for source in group['orderedMembers']],
                memberInputs=[record(source) for source in group['orderedMembers']])
            source_inputs.extend([group['wrapper'], *group['memberInputs']])
        planned = [dict(path=str(output / ('unity-%04d.ii' % index))) for index in range(len(report['groups']))]
        report['plannedProgramInputs'] = compose_inputs(composition['programInputs'], report['groups'], planned, len(units))
        report['plannedProgramInputCount'] = len(report['plannedProgramInputs'])
        report['configuredCoverageExactlyOnce'] = True
        report['status'] = 'inventory-only'
        save()
        if preprocess:
            report['status'] = 'preprocessing'
            save()
            for index, group in enumerate(report['groups']):
                destination = output / ('unity-%04d.ii' % index)
                diagnostics = destination.with_suffix('.stderr')
                begin = time.monotonic()
                with destination.open('wb') as stdout, diagnostics.open('wb') as stderr:
                    result = subprocess.run(group['command'], cwd=output, env=env, stdout=stdout, stderr=stderr, timeout=timeout)
                item = dict(groupIndex=index, exitCode=result.returncode, output=record(destination),
                            stderr=record(diagnostics), elapsedSeconds=round(time.monotonic() - begin, 3))
                report['preprocessing'].append(item)
                save()
                if result.returncode:
                    raise ValueError('Unity preprocessing failed: ' + group['target'])
                dependencies, volatile = dependency_receipt(included_files(destination), initial, epoch)
                item['dependencies'] = dependencies
                if volatile:
                    raise ValueError('Unity preprocessing contains unfixed volatile inputs: ' + repr(volatile))
                source_inputs.extend(dependencies)
                source_inputs.extend([item['output'], item['stderr']])
                save()
                print('preprocessed %d/%d %s' % (index + 1, len(report['groups']), group['target']), flush=True)
            report['programInputs'] = compose_inputs(composition['programInputs'], report['groups'],
                [item['output'] for item in report['preprocessing']], len(units))
            report['programInputCount'] = len(report['programInputs'])
            report['status'] = 'preprocessed-composition'
        verify_staged(portable)
        verify_dependencies(source_inputs + [report['preprocessingCompiler']])
        verify_source_mapping(portable, native)
        verify_dependencies([item for item in fingerprint['headerInventory'] if 'sha256' in item])
        final_fingerprint, _ = preprocessing_inputs(portable, portable_path, compiler, standard,
            includes, preincludes, units, env, 'library', epoch)
        if final_fingerprint != fingerprint:
            raise ValueError('Preprocessing source/header/search inventory changed during composition')
        report['verifiedInputs'] = source_inputs
        report['frozenInputsUnchanged'] = True
        report['policy'] = ('Original core first; complete upstream unity groups replace their members at the first '
                            'original member. Ungrouped TUs retain their original order and preprocessing. '
                            'No partial groups, omitted sources, or relaxed external-symbol rejection.')
    except Exception as error:
        report.update(status='failed', failure=str(error))
        raise
    finally:
        report['elapsedSeconds'] = round(time.monotonic() - started, 3)
        save()
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__, allow_abbrev=False)
    parser.add_argument('--composition', required=True, type=Path)
    parser.add_argument('--unity-manifest', required=True, type=Path, help='native unity CMake sources.json')
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--preprocess', action='store_true')
    parser.add_argument('--timeout', type=int, default=600)
    args = parser.parse_args()
    if args.timeout <= 0:
        parser.error('--timeout must be positive')
    result = prepare(args.composition.resolve(), args.unity_manifest.resolve(), args.output_dir,
                     args.preprocess, args.timeout)
    print(json.dumps({key: result[key] for key in ('status', 'groupCount', 'configuredOriginalTUs',
                                                   'groupedOriginalTUs', 'separateOriginalTUs', 'plannedProgramInputCount')}))


if __name__ == '__main__':
    main()
