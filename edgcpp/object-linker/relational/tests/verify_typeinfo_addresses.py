#!/usr/bin/env python3
"""Inspect exact selected static typeid types, external RTTI objects and ABI views."""
import argparse
from pathlib import Path
import shutil
import sqlite3
import sys

from verify import EDG, RELATIONAL, run


def inspect(path):
    with sqlite3.connect(path.as_uri()+'?mode=ro', uri=True) as db:
        db.row_factory = sqlite3.Row
        assert tuple(db.execute('PRAGMA integrity_check').fetchone()) == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        assert db.execute('PRAGMA user_version').fetchone()[0] == 1018
        tables = list(db.execute("SELECT name,sql FROM sqlite_master WHERE type='table' ORDER BY name"))
        assert len(tables) == 43, len(tables)
        assert not any('BLOB' in row['sql'].upper() or 'JSON' in row['sql'].upper() for row in tables)
        types = {row['id']: row for row in db.execute('SELECT * FROM types')}

        def plain(type_id):
            value = types[type_id]
            while value['kind'] in ('qualified', 'typedef'):
                value = types[value['underlying_type_id']]
            return value

        symbols = list(db.execute("SELECT * FROM symbols WHERE kind='rtti-object' ORDER BY link_name"))
        by_link = {row['link_name']: row for row in symbols}
        assert len(symbols) == len(by_link) == 4, [dict(row) for row in symbols]
        assert set(by_link) == {'_ZTIv', '_ZTIi', '_ZTId', '_ZTIPi'}, set(by_link)
        for symbol in symbols:
            assert symbol['linkage'] == 'external' and not symbol['is_definition'], dict(symbol)
            assert symbol['parent_symbol_id'] is None, dict(symbol)
            assert not list(db.execute('SELECT * FROM global_initializers WHERE symbol_id=?', (symbol['id'],)))
            storage = plain(symbol['type_id'])
            assert storage['kind'] == 'record' and not storage['is_incomplete'], dict(storage)
            assert storage['name'] == ('__pointer_type_info' if symbol['link_name'] == '_ZTIPi'
                                       else '__fundamental_type_info'), dict(storage)
            if symbol['link_name'] != '_ZTIPi':
                assert (storage['size'], storage['alignment']) == (16, 8), dict(storage)
                base = db.execute('SELECT * FROM fields WHERE owner_type_id=? ORDER BY ordinal',
                                  (storage['id'],)).fetchall()
                assert len(base) == 1 and base[0]['name'] == 'base' and base[0]['byte_offset'] == 0
                prefix = plain(base[0]['type_id'])
                fields = db.execute('SELECT name,byte_offset FROM fields WHERE owner_type_id=? ORDER BY ordinal',
                                    (prefix['id'],)).fetchall()
                assert [tuple(row) for row in fields] == [('__vptr', 0), ('__name', 8)], fields

        addresses = list(db.execute('''SELECT n.*,s.link_name,r.id AS reference_id,
            r.type_id AS selected_type_id,r.kind AS reference_kind,
            r.function_id AS reference_function,r.value_category AS reference_category,
            e.ordinal AS selected_ordinal FROM nodes n
            JOIN symbols s ON s.id=n.symbol_id
            JOIN edges e ON e.parent_id=n.id AND e.role='selected-type'
            JOIN nodes r ON r.id=e.child_id WHERE n.kind='type-info-address' ORDER BY n.id'''))
        assert len(addresses) >= 12, len(addresses)
        assert len(addresses) == db.execute("SELECT count(*) FROM nodes WHERE kind='type-info-address'").fetchone()[0]
        selected = {}
        for address in addresses:
            assert address['reference_kind'] == 'type-reference' and address['selected_ordinal'] == 0
            assert address['reference_function'] is None and address['reference_category'] is None
            assert address['value_category'] == 'prvalue' and address['execution_type_id'] is None
            assert address['literal_object_id'] is None and address['constant_text'] is None
            assert address['file_id'] is not None and address['line'] > 0 and address['column'] > 0
            view = plain(address['type_id'])
            assert view['kind'] == 'pointer', dict(view)
            public = plain(view['pointee_type_id'])
            assert public['kind'] == 'record' and public['name'] == 'type_info', dict(public)
            operand = plain(address['selected_type_id'])
            link = address['link_name']
            if link == '_ZTIPi':
                assert operand['kind'] == 'pointer' and plain(operand['pointee_type_id'])['builtin_name'] == 'int'
            else:
                assert operand['builtin_name'] == {'_ZTIv': 'void', '_ZTIi': 'int', '_ZTId': 'double'}[link]
            identity = (address['symbol_id'], address['reference_id'], address['selected_type_id'])
            assert selected.setdefault(link, identity) == identity, (link, identity, selected)
        assert set(selected) == set(by_link)
        assert any(row['function_id'] is not None and row['link_name'] == '_ZTIv' for row in addresses)
        source_typeid = list(db.execute('''SELECT source.kind,source.operator,source.value_category,
            source.type_id,address.type_id AS address_type_id FROM nodes address
            JOIN nodes source ON source.id=address.source_node_id
            WHERE address.kind='type-info-address' AND address.function_id IS NOT NULL'''))
        assert source_typeid, 'Source static typeid lvalue projection is missing'
        for source in source_typeid:
            assert (source['kind'],source['operator'],source['value_category']) == ('operation','indirect','lvalue')
            assert plain(source['address_type_id'])['pointee_type_id'] == source['type_id']
            assert types[source['type_id']]['is_const']
        assert db.execute("SELECT count(*) FROM nodes WHERE kind='type-reference'").fetchone()[0] == 4
        static_symbols = list(db.execute("SELECT id,parent_symbol_id FROM symbols WHERE kind='static-local'"))
        assert len(static_symbols) == 2 and all(row['parent_symbol_id'] is not None for row in static_symbols)
        for symbol in static_symbols:
            root = db.execute('''SELECT n.function_id FROM global_initializers g JOIN nodes n
                ON n.id=g.initializer_node_id WHERE g.symbol_id=?''', (symbol['id'],)).fetchone()
            assert root is not None and root[0] is None
        output = [f'{path.name}: {path.stat().st_size} bytes']
        output += [f'{row["name"]}: {db.execute("SELECT count(*) FROM " + row["name"]).fetchone()[0]} rows'
                   for row in tables]
        output += ['', 'selected RTTI objects', *(str(dict(row)) for row in symbols), '', 'source typeid addresses',
                   *(str(dict(row)) for row in addresses)]
        return '\n'.join(output)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out', type=Path, required=True)
    parser.add_argument('--compiler', type=Path, default=EDG/'build/bin/cpfe-dot')
    parser.add_argument('--linker', type=Path, required=True)
    args = parser.parse_args()
    work = args.out.resolve()
    work.mkdir(parents=True, exist_ok=False)
    source = work/'source.cpp'
    shutil.copy2(RELATIONAL/'tests/typeinfo_addresses.cpp', source)
    obj = work/'typeinfo.dotobj'
    run([sys.executable, RELATIONAL/'compile.py', source, '--output', obj,
         '--work-dir', work/'compile-work', '--compiler', args.compiler.resolve(),
         '-I', EDG/'backend/runtime/include'], work/'compile.log')
    (work/'database-summary.txt').write_text(inspect(obj)+'\n')
    folded_source = work/'folded.cpp'
    shutil.copy2(RELATIONAL/'tests/folded_typeinfo_operand.cpp', folded_source)
    folded_object = work/'folded.dotobj'
    run([sys.executable, RELATIONAL/'compile.py', folded_source, '--output', folded_object,
         '--work-dir', work/'folded-work', '--compiler', args.compiler.resolve(),
         '-I', EDG/'backend/runtime/include'], work/'folded-compile.log')
    with sqlite3.connect(folded_object.as_uri()+'?mode=ro', uri=True) as db:
        assert db.execute('PRAGMA integrity_check').fetchone() == ('ok',)
        assert not list(db.execute('PRAGMA foreign_key_check'))
        selected = db.execute('''SELECT s.link_name,s.is_definition,t.builtin_name,r.function_id,r.value_category
            FROM nodes n JOIN symbols s ON s.id=n.symbol_id
            JOIN edges e ON e.parent_id=n.id AND e.role='selected-type'
            JOIN nodes r ON r.id=e.child_id JOIN types t ON t.id=r.type_id
            WHERE n.kind='type-info-address' ''').fetchall()
        assert selected == [('_ZTIi', 0, 'int', None, None)], selected
        navigation = db.execute('''SELECT n.file_id,n.line,n.column,n.source_node_id,
            parent.source_node_id,source.file_id,source.line,source.column,source.operator
            FROM nodes n JOIN nodes parent ON parent.id=n.source_node_id
            JOIN nodes source ON source.id=parent.source_node_id
            WHERE n.kind='type-info-address' ''').fetchall()
        assert len(navigation) == 1 and navigation[0][0] is not None and navigation[0][1] > 0, navigation
        assert navigation[0][:3] == navigation[0][5:8] and navigation[0][8] == 'address-of', navigation
        assert not list(db.execute('''SELECT n.id FROM nodes n JOIN symbols s ON s.id=n.symbol_id
            WHERE s.name='operand' ''')), 'Folded static typeid operand must not be evaluated'
        (work/'folded-database-summary.txt').write_text(str(selected)+'\n')
    folded_source.rename(work/'original-folded.cpp')
    source.rename(work/'original-source.cpp')
    run(['dotnet', args.linker.resolve(), 'link', '--out', work/'generated', '--entry', 'main', obj],
        work/'link.log', expected=1)
    diagnostic = (work/'link.log').read_text()
    assert 'Unresolved RTTI provider for _ZTIv' in diagnostic, diagnostic
    assert 'exporter defect' not in diagnostic and 'source.cpp:' in diagnostic, diagnostic
    print('PASS: SQL-selected static RTTI types, exact external symbols and ABI views; source-hidden missing-provider diagnostic')
    print(work)


if __name__ == '__main__':
    main()
