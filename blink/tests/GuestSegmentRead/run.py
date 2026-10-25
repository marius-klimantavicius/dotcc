#!/usr/bin/env python3
"""Native Linux ordinary user-segment read fixture; no shared product build."""
import ast
import json
from pathlib import Path
import shutil
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(HERE.parent / 'CoreClrRuntime'))
from evidence import Evidence, sha
sys.path.insert(0, str(ROOT.parent / 'Scripts'))
from production_support import locked_command

EXPECTED = b'guest-segment-read: selectors, registers, word stores, cross-page: PASS\n'

def main():
    ev = Evidence('segment-read')
    ev.value.update(kind='ordinary-guest-segment-read', managed_passed=False,
        expected_stdout_hex=EXPECTED.hex(),
        scope='Valid Linux user selector reads, register widths, exact memory word stores and cross-page access')
    try:
        for source in (Path(__file__), HERE / 'fixture.S', ROOT / 'scripts/cpu_bindings.py'):
            ev.capture(source, ev.directory / source.name)
        ast.parse((ROOT / 'scripts/cpu_bindings.py').read_text())
        linker = ev.directory / 'fixture.ld'
        ev.capture(HERE.parent / 'GuestThreads/fixture.ld', linker)
        for name in ('as', 'ld', 'readelf'):
            ev.pin(shutil.which(name))
        def run(label, args):
            return ev.run(label, locked_command(list(map(str, args)), env=ev.environment))
        run('assemble', ['as', '--64', ev.directory / 'fixture.S', '-o', ev.directory / 'fixture.o'])
        image = ev.directory / 'guest-segment-read.elf'
        run('link', ['ld', '-static', '-z', 'noexecstack', '-z', 'max-page-size=4096', '-T', linker,
            ev.directory / 'fixture.o', '-o', image])
        ev.pin(image)
        identity, error = run('elf-identity', ['readelf', '-h', '-lW', '-dW', image])
        if error or b'INTERP' in identity or b'(NEEDED)' in identity or b'EXEC (Executable file)' not in identity:
            raise RuntimeError('Expected normal static Linux x86-64 ELF')
        out, error = run('linux', [image])
        if out != EXPECTED or error:
            raise RuntimeError('Native user-segment contract differs')
        ev.value['elf'] = dict(path=str(image), sha256=sha(image))
        ev.check()
        ev.value['passed'] = True
    except BaseException as error:
        ev.value['error'] = type(error).__name__ + ': ' + str(error)
    return ev.finish()

if __name__ == '__main__':
    raise SystemExit(main())
