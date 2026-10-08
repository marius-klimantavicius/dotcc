#!/usr/bin/env python3
"""Prepare original-source SQLite/DuckDB manifests for project.py.

These source manifests do not claim translated execution. Shared runtime linking
and upstream test-runner validation remain separate acceptance steps.
"""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
EDG = HERE.parent
REPO = EDG.parent


def module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def unit(source, standard, arguments, role, key=None):
    return dict(id=key or str(source), source=str(source), standard=standard,
                arguments=arguments, role=role)


def libc_arguments():
    return [value for path in (EDG / 'backend/runtime/posix/include', EDG / 'backend/runtime/libc/include')
            for value in ('-I', str(path))]


def musl_units():
    sources, receipt = module(EDG / 'backend/musl_provider.py', 'dotobj_musl').prepare_musl_provider(EDG)
    units = [unit(p, 'c11', libc_arguments(), 'libc', 'musl:' + str(p.relative_to(receipt['snapshot'])))
             for p in sources]
    return units, receipt


def sqlite(out, with_musl):
    preparation = module(EDG / 'sqlite_host.py', 'dotobj_sqlite_host')
    source, definitions, macros = preparation.prepare(out, 'unix')
    arguments = libc_arguments() + ['-I', str(source.parent)] + ['-D' + d for d in definitions]
    result = dict(schema=1, project='sqlite', sourcePreparation='sqlite_host.prepare(unix)',
                  sourcePreparationSha256=sha(EDG / 'sqlite_host.py'),
                  threadsafe=True, nativeExtensionLoading=False, macroExportPattern=macros,
                  units=[unit(source, 'c11', arguments, 'library', 'sqlite:sqlite3.c')],
                  libcSourcesIncluded=with_musl, runtimeBindingVerified=False)
    if with_musl:
        units, receipt = musl_units()
        result['units'] += units
        result['libcProvider'] = receipt
    return result


def duckdb(portable_path, acquisition, with_musl, include_tests):
    portable = json.loads(portable_path.read_text())
    module(REPO / 'duckdb/edg/portable.py', 'dotobj_portable').verify_staged(portable)
    acquired = json.loads((acquisition / 'acquisition.json').read_text())
    for name, digest in acquired['fileHashes'].items():
        if name.startswith(('config/', acquired['source']['archiveRoot'] + '/include/')) and sha(acquisition / name) != digest:
            raise ValueError('Acquired libc++ input changed: ' + name)
    includes = [acquisition / 'config', acquisition / acquired['source']['archiveRoot'] / 'include',
                EDG / 'backend/runtime/libcxx/include']
    shared = [v for p in includes for v in ('-I', str(p))] + libc_arguments()
    preflight = module(REPO / 'duckdb/edg/preflight.py', 'dotobj_preflight')
    selected, seen = [], set()
    root = Path(portable['stagedSource'])
    for original in portable['units']:
        if original['role'] != 'library' and not (include_tests and original['role'] == 'test'):
            continue
        arguments, _ = preflight.frontend_flags(original)
        source = Path(original['file'])
        identity = (str(source), tuple(arguments))
        if identity in seen:
            continue
        seen.add(identity)
        key = str(source.relative_to(root)) if source.is_relative_to(root) else str(source)
        key += ':' + hashlib.sha256(json.dumps(arguments).encode()).hexdigest()[:16]
        if original['role'] == 'library' and '-DDUCKDB_DISABLE_EXTENSION_LOAD' not in arguments:
            # Keep the configured project invariant explicit across all units.
            arguments.append('-DDUCKDB_DISABLE_EXTENSION_LOAD')
        standard = 'c11' if original['language'] == 'c' else 'c++17'
        options = libc_arguments() if standard == 'c11' else shared
        selected.append(unit(source, standard, options + arguments, original['role'], 'duckdb:' + key))
    result = dict(schema=1, project='duckdb', portableManifest=str(portable_path),
                  portableManifestSha256=sha(portable_path), libcxxAcquisition=str(acquisition),
                  libcxxAcquisitionSha256=sha(acquisition / 'acquisition.json'),
                  staticExtensions=portable['staticExtensions'], nativeExtensionLoading=False,
                  standard='c++17 portable profile', units=selected,
                  libcSourcesIncluded=with_musl, libcxxProviderIncluded=False,
                  runtimeBindingVerified=False)
    if with_musl:
        extra, receipt = musl_units()
        result['units'] += extra
        result['libcProvider'] = receipt
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('project', choices=('sqlite', 'duckdb'))
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--portable-manifest', type=Path)
    parser.add_argument('--libcxx-acquisition', type=Path)
    parser.add_argument('--with-musl', action='store_true')
    parser.add_argument('--include-tests', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    if args.project == 'duckdb' and (not args.portable_manifest or not args.libcxx_acquisition):
        parser.error('DuckDB requires --portable-manifest and --libcxx-acquisition')
    out.mkdir(parents=True, exist_ok=False)
    if args.project == 'sqlite':
        report = sqlite(out, args.with_musl)
    else:
        report = duckdb(args.portable_manifest.resolve(), args.libcxx_acquisition.resolve(), args.with_musl, args.include_tests)
    (out / 'project.json').write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(dict(manifest=str(out / 'project.json'), units=len(report['units']))))


if __name__ == '__main__':
    main()
