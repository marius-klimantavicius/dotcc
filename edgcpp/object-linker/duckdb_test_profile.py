#!/usr/bin/env python3
"""Retain DuckDB tests with an explicit portable compiler/terminal profile.

This prepares source inputs only. It does not implement fork, kill, OS crash
signal interception or terminal capability detection, and does not claim that
translated tests run. Previous manifests and original source files stay intact.
"""
import argparse
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil

REPO = Path(__file__).resolve().parents[2]


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def prepare(manifest, output):
    result = copy.deepcopy(manifest)
    if result.get('project') != 'duckdb' or result.get('testProfile'):
        raise ValueError('Expected an unmodified DuckDB project manifest')
    output = Path(output).resolve()
    output.mkdir(parents=True, exist_ok=False)
    portable = json.loads(Path(result['portableManifest']).read_text())
    source = Path(portable['stagedSource'])
    overlay = output / 'source'
    changes = []
    # Stage only the two changed headers and two units with quoted local
    # termcolor includes. Other quoted includes fall back to the original
    # source directory, recorded explicitly in those units' include paths.
    for relative in ('third_party/catch/catch.hpp', 'test/sqlite/termcolor.hpp',
                     'test/sqlite/result_helper.cpp', 'test/sqlite/sqllogic_test_logger.cpp'):
        target = overlay / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source / relative, target)
    path = overlay / 'third_party/catch/catch.hpp'
    original = path.read_text()
    before = '#    define CATCH_INTERNAL_IGNORE_BUT_WARN(...) (void)__builtin_constant_p(__VA_ARGS__)\n'
    after = ('#    if defined(__EDG_DOT_TARGET__)\n'
             '#      define CATCH_INTERNAL_IGNORE_BUT_WARN(...)\n'
             '#    else\n' + before + '#    endif\n')
    if original.count(before) != 1:
        raise ValueError('Pinned Catch GCC diagnostic probe changed')
    path.write_text(original.replace(before, after))
    changes.append(dict(path=str(path), original=str(source / 'third_party/catch/catch.hpp'),
                        originalSha256=sha(source / 'third_party/catch/catch.hpp'), stagedSha256=sha(path),
                        before=before, after=after,
                        reason='Disable only the GCC diagnostic-only unevaluated probe for EDG; C++17 lambdas remain evaluated by the original assertion.'))
    # Reuse the already reviewed terminal adaptation alone. Do not call the
    # general source-preparation pipeline or adopt unrelated source changes.
    spec = importlib.util.spec_from_file_location('dotobj_terminal_profile', REPO / 'duckdb/edg/portable.py')
    portable_module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(portable_module)
    terminal = overlay / 'test/sqlite/termcolor.hpp'
    if 'defined(TERMCOLOR_OS_EDG)' in terminal.read_text():
        if sha(terminal) != 'af447ee81a8d4e63bddc7641ca4ce1c7b95e496fcaf34654c347dd5b3093b839':
            raise ValueError('Existing portable terminal adaptation differs from the reviewed header')
        changes.append(dict(path=str(terminal), stagedSha256=sha(terminal), reason='Retain existing staged portable terminal adaptation'))
    else:
        changes.append(portable_module.adapt_termcolor(source, overlay))
    copied = {str(source / relative): overlay / relative for relative in
              ('test/sqlite/result_helper.cpp', 'test/sqlite/sqllogic_test_logger.cpp')}
    for unit in result['units']:
        if unit['role'] != 'test':
            continue
        unit['arguments'] = ['-I', str(overlay / 'third_party/catch'),
                             '-DCATCH_CONFIG_NO_POSIX_SIGNALS'] + unit['arguments']
        if unit['source'] in copied:
            previous = Path(unit['source'])
            unit['source'] = str(copied[str(previous)])
            unit['arguments'] = ['-I', str(previous.parent)] + unit['arguments']
            changes.append(dict(path=unit['source'], original=str(previous),
                                originalSha256=sha(previous), stagedSha256=sha(Path(unit['source'])),
                                reason='Unchanged unit copied to resolve its adapted quoted terminal header'))
    result['testProfile'] = dict(name='portable-catch-ansi-v1', changes=changes,
        crashSignalInterception=False, testsOmitted=False, runtimeBindingVerified=False,
        notes=['Supported Catch flag disables OS crash-signal interception; ordinary assertions and C++ exception handling remain.',
               'No host OS macro is advertised. Existing ANSI/stdio adaptation uses the redirected-runner profile.',
               'fork/kill remain explicit imports requiring runtime support; no process-test execution is claimed.'])
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', required=True, type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    args = parser.parse_args()
    report = prepare(json.loads(args.manifest.read_text()), args.output_dir)
    report['testProfile']['previousManifest'] = str(args.manifest.resolve())
    report['testProfile']['previousManifestSha256'] = sha(args.manifest)
    path = args.output_dir.resolve() / 'project.json'
    path.write_text(json.dumps(report, indent=2) + '\n')
    print(json.dumps(dict(manifest=str(path), units=len(report['units']))))


if __name__ == '__main__':
    main()
