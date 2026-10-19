"""Require the exact existing SQLite delivery before specialist product tests."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct


def digest(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def require(condition, message):
    if not condition:
        raise RuntimeError('SQLite existing-product identity: ' + message)


def tool_files(directory):
    return {p.relative_to(directory).as_posix(): digest(p) for p in sorted(Path(directory).rglob('*'))
            if p.is_file()}


def source_files(root, source):
    paths = [*Path(source).glob('*.h'), Path(source) / 'sqlite3.c']
    for directory in ('config', 'src', 'scripts'):
        paths += [p for p in (root / directory).rglob('*') if p.is_file()
                  and not {'bin', 'obj', '__pycache__'}.intersection(p.relative_to(root).parts)]
    paths += sorted((root.parent / 'DotCC.Libc').glob('*.cs'))
    paths += [root.parent / 'Scripts/production_support.py', root.parent / 'Scripts/production-common.sh',
              root.parent / 'Scripts/campaign-common.sh', *(root.parent / 'Scripts/campaigns').glob('*.py')]
    return {str(p.resolve()): digest(p) for p in sorted(set(paths))}


def clr_resources(path):
    """Read the PE's CLR resource directory without running a managed tool."""
    data = Path(path).read_bytes()
    require(data[:2] == b'MZ', 'compiler library is not a PE')
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    require(data[pe:pe + 4] == b'PE\0\0', 'invalid compiler PE signature')
    count = struct.unpack_from('<H', data, pe + 6)[0]
    optional_size = struct.unpack_from('<H', data, pe + 20)[0]
    optional = pe + 24
    magic = struct.unpack_from('<H', data, optional)[0]
    require(magic in (0x10b, 0x20b), 'unsupported compiler PE header')
    directory = optional + (96 if magic == 0x10b else 112)
    cli_rva, cli_size = struct.unpack_from('<II', data, directory + 14 * 8)
    sections = [struct.unpack_from('<IIII', data, optional + optional_size + i * 40 + 8)
                for i in range(count)]

    def offset(rva, size):
        for virtual_size, address, raw_size, raw in sections:
            if address <= rva and rva + size <= address + min(virtual_size, raw_size):
                return raw + rva - address
        raise RuntimeError('SQLite existing-product identity: invalid CLR resource RVA')

    require(cli_size >= 32, 'missing CLR header')
    cli = offset(cli_rva, 32)
    rva, size = struct.unpack_from('<II', data, cli + 24)
    start = offset(rva, size)
    return data[start:start + size]


def runtime_binding(repo, compiler, raw_directory=None):
    library = Path(compiler).parent / 'DotCC.Lib.dll'
    resources = clr_resources(library)
    text = None if raw_directory is None else '\n'.join(
        p.read_text(encoding='utf-8-sig') for p in sorted(Path(raw_directory).glob('*.cs')))
    rows = {}
    for source in sorted((repo / 'DotCC.Libc').glob('*.cs')):
        data = source.read_bytes()
        position = resources.find(data)
        require(position >= 4 and resources.count(data) == 1 and
                struct.unpack_from('<I', resources, position - 4)[0] == len(data),
                'current runtime source differs from compiler CLR resource: ' + source.name)
        row = dict(sha256=digest(source), resourceOffset=position - 4, bytes=len(data))
        if text is not None and not source.name.startswith('Zig') and source.name != 'Slice.cs':
            marker = '// ---- ' + source.name + ' ----\n#nullable enable\n'
            require(text.count(marker) == 1, 'missing/duplicate raw runtime section: ' + source.name)
            start = text.index(marker) + len(marker)
            end = text.index('#nullable restore', start)
            actual = text[start:end]
            expected = ''.join(line + '\n' for line in source.read_text().split('\n')
                if not line.lstrip().startswith(('#nullable', 'namespace ')) and not line.startswith('using ')) + '\n'
            require(actual == expected, 'raw product embeds another runtime: ' + source.name)
            row['rawSectionSha256'] = hashlib.sha256(actual.encode()).hexdigest()
        rows[str(source)] = row
    require(bool(rows), 'no current runtime sources')
    return dict(compilerLibrary=str(library), compilerLibrarySha256=digest(library), sources=rows)


