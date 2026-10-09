#!/usr/bin/env python3
"""Verify owned typed-throw generation; deliberately no runtime execution claim."""
import argparse
import hashlib
import importlib.util
import json
import re
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('compiler', 'packager', 'linker', 'output-dir'):
        parser.add_argument('--' + name, type=Path, required=True)
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    tooling = out / 'tooling'
    tooling.mkdir()
    for name in ('compile.py', 'schema.sql'):
        shutil.copy2(args.packager.resolve().parent / name, tooling / name)
    spec = importlib.util.spec_from_file_location('source_throw_packager', tooling / 'compile.py')
    packager = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(packager)
    source = out / 'cpp_source_throw.cpp'
    shutil.copy2(Path(__file__).with_name(source.name), source)
    checks, commands, rejections = [], [], []

    def check(label, condition):
        if not condition:
            raise AssertionError(label)
        checks.append(label)

    def run(label, command, code=0, diagnostic=None):
        command = list(map(str, command))
        result = subprocess.run(command, cwd=out, capture_output=True, timeout=600)
        (out / (label + '.stdout')).write_bytes(result.stdout)
        (out / (label + '.stderr')).write_bytes(result.stderr)
        commands.append(dict(name=label, command=command, exitCode=result.returncode))
        check(label, result.returncode == code)
        if diagnostic:
            check(label + '-diagnostic', diagnostic in result.stderr.decode(errors='replace'))
        return result

    error = None
    try:
        obj = out / 'source-throw.dotobj'
        run('package', [sys.executable, '-B', tooling / 'compile.py', source,
            '--output', obj, '--work-dir', out / 'compile', '--compiler', args.compiler.resolve(),
            '--unit-id', 'source-throw', '--exceptions', '--no-rtti'])
        run('verify', ['dotnet', args.linker.resolve(), 'verify', obj])
        with sqlite3.connect(obj) as db:
            check('owned-throws', db.execute('SELECT count(*) FROM throw_expressions').fetchone()[0] == 17)
            check('owned-dynamic-rethrow', db.execute('SELECT count(*) FROM rethrow_expressions').fetchone()[0] == 1)
            check('owned-start-markers', db.execute('SELECT count(*) FROM throw_markers').fetchone()[0] > 0)
            check('owned-payload-addresses', db.execute('SELECT count(*) FROM throw_object_addresses').fetchone()[0] > 0)
            check('owned-complete-destructors', db.execute('SELECT count(*) FROM throw_expressions WHERE destructor_entity_id IS NOT NULL').fetchone()[0] >= 2)
            check('nonzero-base-adjustment', db.execute('SELECT count(*) FROM exception_base_conversions WHERE byte_offset>0 AND is_public=1 AND is_ambiguous=0').fetchone()[0] > 0)
            check('private-base-fact', db.execute('SELECT count(*) FROM exception_base_conversions WHERE is_public=0').fetchone()[0] > 0)
            check('ambiguous-base-fact', db.execute('SELECT count(*) FROM exception_base_conversions WHERE is_ambiguous=1').fetchone()[0] > 0)
            descriptor = db.execute('SELECT id FROM exception_types ORDER BY id LIMIT 1').fetchone()[0]
            throw_node = db.execute('SELECT node_id FROM throw_expressions ORDER BY node_id LIMIT 1').fetchone()[0]
            destructible = db.execute('SELECT node_id FROM throw_expressions WHERE destructor_entity_id IS NOT NULL LIMIT 1').fetchone()[0]
            address_node, owner = db.execute('SELECT node_id,throw_node_id FROM throw_object_addresses LIMIT 1').fetchone()
            wrong_owner = db.execute('SELECT node_id FROM throw_expressions WHERE node_id<>? LIMIT 1', (owner,)).fetchone()[0]
        secondary = out / 'secondary.cpp'
        secondary.write_text('extern "C" void secondary_long(long value) { throw value; }\n'
            'using Alias = int;\nextern "C" void secondary_integer(Alias value) { throw value; }\n')
        second_obj = out / 'secondary.dotobj'
        run('package-secondary', [sys.executable, '-B', tooling / 'compile.py', secondary,
            '--output', second_obj, '--work-dir', out / 'secondary-compile', '--compiler', args.compiler.resolve(),
            '--unit-id', 'source-throw-secondary', '--exceptions', '--no-rtti'])
        source.rename(out / 'retained-source.cpp')
        secondary.rename(out / 'retained-secondary.cpp')
        run('strict-link-requires-provider', ['dotnet', args.linker.resolve(), 'link', '--out', out / 'strict', obj],
            1, 'Edg.Runtime.CppExceptions')
        generated = out / 'generated'
        run('source-hidden-generation', ['dotnet', args.linker.resolve(), 'link', '--out', generated,
            '--allow-externs', '--namespace', 'Owned.Exceptions', '--class', 'Core', obj, second_obj])
        report = json.loads((generated / 'link-report.json').read_text())
        runtime = report['sourceExceptionRuntime']
        check('explicit-unbound-provider', runtime['required'] and runtime['status'] == 'generation-only-unbound-provider')
        check('typed-and-ellipsis-handlers-supported', runtime['sourceHandlersSupported'] is True and runtime['sourceHandlerKinds'] == ['ellipsis', 'typed'])
        contract = (generated / 'Edg.Runtime.CppExceptions.generated.cs').read_text()
        check('required-partial-provider', 'partial' in contract and all(name in contract for name in
            ('Allocate', 'EnsureStarted', 'MarkComplete', 'ReleaseIncomplete', 'Rethrow')))
        check('owned-descriptor-output', (generated / 'Translated.Exceptions.generated.cs').is_file())
        descriptors = (generated / 'Translated.Exceptions.generated.cs').read_text()
        check('cross-unit-int-identity-coalesced', descriptors.count('"int", "int", "int",') == 1)
        check('cross-unit-long-identity-coalesced', descriptors.count('"long", "long", "long",') == 1)
        check('long-long-distinct-from-long', descriptors.count('"long-long", "long-long", "long-long",') == 1)
        generated_text = '\n'.join(path.read_text() for path in generated.glob('*.cs'))
        check('no-fabricated-terminal-return', 'Reached end of a non-void source function' not in generated_text)
        # No test runtime bodies are invented. The compiler may report only the
        # deliberately unbound required partial methods, not malformed C#.
        compilation = run('compile-requires-provider', ['dotnet', 'build', generated / 'Translated.csproj', '-c', 'Release'], 1)
        diagnostics = re.findall(r'error (CS\d+):', (compilation.stdout + compilation.stderr).decode(errors='replace'))
        check('only-unbound-partial-errors', bool(diagnostics) and set(diagnostics) == {'CS8795'})
        mutations = [
            ('payload-size-mismatch', [('UPDATE exception_types SET payload_size=payload_size+1 WHERE id=?', (descriptor,))]),
            ('required-destructor-missing', [('UPDATE throw_expressions SET destructor_entity_id=NULL WHERE node_id=?', (destructible,))]),
            ('wrong-payload-destructor', [('UPDATE throw_expressions SET destructor_entity_id=(SELECT entity_id FROM source_functions WHERE function_kind=\'constructor\' LIMIT 1) WHERE node_id=?', (destructible,))]),
            ('foreign-throw-payload-address', [('UPDATE throw_object_addresses SET throw_node_id=? WHERE node_id=?', (wrong_owner, address_node))]),
            ('missing-throw-proof', [
                ('DELETE FROM throw_object_addresses WHERE throw_node_id=?', (throw_node,)),
                ('DELETE FROM throw_markers WHERE throw_node_id=?', (throw_node,)),
                ('DELETE FROM throw_cleanup_transitions WHERE throw_node_id=?', (throw_node,)),
                ('DELETE FROM throw_initialization_boundaries WHERE throw_node_id=?', (throw_node,)),
                ('DELETE FROM throw_expressions WHERE node_id=?', (throw_node,))]),
            ('missing-rethrow-proof', [('DELETE FROM rethrow_expressions', ())]),
        ]
        for label, statements in mutations:
            bad = out / (label + '.dotobj')
            shutil.copy2(obj, bad)
            with sqlite3.connect(bad) as db:
                for sql, parameters in statements:
                    db.execute(sql, parameters)
                check(label + '-foreign-keys-valid', not db.execute('PRAGMA foreign_key_check').fetchall())
                check(label + '-schema-valid', db.execute('PRAGMA integrity_check').fetchall() == [('ok',)])
                db.execute('UPDATE content_digest SET sha256=?', (packager.logical_hash(db),))
            result = run(label, ['dotnet', args.linker.resolve(), 'verify', bad], 1)
            check(label + '-semantic-rejection', 'digest' not in result.stderr.decode(errors='replace').lower())
            rejections.append(label)
        unsupported = {
            'enum-identity': 'enum Code { value }; void reject() { throw value; }\n',
            'function-pointer-identity': 'void function() {} void reject() { throw &function; }\n',
            'virtual-exception-layout': 'struct Base {}; struct Derived: virtual Base {}; void reject() { throw Derived(); }\n',
            'payload-array-sequence-cleanup': '''extern void fail();
static int destroyed;
struct Member { Member(int value) { if(value==2) fail(); } ~Member() { ++destroyed; } };
struct Payload { Member values[2]; };
void reject() { throw Payload{{Member(1), Member(2)}}; }
''',
        }
        for label, text in unsupported.items():
            negative = out / (label + '.cpp')
            negative.write_text(text)
            output = out / (label + '.dotobj')
            run('unsupported-' + label, [sys.executable, '-B', tooling / 'compile.py', negative,
                '--output', output, '--work-dir', out / (label + '-compile'),
                '--compiler', args.compiler.resolve(), '--exceptions', '--no-rtti'], 1)
            check('unsupported-' + label + '-not-published', not output.exists())
    except Exception as exception:
        error = str(exception)
    receipt = dict(status='failed' if error else 'generation-only-passed', error=error,
        generatedCodeCompiled=False, generatedCodeExecuted=False, checks=checks, rejections=rejections,
        commands=commands, tools={name: dict(path=str(path.resolve()), sha256=hashlib.sha256(path.read_bytes()).hexdigest())
            for name, path in [('compiler', args.compiler), ('linker', args.linker)]})
    (out / 'results.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(dict(status=receipt['status'], checks=len(checks), error=error)))
    return bool(error)


if __name__ == '__main__':
    raise SystemExit(main())
