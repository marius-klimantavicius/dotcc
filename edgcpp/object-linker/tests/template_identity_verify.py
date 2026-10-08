"""Template nominal identities across namespaces, arguments and translation units."""
from pathlib import Path
import hashlib
import shutil
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    source_directory = out / 'template-identity-source'
    source_directory.mkdir()
    for name in ['template_identity_shared.h', 'template_identity_provider.cpp', 'template_identity_main.cpp']:
        shutil.copy2(inputs / name, source_directory / name)
    provider_source = source_directory / 'template_identity_provider.cpp'
    main_source = source_directory / 'template_identity_main.cpp'
    for optimization in ['0', '2']:
        native = out / ('template-identity-native-' + optimization)
        run('template-identity-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, provider_source, main_source, '-o', native])
        run('template-identity-native-' + optimization, [native])

    objects = [compile_source('template-identity-' + label, source, extra=['--no-exceptions', '--no-rtti'])
               for label, source in [('provider', provider_source), ('main', main_source)]]
    identities = []
    functions = ['read_int', 'read_long', 'read_four', 'read_other', 'read_nested', 'read_shared_local_box']
    for label, obj in zip(['provider', 'main'], objects):
        run('verify-template-identity-' + label, ['dotnet', tool, 'verify', obj])
        with sqlite3.connect(obj) as db:
            signatures = {}
            for name in functions:
                rows = db.execute('''
                    SELECT r.identity_key FROM entities e
                    JOIN parameters p ON p.entity_id=e.id AND p.ordinal=0
                    JOIN types t ON t.id=p.type_id
                    JOIN record_types r ON r.type_id=t.pointee_type_id
                    WHERE e.source_name=?''', (name,)).fetchall()
                check('template-identity-' + label + '-' + name + '-owned-record', len(rows) == 1)
                signatures[name] = rows[0][0]
            private = {}
            for name in ['local_same', 'local_different', 'local_private_function']:
                rows = db.execute('''
                    SELECT r.identity_key,r.size,argument.identity_key FROM storages s
                    JOIN record_types r ON r.type_id=s.type_id
                    JOIN record_fields f ON f.record_type_id=r.type_id AND f.source_name='value'
                    JOIN record_types argument ON argument.type_id=f.type_id
                    WHERE s.source_name=? AND s.linkage='internal' ''', (name,)).fetchall()
                check('template-identity-' + label + '-' + name + '-owned-record', len(rows) == 1)
                private[name] = rows[0]
            check('template-identity-' + label + '-namespace-type-and-value-arguments-distinct',
                  len(set(signatures.values())) == len(functions))
            shared = db.execute('''
                SELECT r.identity_key,argument.identity_key FROM storages s
                JOIN record_types r ON r.type_id=s.type_id
                JOIN record_fields f ON f.record_type_id=r.type_id AND f.source_name='value'
                JOIN record_types argument ON argument.type_id=f.type_id
                WHERE s.source_name='shared_local_box' AND s.linkage='external' ''').fetchall()
            check('template-identity-' + label + '-inline-local-argument-owned', len(shared) == 1)
            identities.append((signatures, private, shared[0]))
    check('template-identity-external-specializations-shared', identities[0][0] == identities[1][0])
    check('template-identity-external-inline-local-and-specialization-shared', identities[0][2] == identities[1][2])
    check('template-identity-anonymous-same-layout-remains-distinct',
          identities[0][1]['local_same'][0] != identities[1][1]['local_same'][0] and
          identities[0][1]['local_same'][1] == identities[1][1]['local_same'][1] == 4)
    check('template-identity-anonymous-different-layout-remains-distinct',
          identities[0][1]['local_different'][0] != identities[1][1]['local_different'][0] and
          identities[0][1]['local_different'][1] == 4 and identities[1][1]['local_different'][1] == 8)
    for name in ['local_same', 'local_different']:
        check('template-identity-anonymous-argument-' + name + '-remains-distinct',
              identities[0][1][name][2] != identities[1][1][name][2])
    check('template-identity-private-function-local-and-specialization-distinct',
          identities[0][1]['local_private_function'][0] != identities[1][1]['local_private_function'][0] and
          identities[0][1]['local_private_function'][2] != identities[1][1]['local_private_function'][2])

    generated = link('link-template-identity', objects)
    execute('template-identity', generated)
    emitted = {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in generated.glob('*.cs')}
    source_directory.rename(out / 'retained-template-identity-source')
    source_free = link('link-template-identity-source-free', objects)
    check('template-identity-source-free-identical-CSharp', emitted == {
        path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in source_free.glob('*.cs')})
    return objects
