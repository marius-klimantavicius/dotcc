"""Original class-value signatures and observed lowered result/parameter slots."""
from pathlib import Path
import hashlib
import shutil
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    directory = out / 'indirect-value-source'
    directory.mkdir()
    source = directory / 'project_cpp_indirect_value_abi.cpp'
    shutil.copy2(inputs / source.name, source)
    for optimization in ['0', '2']:
        native = out / ('indirect-values-native-' + optimization)
        run('indirect-values-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, source, '-o', native])
        run('indirect-values-native-' + optimization, [native])
    obj = compile_source('indirect-values', source, extra=['--no-exceptions', '--no-rtti'])
    run('verify-indirect-values', ['dotnet', tool, 'verify', obj])
    with sqlite3.connect(obj) as db:
        check('indirect-values-original-cpp-language', db.execute('SELECT source_language FROM object_info').fetchone()[0] == 'c++')
        check('indirect-values-all-original-routines-have-abi-proof', db.execute('''
            SELECT count(*) FROM source_routine_types t LEFT JOIN source_routine_abis a ON a.source_routine_type_id=t.type_id
            WHERE a.source_routine_type_id IS NULL OR a.capture_proof<>'observed-routine-lowering' ''').fetchone()[0] == 0)
        routine = db.execute('''SELECT f.routine_source_type_id FROM source_functions f JOIN entities e ON e.id=f.entity_id
            WHERE e.source_name='successor' ''').fetchone()[0]
        slots = db.execute('''SELECT execution_ordinal,parameter_role,source_ordinal FROM source_routine_abi_parameters
            WHERE source_routine_type_id=? ORDER BY execution_ordinal''', (routine,)).fetchall()
        check('indirect-values-result-before-receiver-and-original-argument', slots == [(0, 'result-buffer', None), (1, 'this', None), (2, 'source', 0)])
        rows = db.execute('''SELECT b.id,b.is_synthetic,t.kind,d.id FROM bindings b
            JOIN parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal
            LEFT JOIN source_declarations d ON d.binding_id=b.id LEFT JOIN source_types t ON t.id=d.source_type_id
            WHERE b.kind='parameter' AND p.parameter_role='indirect-source' ''').fetchall()
        check('indirect-values-original-value-parameter-retained', len(rows) > 0 and all(row[1] == 0 and row[2] == 'record' and row[3] is not None for row in rows))
        check('indirect-values-result-bindings-not-fabricated-source', db.execute('''SELECT count(*) FROM bindings b
            JOIN parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal LEFT JOIN source_declarations d ON d.binding_id=b.id
            WHERE b.kind='parameter' AND p.parameter_role='result-buffer' AND (b.is_synthetic<>1 OR d.id IS NOT NULL)''').fetchone()[0] == 0)
        zero = db.execute('''SELECT a.source_routine_type_id FROM source_routine_abis a
            WHERE NOT EXISTS (SELECT 1 FROM source_routine_abi_parameters p WHERE p.source_routine_type_id=a.source_routine_type_id)
            AND NOT EXISTS (SELECT 1 FROM source_functions f WHERE f.routine_source_type_id=a.source_routine_type_id) LIMIT 1''').fetchone()
        check('indirect-values-callback-only-zero-parameter-proof', zero is not None)
        zero = zero[0]
        indirect_routine = db.execute("SELECT source_routine_type_id FROM source_routine_abi_parameters WHERE parameter_role='indirect-source' LIMIT 1").fetchone()[0]
        indirect_binding = rows[0][0]
        result_binding = db.execute('''SELECT b.id FROM bindings b JOIN parameters p
            ON p.entity_id=b.function_id AND p.ordinal=b.ordinal WHERE b.kind='parameter' AND p.parameter_role='result-buffer' LIMIT 1''').fetchone()[0]
    generated = link('link-indirect-values', [obj])
    execute('indirect-values', generated)
    check('indirect-values-readable-result-name', any('__result' in path.read_text() for path in generated.glob('Translated.*.cs')))
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in generated.glob('*.cs')}
    directory.rename(out / 'retained-indirect-value-source')
    source_free = link('link-indirect-values-source-free', [obj])
    check('indirect-values-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in source_free.glob('*.cs')})
    mutations = [
        ('indirect-values-missing-zero-parameter-proof', [('DELETE FROM source_routine_abis WHERE source_routine_type_id=?', (zero,))], 'Missing original routine ABI header'),
        ('indirect-values-false-direct-result', [("UPDATE source_routine_abis SET return_passing='direct' WHERE source_routine_type_id=?", (routine,))], 'result/source mapping is incomplete'),
        ('indirect-values-swapped-receiver-result', [("UPDATE source_routine_abi_parameters SET parameter_role=CASE parameter_role WHEN 'this' THEN 'result-buffer' WHEN 'result-buffer' THEN 'this' ELSE parameter_role END WHERE source_routine_type_id=?", (routine,))], 'Execution parameter role disagrees'),
        ('indirect-values-erased-parameter-indirection', [("UPDATE source_routine_abi_parameters SET parameter_role='source' WHERE source_routine_type_id=? AND parameter_role='indirect-source'", (indirect_routine,))], 'physical parameter type'),
        ('indirect-values-forged-original-synthetic', [('UPDATE bindings SET is_synthetic=1 WHERE id=?', (indirect_binding,))], 'source parameters must be original'),
        ('indirect-values-forged-c-source-proof', [("UPDATE source_routine_abis SET capture_proof='source-c-signature' WHERE source_routine_type_id=?", (routine,))], 'capture proof does not establish'),
        ('indirect-values-fabricated-result-declaration', [('''INSERT INTO source_declarations
            SELECT (SELECT max(id)+1 FROM source_declarations),'parameter',b.source_name,f.return_source_type_id,
                   NULL,b.id,b.function_id,b.source_file_id,b.line,b.column
            FROM bindings b JOIN source_functions f ON f.entity_id=b.function_id WHERE b.id=?''', (result_binding,))], 'Synthetic ABI parameter cannot have an original source declaration'),
    ]
    for label, sql, diagnostic in mutations:
        bad = mutate(label, obj, sql)
        run('reject-' + label, ['dotnet', tool, 'verify', bad], None, diagnostic)
    return obj
