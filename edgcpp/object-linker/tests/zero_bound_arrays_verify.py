"""GNU fixed [0] arrays remain distinct from unspecified [] owned types."""
import hashlib
from pathlib import Path
import re
import shutil
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    directory = out / 'zero-bound-source'
    directory.mkdir()
    for name in ['zero_bound_arrays.h', 'zero_bound_arrays_provider.cpp', 'zero_bound_arrays_main.cpp']:
        shutil.copy2(inputs / name, directory / name)
    sources = [directory / ('zero_bound_arrays_' + role + '.cpp') for role in ['provider', 'main']]
    for optimization in ['0', '2']:
        native = out / ('zero-bound-native-' + optimization)
        run('zero-bound-native-build-' + optimization,
            ['c++', '-std=gnu++17', '-O' + optimization, *sources, '-o', native])
        run('zero-bound-native-' + optimization, [native])

    # Preserve a second oracle using the stock C backend's native layout and
    # pointer strides. This compiler is intentionally distinct from cpfe-dot.
    stock_compiler = Path(__file__).resolve().parents[2] / 'build/bin/cpfe'
    stock_sources = []
    for role, source in zip(['provider', 'main'], sources):
        run('zero-bound-stock-' + role, [stock_compiler, '--c++17', '--gnu_version', '130300',
            '--clear_flag', 'use_predefined_macro_file', '--no_exceptions', '--no_rtti', source])
        lowered = source.with_suffix('.int.c')
        if not lowered.exists():
            lowered = out / lowered.name
        check('zero-bound-stock-' + role + '-retained', lowered.exists())
        stock_sources.append(lowered)
    stock = out / 'zero-bound-stock-c'
    run('zero-bound-stock-build', ['cc', '-O2', *stock_sources, '-o', stock])
    run('zero-bound-stock-run', [stock])

    objects = [compile_source('zero-bound-' + role, source, extra=['--no-exceptions', '--no-rtti'])
               for role, source in zip(['provider', 'main'], sources)]
    identities = []
    for role, obj in zip(['provider', 'main'], objects):
        run('verify-zero-bound-' + role, ['dotnet', tool, 'verify', obj])
        with sqlite3.connect(obj) as db:
            records = {row[0]: row[1:] for row in db.execute('''
                SELECT source_name,identity_key,size,alignment FROM record_types
                WHERE source_name IN ('ZeroInterior','ZeroMatrix','ZeroAligned','ZeroOverlay','FlexibleTail')''')}
            check('zero-bound-' + role + '-record-sizes',
                  len(records) == 5 and records['ZeroInterior'][1:] == (2, 1) and
                  records['ZeroMatrix'][1:] == (2, 1) and records['ZeroAligned'][1:] == (4, 4) and
                  records['ZeroOverlay'][1:] == (4, 4) and records['FlexibleTail'][1:] == (4, 4))
            fields = {row[0]: row[1:] for row in db.execute('''
                SELECT r.source_name,f.byte_offset,t.element_count,f.id,t.id,t.element_type_id
                FROM record_types r JOIN record_fields f ON f.record_type_id=r.type_id
                JOIN types t ON t.id=f.type_id
                WHERE f.source_name IN ('padding','data') AND r.source_name IN
                ('ZeroInterior','ZeroMatrix','ZeroAligned','ZeroOverlay','FlexibleTail')''')}
            check('zero-bound-' + role + '-field-bounds-and-offsets',
                  fields['ZeroInterior'][:2] == (1, 0) and fields['ZeroMatrix'][:2] == (1, 3) and
                  fields['ZeroAligned'][:2] == (0, 0) and fields['ZeroOverlay'][:2] == (0, 0) and
                  fields['FlexibleTail'][:2] == (4, None))
            inner = db.execute('SELECT kind,element_count FROM types WHERE id=?',
                               (fields['ZeroMatrix'][4],)).fetchone()
            check('zero-bound-' + role + '-matrix-inner-bound', inner == ('array', 0))
            return_arrays = {row[0]: row[1:] for row in db.execute('''
                SELECT e.source_name,a.element_count,a.id FROM entities e
                JOIN types p ON p.id=e.return_type_id JOIN types a ON a.id=p.pointee_type_id
                WHERE e.source_name IN ('zero_view','unknown_view')''')}
            check('zero-bound-' + role + '-return-identities-distinct',
                  len(return_arrays) == 2 and return_arrays['zero_view'][0] == 0 and
                  return_arrays['unknown_view'][0] is None and
                  return_arrays['zero_view'][1] != return_arrays['unknown_view'][1])
            source_arrays = db.execute("SELECT element_count FROM source_types WHERE kind='array'").fetchall()
            check('zero-bound-' + role + '-original-bound-distinction',
                  (0,) in source_arrays and (None,) in source_arrays)
            identities.append({name: row[0] for name, row in records.items()})
            if role == 'provider':
                zero_type_id = return_arrays['zero_view'][1]
                zero_field_id = fields['ZeroInterior'][2]
    check('zero-bound-record-identities-shared', identities[0] == identities[1])

    generated = link('link-zero-bound', objects)
    execute('zero-bound', generated)
    text = '\n'.join(path.read_text() for path in generated.glob('*.cs'))
    check('zero-bound-no-physical-array-wrappers', all(
        re.search(r'\bstruct\s+' + name + r'\b', text) is None
        for name in ['__DotObjArray_A0_i8', '__DotObjArray_A0_u32',
                     '__DotObjArray_A3_A0_i8', '__DotObjArray_Ii8']))
    check('zero-bound-no-physical-padding-fields',
          re.search(r'^\s*public\s+[^\n;{}]+\s+@?padding\s*;', text, re.MULTILINE) is None)
    check('zero-bound-original-member-views-retained',
          text.count('// Zero-byte array member:') == 4 and
          'padding at byte offset 1; original extent array(3,array(0,i8))' in text and
          'data at byte offset 4; original extent array(?,i8)' in text)
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
               for path in generated.glob('*.cs')}
    directory.rename(out / 'retained-zero-bound-source')
    source_free = link('link-zero-bound-source-free', objects)
    check('zero-bound-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest()
        for path in source_free.glob('*.cs')})

    mutations = [
        ('zero-bound-erased-fixed-bound', [('UPDATE types SET element_count=NULL WHERE id=?', (zero_type_id,))]),
        ('zero-bound-invented-element', [('UPDATE types SET element_count=1 WHERE id=?', (zero_type_id,))]),
        ('zero-bound-field-outside-owner', [('UPDATE record_fields SET byte_offset=3 WHERE id=?', (zero_field_id,))]),
    ]
    for label, sql in mutations:
        malformed = mutate(label, objects[0], sql)
        result = run('reject-' + label, ['dotnet', tool, 'verify', malformed], None)
        check(label + '-semantic-rejection', 'digest' not in (result.stdout + result.stderr).lower())

    negatives = {
        'zero-bound-standalone-global': 'char zero[0]; int main() { return (int)sizeof(zero); }',
        'zero-bound-standalone-local': 'int main() { char zero[0]; return (int)sizeof(zero); }',
        'zero-bound-unknown-interior': 'struct Bad { unsigned tag; char data[]; unsigned after; }; Bad value;',
    }
    for label, text in negatives.items():
        source = out / (label + '.cpp')
        source.write_text(text + '\n')
        compile_source(label, source, expected=1, extra=['--no-exceptions', '--no-rtti'])
    return objects
