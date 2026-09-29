"""lwIP complete bundled-source translation, isolated host and profile selection."""
import json
import os
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    pin = json_file(root / 'config/sources.json')
    yield Source('product', pin['url'], root / 'ref' / pin['archive'],
                 root / 'ref' / pin['directory'], pin['directory'], pin.get('sha256'), tuple(pin['required']))


def configure(ctx, project, form):
    tree = ET.parse(project)
    group = ET.SubElement(tree.getroot(), 'ItemGroup')
    if ctx.profile == 'makefsdata':
        assembly = tree.find('.//AssemblyName')
        assembly.text = 'TranslatedLwipMakeFsData'
    elif ctx.profile == 'contrib':
        tree.find('.//AssemblyName').text = 'TranslatedLwipContrib'
    host_paths = ([] if ctx.profile == 'makefsdata' else sorted((ctx.root / 'tests/Upstream').glob('*.cs')) if ctx.profile.startswith('unit') else sorted((ctx.root / 'src/Host').glob('*.cs')))
    for path in host_paths:
        if not any(item.get('Include') == str(path) for item in tree.iter('Compile')):
            ET.SubElement(group, 'Compile', Include=str(path), Link='Host/' + path.name)
    shutil.copyfile(ctx.sources['product'] / 'COPYING', project.parent / 'lwIP.COPYING')
    (project.parent / 'THIRD-PARTY-NOTICES').write_text(helper(ctx.root / 'scripts/notices.py').render(ctx.sources['product']))
    if not any(item.get('Update') == 'THIRD-PARTY-NOTICES' for item in tree.iter('None')):
        ET.SubElement(group, 'None', Update='THIRD-PARTY-NOTICES', CopyToOutputDirectory='PreserveNewest', CopyToPublishDirectory='PreserveNewest')
    if not any(item.get('Update') == 'lwIP.COPYING' for item in tree.iter('None')):
        ET.SubElement(group, 'None', Update='lwIP.COPYING', CopyToOutputDirectory='PreserveNewest', CopyToPublishDirectory='PreserveNewest')
    ET.indent(tree, space='  ')
    tree.write(project, encoding='unicode')


def finish(ctx, project, form):
    tree = ET.parse(project)
    for item in tree.iter('Compile'):
        include = item.get('Include')
        if include and (include.startswith(str(ctx.root / 'src/Host')) or include.startswith(str(ctx.root / 'tests/Upstream'))):
            item.set('Include', os.path.relpath(include, project.parent))
    ET.indent(tree, space='  ')
    tree.write(project, encoding='unicode')


