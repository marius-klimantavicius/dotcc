"""Same-pin native control for the actual first translated bootstrap failure."""
import importlib
import json
import os
import sys
import time

assert sys.version_info[:3] == (3, 13, 15)
assert sys.flags.utf8_mode == 1
assert time.strftime('%Y-%m-%d', (1970, 1, 1, 0, 0, 0, 3, 1, 0)) == '1970-01-01'
for name in ('waitpid', 'waitstatus_to_exitcode', 'WIFSTOPPED', 'WSTOPSIG', 'WNOHANG'):
    assert hasattr(os, name), name
assert os.waitstatus_to_exitcode(42 << 8) == 42
assert os.waitstatus_to_exitcode(15) == -15
for status in (0x137f, 0x7f, 0xffff):
    try:
        os.waitstatus_to_exitcode(status)
    except ValueError:
        pass
    else:
        raise AssertionError(('stopped/continued status accepted', status))
origins = {}
for name in ('test.support', 'subprocess', 'asyncio', 'test.test_grammar'):
    module = importlib.import_module(name)
    origins[name] = module.__spec__.origin
print(json.dumps({'status': 'passed', 'scope': 'same-pin native bootstrap/control only; zero translated assertions',
                  'python': sys.version, 'utf8_mode': sys.flags.utf8_mode, 'imports': origins}))
