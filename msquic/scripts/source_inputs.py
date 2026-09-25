"""Discover translation inputs in the selected, unmodified MsQuic snapshot."""
import re


def source_units(reference):
    cmake = (reference / 'src/core/CMakeLists.txt').read_text()
    cmake = re.sub(r'#[^\n]*', '', cmake)
    sources = re.search(r'\bset\s*\(\s*SOURCES\s+([^)]*)\)', cmake, re.I)
    if not sources:
        raise RuntimeError('Cannot find core SOURCES in upstream CMakeLists.txt')
    core = re.findall(r'\b[\w]+\.c\b', sources[1])
    if not core or len(core) != len(set(core)):
        raise RuntimeError('Upstream core SOURCES must contain unique C files')
    units = ['src/core/' + name for name in core]
    units += ['src/platform/' + name + '.c'
              for name in ('crypt', 'hashtable', 'pcp', 'platform_worker', 'toeplitz')]
    for name in units:
        if not (reference / name).is_file():
            raise RuntimeError('Missing selected upstream translation unit: ' + name)
    return units


FRAGMENTS = (
    ('portable_fragment', 'portable.c',
     '/* Copyright (c) Microsoft Corporation. Licensed under the MIT License.\n'
     '   Unchanged reference/rundown implementations extracted from pinned\n'
     '   src/platform/platform_posix.c. Host-owned event operations remain imports. */\n'
     '#include "platform_internal.h"\n\n'),
    ('route_fragment', 'route.c',
     '/* Copyright (c) Microsoft Corporation. Licensed under the MIT License.\n'
     '   Unchanged portable route-copy policy extracted from pinned datapath_xplat.c. */\n'
     '#include "platform_internal.h"\n\n'),
)


def portable_fragments(reference, overlay):
    result = {}
    for key, filename, preamble in FRAGMENTS:
        fragment = overlay[key]
        text = (reference / fragment['source']).read_text()
        start, end = fragment['start'], fragment['end']
        if text.count(start) != 1 or text.count(end) != 1 or text.index(start) >= text.index(end):
            raise RuntimeError('Cannot locate portable function boundaries in ' + fragment['source'])
        result[filename] = preamble + text[text.index(start):text.index(end)]
    return result
