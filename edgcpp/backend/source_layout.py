"""Partition complete compiler-marked members without rewriting C# bodies.

The caller retains storage declarations and the authoritative initialization
driver in its shell. Only members known to be movable may enter this module.
Source paths/names come from compiler metadata, never guessed from C# text.
"""
from dataclasses import dataclass
import hashlib
import os
from pathlib import PurePosixPath
import re


@dataclass(frozen=True)
class SourceMember:
    text: str
    source_file: str = ''
    source_line: int = 0
    source_column: int = 0
    source_name: str = ''
    managed_name: str = ''


@dataclass(frozen=True)
class SourcePart:
    filename: str
    text: str
    locations: tuple[dict, ...]


def _path(value):
    return value.replace('\\', '/')


def _component(value):
    # Portable output basenames only; no user-controlled path traversal.
    result = re.sub(r'[^A-Za-z0-9_.-]+', '_', value).strip(' .')
    return result[:100] or 'anonymous'


def is_layout_filename(value):
    return bool(re.fullmatch(r'Translated\.(?:part[0-9]{5,}|generated|(?:source|function)\.[A-Za-z0-9_.-]+)\.cs', value))


def split_members(members, wrapper, mode, target_size=262144):
    """Return ordered source parts and exact member-start navigation entries.

    `none` remains the driver's inline-shell path. Function and size splitting
    preserve input order. Source grouping preserves order within each source.
    A whole member may exceed the byte target; it is never split internally.
    """
    if mode not in ('function', 'size', 'source'):
        raise ValueError('split_members requires function, size or source mode')
    if target_size <= 0:
        raise ValueError('split size must be positive')
    members = list(members)
    wrapper = wrapper if wrapper.endswith('\n') else wrapper + '\n'
    footer = '}\n'
    source_paths = [_path(member.source_file) for member in members if member.source_file]
    absolute = [path for path in source_paths if path.startswith('/')]
    common = os.path.commonpath([str(PurePosixPath(path).parent) for path in absolute]) if absolute else ''

    def source_key(path):
        path = _path(path)
        if common and path.startswith(common.rstrip('/') + '/'):
            return path[len(common.rstrip('/')) + 1:]
        return path

    groups = []
    if mode == 'source':
        sources = {}
        for member in members:
            key = _path(member.source_file)
            if key not in sources:
                sources[key] = []
                groups.append((key, sources[key]))
            sources[key].append(member)
    elif mode == 'function':
        groups = [(member.source_name or member.managed_name or 'generated', [member]) for member in members]
    else:
        current = []
        size = len((wrapper + footer).encode('utf-8'))
        for member in members:
            text = member.text if member.text.endswith('\n') else member.text + '\n'
            needed = len(text.encode('utf-8'))
            if current and size + needed > target_size:
                groups.append(('', current))
                current = []
                size = len((wrapper + footer).encode('utf-8'))
            current.append(member)
            size += needed
        if current:
            groups.append(('', current))

    used = {'translated.cs', 'translated.members.cs'}
    parts = []
    for index, (key, group) in enumerate(groups):
        if mode == 'size':
            stem = f'Translated.part{index:05d}'
        elif mode == 'source':
            stem = 'Translated.source.' + _component(PurePosixPath(key).name) if key else 'Translated.generated'
        else:
            stem = 'Translated.function.' + _component(key.lstrip('@'))
        filename = stem + '.cs'
        if filename.casefold() in used:
            identity = '\n'.join(source_key(member.source_file) + ':' + str(member.source_line) + ':'
                                 + member.source_name + ':' + member.managed_name + ':' + member.text
                                 for member in group)
            suffix = hashlib.sha256(identity.encode()).hexdigest()[:12]
            filename = stem + '.' + suffix + '.cs'
            duplicate = 2
            while filename.casefold() in used:
                filename = stem + '.' + suffix + '.' + str(duplicate) + '.cs'
                duplicate += 1
        used.add(filename.casefold())
        assert is_layout_filename(filename), filename
        content = wrapper
        line = wrapper.count('\n') + 1
        locations = []
        for member in group:
            text = member.text if member.text.endswith('\n') else member.text + '\n'
            if member.source_file or member.source_name:
                locations.append({
                    'sourceFile': member.source_file,
                    'sourceLine': member.source_line,
                    'sourceColumn': member.source_column,
                    'sourceName': member.source_name,
                    'managedName': member.managed_name,
                    'generatedFile': filename,
                    'generatedLine': line,
                })
            content += text
            line += text.count('\n')
        parts.append(SourcePart(filename, content + footer, tuple(locations)))
    return parts
