"""Stage the pinned source tree with explicit portable-target adaptations."""
import hashlib
import json
from pathlib import Path
import re
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


def adapt_numeric_cast(source, staged):
    """Disambiguate an unused private class tag from the public function template.

    EDG's unchanged C backend rejects their correspondence across translation
    units. Audit the complete pinned tree before applying this one-token rename;
    an upstream use of the class requires a fresh review rather than a broad
    substitution that could rename calls to the public function.
    """
    header = 'src/include/duckdb/common/operator/numeric_cast.hpp'
    public_header = 'src/include/duckdb/common/numeric_utils.hpp'
    files, counts = [], dict(templateCalls=0, functionDefinitions=0, comments=0, classDefinitions=0)
    for path in sorted(Path(source).rglob('*')):
        if not path.is_file():
            continue
        try:
            text = path.read_text()
        except UnicodeError:
            continue
        if re.search(r'\bNumericCastOperator\b', text):
            raise ValueError('Private NumericCastOperator replacement name already exists: ' + str(path))
        matches = list(re.finditer(r'\bNumericCast\b', text))
        if not matches:
            continue
        relative = str(path.relative_to(source))
        files.append(dict(path=relative, sha256=sha(path), occurrences=len(matches)))
        for match in matches:
            suffix = text[match.end():]
            prefix = text[text.rfind('\n', 0, match.start()) + 1:match.start()]
            if re.match(r'\s*<', suffix):
                kind = 'templateCalls'
            elif relative == public_header and prefix == 'TO ' and suffix.startswith('(FROM val) {'):
                kind = 'functionDefinitions'
            elif relative == public_header and prefix == '// ' and suffix.startswith('\n'):
                kind = 'comments'
            elif relative == header and prefix == 'struct ' and suffix.startswith(' {'):
                kind = 'classDefinitions'
            else:
                raise ValueError('Unreviewed NumericCast class/name use: ' + relative + ':' +
                                 str(text.count('\n', 0, match.start()) + 1))
            counts[kind] += 1
    if any(counts[kind] != 1 for kind in ('functionDefinitions', 'comments', 'classDefinitions')):
        raise ValueError('Pinned NumericCast declarations changed')
    path = Path(staged) / header
    before, after = 'struct NumericCast {', 'struct NumericCastOperator {'
    original = path.read_text()
    if original.count(before) != 1:
        raise ValueError('NumericCast operator declaration changed')
    path.write_text(original.replace(before, after))
    return dict(path=header, originalSha256=sha(Path(source) / header), stagedSha256=sha(path),
                before=before, after=after, tokenAudit=dict(counts=counts, files=files, replacementIdentifierAbsent=True),
                reason='Rename only the unused private operator class tag to avoid cross-TU name-kind correspondence rejection also reproduced by the unchanged EDG C backend. Preserve the public NumericCast function template and both implementations.')


def adapt_logical_type_pair(source, staged):
    """Resolve the child-pair copy signature without odr-use or runtime code."""
    header = 'src/include/duckdb/common/types.hpp'
    path = Path(staged) / header
    original = path.read_text()
    marker = '} // namespace duckdb'
    anchor = '''// Resolve this public child-pair copy signature consistently in every TU.
// sizeof is unevaluated: no assignment body, address, storage, or runtime code.
static_assert(sizeof(static_cast<
                  std::pair<string, LogicalType>& (std::pair<string, LogicalType>::*)(
                      const std::pair<string, LogicalType>&)>(
                  &std::pair<string, LogicalType>::operator=)) > 0,
              "LogicalType child-pair copy assignment signature");

'''
    if original.count(marker) != 1 or original.count('struct LogicalType {') != 1:
        raise ValueError('LogicalType declaration or namespace boundary changed')
    if 'LogicalType child-pair copy assignment signature' in original:
        raise ValueError('LogicalType pair signature anchor already exists')
    path.write_text(original.replace(marker, anchor + marker))
    return dict(path=header, originalSha256=sha(Path(source) / header), stagedSha256=sha(path),
                before=marker, after=anchor + marker,
                reason='Unevaluated selection resolves the existing pair<string, LogicalType> copy-assignment signature consistently when one TU calls it and another only instantiates the pair. The unchanged EDG C backend reproduces the original correspondence rejection; the anchor preserves libc++ declarations, algorithms, layout, copyability, and runtime behavior.')


def adapt_value_pair(source, staged):
    """Resolve the child-pair copy signature without odr-use or runtime code."""
    header = 'src/include/duckdb/common/types/value.hpp'
    path = Path(staged) / header
    original = path.read_text()
    marker = '} // namespace duckdb'
    anchor = '''// Resolve this public child-pair copy signature consistently in every TU.
// sizeof is unevaluated: no assignment body, address, storage, or runtime code.
static_assert(sizeof(static_cast<
                  std::pair<string, Value>& (std::pair<string, Value>::*)(
                      const std::pair<string, Value>&)>(
                  &std::pair<string, Value>::operator=)) > 0,
              "Value child-pair copy assignment signature");

'''
    if original.count(marker) != 1 or original.count('class Value {') != 1:
        raise ValueError('Value declaration or namespace boundary changed')
    if 'Value child-pair copy assignment signature' in original:
        raise ValueError('Value pair signature anchor already exists')
    path.write_text(original.replace(marker, anchor + marker))
    return dict(path=header, originalSha256=sha(Path(source) / header), stagedSha256=sha(path),
                before=marker, after=anchor + marker,
                reason='Unevaluated selection resolves the existing pair<string, Value> copy-assignment signature consistently when one TU calls it and another instantiates sizeof, construction, or vector emplace from pair<const char*, Value>. The unchanged EDG C backend reproduces the original correspondence rejection; the anchor preserves libc++ declarations, algorithms, layout, copyability, and runtime behavior.')


