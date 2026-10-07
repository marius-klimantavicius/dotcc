"""Stage the pinned source tree with explicit portable-target adaptations."""
import hashlib
import json
from pathlib import Path
import shutil


def inventory(root):
    """Record files and link targets; never silently follow an unlisted link."""
    root = Path(root)
    records = []
    for path in sorted(root.rglob('*')):
        if path.is_symlink():
            records.append(dict(path=str(path.relative_to(root)), target=str(path.readlink())))
        elif path.is_file():
            records.append(dict(path=str(path.relative_to(root)), sha256=sha(path)))
    return sorted(records, key=lambda item: item['path'])


def verify_staged(report):
    """Reject modified, missing, or additional staged source/header inputs."""
    output = Path(report['stagedSource']).parent
    actual = []
    for name in ('source', 'codegen', 'authored'):
        for item in inventory(output / name):
            actual.append(dict(item, path=name + '/' + item['path']))
    if actual != report['stagedInputs']:
        raise ValueError('Portable staged source/header inventory changed')


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def prepare(manifest_path, output):
    manifest_path, output = Path(manifest_path).resolve(), Path(output).resolve()
    manifest = json.loads(manifest_path.read_text())
    source = Path(manifest['source']).resolve()
    acquisition = json.loads((source.parent / 'source-manifest.json').read_text())
    expected = sorted([*acquisition['files'], *acquisition.get('symlinks', [])], key=lambda item: item['path'])
    if inventory(source) != expected:
        raise ValueError('Pinned DuckDB source inventory changed (file contents, extra/missing files, or links)')
    output.mkdir(parents=True, exist_ok=False)
    staged = output / 'source'
    shutil.copytree(source, staged, symlinks=True)
    header = 'extension/icu/third_party/icu/i18n/double-conversion-utils.h'
    path = staged / header
    original = path.read_text()
    before = '#if defined(_M_X64) || defined(__x86_64__) || '
    after = '#if defined(__EDG_DOT_TARGET__) || defined(_M_X64) || defined(__x86_64__) || '
    if original.count(before) != 1:
        raise ValueError('ICU double-conversion target guard changed')
    path.write_text(original.replace(before, after))
    changes = [dict(path=header, originalSha256=sha(source / header), stagedSha256=sha(path),
        before=before, after=after,
        reason='The portable .NET target evaluates double arithmetic at binary64 precision; identify that capability without advertising a native CPU or selecting its intrinsics.')]

    header = 'third_party/concurrentqueue/lightweightsemaphore.h'
    path = staged / header
    original = path.read_text()
    replacements = [
        ('#elif defined(__unix__)\n', '#elif defined(__EDG_DOT_TARGET__) || defined(__unix__)\n'),
        ('#elif defined(__unix__) || defined(__MVS__)\n',
         '#elif defined(__EDG_DOT_TARGET__) || defined(__unix__) || defined(__MVS__)\n'),
    ]
    updated = original
    for before, after in replacements:
        if updated.count(before) != 1:
            raise ValueError('Concurrentqueue semaphore platform guard changed')
        updated = updated.replace(before, after)
    path.write_text('// Adapted for the EDG portable target: use the shared BCL-backed POSIX semaphore API.\n' + updated)
    changes.append(dict(path=header, originalSha256=sha(source / header), stagedSha256=sha(path),
                        replacements=[dict(before=before, after=after) for before, after in replacements],
                        reason='Select the existing POSIX semaphore algorithm on the portable EDG target; semaphore.h uses owner tokens and cross-platform BCL synchronization rather than native OS layouts.'))

    # Generated CMake headers and the static extension loader are real inputs,
    # even though they are outside the upstream archive. Freeze them with the
    # authored reporter instead of reading a mutable native build during EDG.
    codegen = Path(manifest['build']).resolve() / 'codegen'
    codegen_inputs = inventory(codegen)
    if not codegen_inputs:
        raise ValueError('Native build has no generated extension loader inputs')
    if any('target' in item for item in codegen_inputs):
        raise ValueError('Generated codegen inputs must not contain symlinks')
    shutil.copytree(codegen, output / 'codegen')
    reporter = Path(__file__).resolve().parent / 'tests/reporter.cpp'
    (output / 'authored').mkdir()
    shutil.copy2(reporter, output / 'authored/reporter.cpp')
    external_inputs = [dict(path=str(codegen / item['path']), stagedPath='codegen/' + item['path'],
                           sha256=item['sha256']) for item in codegen_inputs]
    external_inputs.append(dict(path=str(reporter), stagedPath='authored/reporter.cpp', sha256=sha(reporter)))

    def relocate(value):
        return (value.replace(str(source), str(staged))
                .replace(str(codegen), str(output / 'codegen'))
                .replace(str(reporter), str(output / 'authored/reporter.cpp')))

    units = []
    for original_unit in manifest['units']:
        if original_unit['role'] not in ('library', 'test'):
            continue
        unit = dict(original_unit)
        unit['file'] = relocate(unit['file'])
        unit['arguments'] = [relocate(value) for value in unit['arguments']]
        unit['includeDirectories'] = [relocate(value) for value in unit['includeDirectories']]
        source_path = Path(unit['file'])
        if not source_path.is_relative_to(output):
            raise ValueError('Unstaged external translation unit: ' + unit['file'])
        for include in unit['includeDirectories']:
            if not Path(include).is_relative_to(output):
                raise ValueError('Unstaged external include directory: ' + include)
        original_standards = [argument[5:] for argument in original_unit['arguments'] if argument.startswith('-std=')]
        unit['nativeStandard'] = original_standards[-1] if original_standards else 'compiler-default'
        units.append(unit)
    for role in ('library', 'test'):
        (output / (role + '-commands.json')).write_text(json.dumps([u for u in units if u['role'] == role], indent=2) + '\n')
    report = dict(source=str(source), stagedSource=str(staged), sourceManifestSha256=sha(source.parent / 'source-manifest.json'),
                  nativeSourceGraph=dict(path=str(manifest_path), sha256=sha(manifest_path)),
                  verifiedUpstreamFiles=len(acquisition['files']), adaptations=changes,
                  units=units, staticExtensions=manifest['staticExtensions'], bundledDependencies=manifest['bundledDependencies'],
                  externalInputs=external_inputs,
                  standardAdaptation=dict(
                      reason='The EDG whole-program merge and its libc++ provider use one explicitly selected C++ standard. Native per-TU standards are retained as nativeStandard; this is a source-profile adaptation, not identical native compilation semantics.',
                      portableStandard='Selected and recorded by the translation driver/libc++ provider',
                      nativeStandards=sorted({unit['nativeStandard'] for unit in units})),
                  stagedInputs=[])
    for name in ('source', 'codegen', 'authored'):
        for item in inventory(output / name):
            report['stagedInputs'].append(dict(item, path=name + '/' + item['path']))
    if inventory(source) != expected or inventory(codegen) != codegen_inputs:
        raise ValueError('Native source inputs changed while staging')
    for item in external_inputs:
        if sha(item['path']) != item['sha256'] or sha(output / item['stagedPath']) != item['sha256']:
            raise ValueError('External source input changed while staging: ' + item['path'])
    verify_staged(report)
    (output / 'portable.json').write_text(json.dumps(report, indent=2) + '\n')
    return report


if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    args = parser.parse_args()
    result = prepare(args.manifest, args.output_dir)
    print(str(len(result['units'])) + ' library/test translation units staged')