def prepare(ctx):
    source = ctx.sources['product']
    ctx.receipt['source_inventory'] = helper(ctx.root / 'scripts/inventory.py').check(ctx.root, source)
    includes = ctx.work / 'include'
    profile = helper(ctx.root / 'scripts/profiles.py').write_header(ctx.root, source, ctx.profile, includes)
    groups = json_file(ctx.root / 'config/source-groups.json')
    if profile['kind'] == 'tool':
        staged = ctx.work / 'source'
        ctx.receipt['adaptations'] = helper(ctx.root / 'scripts/stage.py').makefsdata(ctx.root, source, staged)
        source = staged
        selected = groups['lwipmakefsdata_SRCS']
        ctx.receipt['source_closure'] = selected
        ctx.receipt['effective_profile'] = profile
        return Translation([Unit(source / name) for name in selected],
                           ['-std=c17', '--instance-methods', *flags(profile.get('compiler_defines', ()), includes=[includes, ctx.root / 'src/port', source / 'src/include'])],
                           [*link_flags('MakeFsData', 'Managed.Networking.Tools'), '--instance-methods'],
                           objects=True, configure=configure, finish=finish)
    selected = [name for group, names in groups.items()
                if group not in ('lwipmbedtls_SRCS', 'lwipmakefsdata_SRCS') for name in names]
    if profile['kind'] == 'diagnostic':
        # Match the upstream test executable's reachable library groups. Serial
        # hardware interface sources have no upstream unit registrations.
        selected = [name for group, names in groups.items() if group in (
            'lwipcore_SRCS', 'lwipcore4_SRCS', 'lwipcore6_SRCS', 'lwipapi_SRCS',
            'lwipppp_SRCS', 'lwipmdns_SRCS', 'lwipmqtt_SRCS') for name in names]
        selected.append('src/netif/ethernet.c')
        selected += json_file(ctx.root / 'config/unit-sources.json')
        ctx.receipt['source_closure'] = selected
        ctx.receipt['effective_profile'] = profile
        test_root = ctx.root / 'tests/Upstream'
        options = ['-std=c17', '--instance-methods', '-DLWIP_UNITTESTS_NOFORK=1',
                   *flags(includes=[test_root / 'include', includes, ctx.root / 'src/port', source / 'src/include', source / 'test/unit']),
                   '--overrides-file', str(test_root / 'overrides.json')]
        return Translation([*[Unit(source / name) for name in selected], Unit(test_root / 'check_support.c'), Unit(test_root / 'check_control.c')], options,
                           [*link_flags('Lwip', 'Managed.Networking'), '--instance-methods'], objects=True,
                           configure=configure, finish=finish, validate_objects=validate_objects)
    if profile['kind'] == 'extension':
        selected += json_file(ctx.root / 'config/contrib-sources.json')
    ctx.receipt['source_closure'] = selected
    ctx.receipt['effective_profile'] = profile
    ctx.receipt['adaptations'] = json_file(ctx.root / 'config/adaptations.json')
    options = ['-std=c17', '--instance-methods',
               *flags(includes=[includes, ctx.root / 'src/port', source / 'src/include', source / 'contrib']),
               '--overrides-file', str(ctx.root / 'config/dotcc-overrides.json')]
    authored = [Unit(p) for p in sorted((ctx.root / 'src/port').glob('*.c')) + sorted((ctx.root / 'src/Host').glob('*.c'))]
    return Translation([*[Unit(source / name, ('-DDOTCC_TCPIP_WORKER=1',) if name == 'src/api/tcpip.c' else ()) for name in selected], *authored], options,
                       [*link_flags('Lwip', 'Managed.Networking'), '--instance-methods'],
                       objects=True, configure=configure, finish=finish, validate_objects=validate_objects)


def validate_objects(ctx, reports):
    records = [json.loads(line) for report in reports for line in report.read_text().splitlines() if line]
    bindings = [row for row in records if row.get('event') == 'function-override']
    ctx.receipt['host_bindings'] = bindings
    ctx.receipt['override_reports'] = [str(path) for path in reports]
    if not bindings:
        raise RuntimeError('No typed host bindings matched the translated source closure')


def run_suite(ctx, suite):
    return helper(ctx.root / 'scripts/test.py').run(ctx, suite)


def validate_options(args):
    if args.profile == 'makefsdata' and args.rid != 'linux-x64':
        raise ValueError('The makefsdata runtime is currently qualified only for linux-x64; Windows orchestration is a separate contract')


_product = ('threaded', 'nosys', 'threaded-locking')
_suites = ('oracle', 'abi', 'sysarch', 'consumer', 'packets', 'socket-api', 'addressing',
           'routing', 'links', 'ppp', 'applications', 'contrib', 'tools', 'upstream',
           'lifecycle', 'recovery', 'interop', 'aot-rooted', 'audit')
recipe = Recipe('lwip', 'TranslatedLwip', (*_product, 'unit', 'unit-nosys', 'makefsdata', 'contrib'),
                ('src/Managed.Lwip/Managed.Lwip.csproj', 'samples/ManagedConsumer/ManagedConsumer.csproj'),
                sources, prepare, consumer=Consumer(property='LwipProject'), validate_options=validate_options,
                suites=tuple(Suite(name, (), run=run_suite, matrix=name != 'oracle',
                                  scope='Required lwIP ' + name + ' qualification; missing cases fail') for name in _suites),
                default_suites=('consumer',), verify_suites=_suites)