def capture(ctx):
    """After real final-path builds, bind their PEs and current runtime sections."""
    require(ctx.receipt['sqlite_source_inputs'] == source_files(ctx.root, ctx.sources['product']),
            'source/configuration changed during translation')
    raw = Path(ctx.receipt['outputs']['raw']['project']).parent
    binding = runtime_binding(ctx.repo, ctx.compiler, raw)
    binding['products'] = {}
    for form, output in ctx.receipt['outputs'].items():
        project = Path(output['project'])
        pe = project.parent / 'bin/Release/net10.0/TranslatedSqlite.dll'
        require(pe.is_file(), 'missing final compiled ' + form + ' PE')
        binding['products'][form] = dict(path=str(pe), sha256=digest(pe))
    ctx.receipt['sqlite_product_identity'] = binding
    ctx.save()


def validate(root, receipt_path, compiler):
    root, receipt_path, compiler = Path(root).resolve(), Path(receipt_path).resolve(), Path(compiler).resolve()
    receipt = json.loads(receipt_path.read_text())
    require(receipt.get('project') == 'sqlite' and receipt.get('profile') == 'default', 'wrong expected receipt')
    require(receipt.get('action') in ('translate', 'verify') and receipt.get('status') in ('passed', 'running'),
            'expected translation did not succeed')
    require(any(t['name'] == 'translation' and t['status'] == 'passed' for t in receipt['tasks']),
            'translation task has not passed')
    pointer = json.loads((root / 'artifacts/campaign/current-default.json').read_text())
    require(Path(pointer['receipt']).resolve() == receipt_path, 'current pointer names another receipt')
    inputs = receipt.get('sqlite_source_inputs')
    require(bool(inputs) and inputs == source_files(root, receipt['sources']['product']),
            'source/configuration/runtime inputs differ or are unbound')
    selected = tool_files(compiler.parent)
    require(bool(selected) and selected == receipt['tools']['DotCC'], 'selected compiler dependency closure differs')
    work = root / receipt['staging']
    for name in ('DotCC', 'DotCC.PostProcess'):
        require(tool_files(work / 'tools' / name) == receipt['tools'][name], 'recorded tool snapshot differs: ' + name)
    identity = receipt.get('sqlite_product_identity')
    require(bool(identity) and set(receipt['outputs']) == {'raw', 'processed'}, 'missing both-form product binding')
    for form, output in receipt['outputs'].items():
        project = Path(output['project'])
        expected = root / 'generated' / ('TranslatedSqlite.Raw' if form == 'raw' else 'TranslatedSqlite') / 'TranslatedSqlite.csproj'
        require(project == expected and project.is_file(), 'missing/wrong canonical project: ' + form)
        hashes = output['hashes']
        require(bool(hashes) and project.name in hashes, 'unbound generated sources: ' + form)
        for name, value in hashes.items():
            path = project.parent / name
            require(path.resolve().is_relative_to(project.parent) and path.is_file() and digest(path) == value,
                    'generated output differs: ' + form + '/' + name)
        require(any(c.get('label') == 'final-' + form + '-build' and c.get('exit_code') == 0
                    and c.get('status') == 'passed' for c in receipt['commands']), 'missing real final build: ' + form)
        pe = identity['products'][form]
        require(digest(pe['path']) == pe['sha256'], 'compiled product differs: ' + form)
    actual = runtime_binding(root.parent, compiler, root / 'generated/TranslatedSqlite.Raw')
    require(actual['compilerLibrarySha256'] == identity['compilerLibrarySha256'] and
            actual['sources'] == identity['sources'], 'compiler/source/raw runtime binding differs')
    return dict(receipt=str(receipt_path), receiptSha256=digest(receipt_path),
                compiler=str(compiler), runtimeSources=len(actual['sources']), products=identity['products'])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--receipt', default=os.environ.get('SQLITE_EXPECTED_RECEIPT'))
    parser.add_argument('--compiler', default=os.environ.get('DOTCC_COMPILER'))
    args = parser.parse_args()
    require(bool(args.receipt) and bool(args.compiler), 'set SQLITE_EXPECTED_RECEIPT and DOTCC_COMPILER explicitly; run translate first')
    print(json.dumps(validate(Path(__file__).resolve().parents[1], args.receipt, args.compiler), indent=2))


if __name__ == '__main__':
    main()
