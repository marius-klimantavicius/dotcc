#!/usr/bin/env python3
"""Translate the all-extensions cpfe or cpfe-dot C++ sources into C#.

This intentionally uses the cpfe compilation database, not the compiler's
previously lowered C output. Host-header mode is diagnostic: it does not claim
that Linux libc layouts or external operations have portable managed bindings.
Musl mode uses the portable POSIX provider and pinned libc++ implementation.
Unresolved runtime contracts are rejected by default.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import resource
from pathlib import Path
import shlex
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parent


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--program', choices=('cpfe', 'cpfe-dot'), default='cpfe',
                        help='include the C backend (cpfe) or our C# backend (cpfe-dot)')
    parser.add_argument('--source-date-epoch', type=int, default=0,
                        help='fixed UTC build timestamp for reproducible self-translation (default Unix epoch)')
    parser.add_argument('--profile', choices=('portable', 'host', 'musl'), default='portable')
    parser.add_argument('--libcxx-acquisition', type=Path,
                        help='pinned libc++ acquisition directory, required for the musl profile')
    parser.add_argument('--split', choices=('source', 'size', 'function', 'none'), default='source')
    parser.add_argument('--emit', choices=('managedlib', 'exe'), default='exe',
                        help='executable with public Run entrypoint (default), or export-everything library')
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--unbound-externals', choices=('reject', 'declarations', 'partial'), default='reject')
    parser.add_argument('--runtime-source', action='append', type=Path, default=[],
                        help='C# runtime implementation file to include (repeatable)')
    parser.add_argument('--exceptions', action=argparse.BooleanOptionalAction, default=None,
                        help='override exception handling (musl runtime requires exceptions; other profiles match native flags)')
    args = parser.parse_args()
    if args.profile == 'musl' and not args.libcxx_acquisition:
        parser.error('--profile musl requires --libcxx-acquisition')
    if args.libcxx_acquisition and args.profile != 'musl':
        parser.error('--libcxx-acquisition is only used by --profile musl')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    database = ROOT / 'build/compile_commands.json'
    entries = [entry for entry in json.loads(database.read_text())
               if '/' + args.program + '.dir/' in entry['command']]
    if not entries:
        raise RuntimeError('No ' + args.program + ' target in compilation database')
    flag_sets = {tuple(flag for flag in shlex.split(entry['command'])
                       if flag.startswith(('-D', '-I'))) for entry in entries}
    if len(flag_sets) != 1:
        raise RuntimeError('Combined translation requires identical input flags')
    flags = list(next(iter(flag_sets)))
    feature_sets = {('-fno-exceptions' not in shlex.split(entry['command']),
                     '-fno-rtti' not in shlex.split(entry['command'])) for entry in entries}
    if len(feature_sets) != 1:
        raise RuntimeError('Combined translation requires identical EH/RTTI feature settings')
    native_exceptions, rtti = next(iter(feature_sets))
    exceptions = (args.profile == 'musl' or native_exceptions) if args.exceptions is None else args.exceptions
    if args.profile == 'musl' and not exceptions:
        parser.error('the translated libc++ runtime requires exceptions; omit --no-exceptions')
    feature_flags = ['--exceptions' if exceptions else '--no-exceptions',
                     '--rtti' if rtti else '--no-rtti']
    sources = sorted((Path(entry['file']) for entry in entries),
                     key=lambda source: (source.name != 'cfe.c', str(source)))
    includes = []
    profile_flags = []
    libcxx = None
    translation_sources = sources
    standard = 'c++17' if args.program == 'cpfe-dot' else 'c++14'
    stamp = datetime.fromtimestamp(args.source_date_epoch, timezone.utc)
    month = ('Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec')[stamp.month - 1]
    build_date = f'{month} {stamp.day:2d} {stamp.year:04d}'
    build_time = stamp.strftime('%H:%M:%S')
    timestamp_flags = [f'-D__DATE__="{build_date}"', f'-D__TIME__="{build_time}"']
    if args.profile == 'host':
        compiler = shlex.split(entries[0]['command'])[0]
        search = subprocess.run([compiler, '-E', '-x', 'c++', '-', '-v'], input='',
                                text=True, capture_output=True, check=True).stderr
        includes = [line.strip() for line in search.split('#include <...> search starts here:')[1]
                    .split('End of search list.')[0].splitlines() if line.strip()]
    elif args.profile == 'musl':
        acquisition = args.libcxx_acquisition.resolve()
        manifest_path = acquisition / 'acquisition.json'
        manifest = json.loads(manifest_path.read_text())
        pin = json.loads((ROOT / 'backend/runtime/libcxx/source.json').read_text())
        if manifest['source']['sha256'] != pin['sha256']:
            raise RuntimeError('libc++ acquisition does not match the pinned source archive')
        # Validate the exact headers/configuration used, including local vendor
        # adaptations recorded by acquire.py. No native C++ implementation is linked.
        header_hashes = {}
        for name, expected in manifest['fileHashes'].items():
            if name.startswith(('config/', pin['archiveRoot'] + '/include/')):
                actual = digest(acquisition / name)
                if actual != expected:
                    raise RuntimeError('libc++ acquisition header changed: ' + name)
                header_hashes[name] = actual
        if not header_hashes:
            raise RuntimeError('libc++ acquisition has no recorded headers')
        includes = [str(ROOT / 'backend/runtime/posix/include'), str(ROOT / 'backend/self/include'), str(acquisition / 'config'),
                    str(acquisition / pin['archiveRoot'] / 'include')]
        # EDG's supported malloc/read-write fallback retains language features.
        # Portable CLI metadata can still request its separate read-only mapping.
        profile_flags = ['-DUSE_MMAP_FOR_MEMORY_REGIONS=0']
        from backend.self.libcxx import prepare_provider
        provider = out / 'libcxx-provider.cpp'
        dot_backend = next((p for p in sources if p.name == 'dot_gen_be.c'), None)
        implementation = prepare_provider(ROOT, acquisition, provider, dot_backend=dot_backend)
        # EDG's whole-program primary TU must own the implementation classes.
        # Keep the original compiler files separate; do not construct a unity
        # build with colliding file-local compiler declarations.
        translation_sources = [provider, *(p for p in sources if p != dot_backend)]
        libcxx = {'acquisition': str(manifest_path), 'sha256': digest(manifest_path),
                  'source': manifest['source'], 'headerHashes': header_hashes,
                  'implementation': implementation}
    if args.profile != 'host':
        from backend.self.profile import portable_self_flags
        profile_flags += portable_self_flags()
    command = [sys.executable, str(ROOT / 'dotnet.py'), *map(str, translation_sources),
               '--language', 'c++', '--std', standard, *flags, *profile_flags, *timestamp_flags,
               *feature_flags,
               *[value for path in includes for value in ('-I', path)],
               '--target', 'legacy-linux-x64' if args.profile == 'host' else 'portable-lp64-binary80-v2',
               '--libc', 'none' if args.profile == 'host' else 'musl' if args.profile == 'musl' else 'dotcc',
               *(['--posix'] if args.profile == 'musl' else []),
               '--emit', args.emit, '--namespace', 'Edg.Compiler', '--class-name', 'EdgCompiler',
               '--compiler', str(args.compiler.resolve()),
               '--unbound-externals', args.unbound_externals,
               *[value for path in args.runtime_source for value in ('--runtime-source', str(path.resolve()))],
               '--raw-output', str(out / 'raw.cs'),
               '--frontend-invocation', str(out / 'frontend-invocation.json'),
               '--split', args.split, '--output-dir', str(out / 'managed')]
    report = {
        'scope': f'{len(sources)}-source all-extensions {args.program} compiler, directly translated as {standard}; no lowered-C fallback',
        'program': args.program, 'standard': standard,
        'buildTimestamp': {'sourceDateEpoch': args.source_date_epoch, 'date': build_date, 'time': build_time,
                           'definitions': timestamp_flags},
        'profile': args.profile, 'emit': args.emit,
        'profileLimitations': ('Native Linux host headers and target; external ABI/runtime portability is unverified'
                              if args.profile == 'host' else 'Portable managed runtime; cross-platform execution is validated separately'),
        'profileOverrides': profile_flags, 'libcxx': libcxx,
        'portableHostProfile': ({'path': 'backend/self/profile.py',
                                 'sha256': digest(ROOT / 'backend/self/profile.py')}
                                if args.profile != 'host' else None),
        'portableHostHeaders': ({str(p.relative_to(ROOT)): digest(p)
                                for p in sorted((ROOT / 'backend/self/include').rglob('*')) if p.is_file()}
                               if args.profile == 'musl' else {}),
        'compileDatabaseSha256': digest(database),
        'compiler': str(args.compiler.resolve()),
        'compilerSha256': digest(args.compiler.resolve()),
        'generatedConfigSha256': digest(ROOT / 'build/src/includes/cmake_defines.h'),
        'sources': [{'path': str(path.relative_to(ROOT)), 'sha256': digest(path)} for path in sources],
        'sourceCount': len(sources), 'definesAndIncludes': flags,
        'languageFeatures': {'exceptions': exceptions, 'rtti': rtti},
        'nativeLanguageFeatures': {'exceptions': native_exceptions, 'rtti': rtti},
        'exceptionOverride': args.exceptions,
        'hostIncludeDirectories': includes if args.profile == 'host' else [],
        'profileIncludeDirectories': includes, 'commands': [],
    }
    def run(label, invocation):
        (out / (label + '.command.json')).write_text(json.dumps(invocation, indent=2) + '\n')
        start = time.monotonic()
        with (out / (label + '.stdout.log')).open('w') as stdout, (out / (label + '.stderr.log')).open('w') as stderr:
            result = subprocess.run(invocation, cwd=out, stdout=stdout, stderr=stderr)
        report['commands'].append({'label': label, 'command': invocation,
                                   'exitCode': result.returncode,
                                   'seconds': round(time.monotonic() - start, 3)})
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        print(f'{label}: exit {result.returncode}', flush=True)
        return result.returncode
    (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    code = run('translate', command)
    if not code and args.profile == 'musl':
        license_file = acquisition / pin['archiveRoot'] / 'LICENSE.TXT'
        target = out / 'managed/Runtime.Libcxx.LICENSE.txt'
        target.write_bytes(license_file.read_bytes())
        report['libcxx']['license'] = {'file': str(target), 'sha256': digest(target)}
        (out / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
    if not code and args.build:
        code = run('build', [sys.executable, str(ROOT / 'build_managed.py'),
                            str(out / 'managed/Generated.csproj'), '--output-dir', str(out / 'build')])
    return code


if __name__ == '__main__':
    raise SystemExit(main())
