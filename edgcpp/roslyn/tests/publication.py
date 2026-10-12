#!/usr/bin/env python3
"""Exercise ordinary output errors using a valid EDG compilation."""
from pathlib import Path
import subprocess
import tempfile

root = Path(__file__).resolve().parents[1]
wrapper = root / 'build' / 'roslyn-cpfe.py'
fixture = root / 'tests' / 'fixtures' / 'basic.cpp'
with tempfile.TemporaryDirectory(prefix='roslynobj-publication-test-') as work:
    directory = Path(work) / 'destination'
    directory.mkdir()
    marker = directory / 'existing'
    marker.write_text('preserve')
    result = subprocess.run(['python3', str(wrapper), '--c++20', str(fixture),
                             '-o', str(directory)], capture_output=True, text=True)
    assert result.returncode != 0, 'Publishing onto a directory must fail.'
    assert marker.read_text() == 'preserve', 'Existing destination contents changed.'
    assert not list(Path(work).glob('destination.tmp.*')), 'Temporary output leaked.'
    missing = Path(work) / 'missing' / 'output.roslynobj'
    result = subprocess.run(['python3', str(wrapper), '--c++20', str(fixture),
                             '-o', str(missing)], capture_output=True, text=True)
    assert result.returncode != 0, 'An absent output parent must fail.'
    assert not missing.exists(), 'Failed publication created an object.'
print('Valid-compilation publication errors preserve destinations and clean temporary output.')
