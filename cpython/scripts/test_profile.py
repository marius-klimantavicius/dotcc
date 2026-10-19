"""Separate static upstream-test closure, derived from reviewed same-pin Setup.

The native helper oracle is never an input/dependency of these products.
Core units/configuration/Host are inherited from the real translated product.
"""
import hashlib
import json
from pathlib import Path
import re
import shlex
from campaigns.model import Unit
from campaigns.recipes import helper, json_file

PROFILE = 'upstream-tests-static'


def definition(ctx):
    return json_file(ctx.root/'tests/Upstream/helper-profile.json')


def extend(ctx, source, units, product):
    selected = definition(ctx)
    template = (source/'Modules/Setup.stdlib.in').read_text().splitlines()
    known = {unit.path.resolve() for unit in units}
    modules = []
    names = list(product['static_builtins'])
    for name in selected['static_modules']:
        matches = [(number, re.sub(r'^@MODULE_[A-Z0-9_]+_TRUE@', '', line))
                   for number, line in enumerate(template, 1)
                   if re.match(r'^@MODULE_[A-Z0-9_]+_TRUE@'+re.escape(name)+r'\s', line)]
        if len(matches) != 1:
            raise RuntimeError('Expected one upstream Setup row for '+name)
        number, upstream = matches[0]
        row = selected['setup_overrides'].get(name, upstream)
        words = shlex.split(row)
        if words[0] != name:
            raise RuntimeError('Test Setup override changes module identity: '+name)
        paths = [Path('Modules')/word for word in words[1:] if word.endswith('.c')]
        options = [word.replace('$(srcdir)', str(source)) for word in words[1:] if not word.endswith('.c')]
        if any(not flag.startswith(('-D', '-U', '-I')) for flag in options):
            raise RuntimeError('Unreviewed test-profile native dependency/option: '+row)
        # Makefile.pre.in gives builtins PY_BUILTIN_MODULE_CFLAGS, rather than
        # PY_CORE_CFLAGS. The product's common flags include Py_BUILD_CORE;
        # remove that inherited flag before selecting the actual builtin role.
        # Public/limited test headers intentionally undef the builtin macro
        # before Python.h; internal helpers retain its normal implication.
        arguments = ('-UPy_BUILD_CORE', '-DPy_BUILD_CORE_BUILTIN', *options)
        for relative in paths:
            if relative.is_absolute() or '..' in relative.parts or not (source/relative).is_file():
                raise RuntimeError('Missing/unsafe real helper source: '+str(relative))
            if (source/relative).resolve() not in known:
                units.append(Unit(source/relative, arguments))
                known.add((source/relative).resolve())
        if name not in names:
            names.append(name)
        modules.append({'name': name, 'setup': 'Modules/Setup.stdlib.in', 'line': number,
                        'upstream': upstream, 'selected': row, 'flags': list(arguments),
                        'sources': [{'path': str(path), 'sha256': hashlib.sha256((source/path).read_bytes()).hexdigest()}
                                    for path in paths]})

    # Retain upstream's special core entries and sentinel exactly. Only fill
    # makesetup's two explicit module-table generation points.
    config = (source/'Modules/config.c.in').read_text()
    replacements = {
        '/* -- ADDMODULE MARKER 1 -- */': '\n'.join('extern PyObject* PyInit_'+name+'(void);' for name in names),
        '/* -- ADDMODULE MARKER 2 -- */': '\n'.join('    {"'+name+'", PyInit_'+name+'},' for name in names),
    }
    for marker, text in replacements.items():
        if config.count(marker) != 1:
            raise RuntimeError('Missing/ambiguous upstream module-table generator marker')
        config = config.replace(marker, text)
    target = source/'Modules/config.c'
    target.write_text(config)
    ctx.receipt['test_profile'] = {'profile': PROFILE, 'scope': selected['scope'], 'modules': modules,
                                  'product_units_retained': len(product['units'])+len(ctx.receipt['authored_units']),
                                  'selected_units': len(units), 'static_setup_modules': names,
                                  'native_oracle_dependency': False}
    return target


def asset_files(ctx):
    # This is a test-only tree. Retain unmodified Lib/test support and resource
    # data, plus all Python dependencies, without broadening shipped stdlib.
    root = ctx.sources['product']/'Lib'
    return sorted(path.relative_to(root).as_posix() for path in root.rglob('*')
                  if path.is_file() and '__pycache__' not in path.parts and path.suffix not in ('.pyc', '.pyo'))


def sysconfig_data(ctx, directory):
    policy = helper(ctx.root/'scripts/pyconfig.py').effective(ctx)
    profile = json_file(ctx.root/'config/profile.json')
    values = {}
    for name, value in policy['values'].items():
        try:
            values[name] = int(value)
        except ValueError:
            if value.startswith('"') and value.endswith('"'):
                values[name] = json.loads(value)
    values.update({name: 0 for name in ('Py_GIL_DISABLED', 'Py_DEBUG', 'Py_ENABLE_SHARED', 'HAVE_DYNAMIC_LOADING')})
    values.update(CC='dotcc', CFLAGS='-std='+profile['dotcc_standard']+' '+
                  ' '.join('-D'+flag for flag in profile['defines']),
                  SOABI='dotcc-cpython-static-linux-x64-wchar16', EXT_SUFFIX='',
                  CONFIG_ARGS='static translated test profile; GIL; pymalloc; no dynamic extensions',
                  ABIFLAGS=values.get('ABIFLAGS', ''), MULTIARCH=values.get('MULTIARCH', ''))
    platform = values.get('PLATFORM', 'unknown')
    name = '_sysconfigdata_'+values['ABIFLAGS']+'_'+platform+'_'+values['MULTIARCH']+'.py'
    target = directory/'stdlib'/name
    # Runtime paths describe this relocated artifact, never a native build tree.
    target.write_text('# Generated from reviewed DotCC configuration; no native pyconfig/sysconfig reuse.\n'
                      'import os as _os\n'
                      'build_time_vars = '+repr(values)+'\n'
                      'build_time_vars.update(LIBDEST=_os.path.dirname(__file__), '
                      'BINLIBDEST=_os.path.dirname(__file__))\n')
    return target
