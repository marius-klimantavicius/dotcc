"""Runtime C semantics required by real project bodies."""
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute):
    objects = {}
    for name in ['project_c_runtime', 'project_c_intrinsics',
                 'project_c_record_values', 'project_c_record_array_values',
                 'project_c_aggregate_initializers', 'project_c_function_designators',
                 'project_c_bitfield_values', 'goto_entries', 'project_c_nested_switch',
                 'project_c_synthetic_bindings', 'project_c_binary80', 'project_c_folded_records',
                 'project_c_unspecified_definitions', 'project_c_volatile_binary80',
                 'project_c_void_sequences', 'project_c_macro_record_identity']:
        source = inputs / (name + '.c')
        obj = compile_source(name, source, std='c11')
        run(name + '-native-build', ['cc', '-std=c11', '-O2', source, '-o', out / (name + '-native')])
        run(name + '-native-run', [out / (name + '-native')])
        run(name + '-verify', ['dotnet', tool, 'verify', obj])
        directory = link('link-' + name, [obj])
        execute(name, directory)
        objects[name] = obj
        if name == 'project_c_runtime':
            code = '\n'.join(path.read_text() for path in directory.glob('*.cs'))
            check('runtime-shared-va-abi', 'params global::System.ReadOnlySpan<global::Edg.Runtime.VaArg>' in code)
            check('runtime-no-varcall-helpers', '__edg_varcall' not in code and '__DotObjVarCall' not in code)
            check('runtime-volatile-access-preserved', 'Volatile.Read' in code or 'volatile int' in code)
            with sqlite3.connect(obj) as db:
                check('runtime-typed-va-list', db.execute("SELECT count(*) FROM types WHERE kind='va-list'").fetchone()[0] == 1)
                check('runtime-volatile-graph', db.execute("SELECT count(*) FROM nodes WHERE kind='volatile-place'").fetchone()[0] >= 7)
                check('runtime-comma-graph', db.execute("SELECT count(*) FROM nodes WHERE kind='sequence'").fetchone()[0] >= 2)
        elif name == 'project_c_synthetic_bindings':
            with sqlite3.connect(obj) as db:
                check('explicit-synthetic-local-provenance', db.execute('SELECT count(*) FROM bindings WHERE is_synthetic=1').fetchone()[0] > 0)
                check('no-fabricated-source-declarations', db.execute('SELECT count(*) FROM bindings b JOIN source_declarations d ON d.binding_id=b.id WHERE b.is_synthetic=1').fetchone()[0] == 0)
        elif name == 'project_c_binary80':
            with sqlite3.connect(obj) as db:
                check('binary80-execution-type', db.execute("SELECT count(*) FROM types WHERE kind='f80'").fetchone()[0] == 1)
                check('binary80-native-aligned-record', db.execute('SELECT count(*) FROM record_types WHERE alignment=16').fetchone()[0] > 0)
                check('binary80-exact-constant-payload', db.execute("SELECT count(*) FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.kind='constant' AND t.kind='f80' AND length(n.constant_text)<>20").fetchone()[0] == 0)
            check('binary80-shared-runtime', (directory / 'Float80.cs').exists() or any('readonly struct Float80' in p.read_text() for p in directory.glob('*.cs')))
        elif name == 'project_c_intrinsics':
            with sqlite3.connect(obj) as db:
                operators = {row[0] for row in db.execute("SELECT operator FROM nodes WHERE kind='intrinsic'")}
                check('runtime-intrinsic-overflow-graph', any('overflow' in value for value in operators))
    return objects


def malformed(objects, tool, run, mutate):
    synthetic = objects['project_c_synthetic_bindings']
    original_missing = mutate('synthetic-marker-missing', synthetic, [(
        'UPDATE bindings SET is_synthetic=0 WHERE id=(SELECT min(id) FROM bindings WHERE is_synthetic=1)', ())])
    run('synthetic-marker-missing', ['dotnet', tool, 'verify', original_missing], None)
    binary80 = objects['project_c_binary80']
    bad_constant = mutate('binary80-invalid-constant', binary80, [(
        "UPDATE nodes SET constant_text='000000000000000000000' WHERE id=(SELECT min(n.id) FROM nodes n JOIN types t ON t.id=n.type_id WHERE n.kind='constant' AND t.kind='f80')", ())])
    run('binary80-invalid-constant', ['dotnet', tool, 'verify', bad_constant], None)
    bad_alignment = mutate('binary80-invalid-record-alignment', binary80, [(
        'UPDATE record_types SET alignment=8 WHERE type_id=(SELECT min(type_id) FROM record_types WHERE alignment=16)', ())])
    run('binary80-invalid-record-alignment', ['dotnet', tool, 'verify', bad_alignment], None)
