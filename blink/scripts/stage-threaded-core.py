#!/usr/bin/env python3
"""Prepare a separate, inactive threaded profile; never compile or publish it.

The caller supplies a reviewed fresh probe-core --stage-only base. This helper
does not invoke that pipeline, mutate its inputs, or enable the stable product.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys

from core_inputs import compiler_identity, emission_identity, profile_sources, stage_managed_boundaries

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
PREFIX = b'#include "host-bindings.h"\n'
AUTHORED_PREFIX = b'#include "config.h"\n'
THREAD_HEADER = 'src/Host/include/host-guest-threads.h'
THREAD_BRIDGE = 'src/Host/HostGuestThreadsBridge.cs'
EPOLL_HEADER = 'src/Host/include/host-epoll.h'
EPOLL_BRIDGE = 'src/Host/HostEpollBridge.cs'
REQUIRED = ('BLINK_MANAGED_GUEST_THREADS', 'HAVE_THREADS', 'NOLINEAR', 'DISABLE_JIT')
FORBIDDEN = ('DISABLE_THREADS', 'HAVE_FORK', 'HAVE_PTHREAD_PROCESS_SHARED', 'HAVE_PTHREAD_SETCANCELSTATE')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def digest(data):
    return hashlib.sha256(data).hexdigest()


def read_json(path):
    return json.loads(path.read_text())


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n')


def relative_file(name):
    path = Path(name)
    if path.is_absolute() or '..' in path.parts:
        raise RuntimeError('Snapshot path must be relative: ' + name)
    return path


def header_text(manifest):
    lines = ['#ifndef BLINK_FULL_HOST_BINDINGS_H', '#define BLINK_FULL_HOST_BINDINGS_H',
             '#include "config.h"', '#undef BLINK_MANAGED_HOST_DECLARATIONS_ONLY',
             '#define BLINK_MANAGED_HOST_BINDINGS 1']
    lines += ['#define ' + key + ' ' + value for key, value in manifest['capabilities'].items()]
    lines += ['#define ' + name + ' blink_host_' + name for name in manifest['isolate']]
    lines += ['#include "' + name + '"' for name in manifest['includeHeaders']]
    return ('\n'.join(lines + ['#endif', ''])).encode()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-profile', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True, help='fresh directory under campaign generated/')
    parser.add_argument('--receipt', type=Path, required=True, help='fresh external receipt under campaign artifacts/')
    parser.add_argument('--instance-methods', action='store_true', help='select explicit instance-v1 object and callback ABI')
    parser.add_argument('--mremap-validation', action='store_true',
                        help='derive reviewed source-range validation after the threaded syscall boundary')
    parser.add_argument('--empty-epoll', action='store_true',
                        help='select private empty epoll descriptor/wait bindings; no registrations')
    args = parser.parse_args()
    base = args.base_profile.resolve()
    profile = args.output.resolve()
    receipt_path = args.receipt.resolve()
    if (not profile.is_relative_to((ROOT / 'generated').resolve()) or profile.exists()
            or profile.is_relative_to(base) or base.is_relative_to(profile)):
        raise SystemExit('Choose a fresh generated profile disjoint from the base')
    if (not receipt_path.is_relative_to((ROOT / 'artifacts').resolve())
            or receipt_path.exists() or receipt_path.is_relative_to(profile)):
        raise SystemExit('Choose a fresh external campaign artifact receipt')
    report = dict(kind='derived-threaded-core-staging', staged=False, compiled=False,
                  runtime_qualified=False, base_profile=str(base), profile=str(profile),
                  helper_sha256=sha(Path(__file__)), mremap_validation=args.mremap_validation,
                  empty_epoll=args.empty_epoll)
    live = {}

    def track(path):
        path = path.resolve()
        value = sha(path)
        if str(path) in live and live[str(path)] != value:
            raise RuntimeError('Input changed during staging: ' + str(path))
        live[str(path)] = value
        return value

    def copy(original, destination):
        expected = track(original)
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(original, destination)
        if sha(destination) != expected:
            raise RuntimeError('Copy changed: ' + str(original))

    try:
        base_inputs_path = base / 'inputs.json'
        inputs = read_json(base_inputs_path)
        track(base_inputs_path)
        compiler = compiler_identity(REPO / 'DotCC/bin/Release/net10.0')
        if inputs['compiler'] != compiler:
            raise RuntimeError('Base compiler differs; prepare a fresh reviewed base')
        if sha(base / 'compiler-identity.py') != track(ROOT / 'scripts/core_inputs.py'):
            raise RuntimeError('Base compiler identity schema differs')
        for name, expected in inputs['staged_headers'].items():
            if track(base / relative_file(name)) != expected:
                raise RuntimeError('Base snapshot changed: ' + name)
        base_entries = profile_sources(base, ROOT, inputs)
        if len(base_entries) != 108:
            raise RuntimeError('Expected exactly 108 reviewed base producers')
        for entry in base_entries:
            if track(ROOT / entry['staged_path']) != entry['sha256']:
                raise RuntimeError('Base selected source changed: ' + entry['path'])
        manifest = read_json(base / 'host-bindings.json')
        if manifest.get('version') != 1 or manifest != read_json(ROOT / 'config/host-bindings.json'):
            raise RuntimeError('Base binding manifest differs from the reviewed active manifest')
        track(ROOT / 'config/host-bindings.json')
        base_binding = read_json(base / 'host-binding-receipt.json')
        if base_binding['manifest_sha256'] != sha(base / 'host-bindings.json'):
            raise RuntimeError('Base binding manifest hash differs')
        if (base_binding['header_sha256'] != sha(base / 'host-bindings.h')
                or (base / 'host-bindings.h').read_bytes() != header_text(manifest)):
            raise RuntimeError('Base host preamble derivation differs')
        source_inputs = dict(base_binding['source_inputs'])
        for name, expected in source_inputs.items():
            if track(ROOT / relative_file(name)) != expected:
                raise RuntimeError('Live authored inputs differ; prepare a fresh base: ' + name)
        for category, directory in (('headers', 'authored'), ('cSources', 'authored'), ('managedSources', 'managed')):
            for name in manifest[category]:
                if sha(base / directory / Path(name).name) != source_inputs[name]:
                    raise RuntimeError('Base authored snapshot/provenance differs: ' + name)
        # These two sources are initial probe-stage copies, not manifest entries.
        for name in ('HostSignals.c', 'HostMemory.c', 'HostSignals.h', 'HostMemory.h'):
            original = ROOT / 'src/Host' / ('include' if name.endswith('.h') else '') / name
            expected = track(original)
            if sha(base / 'authored' / name) != expected:
                raise RuntimeError('Base initial authored input differs: ' + name)
            source_inputs[str(original.relative_to(ROOT))] = expected
        host = ROOT / 'src/Managed.Emulation.Host'
        host_paths = {str(path.relative_to(ROOT)) for path in host.rglob('*') if path.is_file()
                      and not {'bin', 'obj'}.intersection(path.relative_to(host).parts)}
        if host_paths != {name for name in source_inputs if name.startswith('src/Managed.Emulation.Host/')}:
            raise RuntimeError('Host project closure changed; prepare a fresh base')
        for name in host_paths:
            relative = Path(name).relative_to('src/Managed.Emulation.Host')
            if sha(base / 'host-project' / relative) != source_inputs[name]:
                raise RuntimeError('Base Host snapshot/provenance differs: ' + name)
        inventory_path = ROOT / 'config/source-inventory.json'
        track(inventory_path)
        pins = {row['path']: row['sha256'] for row in read_json(inventory_path)['files']}
        upstream = ROOT / 'ref' / read_json(ROOT / 'config/source-manifest.json')['upstream']['directory']
        track(ROOT / 'config/source-manifest.json')
        for name, expected in pins.items():
            if track(upstream / name) != expected:
                raise RuntimeError('Pinned upstream changed: ' + name)

        profile.mkdir(parents=True)
        for name in inputs['staged_headers']:
            copy(base / name, profile / name)
        copy(base_inputs_path, profile / 'base-provenance/inputs.json')
        copy(base / 'host-binding-receipt.json', profile / 'base-provenance/host-binding-receipt.json')
        copy(base / 'host-bindings.json', profile / 'base-provenance/host-bindings.json')
        copy(Path(__file__), profile / 'stage-threaded-core.py')
        for name in ('assemble-core.py', 'isolate-core.py'):
            track(ROOT / 'scripts' / name)

        config_path = profile / 'config.h'
        old_config = config_path.read_bytes()
        config = old_config.decode()
        if len(re.findall(r'^#define\s+DISABLE_THREADS\s+1\s*$', config, re.M)) != 1:
            raise RuntimeError('Base must explicitly select DISABLE_THREADS once')
        config = re.sub(r'^#define\s+DISABLE_THREADS\s+1\s*$',
                        '#define BLINK_MANAGED_GUEST_THREADS 1\n#define HAVE_THREADS 1', config, flags=re.M)
        required = [*REQUIRED]
        forbidden = [*FORBIDDEN]
        if args.empty_epoll:
            if re.search(r'^\s*#\s*define\s+HAVE_EPOLL_PWAIT[12]\b', config, re.M):
                raise RuntimeError('Base unexpectedly selects an epoll capability')
            if not config.endswith('#endif\n'):
                raise RuntimeError('Base config include-guard ending differs')
            config = config[:-len('#endif\n')] + '#define HAVE_EPOLL_PWAIT1 1\n#endif\n'
            required.append('HAVE_EPOLL_PWAIT1')
            forbidden.append('HAVE_EPOLL_PWAIT2')
        definitions = set(re.findall(r'^\s*#\s*define\s+(\w+)', config, re.M))
        if not set(required).issubset(definitions) or set(forbidden).intersection(definitions):
            raise RuntimeError('Derived threaded profile contract differs')
        config_path.write_text(config)
        changes = [dict(path='config.h', base_sha256=digest(old_config), derived_sha256=sha(config_path),
                        operation='select managed guest threads' + (' and private empty epoll waits' if args.empty_epoll else ''))]
        for name in ('pthread.h', 'signal.h'):
            original = ROOT / 'config/managed-threaded' / name
            old = sha(profile / 'host' / name)
            copy(original, profile / 'host' / name)
            changes.append(dict(path='host/' + name, base_sha256=old, derived_sha256=sha(original),
                                original=str(original), operation='highest-priority existing host include slot'))
        if args.empty_epoll:
            original = ROOT / 'config/managed-threaded/sys/epoll.h'
            if (profile / 'host/sys/epoll.h').exists():
                raise RuntimeError('Base unexpectedly contains an epoll overlay')
            copy(original, profile / 'host/sys/epoll.h')
            source_inputs[str(original.relative_to(ROOT))] = sha(original)
            changes.append(dict(path='host/sys/epoll.h', derived_sha256=sha(original),
                                original=str(original), operation='select private epoll callback ABI'))
        if (profile / 'host/config.h').exists():
            raise RuntimeError('Host directory must not shadow the selected root config.h')

        thread_dir = ROOT / 'src/UpstreamGuestThreads'
        for original in thread_dir.iterdir():
            if original.is_file():
                copy(original, profile / 'source-adaptations/UpstreamGuestThreads' / original.name)
        original_header = ROOT / THREAD_HEADER
        original_bridge = ROOT / THREAD_BRIDGE
        copy(original_header, profile / 'authored' / original_header.name)
        copy(original_bridge, profile / 'managed' / original_bridge.name)
        source_inputs[THREAD_HEADER] = sha(original_header)
        source_inputs[THREAD_BRIDGE] = sha(original_bridge)
        for category, name in (('headers', THREAD_HEADER), ('managedSources', THREAD_BRIDGE),
                               ('includeHeaders', original_header.name)):
            if name in manifest[category]:
                raise RuntimeError('Thread binding unexpectedly already selected: ' + name)
            manifest[category].append(name)
        if args.empty_epoll:
            for name, directory in ((EPOLL_HEADER, 'authored'), (EPOLL_BRIDGE, 'managed')):
                original = ROOT / name
                copy(original, profile / directory / original.name)
                source_inputs[name] = sha(original)
            for category, name in (('headers', EPOLL_HEADER), ('managedSources', EPOLL_BRIDGE),
                                   ('includeHeaders', Path(EPOLL_HEADER).name)):
                if name in manifest[category]:
                    raise RuntimeError('Epoll binding unexpectedly already selected: ' + name)
                manifest[category].append(name)
        manifest['scope'] = 'Derived managed guest-thread profile; staging is not runtime qualification'
        write_json(profile / 'host-bindings.json', manifest)
        (profile / 'host-bindings.h').write_bytes(header_text(manifest))
        for name in manifest['includeHeaders']:
            if sum((profile / directory / name).is_file() for directory in ('', 'authored', 'host')) != 1:
                raise RuntimeError('Missing or ambiguous derived binding header: ' + name)
        bindings = read_json(profile / 'binding-sources.json')
        bindings['authored_managed'].append('managed/' + original_bridge.name)
        if args.empty_epoll:
            bindings['authored_managed'].append('managed/' + Path(EPOLL_BRIDGE).name)
        write_json(profile / 'binding-sources.json', bindings)

        stage_output = profile / 'upstream/guest-threads'
        stage_receipt = profile / 'guest-threads-boundary.json'
        command = [sys.executable, '-B', str(thread_dir / 'stage.py'), '--predecessor',
                   str(base / 'upstream/guest-runtime/syscall.c'), '--predecessor-receipt',
                   str(base / 'guest-runtime-boundary.json'), '--output', str(stage_output),
                   '--receipt', str(stage_receipt)]
        with (profile / 'guest-threads-stage.log').open('wb') as log:
            result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=30)
        if result.returncode:
            raise RuntimeError('Thread source derivation failed; inspect guest-threads-stage.log')
        boundary = read_json(stage_receipt)
        if (boundary['stage_sha256'] != track(thread_dir / 'stage.py')
                or boundary['patch_sha256'] != track(thread_dir / 'guest-threads.patch')
                or boundary['required_headers'][THREAD_HEADER] != sha(original_header)):
            raise RuntimeError('Thread source derivation identity differs')
        for name, expected in boundary['frozen_inputs'].items():
            if track(Path(name)) != expected:
                raise RuntimeError('Thread derivation input changed: ' + name)

        adapted_sources = dict(boundary['sources'])
        adapted_paths = {name: stage_output / name for name in adapted_sources}
        mremap_receipt = None
        if args.mremap_validation:
            directory = ROOT / 'src/UpstreamMremap'
            for original in directory.iterdir():
                if original.is_file():
                    copy(original, profile / 'source-adaptations/UpstreamMremap' / original.name)
            output = profile / 'upstream/mremap'
            mremap_receipt = profile / 'mremap-boundary.json'
            command = [sys.executable, '-B', str(directory / 'stage.py'), '--predecessor',
                       str(stage_output / 'syscall.c'), '--predecessor-receipt', str(stage_receipt),
                       '--output', str(output), '--receipt', str(mremap_receipt)]
            with (profile / 'mremap-stage.log').open('wb') as log:
                result = subprocess.run(command, stdout=log, stderr=subprocess.STDOUT, timeout=30)
            if result.returncode:
                raise RuntimeError('Mremap validation derivation failed; inspect mremap-stage.log')
            mapping = read_json(mremap_receipt)
            if (mapping['stage_sha256'] != track(directory / 'stage.py')
                    or mapping['patch_sha256'] != track(directory / 'mremap.patch')
                    or set(mapping['sources']) != {'syscall.c'}):
                raise RuntimeError('Mremap validation derivation identity differs')
            row = mapping['sources']['syscall.c']
            if (row['source_sha256'] != pins['blink/syscall.c']
                    or row['predecessor_sha256'] != boundary['sources']['syscall.c']['staged_sha256']
                    or row['staged_sha256'] != sha(output / 'syscall.c')):
                raise RuntimeError('Mremap validation source chain differs')
            for name, expected in mapping['frozen_inputs'].items():
                if track(Path(name)) != expected:
                    raise RuntimeError('Mremap validation input changed: ' + name)
            adapted_sources['syscall.c'] = row
            adapted_paths['syscall.c'] = output / 'syscall.c'

        track(ROOT / 'config/managed-boundaries.json')
        stage_managed_boundaries(ROOT, profile, args.instance_methods)
        if args.instance_methods:
            write_json(profile / 'instance-abi.json', {'abi': 'instance-v1'})

        overrides = {}
        rows = []
        old_rows = {row['source']: row for row in base_binding['upstream_preambles']}
        upstream_entries = [entry for entry in base_entries if entry['path'].startswith('blink/')]
        if set(old_rows) != {entry['path'] for entry in upstream_entries}:
            raise RuntimeError('Base preamble producer coverage differs')
        for entry in upstream_entries:
            name = entry['path']
            old_row = old_rows[name]
            old_bound = (ROOT / entry['staged_path']).read_bytes()
            if (not old_bound.startswith(PREFIX) or digest(old_bound[len(PREFIX):]) != old_row['pre_binding_sha256']
                    or old_row['original_sha256'] != pins[name] or old_row['staged_sha256'] != entry['sha256']):
                raise RuntimeError('Exact base preamble/source chain differs: ' + name)
            filename = Path(name).name
            adapted = adapted_sources.get(filename)
            if adapted:
                adapted_path = adapted_paths[filename]
                content = adapted_path.read_bytes()
                if adapted['source_sha256'] != pins[name] or digest(content) != adapted['staged_sha256']:
                    raise RuntimeError('Thread replacement source identity differs: ' + name)
                previous = dict(staged_path=str(adapted_path.relative_to(ROOT)),
                                sha256=digest(content), original_sha256=pins[name])
            else:
                content = old_bound[len(PREFIX):]
                previous = old_row['previous_adaptation']
            target = profile / 'bound' / filename
            target.write_bytes(PREFIX + content)
            overrides[name] = dict(staged_path=str(target.relative_to(ROOT)), sha256=sha(target),
                                   original_sha256=pins[name])
            rows.append(dict(source=name, original_sha256=pins[name], pre_binding_sha256=digest(content),
                             previous_adaptation=previous, staged_sha256=sha(target),
                             base_bound_sha256=entry['sha256']))
        if set(boundary['sources']) != {'syscall.c', 'memorymalloc.c', 'signal.c'}:
            raise RuntimeError('Unexpected thread stage replacement set')
        write_json(profile / 'binding-overrides.json', overrides)
        for path in sorted((profile / 'authored').glob('*.c')):
            original = path.read_bytes()
            path.write_bytes(AUTHORED_PREFIX + original)
            changes.append(dict(path=str(path.relative_to(profile)), base_sha256=digest(original),
                                derived_sha256=sha(path), operation='prepend only selected config include'))
        binding_receipt = dict(scope=manifest['scope'], manifest_sha256=sha(profile / 'host-bindings.json'),
            header_sha256=sha(profile / 'host-bindings.h'), source_inputs=source_inputs,
            upstream_preambles=rows, authored_derivations=changes,
            base_binding_receipt_sha256=sha(base / 'host-binding-receipt.json'))
        write_json(profile / 'host-binding-receipt.json', binding_receipt)

        closure = read_json(profile / 'closure.json')
        additions = read_json(profile / 'managed-additions.json')
        for row in closure['sources']:
            row['staged_path'] = overrides[row['path']]['staged_path']
        write_json(profile / 'closure.json', closure)
        for filename, paths in (
                ('core-source-paths.txt', [ROOT / overrides[row['path']]['staged_path'] for row in closure['sources']]),
                ('managed-source-paths.txt', [ROOT / overrides[row['path']]['staged_path'] for row in additions['sources']]),
                ('authored-source-paths.txt', [profile / name for name in bindings['authored_c']])):
            (profile / filename).write_text(''.join(str(path) + '\n' for path in paths))
        write_json(profile / 'threaded-derivation.json', dict(
            base_profile=str(base), base_inputs_sha256=sha(base_inputs_path), helper_sha256=sha(Path(__file__)),
            scope='C# owns lifecycle; no C frontend added', required_defines=required, forbidden_defines=forbidden,
            empty_epoll=args.empty_epoll,
            changes=changes, threaded_boundary_sha256=sha(stage_receipt),
            mremap_boundary_sha256=sha(mremap_receipt) if mremap_receipt else None,
            preserved_upstream_pins=pins, source_inputs=source_inputs))
        derived_inputs = dict(compiler=compiler, source_overrides=overrides,
            staged_headers={str(path.relative_to(profile)): sha(path) for path in profile.rglob('*') if path.is_file()})
        write_json(profile / 'inputs.json', derived_inputs)
        entries = profile_sources(profile, ROOT, derived_inputs)
        if len(entries) != 108 or len({entry['path'] for entry in entries}) != 108:
            raise RuntimeError('Derived profile must resolve exactly 108 unique producers')
        for entry in entries:
            path = ROOT / entry['staged_path']
            if sha(path) != entry['sha256'] or not path.resolve().is_relative_to(profile):
                raise RuntimeError('Producer escaped derived frozen profile: ' + entry['path'])
            if (Path(entry['path']).name in {'managed-driver.c', 'probe.c', 'GuestExecution.c'}
                    or re.search(r'\b(?:main|OnSpawn)\s*\([^;{}]*\)\s*\{', path.read_text())):
                raise RuntimeError('C execution/test frontend found: ' + entry['path'])
        identities = {entry['path']: emission_identity(profile, ROOT, derived_inputs, entry) for entry in entries}
        compatible = []
        for path in sorted((ROOT / 'artifacts/core').glob('isolate-*/result.json')):
            try:
                old = read_json(path)
                for row in old['rows']:
                    identity = identities.get(row.get('source'))
                    if identity is not None and row.get('emission_identity') == identity and row.get('exit_code') == 0:
                        binary = Path(row['object_path'])
                        if binary.is_file() and sha(binary) == row['object_sha256']:
                            compatible.append(dict(source=row['source'], receipt=str(path), receipt_sha256=sha(path),
                                object_path=str(binary), object_sha256=row['object_sha256'],
                                producing_profile=old['profile'], producing_profile_inputs_sha256=old['profile_inputs_sha256']))
            except (OSError, ValueError, KeyError):
                continue
        for name, expected in live.items():
            if sha(Path(name)) != expected:
                raise RuntimeError('Input changed during staging: ' + name)
        if compiler_identity(REPO / 'DotCC/bin/Release/net10.0') != compiler:
            raise RuntimeError('Compiler changed during staging')
        report.update(staged=True, inputs_sha256=sha(profile / 'inputs.json'), compiler=compiler,
            producer_count=len(entries), selected=entries, emission_identities=identities,
            compatible_prior_objects=compatible, original_inputs=live,
            profile_files=derived_inputs['staged_headers'], source_inputs=source_inputs,
            note='Initial changed threaded headers exclude single-thread objects; exact threaded retries may reuse with provenance. No compiler was invoked.')
    except BaseException as error:
        report['failure'] = str(error)
        raise
    finally:
        write_json(receipt_path, report)
    print(profile)


if __name__ == '__main__':
    main()
