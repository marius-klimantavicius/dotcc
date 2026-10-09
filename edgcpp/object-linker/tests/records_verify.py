"""Plain record/layout acceptance for the owned object pipeline."""
import json
import sqlite3
import sys
from pathlib import Path


def verify(inputs, out, tool, run, check, compile_source, link, execute):
    objects = []
    for language, standard, suffix, extension, native in [
        ('cpp', 'c++17', '', '.cpp', 'c++'), ('c', 'c11', '-c', '.c', 'cc')]:
        provider_name, consumer_name = 'record-provider' + suffix, 'records' + suffix
        provider = compile_source(provider_name, inputs / (provider_name + extension), std=standard)
        consumer = compile_source(consumer_name, inputs / (consumer_name + extension), std=standard)
        run('native-records-' + language + '-build', [native, '-std=' + standard,
            inputs / (provider_name + extension), inputs / (consumer_name + extension),
            '-o', out / ('native-records-' + language)])
        run('native-records-' + language + '-run', [out / ('native-records-' + language)])
        directory = link('link-records-' + language, [provider, consumer])
        execute('records-' + language, directory)
        objects.append((provider, consumer))
        with sqlite3.connect(consumer) as db:
            check('record-layout-rows-' + language, db.execute('SELECT count(*) FROM record_types').fetchone()[0] == 3)
            check('record-field-rows-' + language, db.execute('SELECT count(*) FROM record_fields').fetchone()[0] == 10)
            check('record-self-pointer-' + language, db.execute("SELECT count(*) FROM record_fields f JOIN types t ON t.id=f.type_id WHERE t.kind='pointer' AND t.pointee_type_id=f.record_type_id").fetchone()[0] == 1)
            check('record-field-typed-references-' + language, db.execute("SELECT count(*) FROM nodes WHERE kind='member' AND field_id IS NOT NULL").fetchone()[0] > 20)
            check('record-field-source-types-' + language, db.execute('SELECT count(*) FROM record_fields f JOIN source_types s ON s.id=f.source_type_id WHERE f.type_id=s.execution_type_id').fetchone()[0] == 10)
            check('record-owned-original-declarations-' + language,
                  db.execute('SELECT count(*) FROM source_record_types').fetchone()[0] == 3 and
                  db.execute('SELECT count(*) FROM source_record_fields').fetchone()[0] == 10)
            check('record-original-physical-correspondence-' + language, db.execute(
                "SELECT count(*) FROM record_fields f JOIN source_record_fields s ON s.id=f.source_field_id "
                "JOIN record_types r ON r.type_id=f.record_type_id WHERE f.field_role='source' "
                "AND f.source_type_id=s.source_type_id AND r.source_record_id=s.source_record_id").fetchone()[0] == 10)
            check('record-cleanup-' + language, db.execute("SELECT count(*) FROM nodes n JOIN bindings b ON b.id=n.binding_id JOIN types t ON t.id=b.type_id WHERE n.kind='register-cleanup' AND t.kind='record'").fetchone()[0] == 2)
        cs = '\n'.join(p.read_text() for p in directory.glob('Translated.*.cs'))
        check('record-readable-fields-' + language, all(token in cs for token in ['->next', '->data', '.count', '.values', 'FieldOffset(', 'LayoutKind.Explicit']))
        check('record-function-name-wins-collision-' + language, 'public static int RecordPair()' in cs)
        check('record-no-runtime-frames-' + language, '__edg_frame' not in cs and 'NativeMemory.' not in cs and 'GC.Allocate' not in cs)
        source_map = json.loads((directory / 'source-map.json').read_text())
        check('record-map-metadata-' + language, any(len(obj['recordTypes']) == 3 and len(obj['recordFields']) == 10 for obj in source_map['objects']))
    provider, consumer = objects[0]
    execute('records-mixed-language', link('link-records-mixed-language', [objects[1][0], consumer]))
    execute('records-owner-collision', link('link-records-owner-collision', [provider, consumer], cls='RecordNode'))

    for name in ['project_cpp_record_origins', 'project_cpp_vptr_layout', 'project_cpp_constant_storage',
                 'project_cpp_integer128_enum', 'project_cpp_block_copy', 'project_cpp_void_conditionals',
                 'project_cpp_base_values']:
        source = inputs / (name + '.cpp')
        obj = compile_source(name, source, extra=['--exceptions', '--rtti'])
        run(name + '-native-build', ['c++', '-std=c++17', '-O2', source, '-o', out / (name + '-native')])
        run(name + '-native', [out / (name + '-native')])
        if name == 'project_cpp_base_values':
            directory = out / ('link-' + name)
            run('link-' + name, ['dotnet', tool, 'link', '--allow-externs', '--out', directory, '--entry', 'main', obj])
            run('bind-' + name, [sys.executable, Path(__file__).resolve().parents[1] / 'runtime/bind.py', '--project', directory])
            execute(name, directory)
        else:
            execute(name, link('link-' + name, [obj]))
        with sqlite3.connect(obj) as db:
            if name == 'project_cpp_record_origins':
                check('record-original-zero-width-preserved', db.execute(
                    "SELECT count(*) FROM source_record_fields WHERE source_name='' AND bit_width=0").fetchone()[0] == 1)
                check('record-original-mutable-preserved', db.execute(
                    "SELECT is_mutable FROM source_record_fields WHERE source_name='value'").fetchone()[0] == 1)
                check('record-zero-width-has-no-physical-field', db.execute(
                    "SELECT count(*) FROM record_fields f JOIN source_record_fields s ON s.id=f.source_field_id WHERE s.bit_width=0").fetchone()[0] == 0)
            elif name == 'project_cpp_vptr_layout':
                check('record-vptr-origin-is-hidden', db.execute(
                    "SELECT count(*) FROM record_fields WHERE field_role='vptr' AND source_type_id IS NULL AND source_field_id IS NULL AND byte_offset=0").fetchone()[0] == 1)
                check('record-vptr-copy-not-ordinary', db.execute(
                    "SELECT count(*) FROM record_types WHERE capture_proof='observed-class-lowering' AND value_copy_kind='explicit-only'").fetchone()[0] == 1)

    namespaces = inputs / 'record-namespaces.cpp'
    namespaces.write_text('''namespace North { struct Token { int event; }; }
namespace South { struct Token { double event; }; }
int main(){North::Token a={17};South::Token b={2.5};return a.event==17 && b.event==2.5 ? 0 : 1;}
''')
    namespace_object = compile_source('record-namespaces', namespaces)
    namespace_dir = link('link-record-namespaces', [namespace_object])
    execute('record-namespaces', namespace_dir)
    cs = '\n'.join(p.read_text() for p in namespace_dir.glob('Translated.*.cs'))
    check('record-keyword-field', '@event' in cs)
    with sqlite3.connect(namespace_object) as db:
        check('record-qualified-identities', {row[0] for row in db.execute('SELECT identity_key FROM record_types')} == {'record:North::Token', 'record:South::Token'})

    depth_source = inputs / 'record-depth.cpp'
    declarations = ['struct Depth0 { int value; };', 'void depth0(Depth0*){}']
    for depth in range(1, 130):
        declarations += [f'struct Depth{depth} {{ Depth{depth-1} value; }};', f'void depth{depth}(Depth{depth}*){{}}']
    depth_source.write_text('\n'.join(declarations) + '\nint main(){return 0;}\n')
    depth_object = compile_source('record-depth', depth_source)
    with sqlite3.connect(depth_object) as db:
        check('record-depth-leaf-first', db.execute("SELECT type_id FROM record_types WHERE source_name='Depth0'").fetchone()[0]
              < db.execute("SELECT type_id FROM record_types WHERE source_name='Depth129'").fetchone()[0])
    run('reject-record-cached-depth-bypass', ['dotnet', tool, 'verify', depth_object], None, 'nest')

    for name, code in [
        ('byvalue-parameter', 'struct X{int n;};int f(X value){return value.n;}'),
        ('copy', 'struct X{int n;};int f(){X a={1};X b={2};b=a;return b.n;}'),
        ('array', 'struct X{int n;};int f(){X a[2];a[0].n=1;return a[0].n;}'),
        ('sized-array-parameter', 'struct X{int n;};int f(X a[2]){return a[0].n;}'),
        ('incomplete', 'struct X;int f(X* p){return p!=0;}'),
        ('bits', 'struct X{unsigned n:3;};int f(X* p){return p->n;}'),
        ('packed', 'struct __attribute__((packed)) X{char c;int n;};int f(X* p){return p->n;}'),
        ('overaligned', 'struct alignas(32) X{int n;};int f(X* p){return p->n;}'),
        ('raised-alignment', 'struct alignas(8) X{char n;};int f(X* p){return p->n;}'),
        ('empty', 'struct X{};int f(X* p){return p!=0;}'),
        ('local', 'int f(){struct X{int n;};X a={2};return a.n;}'),
        ('inheritance', 'struct Base{int n;};struct X:Base{int v;};int f(X* p){return p->n;}'),
    ]:
        source = inputs / ('record-unsupported-' + name + '.cpp');source.write_text(code + '\n')
        supported = name in {'byvalue-parameter', 'copy', 'array', 'sized-array-parameter', 'incomplete', 'bits', 'empty', 'local', 'raised-alignment', 'inheritance', 'packed'}
        obj = compile_source(('record-supported-' if supported else 'record-unsupported-') + name,
                             source, expected=0 if supported else None)
        if supported:
            run('verify-record-supported-' + name, ['dotnet', tool, 'verify', obj])
        else:
            check('record-not-published-' + name, not obj.exists())
    return provider, consumer


