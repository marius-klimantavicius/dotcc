"""Resolve one target feature policy for the complete profile, including helpers.

Native configure output is deliberately not an input. Profile features are C
source-selection decisions; their documented service contracts remain separate.
"""
import copy
import hashlib
import json
from campaigns.recipes import json_file


LOCAL_SOCKET_FEATURES = frozenset((
    'HAVE_SOCKET', 'HAVE_BIND', 'HAVE_LISTEN', 'HAVE_CONNECT', 'HAVE_ACCEPT',
    'HAVE_GETSOCKNAME', 'HAVE_GETPEERNAME', 'HAVE_GETADDRINFO', 'HAVE_NETDB_H',
    'HAVE_ADDRINFO', 'HAVE_SOCKADDR_STORAGE', 'HAVE_GETNAMEINFO', 'HAVE_GETHOSTBYNAME'))
TARGET_LOCALE_FEATURES = frozenset((
    '_Py_FORCE_UTF8_LOCALE', '_Py_FORCE_UTF8_FS_ENCODING'))
UNIFORM_SERVICE_FEATURES = LOCAL_SOCKET_FEATURES | frozenset((
    'HAVE_GETHOSTNAME', 'HAVE_GETPID', 'HAVE_STRFTIME', 'HAVE_WAITPID', 'HAVE_SYS_WAIT_H', 'HAVE_WCSFTIME',
    'HAVE_PIPE', 'HAVE_SYMLINK', 'HAVE_READLINK', 'HAVE_LSTAT'))


def validate_services(policy):
    values = policy['values']
    # pyport.h derives the filesystem macro (with an empty replacement list)
    # from the whole-target locale choice. Never select only the filesystem
    # path: current_locale=1 would retain the scalar-per-wchar UTF-16 hole.
    if '_Py_FORCE_UTF8_FS_ENCODING' in values or '_Py_FORCE_UTF8_FS_ENCODING' in policy['undefined']:
        raise RuntimeError('_Py_FORCE_UTF8_FS_ENCODING must be derived by upstream pyport.h')
    if '_Py_FORCE_UTF8_LOCALE' in values and values['_Py_FORCE_UTF8_LOCALE'] != '1':
        raise RuntimeError('Selected target locale feature _Py_FORCE_UTF8_LOCALE must be 1')
    if policy.get('target_locale_contract') and (
            values.get('_Py_FORCE_UTF8_LOCALE') != '1' or values.get('SIZEOF_WCHAR_T') != '2'):
        raise RuntimeError('Incomplete unsigned wchar16 CPython UTF-8 target locale contract')
    for name in UNIFORM_SERVICE_FEATURES:
        if name in values and values[name] != '1':
            raise RuntimeError('Selected service feature '+name+' must be 1; zero still selects #ifdef code')
    if ('HAVE_WAITPID' in values) != ('HAVE_SYS_WAIT_H' in values):
        raise RuntimeError('Incomplete waitpid/status-header import contract')
    if 'HAVE_WCSFTIME' in values:
        raise RuntimeError('No shared wcsftime declaration/service contract; retain narrow strftime')
    if 'HAVE_SYMLINK' in values and not {'HAVE_READLINK', 'HAVE_LSTAT'} <= values.keys():
        raise RuntimeError('Incomplete symlink/readlink/lstat filesystem contract')
    # socket alone is a common declared service. The Python self-pipe also
    # requires six methods, the real addrinfo ABI, and a complete getnameinfo
    # service. Its Python wrapper also needs the hostname decoder selected by
    # the nonreentrant host lookup contract. Merely exposing the header activates
    # the legacy resolver fallback unless this complete family is selected.
    socket_methods = LOCAL_SOCKET_FEATURES - {'HAVE_SOCKET'}
    if socket_methods & values.keys() and not LOCAL_SOCKET_FEATURES <= values.keys():
        raise RuntimeError('Incomplete local socket self-pipe service contract')


