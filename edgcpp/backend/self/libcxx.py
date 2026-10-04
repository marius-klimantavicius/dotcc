"""Pinned libc++ allocation/exception source closure for the EDG compiler.

The provider is the primary translation unit, sharing the same public libc++
headers as the subsequent EDG translation units. This does not use the bootstrap replacement std declarations.
"""
import hashlib
import json
from pathlib import Path


def prepare_provider(root: Path, acquisition: Path, output: Path):
    root, acquisition, output = root.resolve(), acquisition.resolve(), output.resolve()
    adapter = root / 'backend/runtime/libcxx'
    pin = json.loads((adapter / 'source.json').read_text())
    manifest = json.loads((acquisition / 'acquisition.json').read_text())
    if manifest['source']['sha256'] != pin['sha256']:
        raise ValueError('libc++ acquisition does not match the pinned source archive')
    upstream = acquisition / pin['archiveRoot']
    profile = json.loads((adapter / 'core-source-profile.json').read_text())
    selected = {'core-source-preamble.h', 'abi_core.cpp', 'abi_exceptions.cpp',
                'src/new.cpp', 'src/new_handler.cpp', 'src/new_helpers.cpp', 'src/typeinfo.cpp'}
    sources = []
    records = []
    for entry in profile['orderedSources']:
        if entry['path'] not in selected:
            continue
        path = (adapter if entry['base'] == 'adapter' else upstream) / entry['path']
        actual = hashlib.sha256(path.read_bytes()).hexdigest()
        if actual != entry['sha256']:
            raise ValueError('libc++ provider source changed: ' + str(path))
        sources.append(path.resolve())
        records.append({'path': str(path.resolve()), 'sha256': actual})
    if len(sources) != len(selected):
        raise ValueError('libc++ core profile is missing a required EDG provider source')
    # Upstream implementation headers are part of the provider, too. Validate
    # the complete src tree so a nested include cannot escape provenance checks.
    implementation_hashes = {}
    header_hashes = {}
    for name, expected in manifest['fileHashes'].items():
        if name.startswith(('config/', pin['archiveRoot'] + '/include/')):
            actual = hashlib.sha256((acquisition / name).read_bytes()).hexdigest()
            if actual != expected:
                raise ValueError('libc++ public header/configuration changed: ' + name)
            header_hashes[name] = actual
        if name.startswith(pin['archiveRoot'] + '/src/'):
            actual = hashlib.sha256((acquisition / name).read_bytes()).hexdigest()
            if actual != expected:
                raise ValueError('libc++ implementation input changed: ' + name)
            implementation_hashes[name] = actual
    if not header_hashes:
        raise ValueError('libc++ acquisition has no recorded public headers/configuration')
    if not implementation_hashes:
        raise ValueError('libc++ acquisition has no recorded implementation sources')
    output.parent.mkdir(parents=True, exist_ok=True)
    atomic_source = upstream / 'src/include/atomic_support.h'
    atomic_original = atomic_source.read_text()
    memory_include = '#include <memory> // for __libcpp_relaxed_load\n'
    if atomic_original.count(memory_include) != 1:
        raise ValueError('Pinned atomic_support.h include adaptation no longer matches')
    # Every template in this private header is self-contained; the historical
    # memory include exposes __libcpp_relaxed_load for other users, but none of
    # this header's bodies or new_handler.cpp calls it. Keep all atomic bodies
    # and their sequentially-consistent default ordering exactly upstream.
    atomic_copy = output.with_name('libcxx-self-atomic-support.h')
    atomic_copy.write_text(atomic_original.replace(memory_include, ''))
    lines = ['// Pinned LLVM libc++ provider for EDG self-translation.']
    for path in sources:
        if path.name == 'new_handler.cpp':
            lines.append('#include ' + json.dumps(str(atomic_copy.resolve())))
            # LLVM builds this file in C++20. Its only constinit object is a
            # null-initialized function pointer, also constant-initialized in
            # C++14. Use libc++'s own compatibility spelling in this one file.
            lines += ['#if __cplusplus < 202002L', '#define constinit _LIBCPP_CONSTINIT', '#endif']
        lines.append('#include ' + json.dumps(str(path)))
        if path.name == 'new_handler.cpp':
            lines += ['#if __cplusplus < 202002L', '#undef constinit', '#endif']
    output.write_text('\n'.join(lines) + '\n')
    return {'kind': 'translated-libcxx-allocation-and-managed-exception-abi',
            'release': profile['release'], 'standard': 'c++14', 'position': 'primary',
            'publicHeaderCount': len(header_hashes), 'translationUnit': str(output.resolve()),
            'translationUnitSha256': hashlib.sha256(output.read_bytes()).hexdigest(),
            'sources': records, 'implementationHashes': implementation_hashes,
            'adaptedPrivateHeader': {'upstream': str(atomic_source), 'path': str(atomic_copy),
                'sha256': hashlib.sha256(atomic_copy.read_bytes()).hexdigest()},
            'adaptations': ['Remove unused <memory> include from private atomic_support.h; all atomic bodies/orderings unchanged', 'Map new_handler.cpp constinit to _LIBCPP_CONSTINIT for C++14; null pointer constant initialization is unchanged'],
            'scope': 'Allocation, exception class definitions and managed EH adapters required by EDG; not the entire libc++ library'}