def malformed(provider, consumer, tool, run, link, mutate):
    with sqlite3.connect(consumer) as db:
        pair = db.execute("SELECT type_id FROM record_types WHERE source_name='RecordPair'").fetchone()[0]
        node = db.execute("SELECT type_id FROM record_types WHERE source_name='RecordNode'").fetchone()[0]
        pair_field = db.execute('SELECT id FROM record_fields WHERE record_type_id=? ORDER BY ordinal LIMIT 1', (pair,)).fetchone()[0]
        pair_source = db.execute("SELECT id FROM source_types WHERE kind='record' AND execution_type_id=?", (pair,)).fetchone()[0]
        member = db.execute("SELECT n.id FROM nodes n JOIN record_fields f ON f.id=n.field_id WHERE n.kind='member' AND f.record_type_id=? LIMIT 1", (node,)).fetchone()[0]
    for name, statements in [
        ('record-field-offset', [("UPDATE record_fields SET byte_offset=byte_offset+1 WHERE id=?", (pair_field,))]),
        ('record-wrong-size', [("UPDATE record_types SET size=size+1 WHERE type_id=?", (pair,))]),
        ('record-wrong-alignment', [("UPDATE record_types SET alignment=1 WHERE type_id=?", (pair,))]),
        ('record-field-order', [("UPDATE record_fields SET ordinal=10 WHERE id=?", (pair_field,))]),
        ('record-cycle', [("UPDATE record_fields SET type_id=?,source_type_id=? WHERE id=?", (pair, pair_source, pair_field))]),
        ('record-wrong-member-owner', [("UPDATE nodes SET field_id=? WHERE id=?", (pair_field, member))]),
        ('record-extra-field-payload', [("UPDATE nodes SET field_id=? WHERE id=(SELECT min(id) FROM nodes WHERE kind='constant')", (pair_field,))]),
        ('record-source-type', [("UPDATE record_fields SET source_type_id=(SELECT id FROM source_types WHERE kind='unsigned-char' LIMIT 1) WHERE id=?", (pair_field,))]),
        ('record-original-identity', [("UPDATE source_record_types SET identity_key=identity_key||':forged' WHERE id=(SELECT source_record_id FROM record_types WHERE type_id=?)", (pair,))]),
        ('record-nontrivial-ordinary-copy', [("UPDATE source_record_types SET is_trivially_copyable=0 WHERE id=(SELECT source_record_id FROM record_types WHERE type_id=?)", (pair,))]),
        ('record-fabricated-hidden-field', [("UPDATE record_fields SET field_role='abi',source_type_id=NULL,source_field_id=NULL WHERE id=?", (pair_field,))]),
    ]:
        obj = mutate(name, consumer, statements)
        run('reject-' + name, ['dotnet', tool, 'verify', obj], None)
    wrong_name = mutate('record-cross-unit-name', provider, [
        ("UPDATE source_record_fields SET source_name='different_count' WHERE id IN (SELECT source_field_id FROM record_fields WHERE source_name='count')", ()),
        ("UPDATE record_fields SET source_name='different_count' WHERE source_name='count'", ())])
    run('verify-record-conflicting-name-object', ['dotnet', tool, 'verify', wrong_name])
    link('reject-record-cross-unit-name', [wrong_name, consumer], expected=None, contains='record')
    wrong_source = mutate('record-cross-unit-source', provider, [
        ("INSERT INTO source_types SELECT max(id)+1,'long','',NULL,8,0,0,NULL,0,0,NULL FROM source_types", ()),
        ("UPDATE source_record_fields SET source_type_id=(SELECT max(id) FROM source_types) WHERE id IN (SELECT source_field_id FROM record_fields WHERE source_name='total')", ()),
        ("UPDATE record_fields SET source_type_id=(SELECT max(id) FROM source_types) WHERE source_name='total'", ())])
    run('verify-record-conflicting-source-object', ['dotnet', tool, 'verify', wrong_source])
    link('reject-record-cross-unit-source', [wrong_source, consumer], expected=None, contains='record')
