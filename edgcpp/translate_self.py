#!/usr/bin/env python3
"""Translate the original all-extensions cpfe C++ sources for C# review.

This intentionally uses the cpfe compilation database, not the compiler's
previously lowered C output. Host-header mode is diagnostic: it does not claim
that Linux libc layouts or external operations have portable managed bindings.
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
    parser.add_argument('--profile', choices=('portable', 'host'), default='portable')
    parser.add_argument('--split', choices=('source', 'size', 'function', 'none'), default='source')
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--compiler', type=Path, default=ROOT / 'build/bin/cpfe-dot')
    parser.add_argument('--unbound-externals', choices=('reject', 'declarations'), default='reject')
    args = parser.parse_args()
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
    exceptions, rtti = next(iter(feature_sets))
    feature_flags = ['--exceptions' if exceptions else '--no-exceptions',
                     '--rtti' if rtti else '--no-rtti']
    sources = sorted((Path(entry['file']) for entry in entries),
                     key=lambda source: (source.name != 'cfe.c', str(source)))
    includes = []
    if args.profile == 'host':
        compiler = shlex.split(entries[0]['command'])[0]
        search = subprocess.run([compiler, '-E', '-x', 'c++', '-', '-v'], input='',
                                text=True, capture_output=True, check=True).stderr
        includes = [line.strip() for line in search.split('#include <...> search starts here:')[1]
                    .split('End of search list.')[0].splitlines() if line.strip()]
    command = [sys.executable, str(ROOT / 'dotnet.py'), *map(str, sources),
               '--language', 'c++', '--std', 'c++14', *flags,
               *feature_flags,
               *[value for path in includes for value in ('-I', path)],
               '--target', 'legacy-linux-x64' if args.profile == 'host' else 'portable-lp64-binary80-v2',
               '--libc', 'none' if args.profile == 'host' else 'dotcc',
               '--emit', 'managedlib', '--namespace', 'Edg.Compiler', '--class-name', 'EdgCompiler',
               '--compiler', str(args.compiler.resolve()),
               '--unbound-externals', args.unbound_externals,
               '--split', args.split, '--output-dir', str(out / 'managed')]
    report = {
        'scope': f'Original {len(sources)}-TU all-extensions cpfe compiler, directly translated as C++14; no lowered-C fallback',
        'profile': args.profile,
        'profileLimitations': ('Native Linux host headers and target; external ABI/runtime portability is unverified'
                              if args.profile == 'host' else 'Portable managed libc headers; EDG host OS facilities may be missing'),
        'compileDatabaseSha256': digest(database),
        'compiler': str(args.compiler.resolve()),
        'compilerSha256': digest(args.compiler.resolve()),
        'generatedConfigSha256': digest(ROOT / 'build/src/includes/cmake_defines.h'),
        'sources': [{'path': str(path.relative_to(ROOT)), 'sha256': digest(path)} for path in sources],
        'sourceCount': len(sources), 'definesAndIncludes': flags,
        'languageFeatures': {'exceptions': exceptions, 'rtti': rtti},
        'hostIncludeDirectories': includes, 'commands': [],
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
