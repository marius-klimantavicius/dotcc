"""C++ anonymous typedef linkage names must survive independent packed objects."""
import hashlib
import re
import shutil
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    directory = out / 'packed-typedef-source'
    directory.mkdir()
    for name in ['packed_typedef_identity.h', 'packed_typedef_identity_provider.cpp',
                 'packed_typedef_identity_main.cpp']:
        shutil.copy2(inputs / name, directory / name)
    sources = [directory / ('packed_typedef_identity_' + role + '.cpp')
               for role in ['provider', 'main']]
    for optimization in ['0', '2']:
        native = out / ('packed-typedef-native-' + optimization)
        run('packed-typedef-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, *sources, '-o', native])
        run('packed-typedef-native-' + optimization, [native])
    objects = [compile_source('packed-typedef-' + role, source,
                              extra=['--no-exceptions', '--no-rtti'])
               for role, source in zip(['provider', 'main'], sources)]
    identities = []
    for role, obj in zip(['provider', 'main'], objects):
        run('verify-packed-typedef-' + role, ['dotnet', tool, 'verify', obj])
        with sqlite3.connect(obj) as db:
            rows = db.execute('''SELECT s.source_name,r.identity_key,r.size,r.alignment
                FROM storages s JOIN record_types r ON r.type_id=s.type_id
                WHERE s.source_name IN ('packet','other_packet','private_packet')''').fetchall()
            named = {row[0]: row[1:] for row in rows}
            check('packed-typedef-' + role + '-all-records-owned',
                  len(rows) == 3 and len(named) == 3)
            check('packed-typedef-' + role + '-accepted-layout',
                  all(row[1:] == (5, 1) for row in named.values()))
            check('packed-typedef-' + role + '-namespace-distinct',
                  named['packet'][0] == 'record:Public::Packet' and
                  named['other_packet'][0] == 'record:Other::Packet')
            identities.append(named)
    check('packed-typedef-public-identity-shared',
          all(identities[0][name] == identities[1][name]
              for name in ['packet', 'other_packet']))
    check('packed-typedef-private-identity-distinct',
          identities[0]['private_packet'][0] != identities[1]['private_packet'][0])
    generated = link('link-packed-typedef', objects)
    execute('packed-typedef', generated)
    text = '\n'.join(path.read_text() for path in generated.glob('*.cs'))
    check('packed-typedef-source-function-names-preserved', all(
        re.search(r'\b' + name + r'\(\)\s*\{', text) is not None
        for name in ['get_packet', 'get_other_packet', 'get_private_packet']))
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
               for path in generated.glob('*.cs')}
    directory.rename(out / 'retained-packed-typedef-source')
    source_free = link('link-packed-typedef-source-free', objects)
    check('packed-typedef-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest()
        for path in source_free.glob('*.cs')})
    return objects
