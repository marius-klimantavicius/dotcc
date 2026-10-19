"""Run with the same-pin generator, never the framework's host Python."""
import importlib
import json
import modulefinder
import os
import sys

source, output = sys.argv[1:]
required = ['json', 're', 'collections', 'functools', 'itertools', 'math', 'struct',
            'random', 'time', 'threading', 'contextvars', 'io', 'gc', 'weakref', 'sys', 'unicodedata',
            # These files were already shipped by the static graph. Import them
            # in the exact-pin oracle too, so their eager package dependencies
            # are included and actually usable. importlib.util is already in
            # the real frozen stdlib table; its file-backed _abc dependency
            # cannot be discovered by treating that frozen module as a leaf.
            'inspect', 'pathlib', 'importlib.util']
for name in required:
    importlib.import_module(name)
assert eval("'\\N{SNOWMAN}'") == '\u2603'
# Exercise branches whose imports are deferred until first use.
import random, json as json_module, re, traceback
assert random.Random('same-pin').getrandbits(17) == random.Random('same-pin').getrandbits(17)
assert json_module.loads(json_module.dumps({'x': [1, 2]})) == {'x': [1, 2]}
assert re.fullmatch(r'\w+', 'CPython')
entry = os.path.join(os.path.dirname(output), 'stdlib-entry.py')
with open(entry, 'w') as stream:
    stream.write('import ' + ','.join(required + ['traceback', 'types', 'importlib', 'warnings', 'tokenize']) + '\n')
finder = modulefinder.ModuleFinder(path=[os.path.join(source, 'Lib')])
finder.run_script(entry)
files = set()
for module in list(finder.modules.values()) + list(sys.modules.values()):
    path = getattr(module, '__file__', None)
    if path and path.endswith('.py') and os.path.commonpath([os.path.realpath(path), source]) == source:
        files.add(os.path.relpath(path, os.path.join(source, 'Lib')))
# Codec lookup is dynamic; deliver the complete upstream encodings package.
for name in os.listdir(os.path.join(source, 'Lib', 'encodings')):
    if name.endswith('.py'):
        files.add('encodings/' + name)
with open(output, 'w') as stream:
    json.dump({'profile': 'embedded-gil', 'basis': 'same-pin modulefinder static import graph plus observed required imports; entire encodings package for dynamic codec lookup',
               'required_modules': required, 'files': sorted(files),
               'missing_static_imports': {name: sorted(callers) for name, callers in sorted(finder.badmodules.items())},
               'builtins': sorted(sys.builtin_module_names),
               'version': sys.version, 'qualification': 'inventory only; required cases and branches remain qualification gates'}, stream, indent=2)
    stream.write('\n')
