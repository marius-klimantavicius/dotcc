"""lwIP complete bundled-source translation, isolated host and profile selection."""
import copy
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
    check = json_file(root / 'config/check-source.json')
    yield Source('check', check['url'], root / 'ref' / check['archive'],
                 root / 'ref' / check['directory'], check['directory'], check.get('sha256'), tuple(check['required']), group='tests')


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
    if ctx.profile == 'http-content':
        helper(ctx.root / 'scripts/generated-http.py').prepare(ctx, includes)
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
    if profile['kind'] == 'diagnostic':
        staged_units, repairs = {}, []
        ctx.receipt['adaptation_inapplicability'] = {'ipv4-forwarding-software-checksums': 'Original diagnostic options select IP_FORWARD=0; native and managed diagnostics retain original ip4.c.', 'eap-request-invalid-state-release': 'Original diagnostic options leave EAP_SUPPORT=0; native and managed diagnostics retain original eap.c.'}
    else:
        staged_units, repairs = helper(ctx.root / 'scripts/stage.py').product_units(ctx.root, source, ctx.work / 'source')
    ctx.receipt['adaptations'] = repairs
    def source_unit(name, unit_flags=()):
        return Unit(staged_units.get(name, source / name), unit_flags)
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
        adapters = []
        if ctx.profile == 'unit-nosys':
            selected.remove('test/unit/api/test_sockets.c')
            adapters.append(Unit(ctx.root / 'tests/Upstream/nosys.c'))
        ctx.receipt['source_closure'] = selected
        ctx.receipt['effective_profile'] = profile
        test_root = ctx.root / 'tests/Upstream'
        options = ['-std=c17', '--instance-methods', '-DLWIP_UNITTESTS_NOFORK=1',
                   *flags(includes=[test_root / 'include', includes, ctx.root / 'src/port', source / 'src/include', source / 'test/unit']),
                   '--overrides-file', str(test_root / 'overrides.json')]
        return Translation([*[source_unit(name) for name in selected], *adapters, Unit(test_root / 'check_support.c'), Unit(test_root / 'check_control.c')], options,
                           [*link_flags('Lwip', 'Managed.Networking'), '--instance-methods'], objects=True,
                           configure=configure, finish=finish, validate_objects=validate_objects)
    if profile['kind'] == 'extension':
        selected += json_file(ctx.root / 'config/contrib-sources.json')
    ctx.receipt['source_closure'] = selected
    ctx.receipt['effective_profile'] = profile
    options = ['-std=c17', '--instance-methods',
               *flags(includes=[includes, ctx.root / 'src/port', source / 'src/include', source / 'contrib']),
               '--overrides-file', str(ctx.root / 'config/dotcc-overrides.json')]
    authored = [Unit(p) for p in sorted((ctx.root / 'src/port').glob('*.c')) + sorted((ctx.root / 'src/Host').glob('*.c'))]
    return Translation([*[source_unit(name, ('-DDOTCC_TCPIP_WORKER=1',) if name == 'src/api/tcpip.c' else ()) for name in selected], *authored], options,
                       [*link_flags('Lwip', 'Managed.Networking'), '--instance-methods'],
                       objects=True, configure=configure, finish=finish, validate_objects=validate_objects)


def validate_objects(ctx, reports):
    records = [json.loads(line) for report in reports for line in report.read_text().splitlines() if line]
    bindings = [row for row in records if row.get('event') == 'function-override']
    ctx.receipt['host_bindings'] = bindings
    ctx.receipt['override_reports'] = [str(path) for path in reports]
    if not bindings:
        raise RuntimeError('No typed host bindings matched the translated source closure')
    config = ctx.root / ('tests/Upstream/overrides.json' if ctx.profile.startswith('unit') else 'config/dotcc-overrides.json')
    expected = {entry['name'] for entry in json_file(config)['functionOverrides']}
    absent = json_file(ctx.root / 'config/host-binding-applicability.json')['absent'].get(ctx.profile, {})
    actual = {entry['name'] for entry in bindings}
    missing = expected - actual - absent.keys()
    if missing:
        raise RuntimeError('Required typed host bindings did not match: ' + ', '.join(sorted(missing)))
    if any(entry.get('passInstance') != 'true' for entry in bindings):
        raise RuntimeError('Host override lost its explicit translated owner')
    ctx.receipt['host_binding_applicability'] = {name: ('matched' if name in actual else absent[name]) for name in sorted(expected)}
    helper(ctx.root / 'scripts/feature-audit.py').run(ctx)


