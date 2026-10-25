#!/usr/bin/env python3
"""Build and natively run valid ELF fixtures and the actual argv.c invariant.

No managed product is built. The retained loader/libc and fixture DSO are the
explicit guest image; no fixture input is downloaded.
"""
import json
import re
import shutil
import sys
from pathlib import Path
from evidence import Evidence, HERE, ROOT, module, sha


def main():
    evidence = Evidence('native')
    receipt = evidence.value
    receipt['managed_execution'] = False
    pin, run = evidence.pin, evidence.run
    try:
        for path in (Path(__file__), HERE / 'evidence.py', HERE / 'entry.S', HERE / 'baseline.S', HERE / 'runtime.c',
                     HERE / 'library.c', HERE / 'entry-register.c', HERE.parent / 'GuestFileMapping/workload.c'):
            pin(path)
        cc, readelf, objdump = [pin(shutil.which(name)) for name in ('cc', 'readelf', 'objdump')]
        run('cc-version', [cc, '--version'])
        stage_path = pin(ROOT / 'src/Host/scripts/stage_elf_entry.py')
        stage = module(stage_path, 'elf_entry_stage')
        source_pin = json.loads(pin(ROOT / 'config/source-manifest.json').read_text())['upstream']
        original = pin(ROOT / 'ref' / source_pin['directory'] / 'blink/argv.c')
        adapted = stage.adapt(original.read_bytes())
        body = adapted[adapted.index(b'#define STACKALIGN'):]
        (evidence.directory / 'adapted-argv.inc').write_bytes(body)
        pin(evidence.directory / 'adapted-argv.inc')
        run('entry-register-build', [cc, '-std=c17', '-Wall', '-Wextra', '-Werror', '-Wno-unused-parameter', '-O2', '-I', evidence.directory,
            HERE / 'entry-register.c', '-o', evidence.directory / 'entry-register'])
        output, errors = run('entry-register', [pin(evidence.directory / 'entry-register')])
        if output != b'entry-register Linux=zero APE=program-string flat=program-string OK\n' or errors:
            raise RuntimeError('Adapted LoadArgv native invariant differs')
        receipt['entry_register'] = dict(upstream=str(original), upstream_sha256=sha(original),
            helper_sha256=sha(stage_path), adapted_sha256=sha(evidence.directory / 'adapted-argv.inc'),
            linux_zero=True, ape_program_string=True, flat_program_string=True)
        images = evidence.directory / 'images'
        binary, libraries = images / 'bin', images / 'lib'
        binary.mkdir(parents=True); libraries.mkdir()
        options = ['-O2', '-g0', '-march=x86-64', '-mtune=generic', '-Wall', '-Wextra', '-Werror']
        run('dso-build', [cc, *options, '-fPIC', '-shared', HERE / 'library.c', '-Wl,-soname,libelf-runtime.so',
            '-o', libraries / 'libelf-runtime.so'])
        builds = [('baseline-static', ['-nostdlib', '-static', '-no-pie'], 'baseline.S'),
                  ('mapping-static', ['-static', '-fno-pie', '-no-pie'], HERE.parent / 'GuestFileMapping/workload.c'),
                  ('entry-static', ['-nostdlib', '-static', '-no-pie'], 'entry.S'),
                  ('entry-static-pie', ['-nostdlib', '-static-pie'], 'entry.S'),
                  ('glibc-static', ['-static', '-fno-pie', '-no-pie'], 'runtime.c'),
                  ('glibc-static-pie', ['-static-pie'], 'runtime.c'),
                  ('glibc-dynamic', ['-fPIE', '-pie', '-DDYNAMIC_FIXTURE', '-L', libraries,
                      '-Wl,-rpath,$ORIGIN/../lib', '-lelf-runtime'], 'runtime.c')]
        receipt['fixtures'] = []
        for name, flags, source in builds:
            image = binary / name
            # Libraries follow the source for --as-needed linkers.
            run(name + '-build', [cc, *options, HERE / source, *flags, '-o', image])
            pin(image)
            elf, errors = run(name + '-readelf', [readelf, '-h', '-lW', '-dW', '-rW', image])
            text = elf.decode()
            if errors or 'Advanced Micro Devices X86-64' not in text:
                raise RuntimeError('Not a Linux x86-64 ELF: ' + name)
            dynamic = name == 'glibc-dynamic'
            if ('INTERP' in text) != dynamic or ('(NEEDED)' in text) != dynamic:
                raise RuntimeError('ELF linking form differs: ' + name)
            if ('DYN (Shared object file)' in text or 'DYN (Position-Independent Executable file)' in text) != ('pie' in name or dynamic):
                raise RuntimeError('ELF type differs: ' + name)
            if name.startswith('glibc-') and 'TLS' not in text:
                raise RuntimeError('Runtime fixture lost PT_TLS')
            if name == 'glibc-static-pie' and 'R_X86_64_RELATIVE' not in text:
                raise RuntimeError('Static PIE has no relative relocations')
            if name.startswith('glibc-'):
                code, errors = run(name + '-main-code', [objdump, '-d', '--disassemble=main', image])
                assembly = code.decode()
                if errors or not re.search(r'\bmov\w*\s+%fs:', assembly) or not re.search(r'\bmov\w*\s+[^,\n]+,%fs:', assembly):
                    raise RuntimeError('main lost actual FS-relative TLS reads/writes: ' + name)
            row = dict(name=name, path=str(image), sha256=sha(image), interpreter=None,
                       readelf=str(evidence.directory / (name + '-readelf.stdout')))
            if dynamic:
                interpreter = re.search(r'Requesting program interpreter: ([^\]]+)\]', text).group(1)
                if not interpreter.startswith('/') or '..' in interpreter.split('/'):
                    raise RuntimeError('Unexpected interpreter path')
                copied = images / interpreter.lstrip('/')
                copied.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(pin(interpreter), copied); copied.chmod(0o755); pin(copied)
                libc = Path(run('libc-path', [cc, '-print-file-name=libc.so.6'])[0].decode().strip()).resolve()
                shutil.copyfile(pin(libc), libraries / 'libc.so.6'); pin(libraries / 'libc.so.6')
                row['interpreter'] = interpreter
                command = [copied, '--inhibit-cache', '--library-path', libraries, image, 'fixture-argument']
            else:
                command = [image, 'fixture-argument']
            work = evidence.directory / ('work-' + name); work.mkdir(); (work / 'input.txt').write_bytes(b'input')
            stdout, stderr = run(name + '-native', command, cwd=work, timeout=15)
            expected = (b'guest-private-mapping OK bytes tails protection bss fixed noreplace discard close\n'
                if name == 'mapping-static' else b'elf-baseline OK\n' if name == 'baseline-static'
                else b'elf-entry OK\n' if name.startswith('entry-') else (
                'elf-runtime tls=13 tls-bss=17 data=43 bss=OK file=input dso=' + ('61' if dynamic else '0') +
                '\nelf-atexit OK\nelf-destructor OK\n').encode())
            if stdout != expected or stderr != (b'elf-stderr OK\n' if name.startswith('glibc-') else b''):
                raise RuntimeError('Native output differs: ' + name)
            row.update(stdout=stdout.decode(), stderr=stderr.decode(), exit_code=0)
            if name.startswith('glibc-'):
                if (work / 'output.txt').read_bytes() != b'file-write OK\n': raise RuntimeError('Native file differs')
                row['written'] = (work / 'output.txt').read_text()
                pin(work / 'output.txt')
            receipt['fixtures'].append(row)
        dso, errors = run('dso-readelf', [readelf, '-lW', '-rW', libraries / 'libelf-runtime.so'])
        if errors or b'TLS' not in dso or b'R_X86_64_DTPMOD64' not in dso or b'R_X86_64_DTPOFF64' not in dso:
            raise RuntimeError('DSO lost dynamic TLS relocation')
        code, errors = run('dso-tls-code', [objdump, '-d', '--disassemble=elf_dso_touch', libraries / 'libelf-runtime.so'])
        if errors or b'__tls_get_addr@plt' not in code:
            raise RuntimeError('DSO lost actual dynamic TLS access')
        pin(libraries / 'libelf-runtime.so')
        receipt['images'] = str(images)
        receipt['image_files'] = {str(p.relative_to(images)): sha(pin(p)) for p in images.rglob('*') if p.is_file()}
        evidence.check(); receipt['passed'] = True
    except BaseException as error:
        receipt['error'] = f'{type(error).__name__}: {error}'
    return evidence.finish()


if __name__ == '__main__':
    raise SystemExit(main())