def resolve(document, profile):
    if profile not in ('embedded-gil', 'upstream-tests-static'):
        raise RuntimeError('Unknown CPython target profile: '+profile)
    policy = copy.deepcopy(document)
    overlays = policy.pop('profiles', {})
    selected = overlays.get(profile, {})
    for section, other in (('values', 'undefined'), ('undefined', 'values')):
        if set(selected.get(section, {})) & set(selected.get(other, {})):
            raise RuntimeError('Ambiguous profile feature policy')
        for name, value in selected.get(section, {}).items():
            policy[other].pop(name, None)
            policy[section][name] = value
    if set(policy['values']) & set(policy['undefined']):
        raise RuntimeError('Target feature is both defined and undefined')
    validate_services(policy)
    policy['profile'] = profile
    policy['profile_contract'] = copy.deepcopy(selected.get('contract', {}))
    return policy


def effective(ctx):
    return resolve(json_file(ctx.root/'config/pyconfig-policy.json'), ctx.profile)


def render(policy):
    header = '/* Generated from reviewed DotCC ABI/service policy; never native pyconfig.h. */\n#ifndef DOTCC_CPYTHON_PYCONFIG_H\n#define DOTCC_CPYTHON_PYCONFIG_H\n'
    header += ''.join('#define '+key+' '+value+'\n' for key, value in policy['values'].items())
    header += ''.join('/* #undef '+key+' -- '+reason+' */\n' for key, reason in policy['undefined'].items())
    return header+'#endif\n'


def bind(ctx, policy, header, units, common):
    """Bind policy and ordered unit options before any object can be emitted."""
    validate_services(policy)
    if header.read_text() != render(policy):
        raise RuntimeError('Generated target header differs from resolved feature policy')
    document = json_file(ctx.root/'config/pyconfig-policy.json')
    overlay = document.get('profiles', {}).get(ctx.profile, {})
    selected_features = set(overlay.get('values', {})) | set(overlay.get('undefined', {}))
    # An unselected feature must remain unselected in every translation unit as
    # well. Include other profiles' features to protect embedded core layout.
    features = set(UNIFORM_SERVICE_FEATURES | TARGET_LOCALE_FEATURES)
    for other in document.get('profiles', {}).values():
        features.update(other.get('values', {}))
        features.update(other.get('undefined', {}))
    rows = []
    for unit in units:
        options = [*common, *unit.flags]
        # A layout-changing profile feature cannot be overridden per unit.
        # Support both CLI spellings rather than assuming glued -D/-U only.
        for index, option in enumerate(options):
            if option in ('-D', '-U'):
                if index+1 >= len(options) or options[index+1].startswith('-'):
                    raise RuntimeError('Missing feature option argument: '+option)
                name = options[index+1].split('=', 1)[0]
            elif option.startswith(('-D', '-U')):
                name = option[2:].split('=', 1)[0]
            else:
                continue
            if name in features:
                raise RuntimeError('Per-unit override of profile feature '+name+': '+str(unit.path))
        rows.append({'path': str(unit.path), 'ordered_options': options})
    canonical = json.dumps(policy, sort_keys=True, separators=(',', ':')).encode()
    ctx.receipt['profile_feature_policy'] = {
        'profile': ctx.profile,
        'policy': str(ctx.root/'config/pyconfig-policy.json'),
        'policy_sha256': hashlib.sha256((ctx.root/'config/pyconfig-policy.json').read_bytes()).hexdigest(),
        'resolver': str(ctx.root/'scripts/pyconfig.py'),
        'resolver_sha256': hashlib.sha256((ctx.root/'scripts/pyconfig.py').read_bytes()).hexdigest(),
        'effective_sha256': hashlib.sha256(canonical).hexdigest(),
        'header': str(header), 'header_sha256': hashlib.sha256(header.read_bytes()).hexdigest(),
        'selected_features': {name: policy['values'].get(name) for name in sorted(selected_features)},
        'uniform_features': {name: policy['values'].get(name) for name in sorted(features)},
        'derived_features': ({'_Py_FORCE_UTF8_FS_ENCODING': {
            'source': 'Include/pyport.h', 'condition': 'defined(_Py_FORCE_UTF8_LOCALE)',
            'definition': 'defined with empty replacement list'}}
            if '_Py_FORCE_UTF8_LOCALE' in policy['values'] else {}),
        'target_locale_contract': copy.deepcopy(policy.get('target_locale_contract', {})),
        'contract': policy['profile_contract'], 'units': rows,
    }
