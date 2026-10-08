"""Runtime C semantics required by real project bodies."""
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute):
    objects = {}
    for name in ['project_c_runtime', 'project_c_intrinsics',
                 'project_c_record_values', 'project_c_record_array_values',
                 'project_c_aggregate_initializers', 'project_c_function_designators',
                 'project_c_bitfield_values', 'goto_entries', 'project_c_nested_switch']:
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
        elif name == 'project_c_intrinsics':
            with sqlite3.connect(obj) as db:
                operators = {row[0] for row in db.execute("SELECT operator FROM nodes WHERE kind='intrinsic'")}
                check('runtime-intrinsic-overflow-graph', any('overflow' in value for value in operators))
    return objects
