"""Prepare a private, metadata-selected PGlite native embedding variant.

No compiler translation or shared compiler builds occur here. The framework
recipe can consume prepare(ctx)'s paths, flags and actual upstream unit manifest.
"""
import hashlib
import json
from pathlib import Path
import platform
import re
import shlex
import shutil
import sys

from campaigns.delivery import lock
from campaigns.inputs import acquire
from campaigns.model import Source


def metadata(root):
    return json.loads((Path(root) / 'config/sources.json').read_text())


def sources(root):
    root = Path(root)
    row = metadata(root)['product']
    yield Source('product', row['url'], root / 'ref' / row['archive'],
                 root / 'ref' / row['directory'], row['prefix'],
                 row.get('sha256'), tuple(row['required']))


def _write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n')


def _fingerprint(source):
    """Local cache identity includes supplied edited trees; never asserts a pin."""
    digest = hashlib.sha256()
    for path in sorted(source.rglob('*')):
        if path.is_file():
            digest.update(path.relative_to(source).as_posix().encode())
            digest.update(path.read_bytes())
    return digest.hexdigest()


def _regularize(root):
    # Upstream configure sometimes creates header links even with LN_S overridden.
    # Delivered stages use ordinary files and do not depend on link privileges.
    for path in root.rglob('*'):
        if path.is_symlink():
            if not path.is_file():
                raise RuntimeError('Unexpected non-file generated link: ' + str(path))
            content = path.read_bytes()
            path.unlink()
            path.write_bytes(content)


