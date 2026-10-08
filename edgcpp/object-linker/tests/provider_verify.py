"""Link-time runtime provider closure without pruning primary library APIs."""
import json


def verify(inputs, out, tool, run, check, compile_source, execute):
    objects = {name: compile_source(name, inputs / (name + '.c'), std='c11')
               for name in ['provider_root', 'provider_library', 'provider_support']}
    stub = out / 'native-provider-stubs.c'
    stub.write_text('int intentionally_unprovided(void){return 99;}\nint another_unprovided(void){return 98;}\n')
    run('provider-native-build', ['cc', '-std=c11', '-O2',
        *[inputs / (name + '.c') for name in objects], stub, '-o', out / 'provider-native'])
    run('provider-native-run', [out / 'provider-native'])
    run('provider-default-retains-all', ['dotnet', tool, 'link', '--out', out / 'provider-all',
        '--entry', 'main', *objects.values()], None, 'Unresolved function')

    def link(directory, reverse=False):
        providers = [objects['provider_library'], objects['provider_support']]
        if reverse:
            providers.reverse()
        run(directory.name, ['dotnet', tool, 'link', '--out', directory,
            '--namespace', 'Tests.Providers', '--class', 'Core', '--entry', 'main',
            *[part for item in providers for part in ['--provider', item]], objects['provider_root']])

    directory = out / 'provider-selected'
    link(directory)
    selection = json.loads((directory / 'link-selection.json').read_text())
    check('provider-function-closure', selection['candidateFunctions'] == 7 and selection['selectedFunctions'] == 5)
    check('provider-storage-closure', selection['candidateStorages'] == 2 and selection['selectedStorages'] == 1)
    source = '\n'.join(p.read_text() for p in directory.glob('*.cs'))
    check('provider-primary-api-kept', 'public_api_not_called_by_main(' in source)
    check('provider-unused-imports-absent', 'intentionally_unprovided' not in source and 'another_unprovided' not in source)
    original = {p.name: p.read_bytes() for p in directory.glob('*.cs')}
    for name in objects:
        (inputs / (name + '.c')).rename(inputs / (name + '.source-retained'))
    relinked = out / 'provider-source-free'
    link(relinked, True)
    check('provider-source-free-deterministic', original == {p.name: p.read_bytes() for p in relinked.glob('*.cs')})
    (directory / 'PublicApiProbe.cs').write_text('''
internal static class PublicApiProbe
{
    [global::System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Check()
    {
        if (global::Tests.Providers.Core.public_api_not_called_by_main() != 14)
            throw new global::System.Exception("Primary public API lost provider dependency");
    }
}
''')
    execute('provider-selection', directory)
