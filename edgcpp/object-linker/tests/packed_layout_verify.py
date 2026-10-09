"""Observed packed layout, source-readable fields, and offset-buffer execution."""
import hashlib
import re
import shutil
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    directory = out / 'packed-layout-source'
    directory.mkdir()
    source = directory / 'project_cpp_packed_layout.cpp'
    shutil.copy2(inputs / source.name, source)
    for optimization in ['0', '2']:
        native = out / ('packed-layout-native-' + optimization)
        run('packed-layout-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, source, '-o', native])
        run('packed-layout-native-' + optimization, [native])
    obj = compile_source('packed-layout', source, extra=['--no-exceptions', '--no-rtti'])
    run('verify-packed-layout', ['dotnet', tool, 'verify', obj])
    with sqlite3.connect(obj) as db:
        records = {row[0]: row[1:] for row in db.execute('''
            SELECT source_name,native_size,native_alignment,is_packed,max_member_alignment,explicit_alignment
            FROM source_record_types WHERE is_complete=1''')}
        check('packed-layout-accepted-size-alignment',
              records['Packed'][:2] == (37, 1) and records['Pragma'][:2] == (10, 2) and
              records['AlignedPacked'][:2] == (6, 2) and records['ExplicitField'][:2] == (16, 8))
        check('packed-layout-directives-preserved',
              records['Packed'][2] == 1 and records['Pragma'][3] == 2 and
              records['AlignedPacked'][4] == 2 and records['ReducedClass'][4] == 1)
        rows = db.execute('''SELECT r.source_name,f.source_name,f.byte_offset,f.observed_alignment,s.explicit_alignment,f.id
            FROM record_types r JOIN record_fields f ON f.record_type_id=r.type_id
            JOIN source_record_fields s ON s.id=f.source_field_id WHERE f.bit_width IS NULL''').fetchall()
        fields = {(row[0], row[1]): row[2:] for row in rows}
        check('packed-layout-observed-scalar-pointer-callback-alignment',
              all(fields['Packed', name][1] == 1 for name in ['number', 'wide', 'pointer', 'callback', 'real']))
        check('packed-layout-request-differs-from-accepted-alignment',
              fields['ReducedField', 'number'][1:3] == (4, 1) and
              fields['ExplicitField', 'number'][:3] == (8, 8, 8))
        number_id = fields['Packed', 'number'][3]
    generated = link('link-packed-layout', [obj])
    execute('packed-layout', generated)
    text = '\n'.join(path.read_text() for path in generated.glob('*.cs'))
    check('packed-layout-emitted-pack-one-and-two', 'Pack = 1' in text and 'Pack = 2' in text)
    check('packed-layout-named-number-access', re.search(r'->@?number\b', text) is not None)
    check('packed-layout-named-pointer-access', re.search(r'->@?pointer\b', text) is not None)
    check('packed-layout-named-callback-access', re.search(r'->@?callback\b', text) is not None)
    check('packed-layout-structured-initializer',
          re.search(r'\btable\[0\]\s*=\s*new\(\)\s*\{\s*tag\s*=', text) is not None)
    for name in ['raw_offset_alignment_probe', 'conditional_alignment_probe']:
        body = re.search(r'\b' + name + r'\([^)]*\)\s*\{(.*?)^    \}', text, re.S | re.M)
        check('packed-layout-' + name + '-retains-unaligned-origin',
              body is not None and 'Unsafe.ReadUnaligned<uint>' in body.group(1))
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
               for path in generated.glob('*.cs')}
    directory.rename(out / 'retained-packed-layout-source')
    source_free = link('link-packed-layout-source-free', [obj])
    check('packed-layout-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest()
        for path in source_free.glob('*.cs')})
    mutations = [
        ('packed-layout-forged-natural-alignment',
         [('UPDATE record_fields SET observed_alignment=4 WHERE id=?', (number_id,))],
         'does not satisfy its observed layout alignment'),
        ('packed-layout-missing-field-alignment',
         [('PRAGMA ignore_check_constraints=ON', ()),
          ('UPDATE record_fields SET observed_alignment=NULL WHERE id=?', (number_id,))],
         'alignment'),
        ('packed-layout-invalid-source-directive',
         [('PRAGMA ignore_check_constraints=ON', ()),
          ("UPDATE source_record_types SET max_member_alignment=3 WHERE source_name='Packed'", ())],
         'metadata'),
    ]
    for label, sql, diagnostic in mutations:
        bad = mutate(label, obj, sql)
        run('reject-' + label, ['dotnet', tool, 'verify', bad], None, diagnostic)
    return obj
