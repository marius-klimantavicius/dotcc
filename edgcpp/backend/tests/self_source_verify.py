#!/usr/bin/env python3
"""Bounded native/translated EDG parser parity on original compiler sources.

Both compilers receive identical explicit portable input-target macros and the
self-translation's header/configuration flags. This is semantic parsing of real
source TUs; it is not an assertion that implicit host-dependent predefines match.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--profile-results', type=Path, required=True, help='translate_self.py results.json')
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--native-compiler', type=Path, default=ROOT / 'build/bin/cpfe')
    parser.add_argument('--managed-command', nargs='+')
    parser.add_argument('--native-only', action='store_true')
    parser.add_argument('--source', action='append', help='Original EDG source basename (repeatable)')
    parser.add_argument('--timeout', type=int, default=180)
    args = parser.parse_args()
    if not args.native_only and not args.managed_command:
        parser.error('--managed-command required unless --native-only is selected')
    out = args.output_dir.resolve(); out.mkdir(parents=True, exist_ok=False)
    profile = json.loads(args.profile_results.read_text())
    if profile['profile'] != 'musl':
        parser.error('This test requires the portable musl self profile')
    definition_file = ROOT / 'backend/target/portable-lp64-binary80-v2.def'
    # Native cpfe protects its system macros from command-line -U. GNU source
    # #undef is explicitly supported by preproc.c and permits the same input
    # profile to be supplied to native and managed front ends without changing
    # either compiler or leaking native OS macros into portable libc++ headers.
    lines = ['// Explicit portable source-input profile for both compiler oracles.']
    removed = ['__linux__', '__x86_64', '__x86_64__', '__unix__', 'unix']
    for macro in removed:
        lines.append('#undef ' + macro)
    macros = re.findall(r'^DOT_TARGET_MACRO\((\w+), (".*")\)$', definition_file.read_text(), re.M)
    for name, literal in macros:
        lines += ['#undef ' + name, '#define ' + name + ' ' + json.loads(literal)]
    preinclude = out / 'portable-input.h'; preinclude.write_text('\n'.join(lines) + '\n')
    includes = [*profile['profileIncludeDirectories'], str(ROOT / 'backend/runtime/libc/include')]
    flags = ['--clear_flag', 'use_predefined_macro_file', '--c++14', '--gnu_version', '130300',
        '--no_rtti', '--no_code_gen', '--preinclude', str(preinclude),
        *profile['definesAndIncludes'], *profile['profileOverrides'],
        *[arg for directory in includes for arg in ['-I', directory]]]
    report = {'scope': __doc__, 'nativeOnly': args.native_only,
        'profileResults': str(args.profile_results.resolve()),
        'profileResultsSha256': hashlib.sha256(args.profile_results.read_bytes()).hexdigest(),
        'nativeCompiler': str(args.native_compiler.resolve()),
        'managedCommand': args.managed_command,
        'explicitInputTarget': {'definitionFile': str(definition_file),
            'definitionSha256': hashlib.sha256(definition_file.read_bytes()).hexdigest(),
            'preincludeSha256': hashlib.sha256(preinclude.read_bytes()).hexdigest(),
            'removedImplicitNativeMacros': removed, 'targetMacroCount': len(macros)},
        'normalization': ['CRLF to LF', 'per-run working directory paths'],
        'commands': [], 'cases': []}
    commands = {'native': [str(args.native_compiler.resolve())]}
    if not args.native_only:
        commands['managed'] = args.managed_command

    def save():
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')

    for basename in args.source or ['cfe.c', 'types.c', 'ifc_modules_read.c']:
        source = ROOT / 'upstream/src' / basename
        if source.parent != ROOT / 'upstream/src' or not source.is_file():
            parser.error('Expected an existing original EDG source basename: ' + basename)
        entry = {'source': str(source), 'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(), 'passed': True}
        report['cases'].append(entry)
        outputs = {}
        for label, prefix in commands.items():
            directory = out / source.stem / label; directory.mkdir(parents=True)
            command = [*prefix, *flags, str(source)]
            start = time.monotonic()
            try:
                result = subprocess.run(command, cwd=directory, text=True, capture_output=True, timeout=args.timeout)
            except subprocess.TimeoutExpired as error:
                result = subprocess.CompletedProcess(command, -999,
                    (error.stdout or b'').decode(errors='replace') if isinstance(error.stdout, bytes) else error.stdout or '',
                    (error.stderr or b'').decode(errors='replace') if isinstance(error.stderr, bytes) else error.stderr or '')
            (directory / 'stdout.log').write_text(result.stdout)
            (directory / 'stderr.log').write_text(result.stderr)
            report['commands'].append({'label': label, 'source': basename, 'command': command, 'cwd': str(directory),
                'exitCode': result.returncode, 'seconds': round(time.monotonic() - start, 3)})
            outputs[label] = (result.returncode, result.stdout.replace('\r\n','\n').replace(str(directory),'<run>'),
                result.stderr.replace('\r\n','\n').replace(str(directory),'<run>'))
            entry['passed'] &= result.returncode == 0
            save()
        if 'managed' in outputs:
            entry['diagnosticParity'] = outputs['native'] == outputs['managed']
            entry['passed'] &= entry['diagnosticParity']
            if not entry['diagnosticParity']:
                import difflib
                difference = ''.join(difflib.unified_diff(
                    (outputs['native'][1]+outputs['native'][2]).splitlines(keepends=True),
                    (outputs['managed'][1]+outputs['managed'][2]).splitlines(keepends=True),
                    fromfile='native', tofile='managed'))
                (out / source.stem / 'diagnostics.diff').write_text(difference)
        save(); print(f"{basename}: {'passed' if entry['passed'] else 'FAILED'}", flush=True)
    report['passed'] = all(case['passed'] for case in report['cases'])
    save()
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
