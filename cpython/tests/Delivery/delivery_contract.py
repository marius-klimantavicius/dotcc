"""Executed by the actual relocated translated interpreter through its API."""
import importlib
import json
import os
import sys


def run(library, application):
    library, application = map(os.path.realpath, (library, application))
    assert sys.flags.isolated == 1
    assert sys.flags.ignore_environment == 1
    assert sys.flags.no_user_site == 1
    assert sys.flags.no_site == 1
    assert sys.flags.utf8_mode == 1
    assert sys.getfilesystemencoding() == 'utf-8'
    assert [os.path.realpath(path) for path in sys.path] == [library, application], sys.path
    assert os.path.realpath(os.getcwd()) not in (library, application)
    origins = {}
    for name in ('json', 'random', 're', 'threading', 'collections', 'contextvars'):
        module = importlib.import_module(name)
        path = os.path.realpath(module.__file__)
        assert os.path.commonpath((library, path)) == library, (name, path)
        origins[name] = path
    for name in ('math', '_struct', '_thread', 'gc', 'unicodedata'):
        module = importlib.import_module(name)
        assert module.__spec__.origin == 'built-in', (name, module.__spec__.origin)
        origins[name] = 'built-in'
    assert os.path.commonpath((application, os.path.realpath(__file__))) == application
    loaded_extensions = {name: module.__spec__.origin for name, module in list(sys.modules.items())
        if getattr(module, '__spec__', None) is not None
        and isinstance(module.__spec__.origin, str)
        and module.__spec__.origin.lower().endswith(('.so', '.pyd', '.dll', '.dylib'))}
    assert not loaded_extensions, loaded_extensions
    with open('/proc/self/maps', encoding='utf-8') as mappings:
        native_images = sorted({line.split()[-1] for line in mappings if '/' in line})
    assert not any('libpython' in path.lower() or 'libpypy' in path.lower() for path in native_images), native_images
    # Actual generated configuration/stdlib performs the checks above.
    return json.dumps({'status': 'passed', 'scope': 'actual relocated translated interpreter',
        'python': sys.version, 'utf8_mode': sys.flags.utf8_mode, 'sys_path': sys.path,
        'cwd': os.getcwd(), 'origins': origins, 'builtin_modules': sorted(sys.builtin_module_names),
        'loaded_extensions': loaded_extensions, 'native_images': native_images}, sort_keys=True)
