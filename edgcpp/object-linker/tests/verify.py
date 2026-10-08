#!/usr/bin/env python3
"""Real independent EDG objects, managed linking, JIT/AOT and rejection checks."""
import argparse
import hashlib
import importlib.util
import json
import platform
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location('dotobj_compile', ROOT / 'compile.py')
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--nativeaot', action='store_true')
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    inputs = out / 'inputs'
    inputs.mkdir()
    for name in ['fixture.h', 'provider.cpp', 'consumer.cpp', 'scopes.cpp', 'conversions.cpp', 'empty.cpp', 'loops.cpp', 'integers.cpp', 'source_types.h', 'source_types.cpp', 'macros.h', 'macros.cpp']:
        shutil.copy2(ROOT / 'tests' / name, inputs / name)
    shutil.copy2(ROOT / 'tests/loops.cpp', inputs / 'loops-c.c')
    shutil.copy2(ROOT / 'tests/integers.cpp', inputs / 'integers-c.c')
    shutil.copy2(ROOT / 'tests/source_types.cpp', inputs / 'source-types-c.c')
    shutil.copy2(ROOT / 'tests/macros.cpp', inputs / 'macros-c.c')
    checks = []

    def save(status):
        (out / 'results.json').write_text(json.dumps(dict(status=status, checks=checks,
            profile='scalar-control-v5', nativeAotRequested=args.nativeaot,
            platform=platform.platform(), implementation='fresh-owned-semantic-graph',
            limitations=['No full prelower source view, macro expansion history, lifetime/EH or globals yet.',
                         'No complete DuckDB or general C++ acceptance.']), indent=2) + '\n')

    def run(label, command, expected=0, contains=None):
        command = [str(x) for x in command]
        result = subprocess.run(command, cwd=out, capture_output=True, text=True)
        (out / (label + '.stdout')).write_text(result.stdout)
        (out / (label + '.stderr')).write_text(result.stderr)
        passed = (result.returncode != 0 if expected is None else result.returncode == expected)
        if contains is not None:
            passed = passed and contains.lower() in (result.stdout + result.stderr).lower()
        checks.append(dict(name=label, command=command, exitCode=result.returncode, passed=passed))
        save('running' if passed else 'failed')
        if not passed:
            raise RuntimeError(label + ' failed; inspect retained logs at ' + str(out))
        print(label + ': passed', flush=True)
        return result

    def check(label, condition):
        checks.append(dict(name=label, passed=bool(condition)))
        save('running' if condition else 'failed')
        if not condition:
            raise RuntimeError(label)

    def compile_source(name, source, output=None, expected=0, unit_id=None, std='c++17', extra=()):
        output = output or out / (name + '.dotobj')
        run('compile-' + name, [sys.executable, '-B', ROOT / 'compile.py', source,
            '--output', output, '--work-dir', out / (name + '-compile'), '--unit-id', unit_id or name, '--std', std, *extra], expected)
        return output

    try:
        run('tool-build', ['dotnet', 'build', ROOT / 'managed/DotObj.Linker.csproj',
            '-c', 'Release', '--artifacts-path', out / 'tool'])
        tool = out / 'tool/bin/DotObj.Linker/release/dotobj-link.dll'
        if not tool.exists():
            raise RuntimeError('Expected linker build output missing')
        provider = compile_source('provider', inputs / 'provider.cpp')
        consumer = compile_source('consumer', inputs / 'consumer.cpp')
        scopes = compile_source('scopes', inputs / 'scopes.cpp')
        for name, obj in [('provider', provider), ('consumer', consumer), ('scopes', scopes)]:
            run('verify-' + name, ['dotnet', tool, 'verify', obj])
            invocation = json.loads((out / (name + '-compile/invocation.json')).read_text())
            check('direct-source-' + name, invocation['arguments'][-1] == str(inputs / (name + '.cpp'))
                  and '--preprocess' not in invocation['arguments'])
        run('native-build', ['c++', '-std=c++17', inputs / 'provider.cpp', inputs / 'consumer.cpp', '-o', out / 'native'])
        run('native-run', [out / 'native'])
        run('native-scopes-build', ['c++', '-std=c++17', inputs / 'scopes.cpp', '-o', out / 'native-scopes'])
        run('native-scopes-run', [out / 'native-scopes'])

        def link(label, objects, entry='main', namespace='Tests.Linked', cls='Core', expected=0, contains=None):
            directory = out / label
            run(label, ['dotnet', tool, 'link', '--out', directory, '--namespace', namespace,
                        '--class', cls, '--entry', entry, *objects], expected, contains)
            return directory

        def execute(label, directory, expected=0):
            run(label + '-build', ['dotnet', 'build', directory / 'Translated.csproj', '-c', 'Release'])
            run(label + '-jit', ['dotnet', directory / 'bin/Release/net10.0/Translated.dll'], expected)
            if args.nativeaot:
                machine = {'x86_64': 'x64', 'aarch64': 'arm64'}.get(platform.machine())
                system = {'Linux': 'linux', 'Darwin': 'osx', 'Windows': 'win'}.get(platform.system())
                if not machine or not system:
                    raise RuntimeError('Unrecognized NativeAOT host RID')
                publish = out / (label + '-aot')
                run(label + '-publish', ['dotnet', 'publish', directory / 'Translated.csproj',
                    '-c', 'Release', '-r', system + '-' + machine, '-p:PublishAot=true', '-o', publish])
                run(label + '-aot-run', [publish / ('Translated.exe' if system == 'win' else 'Translated')], expected)

        linked = link('link', [provider, consumer])
        execute('linked', linked)
        scoped = link('link-scopes', [scopes])
        execute('scopes', scoped)
        for name in ['conversions', 'empty', 'loops', 'integers', 'source_types', 'macros']:
            obj = compile_source(name, inputs / (name + '.cpp'))
            run('native-' + name + '-build', ['c++', '-std=c++17', inputs / (name + '.cpp'), '-o', out / ('native-' + name)])
            run('native-' + name + '-run', [out / ('native-' + name)])
            execute(name, link('link-' + name, [obj]))
        loops_object = out / 'loops.dotobj'
        loops_cs = '\n'.join(p.read_text() for p in (out / 'link-loops').glob('*.cs'))
        check('structured-loop-source', all(token in loops_cs for token in ['for (', 'do', 'while (', 'break;', 'continue;']) and 'goto ' not in loops_cs)
        with sqlite3.connect(loops_object) as db:
            check('explicit-loop-targets', db.execute("SELECT count(*) FROM nodes WHERE kind IN ('break','continue')").fetchone()[0] > 0 and db.execute("SELECT count(*) FROM nodes n LEFT JOIN edges e ON e.parent_id=n.id AND e.role='target' LEFT JOIN nodes t ON t.id=e.child_id WHERE n.kind IN ('break','continue') AND (t.id IS NULL OR t.kind NOT IN ('for','while','do-while'))").fetchone()[0] == 0)
        loops_c = compile_source('loops-c', inputs / 'loops-c.c', std='c11')
        run('native-loops-c-build', ['cc', '-std=c11', inputs / 'loops-c.c', '-o', out / 'native-loops-c'])
        run('native-loops-c-run', [out / 'native-loops-c'])
        execute('loops-c', link('link-loops-c', [loops_c]))
        integers_c = compile_source('integers-c', inputs / 'integers-c.c', std='c11')
        run('native-integers-c-build', ['cc', '-std=c11', inputs / 'integers-c.c', '-o', out / 'native-integers-c'])
        run('native-integers-c-run', [out / 'native-integers-c'])
        execute('integers-c', link('link-integers-c', [integers_c]))
        source_c = compile_source('source-types-c', inputs / 'source-types-c.c', std='c11')
        run('native-source-types-c-build', ['cc', '-std=c11', inputs / 'source-types-c.c', '-o', out / 'native-source-types-c'])
        run('native-source-types-c-run', [out / 'native-source-types-c'])
        execute('source-types-c', link('link-source-types-c', [source_c]))
        for language, obj in [('c++', out / 'source_types.dotobj'), ('c', source_c)]:
            with sqlite3.connect(obj) as db:
                kinds = {row[0] for row in db.execute('SELECT kind FROM source_types')}
                aliases = {row[0] for row in db.execute("SELECT source_name FROM source_types WHERE kind='typedef'")}
                check('source-type-kinds-' + language, {'long','unsigned-long-long','bool','typedef'} <= kinds)
                check('source-typedef-chain-' + language, {'Count','ItemCount','Mask','Toggle'} <= aliases)
                check('source-const-' + language, db.execute('SELECT count(*) FROM source_types WHERE is_const=1').fetchone()[0] > 0)
                check('source-bool-before-lowering-' + language, db.execute("SELECT count(*) FROM source_types s JOIN types t ON t.id=s.execution_type_id WHERE s.kind='bool' AND t.kind='u8'").fetchone()[0] > 0)
                check('distinct-shadowed-source-types-' + language, db.execute("SELECT count(DISTINCT execution_type_id) FROM source_types WHERE kind='typedef' AND source_name='Choice'").fetchone()[0] == 2)
                check('source-binding-coverage-' + language, db.execute("SELECT count(*) FROM source_declarations WHERE kind IN ('local','parameter')").fetchone()[0] == db.execute('SELECT count(*) FROM bindings').fetchone()[0])
        check('source-map-published', (out / 'link-source_types/source-map.json').is_file())
        macros_c = compile_source('macros-c', inputs / 'macros-c.c', std='c11')
        run('native-macros-c-build', ['cc', '-std=c11', inputs / 'macros-c.c', '-o', out / 'native-macros-c'])
        run('native-macros-c-run', [out / 'native-macros-c'])
        execute('macros-c', link('link-macros-c', [macros_c]))
        for language, obj in [('c++', out / 'macros.dotobj'), ('c', macros_c)]:
            with sqlite3.connect(obj) as db:
                names = {row[0] for row in db.execute('SELECT name FROM macro_definitions')}
                check('active-macro-definitions-' + language, {'API_VALUE','API_ADD','API_STRING','API_CAT','API_VARIADIC','API_EMPTY','API_MULTI','API_UTF8','API_OPTIONAL_TEXT'} <= names and 'API_INACTIVE' not in names)
                events = db.execute("SELECT e.kind,d.definition_text FROM macro_events e LEFT JOIN macro_definitions d ON d.id=e.definition_id WHERE e.name='API_VALUE' ORDER BY e.id").fetchall()
                check('macro-redefinition-order-' + language, [x[0] for x in events] == ['define','undef','define'] and '17' in events[0][1] and '23' in events[2][1])
                check('macro-unknown-undef-' + language, db.execute("SELECT count(*) FROM macro_events WHERE name='API_NEVER_DEFINED' AND kind='undef'").fetchone()[0] == 1)
                check('macro-identical-redefinition-' + language, db.execute("SELECT count(*) FROM macro_definitions WHERE name='API_SAME'").fetchone()[0] == 1)
                check('macro-parameters-' + language, [x[0] for x in db.execute("SELECT p.name FROM macro_parameters p JOIN macro_definitions d ON d.id=p.definition_id WHERE d.name='API_ADD' ORDER BY p.ordinal")] == ['a','b'])
                check('macro-paste-stringize-' + language, '##' in db.execute("SELECT definition_text FROM macro_definitions WHERE name='API_CAT'").fetchone()[0] and '#' in db.execute("SELECT definition_text FROM macro_definitions WHERE name='API_STRING'").fetchone()[0].split(')',1)[1])
                check('macro-variadic-' + language, db.execute("SELECT variadic FROM macro_definitions WHERE name='API_VARIADIC'").fetchone()[0] == 1 and db.execute("SELECT count(*) FROM macro_parameters p JOIN macro_definitions d ON d.id=p.definition_id WHERE d.name='API_VARIADIC' AND p.name='__VA_ARGS__'").fetchone()[0] == 1)
                check('macro-source-locations-' + language, db.execute("SELECT count(*) FROM macro_definitions WHERE name LIKE 'API_%' AND (origin<>'source' OR source_file_id IS NULL OR line=0 OR column=0)").fetchone()[0] == 0)
        check('macro-map-published', (out / 'link-macros/macro-map.json').is_file())
        commandline_source = inputs / 'macro-commandline.cpp'
        commandline_source.write_text('#ifdef API_REMOVED\n#error removal failed\n#endif\nint main(void){return API_COMMAND - 41;}\n#undef API_COMMAND\n')
        command_flags = ['-DAPI_COMMAND=41', '-DAPI_REMOVED=9', '-UAPI_REMOVED']
        command_obj = compile_source('macro-commandline', commandline_source, extra=command_flags)
        run('native-macro-commandline-build', ['c++', '-std=c++17', *command_flags, commandline_source, '-o', out / 'native-macro-commandline'])
        run('native-macro-commandline-run', [out / 'native-macro-commandline'])
        execute('macro-commandline', link('link-macro-commandline', [command_obj]))
        with sqlite3.connect(command_obj) as db:
            check('macro-commandline-origin', db.execute("SELECT origin FROM macro_definitions WHERE name='API_COMMAND'").fetchone()[0] == 'command-line')
            check('macro-commandline-undef', db.execute("SELECT count(*) FROM macro_events WHERE name='API_REMOVED' AND kind='undef' AND origin='command-line'").fetchone()[0] == 1)
        integer_cs = '\n'.join(p.read_text() for p in (out / 'link-integers').glob('*.cs'))
        check('direct-integer-operators', all(token in integer_cs for token in [' ? ', ' : ', ' << ', ' >> ', ' & ', ' | ', ' ^ ', '~']))
        reverse = link('link-reversed', [consumer, provider])
        cs = lambda path: {p.name: p.read_bytes() for p in path.glob('*.cs')}
        check('stable-output-input-order', cs(linked) == cs(reverse))

        # Objects contain source snapshots. Linking must not reopen source paths.
        inputs.rename(out / 'inputs-preserved')
        relocated = link('link-without-sources', [provider, consumer], namespace='Different.Output', cls='Engine')
        execute('without-sources', relocated)
        check('source-files-remain-unavailable', not inputs.exists())

        run('unresolved-import', ['dotnet', tool, 'link', '--out', out / 'bad-unresolved', consumer], None, 'Unresolved')
        run('duplicate-unit', ['dotnet', tool, 'link', '--out', out / 'bad-unit', provider, provider], None, 'translation unit')

        def mutate(name, original, sql, repair=True):
            path = out / (name + '.dotobj')
            shutil.copy2(original, path)
            with sqlite3.connect(path) as db:
                for query, params in sql:
                    db.execute(query, params)
                if repair:
                    db.execute('UPDATE content_digest SET sha256=?', (MODULE.logical_hash(db),))
            return path

        duplicate = mutate('duplicate', provider, [('UPDATE object_info SET unit_id=?', ('duplicate',))])
        run('duplicate-strong', ['dotnet', tool, 'link', '--out', out / 'bad-strong', provider, duplicate], None, 'Duplicate strong')
        mismatch = mutate('signature', consumer, [
            ('UPDATE parameters SET type_id=3 WHERE entity_id=(SELECT id FROM entities WHERE source_name=?)', ('from_a',)),
            ("INSERT INTO source_types SELECT max(id)+1,'unsigned-char','',NULL,3,0,0,NULL,0,0 FROM source_types", ()),
            ("UPDATE source_parameters SET source_type_id=(SELECT max(id) FROM source_types) WHERE entity_id=(SELECT id FROM entities WHERE source_name='from_a')", ())])
        run('signature-mismatch', ['dotnet', tool, 'link', '--out', out / 'bad-signature', provider, mismatch], None, 'Signature mismatch')
        damaged = mutate('damaged', provider, [('UPDATE entities SET source_name=? WHERE id=1', ('tampered',))], False)
        run('digest-mismatch', ['dotnet', tool, 'verify', damaged], None, 'digest')
        bad_target = mutate('target', provider, [('UPDATE object_info SET target_abi=?', ('unknown-target',))])
        run('unknown-target', ['dotnet', tool, 'verify', bad_target], None)
        bad_edge = mutate('edge', provider, [('UPDATE edges SET role=? WHERE rowid=(SELECT min(rowid) FROM edges)', ('invented',))])
        run('invalid-edge', ['dotnet', tool, 'verify', bad_edge], None)
        bad_kind = mutate('node', provider, [('UPDATE nodes SET kind=? WHERE id=(SELECT min(id) FROM nodes)', ('raw-csharp',))])
        run('invalid-node-kind', ['dotnet', tool, 'verify', bad_kind], None)
        bad_snapshot = mutate('snapshot', provider, [('UPDATE source_files SET bytes=? WHERE id=1', (b'changed',))])
        run('snapshot-mismatch', ['dotnet', tool, 'verify', bad_snapshot], None)
        bad_primary = mutate('primary-hash', provider, [('UPDATE object_info SET source_sha256=?', ('0' * 64,))])
        run('primary-provenance-mismatch', ['dotnet', tool, 'verify', bad_primary], None)
        bad_schema = mutate('schema', provider, [('ALTER TABLE nodes ADD COLUMN hidden_behavior TEXT', ())])
        run('extra-schema-payload', ['dotnet', tool, 'verify', bad_schema], None)

        wrong_target = mutate('loop-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT body_node_id FROM functions WHERE entity_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('nonloop-exit-target', ['dotnet', tool, 'verify', wrong_target], None)
        missing_target = mutate('loop-missing-target', loops_object, [(
            "DELETE FROM edges WHERE role='target' AND parent_id=(SELECT min(id) FROM nodes WHERE kind IN ('break','continue'))", ())])
        run('missing-loop-exit-target', ['dotnet', tool, 'verify', missing_target], None)
        outer_target = mutate('loop-outer-target', loops_object, [(
            "UPDATE edges SET child_id=(SELECT min(id) FROM nodes WHERE kind='for' AND function_id=(SELECT function_id FROM nodes WHERE id=edges.parent_id)) WHERE role='target' AND parent_id IN (SELECT id FROM nodes WHERE kind IN ('break','continue')) AND child_id IN (SELECT id FROM nodes WHERE kind='for')", ())])
        run('wrong-lexical-loop-target', ['dotnet', tool, 'verify', outer_target], None)

        integers_object = out / 'integers.dotobj'
        invalid_integer = mutate('wide-constant', integers_object, [(
            "UPDATE nodes SET constant_text='18446744073709551616' WHERE id=(SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=(SELECT id FROM types WHERE kind='u64'))", ())])
        run('wide-constant-out-of-range', ['dotnet', tool, 'verify', invalid_integer], None)
        invalid_narrow = mutate('narrow-constant', integers_object, [(
            "UPDATE nodes SET constant_text='128' WHERE id=(SELECT min(id) FROM nodes WHERE kind='constant' AND type_id=(SELECT id FROM types WHERE kind='i8'))", ())])
        run('signed-byte-constant-out-of-range', ['dotnet', tool, 'verify', invalid_narrow], None)
        invalid_truth = mutate('comparison-result', integers_object, [(
            "UPDATE nodes SET type_id=(SELECT id FROM types WHERE kind='i64') WHERE id=(SELECT min(id) FROM nodes WHERE kind='binary' AND operator='ne')", ())])
        run('comparison-result-type', ['dotnet', tool, 'verify', invalid_truth], None)
        bad_conditional = mutate('conditional-edge', integers_object, [(
            "DELETE FROM edges WHERE role='else' AND parent_id=(SELECT min(id) FROM nodes WHERE kind='conditional')", ())])
        run('conditional-missing-arm', ['dotnet', tool, 'verify', bad_conditional], None)
        bad_result = mutate('conditional-type', integers_object, [(
            "UPDATE nodes SET type_id=(SELECT id FROM types WHERE kind='void') WHERE id=(SELECT min(id) FROM nodes WHERE kind='conditional')", ())])
        run('conditional-invalid-result', ['dotnet', tool, 'verify', bad_result], None)

        source_object = out / 'source_types.dotobj'
        wrong_source_type = mutate('source-physical-type', source_object, [(
            "UPDATE source_types SET execution_type_id=(SELECT id FROM types WHERE kind='i64') WHERE kind='bool'", ())])
        run('source-physical-mismatch', ['dotnet', tool, 'verify', wrong_source_type], None)
        cyclic_source = mutate('source-alias-cycle', source_object, [(
            "UPDATE source_types SET underlying_type_id=id WHERE kind='typedef'", ())])
        run('source-type-cycle', ['dotnet', tool, 'verify', cyclic_source], None)
        missing_source = mutate('source-binding-missing', source_object, [(
            "DELETE FROM source_declarations WHERE id=(SELECT min(id) FROM source_declarations WHERE kind='local')", ())])
        run('source-binding-missing', ['dotnet', tool, 'verify', missing_source], None)
        wrong_phase = mutate('source-capture-phase', source_object, [(
            "UPDATE source_functions SET capture_phase='after-lowering'", ())])
        run('source-capture-phase', ['dotnet', tool, 'verify', wrong_phase], None)

        macros_object = out / 'macros.dotobj'
        wrong_macro_event = mutate('macro-event-name', macros_object, [(
            "UPDATE macro_events SET name='DIFFERENT' WHERE id=(SELECT min(id) FROM macro_events WHERE kind='define')", ())])
        run('macro-event-name-mismatch', ['dotnet', tool, 'verify', wrong_macro_event], None)
        orphan_macro = mutate('macro-orphan', macros_object, [(
            "DELETE FROM macro_events WHERE definition_id=(SELECT id FROM macro_definitions WHERE name='API_EMPTY')", ())])
        run('macro-definition-event-missing', ['dotnet', tool, 'verify', orphan_macro], None)
        wrong_macro_origin = mutate('macro-origin', macros_object, [(
            "UPDATE macro_definitions SET origin='predefined' WHERE name='API_VALUE'", ())])
        run('macro-origin-location-mismatch', ['dotnet', tool, 'verify', wrong_macro_origin], None)
        wrong_macro_form = mutate('macro-object-params', macros_object, [(
            "UPDATE macro_definitions SET form='object' WHERE name='API_ADD'", ())])
        run('macro-object-parameter-mismatch', ['dotnet', tool, 'verify', wrong_macro_form], None)
        wrong_macro_ordinal = mutate('macro-param-ordinal', macros_object, [(
            "UPDATE macro_parameters SET ordinal=10 WHERE definition_id=(SELECT id FROM macro_definitions WHERE name='API_ADD') AND ordinal=1", ())])
        run('macro-parameter-order', ['dotnet', tool, 'verify', wrong_macro_ordinal], None)

        negatives = {
            'va-opt': '#define API_OPT(...) __VA_OPT__(17)\nint f(){return API_OPT(1); }\n',
            'push-macro': '#define VALUE 1\n#pragma push_macro("VALUE")\nint f(){return VALUE;}\n',
            'pop-macro': '#pragma pop_macro("VALUE")\nint f(){return 1;}\n',
            'global': 'int storage=1; int f(){return storage;}\n',
            'pointer': 'int f(int* value){return *value;}\n',
            'volatile': 'int f(){volatile int value=1; return value;}\n',
            'inline': 'inline int f(){return 1;} int g(){return f();}\n',
            'noexcept': 'int f() noexcept {return 1;}\n',
            'switch': 'int f(int n){switch(n){case 1:return 2;default:return 3;}}\n',
            'goto': 'int f(){int n=0; again: ++n; if(n<3)goto again; return n;}\n',
        }
        for name, code in negatives.items():
            source = out / ('unsupported-' + name + '.cpp')
            source.write_text(code)
            obj = compile_source('unsupported-' + name, source, expected=None)
            check('not-published-' + name, not obj.exists())
            if name == 'va-opt':
                check('unsupported-va-opt-provenance', '__VA_OPT__' in (out / ('unsupported-' + name + '-compile/stderr')).read_text())

        # One changed TU can be recompiled and relinked with the unchanged other object.
        shutil.copytree(out / 'inputs-preserved', inputs)
        changed = inputs / 'provider.cpp'
        changed.write_text(changed.read_text().replace('event + 1', 'event + 2'))
        changed_object = compile_source('provider-new', changed, unit_id='provider')
        incremental = link('link-incremental', [changed_object, consumer])
        run('native-new-build', ['c++', '-std=c++17', changed, inputs / 'consumer.cpp', '-o', out / 'native-new'])
        run('native-new-run', [out / 'native-new'], 3)
        execute('incremental', incremental, 3)
        save('passed')
        print(json.dumps(dict(status='passed', checks=len(checks), output=str(out))))
    except Exception:
        save('failed')
        raise


if __name__ == '__main__':
    main()
