"""Pinned libc++ source assembly for independent original-source .dotobj units.

This is source selection, not a claim that the selected APIs already execute.
The portable POSIX/BCL configuration is independent of the build host OS.
"""
import hashlib
import json
from pathlib import Path
import re

# LLVM18 src/CMakeLists.txt: base sources plus threads, localization, random,
# filesystem, new/delete. No Windows/zOS source branch and no experimental lib.
UPSTREAM_SOURCES = (
    'algorithm.cpp', 'any.cpp', 'bind.cpp', 'call_once.cpp', 'charconv.cpp',
    'chrono.cpp', 'error_category.cpp', 'filesystem/filesystem_clock.cpp',
    'filesystem/filesystem_error.cpp', 'filesystem/path.cpp', 'functional.cpp',
    'hash.cpp', 'legacy_pointer_safety.cpp', 'memory.cpp', 'memory_resource.cpp',
    'new_handler.cpp', 'new_helpers.cpp', 'optional.cpp', 'print.cpp',
    'random_shuffle.cpp', 'ryu/d2fixed.cpp', 'ryu/d2s.cpp', 'ryu/f2s.cpp',
    'stdexcept.cpp', 'string.cpp', 'system_error.cpp', 'typeinfo.cpp',
    'valarray.cpp', 'variant.cpp', 'vector.cpp', 'verbose_abort.cpp',
    'barrier.cpp', 'condition_variable.cpp', 'future.cpp', 'mutex.cpp',
    'shared_mutex.cpp', 'thread.cpp', 'random.cpp', 'fstream.cpp', 'ios.cpp',
    'ios.instantiations.cpp', 'iostream.cpp', 'locale.cpp', 'ostream.cpp',
    'regex.cpp', 'strstream.cpp', 'filesystem/directory_entry.cpp',
    'filesystem/directory_iterator.cpp', 'filesystem/operations.cpp',
    'filesystem/int128_builtins.cpp', 'new.cpp',
)
REPLACEMENTS = {
    'exception.cpp': ('abi_core.cpp', 'abi_exceptions.cpp'),
    'atomic.cpp': ('abi_atomic.cpp',),
    'mutex_destructor.cpp': ('abi_mutex.cpp',),
    'condition_variable_destructor.cpp': ('abi_mutex.cpp',),
}


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def prepare_libcxx_units(edg, acquisition, libc_arguments):
    edg, acquisition = Path(edg).resolve(), Path(acquisition).resolve()
    adapter = edg / 'backend/runtime/libcxx'
    pin_path = adapter / 'source.json'
    pin = json.loads(pin_path.read_text())
    acquired_path = acquisition / 'acquisition.json'
    acquired = json.loads(acquired_path.read_text())
    if any(acquired['source'].get(key) != pin[key] for key in ('sha256', 'archiveRoot')):
        raise ValueError('libc++ acquisition differs from the pinned archive')
    upstream = acquisition / pin['archiveRoot']
    verified = {}
    for name, expected in sorted(acquired['fileHashes'].items()):
        if name.startswith(('config/', pin['archiveRoot'] + '/include/', pin['archiveRoot'] + '/src/')):
            path = acquisition / name
            if sha(path) != expected:
                raise ValueError('Acquired libc++ provider input changed: ' + name)
            verified[name] = expected
    for prefix in ('config/', pin['archiveRoot'] + '/include/', pin['archiveRoot'] + '/src/'):
        if not any(name.startswith(prefix) for name in verified):
            raise ValueError('libc++ acquisition has no recorded ' + prefix)
    cmake_path = upstream / 'src/CMakeLists.txt'
    cmake_name = pin['archiveRoot'] + '/src/CMakeLists.txt'
    if cmake_name not in verified:
        raise ValueError('libc++ acquisition does not record its source build description')
    # Fail closed if the pinned source set is updated without reviewing its
    # platform/feature branches. This is not an ad-hoc CMake interpreter.
    cmake = cmake_path.read_text().split('# Add all the headers to the project', 1)[0]
    declared = set(re.findall(r'^\s+([\w/.]+\.cpp)\s*$', cmake, re.M))
    portable = {name for name in declared if not name.startswith('support/')}
    if portable != set(UPSTREAM_SOURCES) | set(REPLACEMENTS):
        raise ValueError('Pinned libc++ portable source inventory changed')

    # Reuse authoritative managed ABI adapters, not the old core profile's
    # single-TU amalgamation. Its adapter hashes pin the shared source versions.
    core_path = adapter / 'core-source-profile.json'
    core = json.loads(core_path.read_text())
    adapter_pins = {row['path']: row['sha256'] for row in core['orderedSources'] if row['base'] == 'adapter'}
    adapter_names = sorted({name for names in REPLACEMENTS.values() for name in names})
    for name in ['core-source-preamble.h', *adapter_names]:
        if name not in adapter_pins or sha(adapter / name) != adapter_pins[name]:
            raise ValueError('Canonical libc++ ABI adapter changed: ' + name)
    rtti_path = edg / 'object-linker/libcxx-rtti-profile.json'
    rtti = json.loads(rtti_path.read_text())
    required_rtti = {'backend/runtime/libcxx/abi_typeinfo.cpp',
                     'upstream/include_c++/cxxabi.h', 'upstream/lib_src/typeinfo.c'}
    if set(rtti['inputs']) != required_rtti:
        raise ValueError('RTTI source provider inventory changed')
    rtti_inputs = []
    for name, expected in sorted(rtti['inputs'].items()):
        path = edg / name
        if sha(path) != expected:
            raise ValueError('Canonical RTTI source provider changed: ' + name)
        rtti_inputs.append(dict(path=str(path), sha256=expected))
    config = (acquisition / 'config/__config_site').read_text()
    for declaration in ('_LIBCPP_HAS_THREAD_API_EXTERNAL', '_LIBCPP_ABI_FORCE_ITANIUM'):
        if not re.search(r'^#define\s+' + declaration + r'\s*$', config, re.M):
            raise ValueError('libc++ provider requires portable managed configuration: ' + declaration)
    if not re.search(r'^#define\s+_LIBCPP_ABI_NAMESPACE\s+' + re.escape(pin['abiNamespace']) + r'\s*$', config, re.M):
        raise ValueError('libc++ ABI namespace differs from the pinned provider')

    include_paths = (acquisition / 'config', upstream / 'include',
                     adapter / 'include', upstream / 'src')
    arguments = [value for path in include_paths for value in ('-I', str(path))]
    arguments += list(libc_arguments)
    arguments += ['--preinclude', str(adapter / 'core-source-preamble.h'), '--exceptions', '--rtti']
    units, inputs = [], []
    selected = [(upstream / 'src' / name, 'src/' + name) for name in UPSTREAM_SOURCES]
    selected += [(adapter / name, 'adapter/' + name) for name in adapter_names]
    selected.append((adapter / 'abi_typeinfo.cpp', 'adapter/abi_typeinfo.cpp'))
    for source, key in selected:
        if key.startswith('src/') and pin['archiveRoot'] + '/' + key not in verified:
            raise ValueError('Selected libc++ source is not recorded: ' + key)
        unit_arguments = list(arguments)
        if key in ('adapter/abi_typeinfo.cpp', 'src/typeinfo.cpp'):
            # Actual empty/noexcept key destructors need no source unwind
            # machinery. Declare standard delete before lowering those bodies,
            # avoiding an implicit declaration with an unknown throw policy.
            unit_arguments = ['--no_exceptions' if item == '--exceptions' else item for item in unit_arguments]
            unit_arguments += ['--preinclude', str(upstream / 'include/new')]
        units.append(dict(id='libcxx:' + key, source=str(source), standard='c++20',
                          arguments=unit_arguments, role='libcxx'))
        inputs.append(dict(path=str(source), sha256=sha(source)))
    receipt = dict(schema=1, kind='independent-original-source-libcxx-provider',
                   release=pin['release'], standard='c++20',
                   abiNamespace=pin['abiNamespace'], nativeAbiLibrary=None,
                   sourcePolicy='portable POSIX target with shared managed ABI and external threading',
                   sourcePin=dict(path=str(pin_path), sha256=sha(pin_path)),
                   acquisition=dict(path=str(acquired_path), sha256=sha(acquired_path)),
                   cmake=dict(path=str(cmake_path), sha256=sha(cmake_path)),
                   adapterPins=dict(path=str(core_path), sha256=sha(core_path)),
                   rttiProvider=dict(profile=dict(path=str(rtti_path), sha256=sha(rtti_path)),
                                     sourceInputs=rtti_inputs,
                                     exceptionMode='explicit --no_exceptions for RTTI key destructors only',
                                     deallocatorDeclarations=str(upstream / 'include/new')),
                   preinclude=dict(path=str(adapter / 'core-source-preamble.h'), sha256=adapter_pins['core-source-preamble.h']),
                   sourceInputs=inputs, inputHashes=verified,
                   replacements=REPLACEMENTS,
                   features=dict(threads=True, localization=True, randomDevice=True, filesystem=True, newDelete=True),
                   compilationVerified=False, runtimeBindingVerified=False,
                   limitations=['No experimental libc++ library selected',
                                'Owned C++ layout/lifetime/EH and shared runtime coverage remain separate acceptance gates'])
    return units, receipt
