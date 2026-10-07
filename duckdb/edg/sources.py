#!/usr/bin/env python3
"""Export exact native source closures from CMake File API and compile commands."""
import argparse
import collections
import json
from pathlib import Path
import re
import shlex


def manifest(build, source, output=None):
    build, source = Path(build).resolve(), Path(source).resolve()
    reply = build / '.cmake/api/v1/reply'
    index = json.loads(sorted(reply.glob('index-*.json'))[-1].read_text())
    model = json.loads((reply / index['reply']['codemodel-v2']['jsonFile']).read_text())
    targets = {t['id']: json.loads((reply / t['jsonFile']).read_text())
               for t in model['configurations'][0]['targets']}
    names = {t['name']: ident for ident, t in targets.items()}

    def closure(name):
        pending, selected = [names[name]], set()
        while pending:
            ident = pending.pop()
            if ident in selected:
                continue
            selected.add(ident)
            pending.extend(d['id'] for d in targets[ident].get('dependencies', []))
        return {targets[ident]['name'] for ident in selected}

    core, tests, shell = closure('duckdb'), closure('unittest'), closure('shell')
    units = []
    for command in json.loads((build / 'compile_commands.json').read_text()):
        args = command.get('arguments') or shlex.split(command['command'])
        obj = command.get('output') or args[args.index('-o') + 1]
        target = re.search(r'CMakeFiles/([^/]+)\.dir/', obj).group(1)
        if target in core:
            role = 'library'
        elif target in tests:
            role = 'test'
        elif target in shell:
            role = 'shell'
        else:
            role = 'unselected-build-target'
        file = Path(command['file']).resolve()
        includes, defines = [], []
        for i, arg in enumerate(args):
            if arg == '-I' or arg == '-isystem':
                includes.append(args[i + 1])
            elif arg.startswith('-I'):
                includes.append(arg[2:])
            elif arg.startswith('-D'):
                defines.append(arg[2:])
        units.append(dict(file=str(file), directory=command['directory'], arguments=args,
                          target=target, role=role, includeDirectories=includes, defines=defines,
                          language='c' if file.suffix == '.c' else 'c++'))
    counts = dict(collections.Counter(u['role'] for u in units))
    include_sets = collections.Counter((u['role'], tuple(sorted(set(u['includeDirectories']))))
                                       for u in units)
    inventory = []
    dependency_folders = list((source / 'third_party').iterdir())
    dependency_folders.extend(source.glob('extension/*/third_party/*'))
    for folder in sorted(dependency_folders):
        if not folder.is_dir():
            continue
        compiled = [u for u in units if Path(u['file']).is_relative_to(folder)]
        include_users = collections.Counter()
        for (role, paths), count in include_sets.items():
            if any(Path(p).is_relative_to(folder) for p in paths):
                include_users[role] += count
        reason = {
            'catch': 'Header-only test framework, used by upstream unittest runner.',
            'imdb': 'Bundled benchmark SQL/data scripts, not a C/C++ library.',
            'jemalloc': 'Optional native allocator replaced by shared NativeMemory-backed libc malloc/new.',
            'snowball': 'Bundled stemming sources are not referenced by any configured core/static-extension target.',
        }.get(folder.name)
        inventory.append(dict(name=folder.name, path=str(folder.relative_to(source)),
            compiledRoles=dict(collections.Counter(u['role'] for u in compiled)),
            includeRoles=dict(include_users), reason=reason,
            disposition=('compiled in library' if any(u['role'] == 'library' for u in compiled)
                         else 'library header dependency' if include_users['library']
                         else 'compiled outside library' if compiled
                         else 'header dependency outside library' if include_users
                         else 'test header dependency (relative include)' if folder.name == 'catch'
                         else 'not selected by upstream configuration')))
    result = dict(source=str(source), build=str(build), units=units, counts=counts,
                  roots=dict(library='duckdb', test='unittest', shell='shell'),
                  libraryTargets=sorted(core), testTargets=sorted(tests - core),
                  bundledDependencies=inventory,
                  staticExtensions=['core_functions', 'parquet', 'icu', 'json', 'autocomplete', 'tpcds', 'tpch'],
                  exclusions=[
                      dict(component='jemalloc', reason='Allocator selection: shared NativeMemory-backed libc malloc/new runtime replaces optional jemalloc.'),
                      dict(component='delta', reason='Requires external Git/Rust delta-kernel-rs; not a bundled C/C++ dependency.'),
                      dict(component='demo_capi', reason='Upstream provides only a dynamically loadable demo extension; dynamic extension loading is disabled.'),
                      dict(component='loadable extension targets', reason='Only statically linked extensions are translation inputs; duplicate dynamic-library compile targets excluded.')])
    destination = Path(output) if output else build / 'sources.json'
    destination.write_text(json.dumps(result, indent=2) + '\n')
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build-dir', type=Path, required=True)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    print(json.dumps(manifest(args.build_dir, args.source, args.output)['counts']))