def _generated(stage, source):
    rows = []
    for path in sorted(stage.rglob('*')):
        if path.is_file() and (path.suffix in ('.h', '.c') or path.name == 'Makefile.global'):
            relative = path.relative_to(stage)
            original = source / relative
            if not original.is_file() or original.read_bytes() != path.read_bytes():
                rows.append({'path': relative.as_posix(), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
    return rows


def prepare(ctx):
    """Return source/build/include/units/flags/manifests for the parent recipe.

    This is explicitly an LP64 native engine profile, not wasm32 generated
    headers. All subprocesses go through ctx.run for cancellation and receipts.
    """
    if sys.platform != 'linux':
        raise RuntimeError('embedding-native preparation needs Linux C/make tools; upstream WASM SDK is a separate profile')
    root = Path(ctx.root)
    pin = metadata(root)['product']
    policy = json.loads((root / 'config/profile.json').read_text())
    profile = policy['profiles']['embedding-native']
    if platform.machine() != profile['abi']['architecture'] or sys.byteorder != profile['abi']['endianness']:
        raise RuntimeError('embedding-native profile is currently qualified only for Linux x86_64 little-endian')
    tools = {}
    for name, arguments in [('cc', ['--version']), ('make', ['--version']),
                            ('bison', ['--version']), ('flex', ['--version']),
                            ('perl', ['-e', 'print "$^V\\n"'])]:
        tools[name] = ctx.run([name, *arguments], 'native-tool-' + name, 30).strip()
    source = ctx.sources.get('product')
    if source is None:
        source = acquire(next(sources(root)), ctx.policy, ctx.options.fetch)
        ctx.sources['product'] = source
    source = Path(source).resolve()
    # Hashes-off deliberately uses per-run preparation and never trusts a cache.
    tree_identity = _fingerprint(source)
    identity = hashlib.sha256((tree_identity + json.dumps(profile, sort_keys=True)
                               + json.dumps(tools, sort_keys=True)
                               + 'pglite-native-preparation-v1').encode()).hexdigest()
    suffix = ctx.receipt['run_id'] if ctx.policy.mode == 'off' else identity[:20]
    base = root / 'build/native-control' / suffix
    stage = base / 'source'
    build = stage
    prefix = base / 'install'
    stamp = base / 'prepared.json'
    with lock(root / 'build/native-control/prepare.lock'):
        base.mkdir(parents=True, exist_ok=True)
        existing = json.loads(stamp.read_text()) if stamp.is_file() else {}
        if not stage.exists():
            shutil.copytree(source, stage)
        libc = base / 'pglitec.o'
        cflags = [*profile['cflags'], *('-D' + value for value in profile['overrides'])]
        if not (stage / 'config.status').is_file():
            ctx.run(['cc', *profile['native_libc_flags'], '-c', stage / 'pglite/src/pglitec/pglitec.c',
                     '-o', libc], 'native-libc-build', 120, cwd=base)
            ctx.run(['bash', stage / 'configure', '--prefix=' + str(prefix),
                     *profile['configure'], 'CFLAGS=' + shlex.join(cflags),
                     'LIBS=' + str(libc), 'LN_S=cp -p', 'CC=cc'],
                    'native-configure', 600, cwd=stage)
            _regularize(stage)
        if existing.get('cache_identity') != identity or not (stage / 'src/include/catalog/pg_type_d.h').is_file():
            ctx.run(['make', '-j2', *profile['make_variables'], '-C', stage / 'src/backend',
                     'generated-headers', 'generated-parser-sources'], 'native-generated-inputs', 600, cwd=stage)
            _regularize(stage)
        info = {
            'profile': 'embedding-native', 'classification': profile['classification'],
            'requested_revision': pin['revision'], 'observed_source': str(source),
            'observed_source_sha256': tree_identity, 'cache_identity': identity,
            'source': str(stage), 'build': str(build), 'prefix': str(prefix),
            'libc_object': str(libc), 'include': [str(stage / 'src/include')],
            'include_dirs': [str(stage / 'src/include')],
            'manifest': str(stamp), 'tools': tools,
            'preparation_helper_sha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
            'defines': ['__PGLITE__', *profile['overrides']],
            'flags': ['-std=' + profile['dotcc_standard'], '-D_GNU_SOURCE',
                      *('-D' + x for x in ['__PGLITE__', *profile['overrides']]),
                      '-I', str(stage / 'src/include')],
            'native_jobs': 2, 'prepared_run': ctx.receipt['run_id'],
            'source_reference_mutated': False,
            'wasm_profile_equivalence': False,
        }
        config_header = (stage / 'src/include/pg_config.h').read_text()
        sizes = {name: int(value) for name, value in re.findall(r'^#define SIZEOF_(\w+) (\d+)$', config_header, re.M)}
        if any(sizes.get(name) != size for name, size in [('LONG', 8), ('VOID_P', 8), ('SIZE_T', 8), ('OFF_T', 8)]):
            raise RuntimeError('Generated header does not match selected native LP64 ABI: ' + repr(sizes))
        info['generated_sizes'] = sizes
        info['effective_features'] = [line for line in config_header.splitlines()
                                      if re.match(r'^#define (USE_|HAVE_LIB|ENABLE_)', line)]
        info['generated_inputs'] = _generated(stage, source)
        info['notices'] = [{'path': name, 'sha256': hashlib.sha256((stage / name).read_bytes()).hexdigest()}
                           for name in pin['notices']]
        _write(stamp, info)
        _write(ctx.artifacts / 'pglite-preparation.json', info)
        ctx.receipt['pglite_preparation'] = info
        ctx.save()
    return info


def build_native(ctx, info):
    """Build the real fork, bounded to two native workers, in the private tree."""
    root = Path(ctx.root)
    stage = Path(info['source'])
    profile = json.loads((root / 'config/profile.json').read_text())['profiles']['embedding-native']
    with lock(root / 'build/native-control/prepare.lock'):
        ctx.run(['make', '-j2', *profile['make_variables']], 'native-build', 3600, cwd=stage)
        ctx.run(['make', '-j2', *profile['make_variables'], 'install'], 'native-install', 600, cwd=stage)
        _regularize(stage)
        source_manifest = native_manifest(ctx, stage, info)
        info['source_manifest'] = str(ctx.artifacts / 'native-source-manifest.json')
        info['backend_unit_count'] = len(source_manifest['backend_units'])
        info['generated_inputs'] = _generated(stage, Path(info['observed_source']))
        if _fingerprint(Path(info['observed_source'])) != info['observed_source_sha256']:
            raise RuntimeError('Acquired source changed while native preparation was running')
        _write(Path(info['build']).parent / 'native-build-manifest.json', info)
        _write(ctx.artifacts / 'native-build-manifest.json', info)
        ctx.receipt['native_build_manifest'] = str(ctx.artifacts / 'native-build-manifest.json')
        ctx.save()
    return info


def _compile_commands(text, cwd, stage):
    """Parse serial upstream dry-run output, retaining actual TU flag differences."""
    directories = [Path(cwd)]
    result = {}
    for line in text.splitlines():
        enter = re.search(r"Entering directory ['`](.*)'$", line)
        leave = re.search(r"Leaving directory ['`](.*)'$", line)
        if enter:
            directories.append(Path(enter.group(1)))
        elif leave:
            if directories[-1] != Path(leave.group(1)):
                raise RuntimeError('Unbalanced serial make directory trace')
            directories.pop()
        elif line.startswith(('cc ', 'gcc ', 'clang ')):
            parts = shlex.split(line)
            if '-c' not in parts or '-o' not in parts:
                continue
            inputs = [part for part in parts if part.endswith(('.c', '.S', '.s')) and not part.startswith('-')]
            if len(inputs) != 1:
                raise RuntimeError('Ambiguous upstream compilation input: ' + line)
            working = directories[-1]
            origin = (working / inputs[0]).resolve()
            target = (working / parts[parts.index('-o') + 1]).resolve()
            if not origin.is_relative_to(stage) or not target.is_relative_to(stage):
                raise RuntimeError('Upstream compilation escaped private source stage')
            result[str(target)] = dict(source=origin.relative_to(stage).as_posix(),
                                       object=target.relative_to(stage).as_posix(),
                                       cwd=working.relative_to(stage).as_posix(), command=parts,
                                       language='c' if origin.suffix == '.c' else 'assembler')
    return result


def native_manifest(ctx, stage, info):
    # First retain the link command with every upstream object list intact.
    text = ctx.run(['make', '-n', '-j1', 'LN_S=cp -p', '-C', stage / 'src/backend', 'quick'],
                   'native-backend-link-manifest', 120, cwd=stage)
    links = [shlex.split(line) for line in text.splitlines()
             if line.startswith(('cc ', 'gcc ', 'clang ')) and line.endswith(' -o postgres')]
    if len(links) != 1:
        raise RuntimeError('Missing authoritative upstream backend linker command')
    backend = stage / 'src/backend'
    objects, archives = [], []
    for value in links[0]:
        path = (backend / value).resolve()
        if value.endswith('.o') and path.is_relative_to(stage):
            objects.append(path)
        elif value.endswith('.a') and path.is_relative_to(stage):
            archives.append(path)
            members = ctx.run(['ar', 't', path], 'native-archive-' + path.stem, 30)
            objects.extend(path.parent / item for item in members.splitlines() if item)
    # Explicit object targets in each owning Makefile avoid GNU make's
    # intermediate-object elision and its configure-remaking -B behavior. Only
    # this owner's private objects are parked, and always restored in finally.
    parked, commands, directories = [], {}, {}
    try:
        for path in sorted(set(objects)):
            holding = path.with_name(path.name + '.manifest-hold')
            if holding.exists():
                raise RuntimeError('Unrecovered native manifest object: ' + str(holding))
            path.rename(holding)
            parked.append((path, holding))
            directories.setdefault(path.parent, []).append(path.name)
        for directory, names in directories.items():
            label = 'native-sources-' + directory.relative_to(stage).as_posix().replace('/', '-')
            plan = ctx.run(['make', '-n', '-j1', 'LN_S=cp -p', '-C', directory, *names],
                           label, 120, cwd=stage)
            commands.update(_compile_commands(plan, stage, stage))
    finally:
        for path, holding in reversed(parked):
            if path.exists():
                raise RuntimeError('Dry-run unexpectedly wrote an object: ' + str(path))
            holding.rename(path)
    missing = [str(path.relative_to(stage)) for path in objects if str(path) not in commands]
    if missing:
        raise RuntimeError('Native backend input manifest incomplete: ' + repr(missing))
    units = [commands[str(path)] for path in objects]
    # Include the original libc TU separately: upstream deliberately compiles it
    # before adding macro remaps, so it cannot share backend interception flags.
    report = dict(schema=1, profile=info['profile'], revision=info['requested_revision'],
                  classification=info['classification'], backend_units=units,
                  libc_unit=dict(source='pglite/src/pglitec/pglitec.c',
                                 flags=json.loads((Path(ctx.root) / 'config/profile.json').read_text())
                                 ['profiles']['embedding-native']['native_libc_flags']),
                  archives=[str(p.relative_to(stage)) for p in archives],
                  backend_link_command=links[0], complete_backend_link_closure=True,
                  complete_embedded_init_product_closure=False,
                  caveats=['native LP64 variant, not wasm32 configuration parity',
                           'initdb frontend and optional dynamic libraries are separate source closures'])
    _write(ctx.artifacts / 'native-source-manifest.json', report)
    _write(stage.parent / 'native-source-manifest.json', report)
    return report
