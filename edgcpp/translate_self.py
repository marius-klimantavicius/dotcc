#!/usr/bin/env python3
"""Translate the original all-extensions cpfe C++ sources for C# review.

This intentionally uses the cpfe compilation database, not the compiler's
previously lowered C output. Host-header mode is diagnostic: it does not claim
that Linux libc layouts or external operations have portable managed bindings.
Musl mode uses the selected portable provider and pinned libc++ headers, with
explicit unresolved contracts for missing host and C++ runtime facilities.
"""
import argparse
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
    parser.add_argument('--profile', choices=('portable', 'host', 'musl'), default='portable')
    parser.add_argument('--libcxx-acquisition', type=Path,
                        help='pinned libc++ acquisition directory, required for the musl profile')
    parser.add_argument('--split', choices=('source', 'size', 'function', 'none'), default='source')
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--unbound-externals', choices=('reject', 'declarations', 'partial'), default='reject')
    parser.add_argument('--runtime-source', action='append', type=Path, default=[],
                        help='C# runtime implementation file to include (repeatable)')
    parser.add_argument('--exceptions', action=argparse.BooleanOptionalAction, default=None,
                        help='override exception handling (default: match native compilation database)')
    args = parser.parse_args()
    if args.profile == 'musl' and not args.libcxx_acquisition:
        parser.error('--profile musl requires --libcxx-acquisition')
    if args.libcxx_acquisition and args.profile != 'musl':
        parser.error('--libcxx-acquisition is only used by --profile musl')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    database = ROOT / 'build/compile_commands.json'
    entries = [entry for entry in json.loads(database.read_text())
               if '/cpfe.dir/' in entry['command']]
    if not entries:
        raise RuntimeError('No original cpfe target in compilation database')
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
    exceptions = native_exceptions if args.exceptions is None else args.exceptions
    feature_flags = ['--exceptions' if exceptions else '--no-exceptions',
                     '--rtti' if rtti else '--no-rtti']
    sources = sorted((Path(entry['file']) for entry in entries),
                     key=lambda source: (source.name != 'cfe.c', str(source)))
    includes = []
    profile_flags = []
    libcxx = None
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
        includes = [str(ROOT / 'backend/self/include'), str(acquisition / 'config'),
                    str(acquisition / pin['archiveRoot'] / 'include')]
        # EDG's supported malloc/read-write fallback retains language features.
        # Portable CLI metadata can still request its separate read-only mapping.
        profile_flags = ['-DUSE_MMAP_FOR_MEMORY_REGIONS=0']
        libcxx = {'acquisition': str(manifest_path), 'sha256': digest(manifest_path),
                  'source': manifest['source'], 'headerHashes': header_hashes,
                  'implementation': 'headers only; unresolved C++ routines remain external contracts'}
    command = [sys.executable, str(ROOT / 'dotnet.py'), *map(str, sources),
               '--language', 'c++', '--std', 'c++14', *flags, *profile_flags,
               *feature_flags,
               *[value for path in includes for value in ('-I', path)],
               '--target', 'legacy-linux-x64' if args.profile == 'host' else 'portable-lp64-binary80-v2',
               '--libc', 'none' if args.profile == 'host' else 'musl' if args.profile == 'musl' else 'dotcc',
               '--emit', 'managedlib', '--namespace', 'Edg.Compiler', '--class-name', 'EdgCompiler',
               '--compiler', str(args.compiler.resolve()),
               '--unbound-externals', args.unbound_externals,
               *[value for path in args.runtime_source for value in ('--runtime-source', str(path.resolve()))],
               '--raw-output', str(out / 'raw.cs'),
               '--split', args.split, '--output-dir', str(out / 'managed')]
    report = {
        'scope': f'Original {len(sources)}-TU all-extensions cpfe compiler, directly translated as C++14; no lowered-C fallback',
        'profile': args.profile,
        'profileLimitations': ('Native Linux host headers and target; external ABI/runtime portability is unverified'
                              if args.profile == 'host' else 'Portable managed libc headers; unresolved host OS and C++ facilities remain explicit contracts'),
        'profileOverrides': profile_flags, 'libcxx': libcxx,
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
    if not code and args.build:
        code = run('build', ['dotnet', 'build', str(out / 'managed/Generated.csproj'),
                            '-c', 'Release', '--nologo'])
    return code


if __name__ == '__main__':
    raise SystemExit(main())