def run_suite(ctx, suite):
    if ctx.options.action == 'verify' and suite.verify_profiles:
        from campaigns.runner import Context
        from campaigns.testing import run_suite as execute_suite
        for profile in suite.verify_profiles:
            options = copy.copy(ctx.options)
            options.action, options.profile = 'test', profile
            child = Context(ctx.repo, ctx.recipe, options, ctx.session)
            child.sources = dict(ctx.sources)
            child.env.update({key: value for key, value in ctx.env.items() if key != 'TMPDIR'})
            child.receipt['parent_receipt'] = str(ctx.artifacts / 'receipt.json')
            child.start()
            error = None
            try:
                execute_suite(child, suite)
            except BaseException as failure:
                error = failure
                raise
            finally:
                child.finish(error)
                ctx.receipt.setdefault('profile_suites', []).append({'suite': suite.name, 'profile': profile,
                    'receipt': str(child.artifacts / 'receipt.json'), 'status': child.receipt['status']})
                ctx.save()
        return
    if suite.verify_profiles and ctx.profile not in suite.verify_profiles:
        raise ValueError(suite.name + ' does not apply to profile ' + ctx.profile)
    return run_one(ctx, suite)


def run_one(ctx, suite):
    if suite.name == 'http-transactions' or (suite.name == 'tools' and ctx.profile == 'http-content'):
        return helper(ctx.root / 'scripts/http-tests.py').run(ctx, suite)
    if suite.name == 'tools' and ctx.profile == 'makefsdata':
        return helper(ctx.root / 'scripts/tool-tests.py').run(ctx, suite)
    if suite.name == 'slip-framing':
        return helper(ctx.root / 'scripts/link-tests.py').run(ctx, suite)
    if suite.name == 'ppp-ethernet':
        return helper(ctx.root / 'scripts/pppoe-tests.py').run(ctx, suite)
    if suite.name == 'ppp-serial':
        return helper(ctx.root / 'scripts/ppp-link-tests.py').run(ctx, suite)
    if suite.name == 'bundled-ppp':
        return helper(ctx.root / 'scripts/ppp-tests.py').run(ctx, suite)
    if suite.name == 'contrib-addons':
        return helper(ctx.root / 'scripts/contrib-tests.py').run(ctx, suite)
    if suite.name in ('tool-generator', 'upstream'):
        return helper(ctx.root / ('scripts/tool-tests.py' if suite.name == 'tool-generator' else 'scripts/upstream-tests.py')).run(ctx, suite)
    return helper(ctx.root / 'scripts/test.py').run(ctx, suite)


def validate_options(args):
    if args.profile in ('makefsdata','http-content') and args.rid != 'linux-x64':
        raise ValueError('The makefsdata runtime is currently qualified only for linux-x64; Windows orchestration is a separate contract')


_product = ('threaded', 'nosys', 'threaded-locking')
_suites = ('oracle', 'abi', 'sysarch', 'consumer', 'packets', 'socket-api', 'addressing',
           'routing', 'links', 'ppp', 'applications', 'contrib', 'tools', 'upstream',
           'lifecycle', 'recovery', 'interop', 'aot-rooted', 'audit', 'direct-apis', 'datagram-fixtures', 'owner-lifecycle', 'socket-semantics', 'dns-fixtures', 'routing-ipv4', 'tool-generator', 'contrib-addons', 'bundled-ppp', 'interop-kernel', 'slip-framing', 'http-transactions', 'pool-recovery', 'dhcp-fixtures', 'autoip-fixtures', 'ppp-serial', 'ppp-ethernet')
_applicability = {'tools': ('makefsdata', 'http-content'), 'tool-generator': ('makefsdata',), 'upstream': ('unit', 'unit-nosys'), 'contrib': ('contrib',),
                  'sysarch': ('threaded', 'threaded-locking'), 'socket-api': ('threaded', 'threaded-locking'),
                  'direct-apis': _product, 'socket-semantics': ('threaded', 'threaded-locking'), 'contrib-addons': ('contrib',), 'interop-kernel': _product, 'http-transactions': (*_product, 'http-content')}
recipe = Recipe('lwip', 'TranslatedLwip', (*_product, 'unit', 'unit-nosys', 'makefsdata', 'contrib', 'http-content'),
                ('src/Managed.Lwip/Managed.Lwip.csproj', 'samples/ManagedConsumer/ManagedConsumer.csproj'),
                sources, prepare, consumer=Consumer(property='LwipProject'), validate_options=validate_options,
                suites=tuple(Suite(name, (), run=run_suite, matrix=name != 'oracle',
                                  verify_profiles=_applicability.get(name, _product),
                                  scope='Required lwIP ' + name + ' qualification; missing cases fail') for name in _suites),
                default_suites=('consumer',), verify_suites=_suites)
