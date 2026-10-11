#!/usr/bin/env python3
"""Lossless raw/zlib source snapshots, source-hidden verification and malformed frames."""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys
import zlib


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'linker', 'packager', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'; tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('packager', tooling / 'compile.py')
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    inputs = out / 'inputs'; inputs.mkdir()
    contents = {
        'main.c': b'#include "large.h"\n#include "raw.h"\n#include "empty.h"\nint main(void){return COMPRESSED + RAW == 11 ? 0 : 1;}\n',
        'large.h': ('/* Source snapshot UTF-8: \u03bb repeated. ' + 'abcdefghij ' * 12000 + '*/\n#define COMPRESSED 4\n').encode(),
        'raw.h': b'#define RAW 7\n',
        'empty.h': b'',
    }
    for name, data in contents.items(): (inputs / name).write_bytes(data)
    checks, commands = [], []
    def check(name, yes):
        if not yes: raise AssertionError(name)
        checks.append(name)
    def run(name, command, rejected=False):
        command = list(map(str, command)); p = subprocess.run(command, cwd=out, capture_output=True, timeout=180)
        (out / (name + '.stdout')).write_bytes(p.stdout); (out / (name + '.stderr')).write_bytes(p.stderr)
        commands.append(dict(name=name, command=command, exitCode=p.returncode))
        check(name, p.returncode != 0 if rejected else p.returncode == 0)
    error = None
    try:
        run('native-build', ['cc', '-std=c11', inputs / 'main.c', '-o', out / 'native'])
        run('native-run', [out / 'native'])
        obj = out / 'unit.dotobj'
        run('export', [sys.executable, '-B', tooling / 'compile.py', inputs / 'main.c', '--compiler', args.compiler.resolve(),
                      '--std', 'c11', '--output', obj, '--work-dir', out / 'work'])
        with sqlite3.connect(obj) as db:
            rows = {Path(path).name: (id, sha, data, codec, length) for id,path,sha,data,codec,length in db.execute(
                'SELECT id,path,sha256,bytes,codec,decoded_byte_length FROM source_files WHERE available=1') if Path(path).name in contents}
            check('all-original-files-retained', set(rows) == set(contents))
            for name, original in contents.items():
                id, sha, encoded, codec, length = rows[name]
                decoded = zlib.decompress(encoded) if codec == 'zlib' else encoded
                check('exact-snapshot-' + name, decoded == original and length == len(original) and sha == hashlib.sha256(original).hexdigest())
            check('large-header-compressed', rows['large.h'][3] == 'zlib' and len(rows['large.h'][2]) < rows['large.h'][4])
            check('short-header-raw', rows['raw.h'][3] == 'raw')
            check('empty-header-raw', rows['empty.h'][3:] == ('raw', 0))
            check('source-parent-edges-retained', db.execute('SELECT count(*) FROM source_files WHERE parent_id IS NOT NULL').fetchone()[0] >= 3)
        inputs.rename(out / 'retained-inputs')
        run('verify-source-hidden', ['dotnet', args.linker.resolve(), 'verify', obj])
        generated = out / 'generated'
        run('link-source-hidden', ['dotnet', args.linker.resolve(), 'link', '--entry', 'main', '--out', generated, obj])
        run('managed-build', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release', '-v:q'])
        run('managed-run', ['dotnet', generated / 'bin/Release/net10.0/Translated.dll'])
        target, sha, encoded, codec, length = rows['large.h']
        def mutate(name, assignments, values, rejected=True):
            damaged = out / (name + '.dotobj'); shutil.copy2(obj, damaged)
            with sqlite3.connect(damaged) as db:
                db.execute('PRAGMA ignore_check_constraints=ON')
                check(name + '-changed', db.execute('UPDATE source_files SET ' + assignments + ' WHERE id=?', (*values, target)).rowcount == 1)
                check(name + '-references', not list(db.execute('PRAGMA foreign_key_check')))
                db.execute('UPDATE content_digest SET sha256=?', (module.logical_hash(db),))
            run(name, ['dotnet', args.linker.resolve(), 'verify', damaged], rejected)
        mutate('accept-raw-equivalent', 'codec=?,bytes=?', ('raw', contents['large.h']), False)
        mutate('accept-zlib-level1-equivalent', 'bytes=?', (zlib.compress(contents['large.h'], 1),), False)
        mutations = {
            'reject-codec': ('codec=?', ('invented',)),
            'reject-negative-length': ('decoded_byte_length=?', (-1,)),
            'reject-unbounded-length': ('decoded_byte_length=?', (2147483648,)),
            'reject-short-decoded-length': ('decoded_byte_length=?', (length - 1,)),
            'reject-long-decoded-length': ('decoded_byte_length=?', (length + 1,)),
            'reject-raw-length': ('codec=?', ('raw',)),
            'reject-decoded-hash': ('sha256=?', ('0' * 64,)),
            'reject-truncated-checksum': ('bytes=?', (encoded[:-1],)),
            'reject-truncated-stream': ('bytes=?', (encoded[:-6],)),
            'reject-trailing-byte': ('bytes=?', (encoded + b'\0',)),
            'reject-concatenated-stream': ('bytes=?', (encoded + encoded,)),
            'reject-corrupt-checksum': ('bytes=?', (encoded[:-1] + bytes([encoded[-1] ^ 1]),)),
            'reject-header': ('bytes=?', (b'\0\0' + encoded[2:],)),
            'reject-unavailable-with-payload': ('available=?', (0,)),
        }
        compressor = zlib.compressobj(level=9, zdict=b'abcdefghij ')
        dictionary_data = compressor.compress(contents['large.h']) + compressor.flush()
        mutations['reject-external-dictionary'] = ('bytes=?', (dictionary_data,))
        for name,(sql,values) in mutations.items(): mutate(name,sql,values)
    except Exception as exc:
        error = str(exc)
    result = dict(status='passed' if error is None else 'failed', error=error, checks=checks, checkCount=len(checks), commands=commands)
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({k:result[k] for k in ('status','error','checkCount')}))
    return int(error is not None)

if __name__ == '__main__': raise SystemExit(main())
