#!/usr/bin/env python3
"""Read-only source/artifact pinning. Never invokes a compiler, Git, or EDG.

Only writes baseline/local-files.{json,sha256}, build-commands.json, and
generated-definitions.json next to this script. Run from any working directory.
"""
import hashlib
import json
from pathlib import Path
import re
import shlex

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]


def sha(path):
    h = hashlib.sha256()
    with path.open('rb') as f:
        for block in iter(lambda: f.read(1024 * 1024), b''):
            h.update(block)
    return h.hexdigest()


def save(name, obj):
    (OUT / name).write_text(json.dumps(obj, indent=2, sort_keys=True) + '\n')


def main():
    scopes = {
        'source_headers_tables': 'edgcpp/upstream/src',
        'native_runtime_sources': 'edgcpp/upstream/lib_src',
        'cmake_configuration': 'edgcpp/upstream/cmake',
        'local_macro_configuration': 'edgcpp/config',
        'driver_utilities': 'edgcpp/upstream/util',
        'supplied_cpp_headers': 'edgcpp/upstream/include_c++',
        'supplied_c_headers': 'edgcpp/upstream/include_c99',
        'generated_configuration_and_diagnostics': 'edgcpp/build/src/includes',
    }
    chosen = {}
    for scope, directory in scopes.items():
        for p in sorted((ROOT / directory).rglob('*')):
            if p.is_file():
                chosen[p] = scope
    individual = [
        'edgcpp/upstream-origin.json', 'edgcpp/upstream/CMakeLists.txt',
        'edgcpp/upstream/CMakePresets.json', 'edgcpp/upstream/LICENSE.txt',
        'edgcpp/upstream/BUILD.md', 'edgcpp/upstream/README.md',
        'edgcpp/upstream/dev_tools/bin/edg-cmakedef',
        'edgcpp/upstream/dev_tools/bin/edg-scrape-compiler',
        'edgcpp/build.sh', 'edgcpp/backend/prepare.py',
        'edgcpp/build/CMakeCache.txt', 'edgcpp/build/compile_commands.json',
        'edgcpp/build/build.ninja', 'edgcpp/build/CMakeFiles/rules.ninja',
        'edgcpp/build/environment.sh', 'edgcpp/build/lazy_environment.sh',
        'edgcpp/manual-csharp/tools/oracle.py',
        'edgcpp/manual-csharp/tools/oracle-inputs/profile.cpp',
    ]
    for p in sorted((ROOT / 'edgcpp/build/CMakeFiles').glob('*/CMake*Compiler.cmake')):
        chosen[p] = 'recorded_compiler_metadata'
    for p in sorted((ROOT / 'edgcpp/build/CMakeFiles').glob('*/CMakeSystem.cmake')):
        chosen[p] = 'recorded_compiler_metadata'
    for rel in individual:
        p = ROOT / rel
        if p.is_file():
            chosen[p] = 'provenance_build_and_historical_profile'
    for p in sorted((ROOT / 'edgcpp/build/bin').iterdir()):
        if p.is_file():
            chosen[p] = 'existing_native_artifact_not_executed'
    # Binary reads only: the currently installed compiler may differ from the
    # compiler recorded when CMake configured this native build.
    for rel in ('/usr/bin/c++', '/usr/bin/cc',
                '/usr/lib/gcc/x86_64-linux-gnu/13/cc1plus',
                '/usr/libexec/gcc/x86_64-linux-gnu/13/cc1plus',
                '/usr/lib/gcc/x86_64-linux-gnu/13/include/stddef.h',
                '/usr/include/x86_64-linux-gnu/bits/wordsize.h',
                '/usr/include/x86_64-linux-gnu/bits/typesizes.h'):
        p = Path(rel)
        if p.is_file():
            chosen[p] = 'installed_compiler_or_host_header_read_only'
    missing = [rel for rel in individual if not (ROOT / rel).is_file()]
    entries = []
    for p, scope in sorted(chosen.items(), key=lambda x: str(x[0])):
        try:
            name = str(p.relative_to(ROOT))
        except ValueError:
            name = str(p)
        before = p.stat()
        digest = sha(p)
        after = p.stat()
        if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns):
            raise RuntimeError('Input changed while reading: ' + str(p))
        row = dict(path=name, size=after.st_size, sha256=digest, scope=scope)
        if p.is_symlink():
            row['symlink_target'] = str(p.resolve())
        entries.append(row)
    manifest = ''.join(f"{r['sha256']}  {r['path']}\n" for r in entries)
    (OUT / 'local-files.sha256').write_text(manifest)
    save('local-files.json', dict(
        schema=1, root=str(ROOT), operation='read-only hashing; no native execution',
        missing_optional_named_inputs=missing,
        inventory_status='complete file enumeration for declared scopes only',
        excluded=['upstream/tests and benchmarks', 'dev_tools except two named scripts',
                  'backend implementation (prepare.py hash is provenance only)',
                  'old prototype implementation/evidence', 'native objects and system libraries'],
        source_executable_correspondence='unproven; existing binary is separately pinned',
        entries=entries))
    commands = json.loads((ROOT / 'edgcpp/build/compile_commands.json').read_text())
    groups = {}
    for item in commands:
        argv = shlex.split(item['command'])
        m = re.search(r'CMakeFiles/([^/]+)\.dir/', item.get('output', ''))
        target = m.group(1) if m else 'unresolved'
        row = dict(item)
        row['argv'] = argv
        row['command_sha256'] = hashlib.sha256(item['command'].encode()).hexdigest()
        groups.setdefault(target, []).append(row)
    links = {}
    lines = (ROOT / 'edgcpp/build/build.ninja').read_text().splitlines()
    for i, line in enumerate(lines):
        if line.startswith('build bin/') and 'EXECUTABLE_LINKER' in line:
            block = [line]
            for following in lines[i+1:]:
                if not following.startswith(' '):
                    break
                block.append(following)
            links[line.split(':', 1)[0][6:]] = block
    save('build-commands.json', dict(
        interpretation='recorded generated build instructions, not proof of executed commands',
        targets=groups, ninja_link_blocks=links))
    definitions = []
    path = ROOT / 'edgcpp/build/src/includes/cmake_defines.h'
    for i, line in enumerate(path.read_text().splitlines(), 1):
        m = re.match(r'\s*#define\s+(\w+)(.*)', line)
        if m:
            definitions.append(dict(name=m.group(1), replacement=m.group(2).strip(), line=i))
    save('generated-definitions.json', dict(
        path=str(path.relative_to(ROOT)), sha256=sha(path),
        interpretation='textual #define records; not a compiler-expanded effective macro dump; '
                       'command-line -D overrides and later headers must be inspected',
        definitions=definitions))
    print(json.dumps(dict(files=len(entries), targets={k: len(v) for k,v in groups.items()},
                          manifest_sha256=sha(OUT / 'local-files.sha256'))))


if __name__ == '__main__':
    main()
