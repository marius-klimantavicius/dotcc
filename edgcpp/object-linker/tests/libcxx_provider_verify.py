#!/usr/bin/env python3
"""Check independent libc++ source selection and rejection of stale acquisitions."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[1]
EDG = ROOT.parent


def load(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--acquisition', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    args = parser.parse_args()
    acquisition, out = args.acquisition.resolve(), args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    provider = load(ROOT / 'libcxx_provider.py', 'provider_under_test')
    driver = load(ROOT / 'project.py', 'driver_under_test')
    manifest = load(ROOT / 'project_manifest.py', 'manifest_under_test')
    checks = []

    def check(name, condition):
        checks.append(dict(name=name, passed=bool(condition)))
        if not condition:
            raise ValueError(name)

    error = None
    try:
        units, receipt = provider.prepare_libcxx_units(EDG, acquisition, manifest.libc_arguments())
        check('one-original-cpp-per-unit', len(units) == 56 and all(Path(u['source']).is_file() and Path(u['source']).suffix == '.cpp' for u in units))
        check('unique-unit-identities', len({u['id'] for u in units}) == len(units))
        check('independent-implementation-standard', all(u['standard'] == 'c++20' and u['role'] == 'libcxx' for u in units))
        check('no-preprocessing-or-amalgamation-command', all(not any('preprocess' in a or a.endswith(('.ii', '.i')) for a in u['arguments']) for u in units))
        check('existing-managed-abi-and-real-rtti-providers', {u['id'] for u in units if ':adapter/' in u['id']} == {'libcxx:adapter/' + x for x in ('abi_core.cpp', 'abi_exceptions.cpp', 'abi_atomic.cpp', 'abi_mutex.cpp', 'abi_typeinfo.cpp')})
        rtti = [u for u in units if u['id'] in ('libcxx:adapter/abi_typeinfo.cpp', 'libcxx:src/typeinfo.cpp')]
        check('explicit-rtti-noexcept-profile', len(rtti) == 2 and all('--no_exceptions' in u['arguments'] and '--exceptions' not in u['arguments'] for u in rtti))
        check('other-provider-exception-policy-preserved', all('--exceptions' in u['arguments'] for u in units if u not in rtti))
        check('standard-deallocator-declarations-present', all(str(acquisition / 'libcxx-18.1.8.src/include/new') in u['arguments'] for u in rtti))
        check('real-rtti-authoritative-source-hashes', len(receipt['rttiProvider']['sourceInputs']) == 3 and all(sha(row['path']) == row['sha256'] for row in receipt['rttiProvider']['sourceInputs']))
        check('replacement-originals-not-duplicated', not ({'libcxx:src/' + x for x in provider.REPLACEMENTS} & {u['id'] for u in units}))
        check('complete-pinned-source-hashes', all(sha(row['path']) == row['sha256'] for row in receipt['sourceInputs']))
        check('header-and-private-source-closure-owned', len(receipt['inputHashes']) > 1000)
        check('no-runtime-completion-claim', not receipt['compilationVerified'] and not receipt['runtimeBindingVerified'])
        path = out / 'project.json'
        path.write_text(json.dumps(dict(schema=1, project='libcxx-provider', units=units, libcxxProvider=receipt), indent=2) + '\n')
        check('project-driver-accepts-original-source-manifest', driver.load_manifest(path)['units'] == units)

        # Copy only the recorded compiler-input closure, not tests/docs or large
        # build products. Never mutate or hardlink the user's acquisition.
        staged = out / 'modified-acquisition'
        staged.mkdir()
        shutil.copy2(acquisition / 'acquisition.json', staged / 'acquisition.json')
        for name in receipt['inputHashes']:
            target = staged / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(acquisition / name, target)

        def rejects(name, change, restore):
            change()
            try:
                provider.prepare_libcxx_units(EDG, staged, manifest.libc_arguments())
            except ValueError:
                check(name, True)
            else:
                check(name, False)
            finally:
                restore()

        source_name = next(name for name in receipt['inputHashes'] if name.endswith('/src/string.cpp'))
        target = staged / source_name
        original = target.read_bytes()
        rejects('changed-implementation-rejected', lambda: target.write_bytes(original + b'\n// changed\n'), lambda: target.write_bytes(original))
        config = staged / 'config/__config_site'
        original_config = config.read_bytes()
        rejects('changed-configuration-rejected', lambda: config.write_bytes(original_config + b'\n#define DIFFERENT_ABI 1\n'), lambda: config.write_bytes(original_config))
        acquired_path = staged / 'acquisition.json'
        original_metadata = acquired_path.read_text()
        altered = json.loads(original_metadata)
        altered['source']['sha256'] = '0' * 64
        rejects('different-archive-rejected', lambda: acquired_path.write_text(json.dumps(altered)), lambda: acquired_path.write_text(original_metadata))
        altered = json.loads(original_metadata)
        altered['fileHashes'].pop(source_name)
        rejects('unrecorded-selected-source-rejected', lambda: acquired_path.write_text(json.dumps(altered)), lambda: acquired_path.write_text(original_metadata))
    except Exception as failure:
        error = str(failure)
    result = dict(schema=1, status='failed' if error else 'passed', checks=checks, error=error,
                  acquisition=dict(path=str(acquisition / 'acquisition.json'), sha256=sha(acquisition / 'acquisition.json')),
                  provider=dict(path=str(ROOT / 'libcxx_provider.py'), sha256=sha(ROOT / 'libcxx_provider.py')),
                  scope='Source assembly and provenance only; no C++ translation or execution claim')
    (out / 'results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(dict(status=result['status'], checks=len(checks), error=error)))
    return 1 if error else 0


if __name__ == '__main__':
    raise SystemExit(main())