def adapt_fmt_arguments(source, staged):
    """Spell existing integral conversions in dependent fmt store constants."""
    header = 'third_party/fmt/include/fmt/core.h'
    path = Path(staged) / header
    original = path.read_text()
    enum_declarations = [
        'enum { packed_arg_bits = 5 };',
        'enum { max_packed_args = 63 / packed_arg_bits };',
        'enum : unsigned long long { is_unpacked_bit = 1ULL << 63 };',
    ]
    for declaration in enum_declarations:
        if original.count(declaration) != 1:
            raise ValueError('Pinned fmt packing constant changed: ' + declaration)
    replacements = [
        ('static const bool is_packed = num_args < internal::max_packed_args;',
         'static const bool is_packed = num_args < static_cast<int>(internal::max_packed_args);'),
        ('internal::is_unpacked_bit | num_args;',
         'static_cast<unsigned long long>(internal::is_unpacked_bit) | num_args;'),
    ]
    updated = original
    for before, after in replacements:
        if updated.count(before) != 1:
            raise ValueError('Pinned fmt dependent packing expression changed: ' + before)
        updated = updated.replace(before, after)
    path.write_text(updated)
    return dict(path=header, originalSha256=sha(Path(source) / header), stagedSha256=sha(path),
                replacements=[dict(before=before, after=after) for before, after in replacements],
                unchangedEnumDeclarations=enum_declarations,
                reason='Spell the existing int promotion of max_packed_args (12) and unsigned-long-long conversion of is_unpacked_bit before dependent arithmetic. Avoid unnamed-enum correspondence rejection reproduced by unchanged stock C without changing enum types, packing threshold, values, layouts, or formatting algorithms.')


def adapt_icu_calendar_sub(source, staged):
    """Give the interval operator its own name, preserving date_sub's helper."""
    source = Path(source)
    relative = 'extension/icu/icu-dateadd.cpp'
    expected_counts = {relative: 8, 'extension/icu/icu-datesub.cpp': 18}
    counts, references = {}, []
    for path in sorted(source.rglob('*')):
        if not path.is_file():
            continue
        try:
            text = path.read_text()
        except UnicodeError:
            continue
        if re.search(r'\bICUCalendarIntervalSub\b', text):
            raise ValueError('Private ICUCalendarIntervalSub replacement name already exists: ' + str(path))
        matches = list(re.finditer(r'\bICUCalendarSub\b', text))
        if matches:
            name = str(path.relative_to(source))
            counts[name] = len(matches)
            references.append(dict(path=name, sha256=sha(path), occurrences=len(matches),
                                   lines=[text.count('\n', 0, match.start()) + 1 for match in matches]))
    if counts != expected_counts:
        raise ValueError('Pinned ICUCalendarSub source references changed: ' + str(counts))
    path = Path(staged) / relative
    original = path.read_text()
    replacements = [
        ('struct ICUCalendarSub : public ICUDateFunc {', 'struct ICUCalendarIntervalSub : public ICUDateFunc {', 1),
        ('ICUCalendarSub::Operation', 'ICUCalendarIntervalSub::Operation', 4),
        (', ICUCalendarSub>(', ', ICUCalendarIntervalSub>(', 2),
    ]
    updated = original
    for before, after, count in replacements:
        if updated.count(before) != count:
            raise ValueError('Pinned ICU interval-subtraction reference changed: ' + before)
        updated = updated.replace(before, after)
    diagnostic = '"Unimplemented type for ICUCalendarSub"'
    if updated.count(diagnostic) != 1 or len(re.findall(r'\bICUCalendarSub\b', updated)) != 1:
        raise ValueError('ICU interval-subtraction diagnostic or references changed')
    path.write_text(updated)
    return dict(path=relative, originalSha256=sha(source / relative), stagedSha256=sha(path),
                replacements=[dict(before=before, after=after, occurrences=count) for before, after, count in replacements],
                tokenAudit=dict(counts=counts, files=references, replacementIdentifierAbsent=True),
                preservedDiagnostic=diagnostic,
                reason='Disambiguate dateadd.cpp\'s private interval-subtraction operator from datesub.cpp\'s different namespace-level date-part helper. Rename seven code identifiers only; preserve algorithms, public functions/SQL registrations, datesub.cpp, and the existing diagnostic string. The unchanged stock C backend rejects the original conflicting class definitions across TUs.')


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

    changes.append(adapt_numeric_cast(source, staged))
    changes.append(adapt_logical_type_pair(source, staged))
    changes.append(adapt_value_pair(source, staged))
    changes.append(adapt_fmt_arguments(source, staged))
    changes.append(adapt_icu_calendar_sub(source, staged))

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
