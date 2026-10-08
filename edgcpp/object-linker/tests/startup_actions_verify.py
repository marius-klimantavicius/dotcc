"""Owned startup registration, provider extraction and metadata rejection checks."""
import json
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    order = compile_source('startup-order', inputs / 'project_c_startup_order.c', std='c11')
    # The fixture's expected tie order is independently established with stock
    # EDG C output. C does not require GCC's different declaration ordering.
    execute('startup-stock-order', link('startup-stock-order-link', [order]))
    pair = [compile_source('startup-linkonce-' + name,
                           inputs / ('project_cpp_startup_' + name + '.cpp'))
            for name in ['first', 'second']]
    run('startup-linkonce-native-build', ['c++', '-std=c++17',
        inputs / 'project_cpp_startup_first.cpp', inputs / 'project_cpp_startup_second.cpp',
        '-o', out / 'startup-linkonce-native'])
    run('startup-linkonce-native-run', [out / 'startup-linkonce-native'])
    linked = link('startup-linkonce-link', pair)
    selection = json.loads((linked / 'link-selection.json').read_text())
    check('startup-coalesced-body-keeps-both-registrations', selection['startupActions'] == 2)
    execute('startup-linkonce', linked)

    # A parsed provider copy of a primary inline constructor is not an archive
    # extraction reason. Its alphabetically earlier unit ID must not steal the
    # primary definition and then lose that primary startup root.
    primary_path = inputs / 'startup-coalescing-primary.cpp'
    primary_path.write_text('#include "project_cpp_startup_shared.h"\n'
                            'int startup_registration_count; '
                            'int main(){return startup_registration_count!=1;}\n')
    primary = compile_source('startup-coalescing-primary', primary_path)
    provider = compile_source('startup-coalescing-provider',
        inputs / 'project_cpp_startup_second.cpp', unit_id='aaa-provider')
    directory = out / 'startup-coalescing-link'
    run('startup-coalescing-link', ['dotnet', tool, 'link', '--out', directory,
        '--entry', 'main', primary, '--provider', provider])
    selection = json.loads((directory / 'link-selection.json').read_text())
    check('startup-primary-coalesced-definition-remains-root', selection['startupActions'] == 1)
    check('startup-coalesced-provider-not-extracted', not next(
        row['selected'] for row in selection['objects'] if row['unitId'] == 'aaa-provider'))
    execute('startup-coalescing', directory)

    path = inputs / 'startup-strong-primary.cpp'
    path.write_text('extern "C" __attribute__((used)) inline int public_unused(){return 1;} '
                    'int main(){return 0;}\n')
    primary = compile_source('startup-strong-primary', path)
    path = inputs / 'startup-strong-provider.c'
    path.write_text('int public_unused(void){return 2;}\n')
    provider = compile_source('startup-strong-provider', path, std='c11')
    directory = out / 'startup-strong-link'
    run('startup-strong-link', ['dotnet', tool, 'link', '--out', directory, '--entry', 'main',
        '--namespace', 'Tests.Strong', '--class', 'Core', primary, '--provider', provider])
    (directory / 'StrongProbe.cs').write_text('internal static class StrongProbe { '
        '[global::System.Runtime.CompilerServices.ModuleInitializer] internal static void Run(){ '
        'if(global::Tests.Strong.Core.public_unused()!=2) throw new global::System.Exception("strong provider root"); }}')
    execute('startup-strong', directory)

    sources = {
        'startup-primary': 'extern int startup_value; int main(void){return startup_value != 123;}\n',
        'startup-provider': 'int startup_value; extern void startup_dependency(void); '
            '__attribute__((constructor(101))) static void initialize(void)'
            '{startup_value=12;startup_dependency();}\n',
        'startup-dependency': 'extern int startup_value; void startup_dependency(void){startup_value=startup_value*10+3;}\n',
        'startup-unused': 'extern void missing_startup_provider(void); '
            '__attribute__((constructor)) static void unused(void){missing_startup_provider();}\n',
    }
    objects = {}
    for name, source in sources.items():
        path = inputs / (name + '.c')
        path.write_text(source)
        objects[name] = compile_source(name, path, std='c11')
    directory = out / 'startup-provider-link'
    run('startup-provider-link', ['dotnet', tool, 'link', '--out', directory, '--entry', 'main',
        objects['startup-primary'], '--provider', objects['startup-provider'],
        '--provider', objects['startup-dependency'], '--provider', objects['startup-unused']])
    selection = json.loads((directory / 'link-selection.json').read_text())
    check('startup-provider-extraction-retains-internal-action', selection['startupActions'] == 1)
    check('startup-unused-provider-not-extracted', not next(
        row['selected'] for row in selection['objects'] if row['unitId'] == 'startup-unused'))
    execute('startup-provider', directory)

    # Equal-priority cross-unit actions follow original input order, even when
    # unit identities would sort oppositely for deterministic ordinary emission.
    paths = []
    for name, source in [('startup-z-first', 'int startup_order; __attribute__((constructor(101))) '
                         'static void a(void){startup_order=startup_order*10+1;}'),
                         ('startup-a-second', 'extern int startup_order; __attribute__((constructor(101))) '
                          'static void b(void){startup_order=startup_order*10+2;} '
                          'int main(void){return startup_order!=12;}')]:
        path = inputs / (name + '.c')
        path.write_text(source)
        paths.append(compile_source(name, path, std='c11'))
    execute('startup-input-order', link('startup-input-order-link', paths))
    execute('startup-reversed-order', link('startup-reversed-order-link', list(reversed(paths))), expected=1)

    with sqlite3.connect(order) as db:
        invalid_target = db.execute("SELECT id FROM entities WHERE source_name='main'").fetchone()[0]
    bad = mutate('startup-nonvoid-action', order,
                 [('UPDATE initialization_actions SET entity_id=? WHERE id=(SELECT min(id) FROM initialization_actions)',
                   (invalid_target,))])
    run('startup-reject-nonvoid-action', ['dotnet', tool, 'verify', bad], None,
        'Startup action must name a defined nonvariadic void(void) function')
    compile_source('startup-shutdown-rejected', inputs / 'project_c_lifecycle_actions.c', std='c11', expected=None)
